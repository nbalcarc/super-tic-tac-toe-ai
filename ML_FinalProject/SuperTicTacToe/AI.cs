// <copyright file="AI.cs" company="Adam Nassar and Nathan Balcarcel">
// Copyright (C) 2022  Adam Nassar and Nathan Balcarcel
//
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
    /// Class for AI.
    /// </summary>
    public class AI
    {
        // Used to decide where to move
        private NeuralNetwork placeNN;

        // Used to decide what board to place on (used occasionally)
        private NeuralNetwork boardNN;

        // For databasing, should be unique
        // private int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="AI"/> class.
        /// </summary>
        public AI()
            : this(new NeuralNetwork(99, new int[] { 162, 81, 9 }, false), new NeuralNetwork(90, new int[] { 36, 18, 9 }, false))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AI"/> class.
        /// </summary>
        /// <param name="place">place.</param>
        /// <param name="board">board.</param>
        public AI(NeuralNetwork place, NeuralNetwork board)
        {
            this.placeNN = place;
            this.boardNN = board;
        }

        /// <summary>
        /// reproduce two AIs, return a new AI.
        /// </summary>
        /// <param name="parent">parent.</param>
        /// <param name="parent1">parent1.</param>
        /// <returns>blah.</returns>
        public static AI Reproduce(AI parent, AI parent1)
        {
            return new AI(parent.placeNN.ReproduceWith(parent1.placeNN), parent.boardNN.ReproduceWith(parent1.boardNN));
        }

        /// <summary>
        /// Returns which tile of the 9 open options to go in.
        /// </summary>
        /// <param name="board">board.</param>
        /// <param name="focus_board">focus_board.</param>
        /// <returns>blah.</returns>
        public int NextMove(int[] board, int focus_board)
        {
            // Must choose which of the 9 tiles to go in
            double[] results = this.placeNN.Calculate(board);

            int[] ordered = this.ReturnSorted(results);

            for (int i = 0; i < 9; i++)
            {
                // Console.WriteLine($"DEBUGGING1 results[{i}]: {results[i]}");
            }

            // Ensure a valid option is returned
            for (int i = 0; i < 9; i++)
            {
                // If the given tile in the focused board is open, return that tile
                if (board[(focus_board * 9) + ordered[i]] == 0 && board[81 + focus_board] == 0)
                {
                    return ordered[i];
                }
            }

            // Console.WriteLine($"DEBUGGING, returned -1 as next move");
            // Console.WriteLine($"DEBUGGING ######### {board[80]}, {board[81]}, {board[82]}, {board[83]}, {board[84]}, {board[85]}, {board[86]}, {board[87]}, {board[88]}");
            return -1;
        }

        /// <summary>
        /// Returns its choice of what board to play in, takes whether it is the starting move or not as a parameter (expects inverted board if false).
        /// </summary>
        /// <param name="board">board.</param>
        /// <param name="start">start.</param>
        /// <returns>blah.</returns>
        public int ChooseBoard(int[] board, bool start)
        {
            double[] results = this.boardNN.Calculate(board);

            // If not start of game, wants to minimize opponent's advantage
            int[] ordered = start ? this.ReturnSorted(results) : this.ReturnSortedInverted(results);

            // Ensure a valid option is returned
            for (int i = 0; i < 9; i++)
            {
                // If the given board is not won, return that board
                if (board[81 + ordered[i]] == 0)
                {
                    return ordered[i];
                }
            }

            return 0;
        }

        /// <summary>
        /// Given the output of a neural network (assumes output size of 9), return a list of integers representing the sorted order of the array.
        /// </summary>
        /// <param name="results">results.</param>
        /// <returns>blah.</returns>
        public int[] ReturnSorted(double[] results)
        {
            int len = results.Length;
            int[] ret = new int[len];
            bool[] done = new bool[len];
            double max;
            int choice = 0;

            // For every ranking
            for (int i = 0; i < len; i++)
            {
                max = double.MinValue;

                // For every element in the list (find next biggest)
                for (int j = 0; j < len; j++)
                {
                    // If this element is still open
                    if (!done[j] && results[j] > max)
                    {
                        max = results[j];

                        // Make note of the largest so far
                        choice = j;
                    }
                }

                // Set the ith place as the current choice
                ret[i] = choice;

                // Mark the choice element as closed
                done[choice] = true;
            }

            return ret;
        }

        private int[] ReturnSortedInverted(double[] results)
        {
            int len = results.Length;
            int[] ret = new int[len];
            bool[] done = new bool[len];
            double min;
            int choice = 0;

            // For every ranking
            for (int i = 0; i < len; i++)
            {
                min = double.MaxValue;

                // For every element in the list (find next smallest)
                for (int j = 0; j < len; j++)
                {
                    // If this element is still open
                    if (!done[j] && results[j] < min)
                    {
                        min = results[j];

                        // Make note of the largest so far
                        choice = j;
                    }
                }

                // Set the ith place as the current choice
                ret[i] = choice;

                // Mark the choice element as closed
                done[choice] = true;
            }

            return ret;
        }
    }
}
