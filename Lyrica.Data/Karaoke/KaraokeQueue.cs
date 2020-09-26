using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeQueue
    {
        public Dictionary<IGuildUser, KaraokeEntry> Queue { get; } = new Dictionary<IGuildUser, KaraokeEntry>();

        public IEnumerable<KaraokeEntry> NextUp => Queue.Skip(1).Select(x => x.Value);

        public KaraokeEntry? CurrentSinger => Queue.FirstOrDefault().Value;

        public KaraokeEntry? NextSinger(IGuildUser? user = null)
        {
            if (CurrentSinger != null)
                Queue.Remove(user ?? CurrentSinger?.User ?? Queue.FirstOrDefault().Key, out _);

            return CurrentSinger;
        }

        public void Add(SocketUser user, string? song = null) => Add((IGuildUser) user, song);

        public void Add(IGuildUser user, string? song = null)
        {
            var entry = new KaraokeEntry(user, song);
            Queue.TryAdd(user, entry);
        }

        public void Remove(SocketUser user) => Remove((IGuildUser) user);

        public void Remove(IGuildUser user)
        {
            if (Queue.TryGetValue(user, out var entry))
                Queue.Remove(user, out _);
        }

        public bool HasUser(SocketUser user) => HasUser((IGuildUser) user);

        public bool HasUser(IGuildUser user)
        {
            return Queue.TryGetValue(user, out _);
        }
    }
}