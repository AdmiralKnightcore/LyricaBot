using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Lyrica.Data.Users;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeSetting
    {
        public KaraokeSetting(IVoiceChannel karaokeVc, ITextChannel karaokeChannel, IRole singingRole)
        {
            KaraokeVc = karaokeVc.Id;
            KaraokeChannel = karaokeChannel.Id;
            SingingRole = singingRole.Id;
        }

        public KaraokeSetting() { }

        public Guid Id { get; set; }

        public ulong KaraokeVc { get; set; }

        public ulong KaraokeChannel { get; set; }

        public ulong SingingRole { get; set; }

        public bool Intermission { get; set; } = true;

        public ulong? KaraokeMessage { get; set; }

        public List<User> VoteSkippedUsers { get; set; } = new List<User>();

        public List<KaraokeEntry> Queue { get; set; } = new List<KaraokeEntry>();

        public IEnumerable<KaraokeEntry> NextUp => Queue.OrderBy(e => e.Date).Skip(1).ToList();

        public KaraokeEntry? CurrentSinger => Queue.OrderBy(e => e.Date).FirstOrDefault();

        public bool TryNextSinger(IUser? user, out KaraokeEntry? entry)
        {
            entry = null!;
            if (CurrentSinger is null)
                return false;
            if (user is not null)
                return TryRemove(user, out entry);
            if (Queue.Any())
                Queue.Remove(Queue
                    .OrderBy(e => e.Date).First());

            return false;
        }

        public void Add(User user, string? song = null)
        {
            var entry = new KaraokeEntry(user, song);
            Queue.Add(entry);
        }

        public bool TryRemove(IUser user, out KaraokeEntry? entry)
        {
            entry = Queue
                .FirstOrDefault(e => e.User.Id == user.Id);
            if (entry is not null)
                Queue.Remove(entry);

            return entry is not null;
        }

        public bool HasUser(IUser user)
        {
            return Queue.Any(e => e.User.Id == user.Id);
        }
    }
}