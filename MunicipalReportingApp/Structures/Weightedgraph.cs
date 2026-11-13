using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalReportingApp.Structures
{
    
    public sealed record GraphEdge(string From, string To, double Weight);

   
    public sealed record MstResult(List<GraphEdge> Edges, double TotalWeight);

    /// <summary>
    /// i needed help
    /// An undirected, weighted graph stored as an adjacency list (each node keeps a list of
    /// its neighbours). In this app the nodes are suburbs / service areas and the weights are
    /// road distances in km.
    ///
    /// Algorithms provided:
    ///  - Bfs      breadth-first traversal: areas in rings, nearest (fewest hops) first
    ///  - Dfs      depth-first traversal: follows one road as far as it goes before backing up
    ///  - Dijkstra shortest road distance from one area to all others (uses the binary heap)
    ///  - Kruskal  minimum spanning tree by sorting edges + union-find
    ///  - Prim     minimum spanning tree by growing outward with a heap
    /// </summary>
    public sealed class WeightedGraph
    {
        private readonly Dictionary<string, List<GraphEdge>> _adjacency =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly List<GraphEdge> _edges = new();

        public IReadOnlyList<string> Nodes => _adjacency.Keys.ToList();

        public IReadOnlyList<GraphEdge> Edges => _edges;

        public int NodeCount => _adjacency.Count;

        public bool Contains(string node) => _adjacency.ContainsKey(node);

        public void AddNode(string node)
        {
            if (!_adjacency.ContainsKey(node))
            {
                _adjacency[node] = new List<GraphEdge>();
            }
        }

       
        public void AddEdge(string a, string b, double weight)
        {
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight), "A distance cannot be negative.");

            AddNode(a);
            AddNode(b);

            _adjacency[a].Add(new GraphEdge(a, b, weight));
            _adjacency[b].Add(new GraphEdge(b, a, weight));
            _edges.Add(new GraphEdge(a, b, weight));
        }

        
        public List<GraphEdge> NeighboursOf(string node)
        {
            if (!_adjacency.TryGetValue(node, out var list)) return new List<GraphEdge>();
            return list.OrderBy(e => e.To, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // ------------------------------------------------------------------
        //  Traversals
        // ------------------------------------------------------------------

      
        public List<(string Node, int Hops)> Bfs(string start)
        {
            var result = new List<(string Node, int Hops)>();
            if (!_adjacency.ContainsKey(start)) return result;

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { start };
            var queue = new Queue<(string Node, int Hops)>();
            queue.Enqueue((start, 0));

            while (queue.Count > 0)
            {
                var (node, hops) = queue.Dequeue();
                result.Add((node, hops));

                foreach (var edge in NeighboursOf(node))
                {
                    if (visited.Add(edge.To))        
                    {
                        queue.Enqueue((edge.To, hops + 1));
                    }
                }
            }

            return result;
        }

     
        public List<string> Dfs(string start)
        {
            var order = new List<string>();
            if (!_adjacency.ContainsKey(start)) return order;

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<string>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                string node = stack.Pop();
                if (!visited.Add(node)) continue;

                order.Add(node);

                foreach (var edge in NeighboursOf(node).OrderByDescending(e => e.To, StringComparer.OrdinalIgnoreCase))
                {
                    if (!visited.Contains(edge.To)) stack.Push(edge.To);
                }
            }

            return order;
        }

        
        public bool IsConnected()
        {
            if (_adjacency.Count == 0) return true;
            return Bfs(_adjacency.Keys.First()).Count == _adjacency.Count;
        }

        // ------------------------------------------------------------------
        //  Shortest distances
        // ------------------------------------------------------------------

        /// <summary>
        /// Dijkstra's algorithm. Returns the shortest road distance (km) from the start to every
        /// reachable area. A min-heap always hands back the closest unfinished area next.
        /// </summary>
        public Dictionary<string, double> ShortestDistances(string start)
        {
            var distance = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            if (!_adjacency.ContainsKey(start)) return distance;

            var heap = new BinaryHeap<(string Node, double Dist)>(
                Comparer<(string Node, double Dist)>.Create((x, y) => x.Dist.CompareTo(y.Dist)));

            distance[start] = 0;
            heap.Push((start, 0));

            while (heap.TryPop(out var current))
            { 
                if (current.Dist > distance[current.Node]) continue;

                foreach (var edge in _adjacency[current.Node])
                {
                    double candidate = current.Dist + edge.Weight;

                    if (!distance.TryGetValue(edge.To, out double known) || candidate < known)
                    {
                        distance[edge.To] = candidate;
                        heap.Push((edge.To, candidate));
                    }
                }
            }

            return distance;
        }

        // ------------------------------------------------------------------
        //  Minimum spanning tree
        // ------------------------------------------------------------------

        /// <summary>
        /// Kruskal's algorithm: sort all roads by length, then take each road if it joins two
        /// areas that are not yet connected (union-find detects that quickly). Stops when every
        /// area is linked. For a disconnected graph the result is a spanning forest.
        /// </summary>
        public MstResult Kruskal()
        {
            var chosen = new List<GraphEdge>();
            var sets = new UnionFind(_adjacency.Keys);

            foreach (var edge in _edges.OrderBy(e => e.Weight).ThenBy(e => e.From).ThenBy(e => e.To))
            {
                if (sets.Union(edge.From, edge.To))
                {
                    chosen.Add(edge);
                }
            }

            return new MstResult(chosen, chosen.Sum(e => e.Weight));
        }

    
        public MstResult Prim(string start)
        {
            var chosen = new List<GraphEdge>();
            if (!_adjacency.ContainsKey(start)) return new MstResult(chosen, 0);

            var inTree = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { start };
            var heap = new BinaryHeap<GraphEdge>(Comparer<GraphEdge>.Create((x, y) => x.Weight.CompareTo(y.Weight)));

            foreach (var edge in _adjacency[start]) heap.Push(edge);

            while (heap.TryPop(out var edge))
            {
                if (inTree.Contains(edge.To)) continue;      

                inTree.Add(edge.To);
                chosen.Add(edge);

                foreach (var next in _adjacency[edge.To])
                {
                    if (!inTree.Contains(next.To)) heap.Push(next);
                }
            }

            return new MstResult(chosen, chosen.Sum(e => e.Weight));
        }

        // ------------------------------------------------------------------
        //  Union-find (disjoint sets), used by Kruskal
        // ------------------------------------------------------------------

        private sealed class UnionFind
        {
            private readonly Dictionary<string, string> _parent = new(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, int> _rank = new(StringComparer.OrdinalIgnoreCase);

            public UnionFind(IEnumerable<string> items)
            {
                foreach (var item in items)
                {
                    _parent[item] = item;
                    _rank[item] = 0;
                }
            }

            private string Find(string item)
            {
                while (!string.Equals(_parent[item], item, StringComparison.OrdinalIgnoreCase))
                {
                    _parent[item] = _parent[_parent[item]];   
                    item = _parent[item];
                }
                return item;
            }

            
            public bool Union(string a, string b)
            {
                string rootA = Find(a);
                string rootB = Find(b);
                if (string.Equals(rootA, rootB, StringComparison.OrdinalIgnoreCase)) return false;

                if (_rank[rootA] < _rank[rootB])
                {
                    _parent[rootA] = rootB;
                }
                else if (_rank[rootA] > _rank[rootB])
                {
                    _parent[rootB] = rootA;
                }
                else
                {
                    _parent[rootB] = rootA;
                    _rank[rootA]++;
                }
                return true;
            }
        }
    }
}
