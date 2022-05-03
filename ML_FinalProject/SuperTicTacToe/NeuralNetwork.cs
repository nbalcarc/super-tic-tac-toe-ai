using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SuperTicTacToe {

    public class AI {
        private NeuralNetwork place_nn; //used to decide where to move
        private NeuralNetwork board_nn; //used to decide what board to place on (used occasionally)
        public int id; //for databasing, should be unique

        public AI(): this(new NeuralNetwork(99, new int[]{162, 81, 9}, false), new NeuralNetwork(90, new int[]{36, 18, 9}, false)) {

        }

        public AI(NeuralNetwork place, NeuralNetwork board) {
            this.place_nn = place;
            this.board_nn = board;
            //this.id = id;
        }

        //returns which tile of the 9 open options to go in
        public int NextMove(int[] board, int focus_board) {
            double[] results = this.place_nn.Calculate(board); //must choose which of the 9 tiles to go in
            int[] ordered = ReturnSorted(results);

            for (int i = 0; i < 9; i++) { //DEBUGGING
                //Console.WriteLine($"DEBUGGING1 results[{i}]: {results[i]}");
            }

            for (int i = 0; i < 9; i++) { //ensure a valid option is returned
                if (board[focus_board * 9 + ordered[i]] == 0 && board[81 + focus_board] == 0) { //if the given tile in the focused board is open, return that tile
                    return ordered[i];
                }
            }
            //Console.WriteLine($"DEBUGGING, returned -1 as next move");
            //Console.WriteLine($"DEBUGGING ######### {board[80]}, {board[81]}, {board[82]}, {board[83]}, {board[84]}, {board[85]}, {board[86]}, {board[87]}, {board[88]}");
            return -1;
        }

        //returns its choice of what board to play in, takes whether it is the starting move or not as a parameter (expects inverted board if false)
        public int ChooseBoard(int[] board, bool start) {
            double[] results = this.board_nn.Calculate(board);
            int[] ordered = start ? ReturnSorted(results) : ReturnSortedInverted(results); //if not start of game, wants to minimize opponent's advantage

            for (int i = 0; i < 9; i++) { //ensure a valid option is returned
                if (board[81 + ordered[i]] == 0) { //if the given board is not won, return that board
                    return ordered[i];
                }
            }
            return 0;
        }

        //given the output of a neural network (assumes output size of 9), return a list of integers representing the sorted order of the array
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] ReturnSorted(double[] results) {
            int len = results.Length;
            int[] ret = new int[len];
            bool[] done = new bool[len];
            double max;
            int choice = 0;

            for (int i = 0; i < len; i++) { //for every ranking
                max = Double.MinValue;
                for (int j = 0; j < len; j++) { //for every element in the list (find next biggest)
                    if (!done[j] && results[j] > max) { //if this element is still open
                        max = results[j];
                        choice = j; //make note of the largest so far
                    }
                }
                ret[i] = choice; //set the ith place as the current choice
                done[choice] = true; //mark the choice element as closed
            }
            return ret;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int[] ReturnSortedInverted(double[] results) {
            int len = results.Length;
            int[] ret = new int[len];
            bool[] done = new bool[len];
            double min;
            int choice = 0;

            for (int i = 0; i < len; i++) { //for every ranking
                min = Double.MaxValue;
                for (int j = 0; j < len; j++) { //for every element in the list (find next smallest)
                    if (!done[j] && results[j] < min) { //if this element is still open
                        min = results[j];
                        choice = j; //make note of the largest so far
                    }
                }
                ret[i] = choice; //set the ith place as the current choice
                done[choice] = true; //mark the choice element as closed
            }
            return ret;
        }

        //reproduce two AIs, return a new AI
        public static AI Reproduce(AI parent, AI parent1) {
            return new AI(parent.place_nn.ReproduceWith(parent1.place_nn), parent.board_nn.ReproduceWith(parent1.board_nn));
        }
    }

    public class NeuralNetwork {
        public List<double[,]> weights; //contains all matrices of weights, structure: [layer][weight, node]
        public List<double[]> biases; //contains all vectors of biases
        public int input_size; //contains size of the network's input
        double mutability = 1.0; //network's mutability factor (for reproduction), best 0.5 < mut < 3.0 (2.0 seems to be even thirds)
        public int[] layers; //essentially the shape of the neural network
        private Random random = new Random();
        private Gaussian random_distribution = new Gaussian();
        private bool is_child;

        public NeuralNetwork(int input_size, int[] layers): this(input_size, layers, false) {
            
        }

        //constructor, layers is expected to contain the size of each layer as an int array
        public NeuralNetwork(int input_size, int[] layers, bool is_child) { 
            this.is_child = is_child;
            this.input_size = input_size;
            this.layers = layers;
            this.weights = new List<double[,]>(layers.Length);
            this.biases = new List<double[]>();

            for (int i = 0; i < layers.Length; i++) { //for each layer
                if (i == 0) { //if creating first layer, consider the input size
                    if (this.is_child) {
                        this.weights.Add(new double[input_size, layers[0]]); //if child NN, leave weights blank to be filled in later
                        this.biases.Add(new double[layers[0]]);
                    } else { //if creating any other layer
                        this.weights.Add(GenerateMatrix(this.random, input_size, layers[0])); //if new NN, generate weight values
                        this.biases.Add(GenerateBiases(this.random, layers[0]));
                    }
                    continue;
                }
                if (this.is_child) {
                    this.weights.Add(new double[layers[i - 1], layers[i]]);
                    this.biases.Add(new double[layers[i]]);
                } else {
                    this.weights.Add(GenerateMatrix(this.random, layers[i - 1], layers[i])); //[weights, nodes]
                    this.biases.Add(GenerateBiases(this.random, layers[i]));
                }
            }
        }

        //run an input through the neural network and return the output
        public double[] Calculate(int[] input) {
            //if (input.Length != this.input_size) { //if the input is not the size we want, just return an empty array
            //    return new double[0];
            //}
            double[] hold = new double[0]; //holds the new output from each layer
            double[] prev = new double[this.input_size]; //holds inputs to each layer (and last layer's output)
            for (int i = 0; i < this.input_size; i++) { //convert all the integers to doubles
                prev[i] = input[i];
            }
            for (int i = 0; i < this.weights.Count; i++) { //for every layer
                hold = new double[this.weights[i].GetLength(1)]; //create a new array as big as the number of nodes (outputs)
                for (int k = 0; k < this.weights[i].GetLength(1); k++) { //for each slot in the output
                    for (int j = 0; j < this.weights[i].GetLength(0); j++) { //for each slot in the output
                        hold[k] += prev[j] * this.weights[i][j,k];
                    }
                    hold[k] = Math.Tanh(hold[k] + this.biases[i][k]); //apply the bias and normalization to each output
                }
                prev = hold; //prepare for the next iteration
            }
            return hold;
        }

        //generate a matrix of the given size with randomized doubles for each slot, randomized weights are good for neural networks
        private static double[,] GenerateMatrix(Random random, int weights, int nodes) {
            double[,] ret = new double[weights, nodes];
            for (int i = 0; i < weights; i++) {
                for (int j = 0; j < nodes; j++) {
                    ret[i,j] = random.NextDouble();
                }
            }
            return ret;
        }

        //generate an array of randomized biases
        private static double[] GenerateBiases(Random random, int biases) {
            double[] ret = new double[biases];
            for (int i = 0; i < biases; i++) {
                ret[i] = random.NextDouble();
            }
            return ret;
        }


        //returns a weight given two weights and a mutability
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GenerateWeight(double weight, double weight1, double mutability) {
            return Gaussian.Next(weight, weight1, mutability);
        }

        //returns a mutability taken from two mutabilities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GenerateMutability(double m, double m1) {
            return Gaussian.Next(m, m1, (m + m1 + 2.0) / 4.0); //the third parameter is the same mathematically as this: (((m + m1) / 2) + 1) / 2
        }

        //use sexual reproduction to create a new neural network
        public NeuralNetwork ReproduceWith(NeuralNetwork n) {
            NeuralNetwork ret = new NeuralNetwork(this.input_size, this.layers, true); //generate new blank child
            ret.mutability = GenerateMutability(this.mutability, n.mutability); //generate child's mutability factor
            for (int i = 0; i < this.weights.Count; i++) { //for every layer
                for (int k = 0; k < this.weights[i].GetLength(1); k++) { //for each row (includes biases), ie every output/node
                    for (int j = 0; j < this.weights[i].GetLength(0); j++) { //for each column, ie every input
                        ret.weights[i][j,k] = GenerateWeight(this.weights[i][j,k], n.weights[i][j,k], ret.mutability); //generate new weight
                    }
                    ret.biases[i][k] = GenerateWeight(this.biases[i][k], n.biases[i][k], ret.mutability); //generate new bias
                }
            }
            return ret;
        }
    }

    public class Gaussian {
        private static Random random = new Random();

        //generates a distribution using the box-muller method
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double BoxMuller(double mean, double standard_deviation) {
            //if (standard_deviation < 0.5) {
            //    standard_deviation = 0.5;
            //}
            double ret = 0.0;
            double x = 0.0;
            double y = 0.0;
            while (ret >= 1.0 || ret == 0.0) {
                x = random.NextDouble() * 2.0 - 1.0;
                y = random.NextDouble() * 2.0 - 1.0;
                ret = x * x + y * y;
            }

            ret = Math.Sqrt((Math.Log(ret) * -2.0) / ret) * x; //by this line we have the distribution
            ret = mean + ret * standard_deviation; //apply the given parameters
            return ret;
        }

        //given two numbers and a mutability, will generate a new number
        public static double Next(double num, double num1, double mutability) {
            double distance = Math.Abs(num / 2.0 - num1 / 2.0);
            if (distance == 0) { //if the distance hits 0, then we will never generate anything other than the given number, so we set a minimum
                distance = 0.0625;
            } 
            return BoxMuller(num / 2.0 + num1 / 2.0, mutability * distance); //the averaging technique keeps the double from overflowing
        }
    }
}

