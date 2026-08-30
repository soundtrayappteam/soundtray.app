namespace SoundTray
{
    using NAudio.CoreAudioApi;
    using SoundTray.Properties;
    using SoundTray.SoundModels;
    using System;
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
        }

        /// <summary>
        /// Get the default audio input and output devices
        /// </summary>
        public static void GetDefaultAudioDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var defaultAudioInputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            var defaultAudioOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            DefaultAudioInputDeviceCache = new AudioDevice() { ID = defaultAudioInputDevice.ID, FriendlyName = defaultAudioInputDevice.FriendlyName };
            DefaultAudioOutputDeviceCache = new AudioDevice() { ID = defaultAudioOutputDevice.ID, FriendlyName = defaultAudioOutputDevice.FriendlyName };
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

            // get the filtered audio devices into a list
            var audioInputDevices = SoundTrayStatus.GetAudioInputDevices();

            // get all the audio devices into a list
            var audioOutputDevices = SoundTrayStatus.GetAudioOutputDevices();

            // check if the cache is exactly equal
            var enumerator = new MMDeviceEnumerator();

            var defaultAudioInputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            var defaultAudioOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            // next check if the input devices list are exactly equal to the cache
            var areAudioInputDeviceListsExactlyEqual = AudioInputDevicesCache.SequenceEqual(audioInputDevices, comparer);

            // next check if the output devices list are exactly equal to the cache
            var areAudioOutputDeviceListsExactlyEqual = AudioOutputDevicesCache.SequenceEqual(audioOutputDevices, comparer);

            var isSoundTrayEnabledEqualToStartupCache = isWindowsStartUpEnabled != IsSoundTrayInWindowsStartUp(shortcutPath);

            NotifyIcon.ContextMenuStrip?.Items.Clear();

            // if the cache count doesn't match the existing count, we know there is a difference
            if (!isSoundTrayEnabledEqualToStartupCache || !areAudioInputDeviceListsExactlyEqual || !areAudioOutputDeviceListsExactlyEqual || DefaultAudioInputDeviceCache.FriendlyName != defaultAudioInputDevice.FriendlyName || DefaultAudioOutputDeviceCache.FriendlyName != defaultAudioOutputDevice.FriendlyName)
            {
                UpdateContextMenu(areAudioInputDeviceListsExactlyEqual, areAudioOutputDeviceListsExactlyEqual, audioInputDevices, audioOutputDevices, defaultAudioInputDevice, defaultAudioOutputDevice);
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