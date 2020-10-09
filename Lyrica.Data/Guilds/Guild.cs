﻿using System.ComponentModel.DataAnnotations.Schema;
using Lyrica.Data.Karaoke;

namespace Lyrica.Data.Guilds
{
    public class Guild
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        public ulong Owner { get; set; }

        public ulong? LolaBlessGame { get; set; }

        public KaraokeSetting? Karaoke { get; set; }
    }
}