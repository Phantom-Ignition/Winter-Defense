using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winter_Defense.Extensions
{
    public static class MathUtils
    {
        public static double PiOver180 = Math.PI / 180.0;

        public static double SinInterpolation(double a, double b, double t)
        {
            return a + Math.Sin(t * PiOver180) * (b - a);
        }

        public static double TriangularWave(double a, double b, double t)
        {
            return a + Math.Abs((b - a) - t % (2 * (b - a)));
        }
    }
}
