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
    public class SWPath
    {
        public int c_index;                 // the current walk index - used for building
        public List<int> edges;             // list of all edges (by index)
        public int cost;                    // cost per edge
        public SWPath(int c_index)
        {
            this.c_index = c_index;
            edges = new List<int>();
            cost = int.MaxValue;
        }
        public SWPath(int c_index, List<int> parentEdges, int newEdge)
        {
            this.c_index = c_index;
            edges = new List<int>(parentEdges);
            edges.Add(newEdge);
            cost = int.MaxValue;
        }

        public SWPath(SWPath visiting, int cost)
        {
            this.c_index = visiting.c_index;
            this.edges = new List<int>(visiting.edges);
            this.cost = cost;
        }

        public SWPath(SWPath sWPath)
        {
            this.c_index = sWPath.c_index;
            this.cost = sWPath.cost;
            this.edges = new List<int>(sWPath.edges); 
        }
    }

    // A Edge -> Edge Mapping
    public struct SWMapping
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
        public SWMapping(SWMapping parent)
        {
            if (parent.paths != null)
            {
                paths = new List<SWPath>(parent.paths);
                this.demand_to_go = parent.demand_to_go;
                edge_count = parent.edge_count;
            }
            else
            {
                paths = null;
                demand_to_go = 0;
                edge_count = 0;
            }
        }
    }

    // A graph
    class SwiftGraph
    {
        public SWNode[] nodes;
        public SWEdge[] edges;
        public int[,] w_edge;
        public List<SWPath>[,] paths;        // [n_count][n_count] array of list of paths between each node
        public int n_count, e_count;

        // Create with n nodes and e edges
        public SwiftGraph(int n, int e)
        {
            n_count = n;
            e_count = e;
            this.nodes = new SWNode[n];
            this.edges = new SWEdge[e];
            paths = new List<SWPath>[n, n];
            for (int i = 0; i < n; i++ )
                for (int j = 0; j < n; j++)
                    paths[i,j] = new List<SWPath>();
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
            paths = new List<SWPath>[n_count, n_count];
            for (int i = 0; i < n_count; i++)
                for (int j = 0; j < n_count; j++)
                    paths[i, j] = new List<SWPath>();

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
        //      pathWeight is the weight of the edge trying to be mapped (virtual)
        // This is a naive algorithm that looks for the shortest path.
        // Returns a mapping (all paths that map)
        public SWMapping findMapBetween(int n1, int n2, ref int[] edgeAccumulator, int pathWeight)
        {
            SWMapping map = new SWMapping(pathWeight);

            // Look for a known valid path
            foreach (SWPath p in paths[n1, n2])
            {
                int v = Math.Min(ValidPath(p, ref edgeAccumulator, pathWeight), map.demand_to_go);
                if (v > 0)
                {
                    map.paths.Add(new SWPath(p, v));
                    map.demand_to_go -= v;
                    map.edge_count += p.edges.Count();
                    for (int j = 0; j < p.edges.Count(); j++)
                        edgeAccumulator[p.edges[j]] += v;
                }
            }

            // If known paths completed all the demands, return
            if (map.demand_to_go == 0)
                return map;

            // Search for a new path
            List<SWPath> toVisit = new List<SWPath>();
            List<SWPath> newVisit = new List<SWPath>();
            List<SWPath> visited = new List<SWPath>();
            float demandMultiplier = 0.1f;

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
                for (int p = 0; p < toVisit.Count(); p++)
                {
                    // If the visiting index is equal to the desination, found a path
                    if (n2 == toVisit[p].c_index)
                    {
                        // Calculates the demand that this path covers
                        int v = Math.Min(toVisit[p].cost, map.demand_to_go);
                        toVisit[p].cost = 0;

                        // If it's actually a valid path
                        if (toVisit[p].edges != null && toVisit[p].edges.Count > 0)
                        {
                            // Add this path to the mapping
                            map.paths.Add(new SWPath(toVisit[p], v));
                            map.demand_to_go -= v;
                            map.edge_count += toVisit[p].edges.Count();
                            for (int j = 0; j < toVisit[p].edges.Count(); j++)
                                edgeAccumulator[toVisit[p].edges[j]] += v;

                            // Add this path to the graph's list
                            paths[n1, n2].Add(new SWPath(toVisit[p]));

                            // Check if done
                            if (map.demand_to_go == 0)
                                return map;

                            // Add to visit all nodes adjacent to n1 (again)
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
                    for (int edge = 0; edge < nodes[toVisit[p].c_index].edges.Count; edge++)
                        // If this edge can store a link of at least 1
                        if (edgeAccumulator[nodes[toVisit[p].c_index].edges[edge]] - edges[nodes[toVisit[p].c_index].edges[edge]].weight < -(map.demand_to_go * demandMultiplier))
                            // For each vertex of this edge
                            for (int j = 0; j < edges[nodes[toVisit[p].c_index].edges[edge]].nodes.Length; j++)
                                // If not the original vertex
                                if (edges[nodes[toVisit[p].c_index].edges[edge]].nodes[j] != toVisit[p].c_index)
                                    // If not already visited, consider it to be visited next iteration
                                    if (!indexInPathList(newVisit, edges[nodes[toVisit[p].c_index].edges[edge]].nodes[j]) &&
                                        (!indexInPathList(visited, edges[nodes[toVisit[p].c_index].edges[edge]].nodes[j])))
                                    {
                                        // Update the max that this path will be able to hold (the smaller)
                                        toVisit[p].cost = Math.Min(edges[nodes[toVisit[p].c_index].edges[edge]].weight, toVisit[p].cost);
                                        newVisit.Add(new SWPath(
                                            edges[nodes[toVisit[p].c_index].edges[edge]].nodes[j], toVisit[p].edges, nodes[toVisit[p].c_index].edges[edge]));
                                    }
                }
                foreach (SWPath i in toVisit)
                    visited.Add(i);
                toVisit.Clear();

                // if no path found under these conditions, try with lesser demandMultiplier
                if (newVisit.Count() == 0 && demandMultiplier > 0f)
                {
                    demandMultiplier = 0f;
                    newVisit.Clear();
                    visited.Clear();
                    toVisit.Clear();
                    for (int i = 0; i < nodes[n1].edges.Count(); i++)
                        if (edgeAccumulator[nodes[n1].edges[i]] < edges[nodes[n1].edges[i]].weight)
                            for (int j = 0; j < edges[nodes[n1].edges[i]].nodes.Length; j++)
                                if (edges[nodes[n1].edges[i]].nodes[j] != n1)
                                    newVisit.Add(new SWPath(edges[nodes[n1].edges[i]].nodes[j]));
                }
            } while (newVisit.Count > 0);

            //GeneticAlgorithm.Write("\nError finding path from node (" + n1 + ") to node (" + n2 + ")\n  Visited: " + visited.Count());
            return map;
        }
        // Returns the value that this path can mantain for the given path
        private int ValidPath(SWPath path, ref int[] edgeAccumulator, int pathWeight)
        {
            // Check if using this path doesn't blow things up
            int value = pathWeight;
            for (int i = 0; i < path.edges.Count; i++)
                if (edgeAccumulator[path.edges[i]] < edges[path.edges[i]].weight)
                    value = Math.Min(value, edges[path.edges[i]].weight - edgeAccumulator[path.edges[i]]);
                else
                    return -1;
           return value;
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
           /* for (int i = 0; i < l.Count(); i++)
            {
                if (l[i].c_index == index)
                    return true;
            }
            return false;*/
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
