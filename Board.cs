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
        public bool Stale = false;
        public bool WCheck = false;
        public bool BCheck = false;
        public Board(Player p1, Player p2, Piece[,] pieces, bool wturn)
        {
            P1 = p1; P2 = p2; Pieces = pieces; WTurn = wturn;
        }
        public static Piece[,] initBoard(Board board)
        {
            Player p1 = board.P1; Player p2 = board.P2;
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
            return tempPieces;
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
