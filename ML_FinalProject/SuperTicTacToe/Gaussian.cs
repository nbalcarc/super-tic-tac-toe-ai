// <copyright file="Gaussian.cs" company="Adam nassar &amp; Nathan Balcarcel">
// Copyright (c) Adam nassar &amp; Nathan Balcarcel. All rights reserved.
// </copyright>

namespace SuperTicTacToe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class for a Gaussian model.
    /// </summary>
    public class Gaussian
    {
        private static Random random = new Random();

        /// <summary>
        /// Generates a distribution using the box-muller method.
        /// </summary>
        /// <param name="mean">mean.</param>
        /// <param name="standard_deviation">standard_dev.</param>
        /// <returns>blah.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BoxMuller(double mean, double standard_deviation)
        {
            double ret = 0.0;
            double x = 0.0;
            double y = 0.0;
            while (ret >= 1.0 || ret == 0.0)
            {
                x = (random.NextDouble() * 2.0) - 1.0;
                y = (random.NextDouble() * 2.0) - 1.0;
                ret = (x * x) + (y * y);
            }

            // By this line we have the distribution
            ret = Math.Sqrt((Math.Log(ret) * -2.0) / ret) * x;

            // Apply the given parameters
            ret = mean + (ret * standard_deviation);
            return ret;
        }

        /// <summary>
        /// Given two numbers and a mutability, will generate a new number.
        /// </summary>
        /// <param name="num">num.</param>
        /// <param name="num1">num1.</param>
        /// <param name="mutability">mutability.</param>
        /// <returns>blah.</returns>
        public static double Next(double num, double num1, double mutability)
        {
            double distance = Math.Abs((num / 2.0) - (num1 / 2.0));

            // If the distance hits 0, then we will never generate anything other than the given number, so we set a minimum
            if (distance == 0)
            {
                distance = 0.0625;
            }

            // The averaging technique keeps the double from overflowing
            return BoxMuller((num / 2.0) + (num1 / 2.0), mutability * distance);
        }
    }
}
