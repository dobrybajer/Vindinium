using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redzen.Numerics;

namespace vindinium.NEAT
{
    public interface IRandomGenerator
    {
        int Next(int minValue, int maxValue);
    }

    public class RandomGenerator : IRandomGenerator
    {
        public int Next(int minValue, int maxValue)
        {
            return new XorShiftRandom().Next(minValue, maxValue);
        }
    }
}
