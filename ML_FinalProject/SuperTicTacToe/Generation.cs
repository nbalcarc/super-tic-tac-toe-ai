namespace SuperTicTacToe {

    public class Generation {
        public AI[] ais; //stores all the ais in this generation
        public AI[] ais_back, ais_back1, ais_back2; //backups of the ais (should be ordered)
        public int generation; //the current generation number
        public int generation_back, generation_back1, generation_back2; //backups of the generation numbers
        private Random random = new Random(); //"Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0."
        private int[] scores = new int[1000]; //stores all the scores of the ais, not used in backup however

        public Generation(): this(new AI[1000], 0, new AI[0], new AI[0], new AI[0]) {

        }

        public Generation(AI[] ais, int generation, AI[] ais_back, AI[] ais_back1, AI[] ais_back2) {
            this.generation = generation;
            this.ais = ais;

            this.generation_back = generation - 1;
            this.generation_back1 = generation - 2;
            this.generation_back2 = generation - 3;
            this.ais_back = ais_back;
            this.ais_back1 = ais_back1;
            this.ais_back2 = ais_back2;

            if (generation == -1) { //if first generation, generate new AIs to fill in the array
                for (int i = 0; i < 1000; i++) {
                    this.ais[i] = new AI();
                }
                this.generation++; //make this the first generation
            }
        }

        //this is where evolution takes place, have all AIs play against top 5 AIs 3 times each
        public void NextGeneration() {
            //first backup the current generations and throw out the 4th
            this.ais_back2 = this.ais_back1;
            this.ais_back1 = this.ais_back;
            this.ais_back = this.ais;
            this.ais = (AI[])this.ais.Clone(); //this may cause weird bugs, look into this later if any issues pop up
            this.generation_back2 = this.generation_back1;
            this.generation_back1 = this.generation_back;
            this.generation_back = this.generation;
            this.generation++;

            //now we evolve the current generation TODO
            if (generation == 0) { //if the current generation is the very first, make all AIs play 3 games against 5 other AIs
                int[] games = new int[1000]; //stores how many games each AI has played, all must be at least 5


            } else { //make all AIs play the top 5 AIs 3 times (used to assess if the top 5 should remain the top 5)

            }

            //kill off about half the generation

            //repopulate the gaps


        }

        //evolve in a loop, checking if a stop condition has been met or not after every new generation TODO
        public void Evolve() {
            int start_generation = this.generation; //used to track how many generations we've gone through
            while (true) { //check stop condition here
                //or check stop condition here

            }


        }

        //plays 3 games, and returns a score for each AI TODO
        public (int, int) PlayGames(AI ai, AI ai1) {
            Game game;
            (int[], int[], int, bool) rets; //tiles, tiles_inverted, focus_board, turn
            int place;
            int score = 0, score1 = 0;
            bool new_board = false;
            for (int i = 0; i < 3; i++) { //play 3 games
                game = new Game();
                

                //out of the game loop now, game has finished for one reason or another
            }
            return (score, score1);
        }

        //used to store the last 4 generations into the database TODO
        public void Backup() {
            
        }
    }
}