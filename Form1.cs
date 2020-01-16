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
        private Panel[,] BoardPanel = new Panel[8, 8];
        Board ActiveBoard = new Board();
        private int tilesize = 50;
        public Form1()
        {
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
                    
                    var request = WebRequest.Create(ActiveBoard.Pieces[i, ii].PictureURL);
                    //set request to a different picture for a diff 

                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        Bitmap b = Bitmap.FromStream(stream) as Bitmap;
                        b.MakeTransparent(Color.White);
                        b = new Bitmap(b, new Size(30, 30));
                        Image image = b as Image;
                        newPanel.BackgroundImageLayout = ImageLayout.Center;
                        newPanel.BackgroundImage = image;
                    };

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
        }
    }
}
