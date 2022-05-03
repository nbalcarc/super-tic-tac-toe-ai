using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperTicTacToe
{
    public partial class Board : Form
    {
        private Button[] buttons;
        private PictureBox[] highlights;
        private bool gameWon;
        

        Game ttt = new Game();
        AI ai = new AI();

        public Board()
        {
            InitializeComponent();

            gameWon = false;

            buttons = new Button[81] { TLTL, TLTM, TLTR, TLML, TLMM, TLMR, TLBL, TLBM, TLBR, 
                TMTL, TMTM, TMTR, TMML, TMMM, TMMR, TMBL, TMBM, TMBR, 
                TRTL, TRTM, TRTR, TRML, TRMM, TRMR, TRBL, TRBM, TRBR, 
                MLTL, MLTM, MLTR, MLML, MLMM, MLMR, MLBL, MLBM, MLBR, 
                MMTL, MMTM, MMTR, MMML, MMMM, MMMR, MMBL, MMBM, MMBR,
                MRTL, MRTM, MRTR, MRML, MRMM, MRMR, MRBL, MRBM, MRBR, 
                BLTL, BLTM, BLTR, BLML, BLMM, BLMR, BLBL, BLBM, BLBR, 
                BMTL, BMTM, BMTR, BMML, BMMM, BMMR, BMBL, BMBM, BMBR, 
                BRTL, BRTM, BRTR, BRML, BRMM, BRMR, BRBL, BRBM, BRBR };

            highlights = new PictureBox[9] {TL, TM, TR, ML, MM, MR, BL, BM, BR};
        }

        private void Board_Load(object sender, EventArgs e)
        {

            for (int i = 0; i < buttons.Length; i++)
			{
                UpdateFont(buttons[i]);
			}
            //foreach (Button b in buttons)
            //{
            //    UpdateFont(b);
            //}

            for (int i = 0; i < 9; i++)
            {
                highlights[i].Visible = true;
            }

            TextBox.Text = "Player 1: Click on any square!";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateFont(Button tile)
        {
            tile.Font = new Font(FontFamily.GenericSansSerif, 20);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateText(int globalTile, bool player)
        {
            if (gameWon)
            {
                return;
            }

            TextBox.Text = "Player 1: Click on any square!";

            int board = globalTile / 9, localTile = globalTile % 9;
            int resultBoard = ttt.ChooseBoardFirstPlayer(board);

            if (resultBoard < 0) //if unsuccessful board choice
            {
                return;
            }

            int resultPlace = ttt.PlaceFirstPlayer(localTile);

            if (resultPlace == -1 || resultPlace == -2) //if unsuccessful place
            {
                return;
            }

            switch (resultPlace)
			{
                case -4:
                    buttons[globalTile].Text = "X";
                    for (int i = 0; i < 81; i++)
                    {
                        buttons[i].BackColor = Color.Gray;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "Tie game!";
                    gameWon = true;
                    break;
                case -3:
                    buttons[globalTile].Text = "X";
                    for (int i = 0; i < 9; i++)
                    {
                        buttons[board * 9 + i].BackColor = Color.Gray;
                    }
                    this.TextBox.Text = "Tie board for " + board;
                    break;
                case 0: //successful place
                    buttons[globalTile].Text = "X";
                    break;
                case 1: //local win
                    buttons[globalTile].Text = "X";
                    for (int i = 0; i < 9; i++)
                    {
                        buttons[board * 9 + i].BackColor = Color.Aquamarine;
                    }
                    this.TextBox.Text = "Player 1 has claimed board " + board;
                    break;
                case 2: //global win
                    buttons[globalTile].Text = "X";
                    for (int i = 0; i < 81; i++)
                    {
                        buttons[i].BackColor = Color.Aquamarine;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "Player 1 Wins!";
                    gameWon = true;
                    return;
            }

            //AIButtonClick(localTile); //old ai based on random generation
            SmartAIButtonClick(); //new ai based on neural network (no learning yet, just random weights)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SmartAIButtonClick()
        {
            (int[], int[], int) state = ttt.GetGameInfo();
            if (state.Item3 == -1) //first either choose a new board or pass the current one
            {
                Console.WriteLine($"DEBUGGING, choosing board");
                ttt.ChooseBoardSecondPlayer(ai.ChooseBoard(state.Item2, false));
            } else
            {
                Console.WriteLine($"DEBUGGING, reusing board");
                ttt.ChooseBoardSecondPlayer(state.Item3);
            }
            Console.WriteLine($"DEBUGGING board: {state.Item3}");

            int resultPlace, localTile, board;
            state = ttt.GetGameInfo(); //update the new info

            localTile = ai.NextMove(state.Item2, state.Item3);
            resultPlace = ttt.PlaceSecondPlayer(localTile);
            board = state.Item3;

            Console.WriteLine($"DEBUGGING tile: {localTile}");
            Console.WriteLine($"DEBUGGING result: {resultPlace}");

            int tempBoard;
            switch (resultPlace)
            {
                case -4:
                    buttons[board * 9 + localTile].Text = "O";
                    for (int i = 0; i < 81; i++) //claim all tiles
                    {
                        buttons[i].BackColor = Color.Gray;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "AI Wins!";
                    gameWon = true;
                    break;
                case -3:
                    buttons[board * 9 + localTile].Text = "O";
                    for (int i = 0; i < 9; i++) //claim all tiles
                    {
                        buttons[board * 9 + i].BackColor = Color.Gray;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "Tie board " + board;
                    break;
                case 0: //successful place
                    buttons[board * 9 + localTile].Text = "O";
                    tempBoard = ttt.GetGameInfo().Item3;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == tempBoard)
                        {
                            highlights[tempBoard].Visible = true;
                            continue;
                        }
                        highlights[i].Visible = false;
                    }
                    break;
                case 2: //global win
                    buttons[board * 9 + localTile].Text = "O";
                    for (int i = 0; i < 81; i++) //claim all tiles
                    {
                        buttons[i].BackColor = Color.OrangeRed;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "AI Wins!";
                    gameWon = true;
                    return;
                case 1: //local win
                    buttons[board * 9 + localTile].Text = "O";
                    for (int i = 0; i < 9; i++) //claim all tiles
                    {
                        buttons[board * 9 + i].BackColor = Color.OrangeRed;
                    }
                    tempBoard = ttt.GetGameInfo().Item3;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == tempBoard)
                        {
                            highlights[tempBoard].Visible = true;
                            continue;
                        }
                        highlights[i].Visible = false;
                    }
                    this.TextBox.Text = "AI has claimed board " + board;
                    break;
            }

            state = ttt.GetGameInfo(); //update the new info
            if (state.Item3 == -1) //first either choose a new board or pass the current one
            {
                for (int i = 0; i < 9; i++) //highlight all boards as long as they're not claimed
                {
                    if (state.Item1[81 + i] == 0)
                    {
                        highlights[i].Visible = true;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AIButtonClick(int board)
        {
            Random r = new Random();

            ttt.ChooseBoardSecondPlayer(board);

            int resultPlace, localTile;

            do
            {
                do
                {
                    localTile = r.Next(0,9);
                } while (buttons[board * 9 + localTile].Text == "X" || buttons[board * 9 + localTile].Text == "O");

                resultPlace = ttt.PlaceSecondPlayer(localTile);
            } while (resultPlace < 0);

            switch (resultPlace)
            {
                case 0: //successful place
                    buttons[board * 9 + localTile].Text = "O";
                    int tempBoard = ttt.GetGameInfo().Item3;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i == tempBoard)
                        {
                            highlights[tempBoard].Visible = true;
                            continue;
                        }
                        highlights[i].Visible = false;
                    }
                    break;
                case 1: //local win
                    for (int i = 0; i < 9; i++) //claim all tiles
                    {
                        buttons[board * 9 + i].Text = "O";
                        buttons[board * 9 + i].BackColor = Color.OrangeRed;

                        
                    }
                    int[] state = ttt.GetGameInfo().Item1;
                    for (int i = 0; i < 9; i++) //highlight all boards as long as they're not claimed
                    {
                        if (state[81 + i] == 0)
                        {
                            highlights[i].Visible = true;
                        }
                    }
                    break;
                case 2: //global win
                    // TODO FILL THIS IN LATER
                    break;
            }

            //this.TextBox.Text = ttt.GetGameInfo().Item3.ToString();
        }

        private void TLTL_Click(object sender, EventArgs e)
        {
            UpdateText(0, true);
        }

        private void TMTL_Click(object sender, EventArgs e)
        {
            UpdateText(9, true);
        }

        private void TRTL_Click(object sender, EventArgs e)
        {
           UpdateText(18, true);
        }

        private void MLTL_Click(object sender, EventArgs e)
        {
            UpdateText(27, true);
        }

        private void MMTL_Click(object sender, EventArgs e)
        {
            UpdateText(36, true);
        }

        private void MRTL_Click(object sender, EventArgs e)
        {
            UpdateText(45, true);
        }

        private void BLTL_Click(object sender, EventArgs e)
        {
            UpdateText(54, true);
        }

        private void BMTL_Click(object sender, EventArgs e)
        {
            UpdateText(63, true);
        }

        private void BRTL_Click(object sender, EventArgs e)
        {
            UpdateText(72, true);
        }

        private void TLTM_Click(object sender, EventArgs e)
        {
            UpdateText(1, true);
        }

        private void TMTM_Click(object sender, EventArgs e)
        {
            UpdateText(10, true);
        }

        private void TRTM_Click(object sender, EventArgs e)
        {
            UpdateText(19, true);
        }

        private void MLTM_Click(object sender, EventArgs e)
        {
            UpdateText(28, true);
        }

        private void MMTM_Click(object sender, EventArgs e)
        {
            UpdateText(37, true);
        }

        private void MRTM_Click(object sender, EventArgs e)
        {
            UpdateText(46, true);
        }

        private void BLTM_Click(object sender, EventArgs e)
        {
            UpdateText(55, true);
        }

        private void BMTM_Click(object sender, EventArgs e)
        {
            UpdateText(64, true);
        }

        private void BRTM_Click(object sender, EventArgs e)
        {
            UpdateText(73, true);
        }

        private void TLTR_Click(object sender, EventArgs e)
        {
            UpdateText(2, true);
        }

        private void TMTR_Click(object sender, EventArgs e)
        {
            UpdateText(11, true);
        }

        private void TRTR_Click(object sender, EventArgs e)
        {
            UpdateText(20, true);
        }

        private void MLTR_Click(object sender, EventArgs e)
        {
            UpdateText(29, true);
        }

        private void MMTR_Click(object sender, EventArgs e)
        {
            UpdateText(38, true);
        }

        private void MRTR_Click(object sender, EventArgs e)
        {
            UpdateText(47, true);
        }

        private void BLTR_Click(object sender, EventArgs e)
        {
            UpdateText(56, true);
        }

        private void BMTR_Click(object sender, EventArgs e)
        {
            UpdateText(65, true);
        }

        private void BRTR_Click(object sender, EventArgs e)
        {
            UpdateText(74, true);
        }

        private void TLML_Click(object sender, EventArgs e)
        {
            UpdateText(3, true);
        }

        private void TMML_Click(object sender, EventArgs e)
        {
            UpdateText(12, true);
        }

        private void TRML_Click(object sender, EventArgs e)
        {
            UpdateText(21, true);
        }

        private void MLML_Click(object sender, EventArgs e)
        {
            UpdateText(30, true);
        }

        private void MMML_Click(object sender, EventArgs e)
        {
            UpdateText(39, true);
        }

        private void MRML_Click(object sender, EventArgs e)
        {
            UpdateText(48, true);
        }

        private void BLML_Click(object sender, EventArgs e)
        {
            UpdateText(57, true);
        }

        private void BMML_Click(object sender, EventArgs e)
        {
            UpdateText(66, true);
        }

        private void BRML_Click(object sender, EventArgs e)
        {
            UpdateText(75, true);
        }

        private void TLMM_Click(object sender, EventArgs e)
        {
            UpdateText(4, true);
        }

        private void TMMM_Click(object sender, EventArgs e)
        {
            UpdateText(13, true);
        }

        private void TRMM_Click(object sender, EventArgs e)
        {
            UpdateText(22, true);
        }

        private void MLMM_Click(object sender, EventArgs e)
        {
            UpdateText(31, true);
        }

        private void MMMM_Click(object sender, EventArgs e)
        {
            UpdateText(40, true);
        }

        private void MRMM_Click(object sender, EventArgs e)
        {
            UpdateText(49, true);
        }

        private void BLMM_Click(object sender, EventArgs e)
        {
            UpdateText(58, true);
        }

        private void BMMM_Click(object sender, EventArgs e)
        {
            UpdateText(67, true);
        }

        private void BRMM_Click(object sender, EventArgs e)
        {
            UpdateText(76, true);
        }

        private void TLMR_Click(object sender, EventArgs e)
        {
            UpdateText(5, true);
        }

        private void TMMR_Click(object sender, EventArgs e)
        {
            UpdateText(14, true);
        }

        private void TRMR_Click(object sender, EventArgs e)
        {
            UpdateText(23, true);
        }

        private void MLMR_Click(object sender, EventArgs e)
        {
            UpdateText(32, true);
        }

        private void MMMR_Click(object sender, EventArgs e)
        {
            UpdateText(41, true);
        }

        private void MRMR_Click(object sender, EventArgs e)
        {
            UpdateText(50, true);
        }

        private void BLMR_Click(object sender, EventArgs e)
        {
            UpdateText(59, true);
        }

        private void BMMR_Click(object sender, EventArgs e)
        {
            UpdateText(68, true);
        }

        private void BRMR_Click(object sender, EventArgs e)
        {
            UpdateText(77, true);
        }

        private void TLBL_Click(object sender, EventArgs e)
        {
            UpdateText(6, true);
        }

        private void TMBL_Click(object sender, EventArgs e)
        {
            UpdateText(15, true);
        }

        private void TRBL_Click(object sender, EventArgs e)
        {
            UpdateText(24, true);
        }

        private void MLBL_Click(object sender, EventArgs e)
        {
            UpdateText(33, true);
        }

        private void MMBL_Click(object sender, EventArgs e)
        {
            UpdateText(42, true);
        }

        private void MRBL_Click(object sender, EventArgs e)
        {
            UpdateText(51, true);
        }

        private void BLBL_Click(object sender, EventArgs e)
        {
            UpdateText(60, true);
        }

        private void BMBL_Click(object sender, EventArgs e)
        {
            UpdateText(69, true);
        }

        private void BRBL_Click(object sender, EventArgs e)
        {
            UpdateText(78, true);
        }

        private void TLBM_Click(object sender, EventArgs e)
        {
            UpdateText(7, true);
        }

        private void TMBM_Click(object sender, EventArgs e)
        {
            UpdateText(16, true);
        }

        private void TRBM_Click(object sender, EventArgs e)
        {
            UpdateText(25, true);
        }

        private void MLBM_Click(object sender, EventArgs e)
        {
            UpdateText(34, true);
        }

        private void MMBM_Click(object sender, EventArgs e)
        {
            UpdateText(43, true);
        }

        private void MRBM_Click(object sender, EventArgs e)
        {
            UpdateText(52, true);
        }

        private void BLBM_Click(object sender, EventArgs e)
        {
            UpdateText(61, true);
        }

        private void BMBM_Click(object sender, EventArgs e)
        {
            UpdateText(70, true);
        }

        private void BRBM_Click(object sender, EventArgs e)
        {
            UpdateText(79, true);
        }

        private void TLBR_Click(object sender, EventArgs e)
        {
            UpdateText(8, true);
        }

        private void TMBR_Click(object sender, EventArgs e)
        {
            UpdateText(17, true);
        }

        private void TRBR_Click(object sender, EventArgs e)
        {
            UpdateText(26, true);
        }

        private void MLBR_Click(object sender, EventArgs e)
        {
            UpdateText(35, true);
        }

        private void MMBR_Click(object sender, EventArgs e)
        {
            UpdateText(44, true);
        }

        private void MRBR_Click(object sender, EventArgs e)
        {
            UpdateText(53, true);
        }

        private void BLBR_Click(object sender, EventArgs e)
        {
            UpdateText(62, true);
        }

        private void BMBR_Click(object sender, EventArgs e)
        {
            UpdateText(71, true);
        }

        private void BRBR_Click(object sender, EventArgs e)
        {
            UpdateText(80, true);
        }
    }
}
