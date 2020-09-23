using Discord;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeEntry
    {
        public KaraokeEntry(IGuildUser user, string? song)
        {
            User = user;
            Song = song;
        }

        public IGuildUser User { get; set; }

        public string? Song { get; set; }
    }
}