using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace AG_MRV
{
    struct IterationResult
    {
        public float seconds;
        public int value;
        public float mean_initial_population_value;
        public IterationResult(float seconds, int value, float mean_initial_population_value)
        {
            this.seconds = seconds;
            this.value = value;
            this.mean_initial_population_value = mean_initial_population_value;
        }
        public IterationResult(IterationResult r)
        {
            this.seconds = r.seconds;
            this.value = r.value;
            this.mean_initial_population_value = r.mean_initial_population_value;
        }
    }
    struct InstanceResult
    {
        public float mean_seconds;
        public float mean_value;
        public int best_value, worst_value;
        public float mean_initial_population_value;
        public InstanceResult(List<IterationResult> results)
        {
            mean_seconds = 0;
            mean_value = 0;
            best_value = int.MaxValue;
            worst_value = int.MinValue;
            mean_initial_population_value = 0;
            
            for (int i = 0; i < results.Count(); i++)
            {
                mean_seconds += results[i].seconds;
                mean_value += results[i].value;
                mean_initial_population_value += results[i].mean_initial_population_value;
                if (results[i].value < best_value)
                    best_value = results[i].value;
                if (results[i].value > worst_value)
                    worst_value = results[i].value;
            }
            mean_seconds = mean_seconds / (float)results.Count();
            mean_value = mean_value / (float)results.Count();
            mean_initial_population_value = mean_initial_population_value / (float)results.Count();
        }
    }
    class Program
    {
        public static string path = @"results.txt";
        public static IterationResult current_iteration;

        static void Main(string[] args)
        {
            int[] i_population = { 40, 80, 160 };
            float[] i_mutation_rate = { 0.06f, 0.08f, 0.12f };

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("AG_MRV - Solutions\n");
                }
            }

            // Read from Standard Input the problem
            //string input_substrate = Console.ReadLine();

            List<IterationResult> iterationsResults;

            for (int pop = 0; pop < 3; pop++)
                //for (int mut = 0; mut < 3; mut++)
                {
                    int mut = 1;
                    iterationsResults = new List<IterationResult>();

                    // Ler essas informações da stdin
                    int iterations = 200;
                    int population_size = i_population[pop];
                    int elite_child_count = 2;
                    float mutation_rate = i_mutation_rate[mut];

                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine("");
                        sw.WriteLine("  INSTANCE");
                        sw.WriteLine("    Iterations: " + iterations);
                        sw.WriteLine("    Population Size: " + population_size);
                        sw.WriteLine("    Elite Childs: " + elite_child_count);
                        sw.WriteLine("    Mutation Rate: " + mutation_rate);
                    }

                    Console.WriteLine("  INSTANCE");
                    Console.WriteLine("    Iterations: " + iterations);
                    Console.WriteLine("    Population Size: " + population_size);
                    Console.WriteLine("    Elite Childs: " + elite_child_count);
                    Console.WriteLine("    Mutation Rate: " + mutation_rate);
                    Console.WriteLine("  -> Solving ...");

                    // 10 times
                    for (int it = 0; it < 10; it++)
                    {
                        current_iteration = new IterationResult(0,0,0);

                        Console.WriteLine("\n  -> It " + it);
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

                        Stopwatch watch = Stopwatch.StartNew();

                        // Apply the Genetic Algorithm
                        GAGenome solution = GeneticAlgorithm.geneticAlgorithm(graph_substrate, graph_virtual, iterations, population_size, elite_child_count, mutation_rate);

                        watch.Stop();
                        float seconds = watch.ElapsedMilliseconds / 1000;

                        // Found a solution. Log
                        Console.WriteLine("Solution found in " + seconds +" seconds. Value: " + solution.value + "\n");

                        current_iteration.value = solution.value;
                        current_iteration.seconds = seconds;

                        iterationsResults.Add(new IterationResult(current_iteration));
                    }

                    InstanceResult ir = new InstanceResult(iterationsResults);
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine("  -> Mean Value: " + ir.mean_value);
                        sw.WriteLine("  -> Avarage Seconds: " + ir.mean_seconds);
                        sw.WriteLine("  -> Best Value: " + ir.best_value);
                        sw.WriteLine("  -> Worst Value: " + ir.worst_value);
                        sw.WriteLine("  -> Avarage Initial Population Value: " + ir.mean_initial_population_value);
                    }
                }

            Console.WriteLine("Done.\n");

            Console.ReadLine();
        }
    }
}
