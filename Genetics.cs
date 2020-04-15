using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticChess
{
    class Genetics
    {
        public int PopSize { get; set; }
        public double SurvivalProportion { get; set; }
        public double SurvivalRateChange { get; set; }
        public double MutationProportion { get; set; }
        NN[] NNs { get; set; }

        /// <summary>
        /// Initializes the genetics class
        /// </summary>
        /// <param name="load">Whether or not to load weights and biases from file</param>
        /// <param name="ps">Population size</param>
        /// <param name="sp">Likelyhood of survival between generations (per individual)</param>
        /// <param name="src">How much more likely the strong are to survive than the weak (exponentially)</param>
        /// <param name="mp">How likely mutations are to occur (gene by gene)</param>
        /// <param name="af">The currently active form (a lazy workaround to update the board)</param>
        public Genetics(bool load, int ps, double sp, double src, double mp, Form1 af)
        {
            NN.ActiveForm = af;
            PopSize = ps; SurvivalProportion = sp; SurvivalRateChange = src; MutationProportion = mp;
            //Initialize population
            if (!load) { NNs = GeneratePopulation(PopSize); }
            else { NNs = Load(); }
        }
        public void Save()
        {
            //Save population
            for (int i = 0; i < PopSize; i++)
            {
                IO.Write(NNs[i], i);
            }
        }
        public NN[] Load()
        {
            var nns = new NN[PopSize];
            for (int i = 0; i < PopSize; i++) 
            { 
                nns[i] = IO.Read(i);
            }
            return nns;
        }
        public void Evolve()
        {
            //Compute fitness
            NNs = Tournament();

            //Selection, propegation, mutation
            NNs = Propegate();

            Save();
        }
        //Quicksort!
        public NN[] Tournament()
        {
            //Sort NNs
            NNs = Quicksort.Quick(NNs, 0, NNs.Length - 1);
            return NNs;
        }
        public NN[] Propegate()
        {
            //Select
            //NNs are stored fitest to least fit in this list
            //(opposite of the array)
            var OldNNs = new List<NN>();
            double LikelyhoodOfSurvival = SurvivalProportion;
            var r = new Random();
            for (int i = NNs.Count(); i > 0; i++)
            {
                //Surive if randomly falls within proportion
                if (r.NextDouble() < LikelyhoodOfSurvival) { OldNNs.Add(NNs[i]); }
                //Chance of survival exponentially decreases with lower fitness
                SurvivalProportion *= SurvivalRateChange;
            }
            NNs = new NN[PopSize];
            //Crossover
            int numChildren = PopSize - OldNNs.Count();
            //Add old NNs to new array
            for (int i = 0; i < OldNNs.Count(); i++) { NNs[i] = OldNNs[i]; }
            int index = 0;
            while (numChildren > 0)
            {
                if (index > OldNNs.Count - 1) { index = 0; }
                var child = Crossover(OldNNs[index], OldNNs[index + 1]);
                //Mutate
                child = Mutation(child, MutationProportion);
                //Append child to array
                NNs[OldNNs.Count() - 1 + index] = child;
                numChildren--;
            }
            return NNs;
        }
        public NN Crossover(NN parent1, NN parent2)
        {
            var r = new Random();
            NN child = new NN();
            child.Init();
            for (int i = 0; i < parent1.Layers.Count; i++)
            {
                int crossoverPointX = r.Next(0, parent1.Layers[i].Weights.GetLength(0));
                int crossoverPointY = r.Next(0, parent1.Layers[i].Weights.GetLength(1));
                for (int ii = 0; ii < parent1.Layers[i].Weights.GetLength(0); ii++)
                {
                    for (int iii = 0; iii < parent1.Layers[i].Weights.GetLength(1); iii++)
                    {
                        double gene = parent1.Layers[i].Weights[ii, iii];
                        if (ii > crossoverPointX && iii > crossoverPointY)
                        {
                            gene = parent2.Layers[i].Weights[ii, iii];
                        }
                        child.Layers[i].Weights[ii, iii] = gene;
                    }
                }
            }
            return child;
        }
        public NN Mutation(NN patient, double probability)
        {
            var r = new Random();
            if (r.NextDouble() < probability) { return patient; }
            int mutationLayer = r.Next(0, patient.Layers.Count);
            int mutationPointX = r.Next(0, patient.Layers[mutationLayer].Weights.GetLength(0));
            int mutationPointY = r.Next(0, patient.Layers[mutationLayer].Weights.GetLength(1));
            //Random mutation with Lecun initialization
            patient.Layers[mutationLayer].Weights[mutationPointX, mutationPointY]
                = (r.NextDouble() > .5 ? -1 : 1) * r.NextDouble() *
                Math.Sqrt(3d / (double)(patient.Layers[mutationLayer].InputLength * patient.Layers[mutationLayer].InputLength));
            return patient;
        }
        public NN[] GeneratePopulation(int popsize)
        {
            var nns = new NN[popsize];
            for (int i = 0; i < popsize; i++)
            {
                var nn = new NN();
                nn.Init();
                nns[i] = nn;
            }
            Save();
            return nns;
        }
    }
}
