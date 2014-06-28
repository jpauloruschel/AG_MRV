using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AG_MRV
{
    // A node of the graph
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
    // An edge of the graph
    struct SWEdge
    {
        public int weight;
        public int[] nodes;
    }

    // A special type of Node. Used to store an edge path.
    struct SWPath
    {
        public int c_index;                 // the current walk index - used for building
        public List<int> edges;             // list of all edges (by index)
        public SWPath(int c_index)
        {
            this.c_index = c_index;
            edges = new List<int>();
        }
        public SWPath(int c_index, List<int> parentEdges, int newEdge)
        {
            this.c_index = c_index;
            edges = new List<int>(parentEdges);
            edges.Add(newEdge);
        }
    }

    // A graph
    class SwiftGraph
    {
        public SWNode[] nodes;
        public SWEdge[] edges;
        public int[,] w_edge;
        public SWPath[,] paths;        // [n_count][n_count] array of paths between each node
        public int n_count, e_count;

        // Create with n nodes and e edges
        public SwiftGraph(int n, int e)
        {
            n_count = n;
            e_count = e;
            this.nodes = new SWNode[n];
            this.edges = new SWEdge[e];
            paths = new SWPath[n, n];
            w_edge = new int[n, n];
        }

        // Create according to the String
        public SwiftGraph(String data)
        {
            // Split into lines
            String[] lines = data.Split('\n');
            String[] line_info;

            // Nodes and Edges
            line_info = lines[0].Split(' ');
            //n_count = int.Parse(line_info[1]);
            //e_count = int.Parse(line_info[2]);
            w_edge = new int[int.Parse(line_info[1]), int.Parse(line_info[1])];
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
                    //GeneticAlgorithm.WriteAndWait("addNode(" + int.Parse(line_info[1]) + ", " + int.Parse(line_info[2]) + ") : " + lines[i]);
                }
                else
                // Edges
                if (line_info[0].StartsWith("E"))
                {
                    addEdge(int.Parse(line_info[3]), int.Parse(line_info[1]), int.Parse(line_info[2]));
                    //GeneticAlgorithm.WriteAndWait("addEdge(" + int.Parse(line_info[3]) + ", " + int.Parse(line_info[1]) + ", " + int.Parse(line_info[2]) + "): " + lines[i]);
                }
            }

            // Initialize the paths
            paths = new SWPath[n_count, n_count];

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
            w_edge[nodes[0], nodes[1]] = weight;
            e_count++;

            return e_count - 1;
        }

        // Finds a path between the nodes n1 and n2.
        //   RESTRICTION: sum of all virtual links hosted by a physical edge
        //      must not exceed its limit.
        //   This restriction is satisfied via edgeAccumulator and maxEdgeWeight
        //      edgeAccumulator is a list of the value already used of each edge
        //      pathWeight is the weight of the path
        // This is a naive algorithm that looks for the shortest path.
        // Returns the path from n1 to n2.
        public SWPath findPathBetween(int n1, int n2, ref int[] edgeAccumulator, int pathWeight)
        {
            if (paths[n1, n2].edges != null)
                return paths[n1, n2];

            List<SWPath> toVisit = new List<SWPath>();
            List<SWPath> newVisit = new List<SWPath>();
            List<SWPath> visited = new List<SWPath>();
            newVisit.Add(new SWPath(n1));

            // Iterative Depth-search
            do {
                // Update list of paths to continue
                foreach (SWPath i in newVisit)
                    toVisit.Add(i);
                newVisit.Clear();

                // Foreach path to visit
                foreach (SWPath visiting in toVisit)
                {
                    // If the visiting index is equal to the desination, found the path!
                    if (n2 == visiting.c_index)
                    {
                        paths[n1, n2] = visiting;
                        paths[n2, n1] = visiting;
                        return visiting;
                    }
                    // For each edge of the last vertex of the path
                    for (int edge = 0; edge < nodes[visiting.c_index].edges.Count; edge++)
                    {
                        // For each vertex of this edge
                        for (int j = 0; j < edges[nodes[visiting.c_index].edges[edge]].nodes.Length; j++)
                        {
                            // If not the original vertex
                            if (edges[nodes[visiting.c_index].edges[edge]].nodes[j] != visiting.c_index)
                            {
                                // If not already visited, consider it to be visited next iteration
                                if (!indexInPathList(newVisit, edges[nodes[visiting.c_index].edges[edge]].nodes[j]) &&
                                    (!indexInPathList(visited, edges[nodes[visiting.c_index].edges[edge]].nodes[j])))
                                {
                                    //GeneticAlgorithm.WriteAndWait(edgeAccumulator[nodes[visiting.c_index].edges[edge]] + " + " + pathWeight + " <= " + edges[nodes[visiting.c_index].edges[edge]].weight);
                                    if (edgeAccumulator[nodes[visiting.c_index].edges[edge]] + pathWeight <= edges[nodes[visiting.c_index].edges[edge]].weight)
                                    {
                                        edgeAccumulator[nodes[visiting.c_index].edges[edge]] += pathWeight;
                                        newVisit.Add(new SWPath(
                                            edges[nodes[visiting.c_index].edges[edge]].nodes[j], visiting.edges, nodes[visiting.c_index].edges[edge]));
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (SWPath i in toVisit)
                    visited.Add(i);
                toVisit.Clear();
            } while ((newVisit.Count > 0));

            //GeneticAlgorithm.WriteAndWait("\nError finding path from node (" + n1 + ") to node (" + n2 + ")\n  Visited: " + visited.Count());
            return new SWPath();
        }
        // Simple check for existence
        private static bool indexInPathList(List<SWPath> l, int index)
        {
            bool found = false;
            Parallel.ForEach(l, walkNode =>
                {
                    if (walkNode.c_index == index)
                        found = true;
                });
            return found;
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
