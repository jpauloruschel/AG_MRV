using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AG_MRV
{
    class SWNode
    {
        public int weight;
        public List<int> edges;

        public SWNode(int weight)
        {
            this.weight = weight;
            edges = new List<int>();
        }
    }

    struct SWEdge
    {
        public int weight;
        public int[] nodes;
    }

    class SwiftGraph
    {
        public SWNode[] nodes;
        public SWEdge[] edges;
        public int n_count, e_count;

        // Create with n nodes and e edges
        public SwiftGraph(int n, int e)
        {
            n_count = n;
            e_count = e;
            this.nodes = new SWNode[n];
            this.edges = new SWEdge[e];
        }

        // Create according to the String
        public SwiftGraph(String data)
        {
            // Split into lines
            String[] lines = data.Split('\n');
            String[] line_info;

            // Nodes and Edges count
            line_info = lines[0].Split(' ');
            //n_count = int.Parse(line_info[1]);
            //e_count = int.Parse(line_info[2]);
            this.nodes = new SWNode[int.Parse(line_info[1])];
            for (int i = 0; i < nodes.Count(); i++)
                nodes[i] = new SWNode(0);
            this.edges = new SWEdge[int.Parse(line_info[2])/2];

            // Read all other data
            for (int i = 1; i < lines.Length; i++)
            {
                line_info = lines[i].Split(' ');

                // Nodes
                if (line_info[0].StartsWith("V"))
                {
                    addNode(int.Parse(line_info[1]), int.Parse(line_info[2]));
                }
                else
                // Edges
                if (line_info[0].StartsWith("E"))
                {
                    addEdge(int.Parse(line_info[3]), int.Parse(line_info[1]), int.Parse(line_info[2]));
                }
            }

            // Log
            Console.WriteLine("Graph loaded\n  Nodes: " + n_count + "; Edges: " + e_count);
        }

        // Add a node
        public int addNode(int id, int weight)
        {
            nodes[id] = new SWNode(weight);
            n_count++;

            return id;
        }

        // Add an edge
        public int addEdge(int weight, params int[] nodes)
        {
            edges[e_count].weight = weight;
            edges[e_count].nodes = new int[nodes.Length];
            for (int i = 0; i < nodes.Length; i++) {
                edges[e_count].nodes[i] = nodes[i];
                this.nodes[nodes[i]].edges.Add(e_count);
            }
            e_count++;

            return e_count - 1;
        }

        // Total weight of the graph
        public int totalWeight()
        {
            int w = 0;
            foreach (SWNode n in nodes)
                w += n.weight;
            foreach (SWEdge e in edges)
                w += e.weight;
            return w;
        }
    }
}
