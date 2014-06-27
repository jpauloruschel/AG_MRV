using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static SwiftGraph graph_substrate, graph_virtual;

        // A random number factory
        public static Random random_factory = new Random();


        /** Applies the Genetic Algorithm with the given parameters.
         */
        public static GAGenome geneticAlgorithm(SwiftGraph g_sub, SwiftGraph g_vir, int iterations, int populationSize, float mutation_rate)
        {
            // Graphs
            graph_substrate = g_sub;
            graph_virtual = g_vir;

            // Initializes variables
            GeneticAlgorithm.min_gene_value = 0;
            GeneticAlgorithm.max_gene_value = 0;
            GeneticAlgorithm.genome_size = 1;
            GAGenome.MutationRate = mutation_rate;

            // Initializes the population - random and viable genes
            GAPopulation.InitializePopulation(populationSize, true);

            // Applies all iterations
            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine("Iteration " + (i+1) + "...\n");
                GAPopulation.CreateNewGeneration();
            }

            // Returns the best gene of the populatiosn
            return GAPopulation.getBest();
        }

        /** Returns the value of a given Genome */
        public static int getValue(GAGenome genome)
        {
            return 0;
        }
    }
}
