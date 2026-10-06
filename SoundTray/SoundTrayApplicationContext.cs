namespace SoundTray
{
    using NAudio.CoreAudioApi;
    using SoundTray.Properties;
    using SoundTray.SoundModels;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// The Application Context for SoundTray
    /// </summary>
    public class SoundTrayApplicationContext : ApplicationContext
    {
        private static readonly ContextMenuStrip SoundTrayContextMenuStrip = new ContextMenuStrip();
        private static readonly NotifyIcon NotifyIcon = new NotifyIcon();
        internal static SoundTrayStatus? SoundTrayStatusWindow;

        private List<AudioDevice> AudioOutputDevicesCache = new List<AudioDevice>();
        internal static AudioDevice DefaultAudioOutputDeviceCache = new AudioDevice();

        private List<AudioDevice> AudioInputDevicesCache = new List<AudioDevice>();
        internal static AudioDevice DefaultAudioInputDeviceCache = new AudioDevice();

        private AudioOutputDeviceComparer comparer = new AudioOutputDeviceComparer();
        private Bitmap defaultAudioOutputImage = Resources.AppIcon.ToBitmap();
        private Bitmap cancelImage = Resources.Cancel.ToBitmap();
        private Bitmap defaultAudioInputImage = Resources.Microphone.ToBitmap();
        private Bitmap greenTickImage = Resources.GreenTick.ToBitmap();

        private System.Windows.Forms.Timer? deviceChangeDetectionTimer;

        string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        string exeFilePath = Application.ExecutablePath;
        string shortcutPath = string.Empty;

        private bool isWindowsStartUpEnabled = false;

        /// <summary>
        /// SoundTray Application Context constructor
        /// </summary>
        public SoundTrayApplicationContext()
        {
            // First, get the default audio devices
            GetDefaultAudioDevices();

            // Then in your constructor or a method, initialize it when needed:
            if (SoundTrayStatusWindow == null)
            {
                SoundTrayStatusWindow = new SoundTrayStatus();
            }

            shortcutPath = Path.Combine(startupPath, $"{appName}.lnk");
            isWindowsStartUpEnabled = IsSoundTrayInWindowsStartUp(shortcutPath);

            if (SoundTrayContextMenuStrip.Items.Count > 0)
            {
                SoundTrayContextMenuStrip.Items?.Clear();
            }

            ShowAudioInputAndOutputDevices();

            NotifyIcon.ContextMenuStrip = SoundTrayContextMenuStrip;
            NotifyIcon.Icon = Resources.AppIcon;
            NotifyIcon.Click += new EventHandler(SoundTrayContextMenuStrip_Show);
            NotifyIcon.DoubleClick += new EventHandler(DoubleClickBehaviour);
            NotifyIcon.Visible = true;
            NotifyIcon.BalloonTipTitle = "Sound Tray";
            NotifyIcon.BalloonTipText = "To always show this icon, right-click the taskbar, choose 'Taskbar settings', then 'Select which icons appear on the taskbar'.";
            NotifyIcon.ShowBalloonTip(5000);

            // Initialize device change detection timer to refresh cache periodically
            InitializeDeviceChangeDetection();
        }

        /// <summary>
        /// Initializes a timer to periodically check for audio device changes
        /// </summary>
        private void InitializeDeviceChangeDetection()
        {
            deviceChangeDetectionTimer = new System.Windows.Forms.Timer();
            deviceChangeDetectionTimer.Interval = 5000; // Check every 5 seconds
            deviceChangeDetectionTimer.Tick += (sender, e) =>
            {
                // This will trigger a cache validation and refresh if devices changed
                var currentInputDevices = SoundTrayStatus.GetAudioInputDevices();
                var currentOutputDevices = SoundTrayStatus.GetAudioOutputDevices();

                // Check if device list has changed
                if (!AudioInputDevicesCache.SequenceEqual(currentInputDevices, comparer) ||
                    !AudioOutputDevicesCache.SequenceEqual(currentOutputDevices, comparer))
                {
                    // Devices changed, refresh the context menu next time it's shown
                    Debug.WriteLine("Audio device changes detected, cache will be refreshed on next menu show.");
                    SoundTrayStatus.InvalidateDeviceCache();
                }
            };
            deviceChangeDetectionTimer.Start();
        }

        /// <summary>
        /// Get the default audio input and output devices
        /// </summary>
        public static void GetDefaultAudioDevices()
        {
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                var defaultAudioInputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                var defaultAudioOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                DefaultAudioInputDeviceCache = new AudioDevice() { ID = defaultAudioInputDevice.ID, FriendlyName = defaultAudioInputDevice.FriendlyName };
                DefaultAudioOutputDeviceCache = new AudioDevice() { ID = defaultAudioOutputDevice.ID, FriendlyName = defaultAudioOutputDevice.FriendlyName };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting default audio devices: {ex.Message}");
            }
        }

        /// <summary>
        /// Double click on the icon behaviour
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void DoubleClickBehaviour(object sender, EventArgs e)
        {
            SoundTrayStatusWindow.ShowControlPanelStatus(sender, e);
        }

        /// <summary>
        /// Display the context menu strip options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SoundTrayContextMenuStrip_Show(object sender, EventArgs e)
        {
            ShowAudioInputAndOutputDevices();
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Exit(object sender, EventArgs e)
        {
            // Clean up device change detection timer
            if (deviceChangeDetectionTimer != null)
            {
                deviceChangeDetectionTimer.Stop();
                deviceChangeDetectionTimer.Dispose();
            }

            NotifyIcon.Visible = false;
            NotifyIcon.Icon = null;
            NotifyIcon.Dispose();

            SoundTrayStatusWindow?.Dispose();

            GC.Collect();
            Application.Exit();
        }

        /// <summary>
        /// Displays the list of available audio input and output devices in the sound tray context menu.
        /// Also adds an option to exit the application.
        /// </summary>
        /// <remarks>This method retrieves the audio output devices using <see
        /// cref="SoundTrayStatus.GetAudioInputDevices"/>  and <see
        /// cref="SoundTrayStatus.GetAudioOutputDevices"/>adds each device to the sound tray context menu.
        /// Selecting a device from the menu sets it as the active audio device.</remarks>
        public void ShowAudioInputAndOutputDevices()
        {
            SoundTrayStatus.LoadSettings();

            // Get the filtered audio devices into a list (with caching enabled)
            var audioInputDevices = SoundTrayStatus.GetAudioInputDevices();
            var audioOutputDevices = SoundTrayStatus.GetAudioOutputDevices();

            // Consolidate enumerator usage - use single instance for default device retrieval
            MMDevice? defaultAudioInputDevice = null;
            MMDevice? defaultAudioOutputDevice = null;

            try
            {
                using var enumerator = new MMDeviceEnumerator();
                defaultAudioInputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                defaultAudioOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting default audio endpoints: {ex.Message}");
            }

            // Check if the input devices list are exactly equal to the cache
            var areAudioInputDeviceListsExactlyEqual = AudioInputDevicesCache.SequenceEqual(audioInputDevices, comparer);

            // Check if the output devices list are exactly equal to the cache
            var areAudioOutputDeviceListsExactlyEqual = AudioOutputDevicesCache.SequenceEqual(audioOutputDevices, comparer);

            var isSoundTrayEnabledEqualToStartupCache = isWindowsStartUpEnabled != IsSoundTrayInWindowsStartUp(shortcutPath);

            // Only clear and rebuild if something changed
            if (!isSoundTrayEnabledEqualToStartupCache || !areAudioInputDeviceListsExactlyEqual || !areAudioOutputDeviceListsExactlyEqual || 
                (defaultAudioInputDevice != null && DefaultAudioInputDeviceCache.FriendlyName != defaultAudioInputDevice.FriendlyName) || 
                (defaultAudioOutputDevice != null && DefaultAudioOutputDeviceCache.FriendlyName != defaultAudioOutputDevice.FriendlyName))
            {
                NotifyIcon.ContextMenuStrip?.Items.Clear();
                if (defaultAudioInputDevice != null && defaultAudioOutputDevice != null)
                {
                    UpdateContextMenu(areAudioInputDeviceListsExactlyEqual, areAudioOutputDeviceListsExactlyEqual, audioInputDevices, audioOutputDevices, defaultAudioInputDevice, defaultAudioOutputDevice);
                }
            }

            NotifyIcon.ContextMenuStrip = SoundTrayContextMenuStrip;
        }

        /// <summary>
        /// Refreshes the context menu if it is not the same as the cached audio devices.
        /// </summary>
        /// <param name="areAudioInputDeviceListsExactlyEqual">Boolean to indicate if input device lists are the same as the input device cache.</param>
        /// <param name="areOutputAudioDeviceListsExactlyEqual">Boolean to indicate if output device lists are the same as the output device cache.</param>
        /// <param name="audioInputDevices">List of Audio Input Devices</param>
        /// <param name="audioOutputDevices">List of Audio Output Devices</param>
        /// <param name="defaultAudioInputDevice">The default Audio Input Device.</param>
        /// <param name="defaultAudioOutputDevice">The default Audio Output Device.</param>
        private void UpdateContextMenu(bool areAudioInputDeviceListsExactlyEqual, bool areOutputAudioDeviceListsExactlyEqual, List<AudioDevice> audioInputDevices, List<AudioDevice> audioOutputDevices, MMDevice defaultAudioInputDevice, MMDevice defaultAudioOutputDevice)
        { 
            if (IsSoundTrayInWindowsStartUp(shortcutPath))
            {
                SoundTrayContextMenuStrip.Items.Add("SoundTray Windows Startup Enabled", greenTickImage, new EventHandler(DisableSoundTrayStartUp(shortcutPath)));
            }
            else
            {
                SoundTrayContextMenuStrip.Items.Add("SoundTray Windows Startup Disabled", null, new EventHandler(EnableSoundTrayStartup(shortcutPath)));
            }

            if (SoundTrayContextMenuStrip.Items.Count > 0)
            {
                SoundTrayContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            // update the cache
            if (DefaultAudioInputDeviceCache.FriendlyName != defaultAudioInputDevice.FriendlyName)
            {
                DefaultAudioInputDeviceCache = new AudioDevice() { ID = defaultAudioInputDevice.ID, FriendlyName = defaultAudioInputDevice.FriendlyName };
            }

            // update the cache
            if (DefaultAudioOutputDeviceCache.FriendlyName != defaultAudioOutputDevice.FriendlyName)
            {
                DefaultAudioOutputDeviceCache = new AudioDevice() { ID = defaultAudioOutputDevice.ID, FriendlyName = defaultAudioOutputDevice.FriendlyName };
            }

            // set the input cache to the new input list
            AudioInputDevicesCache = audioInputDevices.Count == 0 ? SoundTrayStatus.GetAudioInputDevices() : audioInputDevices;

            // set the output cache to the new output list
            AudioOutputDevicesCache = audioOutputDevices.Count == 0 ? SoundTrayStatus.GetAudioOutputDevices() : audioOutputDevices;

            GetDefaultAudioDevices();

            // build up the context menu strip for input devices
            foreach (var audioInputDevice in AudioInputDevicesCache)
            {
                if (Program.enabledInputAudioDevices.Contains(audioInputDevice.FriendlyName) && audioInputDevice.FriendlyName == DefaultAudioInputDeviceCache.FriendlyName)
                {
                    SoundTrayContextMenuStrip.Items.Add(audioInputDevice.FriendlyName, defaultAudioInputImage, new EventHandler(SoundTrayStatusWindow?.SetDefaultAudioDevice(audioInputDevice, ERole.eCommunications)));
                }
                else if (Program.enabledInputAudioDevices.Contains(audioInputDevice.FriendlyName))
                {
                    SoundTrayContextMenuStrip.Items.Add(audioInputDevice.FriendlyName, null, new EventHandler(SoundTrayStatusWindow?.SetDefaultAudioDevice(audioInputDevice, ERole.eCommunications)));
                }
            }

            if (SoundTrayContextMenuStrip.Items.Count > 0)
            {
                SoundTrayContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            // build up the context menu strip for output devices
            foreach (var audioOutputDevice in AudioOutputDevicesCache)
            {
                if (Program.enabledOutputAudioDevices.Contains(audioOutputDevice.FriendlyName) && audioOutputDevice.FriendlyName == DefaultAudioOutputDeviceCache.FriendlyName)
                {
                    SoundTrayContextMenuStrip.Items.Add(audioOutputDevice.FriendlyName, defaultAudioOutputImage, new EventHandler(SoundTrayStatusWindow.SetDefaultAudioDevice(audioOutputDevice, ERole.eMultimedia)));
                }
                else if (Program.enabledOutputAudioDevices.Contains(audioOutputDevice.FriendlyName))
                {
                    SoundTrayContextMenuStrip.Items.Add(audioOutputDevice.FriendlyName, null, new EventHandler(SoundTrayStatusWindow.SetDefaultAudioDevice(audioOutputDevice, ERole.eMultimedia)));
                }
            }

            if (SoundTrayContextMenuStrip.Items.Count > 0)
            {
                SoundTrayContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            // always add Exit menu item to the end of the menu.
            SoundTrayContextMenuStrip.Items.Add("Exit", cancelImage, new EventHandler(Exit));
        }

        /// <summary>
        /// Enables SoundTray to start up with Windows
        /// </summary>
        /// <param name="shortcutPath">The shortcut path to check for the SoundTray.lnk</param>
        /// <returns></returns>
        private EventHandler EnableSoundTrayStartup(string shortcutPath)
        {
            return (object? sender, EventArgs e) =>
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                shortcut.Save();

                isWindowsStartUpEnabled = true;
            };
        }

        /// <summary>
        /// Disables SoundTray from starting up with Windows
        /// </summary>
        /// <param name="shortcutPath">The shortcut path to check for the SoundTray.lnk</param>
        /// <returns></returns>
        private EventHandler DisableSoundTrayStartUp(string shortcutPath)
        {
            return (object? sender, EventArgs e) =>
            {
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                isWindowsStartUpEnabled = false;
            };
        }

        /// <summary>
        /// Checks if Windows Startup has SoundTray enabled or disabled
        /// </summary>
        /// <param name="shortcutPath">The shortcut path to check for the SoundTray.lnk</param>
        /// <returns>True or false depending on Windows Startup has SoundTray enabled or disabled.</returns>
        private bool IsSoundTrayInWindowsStartUp(string shortcutPath)
        {
            if (File.Exists(shortcutPath))
            {
                isWindowsStartUpEnabled = true;
                return true;
            }

            isWindowsStartUpEnabled = false;
            return false;
        }
    }
}