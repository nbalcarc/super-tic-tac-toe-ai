// <copyright file="NeuralNetwork.cs" company="Adam Nassar and Nathan Balcarcel">
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
    /// Constructor for the NN.boardNN.
    /// </summary>
    public class NeuralNetwork
    {
        // Contains all matrices of weights, structure: [layer][weight, node]
        private List<double[,]> weights;

        // Contains all vectors of biases
        private List<double[]> biases;

        // Contains size of the network's input
        private int inputSize;

        // Network's mutability factor (for reproduction), best 0.5 < mut < 3.0 (2.0 seems to be even thirds)
        private double mutability = 1.0;

        // Essentially the shape of the neural network
        private int[] layers;

        private Random random = new Random();
        private Gaussian randomDistribution = new Gaussian();
        private bool isChild;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuralNetwork"/> class.
        /// </summary>
        /// <param name="inputSize">inputSize.</param>
        /// <param name="layers">layers.</param>
        public NeuralNetwork(int inputSize, int[] layers)
            : this(inputSize, layers, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuralNetwork"/> class.
        /// </summary>
        /// <param name="inputSize">inputSize.</param>
        /// <param name="layers">layers.</param>
        /// <param name="isChild">isChild.</param>
        public NeuralNetwork(int inputSize, int[] layers, bool isChild)
        {
            this.isChild = isChild;
            this.inputSize = inputSize;
            this.layers = layers;
            this.weights = new List<double[,]>(layers.Length);
            this.biases = new List<double[]>();

            // For each layer
            for (int i = 0; i < layers.Length; i++)
            {
                // If creating first layer, consider the input size
                if (i == 0)
                {
                    // If child NN, leave weights blank to be filled in later
                    if (this.isChild)
                    {
                        this.weights.Add(new double[inputSize, layers[0]]);
                        this.biases.Add(new double[layers[0]]);
                    }
                    else
                    {
                        // If creating any other layer
                        // If new NN, generate weight values
                        this.weights.Add(GenerateMatrix(this.random, inputSize, layers[0]));
                        this.biases.Add(GenerateBiases(this.random, layers[0]));
                    }

                    continue;
                }

                if (this.isChild)
                {
                    this.weights.Add(new double[layers[i - 1], layers[i]]);
                    this.biases.Add(new double[layers[i]]);
                }
                else
                {
                    // [weights, nodes]
                    this.weights.Add(GenerateMatrix(this.random, layers[i - 1], layers[i]));
                    this.biases.Add(GenerateBiases(this.random, layers[i]));
                }
            }
        }

        /// <summary>
        /// Run an input through the neural network and return the output.
        /// </summary>
        /// <param name="input">input.</param>
        /// <returns>blah.</returns>
        public double[] Calculate(int[] input)
        {
            // Holds the new output from each layer
            double[] hold = new double[0];

            // Holds inputs to each layer (and last layer's output)
            double[] prev = new double[this.inputSize];

            // Convert all the integers to doubles
            for (int i = 0; i < this.inputSize; i++)
            {
                prev[i] = input[i];
            }

            // For every layer
            for (int i = 0; i < this.weights.Count; i++)
            {
                // Create a new array as big as the number of nodes (outputs)
                hold = new double[this.weights[i].GetLength(1)];

                // For each slot in the output
                for (int k = 0; k < this.weights[i].GetLength(1); k++)
                {
                    // For each slot in the output
                    for (int j = 0; j < this.weights[i].GetLength(0); j++)
                    {
                        hold[k] += prev[j] * this.weights[i][j, k];
                    }

                    // Apply the bias and normalization to each output
                    hold[k] = Math.Tanh(hold[k] + this.biases[i][k]);
                }

                // Prepare for the next iteration
                prev = hold;
            }

            return hold;
        }

        /// <summary>
        /// Use sexual reproduction to create a new neural network.
        /// </summary>
        /// <param name="n">n.</param>
        /// <returns>blah.</returns>
        public NeuralNetwork ReproduceWith(NeuralNetwork n)
        {
            // Generate new blank child
            NeuralNetwork ret = new NeuralNetwork(this.inputSize, this.layers, true);

            // Generate child's mutability factor
            ret.mutability = GenerateMutability(this.mutability, n.mutability);

            // For every layer
            for (int i = 0; i < this.weights.Count; i++)
            {
                // For each row (includes biases), ie every output/node
                for (int k = 0; k < this.weights[i].GetLength(1); k++)
                {
                    // For each column, ie every input
                    for (int j = 0; j < this.weights[i].GetLength(0); j++)
                    {
                        // Generate new weight
                        ret.weights[i][j, k] = GenerateWeight(this.weights[i][j, k], n.weights[i][j, k], ret.mutability);
                    }

                    // Generate new bias
                    ret.biases[i][k] = GenerateWeight(this.biases[i][k], n.biases[i][k], ret.mutability);
                }
            }

            return ret;
        }

        /// <summary>
        /// Generate a matrix of the given size with randomized doubles for each slot, randomized weights are good for neural networks.
        /// </summary>
        /// <param name="random">random.</param>
        /// <param name="weights">weights.</param>
        /// <param name="nodes">nodes.</param>
        /// <returns>blah.</returns>
        private static double[,] GenerateMatrix(Random random, int weights, int nodes)
        {
            double[,] ret = new double[weights, nodes];
            for (int i = 0; i < weights; i++)
            {
                for (int j = 0; j < nodes; j++)
                {
                    ret[i, j] = random.NextDouble();
                }
            }

            return ret;
        }

        /// <summary>
        /// Generate an array of randomized biases.
        /// </summary>
        /// <param name="random">random.</param>
        /// <param name="biases">biases.</param>
        /// <returns>blah.</returns>
        private static double[] GenerateBiases(Random random, int biases)
        {
            double[] ret = new double[biases];

            for (int i = 0; i < biases; i++)
            {
                ret[i] = random.NextDouble();
            }

            return ret;
        }

        /// <summary>
        /// Returns a weight given two weights and a mutability.
        /// </summary>
        /// <param name="weight">weight.</param>
        /// <param name="weight1">weight1.</param>
        /// <param name="mutability">mutability.</param>
        /// <returns>blah.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GenerateWeight(double weight, double weight1, double mutability)
        {
            return Gaussian.Next(weight, weight1, mutability);
        }

        /// <summary>
        /// Returns a mutability taken from two mutabilities.
        /// </summary>
        /// <param name="m">m.</param>
        /// <param name="m1">m1.</param>
        /// <returns>blah.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GenerateMutability(double m, double m1)
        {
            // The third parameter is the same mathematically as this: (((m + m1) / 2) + 1) / 2
            return Gaussian.Next(m, m1, (m + m1 + 2.0) / 4.0);
        }
    }
}