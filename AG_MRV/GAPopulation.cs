using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AG_MRV
{
    static class GAPopulation
    {
        // Generation - All the Genomes (population itself)
        public static GAGenome[] generation;
        public static int m_length;
        private static GAGenome bestSolution;

        /** Initializes GAPopulation with <size> genomes. */
        public static void InitializePopulation(int size, bool createGenes)
        {
            bestSolution = null;
            m_length = size;
            generation = new GAGenome[m_length];

            Console.WriteLine("Creating Initial Population");

            for (int i = 0; i < m_length; i++)
            {
                Console.Write("  +" + (i + 1) + ": ");
                generation[i] = new GAGenome(GeneticAlgorithm.genome_size, createGenes);
                Console.WriteLine(generation[i].value);
            }
        }

        /** Creates a new generation. Applies:
            * Selection
            * Crossover
            * Mutation */
        public static void CreateNewGeneration()
        {
            // Gets the parents
            GAGenome[] parents = rouletteSelection();

            // Reproduction (Crossover)
            GAGenome[] children = crossOver(parents);

            // Joins both lists as new generation, with mutation
            generation = new GAGenome[parents.Length + children.Length];
            int it = 0;
            for (it = 0; it < parents.Length; it++)
            {
                generation[it] = parents[it];
                generation[it].mutate();
            }
            for (int j = 0; j < children.Length; j++)
            {
                generation[it] = children[j];
                generation[it].mutate();
                it++;
            }

            // Gets the best and saves it
            GAGenome b = getBestFromGeneration();
            if (bestSolution == null || b.value < bestSolution.value)
                bestSolution = b;
        }

        /** Roulette Selection
         * by default, this selects 50% of the population */
        private static GAGenome[] rouletteSelection()
        {
            // Initialize counters and result
            GAGenome[] result = new GAGenome[m_length / 2];
            double[] probability = new double[m_length];
            int[] values = new int[m_length];
            double prob_sum = 0;
            int sum = 0;
            int r_current = 0;

            for (int i = 0; i < m_length; i++)
            {
                values[i] = generation[i].value;
                sum += values[i];
            }
            for (int i = 0; i < m_length; i++)
            {
                probability[i] = prob_sum + ((double)(sum-values[i]) / (double)sum);
                prob_sum += probability[i];
                //Console.WriteLine("Fitness: " + values[i] + "; Probability: " + ((double)(sum - values[i]) / sum));
            }
    
            // Chose m_length/2 random numbers between 0 and r_current
            double random_f;
            for (int i = 0; i < m_length / 2; i++)
            {
                random_f = (GeneticAlgorithm.random_factory.NextDouble());
                
                int j = 1;
                while (j < m_length)
                {
                    if (probability[j] <= random_f)
                        break;
                    j++;
                }
                result[r_current] = generation[j - 1];
                r_current++;
            }

            // Return the result
            return result;
        }

        /** Crossover (reproduction)
         * Algorithm: Modified 2-Point Crossover
         *    Also known as Ordered Crossover Operation */
        private static GAGenome[] crossOver(GAGenome[] parents)
        {
            GAGenome[] result = new GAGenome[m_length / 2];
            int r_current = 0;

            for (int i = 0; i < m_length / 4; i++ )
            {
                // New Children - copy of each parent
                GAGenome newG1, newG2;

                // Crossover
                do {
                    newG1 = new GAGenome(parents[i]);
                    newG2 = new GAGenome(parents[i + (m_length / 4)]);
                    // Select points
                    int p1 = (GeneticAlgorithm.random_factory.Next(0, GeneticAlgorithm.genome_size-1));
                    int p2 = (GeneticAlgorithm.random_factory.Next(p1, GeneticAlgorithm.genome_size));

                    // Change
                    for (int j = p1; j < p2; j++)
                    {
                        if (!newG2.containsGene(parents[i].genes[p1]))
                            newG2.genes[p1] = parents[i].genes[p1];
                        if (!newG1.containsGene(parents[i + (m_length / 4)].genes[p1]))
                            newG1.genes[p1] = parents[i + (m_length / 4)].genes[p1];
                    }
                } while((!newG1.updateValue()) || (!newG2.updateValue()));

                // Adds to the list of children
                result[r_current] = newG1;
                result[r_current+1] = newG2;
                r_current += 2;
            }

            return result;
        }

        /** Returns the best genome from this generation */
        public static GAGenome getBestFromGeneration()
        {
            GAGenome best_genome = generation[0];
            int best_value = int.MaxValue;
            foreach(GAGenome g in generation)
            {
                if (g.value < best_value)
                {
                    best_genome = g;
                    best_value = g.value;
                }
            }
            return best_genome;
        }

        /** Returns the best genome ever */
        public static GAGenome getBestEver()
        {
            return bestSolution;
        }
    }
}
