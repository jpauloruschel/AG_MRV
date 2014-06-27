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
            int iterations = 100;
            int population_size = 100;
            float mutation_rate = 0.1f;

            // Apply the Genetic Algorithm
            Console.WriteLine("\n\nApplying Genetic Algorithm...");
            GAGenome solution = GeneticAlgorithm.geneticAlgorithm(graph_substrate, graph_virtual, iterations, population_size, mutation_rate);

            // Found a solution. Log
            Console.WriteLine("\nSolution found. Value: " + GeneticAlgorithm.getValue(solution));

            // Wait
            Console.ReadKey();
        }
    }
}
