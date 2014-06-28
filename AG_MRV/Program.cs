using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AG_MRV
{
    class Program
    {

        static void Main(string[] args)
        {
            // Read from Standard Input the problem
            //string input_substrate = Console.ReadLine();

            List<int> results = new List<int>();

            for (int i = 0; i < 4; i++ )
            {
                // Ler os arquivos da stdin
                StreamReader sr_substrate = new StreamReader("sub.gtt");
                string txt_substrate = sr_substrate.ReadToEnd();
                sr_substrate.Close();
                StreamReader sr_virtual = new StreamReader("vir.gtt");
                string txt_virtual = sr_virtual.ReadToEnd();
                sr_virtual.Close();

                Console.WriteLine("Loading Substrate Graph...");
                SwiftGraph graph_substrate = new SwiftGraph(txt_substrate);
                Console.WriteLine("\nLoading Virtual Graph...");
                SwiftGraph graph_virtual = new SwiftGraph(txt_virtual);

                // Ler essas informações da stdin
                int iterations = 7000;
                int population_size = 256;
                float mutation_rate = 0.15f;

                // Apply the Genetic Algorithm
                Console.WriteLine("\n\nApplying Genetic Algorithm...");
                GAGenome solution = GeneticAlgorithm.geneticAlgorithm(graph_substrate, graph_virtual, iterations, population_size, mutation_rate);

                // Found a solution. Log
                Console.WriteLine("\nSolution found. Value: " + solution.value);

                results.Add(solution.value);
                // Wait
                //Console.ReadKey();
            }

            Console.WriteLine("Done.\n");
            for (int i = 0; i < results.Count(); i++)
                Console.WriteLine(results[i]);

            Console.ReadLine();
        }
    }
}
