using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vindinium.NEAT;

namespace Testy
{
    public class MockRandom : IRandomGenerator
    {
        public int[] ValueToReturn { get; set; }
        private int index;
        public int Next(int minValue, int maxValue)
        {
            return ValueToReturn[index++ % ValueToReturn.Length];
        }
    }
}
