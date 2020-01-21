using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;

namespace GeneticChess
{
    abstract class Piece
    {
        double Value;
        string Abreviation;
        public bool IsWhite;
        public Image PieceImage; 
        string PictureURL = "https://cdn5.vectorstock.com/i/1000x1000/15/29/chess-pieces-including-king-queen-rook-pawn-knight-vector-2621529.jpg";
        public virtual Piece[,] Move(Piece[,] board) { return null; }
        protected Image GetImage(int x1, int y1, int x2, int y2)
        {
            var request = WebRequest.Create(PictureURL);
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                Bitmap b = Bitmap.FromStream(stream) as Bitmap;
                Rectangle rectangle = new Rectangle(x1, y1, x2, y2);
                System.Drawing.Imaging.PixelFormat format = b.PixelFormat;
                Bitmap b2 = b.Clone(rectangle, format);
                //b2.MakeTransparent(Color.White);
                b2 = new Bitmap(b2, new Size(40, 40));
                for (int i = 0; i < b2.Height; i++)
                {
                    for (int ii = 0; ii < b2.Width; ii++)
                    {
                        Color pixelColor = b2.GetPixel(i, ii);
                        if (pixelColor.GetBrightness() > .5) { b2.SetPixel(i, ii, Color.Transparent); }
                        else {  b2.SetPixel(i, ii, Color.HotPink); }
                    }
                }
                return b2 as Image;
            };
        }
    }
    class Pawn : Piece
    {
        public Pawn(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(85, 100, 200, 200); }
            else { PieceImage = GetImage(85, 385, 200, 200); }
        }
        public override Piece[,] Move(Piece[,] board) { return null; }
    }
    class Knight : Piece
    {
        public Knight(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(700, 60, 230, 230); }
            else { PieceImage = GetImage(700, 350, 230, 230); }
        }
    }
    class Bishop : Piece
    {
        public Bishop(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(485, 55, 240, 240); }
            else { PieceImage = GetImage(485, 340, 240, 240); }
        }
    }
    class Rook : Piece
    {
        public Rook(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(285, 90, 220, 220); }
            else { PieceImage = GetImage(285, 365, 220, 220); }
        }
    }
    class Queen : Piece
    {
        public Queen(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(45, 645, 282, 282); }
            else { PieceImage = GetImage(469, 645, 282, 282); }
        }
    }
    class King : Piece
    {
        public King(bool iswhite)
        {
            IsWhite = iswhite;
            if (iswhite) { PieceImage = GetImage(250, 610, 290, 290); }
            else { PieceImage = GetImage(677, 610, 290, 290); }
        }
    }
    class Empty : Piece
    {
    }
}
