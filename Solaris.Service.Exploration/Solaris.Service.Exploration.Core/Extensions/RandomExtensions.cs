using System;

namespace Solaris.Service.Exploration.Core.Extensions
{
    public static class RandomExtensions
    {
        public static float NextFloat(this Random generator, float min, float max)
        {
            var range = (double) (max - min);
            var sample = generator.NextDouble();
            var scaled = sample * range + min;
            return (float) scaled;
        }
    }
}