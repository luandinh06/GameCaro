using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GameCaro
{
    class ChessBoardManager
    {
        #region Properties

        public Panel ChessBoard { get => chessBoard; set => chessBoard = value; }
        public List<Player> Players { get => players; set => players = value; }
        public int CurentPlayer { get => curentPlayer; set => curentPlayer = value; }
        public TextBox TextBox { get => textBox; set => textBox = value; }
        public PictureBox PictureBox { get => pictureBox; set => pictureBox = value; }
        public List<List<Button>> MaTrix { get => maTrix; set => maTrix = value; }
        public Stack<PlayerInfo> PlayerInfo { get => playerInfo; set => playerInfo = value; }
        public bool IsYourTurn { get => isYourTurn; set => isYourTurn = value; }

        private Panel chessBoard;

        private List<Player> players;

        private int curentPlayer;

        private TextBox textBox;

        private PictureBox pictureBox;

        private List<List<Button>> maTrix;

        private Stack<PlayerInfo> playerInfo;

        private bool isYourTurn;

        #endregion

        #region Initialize
        public ChessBoardManager(Panel chessBoard, TextBox textBox, PictureBox pictureBox)
        {
            this.ChessBoard = chessBoard;
            this.TextBox = textBox;
            this.PictureBox = pictureBox;
            this.Players = new List<Player>()
            {
                new Player("Player X", Image.FromFile(Application.StartupPath + @"\Resources\X.png")),
                new Player("Player O", Image.FromFile(Application.StartupPath + @"\Resources\O.png"))
            };
            this.CurentPlayer = 0;

            PlayerInfo = new Stack<PlayerInfo>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// draw matrix chessboard
        /// </summary>
        public void DrawChessBoard()
        {
            // add matrix
            ChessBoard.Enabled = true;
            ChessBoard.Controls.Clear();

            MaTrix = new List<List<Button>>();

            Button oldbtn = new Button() { Width = 0, Height = 0 };
            for (int i = 0; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                // add new row into matrix
                MaTrix.Add(new List<Button>());

                for (int j = 0; j <= Cons.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Cons.BUTTON_WIDTH,
                        Height = Cons.BUTTON_HEIGHT,
                        Location = new Point(oldbtn.Location.X + Cons.BUTTON_WIDTH, oldbtn.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = new Point() { X = j, Y = i }
                    };
                    ChessBoard.Controls.Add(btn);

                    //add button into matrix
                    MaTrix[i].Add(btn);

                    oldbtn = btn;

                    //event click
                    btn.Click += Btn_Click;
                }

                oldbtn.Location = new Point(0, oldbtn.Location.Y + Cons.BUTTON_HEIGHT);
                oldbtn.Width = 0;
                oldbtn.Height = 0;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (isYourTurn != true)
            {
                return;
            }

            if (btn.BackgroundImage != null)
                return;

            isYourTurn = false;

            mark(btn,true);
            //btn.BackgroundImage = Players[CurentPlayer].Mark;
            //btn.BackgroundImageLayout = ImageLayout.Stretch;

            SaveStatusForUndo(btn);

            if (playerMarked != null)
                playerMarked(this, new ButtonClickEvent(GetChessPoint(btn)));

            if (isEndGame(btn))
            {
                EndedGame();
            }

        }

        public void EndedGame()
        {
            if (endGame != null)
                endGame(this, new EventArgs());
        }

        private void mark(Button btn, bool IsYourMark)
        {
            int OppositionOFCurentPlayer = CurentPlayer == 1 ? 0 : 1;
            if (IsYourMark == true)
            {
                btn.BackgroundImage = Players[CurentPlayer].Mark;
                TextBox.Text = Players[OppositionOFCurentPlayer].Name;
                PictureBox.Image = Players[OppositionOFCurentPlayer].Mark;
            }
            else
            {
                btn.BackgroundImage = Players[OppositionOFCurentPlayer].Mark;
                TextBox.Text = Players[CurentPlayer].Name;
                PictureBox.Image = Players[CurentPlayer].Mark;
            }
            
            btn.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimaryDiagoalLine(btn) || isEndSubDiagoalLine(btn);
        }

        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (MaTrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }

            int countRight = 0;
            for (int i = point.X + 1; i < Cons.CHESS_BOARD_WIDTH; i++)
            {
                if (MaTrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }

            return countLeft + countRight == 5;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countHigh = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (MaTrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countHigh++;
                }
                else
                    break;
            }

            int countLow = 0;
            for (int i = point.Y + 1; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                if (MaTrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countLow++;
                }
                else
                    break;
            }
            return countHigh + countLow == 5;
        }

        private bool isEndPrimaryDiagoalLine(Button btn)
        {
            /*
            Point point = GetChessPoint(btn);
            int countA = 0;
            int j = point.Y;
            for (int i = point.X; i >= 0; i--)
            {               
                if (i< 0 || j<0)
                    break;
                if (MaTrix[j][i].BackgroundImage == btn.BackgroundImage)
                    {                        
                        countA++;
                        j--;
                    }
                else
                    {
                        j--;
                        break;
                    }
            }
            int countB = 0;
            point.Y++;
            point.X++;
            j = point.Y;
            for (int i = point.X; i <= Cons.CHESS_BOARD_HEIGHT; i++)
            {                
                if (i >= Cons.CHESS_BOARD_HEIGHT || j >= Cons.CHESS_BOARD_WIDTH)
                    break;
                if (MaTrix[j][i].BackgroundImage == btn.BackgroundImage)
                {                 
                    countB++;
                    j++;
                }
                else
                {
                    j++;
                    break;
                }                                
            }
            return countA + countB == 5;
            */
            Point point = GetChessPoint(btn);
            int countA = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if ((point.X - i) < 0 || (point.Y - i) < 0)
                    break;
                if (MaTrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countA++;
                }
                else
                    break;
            }
            int countB = 0;
            for (int i = 1; (i + point.X) <= (Cons.CHESS_BOARD_HEIGHT - 1); i++)
            {
                if ((point.X + i) > (Cons.CHESS_BOARD_HEIGHT - 1) || (point.Y + i) > (Cons.CHESS_BOARD_WIDTH - 1))
                    break;
                if (MaTrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countB++;
                }
                else
                    break;
            }
            return countA + countB == 5;
        }

        private bool isEndSubDiagoalLine(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countA = 0;
            int j = point.Y;
            for (int i = point.X; i >= 0; i++)
            {
                if (i >= Cons.CHESS_BOARD_HEIGHT || j < 0)
                    break;
                if (MaTrix[j][i].BackgroundImage == btn.BackgroundImage)
                {
                    countA++;
                    j--;
                }
                else
                {
                    j--;
                    break;
                }
            }
            int countB = 0;
            point.Y++;
            point.X--;
            j = point.Y;
            for (int i = point.X; i <= Cons.CHESS_BOARD_HEIGHT; i--)
            {
                if (i < 0 || j >= Cons.CHESS_BOARD_WIDTH)
                    break;
                if (MaTrix[j][i].BackgroundImage == btn.BackgroundImage)
                {
                    countB++;
                    j++;
                }
                else
                {
                    j++;
                    break;
                }
            }
            return countA + countB == 5;
        }

        /// <summary>
        /// Get positon of button in matrix
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        private Point GetChessPoint(Button btn)
        {
            Point point = new Point();
            point = (Point)btn.Tag;
            return point;
        }

        public void Undo()
        {
            if (PlayerInfo.Count <= 0)
                return;
            IsYourTurn = !isYourTurn; 

            PlayerInfo playerInfo = PlayerInfo.Pop();
            Point point = playerInfo.Point;
            MaTrix[point.Y][point.X].BackgroundImage = null;
        }


        private void SaveStatusForUndo(Button btn)
        {
            PlayerInfo.Push(new PlayerInfo(GetChessPoint(btn), CurentPlayer));
        }


        public void OtherPlayerMark(Point point, int? currentPlayer = null)
        {
            Button btn = MaTrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            isYourTurn = true;

            mark(btn,false);
            SaveStatusForUndo(btn);

            //if (playerMarked != null)
            //    playerMarked(this, new ButtonClickEvent(GetChessPoint(btn)));          

            if (isEndGame(btn))
            {
                EndedGame();
            }

        }
        #endregion


        #region event
        private event EventHandler<ButtonClickEvent> playerMarked;
        public event EventHandler<ButtonClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }

        private event EventHandler endGame;
        public event EventHandler EndGame
        {
            add
            {
                endGame += value;
            }
            remove
            {
                endGame -= value;
            }
        }

        #endregion
    }

    public class ButtonClickEvent : EventArgs
    {
        private Point point;

        public Point Point { get => point; set => point = value; }

        public ButtonClickEvent(Point point)
        {
            this.point = point;
        }
    }
}
