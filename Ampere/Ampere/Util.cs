using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ampere
{
    static class Util
    {
        public static int Clamp(int val, int min, int max) => 
            val < min 
                ? min 
                : val > max
                    ? max
                    : val; 
    }

    static class Rnd
    {
        private static Random _rnd { get; } = new Random();
        public static int Next => _rnd.Next();
        public static int NextRange(int min, int max) => _rnd.Next(min, max);
    }
}
