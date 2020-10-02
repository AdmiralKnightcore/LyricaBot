using System.ComponentModel.DataAnnotations.Schema;

namespace Lyrica.Data.GenshinImpact
{
    public class Save
    {
        public Save(ulong id)
        {
            Id = id;
            Region = (Region) (id / 100_000_000);
        }

        public Save() { }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public ulong Id { get; set; }

        public Region Region { get; set; }
    }
}