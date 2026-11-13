using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MunicipalReportingApp.Models;
using MunicipalReportingApp.Structures;

namespace MunicipalReportingApp.Services
{
    /// <summary>One filter set for <see cref="ServiceRequestRepository.Search"/>.</summary>
    public sealed record RequestQuery(
        string Text = "",
        RequestStatus? Status = null,
        string Category = "",
        int DaysBack = 0,
        bool TitleStartsWith = false,
        RequestSort Sort = RequestSort.Reference);

    public enum RequestSort
    {
        Reference,
        NewestFirst,
        OldestFirst,
        MostUrgent,
        TitleAtoZ
    }

    /// <summary>An area near a request, found by walking the graph.</summary>
    public sealed record NearbyArea(string Area, int Hops, double DistanceKm, int OpenRequests);

    /// <summary>
    /// Holds every service request and the data structures that organise them.
    /// There is one shared copy for the whole program (singleton).
    ///
    /// Which structure does which job:
    ///   AVL tree            index by request number  (track a request by its unique ID)
    ///   Binary search tree  index by title           (alphabetical list, "starts with" search)
    ///   Red-black tree      index by date submitted  (newest / oldest first, "last N days")
    ///   Binary heap         most urgent open requests first
    ///   General tree        department -> category organisation chart
    ///   Weighted graph      suburbs joined by roads  (nearby areas, crew route, shortest distance, MST)
    ///   HashSet             quick membership tests when combining filters
    /// </summary>
    public sealed class ServiceRequestRepository
    {
        public const string AllCategories = "All categories";
        public const string DepotArea = "Kimberley CBD";

        private static readonly Lazy<ServiceRequestRepository> _instance = new(() => new ServiceRequestRepository());
        public static ServiceRequestRepository Instance => _instance.Value;

        private readonly AvlTree<int, ServiceRequest> _byNumber = new();
        private readonly BinarySearchTree<(string Title, int Number), ServiceRequest> _byTitle = new(new TitleKeyComparer());
        private readonly RedBlackTree<(DateTime Submitted, int Number), ServiceRequest> _byDate = new();

        private readonly WeightedGraph _areas = new();
        private readonly GeneralTree<string> _departments = new("Sol Plaatje Municipality");
        private readonly Dictionary<string, string> _categoryToDepartment = new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<int> _importedIssueIds = new();
        private readonly string _storageFile;
        private int _nextNumber = 1001;

        private ServiceRequestRepository()
        {
            _storageFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service_requests.json");

            BuildDepartments();
            BuildAreaGraph();

            if (!LoadFromDisk())
            {
                SeedSampleData();
                SaveToDisk();
            }

            SyncWithReportedIssues();
        }

        // ------------------------------------------------------------------
        //  Read access for the screen
        // ------------------------------------------------------------------

        public int Count => _byNumber.Count;

        public WeightedGraph Areas => _areas;

        public GeneralTree<string> Departments => _departments;

        public AvlTree<int, ServiceRequest> NumberIndex => _byNumber;

        public BinarySearchTree<(string Title, int Number), ServiceRequest> TitleIndex => _byTitle;

        public RedBlackTree<(DateTime Submitted, int Number), ServiceRequest> DateIndex => _byDate;

        public IEnumerable<ServiceRequest> All => _byNumber.InOrder().Select(p => p.Value);

