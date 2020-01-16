using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticChess
{
    class Board
    {
        public Piece[,] Pieces { get; set; }
        public Board()
        {
            Pieces = new Piece[8, 8]
            {
                { new Rook(false), new Knight(false), new Bishop(false), new King(false), new Queen(false), new Bishop(false), new Knight(false), new Rook(false) },
                { new Pawn(false), new Pawn(false), new Pawn(false), new Pawn(false),  new Pawn(false), new Pawn(false), new Pawn(false), new Pawn(false), },
                { new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty() },
                { new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty() },
                { new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty() },
                { new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty(), new Empty() },
                { new Pawn(true), new Pawn(true), new Pawn(true), new Pawn(true), new Pawn(true), new Pawn(true), new Pawn(true), new Pawn(true), },
                { new Rook(true), new Knight(true), new Bishop(true), new Queen(true), new King(true), new Bishop(true), new Knight(true), new Rook(true) }
            };
        }
    }
}
