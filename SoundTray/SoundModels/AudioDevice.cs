namespace SoundTray.SoundModels
{
    /// <summary>
    /// A class that represents audio output devices
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Returns a unique identifier for the audio output device
        /// </summary>
        public string? ID { get; set; }

        /// <summary>
        /// Returns a friendly name for the audio output device
        /// </summary>
        public string? FriendlyName { get; set; }
    }
}
