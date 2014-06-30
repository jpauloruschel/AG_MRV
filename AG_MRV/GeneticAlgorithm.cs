using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AG_MRV
{
    /** This class holds all Genetic Algorithm methods.
     * Specific functions reggarding Population and Genomes
         are in their respective files. 
     * This class and all others in this application were designed
         to solve the Virtual Network Mapping onto Substrate Networks
         problem. */
    static class GeneticAlgorithm
    {
        // Variables used for the algorithm
        public static int min_gene_value, max_gene_value;
        public static int genome_size;
        public static int elite_child_count;
        public static SwiftGraph graph_substrate, graph_virtual;

        // A random number factory - used to prevent many Random Factories with same seed
        public static Random random_factory = new Random();


        /** Applies the Genetic Algorithm with the given parameters.
         */
        public static GAGenome geneticAlgorithm(SwiftGraph g_sub, SwiftGraph g_vir, int iterations, int populationSize, int elite_child_count, float mutation_rate)
        {
            // Graphs
            graph_substrate = g_sub;
            graph_virtual = g_vir;

            // Initializes variables
            random_factory = new Random();
            GeneticAlgorithm.min_gene_value = 0;
            GeneticAlgorithm.max_gene_value = graph_substrate.nodes.Length-1;
            GeneticAlgorithm.genome_size = graph_virtual.nodes.Length;
            GeneticAlgorithm.elite_child_count = elite_child_count;
            GAGenome.MutationRate = mutation_rate;
            GAGenome.accMutationRate = mutation_rate;

            // Initializes the population - random and viable genes
            GAPopulation.InitializePopulation(populationSize, true);

            // Applies all iterations
            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine("Iteration " + (i+1) + "...");
                GAPopulation.CreateNewGeneration();
                Console.WriteLine(" Best: generation:" + GAPopulation.getBestFromGeneration().value + "; ever:" + GAPopulation.getBestEver().value + "\n");
            }

            // Returns the best gene of the populatiosn
            return GAPopulation.getBestEver();
        }


        public static void WriteAndWait(int i)
        {
            Console.WriteLine(i);
            Console.ReadKey();
        }
        public static void WriteAndWait(string s)
        {
            Console.WriteLine(s);
            Console.ReadKey();
        }
        public static void Write(string s)
        {
            Console.WriteLine(s);
        }

        // ShellSort
        public struct ValueWeight
        {
            public int value;
            public int weight;
            public ValueWeight(int value, int weight)
            {
                this.value = value;
                this.weight = weight;
            }
        }
        public static void ShellSort(ref GAGenome[] Arjo)
        {
            int j;
            int Gap = Arjo.Length / 2;
            while (Gap > 0)
            {
                for (int i = 1 + Gap; i < Arjo.Length; i++)
                {
                    GAGenome tmp = new GAGenome(Arjo[i]);
                    j = i;
                    while (j - Gap >= 0)
                    {
                        if (Arjo[j - Gap].value > tmp.value)
                        {
                            Arjo[j] = new GAGenome(Arjo[j - Gap]);
                        }
                        else
                            break;
                        j = j - Gap;
                    }
                    Arjo[j] = new GAGenome(tmp);
                }
                Gap = Gap / 2;
            }
            // check
            if (Arjo[0].value > Arjo[1].value)
            {
                GAGenome tmp = new GAGenome(Arjo[0]);
                Arjo[0] = new GAGenome(Arjo[1]);
                Arjo[1] = tmp;
            }
        }
        public static void ShellSort(ref List<ValueWeight>Arjo)
        {
            int j;
            int Gap = Arjo.Count() / 2;
            while (Gap > 0)
            {
                for (int i = 1 + Gap; i < Arjo.Count(); i++)
                {
                    ValueWeight tmp = Arjo[i];
                    j = i;
                    while (j - Gap >= 0)
                    {
                        if (Arjo[j - Gap].weight > tmp.weight)
                        {
                            Arjo[j] = Arjo[j - Gap];
                        }
                        else
                            break;
                        j = j - Gap;
                    }
                    Arjo[j] = tmp;
                }
                Gap = Gap / 2;
            }
            // check
            if (Arjo[0].weight > Arjo[1].weight)
            {
                ValueWeight tmp = Arjo[0];
                Arjo[0] = Arjo[1];
                Arjo[1] = tmp;
            }
        }
    }
}
