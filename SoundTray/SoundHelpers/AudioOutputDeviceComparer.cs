using SoundTray.SoundModels;

namespace SoundTray
{
    class AudioOutputDeviceComparer : IEqualityComparer<AudioDevice>
    {
        public bool Equals(AudioDevice? x, AudioDevice? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            // Compare whichever fields define identity (ID typically)
            return string.Equals(x.ID, y.ID, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.FriendlyName, y.FriendlyName, System.StringComparison.Ordinal);
        }

        public int GetHashCode(AudioDevice obj)
        {
            if (obj is null) return 0;
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.ID?.ToUpperInvariant().GetHashCode() ?? 0);
                hash = hash * 23 + (obj.FriendlyName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}