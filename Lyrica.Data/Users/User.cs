﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Lyrica.Data.Bless;
using Lyrica.Data.GenshinImpact;

namespace Lyrica.Data.Users
{
    public class User
    {
        public User(IGuildUser user)
        {
            Id = user.Id;
            UserCreatedAt = user.CreatedAt;
            JoinedAt = user.JoinedAt;
            Stats = new Stats();
        }

        public User() { }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        public DateTimeOffset UserCreatedAt { get; set; }

        public DateTimeOffset? JoinedAt { get; set; }

        public DateTimeOffset LastSeenAt { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? HasYear { get; set; }

        public TimeSpan? Timezone { get; set; }

        public Stats Stats { get; set; }

        public GenshinAccount? ActiveGenshinAccount { get; set; }

        public Save? ActiveSave { get; set; }

        public List<GenshinAccount> GenshinAccounts { get; set; } = new List<GenshinAccount>();
    }
}