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
            bestSolution = null;

            Console.WriteLine("Creating Initial Population");

            for (int i = 0; i < m_length; i++)
            {
                Console.Write("  +" + (i + 1) + ": ");
                generation[i] = new GAGenome(GeneticAlgorithm.genome_size, createGenes);
                Program.current_iteration.mean_initial_population_value += generation[i].value;
                Console.WriteLine(generation[i].value);
            }
            Program.current_iteration.mean_initial_population_value = Program.current_iteration.mean_initial_population_value / (float)m_length;
        }

        /** Creates a new generation. Applies:
            * Selection
            * Crossover
            * Mutation */
        public static void CreateNewGeneration()
        {
            GeneticAlgorithm.ShellSort(ref generation);

            // Gets the parents
            GAGenome[] parents = rouletteSelection();

            // Reproduction (Crossover)
            GAGenome[] children = crossOver(parents);

            // Elite children (best)
            GAGenome[] eliteChildren = new GAGenome[GeneticAlgorithm.elite_child_count];
            for (int i = 0; i < GeneticAlgorithm.elite_child_count; i++)
                eliteChildren[i] = generation[i];
            //Console.Write("");
            // Joins both lists as new generation, with mutation on both
            generation = new GAGenome[children.Length + eliteChildren.Length];
            int it = 0;
            for (it = 0; it < eliteChildren.Length; it++)
            {
                generation[it] = eliteChildren[it];
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
         * by default, this selects population-elite. */
        private static GAGenome[] rouletteSelection()
        {
            // Initialize counters and result
            int new_parents_count = m_length - GeneticAlgorithm.elite_child_count;
            GAGenome[] result = new GAGenome[new_parents_count];
            List<int> chosen_ones = new List<int>();
            double[] probability = new double[m_length];
            int[] values = new int[m_length];
            double prob_sum = 0;
            int sum = 0;

            for (int i = 0; i < m_length; i++)
            {
                values[i] = generation[i].value;
                sum += values[i];
            }
            for (int i = 0; i < m_length; i++)
            {
                probability[i] = prob_sum + ((double)(sum-values[i]) / (double)sum);
                prob_sum += probability[i] - prob_sum;
                //Console.WriteLine("Fitness: " + values[i] + "; Probability: " + ((double)(sum - values[i]) / sum));
            }

            // Chose new_parents_count random genomes
            double random_f;
            for (int i = 0; i < new_parents_count; i++)
            {
                int j;
                do
                {
                    random_f = (GeneticAlgorithm.random_factory.NextDouble() * prob_sum);

                    for (j = 1; j < m_length; j++)
                        if (probability[j] > random_f) 
                            break;
                } while (chosen_ones.Contains(j-1));
                /*
                int j = 1;
                while (j < m_length)
                {
                    if (probability[j] > random_f)
                        break;
                    j++;
                }*/
               // Console.WriteLine("random_f: " + random_f + "; chose: " + (j-1));
                chosen_ones.Add(j-1);
                result[i] = generation[j - 1];
            }

            // Return the result
            return result;
        }

        /** Crossover (reproduction)
         * Algorithm: Modified 2-Point Crossover
         *    Also known as Ordered Crossover Operation
         * It generates the same number of parents*/
        private static GAGenome[] crossOver(GAGenome[] parents)
        {
            int child_count = parents.Length;
            GAGenome[] result = new GAGenome[child_count];
            int r_current = 0;

            for (int i = 0; i < child_count / 2; i++)
            {
                // New Children - copy of each parent
                GAGenome newG1, newG2;

                // Crossover
                do {
                    newG1 = new GAGenome(parents[i]);
                    newG2 = new GAGenome(parents[i + (child_count / 2)]);
                    // Select points
                    int p1 = (GeneticAlgorithm.random_factory.Next(0, GeneticAlgorithm.genome_size-1));
                    int p2 = (GeneticAlgorithm.random_factory.Next(p1, GeneticAlgorithm.genome_size));

                    // Change
                    for (int j = p1; j <= p2; j++)
                    {
                        if (!newG2.containsGene(parents[i].genes[j]))
                            newG2.genes[j] = parents[i].genes[j];
                        if (!newG1.containsGene(parents[i + (child_count / 2)].genes[j]))
                            newG1.genes[j] = parents[i + (child_count / 2)].genes[j];
                    }
                } while((!newG1.updateValue()) || (!newG2.updateValue()));

                // Adds to the list of children
                result[r_current] = newG1;
                result[r_current+1] = newG2;
                r_current += 2;
            }
            // If the last Genome is null, then it was an odd number.
            // Use a mutation of the best
            if (result[child_count - 1] == null)
            {
                result[child_count - 1] = new GAGenome(getBestEver());
                result[child_count - 1].mutate();
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
