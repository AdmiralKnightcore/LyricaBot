using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeQueue
    {
        public List<KaraokeEntry> Queue { get; } = new List<KaraokeEntry>();

        public IEnumerable<KaraokeEntry> NextUp => Queue.Skip(1);

        public KaraokeEntry? CurrentSinger => Queue.FirstOrDefault();

        public KaraokeEntry? NextSinger(IGuildUser? user = null)
        {
            if (CurrentSinger != null)
                Queue.RemoveAt(0);

            return CurrentSinger;
        }

        public void Add(SocketUser user, string? song = null) => Add((IGuildUser) user, song);

        public void Add(IGuildUser user, string? song = null)
        {
            var entry = new KaraokeEntry(user, song);
            Queue.Add(entry);
        }

        public void Remove(SocketUser user) => Remove((IGuildUser) user);

        public void Remove(IGuildUser user)
        {
            var entry = Queue.FirstOrDefault(e => e.User == user);
                if(entry != null)
                    Queue.Remove(entry);
        }

        public bool HasUser(SocketUser user) => HasUser((IGuildUser) user);

        public bool HasUser(IGuildUser user)
        {
            return Queue.Any(e => e.User == user);
        }
    }
}