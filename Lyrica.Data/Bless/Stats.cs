using System;
using System.Collections.Generic;
using System.Linq;

namespace Lyrica.Data.Bless
{
    public class Stats
    {
        public Guid Id { get; set; }

        public int Rolls { get; set; }

        public List<BlessingResult> BlessingResults { get; set; } = new List<BlessingResult>();

        public BlessingResult this[BlessingType type] => GetOrAdd(type);

        private BlessingResult GetOrAdd(BlessingType type)
        {
            var blessing = BlessingResults.FirstOrDefault(b => b.Type == type);
            if (blessing is null)
            {
                blessing = new BlessingResult { Type = type };
                BlessingResults.Add(blessing);
            }

            return blessing;
        }
    }
}