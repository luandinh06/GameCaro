using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameCaro
{
    public partial class Form1 : Form
    {
        #region Properties 

        ChessBoardManager chessboard;
        SocketManager Socket;

        #endregion

        public Form1()
        {
            InitializeComponent();
            // fix lỗi xung đột giữa các Thread
            //Control.CheckForIllegalCrossThreadCalls = false;
            Load();

        }

        #region Function  

        private void Load()
        {
            ProcessBarCountDown_Init();
            TimerCountDown_Init();
            

            chessboard = new ChessBoardManager(pnChessBoard, tbPlayer, pictureBox);
            Socket = new SocketManager();
            chessboard.PlayerMarked += Chessboard_PlayerMarked;
            chessboard.EndGame += Chessboard_EndGame;
            Socket.ReceivedEvent += Socket_ReceivedEvent;

            Receive();

            Newgame();
        }

        private void Chessboard_EndGame(object sender, EventArgs e)
        {
            pnChessBoard.Enabled = false;
            timerCountDown.Stop();
            MessageBox.Show("Đã 5 con");
        }

        private void Newgame()
        {
            pnChessBoard.Enabled = true;
            undoToolStripMenuItem1.Enabled = false;
            PB.Enabled = true;
            timerCountDown.Stop();
            PB.Value = 0;
            chessboard.DrawChessBoard();
        }

        private void Undo()
        {
            chessboard.Undo();
            undoToolStripMenuItem1.Enabled = false;
            Socket.Send(new SocketData((int)SocketCommand.UNDO, new Point(0, 0), ""));
            timerCountDown.Stop();
            timerCountDown.Start();
            PB.Value = 0;
        }

        private void Quit()
        {
            Application.Exit();
        }

        private void EndGame()
        {
            timerCountDown.Stop();
            MessageBox.Show("Hết giờ");
        }


        private void Receive()
        {
            try
            {
                Thread listenThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(20);
                        try
                        {
                            Socket.ReceiveEvent();
                        }
                        catch (Exception)
                        {
                            //MessageBox.Show("Lỗi khi nhận thông tin");
                        }
                    }
                });

                listenThread.IsBackground = true;
                listenThread.Start();
            }
            catch (Exception)
            {
            }

        }

        #endregion

        #region Event

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát", "Thông Báo", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                e.Cancel = true;
            else
            {
                try
                {
                    Socket.Send(new SocketData((int)SocketCommand.QUIT, new Point(0, 0), ""));
                }
                catch (Exception)
                {

                }
            }
        }

        private void timerCountDown_Tick(object sender, EventArgs e)
        {
            PB.PerformStep();
            if (PB.Value >= PB.Maximum)
                EndGame(); 
        }

        private void ProcessBarCountDown_Init()
        {
            PB.Step = Cons.COOL_DOWN_STEP;
            PB.Maximum = Cons.COOL_DOWN_TIME;
            PB.Value = 0;
        }

        private void TimerCountDown_Init()
        {
            timerCountDown.Enabled = true;
            timerCountDown.Interval = Cons.COOL_DOWN_INTERNAL;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // get TP of computer
            tbIP.Text = Socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(tbIP.Text))
            {
                tbIP.Text = Socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }

            //connect 
            Socket.IP = tbIP.Text;
            if (!Socket.ConnectServer())
            {
                Socket.CreateServer();

                label1.Text = "server";
                tbLan.Text = "Đang Đợi Kết Nối";
            }
            else
            {
                label1.Text = "client";
                tbLan.Text = "Đã Kết Nối";
                chessboard.IsYourTurn = false;
                chessboard.CurentPlayer = 1;
                try
                {
                    Socket.Send(new SocketData((int)SocketCommand.START_GAME, new Point(0,0), ""));
                }
                catch (Exception)
                {
                }
            }
        }

        private void Chessboard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            undoToolStripMenuItem1.Enabled = true;
            timerCountDown.Start();
            PB.Value = 0;

            Socket.Send(new SocketData((int)SocketCommand.SEND_POINT, e.Point, ""));

        }

        private void Socket_ReceivedEvent(object sender, ReceiveEvent e)
        {
            ProcessData(e.Data);
        }

        #region MenuStrip Event

        private void newGameToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Newgame();
            Socket.Send(new SocketData((int)SocketCommand.NEW_GAME, new Point(0, 0), ""));
        }

        private void undoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Quit();
        }

        #endregion
        #endregion

        private void ProcessData(SocketData data)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                switch (data.Command)
                {
                    case (int)SocketCommand.NOTIFY:
                        MessageBox.Show(data.Message);
                        break;
                    case (int)SocketCommand.NEW_GAME:
                        Newgame();
                        break;
                    case (int)SocketCommand.SEND_POINT:
                        PB.Value = 0;
                        timerCountDown.Start();
                        chessboard.OtherPlayerMark(data.Point);
                        break;
                    case (int)SocketCommand.UNDO:
                        chessboard.Undo();
                        timerCountDown.Stop();
                        timerCountDown.Start();
                        PB.Value = 0;
                        break;
                    case (int)SocketCommand.END_GAME:
                        break;
                    case (int)SocketCommand.QUIT:
                        MessageBox.Show("Đối thủ đã thoát");
                        break;
                    case (int)SocketCommand.START_GAME:
                        chessboard.IsYourTurn = true;
                        chessboard.CurentPlayer = 0;
                        tbLan.Text = "Đã Kết Nối";        
                        break;
                    default:
                        break;
                }
            }));
        }
    }

}
