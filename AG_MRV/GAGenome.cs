﻿using System;
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
        // list of all mappings from all virtual nodes to all virtual nodes
        public SWMapping[,] maps;

        // Static constant - mutation rate
        public static float MutationRate;
        public static float accMutationRate;
        public const int MAX_MUTATION_TRIES = 1000;

        /** Initializes a GAGenome */
        public GAGenome(int size, bool randomize)
        {
            maps = new SWMapping[GeneticAlgorithm.graph_virtual.n_count, 
                GeneticAlgorithm.graph_virtual.n_count];
            genes = new int[size];
            if (randomize)
            {
                // Repeat until it's a valid solution
                do
                {
                    for (int i = 0; i < size; i++)
                    {
                        /** RESTRICTION: 1 physical node -> 1 virtual node. */
                        /** RESTRICTION: CPU capacity */
                        int r;
                        do
                        {
                            r = GeneticAlgorithm.random_factory.Next(
                                GeneticAlgorithm.min_gene_value,
                                GeneticAlgorithm.max_gene_value + 1);
                        } while (containsGene(r));
                        genes[i] = r;
                    }
                } while (!updateValue());
            }
        }

        /** Initializes a GAGenome */
        public GAGenome(GAGenome genome)
        {
            CopyFrom(genome);
        }

        private void CopyFrom(GAGenome genome)
        {
            maps = new SWMapping[GeneticAlgorithm.graph_virtual.n_count,
                GeneticAlgorithm.graph_virtual.n_count];
            for (int i = 0; i < GeneticAlgorithm.graph_virtual.n_count; i++)
                for (int j = 0; j < GeneticAlgorithm.graph_virtual.n_count; j++)
                {
                    maps[i, j] = genome.maps[i, j];
                }
            genes = new int[genome.genes.Length];
            for (int i = 0; i < genes.Length; i++)
                genes[i] = genome.genes[i];
            this.value = genome.value;
        }

        /** Calculates a probability to mutate and mutates.
         * Mutation: chose randomly 2 genes and swap them */
        public void mutate(bool forceMutation = false)
        {
            // Calculates a random number and checks if it's in the rate
            if (forceMutation || GeneticAlgorithm.random_factory.NextDouble() < accMutationRate)
            {
                // Repeat until it's a valid solution
                int tries = 0;
                int g1 = 0;
                int g2 = 0;
                GAGenome tmp = new GAGenome(this);
                SWMapping tmp_map = new SWMapping(this.maps[g1, g2]);
                do 
                {
                    // if not the first try, reverse last mutation
                    if (tries > 0)
                    {
                        genes[g1] = genes[g1] + genes[g2];
                        genes[g2] = genes[g1] - genes[g2];
                        genes[g1] = genes[g1] - genes[g2];  
                        this.maps[g1, g2] = tmp_map;

                        // if past limit, just cancel and add accumulator
                        if (tries > MAX_MUTATION_TRIES)
                        {
                            accMutationRate += (0.5f * accMutationRate);
                            CopyFrom(tmp);
                            return;
                        }
                    }

                    tries++;
                    // Calculates other 2 different genes
                    g1 = GeneticAlgorithm.random_factory.Next(0, 
                        GeneticAlgorithm.genome_size);
                    g2 = g1;
                    while (g2 == g1)
                        g2 = GeneticAlgorithm.random_factory.Next(0, 
                            GeneticAlgorithm.genome_size);
                
                    // Swap them
                    genes[g1] = genes[g1] + genes[g2];
                    genes[g2] = genes[g1] - genes[g2];
                    genes[g1] = genes[g1] - genes[g2];
                    maps[g1, g2].paths = null;

                    tmp_map = new SWMapping(this.maps[g1, g2]);
                } while (!updateValue());
                accMutationRate = MutationRate;
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
            List<SWMapping> r_maps = new List<SWMapping>();
            for (int i = 0; i < GeneticAlgorithm.graph_virtual.edges.Length; i++)
            {
                // if already a valid map saved
                if (maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths != null &&
                    maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths.Count() > 0)
                {
                    value += GeneticAlgorithm.graph_virtual.edges[i].weight * 
                        maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].edge_count;
                    r_maps.Add(maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]]);
                    for (int j = 0; j < maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths.Count(); j++)
                        for (int k = 0; k < maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths[j].edges.Count(); k++)
                            edge_acc[maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths[j].edges[k]] += 
                                maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0], GeneticAlgorithm.graph_virtual.edges[i].nodes[1]].paths[j].cost;
                } else
                {
                    // find a new mapping
                    SWMapping m = GeneticAlgorithm.graph_substrate.findMapBetween(
                        genes[GeneticAlgorithm.graph_virtual.edges[i].nodes[0]],
                        genes[GeneticAlgorithm.graph_virtual.edges[i].nodes[1]],
                        ref edge_acc, GeneticAlgorithm.graph_virtual.edges[i].weight);
                    if (m.demand_to_go > 0)
                        return false;
                    r_maps.Add(m);
                    value += GeneticAlgorithm.graph_virtual.edges[i].weight * m.edge_count;
                }
            }
            if (r_maps.Count() > 0)
                for (int i = 0; i < GeneticAlgorithm.graph_virtual.edges.Length; i++)
                {
                    // Update the path between those 2 nodes
                    maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[0],
                        GeneticAlgorithm.graph_virtual.edges[i].nodes[1]] = r_maps[i];
                    maps[GeneticAlgorithm.graph_virtual.edges[i].nodes[1],
                        GeneticAlgorithm.graph_virtual.edges[i].nodes[0]] = r_maps[i];
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
