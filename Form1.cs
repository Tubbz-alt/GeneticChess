using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace GeneticChess
{
    public partial class Form1 : Form
    {
        int[] priorSquare { get; set; }
        int[] currentSquare { get; set; }

        private Panel[,] BoardPanel = new Panel[8, 8];
        private Button[] Buttons = new Button[2];
        Board activeboard;
        public Board ActiveBoard { get { return activeboard; } set { activeboard = value; PanelUpdate(); } }
        private int tilesize;
        private int[] formsize { get; set; }
        public Form1()
        {
            ActiveBoard = new Board(new Player(true), new Player(false), new Piece[8, 8], true).initBoard();
            InitializeComponent();
            for (int i = 0; i < 8; i++)
            {
                for (int ii = 0; ii < 8; ii++)
                {
                    var newPanel = new Panel
                    {
                        Size = new Size(tilesize, tilesize),
                        Location = new Point(tilesize * i, tilesize * ii)
                    };

                    newPanel.Click += newPanel_Click;
                    newPanel.BackgroundImageLayout = ImageLayout.Center;
                    if (!(ActiveBoard.Pieces[ii, i] is Empty))
                    {
                        newPanel.BackgroundImage = ActiveBoard.Pieces[ii, i].PieceImage;
                        newPanel.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
                    }

                    //Add panel to controls
                    Controls.Add(newPanel);
                    //Add panel to board
                    BoardPanel[i, ii] = newPanel;

                    //Color the board
                    if (i % 2 == 0)
                        newPanel.BackColor = ii % 2 != 0 ? Color.Black : Color.White;
                    else
                        newPanel.BackColor = ii % 2 != 0 ? Color.White : Color.Black;
                }
            }
            Button button = new Button
            {
                Size = new Size(tilesize * 2, tilesize / 2),
                Location = new Point(tilesize * 2, tilesize * 8),
                Text = "Move"
            };
            Buttons[0] = button;
            Button button2 = new Button
            {
                Size = new Size(tilesize * 2, tilesize / 2),
                Location = new Point(tilesize * 4, tilesize * 8),
                Text = "Compete"
            };
            Buttons[1] = button2;
            button.Click += Button_Click;
            button2.Click += Button2_Click;
            Controls.Add(button);
            Controls.Add(button2);

            //Set dynamic scaling
            this.ResizeEnd += Form1_Resize;
            Size = new Size(500, 500);
            formsize = new int[] { 0, 0 };
            Form1_Resize(this, new EventArgs());
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (Size.Width == formsize[0] && Size.Height == formsize[1]) { return; }
            Control control = (Control)sender;
            tilesize = control.Size.Width / 9;
            // Set form dimentions
            control.Size = new Size(control.Size.Width - (tilesize / 2), control.Size.Width + (tilesize / 2));
            //Assuming the form is >16 px resize the board
            if (control.Size.Width > 16)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int ii = 0; ii < 8; ii++)
                    {
                        BoardPanel[i, ii].Size = new Size(tilesize, tilesize);
                        BoardPanel[i, ii].Location = new Point(tilesize * i, tilesize * ii);
                    }
                }
                int fontsize = 8;
                if (tilesize < 40) { fontsize = 4; }
                if (tilesize > 60) { fontsize = 12; }
                if (tilesize > 70) { fontsize = 16; }
                Buttons[0].Size = new Size(tilesize * 2, tilesize / 2);
                Buttons[0].Location = new Point(tilesize * 2, tilesize * 8);
                Buttons[0].Font = new Font("Tahoma", fontsize);
                Buttons[1].Size = new Size(tilesize * 2, tilesize / 2);
                Buttons[1].Location = new Point(tilesize * 4, tilesize * 8);
                Buttons[1].Font = new Font("Tahoma", fontsize);
            }
            formsize[0] = Serializer.DeepClone(Size.Width);
            formsize[1] = Serializer.DeepClone(Size.Height);
        }
        private void Button_Click(object sender, EventArgs e)
        {
            if (!(currentSquare is null && priorSquare is null))
            {
                Board board;
                try
                {
                    if (ActiveBoard.WTurn != ActiveBoard.Pieces[priorSquare[1], priorSquare[0]].Player.IsW) { throw new Exception("Not your turn"); }
                    List<Board> possibilities = ActiveBoard.GenMoves(ActiveBoard.WTurn, false);
                    //Need to redo (the pieces can't be the same if they're on different squares lol)
                    foreach (Board b in possibilities) { if (b.Pieces[priorSquare[1], priorSquare[0]] is Empty && b.Pieces[currentSquare[1], currentSquare[0]].ValidMoveType(ActiveBoard.Pieces[priorSquare[1], priorSquare[0]]))
                            //If the piece moved is the same on both boards then the boards are the same
                        { board = b; goto noerror; }
                    }
                    //If the board was not generated, it is invalid
                    throw new Exception("Not a valid move");
                noerror:

                    //If king is in check the move is invalid
                    foreach (Piece p in board.Pieces) { if (p is King && p.Player.IsW == ActiveBoard.WTurn) {
                            if ((p as King).Check(board)) { board = null; throw new Exception("Can't leave king in check"); } } } 
                }
                catch (Exception ex) { MessageBox.Show("Invalid move: " + ex.ToString()); return; }
                ActiveBoard = board;
               
                //Reset colors and then the prior/current squares
                if (priorSquare[0] % 2 == 0) { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = priorSquare[1] % 2 != 0 ? Color.Black : Color.White; }
                else { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = priorSquare[1] % 2 != 0 ? Color.White : Color.Black; }                
                if (currentSquare[0] % 2 == 0) { BoardPanel[currentSquare[0], currentSquare[1]].BackColor = currentSquare[1] % 2 != 0 ? Color.Black : Color.White; }
                else { BoardPanel[currentSquare[0], currentSquare[1]].BackColor = currentSquare[1] % 2 != 0 ? Color.White : Color.Black; }
                
                priorSquare = null;
                currentSquare = null;
                
                PanelUpdate();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var thread =
            new Thread(() =>
            {
                var tempboard = Serializer.DeepClone(ActiveBoard);
                var genetics = new Genetics(true, 5, .5, 1.2, .1, this);
                genetics.Evolve();
                /*
                NN nn1 = new NN().Init(); NN nn2 = new NN().Init();
                var p1 = new Player(true); var p2 = new Player(false);
                nn1.player = p1; nn2.player = p2;

                while (!tempboard.WWin && !tempboard.BWin)
                {
                    tempboard = nn1.Move(tempboard, tempboard.WTurn);
                    Invoke((Action)delegate { ActiveBoard = Serializer.DeepClone(tempboard); PanelUpdate(); });

                    ActiveBoard = nn2.Move(tempboard, tempboard.WTurn);
                    Invoke((Action)delegate { ActiveBoard = Serializer.DeepClone(tempboard); PanelUpdate(); });
                }
                */
            });
            thread.IsBackground = true;
            thread.Start();
        }

        void newPanel_Click(object sender, EventArgs e)
        {
            Panel p = sender as Panel;
            //Update old priorsquare's color
            if (!(priorSquare is null))
            {
                //Don't update if an already selected tile was clicked on
                if (p.Location.X / tilesize == priorSquare[0] && p.Location.Y / tilesize == priorSquare[1]) { return; }
                if (p.Location.X / tilesize == currentSquare[0] && p.Location.Y / tilesize == currentSquare[1]) { return; }
                //Set prior square's color to default
                if (priorSquare[0] % 2 == 0) { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = priorSquare[1] % 2 != 0 ? Color.Black : Color.White; }
                else { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = priorSquare[1] % 2 != 0 ? Color.White : Color.Black; }
            }
            priorSquare = currentSquare;
            currentSquare = new int[] { p.Location.X / tilesize, p.Location.Y / tilesize };
            PanelUpdate();
        }

        void PanelUpdate()
        {
            if (BoardPanel[0, 0] is null) { return; }
            for (int i = 0; i < 8; i++)
            {
                for (int ii = 0; ii < 8; ii++)
                {
                    var p = BoardPanel[i, ii];
                    p.BackgroundImage = ActiveBoard.Pieces[ii, i].PieceImage;
                }
            }
            if (priorSquare != null) { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = Color.Red; }
            if (currentSquare != null) { BoardPanel[currentSquare[0], currentSquare[1]].BackColor = Color.Green; }
        }
    }
}
