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
        public int cost;                    // cost per edge
        public SWPath(int c_index)
        {
            this.c_index = c_index;
            edges = new List<int>();
            cost = 0;
        }
        public SWPath(int c_index, List<int> parentEdges, int newEdge)
        {
            this.c_index = c_index;
            edges = new List<int>(parentEdges);
            edges.Add(newEdge);
            cost = 0;
        }

        public SWPath(SWPath visiting, int cost)
        {
            this.c_index = visiting.c_index;
            this.edges = new List<int>(visiting.edges);
            this.cost = cost;
        }
    }

    // A Edge -> Edge Mapping
    struct SWMapping
    {
        public List<SWPath> paths;
        public int demand_to_go;
        public int edge_count;
        public SWMapping(int demand_to_go)
        {
            paths = new List<SWPath>();
            this.demand_to_go = demand_to_go;
            edge_count = 0;
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

        // Finds a Mapping between the nodes n1 and n2.
        //   RESTRICTION: sum of all virtual links hosted by a physical edge
        //      must not exceed its limit.
        //   This restriction is satisfied via edgeAccumulator and pathWeight
        //      edgeAccumulator is a list of the value already used of each edge
        //      pathWeight is the weight of the path
        // This is a naive algorithm that looks for the shortest path.
        // Returns a mapping (all paths that map)
        public SWMapping findMapBetween(int n1, int n2, ref int[] edgeAccumulator, int pathWeight)
        {
            SWMapping map = new SWMapping(pathWeight);

            List<SWPath> toVisit = new List<SWPath>();
            List<SWPath> newVisit = new List<SWPath>();
            List<SWPath> visited = new List<SWPath>();

            // Add to visit all nodes adjacent to n1
            for (int i = 0; i < nodes[n1].edges.Count(); i++)
                for (int j = 0; j < edges[nodes[n1].edges[i]].nodes.Length; j++ )
                    if (edges[nodes[n1].edges[i]].nodes[j] != n1)
                        newVisit.Add(new SWPath(edges[nodes[n1].edges[i]].nodes[j]));

            // Iterative Depth-search
            do {
                // Update list of paths to continue
                foreach (SWPath i in newVisit)
                    toVisit.Add(i);
                newVisit.Clear();

                // Foreach path to visit
                foreach (SWPath visiting in toVisit)
                {
                    // If the visiting index is equal to the desination, found a path
                    if (n2 == visiting.c_index)
                    {
                        // Calculates the demand that this path covers
                        int lowest = Int16.MaxValue;
                        for (int i = 0; i < visiting.edges.Count(); i++)
                            if (edges[visiting.edges[i]].weight < lowest)
                                lowest = edges[visiting.edges[i]].weight;
                        
                        int v = Math.Min(lowest, map.demand_to_go);
                        if (visiting.edges != null && visiting.edges.Count > 0)
                        {
                            map.paths.Add(new SWPath(visiting, v));
                            map.demand_to_go -= v;
                            map.edge_count += visiting.edges.Count();
                            for (int j = 0; j < visiting.edges.Count(); j++)
                                edgeAccumulator[visiting.edges[j]] += v;

                            // Check if done
                            if (map.demand_to_go == 0)
                                return map;

                            // Add to visit all nodes adjacent to n1
                            newVisit.Clear();
                            visited.Clear();
                            toVisit.Clear();
                            for (int i = 0; i < nodes[n1].edges.Count(); i++)
                                if (edgeAccumulator[nodes[n1].edges[i]] < edges[nodes[n1].edges[i]].weight)
                                    for (int j = 0; j < edges[nodes[n1].edges[i]].nodes.Length; j++)
                                        if (edges[nodes[n1].edges[i]].nodes[j] != n1)
                                            newVisit.Add(new SWPath(edges[nodes[n1].edges[i]].nodes[j]));
                            break;
                        }
                    }

                    // For each edge of the last vertex of the path
                    for (int edge = 0; edge < nodes[visiting.c_index].edges.Count; edge++)
                    {
                       // if (edgeAccumulator[nodes[visiting.c_index].edges[edge]] != 0)
                       // GeneticAlgorithm.Write(edgeAccumulator[nodes[visiting.c_index].edges[edge]] + " + " + pathWeight + " <= " + edges[nodes[visiting.c_index].edges[edge]].weight);
                        if (edgeAccumulator[nodes[visiting.c_index].edges[edge]] < edges[nodes[visiting.c_index].edges[edge]].weight) // if it can do at least 1
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
                                        //edgeAccumulator[nodes[visiting.c_index].edges[edge]] += pathWeight;
                                        //visited_edges.Add(nodes[visiting.c_index].edges[edge]);
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
            } while (newVisit.Count > 0);

            //for (int i = 0; i < visited_edges.Count(); i++)
            //    edgeAccumulator[visited_edges[i]] -= pathWeight;
            //GeneticAlgorithm.WriteAndWait("\nError finding path from node (" + n1 + ") to node (" + n2 + ")\n  Visited: " + visited.Count());
            return new SWMapping(1);
        }
        // Returns true if the passed Path is valid
        private bool IsAValidPath(SWPath path, ref int[] edgeAccumulator, int pathWeight)
        {
            // Check if using this path doesn't blow things up
            bool usable = true;
            for (int i = 0; i < path.edges.Count; i++)
                if ((edgeAccumulator[path.edges[i]] + pathWeight) > edges[path.edges[i]].weight)
                    return false;
            return usable;
        }
        private bool IsAValidPath(SWPath path, ref int[] edgeAccumulator, int pathWeight, out int badEdge)
        {
            // Check if using this path doesn't blow things up
            bool usable = true;
            badEdge = -1;
            for (int i = 0; i < path.edges.Count; i++)
                if (edgeAccumulator[path.edges[i]] + pathWeight > edges[path.edges[i]].weight)
                {
                    usable = false;
                    badEdge = path.edges[i];
                }
            return usable;
        }
        // Simple check for existence
        private static bool indexInPathList(List<SWPath> l, int index)
        {
            bool found = false;
            for (int i = 0; i < l.Count(); i++)
            {
                if (l[i].c_index == index)
                    return true;
            }
            return false;
            /*    Parallel.ForEach(l, walkNode =>
                    {
                        if (walkNode.c_index == index)
                            found = true;
                    });
            return found;*/
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
