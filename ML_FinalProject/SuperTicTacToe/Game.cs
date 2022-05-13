// <copyright file="Game.cs" company="Adam Nassar and Nathan Balcarcel">
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
    using System.Threading;

    /// <summary>
    /// Game engine.
    /// </summary>
    public class Game
    {
        // First 81 tiles represent the boards, next 9 represent board tiles, last 9 represent active board (for AI only)
        private int[] tiles = new int[99];

        // An inverted board that stays updated like the first, meant for player2
        private int[] tilesInverted = new int[99];

        // 8 ways to win, two people, 10 boards (9 real + 1 overall)
        private bool[] winEvents = new bool[160];

        // If -1, this means the focus board hasn't been decided and is up to the player's choice, [-1, 8]
        private int focusBoard = -1;

        // Counts how many turns have occurred in each board
        private int[] turns = new int[10];

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        public Game()
        {
            for (int i = 0; i < 160; i++)
            {
                this.winEvents[i] = true;
            }
        }

        /// <summary>
        /// Gets tiles.
        /// </summary>
        public int[] Tiles
        {
            get
            {
                return this.tiles;
            }
        }

        ////////////////////////////////
        /////////  INTERFACE  //////////
        ////////////////////////////////

        /// <summary>
        /// Returns the current game state, including the global board tiles, current focus board, and player turn.
        /// </summary>
        /// <returns>Game info.</returns>
        public (int[], int[], int) GetGameInfo()
        {
            // Note: this whole if statement is solely for neural networks
            // If a board is focused
            if (this.focusBoard > -1)
            {
                // Clear out focused board
                for (int i = 90; i < 99; i++)
                {
                    this.tiles[i] = 0;
                    this.tilesInverted[i] = 0;
                }

                // Set the focus board
                this.tiles[90 + this.focusBoard] = 1;
                this.tiles[90 + this.focusBoard] = 1;
            }

            return (this.tiles, this.tilesInverted, this.focusBoard);
        }

        /// <summary>
        /// Places first player.
        /// </summary>
        /// <param name="local_tile">local_tile.</param>
        /// <returns>A success value.</returns>
        public int PlaceFirstPlayer(int local_tile)
        {
            int ret = this.Place(local_tile, true);

            // If a successful place has occurred, update the board
            if (ret > -1 || ret == -3 || ret == -4)
            {
                this.UpdateFocusBoard(local_tile);
            }

            return ret;
        }

        /// <summary>
        /// Places second player.
        /// </summary>
        /// <param name="local_tile">local_tile.</param>
        /// <returns>A success value.</returns>
        public int PlaceSecondPlayer(int local_tile)
        {
            int ret = this.Place(local_tile, false);

            if (ret > -1 || ret == -3 || ret == -4)
            {
                this.UpdateFocusBoard(local_tile);
            }

            return ret;
        }

        /// <summary>
        /// Chooses board for first player.
        /// </summary>
        /// <param name="board">board.</param>
        /// <returns>If chosen board valid.</returns>
        public int ChooseBoardFirstPlayer(int board)
        {
            return this.ChooseBoard(board, true);
        }

        /// <summary>
        /// Chooses board for second player.
        /// </summary>
        /// <param name="board">board.</param>
        /// <returns>If chosen board valid.</returns>
        public int ChooseBoardSecondPlayer(int board)
        {
            return this.ChooseBoard(board, false);
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

        Sections of winEvents:
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

        /// <summary>
        /// Simple way to update both boards at the same time.
        /// </summary>
        /// <param name="global_tile">global_tile.</param>
        /// <param name="player">player.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateTile(int global_tile, bool player)
        {
            if (player)
            {
                this.tiles[global_tile] = 1;
                this.tilesInverted[global_tile] = -1;
            }
            else
            {
                this.tiles[global_tile] = -1;
                this.tilesInverted[global_tile] = 1;
            }
        }

        /// <summary>
        /// If player is choosing the next board, update it here, returns -2: wrong player, -1: invalid board, 0: no issues.
        /// </summary>
        /// <param name="board">board.</param>
        /// <param name="player">player.</param>
        /// <returns>If board is valid..</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ChooseBoard(int board, bool player)
        {
            // Player is allowed to choose the board and that board has not been won
            if (this.focusBoard == -1 && this.tiles[81 + board] == 0)
            {
                this.focusBoard = board;
                return 0;
            }

            if (this.focusBoard == board)
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// -5: -4 and -3, -4: game tied, -3: board tied, -2: wrong player, -1: failed to place, 0: placed correctly, 1: local win, 2: game win.
        /// </summary>
        /// <param name="local_tile">local_tile.</param>
        /// <param name="player">player.</param>
        /// <returns>If place was successful.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Place(int local_tile, bool player)
        {
            // Checks if we can place at this location
            if (this.tiles[this.focusBoard + 81] == 0 && this.tiles[local_tile + (this.focusBoard * 9)] == 0)
            {
                // Updates the tile
                this.UpdateTile(local_tile + (this.focusBoard * 9), player);

                // Increments turn number for specific board
                this.turns[this.focusBoard] += 1;

                // Increment global turns
                this.turns[9] += 1;

                // Checks for win
                int ret = this.PlaceResult(local_tile, player);

                // If any win detected, return
                if (ret > 0)
                {
                    // this.focusBoard = -1; //if a win has taken place, let the other player choose their board
                    return ret;
                }

                // Tied or already won the newly focused board
                if (this.focusBoard == -1 || this.turns[this.focusBoard] == 9 || this.tiles[81 + this.focusBoard] != 0)
                {
                    // If tie has occurred in this board
                    if (this.turns[this.focusBoard] == 9 && this.tiles[81 + this.focusBoard] == 0)
                    {
                        this.tiles[81 + this.focusBoard] = -2;
                        this.tilesInverted[81 + this.focusBoard] = -2;
                    }

                    return -3;
                }

                return 0;
            }

            // Blocked place location
            if (this.focusBoard == -1 || this.turns[this.focusBoard] == 9 || this.tiles[81 + this.focusBoard] != 0)
            {
                return -3;
            }

            return -1;
        }

        /// <summary>
        /// Updates the focus board.
        /// </summary>
        /// <param name="board">board.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateFocusBoard(int board)
        {
            // Checks if board has been won
            if (this.tiles[81 + board] == 0 && this.turns[board] < 9)
            {
                this.focusBoard = board;
                return;
            }

            // Board has been won, player will choose next location
            this.focusBoard = -1;
        }

        /// <summary>
        /// Will update winEvents according to the last move, 0: no win, 1: local win, 2: game win.
        /// </summary>
        /// <param name="local_tile">local_tile.</param>
        /// <param name="player">player.</param>
        /// <returns>The success value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PlaceResult(int local_tile, bool player)
        {
            // Check for local win
            int ret = this.CheckWin(this.focusBoard, player);

            // If a local win has taken place
            if (ret == 1)
            {
                // Update global board to reflect this victory
                this.UpdateTile(81 + this.focusBoard, player);

                // Check for a victory in the main board
                ret += this.CheckWin(9, player);

                // If victory in main board, return early
                if (ret == 2)
                {
                    return ret;
                }

                // Update the global board win conditions for the opponent
                this.UpdateWinConditions(player ? 152 : 72, this.focusBoard);

                return ret;
            }

            // Cancel out the enemy's win conditions
            this.UpdateWinConditions((player ? 80 : 0) + (this.focusBoard * 8), local_tile);

            return ret;
        }

        /// <summary>
        /// Checks board for win, 1 is win, 0 is no win.
        /// </summary>
        /// <param name="board">board.</param>
        /// <param name="player">player.</param>
        /// <returns>If win.</returns>
        public int CheckWin(int board, bool player)
        {
            int player_const = player ? 1 : -1;
            int ret = 0;

            // Offset within the game tiles
            int offset = board * 9;

            // Offset within the win conditions, saved to int because used multiple times during loop
            int offset_conditions = (board * 8) + (player ? 0 : 80);

            // For all eight conditions
            for (int condition = 0; condition < 8; condition++)
            {
                // This win condition is not possible
                if (!this.winEvents[offset_conditions + condition])
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

        /// <summary>
        /// Cancels out the opponent's win conditions for this given move.
        /// </summary>
        /// <param name="offset">offset.</param>
        /// <param name="local_tile">local_tile.</param>
        public void UpdateWinConditions(int offset, int local_tile)
        {
            switch (local_tile)
            {
                case 0: // Top left
                    this.winEvents[offset] = false; // Top row
                    this.winEvents[offset + 3] = false; // Left column
                    this.winEvents[offset + 6] = false;
                    break;
                case 1: // Top mid
                    this.winEvents[offset] = false;
                    this.winEvents[offset + 4] = false;
                    break;
                case 2: // Top right
                    this.winEvents[offset] = false;
                    this.winEvents[offset + 5] = false;
                    this.winEvents[offset + 7] = false;
                    break;
                case 3: // Mid left
                    this.winEvents[offset + 1] = false;
                    this.winEvents[offset + 3] = false;
                    break;
                case 4: // Mid mid
                    this.winEvents[offset + 1] = false;
                    this.winEvents[offset + 4] = false;
                    this.winEvents[offset + 6] = false;
                    this.winEvents[offset + 7] = false;
                    break;
                case 5: // Mid right
                    this.winEvents[offset + 1] = false;
                    this.winEvents[offset + 5] = false;
                    break;
                case 6: // Bottom left
                    this.winEvents[offset + 2] = false;
                    this.winEvents[offset + 3] = false;
                    this.winEvents[offset + 7] = false;
                    break;
                case 7: // Bottom mid
                    this.winEvents[offset + 2] = false;
                    this.winEvents[offset + 4] = false;
                    break;
                case 8: // Bottom right
                    this.winEvents[offset + 2] = false;
                    this.winEvents[offset + 5] = false;
                    this.winEvents[offset + 6] = false;
                    break;
            }
        }
    }
}
