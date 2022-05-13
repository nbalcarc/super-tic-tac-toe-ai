// <copyright file="Board.cs" company="Adam Nassar and Nathan Balcarcel">
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
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// WinForms Interface, runs the main board.
    /// </summary>
    public partial class Board : Form
    {
        private Button[] buttons;
        private PictureBox[] highlights;

        private Generation g = new Generation();
        private Game ttt = new Game();
        private AI ai = new AI();
        private Thread generationThread;

        private bool gameWon;
        private bool generationLoop;

        private int interrupt;
        private string interruptString;

        /// <summary>
        /// Initializes a new instance of the <see cref="Board"/> class.
        /// </summary>
        public Board()
        {
            this.InitializeComponent();
            Application.Idle += this.HandleApplicationIdle;
            this.gameWon = false;
            this.generationLoop = false;
            this.interrupt = 0;

            this.buttons = new Button[81]
            {
                this.TLTL, this.TLTM, this.TLTR, this.TLML, this.TLMM, this.TLMR, this.TLBL, this.TLBM, this.TLBR,
                this.TMTL, this.TMTM, this.TMTR, this.TMML, this.TMMM, this.TMMR, this.TMBL, this.TMBM, this.TMBR,
                this.TRTL, this.TRTM, this.TRTR, this.TRML, this.TRMM, this.TRMR, this.TRBL, this.TRBM, this.TRBR,
                this.MLTL, this.MLTM, this.MLTR, this.MLML, this.MLMM, this.MLMR, this.MLBL, this.MLBM, this.MLBR,
                this.MMTL, this.MMTM, this.MMTR, this.MMML, this.MMMM, this.MMMR, this.MMBL, this.MMBM, this.MMBR,
                this.MRTL, this.MRTM, this.MRTR, this.MRML, this.MRMM, this.MRMR, this.MRBL, this.MRBM, this.MRBR,
                this.BLTL, this.BLTM, this.BLTR, this.BLML, this.BLMM, this.BLMR, this.BLBL, this.BLBM, this.BLBR,
                this.BMTL, this.BMTM, this.BMTR, this.BMML, this.BMMM, this.BMMR, this.BMBL, this.BMBM, this.BMBR,
                this.BRTL, this.BRTM, this.BRTR, this.BRML, this.BRMM, this.BRMR, this.BRBL, this.BRBM, this.BRBR,
            };

            this.highlights = new PictureBox[9] { this.TL, this.TM, this.TR, this.ML, this.MM, this.MR, this.BL, this.BM, this.BR };
        }

        /// <summary>
        /// Should be called when the GUI goes idle. If there is an interrupt, it will be handled here.
        /// </summary>
        /// /// <param name="sender">sender.</param>
        /// /// <param name="e">e.</param>
        public void HandleApplicationIdle(object sender, EventArgs e)
        {
            if (this.interrupt != 0)
            {
                switch (this.interrupt)
                {
                    // Update textbox display.
                    case 1:
                        Console.WriteLine("bing bang");
                        this.TextBox.Text = this.interruptString;
                        this.UpdateGUI();
                        break;
                    default:
                        break;
                }

                this.interrupt = 0;
            }
        }

        private void Board_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < this.buttons.Length; i++)
            {
                this.UpdateFont(this.buttons[i]);
            }

            for (int i = 0; i < 9; i++)
            {
                this.highlights[i].Visible = true;
            }

            this.TextBox.Text = "Right click on board for options, otherwise click a square!";
        }

        /// <summary>
        /// Updates the font given a button.
        /// </summary>
        /// <param name="tile">tile.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateFont(Button tile)
        {
            tile.Font = new Font(FontFamily.GenericSansSerif, 20);
        }

        /// <summary>
        /// Receives the player's move and updates the board.
        /// </summary>
        /// <param name="globalTile">globalTile.</param>
        /// <param name="player">player.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PlayerMove(int globalTile, bool player)
        {
            if (this.gameWon)
            {
                return;
            }

            this.TextBox.Text = "Player 1: Click on any square!";

            int board = globalTile / 9, localTile = globalTile % 9;
            int resultBoard = this.ttt.ChooseBoardFirstPlayer(board);

            // If unsuccessful board choice
            if (resultBoard < 0)
            {
                return;
            }

            int resultPlace = this.ttt.PlaceFirstPlayer(localTile);

            // If unsuccessful place
            if (resultPlace == -1 || resultPlace == -2)
            {
                return;
            }

            switch (resultPlace)
            {
                case -3:
                    this.buttons[globalTile].Text = "X";

                    for (int i = 0; i < 9; i++)
                    {
                        this.buttons[(board * 9) + i].BackColor = Color.Gray;
                    }

                    this.TextBox.Text = "Tie board for " + board;

                    break;
                case 0:
                    // Successful place
                    this.buttons[globalTile].Text = "X";
                    break;
                case 1:
                    // Local win
                    this.buttons[globalTile].Text = "X";

                    for (int i = 0; i < 9; i++)
                    {
                        this.buttons[(board * 9) + i].BackColor = Color.Aquamarine;
                    }

                    this.TextBox.Text = "Player 1 has claimed board " + board;

                    break;
                case 2:
                    // Global win
                    this.buttons[globalTile].Text = "X";

                    /*
                    for (int i = 0; i < 81; i++)
                    {
                        buttons[i].BackColor = Color.Aquamarine;
                    }
                    */

                    for (int i = 0; i < 9; i++)
                    {
                        this.highlights[i].Visible = false;
                    }

                    this.TextBox.Text = "Player 1 Wins!";
                    this.gameWon = true;

                    return;
            }

            if (this.TieDetector(board))
            {
                return;
            }

            // New ai based on neural network (no learning yet, just random weights)
            board = this.AIMove();

            this.TieDetector(board);
        }

        /// <summary>
        /// Retrieves the AI's move and updates the board.
        /// </summary>
        /// <returns>The tile chosen.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AIMove()
        {
            (int[], int[], int) state = this.ttt.GetGameInfo();

            // First either choose a new board or pass the current one
            if (state.Item3 == -1)
            {
                Console.WriteLine($"DEBUGGING, choosing board");
                this.ttt.ChooseBoardSecondPlayer(this.ai.ChooseBoard(state.Item2, false));
            }
            else
            {
                Console.WriteLine($"DEBUGGING, reusing board");
                this.ttt.ChooseBoardSecondPlayer(state.Item3);
            }

            Console.WriteLine($"DEBUGGING board: {state.Item3}");

            int resultPlace, localTile, board;

            // Update the new info
            state = this.ttt.GetGameInfo();

            localTile = this.ai.NextMove(state.Item2, state.Item3);
            resultPlace = this.ttt.PlaceSecondPlayer(localTile);
            board = state.Item3;

            Console.WriteLine($"DEBUGGING tile: {localTile}");
            Console.WriteLine($"DEBUGGING result: {resultPlace}");

            int tempBoard;

            switch (resultPlace)
            {
                case -3:
                    this.buttons[(board * 9) + localTile].Text = "O";

                    // Claim all Tiles
                    for (int i = 0; i < 9; i++)
                    {
                        this.buttons[(board * 9) + i].BackColor = Color.Gray;
                    }

                    for (int i = 0; i < 9; i++)
                    {
                        this.highlights[i].Visible = false;
                    }

                    this.TextBox.Text = "Tie board " + board;

                    break;
                case 0:
                    // Successful place
                    this.buttons[(board * 9) + localTile].Text = "O";
                    tempBoard = this.ttt.GetGameInfo().Item3;

                    for (int i = 0; i < 9; i++)
                    {
                        if (i == tempBoard)
                        {
                            this.highlights[tempBoard].Visible = true;
                            continue;
                        }

                        this.highlights[i].Visible = false;
                    }

                    break;
                case 2:
                    // Global win
                    this.buttons[(board * 9) + localTile].Text = "O";

                    /*
                    for (int i = 0; i < 81; i++) //claim all tiles
                    {
                        buttons[i].BackColor = Color.OrangeRed;
                    }
                    */

                    for (int i = 0; i < 9; i++)
                    {
                        this.highlights[i].Visible = false;
                    }

                    this.TextBox.Text = "AI Wins!";
                    this.gameWon = true;

                    return board;
                case 1:
                    // Local win
                    this.buttons[(board * 9) + localTile].Text = "O";

                    // Claim all Tiles
                    for (int i = 0; i < 9; i++)
                    {
                        this.buttons[(board * 9) + i].BackColor = Color.OrangeRed;
                    }

                    tempBoard = this.ttt.GetGameInfo().Item3;

                    for (int i = 0; i < 9; i++)
                    {
                        if (i == tempBoard)
                        {
                            this.highlights[tempBoard].Visible = true;
                            continue;
                        }

                        this.highlights[i].Visible = false;
                    }

                    this.TextBox.Text = "AI has claimed board " + board;

                    break;
            }

            // Update the new info
            state = this.ttt.GetGameInfo();

            // First either choose a new board or pass the current one
            if (state.Item3 == -1)
            {
                // Highlight all boards as long as they're not claimed
                for (int i = 0; i < 9; i++)
                {
                    if (state.Item1[81 + i] == 0)
                    {
                        this.highlights[i].Visible = true;
                    }
                }
            }

            return board;
        }

        /// <summary>
        /// Detects if a tie has occured.
        /// </summary>
        /// <param name="board">board.</param>
        /// <returns>boolean.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TieDetector(int board)
        {
            int ties = 0, p1 = 0, p2 = 0;
            for (int i = 0; i < 9; i++)
            {
                // Local tie
                if (this.ttt.Tiles[i + 81] == -2)
                {
                    ties++;
                }

                // p1
                else if (this.ttt.Tiles[i + 81] == 1)
                {
                    p1++;
                }
                else if (this.ttt.Tiles[i + 81] == -1)
                {
                    p2++;
                }
            }

            // All boards are either tied or won, and no one has won the game
            if (ties + p1 + p2 == 9)
            {
                this.gameWon = true;

                // If game tie, need to still update the board
                switch (this.ttt.Tiles[81 + board])
                {
                    case -2:
                        // A tie
                        for (int i = 0; i < 9; i++)
                        {
                            this.buttons[(board * 9) + i].BackColor = Color.Gray;
                        }

                        break;
                    case 1:
                        // p1 win
                        for (int i = 0; i < 9; i++)
                        {
                            this.buttons[(board * 9) + i].BackColor = Color.Aquamarine;
                        }

                        break;
                    case 2:
                        // p2 win
                        for (int i = 0; i < 9; i++)
                        {
                            this.buttons[(board * 9) + i].BackColor = Color.OrangeRed;
                        }

                        break;
                }

                return true;
            }

            return false;
        }

        private void ResetBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Button b in this.buttons)
            {
                b.Text = string.Empty;
                b.BackColor = Color.White;
            }

            for (int i = 0; i < 9; i++)
            {
                this.highlights[i].Visible = true;
            }

            this.gameWon = false;

            this.ttt = new Game();
        }

        private void Train10GenerationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int generationsToTrain = 10;
            this.TextBox.Text = "Training... ";
            this.UpdateGUI();
            Thread generationThread;

            for (int i = 0; i < 10; i++)
            {
                generationThread = new Thread(this.g.NextGeneration);
                ////this.g.NextGeneration();
                generationThread.Start();
                generationThread.Join();
                this.TextBox.Text = "Training... Generation " + (i + 1).ToString() + "/" + generationsToTrain.ToString() + " completed.";
                this.UpdateGUI();
            }

            this.ai = this.g.AIS[0];

            this.TextBox.Text = "Current Generation: " + this.g.GetGeneration.ToString();
        }

        private void TrainingMenuItem_Click(object sender, EventArgs e)
        {
            this.generationLoop = !this.generationLoop;

            // Begin training loop
            if (this.generationLoop)
            {
                this.TextBox.Text = "Initiating training session... ";
                this.generationThread = new Thread(this.TrainingLoop);
                this.generationThread.IsBackground = true;
                this.generationThread.Start();
                this.trainingMenuItem.Text = "End Training";
            }

            // End training loop
            else
            {
                this.TextBox.Text = "Ending training session... ";
                this.UpdateGUI();
                this.generationThread.Join();
                this.TextBox.Text = "Completed training on Generation " + this.g.GetGeneration.ToString();
                this.trainingMenuItem.Text = "Start Training";
            }
        }

        /// <summary>
        /// The main training loop. Controlled via a toggleable menu item.
        /// </summary>
        private void TrainingLoop()
        {
            int generation = this.g.GetGeneration;
            string text = string.Empty;
            int updateText = 2;

            while (this.generationLoop)
            {
                this.g.NextGeneration();

                if (updateText == 0)
                {
                    continue;
                }

                switch (updateText)
                {
                    case 1:
                        if (text.Equals(this.TextBox.Text))
                        {
                            text = "Training... Generation " + this.g.GetGeneration.ToString() + " completed.";
                            this.interruptString = text;
                            this.interrupt = 1;
                        }
                        else
                        {
                            updateText = 0;
                        }

                        break;
                    case 2:
                        text = "Training... Generation " + this.g.GetGeneration.ToString() + " completed.";
                        this.interruptString = text;
                        this.interrupt = 1;
                        updateText = 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the GUI instantly instead of when a method ends.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateGUI()
        {
            this.Invalidate();
            this.Update();
            this.Refresh();
            Application.DoEvents();
        }

        private void TLTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(0, true);
        }

        private void TMTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(9, true);
        }

        private void TRTL_Click(object sender, EventArgs e)
        {
           this.PlayerMove(18, true);
        }

        private void MLTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(27, true);
        }

        private void MMTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(36, true);
        }

        private void MRTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(45, true);
        }

        private void BLTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(54, true);
        }

        private void BMTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(63, true);
        }

        private void BRTL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(72, true);
        }

        private void TLTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(1, true);
        }

        private void TMTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(10, true);
        }

        private void TRTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(19, true);
        }

        private void MLTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(28, true);
        }

        private void MMTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(37, true);
        }

        private void MRTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(46, true);
        }

        private void BLTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(55, true);
        }

        private void BMTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(64, true);
        }

        private void BRTM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(73, true);
        }

        private void TLTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(2, true);
        }

        private void TMTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(11, true);
        }

        private void TRTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(20, true);
        }

        private void MLTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(29, true);
        }

        private void MMTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(38, true);
        }

        private void MRTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(47, true);
        }

        private void BLTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(56, true);
        }

        private void BMTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(65, true);
        }

        private void BRTR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(74, true);
        }

        private void TLML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(3, true);
        }

        private void TMML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(12, true);
        }

        private void TRML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(21, true);
        }

        private void MLML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(30, true);
        }

        private void MMML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(39, true);
        }

        private void MRML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(48, true);
        }

        private void BLML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(57, true);
        }

        private void BMML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(66, true);
        }

        private void BRML_Click(object sender, EventArgs e)
        {
            this.PlayerMove(75, true);
        }

        private void TLMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(4, true);
        }

        private void TMMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(13, true);
        }

        private void TRMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(22, true);
        }

        private void MLMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(31, true);
        }

        private void MMMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(40, true);
        }

        private void MRMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(49, true);
        }

        private void BLMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(58, true);
        }

        private void BMMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(67, true);
        }

        private void BRMM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(76, true);
        }

        private void TLMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(5, true);
        }

        private void TMMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(14, true);
        }

        private void TRMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(23, true);
        }

        private void MLMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(32, true);
        }

        private void MMMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(41, true);
        }

        private void MRMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(50, true);
        }

        private void BLMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(59, true);
        }

        private void BMMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(68, true);
        }

        private void BRMR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(77, true);
        }

        private void TLBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(6, true);
        }

        private void TMBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(15, true);
        }

        private void TRBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(24, true);
        }

        private void MLBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(33, true);
        }

        private void MMBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(42, true);
        }

        private void MRBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(51, true);
        }

        private void BLBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(60, true);
        }

        private void BMBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(69, true);
        }

        private void BRBL_Click(object sender, EventArgs e)
        {
            this.PlayerMove(78, true);
        }

        private void TLBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(7, true);
        }

        private void TMBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(16, true);
        }

        private void TRBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(25, true);
        }

        private void MLBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(34, true);
        }

        private void MMBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(43, true);
        }

        private void MRBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(52, true);
        }

        private void BLBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(61, true);
        }

        private void BMBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(70, true);
        }

        private void BRBM_Click(object sender, EventArgs e)
        {
            this.PlayerMove(79, true);
        }

        private void TLBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(8, true);
        }

        private void TMBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(17, true);
        }

        private void TRBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(26, true);
        }

        private void MLBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(35, true);
        }

        private void MMBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(44, true);
        }

        private void MRBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(53, true);
        }

        private void BLBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(62, true);
        }

        private void BMBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(71, true);
        }

        private void BRBR_Click(object sender, EventArgs e)
        {
            this.PlayerMove(80, true);
        }
    }
}
