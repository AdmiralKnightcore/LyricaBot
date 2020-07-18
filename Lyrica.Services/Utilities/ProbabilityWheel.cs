using System;
using System.Collections.Generic;
using System.Linq;

namespace Lyrica.Services.Utilities
{
    public class ProbabilityWheel<T>
    {
        private static readonly Random rnd = new Random();
        private readonly List<(int probability, T item)> _items = new List<(int, T)>();
        private int _accumulatedProbability;

        public void Add(T add, int probability)
        {
            _accumulatedProbability += probability;
            _items.Add((_accumulatedProbability, add));
        }

        public T SelectRandom()
        {
            var r = rnd.Next(1, _accumulatedProbability + 1);
            return _items.First(i => r <= i.probability).item;
        }
    }
}