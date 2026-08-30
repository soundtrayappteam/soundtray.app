namespace SoundTray
{
    internal static class Program
    {
        public static List<string> enabledInputAudioDevices = new List<string>();
        public static List<string> enabledOutputAudioDevices = new List<string>();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string appName = "Sound Tray";

            var mutex = new Mutex(true, appName, out var createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SoundTrayApplicationContext());

            GC.KeepAlive(mutex);
        }
    }
}