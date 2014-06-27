using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AG_MRV
{
    static class GAPopulation
    {
        // Generation - All the Genomes (population itself)
        public static GAGenome[] generation;
        public static int m_length;

        /** Initializes GAPopulation with <size> genomes. */
        public static void InitializePopulation(int size, bool createGenes)
        {
            m_length = size;
            generation = new GAGenome[m_length];

            for (int i = 0; i < m_length; i++)
                generation[i] = new GAGenome(GeneticAlgorithm.genome_size, createGenes);
        }

        /** Creates a new generation */
        public static void CreateNewGeneration()
        {

        }

        /** Returns the best genome */
        public static GAGenome getBest()
        {
        }
    }
}
