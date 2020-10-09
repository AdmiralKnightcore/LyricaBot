using System;
using Lyrica.Data.Users;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeEntry
    {
        public KaraokeEntry(User user, string? song)
        {
            User = user;
            Song = song;
            Date = DateTime.UtcNow;
        }

        public KaraokeEntry() { }

        public Guid Id { get; set; }

        public User User { get; set; }

        public string? Song { get; set; }

        public DateTime Date { get; set; }
    }
}