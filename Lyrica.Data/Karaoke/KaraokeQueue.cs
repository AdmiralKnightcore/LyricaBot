using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Lyrica.Data.Karaoke
{
    public class KaraokeQueue
    {
        public List<KaraokeEntry> Queue { get; set; } = new List<KaraokeEntry>();

        public IEnumerable<KaraokeEntry> NextUp => Queue.Skip(1);

        public KaraokeEntry? CurrentSinger => Queue.FirstOrDefault();

        public void NextSinger(IUser? user)
        {
            if (CurrentSinger == null)
                return;
            if (user != null)
                Remove(user);
            else Queue.RemoveAt(0);
        }

        public void Add(SocketUser user, string? song = null) => Add((IGuildUser) user, song);

        public void Add(IGuildUser user, string? song = null)
        {
            var entry = new KaraokeEntry(user, song);
            Queue.Add(entry);
        }
        public void Remove(IUser user)
        {
            var entry = Queue.FirstOrDefault(e => e.User.Id == user.Id);
            if (entry != null)
                Queue.Remove(entry);
        }

        public bool HasUser(IUser user)
        {
            return Queue.Any(e => e.User.Id == user.Id);
        }
    }
}