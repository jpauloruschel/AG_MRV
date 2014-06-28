using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AG_MRV
{
    class GAGenome
    {
        // genes
        public int[] genes;
        // The value (Z) of this solution
        public int value;
        // list of all paths from all virtual nodes to all virtual nodes
        public SWPath[,] paths;

        // Static constant - mutation rate
        public static float MutationRate;

        /** Initializes a GAGenome */
        public GAGenome(int size, bool randomize)
        {
            paths = new SWPath[GeneticAlgorithm.graph_virtual.n_count, 
                GeneticAlgorithm.graph_virtual.n_count];
            genes = new int[size];
            if (randomize)
                // Repeat until it's a valid solution
                do 
                {
                    //Console.WriteLine("tryin...");
                    for (int i = 0; i < size; i++)
                    {
                        /** RESTRICTION: 1 physical node -> 1 virtual node */
                        int r;
                        do 
                        {
                            r = GeneticAlgorithm.random_factory.Next(
                                GeneticAlgorithm.min_gene_value,
                                GeneticAlgorithm.max_gene_value + 1);
                        } while (containsGene(r));
                        genes[i] = r;
                    }
                } while(!updateValue());
        }

        /** Initializes a GAGenome */
        public GAGenome(GAGenome genome)
        {
            paths = new SWPath[GeneticAlgorithm.graph_virtual.n_count,
                GeneticAlgorithm.graph_virtual.n_count];
            genes = new int[genome.genes.Length];
            for (int i = 0; i < genes.Length; i++)
                genes[i] = genome.genes[i];
        }

        /** Calculates a probability to mutate and mutates.
         * Mutation: chose randomly 2 genes and swap them */
        public void mutate()
        {
            // Calculates a random number and checks if it's in the rate
            if (GeneticAlgorithm.random_factory.NextDouble() < MutationRate)
            {
                // Repeat until it's a valid solution
                do 
                {
                    // Calculates other 2 different genes
                    int g1 = GeneticAlgorithm.random_factory.Next(0, 
                        GeneticAlgorithm.genome_size);
                    int g2 = g1;
                    while (g2 == g1)
                        g2 = GeneticAlgorithm.random_factory.Next(0, 
                            GeneticAlgorithm.genome_size);
                
                    // Swap them
                    genes[g1] = genes[g1] + genes[g2];
                    genes[g2] = genes[g1] - genes[g2];
                    genes[g1] = genes[g1] - genes[g2];
                } while (!updateValue());
            }
        }
        
        /** Updates the Value. Returns false if it's a non-valid solution. */
        public bool updateValue()
        {
            value = 0;

            /** RESTRICTION: virtual nodes must only map to physical nodes that
             *   can host them. */
            for (int i = 0; i < genes.Length; i++)
            {
                if (GeneticAlgorithm.graph_substrate.nodes[genes[i]].weight <  
                    GeneticAlgorithm.graph_virtual.nodes[i].weight)
                    return false;
            }

            /** RESTRICTION: sum of all virtual links hosted by a physical edge
             *     must not exceed its limit. */
            int[] edge_acc = new int[GeneticAlgorithm.graph_substrate.e_count];
            for (int i = 0; i < GeneticAlgorithm.graph_virtual.edges.Length; i++)
            {
                SWPath p = GeneticAlgorithm.graph_substrate.findPathBetween(
                        genes[GeneticAlgorithm.graph_virtual.edges[i].nodes[0]],
                        genes[GeneticAlgorithm.graph_virtual.edges[i].nodes[1]],
                        ref edge_acc, GeneticAlgorithm.graph_virtual.edges[i].weight);
                if (p.edges == null || p.edges.Count == 0)
                    return false;
                // Update the path between those 2 nodes
                paths[GeneticAlgorithm.graph_virtual.edges[i].nodes[0],
                    GeneticAlgorithm.graph_virtual.edges[i].nodes[1]] = p;
                paths[GeneticAlgorithm.graph_virtual.edges[i].nodes[1],
                    GeneticAlgorithm.graph_virtual.edges[i].nodes[0]] = p;

                value += GeneticAlgorithm.graph_virtual.edges[i].weight * p.edges.Count();
            }

            return true;
        }

        /** Checks if the Genome contains a Gene */
        public bool containsGene(int gene)
        {
            int i = 0;
            while (i < genes.Length)
            {
                if (genes[i] == gene)
                    return true;
                i++;
            }
            return false;
        }
    }
}
