using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace GeneticChess
{
    class NN
    {
        static int Resolution = 8;
        public int NumLayers = 3;
        public int INCount = 16;
        public int NCount = 8;
        public int ONCount = 1;
        public List<Layer> Layers { get; set; }
        public static double Momentum = .9;
        public static double LearningRate = .000146;
        public static double RMSDecay = .9;
        public static bool UseMomentum = false;
        public static bool UseRMSProp = true;
        public int MaxMoves = 50;
        public double TrialNum = 0;
        public double AvgGradient = 0;
        public double PercCorrect = 0;
        public double Error = 0;
        public Player player { get; set; }
        public int Guess { get; set; }

        public NN(double momentspeed, double learningrate, int numlayers, int incount, int hidcount, int outcount)
        {
            NumLayers = numlayers;
            INCount = incount; NCount = hidcount; ONCount = outcount; LearningRate = learningrate;
        }
        public NN() { }
        public bool Vs(NN competitor, bool isW)
        {
            Board Compitition = new Board(new Player(true), new Player(false), new Piece[8, 8], true).initBoard();
            int movecount = 0;
            //Compete until a victor is decided or movecount is exceeded
            while (!Compitition.WWin && !Compitition.BWin && movecount < MaxMoves)
            {
                //Ensure nothing goes wrong
                if (Compitition is null) { break; }

                if (isW == Compitition.WTurn) { Compitition = Move(Compitition, isW); }
                else { Compitition = competitor.Move(Compitition, !isW); }
                movecount++;
            }
            //Award win to the victor
            if ((isW && Compitition.WWin) || (!isW && Compitition.BWin)) { return true; }
            if ((!isW && Compitition.WWin) || (isW && Compitition.BWin)) { return false; }
            //If stale winner is non-competitor by default
            if (movecount < MaxMoves) { return true; }
            //If it broke without anything happening something went wrong
            throw new Exception("Unknown board state");
        }
        public Board Move(Board board, bool wTurn)
        {
            double maxscore = double.MinValue;
            Board bestBoard = null;
            List<Board> possibilities = board.GenMoves(wTurn);
            if (possibilities.Count == 0) { /*forfeit*/ }
            foreach (Board b in possibilities)
            {
                var score = Score(b);
                if (score > maxscore) { score = maxscore; bestBoard = b; }
            }
            return bestBoard;
        }
        public double Score(Board board)
        {
            var input = new double[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int ii = 0; ii < 8; ii++)
                {
                    //Set piece values equal to standard chess piece values
                    Piece p = board.Pieces[i, ii];
                    //Don't have to set empty piece = 0 b/c array initialization does it for me
                    if (p is Empty) { continue; }
                    if (p is Pawn) { input[i, ii] = 1; }
                    if (p is Knight || p is Bishop) { input[i, ii] = 3; }
                    if (p is Rook) { input[i, ii] = 5; }
                    if (p is Queen) { input[i, ii] = 9; }
                    if (p is King) { input[i, ii] = 99; }

                    //Set opposite color piece values to negative
                    if (p.Player.IsW != player.IsW) { input[i, ii] *= -1; }
                }
            }
            //Normalize everything
            input = ActivationFunctions.Normalize(input, 8, 8);
            return Run(input);
        }
        public NN Init()
        {
            Layers = new List<Layer>();
            int count = NCount;
            int lowercount = Resolution * Resolution;
            Random r = new Random();
            for (int i = 0; i < NumLayers; i++)
            {
                if (i != 0) { lowercount = Layers[i - 1].Length; count = NCount; }
                if (i == 0) { count = INCount; }
                if (i == NumLayers - 1) { count = ONCount; }
                Layers.Add(new Layer(count, lowercount));
                for (int j = 0; j < count; j++)
                {
                    for (int jj = 0; jj < lowercount; jj++)
                    {
                        //Weights initialized to a random number in range (-1/(2 * lowercount^2) - 1/(2 * lowercount^2))
                        //This is Lecun initialization
                        Layers[i].Weights[j, jj] = (r.NextDouble() > .5 ? -1 : 1) * r.NextDouble() * Math.Sqrt(3d / (double)(lowercount * lowercount));
                    }
                }
            }
            return this;
        }
        public double Run(double[,] image)
        {
            double[,] input = new double[Resolution, Resolution];
            //Deepclone?
            for (int i = 0; i < Resolution; i++) { for (int ii = 0; ii < Resolution; ii++) { input[i, ii] = image[i, ii]; } }

            //Forward
            for (int i = 0; i < Layers.Count; i++)
            {
                if (i == 0) { Layers[i].Calculate(input); continue; }
                Layers[i].Calculate(Layers[i - 1].Values, i == Layers.Count - 1);
            }
            return Layers[Layers.Count() - 1].Values[0];
        }
    }
    class ActivationFunctions
    {
        public static double Tanh(double number)
        {
            return (Math.Pow(Math.E, 2 * number) - 1) / (Math.Pow(Math.E, 2 * number) + 1);
        }
        public static double TanhDerriv(double number)
        {
            return (1 - number) * (1 + number);
        }
        public static double[] Normalize(double[] array)
        {
            double mean = 0;
            double stddev = 0;
            //Calc mean of data
            foreach (double d in array) { mean += d; }
            mean /= array.Length;
            //Calc std dev of data
            foreach (double d in array) { stddev += (d - mean) * (d - mean); }
            stddev /= array.Length;
            stddev = Math.Sqrt(stddev);
            //Prevent divide by zero b/c of sigma = 0
            if (stddev == 0) { stddev = .000001; }
            //Calc zscore
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (array[i] - mean) / stddev;
            }

            return array;
        }
        public static double[,] Normalize(double[,] array, int depth, int count)
        {
            double[] smallarray = new double[depth * count];
            int iterator = 0;
            for (int i = 0; i < depth; i++)
            {
                for (int ii = 0; ii < count; ii++)
                {
                    smallarray[iterator] = array[i, ii];
                    iterator++;
                }
            }
            smallarray = Normalize(smallarray);
            iterator = 0;
            for (int i = 0; i < depth; i++)
            {
                for (int ii = 0; ii < count; ii++)
                {
                    array[i, ii] = smallarray[iterator];
                    iterator++;
                }
            }
            return array;
        }
    }
    class Layer
    {
        public double[,] Weights { get; set; }
        public double[] Biases { get; set; }
        public double[] Values { get; set; }
        public double[] Errors { get; set; }
        public int Length { get; set; }
        public int InputLength { get; set; }
        public Layer(int length, int inputlength)
        {
            Length = length;
            InputLength = inputlength;
            Weights = new double[Length, InputLength];
            Biases = new double[Length];
        }

        public void Calculate(double[] input, bool output)
        {
            Values = new double[Length];
            for (int k = 0; k < Length; k++)
            {
                for (int j = 0; j < InputLength; j++)
                {
                    Values[k] += ((Weights[k, j]) * input[j]);
                }
                if (!output)
                {
                    Values[k] += Biases[k];
                    Values[k] = ActivationFunctions.Tanh(Values[k]);
                }
                else { Values[k] = Values[k]; }
            }
        }
        public void Calculate(double[,] input)
        {
            double[] input2 = new double[input.Length];
            int iterator = 0;
            foreach (double d in input) { input2[iterator] = d; iterator++; }
            Calculate(input2, false);
        }
    }
}