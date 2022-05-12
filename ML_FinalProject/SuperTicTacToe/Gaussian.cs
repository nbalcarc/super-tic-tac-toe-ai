// <copyright file="Gaussian.cs" company="Adam Nassar &amp; Nathan Balcarcel">
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
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
