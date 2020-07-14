using System;

namespace Lyrica.Data.Bless
{
    public class BlessingResult
    {
        public Guid Id { get; set; }

        public BlessingType Type { get; set; }

        public int Amount { get; set; }
    }
}