using MunicipalReportingApp.Common;
using MunicipalReportingApp.Models;
using MunicipalReportingApp.Services;
using MunicipalReportingApp.Structures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MunicipalReportingApp.Forms
{
    /// <summary>
    /// Service Request Status screen (Task 3).
    ///
    ///  - Left: a list of service requests with their status; filter, search and sort them.
    ///  - "Track a request": type a unique reference (e.g. SR-1004) to jump straight to it.
    ///  - Tabs on the right:
    ///        Progress          the selected request's stage tracker and history
    ///        Urgent queue      the most urgent open requests (binary heap)
    ///        Areas and crews   nearby areas, crew route and road network (graph, BFS, DFS, MST)
    ///        Departments       who handles what (general tree)
    ///        Data structures   live facts about the trees, heap and graph
    ///
    /// The structures themselves live in Structures/ and are used through ServiceRequestRepository.
    /// </summary>
    public partial class ServiceStatusForm : Form
    {
        private static readonly int[] PeriodDays = { 0, 7, 30 };

        private readonly ServiceRequestRepository _repo = ServiceRequestRepository.Instance;

        private ServiceRequest? _selected;
        private bool _loading;

        public ServiceStatusForm()
        {
            InitializeComponent();

            // Pick up anything citizens reported on the Report Issues screen since this form was last opened.
            _repo.SyncWithReportedIssues();

            ConfigureControls();
            WireEventHandlers();

            this.WindowState = FormWindowState.Maximized;
            this.Load += ServiceStatusForm_Load;
        }

        // ------------------------------------------------------------------
        //  Setup
        // ------------------------------------------------------------------

        private void ConfigureControls()
        {
            cmbStatus.Items.Add("All statuses");
            foreach (RequestStatus status in Enum.GetValues(typeof(RequestStatus)))
            {
                cmbStatus.Items.Add(StatusText(status));
            }
            cmbStatus.SelectedIndex = 0;

            cmbCategory.Items.Add(ServiceRequestRepository.AllCategories);
            foreach (var category in _repo.GetCategories())
            {
                cmbCategory.Items.Add(category);
            }
            cmbCategory.SelectedIndex = 0;

            cmbPeriod.Items.AddRange(new object[] { "Any time", "Last 7 days", "Last 30 days" });
            cmbPeriod.SelectedIndex = 0;

            // The order here must match the RequestSort enum.
            cmbSort.Items.AddRange(new object[] { "Reference", "Newest first", "Oldest first", "Most urgent", "Title A-Z" });
            cmbSort.SelectedIndex = 0;

            toolTip.SetToolTip(chkStartsWith, "Tick to match only titles that begin with your text (uses the binary search tree).");
            toolTip.SetToolTip(btnAdvance, "Demonstration only: moves the request on to its next stage so you can see the tracker change.");
            toolTip.SetToolTip(txtTrack, "Examples: SR-1004, sr1004 or just 1004");
        }

        private void WireEventHandlers()
        {
            btnBack.Click += (_, _) => this.Close();
            btnTrack.Click += (_, _) => TrackFromBox();
            btnSearch.Click += (_, _) => RunSearch(_selected?.Number);
            btnClear.Click += (_, _) => ClearFilters();
            btnAdvance.Click += BtnAdvance_Click;

            txtTrack.Enter += (_, _) => this.AcceptButton = btnTrack;     // Enter key tracks
            txtSearch.Enter += (_, _) => this.AcceptButton = btnSearch;   // Enter key searches

            lvRequests.SelectedIndexChanged += LvRequests_SelectedIndexChanged;
            lvUrgent.DoubleClick += LvUrgent_DoubleClick;
        }

        private void ServiceStatusForm_Load(object? sender, EventArgs e)
        {
            RefreshDepartments();
            RefreshUrgent();
            RefreshGraphPanels();
            RunSearch(null);
            this.AcceptButton = btnSearch;
        }

        // ------------------------------------------------------------------
        //  Tracking, searching and the request list
        // ------------------------------------------------------------------

        /// <summary>Looks the typed reference up in the AVL tree and shows the request.</summary>
        private void TrackFromBox()
        {
            string text = txtTrack.Text.Trim();

            if (text.Length == 0)
            {
                lblTrackResult.ForeColor = AppTheme.AccentDarkColor;
                lblTrackResult.Text = "Please type a reference such as SR-1004.";
                return;
            }

            if (_repo.TryFindByReference(text, out var request, out int comparisons) && request != null)
            {
                lblTrackResult.ForeColor = AppTheme.AmberTextColor;
                lblTrackResult.Text = $"Found {request.Reference}: {StatusText(request.Status)}. "
                    + $"The AVL tree needed {comparisons} comparison(s) out of {_repo.Count} requests.";

                SelectRequestByNumber(request.Number);
                tabs.SelectedTab = tabProgress;
            }
            else
            {
                lblTrackResult.ForeColor = AppTheme.AccentDarkColor;
                lblTrackResult.Text = $"No request was found with the reference \"{text}\". Please check the number.";
            }
        }

        private void ClearFilters()
        {
            txtSearch.Clear();
            chkStartsWith.Checked = false;
            cmbStatus.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            cmbPeriod.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;
            RunSearch(_selected?.Number);
        }

        private RequestQuery ReadQuery()
        {
            RequestStatus? status = cmbStatus.SelectedIndex > 0
                ? (RequestStatus)(cmbStatus.SelectedIndex - 1)
                : null;

            string category = cmbCategory.SelectedItem?.ToString() ?? ServiceRequestRepository.AllCategories;
            int days = cmbPeriod.SelectedIndex >= 0 ? PeriodDays[cmbPeriod.SelectedIndex] : 0;
            var sort = cmbSort.SelectedIndex >= 0 ? (RequestSort)cmbSort.SelectedIndex : RequestSort.Reference;

            return new RequestQuery(txtSearch.Text, status, category, days, chkStartsWith.Checked, sort);
        }

        /// <summary>Runs the current filters and refills the list, keeping the same request selected if it is still there.</summary>
        private void RunSearch(int? keepNumber)
        {
            var results = _repo.Search(ReadQuery());
            FillRequestList(results, keepNumber);
        }

        private void FillRequestList(List<ServiceRequest> results, int? keepNumber)
        {
            ListViewItem? toSelect = null;

            _loading = true;
            lvRequests.BeginUpdate();
            try
            {
                lvRequests.Items.Clear();

                foreach (var r in results)
                {
                    var item = new ListViewItem(new[]
                    {
                        r.Reference,
                        r.Title,
                        r.Category,
                        r.Area,
                        r.Priority.ToString(),
                        StatusText(r.Status),
                        r.Submitted.ToString("dd MMM yyyy")
                    })
                    {
                        Tag = r,
                        BackColor = RowColour(r.Status),
                        ForeColor = r.Priority == RequestPriority.Critical && r.IsOpen
                            ? AppTheme.AccentDarkColor
                            : AppTheme.TextColor
                    };

                    lvRequests.Items.Add(item);

                    if (keepNumber.HasValue && r.Number == keepNumber.Value) toSelect = item;
                }
            }
            finally
            {
                lvRequests.EndUpdate();
                _loading = false;
            }

            lblResults.Text = results.Count == _repo.Count
                ? $"Service requests ({results.Count})"
                : $"Service requests ({results.Count} of {_repo.Count} match your filters)";

            if (toSelect == null && lvRequests.Items.Count > 0 && keepNumber == null)
            {
                toSelect = lvRequests.Items[0];
            }

            if (toSelect != null)
            {
                toSelect.Selected = true;
                toSelect.EnsureVisible();
                ShowRequest((ServiceRequest)toSelect.Tag!);
            }
            else if (keepNumber.HasValue && _repo.NumberIndex.TryGetValue(keepNumber.Value, out var still))
            {
                ShowRequest(still);          // filtered out of the list, but keep showing it on the right
            }
            else
            {
                ShowRequest(null);
            }
        }

        private void LvRequests_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading || lvRequests.SelectedItems.Count == 0) return;

            ShowRequest((ServiceRequest)lvRequests.SelectedItems[0].Tag!);
        }

        /// <summary>Makes sure the request is visible in the list (clearing filters if it is not) and selects it.</summary>
        private void SelectRequestByNumber(int number)
        {
            if (FindItem(number) == null)
            {
                // It is hidden by the current filters, so show everything again.
                txtSearch.Clear();
                chkStartsWith.Checked = false;
                cmbStatus.SelectedIndex = 0;
                cmbCategory.SelectedIndex = 0;
                cmbPeriod.SelectedIndex = 0;
                RunSearch(number);
                return;
            }

            var item = FindItem(number);
            if (item != null)
            {
                item.Selected = true;
                item.EnsureVisible();
                lvRequests.Focus();
            }
        }

        private ListViewItem? FindItem(int number)
        {
            foreach (ListViewItem item in lvRequests.Items)
            {
                if (item.Tag is ServiceRequest r && r.Number == number) return item;
            }
            return null;
        }

        // ------------------------------------------------------------------
        //  Showing one request
        // ------------------------------------------------------------------

        private void ShowRequest(ServiceRequest? r)
        {
            _selected = r;

            if (r == null)
            {
                lblRefTitle.Text = "Select a request";
                tagStatus.Text = "";
                tagPriority.Text = "";
                tagCategory.Text = "";
                tracker.Stage = -1;
                lblMeta.Text = "";
                lblDescription.Text = "";
                lstHistory.Items.Clear();
                btnAdvance.Enabled = false;
                lvNearby.Items.Clear();
                lblNearbyHeading.Text = "Areas near the selected request (nearest first)";
                txtStructures.Lines = _repo.BuildStructureReport(null).ToArray();
                return;
            }

            lblRefTitle.Text = r.Reference + "  -  " + r.Title;

            tagStatus.Text = StatusText(r.Status);
            (tagStatus.BackColor, tagStatus.ForeColor) = StatusTagColours(r.Status);

            tagPriority.Text = r.Priority + " priority";
            (tagPriority.BackColor, tagPriority.ForeColor) = PriorityTagColours(r.Priority);

            tagCategory.Text = r.Category;

            tracker.Stage = (int)r.Status;

            double km = _repo.DistanceFromDepot(r.Area);

            lblMeta.Text =
                "Area: " + r.Area + (string.IsNullOrEmpty(r.Street) ? "" : "   |   Street: " + r.Street) + "\n" +
                "Department: " + _repo.GetDepartmentFor(r.Category) + "\n" +
                "Assigned team: " + (string.IsNullOrEmpty(r.AssignedTeam) ? "Not assigned yet" : r.AssignedTeam) + "\n" +
                "Submitted: " + r.Submitted.ToString("dd MMM yyyy HH:mm") + "\n" +
                "Last update: " + r.LastUpdated.ToString("dd MMM yyyy HH:mm") + "\n" +
                "Crew distance from the depot: " + (km >= 0 ? km.ToString("0.0") + " km" : "unknown");

            lblDescription.Text = r.Description;

            lstHistory.Items.Clear();
            foreach (var update in r.History.OrderByDescending(h => h.When))
            {
                lstHistory.Items.Add(update.When.ToString("dd MMM yyyy HH:mm") + "   " + StatusText(update.Status) + ": " + update.Note);
            }

            if (r.IsOpen)
            {
                btnAdvance.Enabled = true;
                btnAdvance.Text = "Move to \"" + StatusText((RequestStatus)((int)r.Status + 1)) + "\" (demo)";
            }
            else
            {
                btnAdvance.Enabled = false;
                btnAdvance.Text = "This request is resolved";
            }

            RefreshNearby(r);
            txtStructures.Lines = _repo.BuildStructureReport(r).ToArray();
        }

        private void BtnAdvance_Click(object? sender, EventArgs e)
        {
            if (_selected == null) return;

            int number = _selected.Number;

            if (_repo.AdvanceStatus(number))
            {
                RunSearch(number);
                RefreshUrgent();
                RefreshDepartments();
                RefreshGraphPanels();
            }
        }

        // ------------------------------------------------------------------
        //  Side tabs
        // ------------------------------------------------------------------

        /// <summary>Heap: the ten most urgent open requests.</summary>
        private void RefreshUrgent()
        {
            lvUrgent.BeginUpdate();
            try
            {
                lvUrgent.Items.Clear();

                int rank = 1;
                foreach (var r in _repo.GetMostUrgent(10))
                {
                    var item = new ListViewItem(new[]
                    {
                        rank.ToString(), r.Reference, r.Title, r.Priority.ToString(), StatusText(r.Status)
                    })
                    {
                        Tag = r,
                        BackColor = RowColour(r.Status)
                    };
                    lvUrgent.Items.Add(item);
                    rank++;
                }
            }
            finally
            {
                lvUrgent.EndUpdate();
            }
        }

        private void LvUrgent_DoubleClick(object? sender, EventArgs e)
        {
            if (lvUrgent.SelectedItems.Count == 0) return;

            var request = (ServiceRequest)lvUrgent.SelectedItems[0].Tag!;
            SelectRequestByNumber(request.Number);
            tabs.SelectedTab = tabProgress;
        }

        /// <summary>Graph (breadth-first walk): the suburbs close to the selected request.</summary>
        private void RefreshNearby(ServiceRequest r)
        {
            lblNearbyHeading.Text = "Areas near " + r.Area + " (nearest first)";

            lvNearby.BeginUpdate();
            try
            {
                lvNearby.Items.Clear();

                foreach (var near in _repo.GetNearbyAreas(r.Area, 3))
                {
                    lvNearby.Items.Add(new ListViewItem(new[]
                    {
                        near.Area,
                        near.Hops.ToString(),
                        near.DistanceKm.ToString("0.0") + " km",
                        near.OpenRequests.ToString()
                    }));
                }
            }
            finally
            {
                lvNearby.EndUpdate();
            }
        }

        /// <summary>Graph: the depth-first crew route and the minimum spanning tree.</summary>
        private void RefreshGraphPanels()
        {
            lstRoute.Items.Clear();
            int stop = 1;
            foreach (var (area, open) in _repo.GetCrewRoute(ServiceRequestRepository.DepotArea))
            {
                string label = stop == 1 ? area + " (depot)" : area;
                lstRoute.Items.Add(stop + ". " + label + "  -  " + open + " open request(s)");
                stop++;
            }

            lstMst.Items.Clear();
            var kruskal = _repo.GetMinimumRoadNetwork();
            var prim = _repo.GetMinimumRoadNetworkPrim();

            foreach (var edge in kruskal.Edges)
            {
                lstMst.Items.Add(edge.From + "  to  " + edge.To + "   " + edge.Weight.ToString("0.0") + " km");
            }
            lstMst.Items.Add("");
            lstMst.Items.Add("Total: " + kruskal.TotalWeight.ToString("0.0") + " km of road joins all "
                             + _repo.Areas.NodeCount + " areas (Kruskal).");
            lstMst.Items.Add("Prim's method from the depot gives " + prim.TotalWeight.ToString("0.0") + " km: the same total.");
        }

        /// <summary>General tree: departments and categories with the number of open requests.</summary>
        private void RefreshDepartments()
        {
            var open = _repo.CountOpenByCategory();

            tvDepartments.BeginUpdate();
            try
            {
                tvDepartments.Nodes.Clear();
                AddTreeNodes(_repo.Departments.Root, tvDepartments.Nodes, open);
                tvDepartments.ExpandAll();
            }
            finally
            {
                tvDepartments.EndUpdate();
            }
        }

        private static int AddTreeNodes(TreeNode<string> source, TreeNodeCollection target, Dictionary<string, int> openByCategory)
        {
            var node = new TreeNode(source.Value);
            target.Add(node);

            int total = 0;

            if (source.IsLeaf)
            {
                openByCategory.TryGetValue(source.Value, out total);
            }
            else
            {
                foreach (var child in source.Children)
                {
                    total += AddTreeNodes(child, node.Nodes, openByCategory);
                }
            }

            node.Text = source.Value + "  (" + total + " open)";
            return total;
        }

        // ------------------------------------------------------------------
        //  Wording and colours
        // ------------------------------------------------------------------

        private static string StatusText(RequestStatus status)
        {
            return status == RequestStatus.InProgress ? "In progress" : status.ToString();
        }

        private static Color RowColour(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.Assigned: return AppTheme.TagBlueTint;
                case RequestStatus.InProgress: return AppTheme.AmberTintColor;
                case RequestStatus.Resolved: return AppTheme.TagTealTint;
                default: return Color.White;
            }
        }

        private static (Color Back, Color Fore) StatusTagColours(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.Assigned: return (AppTheme.TagBlueTint, AppTheme.TagBlueText);
                case RequestStatus.InProgress: return (AppTheme.AmberTintColor, AppTheme.AmberTextColor);
                case RequestStatus.Resolved: return (AppTheme.TagTealTint, AppTheme.TagTealText);
                default: return (AppTheme.TagPurpleTint, AppTheme.TagPurpleText);
            }
        }

        private static (Color Back, Color Fore) PriorityTagColours(RequestPriority priority)
        {
            switch (priority)
            {
                case RequestPriority.Critical: return (AppTheme.AccentTintColor, AppTheme.AccentDarkColor);
                case RequestPriority.High: return (AppTheme.AmberTintColor, AppTheme.AmberTextColor);
                case RequestPriority.Medium: return (AppTheme.TagBlueTint, AppTheme.TagBlueText);
                default: return (AppTheme.TagTealTint, AppTheme.TagTealText);
            }
        }
    }
}