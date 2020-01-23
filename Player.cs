using System;

namespace GeneticChess
{
    [Serializable]
    public class Player
    {
        public bool IsW { get; set; }
        public Player(bool isw)
        {
            IsW = isw;
        }
    }
}
