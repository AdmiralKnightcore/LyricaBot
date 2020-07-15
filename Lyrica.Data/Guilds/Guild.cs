using System.ComponentModel.DataAnnotations.Schema;

namespace Lyrica.Data.Guilds
{
    public class Guild
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        public ulong Owner { get; set; }

        public ulong? LolaBlessGame { get; set; } = null!;
    }
}