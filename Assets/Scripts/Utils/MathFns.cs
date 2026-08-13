using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class MathFns
    {
        public static int Mod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }
}