        /// <summary>The distinct categories, taken from the department tree (its leaf nodes).</summary>
        public List<string> GetCategories()
        {
            return _categoryToDepartment.Keys.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public string GetDepartmentFor(string category)
        {
            return _categoryToDepartment.TryGetValue(category, out var dept) ? dept : "General Services";
        }

        // ------------------------------------------------------------------
        //  Tracking by unique identifier (AVL tree)
        // ------------------------------------------------------------------

        /// <summary>
        /// Accepts "SR-1004", "sr1004", "SR 1004" or just "1004" and looks it up in the AVL tree.
        /// <paramref name="comparisons"/> is how many keys were compared to find (or rule out) it.
        /// </summary>
        public bool TryFindByReference(string text, out ServiceRequest? request, out int comparisons)
        {
            request = null;
            comparisons = 0;

            if (!TryParseReference(text, out int number)) return false;

            bool found = _byNumber.TryGetValue(number, out var value);
            comparisons = _byNumber.LastSearchComparisons;
            request = found ? value : null;
            return found;
        }

        /// <summary>Reads a request number out of text such as "SR-1004". Returns false if there is none.</summary>
        public static bool TryParseReference(string text, out int number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            string t = text.Trim();

            if (t.StartsWith("SR", StringComparison.OrdinalIgnoreCase)) t = t.Substring(2);
            t = t.TrimStart('-', ' ', '#');

            return int.TryParse(t, out number) && number > 0;
        }

        // ------------------------------------------------------------------
        //  Searching and ordering
        // ------------------------------------------------------------------

        /// <summary>
        /// Finds requests that match every filter, then puts them in the order asked for.
        ///
        ///  1. Text: a request number goes straight to the AVL tree. With "title starts with" ticked,
        ///     the binary search tree answers with a range query. Otherwise every request is checked
        ///     for the text in its title, street, area, category or description.
        ///  2. Status, category and date filters each produce a set of numbers; the sets are
        ///     combined with IntersectWith.
        ///  3. The survivors are listed in the order of whichever structure suits the sort:
        ///     AVL (reference), red-black tree (date), binary search tree (title) or heap (urgency).
        /// </summary>
        public List<ServiceRequest> Search(RequestQuery query)
        {
            // Start with every request number, then narrow it down.
            var matching = new HashSet<int>(_byNumber.InOrder().Select(p => p.Key));

            string text = (query.Text ?? string.Empty).Trim();

            if (text.Length > 0)
            {
                matching.IntersectWith(NumbersMatchingText(text, query.TitleStartsWith));
            }

            if (query.Status.HasValue)
            {
                matching.IntersectWith(_byNumber.InOrder()
                    .Where(p => p.Value.Status == query.Status.Value).Select(p => p.Key));
            }

            if (!string.IsNullOrEmpty(query.Category) && query.Category != AllCategories)
            {
                matching.IntersectWith(_byNumber.InOrder()
                    .Where(p => string.Equals(p.Value.Category, query.Category, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Key));
            }

            if (query.DaysBack > 0)
            {
                // Red-black tree range query: only the last N days are visited.
                var from = DateTime.Now.Date.AddDays(-(query.DaysBack - 1));
                var recent = _byDate.Range((from, int.MinValue), (DateTime.MaxValue, int.MaxValue));
                matching.IntersectWith(recent.Select(p => p.Value.Number));
            }

            return Order(matching, query.Sort);
        }

        private HashSet<int> NumbersMatchingText(string text, bool titleStartsWith)
        {
            var numbers = new HashSet<int>();

            // A request number such as "SR-1004": one AVL lookup.
            if (TryParseReference(text, out int wanted) && _byNumber.TryGetValue(wanted, out var exact))
            {
                numbers.Add(exact.Number);
                return numbers;
            }

            if (titleStartsWith)
            {
                // Titles that START with the text: a range query on the binary search tree.
                // Only the branches that can hold a match are visited.
                var prefixHits = _byTitle.Range((text, int.MinValue), (text + "\uffff", int.MaxValue));
                foreach (var pair in prefixHits) numbers.Add(pair.Value.Number);
                return numbers;
            }

            // Otherwise look for the text anywhere in the request.
            foreach (var pair in _byNumber.InOrder())
            {
                var r = pair.Value;
                if (Contains(r.Title, text) || Contains(r.Street, text) || Contains(r.Area, text)
                    || Contains(r.Description, text) || Contains(r.Category, text))
                {
                    numbers.Add(r.Number);
                }
            }

            return numbers;
        }

        private static bool Contains(string source, string text)
        {
            return source.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private List<ServiceRequest> Order(HashSet<int> numbers, RequestSort sort)
        {
            switch (sort)
            {
                case RequestSort.NewestFirst:
                    return _byDate.ReverseInOrder().Select(p => p.Value).Where(r => numbers.Contains(r.Number)).ToList();

                case RequestSort.OldestFirst:
                    return _byDate.InOrder().Select(p => p.Value).Where(r => numbers.Contains(r.Number)).ToList();

                case RequestSort.TitleAtoZ:
                    return _byTitle.InOrder().Select(p => p.Value).Where(r => numbers.Contains(r.Number)).ToList();

                case RequestSort.MostUrgent:
                    {
                        // Heap sort: push everything, then pop the most urgent one at a time.
                        var heap = new BinaryHeap<ServiceRequest>(
                            _byNumber.InOrder().Select(p => p.Value).Where(r => numbers.Contains(r.Number)),
                            new UrgencyComparer());

                        var ordered = new List<ServiceRequest>();
                        while (heap.TryPop(out var next)) ordered.Add(next);
                        return ordered;
                    }

                default:
                    return _byNumber.InOrder().Select(p => p.Value).Where(r => numbers.Contains(r.Number)).ToList();
            }
        }

        // ------------------------------------------------------------------
        //  Heap: the most urgent open requests
        // ------------------------------------------------------------------

        /// <summary>Builds a heap of every unresolved request, most urgent on top.</summary>
        public BinaryHeap<ServiceRequest> BuildUrgencyHeap()
        {
            return new BinaryHeap<ServiceRequest>(All.Where(r => r.IsOpen), new UrgencyComparer());
        }

        /// <summary>The <paramref name="count"/> most urgent unresolved requests.</summary>
        public List<ServiceRequest> GetMostUrgent(int count)
        {
            var heap = BuildUrgencyHeap();
            var top = new List<ServiceRequest>();

            while (top.Count < count && heap.TryPop(out var next))
            {
                top.Add(next);
            }

            return top;
        }

        // ------------------------------------------------------------------
        //  Graph: nearby areas, crew route, spanning tree
        // ------------------------------------------------------------------

        /// <summary>
        /// Walks outward from a request's area (breadth-first) so the nearest suburbs come first,
        /// attaching the road distance (Dijkstra) and how many open requests each suburb has.
        /// </summary>
        public List<NearbyArea> GetNearbyAreas(string area, int maxHops)
        {
            var distances = _areas.ShortestDistances(area);
            var openByArea = CountOpenByArea();
            var result = new List<NearbyArea>();

            foreach (var (node, hops) in _areas.Bfs(area))
            {
                if (hops == 0 || hops > maxHops) continue;

                distances.TryGetValue(node, out double km);
                openByArea.TryGetValue(node, out int open);
                result.Add(new NearbyArea(node, hops, km, open));
            }

            return result;
        }

        /// <summary>
        /// A crew route that starts at the depot and follows the roads depth-first through every area.
        /// Each stop shows how many open requests are waiting there.
        /// </summary>
        public List<(string Area, int OpenRequests)> GetCrewRoute(string depot)
        {
            var openByArea = CountOpenByArea();

            return _areas.Dfs(depot)
                .Select(a => (a, openByArea.TryGetValue(a, out int n) ? n : 0))
                .ToList();
        }

        /// <summary>The shortest road distance from the depot to an area, in km (-1 if it cannot be reached).</summary>
        public double DistanceFromDepot(string area)
        {
            var distances = _areas.ShortestDistances(DepotArea);
            return distances.TryGetValue(area, out double km) ? km : -1;
        }

        public MstResult GetMinimumRoadNetwork() => _areas.Kruskal();

        public MstResult GetMinimumRoadNetworkPrim() => _areas.Prim(DepotArea);

        public Dictionary<string, int> CountOpenByArea()
        {
            return All.Where(r => r.IsOpen)
                      .GroupBy(r => r.Area, StringComparer.OrdinalIgnoreCase)
                      .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, int> CountOpenByCategory()
        {
            return All.Where(r => r.IsOpen)
                      .GroupBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
                      .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        //  Changing a request
        // ------------------------------------------------------------------

        /// <summary>
        /// Moves a request to its next stage (Submitted, Assigned, In progress, Resolved) and records it
        /// in the request's history. Used by the screen's demo button. Returns false if it is already resolved.
        /// </summary>
        public bool AdvanceStatus(int number)
        {
            if (!_byNumber.TryGetValue(number, out var request)) return false;
            if (request.Status == RequestStatus.Resolved) return false;

            request.Status = (RequestStatus)((int)request.Status + 1);
            request.LastUpdated = DateTime.Now;

            if (request.Status == RequestStatus.Assigned && string.IsNullOrEmpty(request.AssignedTeam))
            {
                request.AssignedTeam = GetDepartmentFor(request.Category) + " team";
            }

            request.History.Add(new StatusUpdate
            {
                Status = request.Status,
                When = DateTime.Now,
                Note = DescribeStage(request.Status, request.AssignedTeam)
            });

            SaveToDisk();
            return true;
        }

        private static string DescribeStage(RequestStatus status, string team)
        {
            return status switch
            {
                RequestStatus.Submitted => "Request received.",
                RequestStatus.Assigned => string.IsNullOrEmpty(team) ? "Assigned to a team." : "Assigned to the " + team + ".",
                RequestStatus.InProgress => "Work has started on site.",
                RequestStatus.Resolved => "Work completed and the request closed.",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Turns every issue citizens reported on the Report Issues screen (Part 1) into a service
        /// request, once. A HashSet remembers which issues have already been imported.
        /// </summary>
        public int SyncWithReportedIssues()
        {
            int added = 0;

            foreach (var issue in IssueRepository.Instance.Issues)
            {
                if (_importedIssueIds.Contains(issue.Id)) continue;

                var request = new ServiceRequest
                {
                    Number = _nextNumber++,
                    Title = issue.Category + " - " + issue.Location,
                    Category = _categoryToDepartment.ContainsKey(issue.Category) ? issue.Category : "Other",
                    Area = GuessArea(issue.Location + " " + issue.StreetName),
                    Street = issue.StreetName,
                    Description = issue.Description,
                    Priority = DefaultPriorityFor(issue.Category),
                    Status = RequestStatus.Submitted,
                    Submitted = issue.DateReported,
                    LastUpdated = issue.DateReported,
                    SourceIssueId = issue.Id
                };

                request.History.Add(new StatusUpdate
                {
                    Status = RequestStatus.Submitted,
                    When = issue.DateReported,
                    Note = "Reported through the Report Issues screen."
                });

                AddToIndexes(request);
                added++;
            }

            if (added > 0) SaveToDisk();
            return added;
        }

        private string GuessArea(string text)
        {
            foreach (var area in _areas.Nodes)
            {
                if (text.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0) return area;
            }
            return DepotArea;
        }

        private static RequestPriority DefaultPriorityFor(string category)
        {
            switch (category)
            {
                case "Public Safety": return RequestPriority.Critical;
                case "Water & Utilities": return RequestPriority.High;
                case "Street Lighting": return RequestPriority.High;
                case "Noise Complaint": return RequestPriority.Low;
                default: return RequestPriority.Medium;
            }
        }

        // ------------------------------------------------------------------
        //  Structure report (shown on the "Data structures" tab)
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds a plain-text summary of how each structure is doing, including a live comparison of a
        /// plain binary search tree against the AVL and red-black trees when request numbers arrive in order.
        /// </summary>
        public List<string> BuildStructureReport(ServiceRequest? selected)
        {
            var lines = new List<string>();
            int n = Count;

            lines.Add($"Requests stored: {n}");
            lines.Add("");

            lines.Add("AVL tree - index by request number");
            lines.Add($"   height {_byNumber.Height()}, rotations so far {_byNumber.Rotations}, balanced: {_byNumber.IsBalanced()}");
            if (selected != null)
            {
                _byNumber.TryGetValue(selected.Number, out _);
                lines.Add($"   finding {selected.Reference} took {_byNumber.LastSearchComparisons} comparison(s) (a plain list may need up to {n})");
            }
            lines.Add("");

            lines.Add("Binary search tree - index by title");
            lines.Add($"   height {_byTitle.Height()} for {_byTitle.Count} titles (titles arrive in mixed order, so it stays fairly even)");
            lines.Add("");

            lines.Add("Red-black tree - index by date submitted");
            lines.Add($"   height {_byDate.Height()}, rotations {_byDate.Rotations}, colour flips {_byDate.ColourFlips}, all rules hold: {_byDate.IsValid()}");
            lines.Add("");

            // Same keys, inserted in ascending order: the worst case for an unbalanced tree.
            var plain = new BinarySearchTree<int, int>();
            var avl = new AvlTree<int, int>();
            var red = new RedBlackTree<int, int>();
            foreach (var pair in _byNumber.InOrder())
            {
                plain.Insert(pair.Key, pair.Key);
                avl.Insert(pair.Key, pair.Key);
                red.Insert(pair.Key, pair.Key);
            }

            lines.Add($"Why balancing matters (the same {n} request numbers, inserted in order):");
            lines.Add($"   plain binary search tree  height {plain.Height()}");
            lines.Add($"   AVL tree                  height {avl.Height()}");
            lines.Add($"   red-black tree            height {red.Height()}");
            lines.Add("");

            var firstKeys = _byNumber.InOrder().Take(7).Select(p => p.Key).ToList();
            var small = new AvlTree<int, int>();
            foreach (var k in firstKeys) small.Insert(k, k);

            lines.Add("Traversals of an AVL tree built from the first " + firstKeys.Count + " request numbers:");
            lines.Add("   in-order    " + string.Join(", ", small.InOrder().Select(p => p.Key)));
            lines.Add("   pre-order   " + string.Join(", ", small.PreOrder().Select(p => p.Key)));
            lines.Add("   post-order  " + string.Join(", ", small.PostOrder().Select(p => p.Key)));
            lines.Add("   level-order " + string.Join(", ", small.LevelOrder().Select(p => p.Key)));
            lines.Add("");

            var heap = BuildUrgencyHeap();
            var array = heap.ToArray();
            lines.Add($"Binary heap - {heap.Count} open requests; the most urgent is always at the top:");
            if (heap.Count > 0)
            {
                lines.Add("   top: " + heap.Peek().Reference + " (" + heap.Peek().Priority + ")");
                lines.Add("   first array slots: " + string.Join(", ", array.Take(6).Select(r => r.Reference)));
            }
            lines.Add("");

            lines.Add($"Area graph - {_areas.NodeCount} areas, {_areas.Edges.Count} roads, connected: {_areas.IsConnected()}");
            var kruskal = GetMinimumRoadNetwork();
            var prim = GetMinimumRoadNetworkPrim();
            lines.Add($"   spanning tree: Kruskal {kruskal.TotalWeight:0.0} km, Prim {prim.TotalWeight:0.0} km (the two always agree)");

            lines.Add("");
            lines.Add($"Department tree - {_departments.Count()} nodes, {_departments.Height()} levels");

            return lines;
        }

        // ------------------------------------------------------------------
        //  Building the structures
        // ------------------------------------------------------------------

        private void AddToIndexes(ServiceRequest request)
        {
            _byNumber.Insert(request.Number, request);
            _byTitle.Insert((request.Title, request.Number), request);
            _byDate.Insert((request.Submitted, request.Number), request);

            if (request.SourceIssueId > 0) _importedIssueIds.Add(request.SourceIssueId);
            if (request.Number >= _nextNumber) _nextNumber = request.Number + 1;
        }

        private void BuildDepartments()
        {
            AddDepartment("Roads & Transport", "Roads & Potholes");
            AddDepartment("Water & Sanitation", "Water & Utilities", "Sanitation & Waste");
            AddDepartment("Electricity", "Street Lighting");
            AddDepartment("Community Services", "Parks & Recreation", "Noise Complaint");
            AddDepartment("Public Safety", "Public Safety");
            AddDepartment("General Services", "Other");
        }

        private void AddDepartment(string department, params string[] categories)
        {
            var node = _departments.Root.AddChild(department);

            foreach (var category in categories)
            {
                node.AddChild(category);
                _categoryToDepartment[category] = department;
            }
        }

        
        private void BuildAreaGraph()
        {
            _areas.AddEdge("Kimberley CBD", "Beaconsfield", 3.5);
            _areas.AddEdge("Kimberley CBD", "Kimdustria", 4.0);
            _areas.AddEdge("Kimberley CBD", "Hadison Park", 3.0);
            _areas.AddEdge("Kimberley CBD", "Greenpoint", 4.5);
            _areas.AddEdge("Kimberley CBD", "Galeshewe", 6.0);
            _areas.AddEdge("Kimberley CBD", "Roodepan", 6.5);
            _areas.AddEdge("Beaconsfield", "Homevale", 2.5);
            _areas.AddEdge("Beaconsfield", "Roodepan", 5.0);
            _areas.AddEdge("Galeshewe", "Roodepan", 4.0);
            _areas.AddEdge("Galeshewe", "Homevale", 3.5);
            _areas.AddEdge("Homevale", "Kimdustria", 3.0);
            _areas.AddEdge("Kimdustria", "Verwoerdpark", 3.5);
            _areas.AddEdge("Hadison Park", "Verwoerdpark", 2.5);
            _areas.AddEdge("Hadison Park", "Carters Glen", 3.0);
            _areas.AddEdge("Verwoerdpark", "Carters Glen", 2.0);
            _areas.AddEdge("Hadison Park", "Greenpoint", 3.5);
            _areas.AddEdge("Greenpoint", "Carters Glen", 5.5);
        }

        // ------------------------------------------------------------------
        //  Seed data and saving
        // ------------------------------------------------------------------

        private void SeedSampleData()
        {
            
            var seed = new (string Title, string Category, string Area, string Street, string Description,
                            RequestPriority Priority, RequestStatus Status, int DaysAgo)[]
            {
                ("Large pothole near the traffic circle", "Roads & Potholes", "Kimberley CBD", "Du Toitspan Road", "A deep pothole is damaging car tyres at the traffic circle.", RequestPriority.High, RequestStatus.InProgress, 9),
                ("Burst water pipe flooding the street", "Water & Utilities", "Galeshewe", "Ntlakohlaki Street", "Water has been running into the road since early morning.", RequestPriority.Critical, RequestStatus.Assigned, 2),
                ("Streetlight out for two weeks", "Street Lighting", "Hadison Park", "Hadison Park Drive", "The lamp outside number 12 is dark and the corner feels unsafe.", RequestPriority.High, RequestStatus.Submitted, 14),
                ("Rubbish not collected this week", "Sanitation & Waste", "Roodepan", "Voortrekker Street", "Refuse bags are piling up and attracting dogs.", RequestPriority.Medium, RequestStatus.Submitted, 6),
                ("Broken playground swings", "Parks & Recreation", "Verwoerdpark", "Park Road", "Two swings have snapped chains and children are at risk.", RequestPriority.Medium, RequestStatus.Assigned, 11),
                ("Loud music from a tavern at night", "Noise Complaint", "Beaconsfield", "Beaconsfield Main Road", "Music continues past midnight most weekends.", RequestPriority.Low, RequestStatus.Resolved, 25),
                ("Open manhole cover on the pavement", "Public Safety", "Kimdustria", "Industrial Road", "A missing cover is a danger to pedestrians and cyclists.", RequestPriority.Critical, RequestStatus.InProgress, 3),
                ("Sewage overflowing into yard", "Sanitation & Waste", "Homevale", "Homevale Crescent", "The drain at the corner has been blocked for days.", RequestPriority.High, RequestStatus.Assigned, 5),
                ("Pothole cluster after the rains", "Roads & Potholes", "Greenpoint", "Greenpoint Avenue", "Several potholes have opened along a 50 metre stretch.", RequestPriority.Medium, RequestStatus.Submitted, 4),
                ("Traffic light not working", "Roads & Potholes", "Kimberley CBD", "Jones Street", "The signal is stuck on red at the busy intersection.", RequestPriority.Critical, RequestStatus.Resolved, 18),
                ("No water since yesterday", "Water & Utilities", "Carters Glen", "Carters Glen Road", "The whole street has had no water supply for 24 hours.", RequestPriority.High, RequestStatus.InProgress, 1),
                ("Overgrown park and long grass", "Parks & Recreation", "Galeshewe", "Mapoteng Street", "Grass is above knee height and is a fire risk.", RequestPriority.Low, RequestStatus.Submitted, 20),
                ("Streetlights flickering on the main road", "Street Lighting", "Beaconsfield", "Beaconsfield Main Road", "Four lamps flicker all night.", RequestPriority.Medium, RequestStatus.InProgress, 8),
                ("Illegal dumping on empty plot", "Sanitation & Waste", "Kimdustria", "Dump Road", "Building rubble and tyres have been dumped on the plot.", RequestPriority.Medium, RequestStatus.Resolved, 30),
                ("Damaged stop sign at school", "Public Safety", "Verwoerdpark", "School Street", "The sign was knocked down and learners cross here.", RequestPriority.High, RequestStatus.Assigned, 7),
                ("Water leak at the community hall", "Water & Utilities", "Roodepan", "Hall Street", "A tap is leaking and the floor is wet.", RequestPriority.Low, RequestStatus.Submitted, 12),
                ("Stray animals on the road", "Other", "Hadison Park", "Hadison Park Drive", "A pack of stray dogs is near the school gate.", RequestPriority.Medium, RequestStatus.Submitted, 2),
                ("Road sign missing at the junction", "Roads & Potholes", "Homevale", "Homevale Road", "The street name sign was stolen.", RequestPriority.Low, RequestStatus.Resolved, 22),
                ("Dangerous cable hanging low", "Street Lighting", "Greenpoint", "Greenpoint Avenue", "A cable has dropped to head height after the storm.", RequestPriority.Critical, RequestStatus.Assigned, 1),
                ("Blocked storm drain", "Water & Utilities", "Kimberley CBD", "Bultfontein Road", "Leaves block the drain and the road floods when it rains.", RequestPriority.Medium, RequestStatus.InProgress, 10),
                ("Noisy construction after hours", "Noise Complaint", "Carters Glen", "Carters Glen Road", "Building work continues until late in the evening.", RequestPriority.Low, RequestStatus.Submitted, 3),
                ("Fallen tree blocking the pavement", "Parks & Recreation", "Kimberley CBD", "Lyndhurst Road", "A large branch fell and blocks wheelchair access.", RequestPriority.High, RequestStatus.Resolved, 15),
                ("Broken fence around the sports field", "Parks & Recreation", "Roodepan", "Sports Avenue", "Gaps in the fence let animals onto the field.", RequestPriority.Low, RequestStatus.Assigned, 13),
                ("Refuse bins stolen", "Sanitation & Waste", "Hadison Park", "Hadison Park Drive", "Several wheelie bins in the street have gone missing.", RequestPriority.Low, RequestStatus.Submitted, 5)
            };

            
            foreach (var s in seed)
            {
                var submitted = DateTime.Now.Date.AddDays(-s.DaysAgo).AddHours(8 + (_nextNumber % 7));

                var request = new ServiceRequest
                {
                    Number = _nextNumber++,
                    Title = s.Title,
                    Category = s.Category,
                    Area = s.Area,
                    Street = s.Street,
                    Description = s.Description,
                    Priority = s.Priority,
                    Status = s.Status,
                    Submitted = submitted,
                    LastUpdated = submitted
                };

              
                string team = GetDepartmentFor(s.Category) + " team";
                for (int stage = 0; stage <= (int)s.Status; stage++)
                {
                    var status = (RequestStatus)stage;
                    request.History.Add(new StatusUpdate
                    {
                        Status = status,
                        When = submitted.AddDays(stage * Math.Max(1, s.DaysAgo / 4.0)),
                        Note = DescribeStage(status, team)
                    });
                }

                if (s.Status != RequestStatus.Submitted) request.AssignedTeam = team;
                request.LastUpdated = request.History[request.History.Count - 1].When;

                AddToIndexes(request);
            }
        }

        private void SaveToDisk()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_storageFile, JsonSerializer.Serialize(All.ToList(), options));
            }
            catch
            {
                // A failure to save (for example a read-only folder) must never crash the app.Thank you gitrepo for explaining this part
            }
        }

        private bool LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_storageFile)) return false;

                var loaded = JsonSerializer.Deserialize<List<ServiceRequest>>(File.ReadAllText(_storageFile));
                if (loaded == null || loaded.Count == 0) return false;

                foreach (var request in loaded) AddToIndexes(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ------------------------------------------------------------------
        //  Comparers
        // ------------------------------------------------------------------

        
        private sealed class TitleKeyComparer : IComparer<(string Title, int Number)>
        {
            public int Compare((string Title, int Number) x, (string Title, int Number) y)
            {
                int c = string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
                return c != 0 ? c : x.Number.CompareTo(y.Number);
            }
        }

        
        private sealed class UrgencyComparer : IComparer<ServiceRequest>
        {
            public int Compare(ServiceRequest? x, ServiceRequest? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                int c = x.Priority.CompareTo(y.Priority);
                if (c != 0) return c;

                c = x.Submitted.CompareTo(y.Submitted);
                return c != 0 ? c : x.Number.CompareTo(y.Number);
            }
        }
    }
}