using NAudio.CoreAudioApi;
using SoundTray.SoundModels;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SoundTray
{
    /// <summary>
    /// The WinForms class SoundTrayStatus.
    /// </summary>
    public partial class SoundTrayStatus : Form
    {
        public SoundTrayStatus()
        {
            InitializeComponent();

            PopulateInputAndOutputDevicesGrid();

            // Subscribe to the event
            inputDevicesDataGridView.CellContentClick += InputDevicesDataGridView_CellContentClick;
            outputDevicesDataGridView.CellContentClick += OutputDevicesDataGridView_CellContentClick;
            FormClosing += SoundTrayStatus_FormClosing;
        }

        /// <summary>
        /// Gets a list of all audio input devices
        /// </summary>
        /// <returns></returns>
        public static List<AudioDevice> GetAudioInputDevices()
        {
            // Create an MMDeviceEnumerator instance
            using var enumerator = new MMDeviceEnumerator();
            // Enumerate active render (output) devices
            return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(a => new AudioDevice() { ID = a.ID, FriendlyName = a.FriendlyName }).ToList();
        }

        /// <summary>
        /// Gets a list of audio output devices
        /// </summary>
        /// <returns></returns>
        public static List<AudioDevice> GetAudioOutputDevices()
        {
            // Create an MMDeviceEnumerator instance
            using var enumerator = new MMDeviceEnumerator();
            // Enumerate active render (output) devices
            return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(a => new AudioDevice() { ID = a.ID, FriendlyName = a.FriendlyName }).ToList();
        }

        /// <summary>
        /// Sets the audio input device
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <returns></returns>
        internal EventHandler SetDefaultAudioDevice(AudioDevice audioDevice, ERole eRole)
        {
            return (object? sender, EventArgs e) =>
            {
                using var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDevice(audioDevice.ID);
                if (device != null)
                {
                    device.AudioEndpointVolume.Mute = false;
                    // Set the default audio endpoint
                    var policyConfig = new PolicyConfig();

                    // validate & log deviceId so we can debug failures like the one you saw
                    if (string.IsNullOrWhiteSpace(audioDevice.ID))
                    {
                        Debug.WriteLine("SetDefaultEndpoint called with null/empty deviceId");
                    }
                    else
                    {
                        if (eRole == ERole.eCommunications)
                        {
                            Debug.WriteLine($"Setting default input endpoint to: {audioDevice.FriendlyName}'");
                        }
                        else if (eRole == ERole.eMultimedia || eRole == ERole.eConsole)
                        {
                            Debug.WriteLine($"Setting default output endpoint to: {audioDevice.FriendlyName}'");
                        }

                        policyConfig.SetDefaultEndpoint(audioDevice.ID, eRole);

                        PopulateInputAndOutputDevicesGrid();
                    }
                }
            };
        }

        /// <summary>
        /// Event shows the Control Panel Status form
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The EventArgs.</param>
        internal void ShowControlPanelStatus(object sender, EventArgs e)
        {
            PopulateInputAndOutputDevicesGrid();
            Show();
        }

        /// <summary>
        /// Method is called to populate the input and output devices DataGridViews
        /// </summary>
        private void PopulateInputAndOutputDevicesGrid()
        {
            SoundTrayApplicationContext.GetDefaultAudioDevices();

            inputDevicesDataGridView.Rows.Clear();
            outputDevicesDataGridView.Rows.Clear();

            var inputDevices = GetAudioInputDevices();
            var outputDevices = GetAudioOutputDevices();

            var enableDisableInputDeviceCheckBoxColumn =
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = string.Empty,
                    Name = "enableDisableInputDeviceCheckBox",
                };

            var inputDeviceNameColumn =
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Input Device",
                    Name = "friendlyNameTextBox",
                    ReadOnly = true,
                };

            var enableDisableOutputDeviceCheckBoxColumn =
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = string.Empty,
                    Name = "enableDisableOutputDeviceCheckBox",

                };

            var outputDeviceNameColumn =
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Output Device",
                    Name = "friendlyNameTextBox",
                    ReadOnly = true,
                };

            inputDevicesDataGridView.ColumnCount = 0;
            inputDevicesDataGridView.RowCount = 0;

            inputDevicesDataGridView.Columns.Add(enableDisableInputDeviceCheckBoxColumn);
            inputDevicesDataGridView.Columns.Add(inputDeviceNameColumn);

            // Center align column headers for DataGridView
            inputDevicesDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            inputDevicesDataGridView.Columns[0].HeaderText = "Enable/Disable";
            inputDevicesDataGridView.Columns[1].HeaderText = "Input Device Name";

            // Disable selection highlighting on inputDevicesDataGridView
            inputDevicesDataGridView.DefaultCellStyle.SelectionBackColor = inputDevicesDataGridView.DefaultCellStyle.BackColor;
            inputDevicesDataGridView.DefaultCellStyle.SelectionForeColor = inputDevicesDataGridView.DefaultCellStyle.ForeColor;

            // Disable selection highlighting on outputDevicesDataGridView
            outputDevicesDataGridView.DefaultCellStyle.SelectionBackColor = outputDevicesDataGridView.DefaultCellStyle.BackColor;
            outputDevicesDataGridView.DefaultCellStyle.SelectionForeColor = outputDevicesDataGridView.DefaultCellStyle.ForeColor;

            // populate the input devices datagridview
            foreach (var (inputDevice, index) in inputDevices.Select((inputDevice, index) => (inputDevice, index)))
            {
                FormatInputDataGridView(inputDevice.FriendlyName ?? string.Empty, inputDevice.FriendlyName, index);
            }

            outputDevicesDataGridView.ColumnCount = 0;
            outputDevicesDataGridView.RowCount = 0;

            outputDevicesDataGridView.Columns.Add(enableDisableOutputDeviceCheckBoxColumn);
            outputDevicesDataGridView.Columns.Add(outputDeviceNameColumn);

            // Center align column headers for DataGridView
            outputDevicesDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            outputDevicesDataGridView.Columns[0].HeaderText = "Enable/Disable";
            outputDevicesDataGridView.Columns[1].HeaderText = "Output Device Name";

            // populate the output devices datagridview
            foreach (var (outputDevice, index) in outputDevices.Select((outputDevice, index) => (outputDevice, index)))
            {
                FormatOutputDataGridView(outputDevice.FriendlyName ?? string.Empty, outputDevice.FriendlyName, index);
            }
        }

        /// <summary>
        /// Formats the inputDevicesDataGridView
        /// </summary>
        /// <param name="friendlyName">The friendly name of the input device for that row.</param>
        /// <param name="deviceID">The ID of the output device</param>
        /// <param name="index">The row index for that row.</param>
        private void FormatInputDataGridView(string friendlyName, string deviceID, int index)
        {
            inputDevicesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            inputDevicesDataGridView.RowHeadersVisible = false;
            inputDevicesDataGridView.AllowUserToAddRows = false;
            inputDevicesDataGridView.AllowUserToDeleteRows = false;
            inputDevicesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            inputDevicesDataGridView.MultiSelect = false;

            inputDevicesDataGridView.Rows.Add();

            // checks or unchecks the input device checkboxes
            if (Program.enabledInputAudioDevices.Contains(deviceID))
            {
                inputDevicesDataGridView.Rows[index].Cells[0].Value = true; // set the checkbox to checked
                                                                            // grey out the default device row
            }
            else
            {
                inputDevicesDataGridView.Rows[index].Cells[0].Value = false; // set the checkbox to unchecked
            }

            if (SoundTrayApplicationContext.DefaultAudioInputDeviceCache.FriendlyName == friendlyName)
            {
                // Disable selection highlighting on outputDevicesDataGridView
                inputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionBackColor = Color.LightGray;
                inputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionForeColor = Color.Black;

                inputDevicesDataGridView.Rows[index].Cells[0].Style.BackColor = Color.LightGray;
                inputDevicesDataGridView.Rows[index].Cells[0].Style.ForeColor = Color.Black;

                inputDevicesDataGridView.Rows[index].Cells[1].Style.BackColor = Color.LightGray;
                inputDevicesDataGridView.Rows[index].Cells[1].Style.ForeColor = Color.Black;

                inputDevicesDataGridView.Rows[index].Cells[1].Value = friendlyName + " -- DEFAULT";
            }
            else
            {
                // Disable selection highlighting on inputDevicesDataGridView
                inputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionBackColor = inputDevicesDataGridView.DefaultCellStyle.BackColor;
                inputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionForeColor = Color.Black;

                // Disable selection highlighting on inputDevicesDataGridView
                inputDevicesDataGridView.Rows[index].Cells[0].Style.BackColor = inputDevicesDataGridView.DefaultCellStyle.BackColor;
                inputDevicesDataGridView.Rows[index].Cells[0].Style.ForeColor = inputDevicesDataGridView.DefaultCellStyle.ForeColor;

                // Disable selection highlighting on inputDevicesDataGridView
                inputDevicesDataGridView.Rows[index].Cells[1].Style.BackColor = inputDevicesDataGridView.DefaultCellStyle.BackColor;
                inputDevicesDataGridView.Rows[index].Cells[1].Style.ForeColor = inputDevicesDataGridView.DefaultCellStyle.ForeColor;

                inputDevicesDataGridView.Rows[index].Cells[1].Value = friendlyName;
            }
        }

        /// <summary>
        /// Formats the outputDevicesDataGridView
        /// </summary>
        /// <param name="friendlyName">The friendly name of the output device for that row.</param>
        /// <param name="deviceID">The ID of the output device</param>
        /// <param name="index">The row index for that row.</param>
        private void FormatOutputDataGridView(string friendlyName, string deviceID, int index)
        {
            outputDevicesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            outputDevicesDataGridView.RowHeadersVisible = false;
            outputDevicesDataGridView.AllowUserToAddRows = false;
            outputDevicesDataGridView.AllowUserToDeleteRows = false;
            outputDevicesDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            outputDevicesDataGridView.MultiSelect = false;

            outputDevicesDataGridView.Rows.Add();

            // checks or unchecks the input device checkboxes
            if (Program.enabledOutputAudioDevices.Contains(deviceID))
            {
                outputDevicesDataGridView.Rows[index].Cells[0].Value = true; // set the checkbox to checked
                                                                             // grey out the default device row
            }
            else
            {
                outputDevicesDataGridView.Rows[index].Cells[0].Value = false; // set the checkbox to unchecked
            }

            // grey out the default device row
            if (SoundTrayApplicationContext.DefaultAudioOutputDeviceCache.FriendlyName == friendlyName)
            {
                // Disable selection highlighting on outputDevicesDataGridView
                outputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionBackColor = Color.LightGray;
                outputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionForeColor = Color.Black;

                outputDevicesDataGridView.Rows[index].Cells[0].Style.BackColor = Color.LightGray;
                outputDevicesDataGridView.Rows[index].Cells[0].Style.ForeColor = Color.Black;

                outputDevicesDataGridView.Rows[index].Cells[1].Style.BackColor = Color.LightGray;
                outputDevicesDataGridView.Rows[index].Cells[1].Style.ForeColor = Color.Black;

                outputDevicesDataGridView.Rows[index].Cells[1].Value = friendlyName + " -- DEFAULT";
            }
            else
            {
                // Disable selection highlighting on outputDevicesDataGridView
                outputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionBackColor = outputDevicesDataGridView.DefaultCellStyle.BackColor;
                outputDevicesDataGridView.Rows[index].DefaultCellStyle.SelectionForeColor = Color.Black;

                // Disable selection highlighting on outputDevicesDataGridView
                outputDevicesDataGridView.Rows[index].Cells[0].Style.BackColor = outputDevicesDataGridView.DefaultCellStyle.BackColor;
                outputDevicesDataGridView.Rows[index].Cells[0].Style.ForeColor = outputDevicesDataGridView.DefaultCellStyle.ForeColor;

                // Disable selection highlighting on outputDevicesDataGridView
                outputDevicesDataGridView.Rows[index].Cells[1].Style.BackColor = outputDevicesDataGridView.DefaultCellStyle.BackColor;
                outputDevicesDataGridView.Rows[index].Cells[1].Style.ForeColor = outputDevicesDataGridView.DefaultCellStyle.ForeColor;

                outputDevicesDataGridView.Rows[index].Cells[1].Value = friendlyName;
            }
        }

        /// <summary>
        /// Finds the Default Device Row index based on device friendly name
        /// </summary>
        /// <param name="dataGridView">The target DataGridView</param>
        /// <param name="defaultDeviceFriendlyName">The friendly name of the default device</param>
        private int GetDefaultDeviceRowIndex(DataGridView dataGridView, string defaultDeviceFriendlyName)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (dataGridView.Rows[i].Cells[1].Value?.ToString() == defaultDeviceFriendlyName)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        /// <summary>
        /// Captures event fires when a cell is clicked in the inputDevicesDataGridView
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The DataGridViewCellEventArgs.</param>
        private void InputDevicesDataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // only action a click on a non default device row
            int defaultInputDeviceRowIndex = GetDefaultDeviceRowIndex(inputDevicesDataGridView, SoundTrayApplicationContext.DefaultAudioInputDeviceCache.FriendlyName);

            // Check if the clicked cell is in the checkbox column
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && defaultInputDeviceRowIndex != e.RowIndex)
            {
                // Commit the edit immediately to get the new value
                inputDevicesDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

                // Get the checkbox value
                bool isChecked = (bool)inputDevicesDataGridView.Rows[e.RowIndex].Cells[0].Value;

                // Get the device name
                string deviceName = inputDevicesDataGridView.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? string.Empty;

                List<string> enabledInputAudioDevices = new List<string>();
                List<string> enabledOutputAudioDevices = new List<string>();

                int rowCounter = 0;

                var inputAudioDevices = GetAudioInputDevices();
                var outputAudioDevices = GetAudioOutputDevices();

                // populate the enabledInputAudioDevices list of ID's so it can be saved as a CSV list of values
                foreach (var row in inputDevicesDataGridView.Rows)
                {
                    var dataGridViewRow = (DataGridViewRow)row;

                    // if the checkbox for that device is checked
                    if ((bool)inputDevicesDataGridView.Rows[rowCounter].Cells[0].Value == true)
                    {
                        enabledInputAudioDevices.Add(inputAudioDevices.FirstOrDefault(i => i.FriendlyName == inputDevicesDataGridView.Rows[rowCounter].Cells[1].Value.ToString().Replace(" -- DEFAULT", string.Empty)).FriendlyName);
                    }

                    rowCounter++;
                }

                rowCounter = 0;

                // populate the enabledOutputAudioDevices list of ID's so it can be saved as a CSV list of values
                foreach (var row in outputDevicesDataGridView.Rows)
                {
                    var dataGridViewRow = (DataGridViewRow)row;

                    if ((bool)outputDevicesDataGridView.Rows[rowCounter].Cells[0].Value == true)
                    {
                        enabledOutputAudioDevices.Add(outputAudioDevices.FirstOrDefault(i => i.FriendlyName == outputDevicesDataGridView.Rows[rowCounter].Cells[1].Value.ToString().Replace(" -- DEFAULT", string.Empty)).FriendlyName);
                    }

                    rowCounter++;
                }

                SaveSettings(string.Join(",", enabledInputAudioDevices), string.Join(",", enabledOutputAudioDevices));
            }
        }

        /// <summary>
        /// Captures event fires when a cell is clicked in the outputDevicesDataGridView
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The DataGridViewCellEventArgs.</param>
        private void OutputDevicesDataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // only action a click on a non default device row
            int defaultOutputDeviceRowIndex = GetDefaultDeviceRowIndex(outputDevicesDataGridView, SoundTrayApplicationContext.DefaultAudioOutputDeviceCache.FriendlyName);

            // Check if the clicked cell is in the checkbox column
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && defaultOutputDeviceRowIndex != e.RowIndex)
            {
                // Commit the edit immediately to get the new value
                outputDevicesDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

                // Get the checkbox value
                bool isChecked = (bool)outputDevicesDataGridView.Rows[e.RowIndex].Cells[0].Value;

                // Get the device name
                string deviceName = outputDevicesDataGridView.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? string.Empty;

                List<string> enabledInputAudioDevices = new List<string>();
                List<string> enabledOutputAudioDevices = new List<string>();

                int rowCounter = 0;

                var inputAudioDevices = GetAudioInputDevices();
                var outputAudioDevices = GetAudioOutputDevices();

                // populate the enabledInputAudioDevices list of ID's so it can be saved as a CSV list of values
                foreach (var row in inputDevicesDataGridView.Rows)
                {
                    var dataGridViewRow = (DataGridViewRow)row;

                    // if the checkbox for that device is checked
                    if ((bool)inputDevicesDataGridView.Rows[rowCounter].Cells[0].Value == true)
                    {
                        enabledInputAudioDevices.Add(inputAudioDevices.FirstOrDefault(i => i.FriendlyName == inputDevicesDataGridView.Rows[rowCounter].Cells[1].Value.ToString().Replace(" -- DEFAULT", string.Empty)).FriendlyName);
                    }

                    rowCounter++;
                }

                rowCounter = 0;

                // populate the enabledOutputAudioDevices list of ID's so it can be saved as a CSV list of values
                foreach (var row in outputDevicesDataGridView.Rows)
                {
                    var dataGridViewRow = (DataGridViewRow)row;

                    // if the checkbox for that device is checked
                    if ((bool)outputDevicesDataGridView.Rows[rowCounter].Cells[0].Value == true)
                    {
                        enabledOutputAudioDevices.Add(outputAudioDevices.FirstOrDefault(i => i.FriendlyName == outputDevicesDataGridView.Rows[rowCounter].Cells[1].Value.ToString().Replace(" -- DEFAULT", string.Empty)).FriendlyName);
                    }

                    rowCounter++;
                }

                SaveSettings(string.Join(",", enabledInputAudioDevices), string.Join(",", enabledOutputAudioDevices));
            }
        }

        /// <summary>
        /// Fires on closing the top right X button the Status form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SoundTrayStatus_FormClosing(Object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // Cancel the close event
            Hide();
        }

        /// <summary>
        /// Loads the settings from the settings.cfg file. If the file does not exist or is invalid, it initializes the enabled input and output audio devices with all available devices and saves them to the settings.cfg file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                var settingsLines = File.ReadAllLines("settings.cfg");

                if (settingsLines.Length == 2)
                {
                    if (settingsLines[0].StartsWith("EnableInputDeviceIds"))
                    {
                        Program.enabledInputAudioDevices = settingsLines[0].Split("|")[1].Split(",").ToList();
                    }

                    if (settingsLines[1].StartsWith("EnableOutputDeviceIds"))
                    {
                        Program.enabledOutputAudioDevices = settingsLines[1].Split("|")[1].Split(",").ToList();
                    }
                }
                else
                {
                    Program.enabledInputAudioDevices = GetAudioInputDevices().Select(i => i.FriendlyName).ToList();
                    Program.enabledOutputAudioDevices = GetAudioOutputDevices().Select(o => o.FriendlyName).ToList();
                    SaveSettings(string.Join(",", Program.enabledInputAudioDevices), string.Join(",", Program.enabledOutputAudioDevices));
                }
            }
            catch
            {
                Program.enabledInputAudioDevices = GetAudioInputDevices().Select(i => i.FriendlyName).ToList();
                Program.enabledOutputAudioDevices = GetAudioOutputDevices().Select(o => o.FriendlyName).ToList();
                SaveSettings(string.Join(",", Program.enabledInputAudioDevices), string.Join(",", Program.enabledOutputAudioDevices));
            }
        }

        internal static void SaveSettings(string enabledInputAudioDeviceFriendlyNames, string enabledOutputAudioDeviceFriendlyNames)
        {
            string[] settingsNames = { "EnableInputDeviceIds", "EnableOutputDeviceIds" };

            List<string> settings = new List<string>();

            settings.Add(settingsNames[0] + "|" + enabledInputAudioDeviceFriendlyNames);

            settings.Add(settingsNames[1] + "|" + enabledOutputAudioDeviceFriendlyNames);

            File.WriteAllLinesAsync("settings.cfg", settings);
        }
    }

    // ERole enum definition
    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    // PolicyConfig COM interop definition - concrete signatures instead of placeholders
    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        // These signatures occupy the vtable slots before SetDefaultEndpoint on many Windows builds.
        // Exact signatures/params are not part of a public contract so include the commonly used forms.
        [PreserveSig] int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceName, out IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceName, [MarshalAs(UnmanagedType.Bool)] out bool pbDefault, out IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceName);
        [PreserveSig] int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string deviceName, IntPtr pEndpointFormat, IntPtr pMixFormat);
        [PreserveSig] int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceName, out long pmftDefault, out long pmftMinimum);
        [PreserveSig] int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string deviceName, long pmftDefault, long pmftMinimum);
        [PreserveSig] int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceName, out IntPtr pMode);
        [PreserveSig] int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string deviceName, IntPtr mode);
        [PreserveSig] int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceName, ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string deviceName, ref PROPERTYKEY key, ref PROPVARIANT pv);

        // The SetDefaultEndpoint slot (commonly follows the above)
        [PreserveSig]
        int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ERole eRole);

        [PreserveSig]
        int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string deviceId, int visibility);
    }

    /// <summary>
    /// The PolicyConfig in which to change default input or output devices.
    /// </summary>
    class PolicyConfig
    {
        private static readonly Guid CLSID_PolicyConfig = new Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9");

        private IPolicyConfig _policyConfig;

        public PolicyConfig()
        {
            Type type = Type.GetTypeFromCLSID(CLSID_PolicyConfig);
            if (type == null) throw new InvalidOperationException("PolicyConfig CLSID not found on this machine.");
            _policyConfig = (IPolicyConfig)Activator.CreateInstance(type);
            if (_policyConfig == null) throw new InvalidOperationException("Failed to create PolicyConfig COM instance.");
        }

        public void SetDefaultEndpoint(string deviceId, ERole role)
        {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            // Call returns HRESULT because we used PreserveSig
            int hr = _policyConfig.SetDefaultEndpoint(deviceId, role);
            if (hr != 0)
            {
                Debug.WriteLine($"SetDefaultEndpoint failed: HRESULT=0x{hr:X8}, deviceId='{deviceId}', role={role}");
                // Raise a managed exception with the HRESULT so you can see the cause in logs/debugger
                Marshal.ThrowExceptionForHR(hr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr p;
        public int cVal;
    }
}