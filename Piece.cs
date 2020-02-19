using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;

namespace GeneticChess
{
    [Serializable]
    public abstract class Piece
    {
        public string Name { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int LegalX { get; set; }
        public int LegalY { get; set; }
        public Image PieceImage { get; set; }
        public Player Player { get; set; }
        public abstract Board Move(Board b, int toX, int toY);
        public abstract List<Board> GenerateMoves(Board b);
        string PictureURL = "https://cdn5.vectorstock.com/i/1000x1000/15/29/chess-pieces-including-king-queen-rook-pawn-knight-vector-2621529.jpg";
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
                        else { b2.SetPixel(i, ii, Color.HotPink); }
                    }
                }
                return b2 as Image;
            };
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    public class Pawn : Piece
    {
        public bool enPass, twoStep;
        public Pawn(Player player, int posX, int posY)
        {
            Name = "Pawn"; PosX = posX; PosY = posY; Player = player; twoStep = true; enPass = false;
            if (player.IsW == true) { LegalX = -1; }
            else { LegalX = 1; }
            if (player.IsW == true) { PieceImage = GetImage(85, 100, 200, 200); }
            else { PieceImage = GetImage(85, 385, 200, 200); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();
            int scale = Player.IsW ? -1 : 1;
            for (int i = 1 * scale; i <= 2 * scale; i += scale)
            {
                for (int ii = -1 * scale; ii <= 1 * scale; ii += scale)
                {
                    //Ignore moves outside the board
                    if (PosX + ii > 7 || PosY + i > 7) { continue; }

                    //If moving sideways
                    if (ii != 0) {
                        //Can't move forward and sideways two steps
                        if (i == 2) { continue; }
                        //If an empty square check if can enpassed
                        if (b.Pieces[PosY + i, PosX + ii] is Empty) {
                            if (b.Pieces[PosY, PosX + ii] is Pawn && ((Pawn)b.Pieces[PosY, PosX + ii]).enPass)
                            { goto addmove; }
                            continue;
                        }
                        //If enemy piece can capture
                        if (b.Pieces[PosY + i, PosX + ii].Player.IsW != Player.IsW) { goto addmove; }
                    }
                    //If it's moving forward one step
                    if (i == 1) {
                        //If there is nothing in front, it's legal
                        if (b.Pieces[PosY + i, PosX + ii] is Empty) { goto addmove; }
                    }
                    //If it's moving enpassant, check if it hasn't moved and if interceptings squares are empty
                    if (twoStep && b.Pieces[PosY + i, PosX + ii] is Empty && b.Pieces[PosY + 1, PosX + ii] is Empty)
                    //If so, then it's legal
                    { goto addmove; }

                    addmove:
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
            }
            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            var board = Serializer.DeepClone(b);
            int prex = PosX, prey = PosY;
            bool move = true;
            if (board.Pieces[toX, toY] is Empty || board.Pieces[toX, toY].Player.IsW != Player.IsW)
            {
                //standard move
                if (toX == PosX + LegalX && toY == PosY && toX <= 7 && toY <= 7 && board.Pieces[toX, toY] is Empty)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    move = false; enPass = false; twoStep = false;
                    board.WTurn = !board.WTurn;
                }
                //capture
                if (!(board.Pieces[toX, toY] is Empty) && move && (toY == PosY + 1 || toY == PosY - 1) && toX == PosX + LegalX && toX <= 7 && toY <= 7 &&
                    board.Pieces[toX, toY].Player.IsW != Player.IsW)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    move = false; enPass = false; twoStep = false;
                    board.WTurn = !board.WTurn;
                }
                //twostep
                if (board.Pieces[toX, toY] is Empty && twoStep == true && toX == PosX + (2 * LegalX) && toY == PosY && toX <= 7 && toY <= 7 && move)
                {
                    if (board.Pieces[PosX + LegalX, PosY] is Empty)
                    {
                        board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                        PosX = toX; PosY = toY;
                        board.Pieces.SetValue(this, new int[] { PosX, PosY });
                        twoStep = false; move = false; enPass = true;
                        board.WTurn = !board.WTurn;
                    }
                }
                //enpass
                if ((toY == PosY + 1 || toY == PosY - 1) && toX == PosX + LegalX && toX <= 7 && toY <= 7 && board.Pieces[toX - LegalX, toY] is Pawn &&
                    ((Pawn)board.Pieces[toX - LegalX, toY]).enPass && move && board.Pieces[toX - LegalX, toY].Player.IsW != Player.IsW && board.Pieces[toX, toY] is Empty)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    board.Pieces.SetValue(new Empty(toX - LegalX, toY), new int[] { toX - LegalX, toY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    move = false; twoStep = false;
                    board.WTurn = !board.WTurn;
                }
                if (move) { throw new Exception("Failure of pawn move"); }
            }
            else { throw new Exception("Failure of pawn move"); }
            //Promotion
            if (PosX == 7 || PosX == 0)
            { board.Pieces.SetValue(new Queen(Player, PosX, PosY), new int[] { PosX, PosY }); }
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    class Rook : Piece
    {
        public new int LegalX = 7, LegalY = 7; public bool CanCastle = true;
        public Rook(Player player, int posX, int posY)
        {
            Player = player; PosX = posX; PosY = posY; Name = "Rook";
            if (player.IsW) { PieceImage = GetImage(285, 90, 220, 220); }
            else { PieceImage = GetImage(285, 365, 220, 220); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();
            for (int i = 0; i <= 7 - PosY; i++)
            {
                for (int ii = 0; ii <= 7 - PosX; ii++)
                {
                    if (i == 0 && ii == 0) { continue; }
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[PosX + ii, PosY + i] is Empty))
                    {
                        //If an enemy piece can move there
                        if (b.Pieces[PosX + ii, PosY + i].Player.IsW != Player.IsW) { boards.Add(Move(b, PosX + ii, PosY + i)); }
                        //If an ally piece can't move there
                        break;
                    }
                    //If it's still empty then just add it
                    if (b.Pieces[PosX + ii, PosY + i].Player.IsW = Player.IsW) { boards.Add(Move(b, PosY + i, PosX + ii)); }
                }
            }
            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            Board board = Serializer.DeepClone(b);
            int prex = PosX, prey = PosY;
            if (board.Pieces[toX, toY] is Empty || board.Pieces[toX, toY].Player.IsW != Player.IsW)
            {
                bool throughX = false, throughY = false;
                if (toX != PosX)
                {
                    for (int i = 1; i < Math.Abs(PosX - toX); i = Math.Abs(i) + 1)
                    {
                        if (toX < PosX) { i = i * -1; }
                        if (!(board.Pieces[PosX + i, toY] is Empty)) { throughX = true; break; }
                    }
                }
                if (toY != PosY)
                {
                    for (int i = 1; i < Math.Abs(PosY - toY); i = Math.Abs(i) + 1)
                    {
                        if (toY < PosY) { i = i * -1; }
                        if (!(board.Pieces[toX, PosY + i] is Empty)) { throughY = true; break; }
                    }
                }
                //Shift the legal + to outside of the loop for efficiency?
                if (!throughX && !throughY && ((toX <= LegalX && toY == PosY) || (toY <= LegalY && toX == PosX)))
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { toX, toY });
                    CanCastle = false;
                    board.WTurn = !board.WTurn;
                }
                else { throw new Exception("Failure of rook move"); }
                if (throughX || throughY) { throw new Exception("Rook can't move through pieces"); }
            }
            else { throw new Exception("Rook can't move on own pieces"); }
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    class Knight : Piece
    {
        public new int LegalX = 2, LegalY = 2;
        public Knight(Player player, int posX, int posY)
        {
            Player = player; PosX = posX; PosY = posY; Name = "Knight";
            if (player.IsW) { PieceImage = GetImage(700, 60, 230, 230); }
            else { PieceImage = GetImage(700, 350, 230, 230); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();
            for (int i = -2; i <= 2; i += 2)
            {
                for (int ii = -1; ii <= 1; ii += 2)
                {
                    //If the knight is moving off the board or capturing a like-kind piece, it's invalid
                    if (PosY + i > 7 || PosX + ii > 7  || PosY + i < 0 || PosX + ii < 0) { continue; }
                    if (!(b.Pieces[PosX + ii, PosY + i] is Empty) && b.Pieces[PosX + ii, PosY + i].Player.IsW == Player.IsW) { continue; }
                    boards.Add(Move(b, PosX + ii, PosY + i));
                }
            }
            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            Board board = Serializer.DeepClone(b);
            int prex = PosX, prey = PosY;
            if (board.Pieces[toX, toY] is Empty || board.Pieces[toX, toY].Player.IsW != Player.IsW)
            {
                bool L = false;
                if (Math.Abs(PosX - toX) + Math.Abs(PosY - toY) == 3) { L = true; }
                if (L && toX <= 7 && toY <= 7)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    board.WTurn = !board.WTurn;
                }
                else { throw new Exception("Failure of knight move"); }
            }
            else { throw new Exception("Knight can't move on own pieces"); }
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    class Bishop : Piece
    {
        //Should use legalx and legaly in the initializer to make move easier
        public new int LegalX = 7, LegalY = 7;
        public Bishop(Player player, int posX, int posY)
        {
            Player = player; PosX = posX; PosY = posY; Name = "Bishop";
            if (player.IsW) { PieceImage = GetImage(485, 55, 240, 240); }
            else { PieceImage = GetImage(485, 340, 240, 240); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();
            for (int i = -PosY; i < 7 - PosY; i++)
            {
                for (int ii = -PosX; ii < 7 - PosX; ii++)
                {
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[PosY + i, PosX + ii] is Empty)) { boards.Add(Move(b, PosY + i, PosX + ii)); break; }
                    //If it's empty just add it
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
                //Same process for the inverse proportional relationship of x/y
                for (int ii = -PosX; ii < 7 - PosX; ii++)
                {
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[PosY + i, PosX + ii] is Empty)) { boards.Add(Move(b, PosY + i, PosX - ii)); break; }
                    //If it's empty just add it
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
            }
            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            //Unecessary?
            for (int i = -7; i <= 7; i++)
            {
                if (PosX + i == toX && PosY + i == toY) { break; }
                if (PosX - i == toX && PosY + i == toY) { break; }
                if (i == 7)
                { throw new Exception("Failure of bishop move"); }
            }
            Board board = Serializer.DeepClone(b);
            int prex = PosX, prey = PosY;
            if (board.Pieces[toX, toY] is Empty || board.Pieces[toX, toY].Player.IsW != Player.IsW)
            {
                bool throughPiece = false;
                int xFactor = -1; int yFactor = -1;
                if (PosX < toX) { xFactor = 1; }
                if (PosY < toY) { yFactor = 1; }
                if ((Math.Abs(PosX - toX) + Math.Abs(PosY - toY)) % 2 == 0 && toX <= 7 && toY <= 7)
                {
                    for (int i = 1; i <= ((Math.Abs(PosX - toX) + Math.Abs(PosY - toY)) / 2) - 1; i = Math.Abs(i) + 1)
                    {
                        int ii = Serializer.DeepClone(i);
                        i = i * xFactor;
                        ii = ii * yFactor;
                        if (!(board.Pieces[PosX + i, PosY + ii] is Empty))
                        { throughPiece = true; }
                    }
                }
                else { throughPiece = true; throw new Exception("Failure of bishop move"); }
                if (throughPiece) { throw new Exception("Can't move through pieces"); }
                if ((Math.Abs(PosX - toX) + (PosY - toY)) % 2 == 0 && toX <= 7 && toY <= 7 && !throughPiece)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    board.WTurn = !board.WTurn;
                }
                else { throw new Exception("Failure of bishop move"); }
            }
            else { throw new Exception("Bishop can't move on own pieces"); }
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    class Queen : Piece
    {
        public new int LegalX = 7, LegalY = 7;
        public Queen(Player player, int posX, int posY)
        {
            Player = player; PosX = posX; PosY = posY; Name = "Queen";
            if (player.IsW) { PieceImage = GetImage(45, 645, 282, 282); }
            else { PieceImage = GetImage(469, 645, 282, 282); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();
            //I can't cast queen to another piece type, so I'm just copy-pasting the move-gen code from bishops and rooks

            //Rook
            for (int i = -PosY; i <= 7 - PosY; i++)
            {
                for (int ii = -PosX; ii <= 7 - PosX; ii++)
                {
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[i, ii] is Empty)) { boards.Add(Move(b, PosY + i, PosX + ii)); break; }
                    //If it's still empty then just add it
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
            }
            //Bishop
            for (int i = -PosY; i < 7 - PosY; i++)
            {
                for (int ii = -PosX; ii < 7 - PosX; ii++)
                {
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[PosY + i, PosX + ii] is Empty)) { boards.Add(Move(b, PosY + i, PosX + ii)); break; }
                    //If it's empty just add it
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
                //Same process for the inverse proportional relationship of x/y
                for (int ii = -PosX; ii < 7 - PosX; ii++)
                {
                    //If it reaches a piece, that is the furthest it can move
                    if (!(b.Pieces[PosY + i, PosX + ii] is Empty)) { boards.Add(Move(b, PosY + i, PosX - ii)); break; }
                    //If it's empty just add it
                    boards.Add(Move(b, PosY + i, PosX + ii));
                }
            }

            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            bool rMove = true; bool bMove = true;
            int prex = PosX, prey = PosY;
            Board board = Serializer.DeepClone(b);
            try
            {
                Rook Qrook = new Rook(Player, PosX, PosY);
                board = Qrook.Move(board, toX, toY);
            }
            catch
            {
                rMove = false;
                try
                {
                    Bishop Qbish = new Bishop(Player, PosX, PosY);
                    board = Qbish.Move(board, toX, toY);
                }
                catch { bMove = false; }
            }
            finally
            {
                board = Serializer.DeepClone(b);
                if (rMove)
                {
                    board.Pieces[PosX, PosY] = new Empty(PosX, PosY);
                    board.Pieces[toX, toY] = new Queen(Player, toX, toY);
                }
                else
                {
                    if (bMove)
                    {
                        board.Pieces[PosX, PosY] = new Empty(PosX, PosY);
                        board.Pieces[toX, toY] = new Queen(Player, toX, toY);
                    }
                    else { throw new Exception("QMove failure"); }
                }
            }
            board.WTurn = !board.WTurn;
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// No bugs known
    /// </summary>
    [Serializable]
    class King : Piece
    {
        public new int LegalX = 1, LegalY = 1; public bool CanCastle = true;
        public King(Player player, int posX, int posY)
        {
            Player = player; PosX = posX; PosY = posY; Name = "king"; LegalX = 1; LegalY = 1; CanCastle = true;
            if (player.IsW) { PieceImage = GetImage(250, 610, 290, 290); }
            else { PieceImage = GetImage(677, 610, 290, 290); }
        }
        public override List<Board> GenerateMoves(Board b)
        {
            var boards = new List<Board>();

            //If can castle check if it works
            if (CanCastle)
            {
                Board temp, temp2;
                try { temp = Move(b, PosY, PosX + 2); boards.Add(temp);  }
                catch { }
                try { temp2 = Move(b, PosY, PosX - 2); boards.Add(temp2);  }
                catch { }
            }

            for (int i = -1; i <= 1; i++)
            {
                for (int ii = -1; ii <= 1; ii++)
                {
                    //Skip if it goes off the board
                    if (PosX + ii > 7 || PosY + i > 7) { continue; }
                    //If the desired location is empty or an enemy piece, it is [psuedo] legal
                    if (b.Pieces[PosX, PosY] is Empty || b.Pieces[PosX, PosY].Player.IsW != Player.IsW) { boards.Add(Move(b, PosY + i, PosX + ii)); }
                }


                //*TODO*: check if it puts the king in check!


            }
            return boards;
        }
        public override Board Move(Board b, int toX, int toY)
        {
            Board board = Serializer.DeepClone(b);
            int prex = PosX, prey = PosY;
            if (board.Pieces[toX, toY] is Empty || board.Pieces[toX, toY].Player.IsW != Player.IsW)
            {
                //Castling
                if (toX == PosX && toY == 2 || toY == 6 && CanCastle && !b.Checks(Player.IsW))
                {
                    if (toY == 2)
                    {
                        if (board.Pieces[PosX, toY - 2] is Rook && ((Rook)board.Pieces[PosX, toY - 2]).CanCastle)
                        {
                            if (board.Pieces[PosX, toY + 1] is Empty && board.Pieces[PosX, toY - 1] is Empty && board.Pieces[PosX, toY] is Empty)
                            {
                                board.Pieces.SetValue(new Empty(PosX, toY - 2), new int[] { PosX, toY - 2 }); //Rook
                                board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY }); //King
                                board.Pieces.SetValue(new King(Player, PosX, toY), new int[] { PosX, toY }); //King
                                board.Pieces.SetValue(new Rook(Player, PosX, toY + 1), new int[] { PosX, toY + 1 }); //Rook
                                board.WTurn = !board.WTurn;
                                return board;
                            }
                            else
                            {
                                throw new Exception("Can't move through pieces");
                            }
                        }
                        else { throw new Exception("The rook can't castle"); }
                    }
                    if (toY == 6)
                    {
                        if (board.Pieces[PosX, toY + 1] is Rook && ((Rook)board.Pieces[PosX, toY + 1]).CanCastle)
                        {
                            if (board.Pieces[PosX, toY - 1] is Empty && board.Pieces[PosX, toY] is Empty)
                            {
                                board.Pieces.SetValue(new Empty(PosX, toY + 1), new int[] { PosX, toY + 1 }); //Rook
                                board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY }); //King
                                board.Pieces.SetValue(new King(Player, PosX, toY), new int[] { PosX, toY }); //King
                                board.Pieces.SetValue(new Rook(Player, PosX, toY - 1), new int[] { PosX, toY - 1 }); //Rook
                                board.WTurn = !board.WTurn;
                                return board;
                            }
                            else
                            {
                                throw new Exception("Can't move through pieces");
                            }
                        }
                        else { throw new Exception("The rook can't castle"); }
                    }
                }
                if ((toX == PosX + LegalX || toX == PosX - LegalX || toX == PosX) && (toY == PosY + LegalY || toY == PosY - LegalY || toY == PosY) && toX <= 7 && toY <= 7)
                {
                    board.Pieces.SetValue(new Empty(PosX, PosY), new int[] { PosX, PosY });
                    PosX = toX; PosY = toY;
                    board.Pieces.SetValue(this, new int[] { PosX, PosY });
                    CanCastle = false;
                    board.WTurn = !board.WTurn;
                }
                else { throw new Exception("Failure of king move"); }
            }
            else { throw new Exception("Failure of king move"); }
            if (board.Checks(Player.IsW)) { throw new Exception("Can't leave king in check"); }
            board.MoveNumber++;
            board.Moves += board.MoveNumber + " " + board.ChessNotation(this, prey, prex, toY, toX) + "\n";
            return board;
        }
    }
    /// <summary>
    /// Will f*** you up if you forget it DOES NOT HAVE A PLAYER! 
    /// Usually occurs when verifying the isW parameter
    /// </summary>
    [Serializable]
    class Empty : Piece
    {
        public Empty(int posX, int posY)
        {
            PosX = posX; PosY = posY; Name = "empty";
        }
        public override List<Board> GenerateMoves(Board b)
        {
            throw new Exception("Can't generate moves for nothing");
        }
        public override Board Move(Board board, int toX, int toY)
        {
            throw new Exception("Can't move nothing");
        }
    }
}
