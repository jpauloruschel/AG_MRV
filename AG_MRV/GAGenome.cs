using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AG_MRV
{
    class GAGenome
    {
        int[] genes;

        public GAGenome(int size, bool randomize)
        {
            genes = new int[size];
            if (randomize)
                for (int i = 0; i < size; i++)
                    genes[i] = GeneticAlgorithm.random_factory.Next(GeneticAlgorithm.min_gene_value, GeneticAlgorithm.max_gene_value + 1);
        }

        public void mutate()
        {

        }
    }
}
