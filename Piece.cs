using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticChess
{
    abstract class Piece
    {
        double Value;
        string Abreviation;
        public string PictureURL;
        public virtual Piece[,] Move(Piece[,] board) { return null; }
    }
    class Pawn : Piece
    {
        public Pawn()
        {
            PictureURL = "https://media.istockphoto.com/vectors/chess-pawn-icon-element-of-minimalistic-icon-for-mobile-concept-and-vector-id948733576?k=6&m=948733576&s=612x612&w=0&h=HFHf_VyUghVVBDxJvokkjxiqZZ9RC3bmdH1Rb63VT0k=";
        }
        public override Piece[,] Move(Piece[,] board) { return null; }
    }
    class Knight : Piece
    {

    }
    class Bishop : Piece
    {

    }
    class Rook : Piece
    {

    }
    class Queen : Piece
    {

    }
    class King : Piece
    {

    }
    class Empty : Piece
    {

    }
}
