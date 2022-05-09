// <copyright file="Generation.cs" company="Adam nassar &amp; Nathan Balcarcel">
// Copyright (c) Adam nassar &amp; Nathan Balcarcel. All rights reserved.
// </copyright>

namespace SuperTicTacToe
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Class for the Generation algorithm.
    /// </summary>
    public class Generation
    {
        // Stores all the ais in this generation
        public AI[] ais;

        // Backups of the ais (should be ordered)
        public AI[] ais_back, ais_back1, ais_back2;

        // The current generation number
        public int generation;

        // Backups of the generation numbers
        public int generation_back, generation_back1, generation_back2;

        // "Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0."
        private Random random = new Random();

        // Stores all the scores of the ais, not used in backup however
        private int[] scores;

        // Number of ais per generation
        public int aiNum;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generation"/> class.
        /// </summary>
        public Generation() 
            : this(new AI[50], -1, new AI[0], new AI[0], new AI[0], 50)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generation"/> class.
        /// </summary>
        /// <param name="ais">ais.</param>
        /// <param name="generation">generation.</param>
        /// <param name="ais_back">ais_back.</param>
        /// <param name="ais_back1">ais_back1.</param>
        /// <param name="ais_back2">ais_back2.</param>
        /// <param name="aiNum">aiNum</param>
        public Generation(AI[] ais, int generation, AI[] ais_back, AI[] ais_back1, AI[] ais_back2, int aiNum)
        {
            this.aiNum = aiNum;
            this.generation = generation;
            this.ais = ais;

            this.generation_back = generation - 1;
            this.generation_back1 = generation - 2;
            this.generation_back2 = generation - 3;
            this.ais_back = ais_back;
            this.ais_back1 = ais_back1;
            this.ais_back2 = ais_back2;
            this.scores = new int[aiNum];

            // If first generation, generate new AIs to fill in the array
            if (generation == -1)
            {
                for (int i = 0; i < aiNum; i++)
                {
                    this.ais[i] = new AI();
                }

                // Make this the first generation
                this.generation++;
            }
        }

        /// <summary>
        /// Evolve in a loop, checking if a stop condition has been met or not after every new generation TODO.
        /// </summary>
        /// <returns>generation.</returns>
        public int Evolve()
        {
            // Used to track how many generations we've gone through
            int generation = this.generation; 

            return generation;
        }

        /// <summary>
        /// This is where evolution takes place, have all AIs play against top 5 AIs 3 times each.
        /// </summary>
        public void NextGeneration()
        {
            // First backup the current generations and throw out the 4th
            this.ais_back2 = this.ais_back1;
            this.ais_back1 = this.ais_back;
            this.ais_back = this.ais;

            // This may cause weird bugs, look into this later if any issues pop up
            this.ais = (AI[])this.ais.Clone(); 

            this.generation_back2 = this.generation_back1;
            this.generation_back1 = this.generation_back;
            this.generation_back = this.generation;
            this.generation++;
            int[] rewards = new int[this.aiNum];
            (int, int) rewards_game;
            AI ai, ai1;

            // now we evolve the current generation TODO
            // if (generation == 0)
            // { //if the current generation is the very first, make all AIs play 3 games against 5 other AIs
            //     int[] games = new int[500]; //stores how many games each AI has played, all must be at least 5
            // }
            // else
            // { //make all AIs play the top 5 AIs 3 times (used to assess if the top 5 should remain the top 5)
            for (int i = 0; i < this.aiNum; i++)
            {
                // Grab the ith ai
                ai = this.ais[i];

                // All top 5 ais
                for (int j = 0; j < 5; j++)
                {
                    ai1 = this.ais[j];
                    rewards_game = this.PlayGames(ai, ai1);
                    // Add the reward from this game to the total of all 5 games
                    rewards[i] += rewards_game.Item1;
                }
            }

            // Sort the ais array based on the scores
            Array.Sort(rewards, ais); 

            // Kill off about half the generation, for now just kill the bottom half, and repopulate
            Random r = new Random();

            for (int i = this.aiNum / 2; i < this.aiNum; i++)
            {
                this.ais[i] = AI.Reproduce(this.ais[r.Next(0, this.aiNum / 2)], this.ais[r.Next(0, this.aiNum / 2)]);
            }
        }

        /// <summary>
        /// Plays 3 games, and returns a score for each AI TODO.
        /// </summary>
        /// <param name="ai">ai.</param>
        /// <param name="ai1">ai1.</param>
        /// <returns>Score.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int, int) PlayGames(AI ai, AI ai1)
        {
            Game game;
            Random random = new Random();
            int score = 0, score1 = 0;
            (int, int) result;
            bool turn, playing;

            // Play 3 games
            for (int i = 0; i < 3; i++)
            {
                game = new Game();

                // Choose first turn randomly
                turn = random.Next(2) == 1;
                playing = true;
                while (playing)
                {
                    Console.WriteLine("trapped in this loop eyyyyyyyyyyyyyyyyyyy whats up homie");
                    if (turn)
                    {
                        result = this.AIMove(ai, game);
                        score += result.Item2;
                    }
                    else
                    {
                        result = this.AIMove(ai1, game);
                        score1 += result.Item2;
                    }

                    // Game over
                    if (result.Item1 == 2)
                    {
                        playing = false;
                    }

                    // Game over
                    if (this.TieDetector(game))
                    {
                        playing = false;
                    }

                    turn = !turn;
                }
            }

            return (score, score1);
        }

        /// <summary>
        /// Detects if tie.
        /// </summary>
        /// <param name="game">game.</param>
        /// <returns>boolean.</returns>
        public bool TieDetector(Game game)
		{
            int ties = 0, p1 = 0, p2 = 0;
            for (int i = 0; i < 9; i++)
            {
                // Local tie
                if (game.Tiles[81 + i] == -2)
                {
                    ties++;
                }
                else if (game.Tiles[81 + i] == 1)
                {
                    // p1
                    p1++;
                }
                else if (game.Tiles[81 + i] == -1)
                {
                    p2++;
                }
            }

            if (ties + p1 + p2 == 9)
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Simulates the AI move.
        /// </summary>
        /// <param name="ai">ai.</param>
        /// <param name="game">game.</param>
        /// <returns>blah.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int, int) AIMove(AI ai, Game game)
        {
            int score = 0;
            (int[], int[], int) state = game.GetGameInfo();

            // First either choose a new board or pass the current one
            if (state.Item3 == -1)
            {
                game.ChooseBoardSecondPlayer(ai.ChooseBoard(state.Item2, false));
            }
            else
            {
                game.ChooseBoardSecondPlayer(state.Item3);
            }

            int resultPlace, localTile;

            // Update the new info
            state = game.GetGameInfo(); 
            localTile = ai.NextMove(state.Item2, state.Item3);
            resultPlace = game.PlaceSecondPlayer(localTile);

            switch (resultPlace)
            {
                case -3: // Board tie
                    score = 3;
                    break;
                case 0: // Successful place
                    score = -1;
                    break;
                case 1: // Local win
                    score = 5;
                    break;
                case 2: // G lobal win
                    score = 10;
                    break;
            }

            return (resultPlace, score);
        }

        /// <summary>
        /// Used to store the last 4 generations into the database TODO.
        /// </summary>
        public void Backup()
        {
        }
    }
}