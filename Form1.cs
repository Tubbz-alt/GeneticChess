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

namespace GeneticChess
{
    public partial class Form1 : Form
    {
        int[] priorSquare { get; set; }
        int[] currentSquare { get; set; }

        private Panel[,] BoardPanel = new Panel[8, 8];
        private Panel[,] BlankPanel = new Panel[8, 8];

        Board ActiveBoard = new Board(new Player(true), new Player(false), new Piece[8, 8], true);
        private int tilesize = 50;
        public Form1()
        {
            InitializeComponent();
            ActiveBoard.Pieces = Board.initBoard(ActiveBoard);
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
                Location = new Point(tilesize * 3, tilesize * 8),
                Text = "Move"
            };
            button.Click += Button_Click;
            Controls.Add(button);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (!(currentSquare is null && priorSquare is null))
            {
                Board board = null;
                try
                {
                    if (ActiveBoard.WTurn != ActiveBoard.Pieces[priorSquare[1], priorSquare[0]].Player.IsW) { throw new Exception("Not your turn"); }
                    board = ActiveBoard.Pieces[priorSquare[1], priorSquare[0]].Move(ActiveBoard, currentSquare[1], currentSquare[0]);

                    if (board.Checks(ActiveBoard.WTurn)) { board = null; MessageBox.Show("Can't leave king in check"); }
                }
                catch(Exception ex) { MessageBox.Show("Invalid move: " + ex.ToString()); return; }
                ActiveBoard = board;
                PanelUpdate();
                MessageBox.Show(board.Moves);
                //ActiveBoard.WTurn = !ActiveBoard.WTurn;
            }
        }

        void newPanel_Click(object sender, EventArgs e)
        {
            Panel p = sender as Panel;
            priorSquare = currentSquare;
            currentSquare = new int[] { p.Location.X / tilesize, p.Location.Y / tilesize };
            PanelUpdate();
        }

        void PanelUpdate()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int ii = 0; ii < 8; ii++)
                {
                    var p = BoardPanel[i, ii];
                    if (i % 2 == 0)
                        p.BackColor = ii % 2 != 0 ? Color.Black : Color.White;
                    else
                        p.BackColor = ii % 2 != 0 ? Color.White : Color.Black;
                    p.BackgroundImage = ActiveBoard.Pieces[ii, i].PieceImage;
                }
            }
            if (priorSquare != null) { BoardPanel[priorSquare[0], priorSquare[1]].BackColor = Color.Red; }
            BoardPanel[currentSquare[0], currentSquare[1]].BackColor = Color.Green;
        }
    }
}
