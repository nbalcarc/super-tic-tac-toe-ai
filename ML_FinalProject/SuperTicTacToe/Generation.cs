using System;
using System.Runtime.CompilerServices;

namespace SuperTicTacToe
{

    public class Generation
    {
        public AI[] ais; //stores all the ais in this generation
        public AI[] ais_back, ais_back1, ais_back2; //backups of the ais (should be ordered)
        public int generation; //the current generation number
        public int generation_back, generation_back1, generation_back2; //backups of the generation numbers
        private Random random = new Random(); //"Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0."
        private int[] scores; //stores all the scores of the ais, not used in backup however
        public int ai_num; //number of ais per generation

        public Generation() : this(new AI[50], -1, new AI[0], new AI[0], new AI[0], 50)
        {

        }

        public Generation(AI[] ais, int generation, AI[] ais_back, AI[] ais_back1, AI[] ais_back2, int ai_num)
        {
            this.ai_num = ai_num;
            this.generation = generation;
            this.ais = ais;

            this.generation_back = generation - 1;
            this.generation_back1 = generation - 2;
            this.generation_back2 = generation - 3;
            this.ais_back = ais_back;
            this.ais_back1 = ais_back1;
            this.ais_back2 = ais_back2;
            this.scores = new int[ai_num];

            if (generation == -1)
            { //if first generation, generate new AIs to fill in the array
                for (int i = 0; i < ai_num; i++)
                {
                    this.ais[i] = new AI();
                }
                this.generation++; //make this the first generation
            }
        }

        //evolve in a loop, checking if a stop condition has been met or not after every new generation TODO
        public int Evolve()
        {
            int generation = this.generation; //used to track how many generations we've gone through
            //while (true) { //check stop condition here
            //    //or check stop condition here
            //}
            return generation;
        }

        //this is where evolution takes place, have all AIs play against top 5 AIs 3 times each
        public void NextGeneration()
        {
            //first backup the current generations and throw out the 4th
            this.ais_back2 = this.ais_back1;
            this.ais_back1 = this.ais_back;
            this.ais_back = this.ais;
            this.ais = (AI[])this.ais.Clone(); //this may cause weird bugs, look into this later if any issues pop up
            this.generation_back2 = this.generation_back1;
            this.generation_back1 = this.generation_back;
            this.generation_back = this.generation;
            this.generation++;
            int[] rewards = new int[ai_num];
            (int, int) rewards_game;
            AI ai, ai1;

            //now we evolve the current generation TODO
            //if (generation == 0)
            //{ //if the current generation is the very first, make all AIs play 3 games against 5 other AIs
            //    int[] games = new int[500]; //stores how many games each AI has played, all must be at least 5

            //}
            //else
            //{ //make all AIs play the top 5 AIs 3 times (used to assess if the top 5 should remain the top 5)
            for (int i = 0; i < ai_num; i++)
            {
                ai = this.ais[i]; //grab the ith ai
                for (int j = 0; j < 5; j++)
                { //all top 5 ais
                    ai1 = this.ais[j];
                    rewards_game = this.PlayGames(ai, ai1);
                    rewards[i] += rewards_game.Item1; //add the reward from this game to the total of all 5 games
                }
            }
            //}
            Array.Sort(rewards, ais); //sort the ais array based on the scores

            //kill off about half the generation, for now just kill the bottom half, and repopulate
            Random r = new Random();
            for (int i = ai_num/2; i < ai_num; i++)
			{
                this.ais[i] = AI.Reproduce(this.ais[r.Next(0, ai_num/2)], this.ais[r.Next(0, ai_num)]);
			}
        }



        //plays 3 games, and returns a score for each AI TODO
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int, int) PlayGames(AI ai, AI ai1)
        {
            Game game;
            Random random = new Random();
            (int[], int[], int) rets; //tiles, tiles_inverted, focus_board, turn
            int score = 0, score1 = 0;
            (int, int) result;
            bool turn, playing;

            for (int i = 0; i < 3; i++)
            { //play 3 games
                game = new Game();
                turn = random.Next(2) == 1; //choose first turn randomly
                playing = true;
                while (playing)
                {
                    Console.WriteLine("trapped in this loop eyyyyyyyyyyyyyyyyyyy whats up homie");
                    if (turn)
                    {
                        result = AIMove(ai, game);
                        score += result.Item2;
                    }
                    else
                    {
                        result = AIMove(ai1, game);
                        score1 += result.Item2;
                    }
                    if (result.Item1 == 2)
                    { //game over
                        playing = false;
                    }
                    if (TieDetector(game))
					{
                        playing = false;
					}
                    turn = !turn;
                }
            }
            return (score, score1);
        }

        public bool TieDetector(Game game)
		{
            int ties = 0, p1 = 0, p2 = 0;
            for (int i = 0; i < 9; i++)
            {
                if (game.tiles[81 + i] == -2)
                { //local tie
                    ties++;
                }
                else if (game.tiles[81 + i] == 1)
                { //p1
                    p1++;
                }
                else if (game.tiles[81 + i] == -1)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int, int) AIMove(AI ai, Game game)
        {
            int score = 0;
            (int[], int[], int) state = game.GetGameInfo();
            if (state.Item3 == -1)
            { //first either choose a new board or pass the current one
                game.ChooseBoardSecondPlayer(ai.ChooseBoard(state.Item2, false));
            }
            else
            {
                game.ChooseBoardSecondPlayer(state.Item3);
            }
            int resultPlace, localTile, board;
            state = game.GetGameInfo(); //update the new info
            localTile = ai.NextMove(state.Item2, state.Item3);
            resultPlace = game.PlaceSecondPlayer(localTile);
            switch (resultPlace)
            {
                case -3: //board tie
                    score = 3;
                    break;
                case 0: //successful place
                    score = -1;
                    break;
                case 1: //local win
                    score = 5;
                    break;
                case 2: //global win
                    score = 10;
                    break;
            }
            return (resultPlace, score);
        }

        //used to store the last 4 generations into the database TODO
        public void Backup()
        {

        }
    }
}