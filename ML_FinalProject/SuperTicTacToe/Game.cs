using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SuperTicTacToe
{
    public class Game
    {
        // First 81 tiles represent the boards, next 9 represent board tiles, last 9 represent active board (for AI only)
        public int[] tiles = new int[99];
        // An inverted board that stays updated like the first, meant for player2
        public int[] tiles_inverted = new int[99];
        // 8 ways to win, two people, 10 boards (9 real + 1 overall)
        public bool[] win_events = new bool[160];
        // If -1, this means the focus board hasn't been decided and is up to the player's choice, [-1, 8]
        public int focus_board = -1;
        // Counts how many turns have occurred in each board
        public int[] turns = new int[10];
        // Remembers whose turn it is
        //private bool turn;


        ////////////////////////////////
        /////////  INTERFACE  //////////
        ////////////////////////////////

        //returns the current game state, including the global board tiles, current focus board, and player turn
        public (int[], int[], int) GetGameInfo()
        {
            //Note: this whole if statement is solely for neural networks
            // If a board is focused
            if (this.focus_board > -1)
            {
                // Clear out focused board
                for (int i = 90; i < 99; i++)
                {
                    this.tiles[i] = 0;
                    this.tiles_inverted[i] = 0;
                }
                this.tiles[90 + this.focus_board] = 1; //set the focus board
                this.tiles[90 + this.focus_board] = 1;
            }
            return (this.tiles, this.tiles_inverted, this.focus_board);
        }

        // Refer to Place
        public int PlaceFirstPlayer(int local_tile)
        {
            int ret = Place(local_tile, true);
            if (ret > -1 || ret == -3 || ret == -4)
			{
                UpdateFocusBoard(local_tile);
            }
            return ret;
        }

        // Refer to Place
        public int PlaceSecondPlayer(int local_tile)
        {
            int ret = Place(local_tile, false);
            if (ret > -1 || ret == -3 || ret == -4)
            {
                UpdateFocusBoard(local_tile);
            }
            return ret;
        }

        // Refer to ChooseBoard
        public int ChooseBoardFirstPlayer(int board)
        {
            return ChooseBoard(board, true);
        }

        // Refer to ChooseBoard
        public int ChooseBoardSecondPlayer(int board)
        {
            return ChooseBoard(board, false);
        }

        ////////////////////////////////
        ////////////  END  /////////////
        ////////////////////////////////


        /*
        Win conditions:
        0 - row across top
        1 - row across middle
        2 - row across bottom
        3 - column on left
        4 - column in middle
        5 - column on right
        6 - diagonal with negative slope
        7 - diagonal with positive slope

        Sections of win_events:
        0 - Player1 first board
        8 - Player1 second board
        ...
        64 - Player1 ninth board
        72 - Player1 total board
        80 - Player2 first board
        88 - Player2 second board
        ...
        152 - Player2 total board
        160 - OUT OF BOUNDS
        */

        // Constructor
        public Game()
        {
            for (int i = 0; i < 160; i++)
            {
                this.win_events[i] = true;
            }

            /*
            if (randomPlayer)
            {
                Random random = new Random();
                this.turn = random.Next(2) == 1; //choose first turn randomly
            }
            else
            {
                this.turn = true;
            }
            */
        }

        // Simple way to update both boards at the same time
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateTile(int global_tile, bool player)
        {
            if (player)
            {
                this.tiles[global_tile] = 1;
                this.tiles_inverted[global_tile] = -1;
            }
            else
            {
                this.tiles[global_tile] = -1;
                this.tiles_inverted[global_tile] = 1;
            }
        }

        // If player is choosing the next board, update it here, returns -2: wrong player, -1: invalid board, 0: no issues
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ChooseBoard(int board, bool player)
        {
            //if (player != this.turn)
            //{
            //    return -2;
            //}
            // Player is allowed to choose the board and that board has not been won
            if (this.focus_board == -1 && this.tiles[81 + board] == 0)
            {
                this.focus_board = board;
                return 0;
            }

            if (this.focus_board == board)
            {
                return 0;
            }

            return -1;
        }

        //-4: game tied, -3: board tied, -2: wrong player, -1: failed to place, 0: placed correctly, 1: local win, 2: game win
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Place(int local_tile, bool player)
        {
            //if (player != this.turn)
            //{
            //    return -2;
            //}

            // Checks if we can place at this location
            if (this.tiles[81 + this.focus_board] == 0 && this.tiles[local_tile + 9 * this.focus_board] == 0)
            {
                // Updates the tile
                UpdateTile(local_tile + 9 * this.focus_board, player);

                // Increments turn number for specific board
                this.turns[this.focus_board] += 1;

                // Increment global turns
                this.turns[9] += 1;

                // Flip whoever's turn it is
                //this.turn = !this.turn;

                // Checks for win
                int ret = PlaceResult(local_tile, player);

                //int current_focus_board = this.focus_board;
                //UpdateFocusBoard(local_tile);

                // If any win detected, return
                if (ret > 0)
                {
                    //this.focus_board = -1; //if a win has taken place, let the other player choose their board
                    return ret;
                }

                // Tied the game
                if (this.turns[9] == 81)
                {
                    return -4;
                }
                int ties = 0, p1 = 0, p2 = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (this.turns[i] == 9)
                    { //local tie
                        ties++;
                    }
                    else if (this.tiles[81 + i] == 1)
                    { //p1
                        p1++;
                    }
                    else if (this.tiles[81 + i] == -1)
                    {
                        p2++;
                    }
                }
                if (ties + p1 + p2 == 9)
                { //all boards are either tied or won, and no one has won the game
                    return -4;
                }

                // Tied or already won the newly focused board
                if (this.focus_board == -1 || this.turns[this.focus_board] == 9 || this.tiles[81 + this.focus_board] != 0)
                {
                    //Console.WriteLine($"DEBUGGING first in return -3: {this.focus_board == -1}");
                    if (this.turns[this.focus_board] == 9 && this.tiles[81 + this.focus_board] == 0) { //if tie has occurred in this board
                        this.tiles[81 + this.focus_board] = -2;
                        this.tiles_inverted[81 + this.focus_board] = -2;
                    }
                    return -3;
                }

                return 0;
            }

            // Blocked place location
            if (this.focus_board == -1 || this.turns[this.focus_board] == 9 || this.tiles[81 + this.focus_board] != 0)
            { 
                return -3;
            }

            return -1;
        }

        //update the focus_board
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateFocusBoard(int board)
        {
            // Checks if board has been won
            if (this.tiles[81 + board] == 0 && this.turns[board] < 9)
            {
                this.focus_board = board;
                return;
            }

            // Board has been won, player will choose next location
            this.focus_board = -1;
        }

        // Will update win_events according to the last move, 0: no win, 1: local win, 2: game win
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PlaceResult(int local_tile, bool player)
        {
            // Check for local win
            int ret = CheckWin(this.focus_board, player);

            // If a local win has taken place
            if (ret == 1)
            {
                // Update global board to reflect this victory
                UpdateTile(81 + this.focus_board, player);

                // Check for a victory in the main board
                ret += CheckWin(9, player);

                // If victory in main board, return early
                if (ret == 2)
                {
                    return ret;
                }

                // Update the global board win conditions for the opponent
                UpdateWinConditions((player ? 152 : 72), this.focus_board);

                return ret;
            }

            // Cancel out the enemy's win conditions
            UpdateWinConditions((player ? 80 : 0) + this.focus_board * 8, local_tile);

            return ret;
        }

        // Checks board for win, 1 is win, 0 is no win
        public int CheckWin(int board, bool player)
        {
            int player_const = player ? 1 : -1;
            int ret = 0;

            // Offset within the game tiles
            int offset = board * 9;

            // Offset within the win conditions, saved to int because used multiple times during loop
            int offset_conditions = board * 8 + (player ? 0 : 80);

            // For all eight conditions
            for (int condition = 0; condition < 8; condition++)
            {
                // This win condition is not possible
                if (!this.win_events[offset_conditions + condition])
                {
                    continue;
                }

                // This win condition is still possible, check for it
                switch (condition)
                {
                    case 0: // Row across top
                        if (this.tiles[offset] == player_const && this.tiles[offset + 1] == player_const && this.tiles[offset + 2] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 1: // Row across middle
                        if (this.tiles[offset + 3] == player_const && this.tiles[offset + 4] == player_const && this.tiles[offset + 5] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 2: // Row across bottom
                        if (this.tiles[offset + 6] == player_const && this.tiles[offset + 7] == player_const && this.tiles[offset + 8] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 3: // Column on left
                        if (this.tiles[offset] == player_const && this.tiles[offset + 3] == player_const && this.tiles[offset + 6] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 4: // Column in middle
                        if (this.tiles[offset + 1] == player_const && this.tiles[offset + 4] == player_const && this.tiles[offset + 7] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 5: // Column on right
                        if (this.tiles[offset + 2] == player_const && this.tiles[offset + 5] == player_const && this.tiles[offset + 8] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 6: // Diagonal with negative slope
                        if (this.tiles[offset] == player_const && this.tiles[offset + 4] == player_const && this.tiles[offset + 8] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                    case 7: // Diagonal with positive slope
                        if (this.tiles[offset + 2] == player_const && this.tiles[offset + 4] == player_const && this.tiles[offset + 6] == player_const)
                        {
                            ret = 1;
                        }
                        break;
                }
            }
            return ret;
        }

        // Cancels out the opponent's win conditions for this given move
        public void UpdateWinConditions(int offset, int local_tile)
        {
            switch (local_tile)
            {
                case 0: // Top left
                    this.win_events[offset] = false; // Top row
                    this.win_events[offset + 3] = false; // Left column
                    this.win_events[offset + 6] = false;
                    break;
                case 1: // Top mid
                    this.win_events[offset] = false;
                    this.win_events[offset + 4] = false;
                    break;
                case 2: // Top right
                    this.win_events[offset] = false;
                    this.win_events[offset + 5] = false;
                    this.win_events[offset + 7] = false;
                    break;
                case 3: // Mid left
                    this.win_events[offset + 1] = false;
                    this.win_events[offset + 3] = false;
                    break;
                case 4: // Mid mid
                    this.win_events[offset + 1] = false;
                    this.win_events[offset + 4] = false;
                    this.win_events[offset + 6] = false;
                    this.win_events[offset + 7] = false;
                    break;
                case 5: // Mid right
                    this.win_events[offset + 1] = false;
                    this.win_events[offset + 5] = false;
                    break;
                case 6: // Bottom left
                    this.win_events[offset + 2] = false;
                    this.win_events[offset + 3] = false;
                    this.win_events[offset + 7] = false;
                    break;
                case 7: // Bottom mid
                    this.win_events[offset + 2] = false;
                    this.win_events[offset + 4] = false;
                    break;
                case 8: // Bottom right
                    this.win_events[offset + 2] = false;
                    this.win_events[offset + 5] = false;
                    this.win_events[offset + 6] = false;
                    break;
            }
        }
    }
}
