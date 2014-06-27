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

        // A random number factory
        public static Random random_factory = new Random();


        /** Applies the Genetic Algorithm with the given parameters.
         */
        public static GAGenome geneticAlgorithm(int populationSize, int iterations, int genome_size, int min_gene_value, int max_gene_value)
        {
            // Initializes variables
            GeneticAlgorithm.min_gene_value = min_gene_value;
            GeneticAlgorithm.max_gene_value = max_gene_value;
            GeneticAlgorithm.genome_size = genome_size;

            // Initializes the population - random and viable genes
            GAPopulation.InitializePopulation(populationSize, true);

            // Applies all iterations
            for (int i = 0; i < iterations; i++)
            {
                GAPopulation.CreateNewGeneration();
            }

            // Returns the best gene of the populatiosn
            return GAPopulation.getBest();
        }
    }
}
