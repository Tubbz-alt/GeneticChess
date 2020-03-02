using System;
using System.Collections.Generic;

namespace GeneticChess
{
    [Serializable]
    public class Board
    {
        public Player P1 { get; set; }
        public Player P2 { get; set; }
        public Piece[,] Pieces { get; set; }
        public bool WTurn = true;
        public bool WWin = false;
        public bool BWin = false;
        public bool WCheck = false;
        public bool BCheck = false;
        public int MoveNumber = 0;
        public string Moves = "";
        public Board(Player p1, Player p2, Piece[,] pieces, bool wturn)
        {
            P1 = p1; P2 = p2; Pieces = pieces; WTurn = wturn;
        }
        public Board initBoard()
        {
            Player p1 = P1; Player p2 = P2;
            Piece[,] tempPieces = new Piece[8, 8]
            {
                { new Rook(p2, 0, 0), new Knight(p2, 0, 1), new Bishop(p2, 0, 2), new Queen(p2, 0, 3), new King(p2, 0, 4), new Bishop(p2, 0, 5), new Knight(p2, 0, 6), new Rook(p2, 0, 7) },
                { new Pawn(p2, 1, 0), new Pawn(p2, 1, 1), new Pawn(p2, 1, 2), new Pawn(p2, 1, 3), new Pawn(p2, 1, 4), new Pawn(p2, 1, 5), new Pawn(p2, 1, 6), new Pawn(p2, 1, 7) },
                { new Empty(2, 0), new Empty(2, 1), new Empty(2, 2), new Empty(2, 3), new Empty(2, 4), new Empty(2, 5), new Empty(2, 6), new Empty(2, 7) },
                { new Empty(3, 0), new Empty(3, 1), new Empty(3, 2), new Empty(3, 3), new Empty(3, 4), new Empty(3, 5), new Empty(3, 6), new Empty(3, 7) },
                { new Empty(4, 0), new Empty(4, 1), new Empty(4, 2), new Empty(4, 3), new Empty(4, 4), new Empty(4, 5), new Empty(4, 6), new Empty(4, 7) },
                { new Empty(5, 0), new Empty(5, 1), new Empty(5, 2), new Empty(5, 3), new Empty(5, 4), new Empty(5, 5), new Empty(5, 6), new Empty(5, 7) },
                { new Pawn(p1, 6, 0), new Pawn(p1, 6, 1), new Pawn(p1, 6, 2), new Pawn(p1, 6, 3), new Pawn(p1, 6, 4), new Pawn(p1, 6, 5), new Pawn(p1, 6, 6), new Pawn(p1, 6, 7) },
                { new Rook(p1, 7, 0), new Knight(p1, 7, 1), new Bishop(p1, 7, 2), new Queen(p1, 7, 3), new King(p1, 7, 4), new Bishop(p1, 7, 5), new Knight(p1, 7, 6), new Rook(p1, 7, 7) }
            };
            Pieces = tempPieces;
            return this;
        }
        public static Piece[,] Flip(Piece[,] p)
        {
            Piece[,] a2 = new Piece[8, 8];
            for (int i = 0; i <= 7; i++)
            {
                for (int ii = 0; ii <= 7; ii++)
                {
                    a2[i, ii] = Serializer.DeepClone(p[7 - i, 7 - ii]);
                    a2[i, ii].PosX = i; a2[i, ii].PosY = ii;
                }
            }
            return a2;
        }
        public static Piece[,] AdjustFlip(Piece[,] p, bool isW)
        {
            Piece[,] pieces = Serializer.DeepClone(p);
            if (!isW) { pieces = Flip(pieces); }
            return pieces;
        }
        /// <summary>
        /// Checks if one is in check
        /// </summary>
        /// <param isW?="isW"></param>
        /// <returns></returns>
        public bool amICheck(bool isW)
        {
            if (isW) { if (WCheck) { return true; } }
            else { if (BCheck) { return true; } }
            return false;
        }
        public string ChessNotation(Piece p, int fromx, int fromy, int tox, int toy)
        {
            string temp = string.Empty;
            temp = PieceString(p) + NumLetter(fromx) + (fromy + 1);
            temp += " " + NumLetter(tox) + (toy + 1);
            return temp;
        }
        private char NumLetter(int i)
        {
            if (i > 8 || i < 1) { throw new Exception("Invalid character input"); }
            switch (i)
            {
                case 1: return 'A';
                case 2: return 'B';
                case 3: return 'C';
                case 4: return 'D';
                case 5: return 'E';
                case 6: return 'F';
                case 7: return 'G';
                case 8: return 'H';
            }
            return '$';
        }
        private string PieceString(Piece p)
        {
            if (p is Pawn) { return ""; }
            if (p is Rook) { return "R"; }
            if (p is Queen) { return "Q"; }
            if (p is King) { return "K"; }
            if (p is Bishop) { return "B"; }
            if (p is Knight) { return "N"; }
            throw new Exception("Invalid piece input");
        }
        public Board Swap(int[] start, int[] end)
        {
            //Input verification
            if (Pieces[start[0], start[1]] is Empty) { throw new Exception("Can't swap nothing"); }
            if (!(Pieces[end[0], end[1]] is Empty) && Pieces[start[0], start[1]].Player.IsW == Pieces[end[0], end[1]].Player.IsW)
            { throw new Exception("Can't swap on an own piece"); }
            //Movement
            Board board = Serializer.DeepClone(this);
            board.Pieces[end[0], end[1]] = board.Pieces[start[0], start[1]];
            board.Pieces[end[0], end[1]].PosX = end[0]; board.Pieces[end[0], end[1]].PosY = end[1];
            board.Pieces[start[0], start[1]] = new Empty(start[0], start[1]);
            //Set on first move stuff to false (and enpass to true) for applicable pieces
            if (board.Pieces[end[0], end[1]] is Pawn) { (board.Pieces[end[0], end[1]] as Pawn).twoStep = false; }
            if (board.Pieces[end[0], end[1]] is Pawn && start[0] + (board.Pieces[end[0], end[1]] as Pawn).LegalX == end[0])
            { (board.Pieces[end[0], end[1]] as Pawn).enPass = true; }
            if (board.Pieces[end[0], end[1]] is King) { (board.Pieces[end[0], end[1]] as King).CanCastle = false; }
            if (board.Pieces[end[0], end[1]] is Rook) { (board.Pieces[end[0], end[1]] as Rook).CanCastle = false; }
            board.WTurn = !board.WTurn;
            return board;
        }
        public List<Board> GenerateBoards(bool isW)
        {
            List<Board> Moves = new List<Board>();
            if (WTurn != isW)
            { Console.WriteLine("Not my turn"); return Moves; }
            //Foreach square on the board
            for (int j = 0; j <= 7; j++)
            {
                for (int jj = 0; jj <= 7; jj++)
                {
                    //Piece selected
                    Piece piece = Pieces[j, jj];
                    //If the piece is empty, it can't be moved
                    if (piece is Empty) { continue; }
                    //If the piece is not yours to move, it can't be moved
                    if (piece.Player.IsW != isW) { continue; }
                    //Which side of the board you are on
                    int iFactor;
                    if (isW) { iFactor = -1; }
                    else { iFactor = 1; }

                    if (piece is Pawn)
                    {
                        //Because it is my turn anyway, I can set my remaining pawns' enpass to false (so long as they don't do it this turn)
                        ((Pawn)piece).enPass = false;
                        //x
                        for (int i = 1 * iFactor; Math.Abs(i) <= Math.Abs(2 * iFactor); i = i + iFactor)
                        {   //y
                            for (int ii = -1; ii <= 1; ii++)
                            {
                                Board trialBoard = Serializer.DeepClone(this);
                                try { trialBoard = ((Pawn)trialBoard.Pieces[j, jj]).Move(trialBoard, j + i, jj + ii); }
                                catch { continue; ; }
                                if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            }
                        }
                        continue;
                    }
                    if (piece is Rook)
                    {
                        for (int df = -7; df <= 7; df++)
                        {
                            Board trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Rook)trialBoard.Pieces[j, jj]).Move(trialBoard, j + df, jj); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            
                            trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Rook)trialBoard.Pieces[j, jj]).Move(trialBoard, j, jj + df); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                        }
                        continue;
                    }
                    if (piece is Knight)
                    {
                        for (int dfx = -1 * iFactor; Math.Abs(dfx) <= Math.Abs(2 * iFactor); dfx = dfx + iFactor)
                        {
                            for (int dfy = -1 * iFactor; Math.Abs(dfy) <= Math.Abs(2 * iFactor); dfy = Math.Abs(dfy) + 1)
                            {
                                Board trialBoard = Serializer.DeepClone(this);
                                try { trialBoard = ((Knight)trialBoard.Pieces[j, jj]).Move(trialBoard, j + dfx, jj + dfy); }
                                catch { continue; }
                                if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            }
                        }
                        continue;
                    }
                    if (piece is Bishop)
                    {
                        for (int df = -7; df <= 7; df++)
                        {
                            Board trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Bishop)trialBoard.Pieces[j, jj]).Move(trialBoard, j + df, jj + df); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            
                            trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Bishop)trialBoard.Pieces[j, jj]).Move(trialBoard, j - df, jj + df); }
                            catch { continue; ; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                        }
                        continue;
                    }
                    if (piece is Queen)
                    {
                        //fixed?
                        for (int df = -7; df <= 7; df++)
                        {
                            Board trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Queen)trialBoard.Pieces[j, jj]).Move(trialBoard, j + df, jj); }
                            catch { continue; }
                            if (!trialBoard.amICheck(isW) && trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            
                            trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Queen)trialBoard.Pieces[j, jj]).Move(trialBoard, j, jj + df); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                        }
                        //Bishop
                        for (int df = -7; df <= 7; df++)
                        {
                            Board trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Queen)trialBoard.Pieces[j, jj]).Move(trialBoard, j + df, jj + df); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            
                            trialBoard = Serializer.DeepClone(this);
                            try { trialBoard = ((Queen)trialBoard.Pieces[j, jj]).Move(trialBoard, j - df, jj + df); }
                            catch { continue; }
                            if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                        }
                        continue;
                    }
                    if (piece is King)
                    {
                        for (int dfx = -1; dfx <= 1; dfx++)
                        {
                            for (int dfy = -3; dfy <= 3; dfy++)
                            {
                                Board trialBoard = Serializer.DeepClone(this);
                                try { trialBoard = ((King)trialBoard.Pieces[j, jj]).Move(trialBoard, j + dfx, jj + dfy); }
                                catch { continue; }
                                if (trialBoard.Pieces != Pieces) { Moves.Add(trialBoard); }
                            }
                        }
                        continue;
                    }
                }
            }
            return Moves;
        }
        public List<Board> GenMoves(bool wturn)
        {
            var boards = new List<Board>();
            foreach (Piece p in Pieces)
            {
                if (p is Empty || p.Player.IsW != wturn) { continue; }
                //Does nothing (abstract method)
                var v = p.GenerateMoves(this);

                if (p is Pawn) { v = (p as Pawn).GenerateMoves(this); }
                if (p is King) { v = (p as King).GenerateMoves(this); }
                if (p is Knight) { v = (p as Knight).GenerateMoves(this); }
                if (p is Rook) { v = (p as Rook).GenerateMoves(this); }
                if (p is Queen) { v = (p as Queen).GenerateMoves(this); }
                if (p is Bishop) { v = (p as Bishop).GenerateMoves(this); }
                

                foreach (Board b in v)
                {
                    //Need to check detect these before adding
                    boards.Add(b);
                }
            }
            return boards;
        }
        /// <summary>
        /// Check if king is in check
        /// </summary>
        public bool Checks(bool isW)
        {
            WCheck = false; BCheck = false;
            foreach (Piece p in Pieces)
            {
                if (p is King && p.Player.IsW == isW)
                {
                    bool? oleft = null; bool? oright = null; bool? oup = null; bool? odown = null;
                    bool? dleft = null; bool? dright = null; bool? dup = null; bool? ddown = null;

                    for (int i = 1; i <= 7; i++)
                    {
                        //oleft
                        if (oleft is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX - i, p.PosY] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX - i, p.PosY].Player.IsW != isW
                                        && (Pieces[p.PosX - i, p.PosY] is Rook
                                        || Pieces[p.PosX - i, p.PosY] is Queen))
                                    { oleft = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        oleft = false;
                                    }
                                }
                            }
                            catch { oleft = false; }
                        }
                        //oright
                        if (oright is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX + i, p.PosY] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX + i, p.PosY].Player.IsW != isW
                                        && (Pieces[p.PosX + i, p.PosY] is Rook
                                        || Pieces[p.PosX + i, p.PosY] is Queen))
                                    { oright = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        oright = false;
                                    }
                                }
                            }
                            catch { oright = false; }
                        }
                        //oup
                        if (oup is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX, p.PosY - i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX, p.PosY - i].Player.IsW != isW
                                        && (Pieces[p.PosX, p.PosY - i] is Rook
                                        || Pieces[p.PosX, p.PosY - i] is Queen))
                                    { oup = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        oup = false;
                                    }
                                }
                            }
                            catch { oup = false; }
                        }
                        //odown
                        if (odown is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX, p.PosY + i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX, p.PosY + i].Player.IsW != isW
                                        && (Pieces[p.PosX, p.PosY + i] is Rook
                                        || Pieces[p.PosX, p.PosY + i] is Queen))
                                    { odown = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        odown = false;
                                    }
                                }
                            }
                            catch { odown = false; }
                        }

                        //dleft
                        if (dleft is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX + i, p.PosY + i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a bishop/queen, then you're in check
                                    if (Pieces[p.PosX + i, p.PosY + i].Player.IsW != isW
                                        && (Pieces[p.PosX + i, p.PosY + i] is Bishop
                                        || Pieces[p.PosX + i, p.PosY + i] is Queen))
                                    { dleft = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        dleft = false;
                                    }
                                }
                            }
                            catch { dleft = false; }
                        }
                        //dright
                        if (dright is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX - i, p.PosY - i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX - i, p.PosY - i].Player.IsW != isW
                                        && (Pieces[p.PosX - i, p.PosY - i] is Bishop
                                        || Pieces[p.PosX - i, p.PosY - i] is Queen))
                                    { dright = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        dright = false;
                                    }
                                }
                            }
                            catch { dright = false; }
                        }
                        //dup
                        if (dup is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX + i, p.PosY - i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX + i, p.PosY - i].Player.IsW != isW
                                        && (Pieces[p.PosX + i, p.PosY - i] is Bishop
                                        || Pieces[p.PosX + i, p.PosY - i] is Queen))
                                    { dup = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        dup = false;
                                    }
                                }
                            }
                            catch { dup = false; }
                        }
                        //ddown
                        if (ddown is null)
                        {
                            try
                            {
                                //if empty, pass
                                if ((Pieces[p.PosX - i, p.PosY + i] is Empty)) { }
                                //Otherwise, find out what it is
                                else
                                {
                                    //If it's hostile and a rook/queen, then you're in check
                                    if (Pieces[p.PosX - i, p.PosY + i].Player.IsW != isW
                                        && (Pieces[p.PosX - i, p.PosY + i] is Bishop
                                        || Pieces[p.PosX - i, p.PosY + i] is Queen))
                                    { ddown = true; }
                                    //Otherwise, you're not in check
                                    else
                                    {
                                        ddown = false;
                                    }
                                }
                            }
                            catch { ddown = false; }
                        }
                    }

                    bool? kingpawn = null;
                    for (int i = -1; i <= 1; i++)
                    {
                        if (kingpawn != true)
                        {
                            if (i != 0)
                            {
                                try
                                {
                                    if (Pieces[p.PosX + i, p.PosY + i] is King
                                        || (Pieces[p.PosX + i, p.PosY + i] is Pawn
                                        && Pieces[p.PosX + i, p.PosY + i].Player.IsW != isW))
                                    { kingpawn = true; }
                                    else { if (kingpawn != true) { kingpawn = false; } }
                                }
                                catch { if (kingpawn != true) { kingpawn = false; } }
                                try
                                {
                                    if (Pieces[p.PosX - i, p.PosY + i] is King
                                        || (Pieces[p.PosX - i, p.PosY + i] is Pawn
                                        && Pieces[p.PosX - i, p.PosY + i].Player.IsW != isW))
                                    { kingpawn = true; }
                                    else { if (kingpawn != true) { kingpawn = false; } }
                                }
                                catch { if (kingpawn != true) { kingpawn = false; } }
                                try
                                {
                                    if (Pieces[p.PosX, p.PosY + i] is King) { kingpawn = true; }
                                    else { if (kingpawn != true) { kingpawn = false; } }
                                }
                                catch { if (kingpawn != true) { kingpawn = false; } }
                                try
                                {
                                    if (Pieces[p.PosX + i, p.PosY] is King) { kingpawn = true; }
                                    else { if (kingpawn != true) { kingpawn = false; } }
                                }
                                catch { if (kingpawn != true) { kingpawn = false; } }
                            }
                        }

                        bool? knight = null;
                        //May break on sides of board?
                        //SUPER VERBOSE
                        try
                        {
                            if (Pieces[p.PosX + 1, p.PosY + 2] is Knight) { knight = true; }
                            else { if (knight != true) { knight = false; } }
                        }
                        catch { if (knight != true) { knight = false; } }
                        try
                        {
                            if (Pieces[p.PosX - 1, p.PosY - 2] is Knight) { knight = true; }
                            else { if (knight != true) { knight = false; } }
                        }
                        catch { if (knight != true) { knight = false; } }
                        try
                        {
                            if (Pieces[p.PosX + 1, p.PosY - 2] is Knight) { knight = true; }
                            else { if (knight != true) { knight = false; } }
                        }
                        catch { if (knight != true) { knight = false; } }
                        try
                        {
                            if (Pieces[p.PosX - 1, p.PosY + 2] is Knight) { knight = true; }
                            else { if (knight != true) { knight = false; } }
                        }
                        catch { if (knight != true) { knight = false; } }

                        if (oright == true || oleft == true || oup == true || odown == true
                            || dright == true || dleft == true || dup == true || ddown == true
                            || knight == true || kingpawn == true)
                        {
                            if (isW) { WCheck = true; }
                            else { BCheck = true; }
                        }
                    }
                }
            }
            if (isW) { if (WCheck) { return true; } else { return false; } }
            else { if (BCheck) { return true; } else { return false; } }
        }
    }
}
