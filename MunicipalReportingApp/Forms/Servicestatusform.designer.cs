using System;
using System.Drawing;
using System.Windows.Forms;
using MunicipalReportingApp.Common;

namespace MunicipalReportingApp.Forms
{
    partial class ServiceStatusForm
    {
        private System.ComponentModel.IContainer components = null;

        // ---- Header ----
        private Panel pnlHeader = null!;
        private PictureBox pbLogo = null!;
        private Label lblTitle = null!;
        private Label lblHeaderSub = null!;
        private Button btnBack = null!;

        // ---- Track by reference ----
        private Panel pnlTrack = null!;
        private Label chipTrack = null!;
        private TextBox txtTrack = null!;
        private Button btnTrack = null!;
        private Label lblTrackResult = null!;

        // ---- Filters ----
        private TableLayoutPanel tblSearch = null!;
        private Label chipSearch = null!;
        private Label chipStatus = null!;
        private Label chipCategory = null!;
        private Label chipPeriod = null!;
        private Label chipSort = null!;
        private TextBox txtSearch = null!;
        private CheckBox chkStartsWith = null!;
        private ComboBox cmbStatus = null!;
        private ComboBox cmbCategory = null!;
        private ComboBox cmbPeriod = null!;
        private ComboBox cmbSort = null!;
        private FlowLayoutPanel pnlSearchButtons = null!;
        private Button btnSearch = null!;
        private Button btnClear = null!;

        // ---- Body ----
        private TableLayoutPanel tblBody = null!;
        private Panel pnlLeft = null!;
        private Label lblResults = null!;
        private ListView lvRequests = null!;
        private TabControl tabs = null!;

        // ---- Tab: progress ----
        private TabPage tabProgress = null!;
        private Label lblRefTitle = null!;
        private FlowLayoutPanel flpTags = null!;
        private Label tagStatus = null!;
        private Label tagPriority = null!;
        private Label tagCategory = null!;
        private StageTracker tracker = null!;
        private Label lblMeta = null!;
        private Label lblDescription = null!;
        private Label lblHistoryHeading = null!;
        private ListBox lstHistory = null!;
        private Button btnAdvance = null!;

        // ---- Tab: urgent queue (heap) ----
        private TabPage tabUrgent = null!;
        private Label lblUrgentInfo = null!;
        private ListView lvUrgent = null!;

        // ---- Tab: areas and crews (graph) ----
        private TabPage tabAreas = null!;
        private TableLayoutPanel tblAreas = null!;
        private Label lblNearbyHeading = null!;
        private ListView lvNearby = null!;
        private Label lblRouteHeading = null!;
        private ListBox lstRoute = null!;
        private Label lblMstHeading = null!;
        private ListBox lstMst = null!;

        // ---- Tab: departments (general tree) ----
        private TabPage tabDepartments = null!;
        private Label lblTreeInfo = null!;
        private TreeView tvDepartments = null!;

        // ---- Tab: data structures ----
        private TabPage tabStructures = null!;
        private TextBox txtStructures = null!;

        private ToolTip toolTip = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlHeader = new Panel();
            this.pbLogo = new PictureBox();
            this.lblTitle = new Label();
            this.lblHeaderSub = new Label();
            this.btnBack = new Button();

            this.pnlTrack = new Panel();
            this.chipTrack = new Label();
            this.txtTrack = new TextBox();
            this.btnTrack = new Button();
            this.lblTrackResult = new Label();

            this.tblSearch = new TableLayoutPanel();
            this.chipSearch = new Label();
            this.chipStatus = new Label();
            this.chipCategory = new Label();
            this.chipPeriod = new Label();
            this.chipSort = new Label();
            this.txtSearch = new TextBox();
            this.chkStartsWith = new CheckBox();
            this.cmbStatus = new ComboBox();
            this.cmbCategory = new ComboBox();
            this.cmbPeriod = new ComboBox();
            this.cmbSort = new ComboBox();
            this.pnlSearchButtons = new FlowLayoutPanel();
            this.btnSearch = new Button();
            this.btnClear = new Button();

            this.tblBody = new TableLayoutPanel();
            this.pnlLeft = new Panel();
            this.lblResults = new Label();
            this.lvRequests = new ListView();
            this.tabs = new TabControl();

            this.tabProgress = new TabPage();
            this.lblRefTitle = new Label();
            this.flpTags = new FlowLayoutPanel();
            this.tagStatus = new Label();
            this.tagPriority = new Label();
            this.tagCategory = new Label();
            this.tracker = new StageTracker();
            this.lblMeta = new Label();
            this.lblDescription = new Label();
            this.lblHistoryHeading = new Label();
            this.lstHistory = new ListBox();
            this.btnAdvance = new Button();

            this.tabUrgent = new TabPage();
            this.lblUrgentInfo = new Label();
            this.lvUrgent = new ListView();

            this.tabAreas = new TabPage();
            this.tblAreas = new TableLayoutPanel();
            this.lblNearbyHeading = new Label();
            this.lvNearby = new ListView();
            this.lblRouteHeading = new Label();
            this.lstRoute = new ListBox();
            this.lblMstHeading = new Label();
            this.lstMst = new ListBox();

            this.tabDepartments = new TabPage();
            this.lblTreeInfo = new Label();
            this.tvDepartments = new TreeView();

            this.tabStructures = new TabPage();
            this.txtStructures = new TextBox();

            this.toolTip = new ToolTip(this.components);

            this.SuspendLayout();

            this.ClientSize = new Size(1100, 700);

            // =============================================================
            //  HEADER (deep green - distinct from the other screens)
            // =============================================================
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 70;
            this.pnlHeader.BackColor = AppTheme.ServiceGreenColor;
            this.pnlHeader.Controls.Add(this.pbLogo);
            this.pnlHeader.Controls.Add(this.btnBack);
            this.pnlHeader.Controls.Add(this.lblHeaderSub);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.pbLogo.BackColor = Color.White;
            this.pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.pbLogo.Image = BrandImages.Logo;
            this.pbLogo.Size = new Size(108, 54);
            this.pbLogo.Location = new Point(16, 8);

            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = Color.Transparent;
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.lblTitle.Location = new Point(140, 10);
            this.lblTitle.Text = "Service Request Status";

            this.lblHeaderSub.AutoSize = true;
            this.lblHeaderSub.BackColor = Color.Transparent;
            this.lblHeaderSub.ForeColor = AppTheme.TagTealTint;
            this.lblHeaderSub.Font = AppTheme.SubtitleFont;
            this.lblHeaderSub.Location = new Point(142, 42);
            this.lblHeaderSub.Text = "Follow your requests from the day you report them until they are resolved.";

            this.btnBack.Text = "← Back to Main Menu";
            this.btnBack.Font = AppTheme.FieldFont;
            this.btnBack.BackColor = Color.White;
            this.btnBack.ForeColor = AppTheme.ServiceGreenColor;
            this.btnBack.FlatStyle = FlatStyle.Flat;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.Size = new Size(180, 38);
            this.btnBack.Location = new Point(this.ClientSize.Width - 204, 16);
            this.btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBack.Cursor = Cursors.Hand;

            // =============================================================
            //  TRACK BY REFERENCE
            // =============================================================
            this.pnlTrack.Dock = DockStyle.Top;
            this.pnlTrack.Height = 58;
            this.pnlTrack.BackColor = AppTheme.AmberTintColor;
            this.pnlTrack.Controls.Add(this.lblTrackResult);
            this.pnlTrack.Controls.Add(this.btnTrack);
            this.pnlTrack.Controls.Add(this.txtTrack);
            this.pnlTrack.Controls.Add(this.chipTrack);

            UiHelpers.StyleAsTag(this.chipTrack, Color.White, AppTheme.TagAmberText);
            this.chipTrack.Text = "Track a request by its reference";
            this.chipTrack.Location = new Point(24, 16);

            this.txtTrack.Font = AppTheme.FieldFont;
            this.txtTrack.PlaceholderText = "e.g. SR-1004";
            this.txtTrack.Size = new Size(170, 30);
            this.txtTrack.Location = new Point(262, 14);

            this.btnTrack.Text = "Track";
            this.btnTrack.Font = AppTheme.ButtonFont;
            this.btnTrack.BackColor = AppTheme.ServiceGreenColor;
            this.btnTrack.ForeColor = Color.White;
            this.btnTrack.FlatStyle = FlatStyle.Flat;
            this.btnTrack.FlatAppearance.BorderSize = 0;
            this.btnTrack.Size = new Size(100, 34);
            this.btnTrack.Location = new Point(444, 12);
            this.btnTrack.Cursor = Cursors.Hand;

            this.lblTrackResult.AutoSize = false;
            this.lblTrackResult.Font = AppTheme.EngagementFont;
            this.lblTrackResult.ForeColor = AppTheme.AmberTextColor;
            this.lblTrackResult.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTrackResult.AutoEllipsis = true;
            this.lblTrackResult.Location = new Point(560, 12);
            this.lblTrackResult.Size = new Size(this.ClientSize.Width - 584, 34);
            this.lblTrackResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblTrackResult.Text = "Type the reference you were given when the request was logged.";

            // =============================================================
            //  FILTER BAR
            // =============================================================
            this.tblSearch.Dock = DockStyle.Top;
            this.tblSearch.Height = 92;
            this.tblSearch.BackColor = AppTheme.PanelColor;
            this.tblSearch.Padding = new Padding(24, 8, 24, 8);
            this.tblSearch.ColumnCount = 8;
            this.tblSearch.RowCount = 2;
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));      
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));    
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122F));    
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));      
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 122F));    
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));    
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));    
            this.tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));      
            this.tblSearch.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this.tblSearch.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

            UiHelpers.StyleAsTag(this.chipSearch, AppTheme.TagBlueTint, AppTheme.TagBlueText);
            this.chipSearch.Text = "Search requests";

            UiHelpers.StyleAsTag(this.chipStatus, AppTheme.TagPurpleTint, AppTheme.TagPurpleText);
            this.chipStatus.Text = "Status";

            UiHelpers.StyleAsTag(this.chipCategory, AppTheme.TagTealTint, AppTheme.TagTealText);
            this.chipCategory.Text = "Category";

            UiHelpers.StyleAsTag(this.chipPeriod, AppTheme.TagAmberTint, AppTheme.TagAmberText);
            this.chipPeriod.Text = "Submitted";

            UiHelpers.StyleAsTag(this.chipSort, AppTheme.AccentTintColor, AppTheme.AccentDarkColor);
            this.chipSort.Text = "Sort by";

            this.txtSearch.Font = AppTheme.FieldFont;
            this.txtSearch.PlaceholderText = "e.g. pothole, water, Galeshewe...";
            this.txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.txtSearch.Margin = new Padding(0, 6, 12, 0);

            this.chkStartsWith.Text = "Title starts with";
            this.chkStartsWith.Font = AppTheme.LabelFont;
            this.chkStartsWith.AutoSize = true;
            this.chkStartsWith.Margin = new Padding(0, 10, 8, 0);

            this.cmbStatus.Font = AppTheme.FieldFont;
            this.cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.cmbStatus.Margin = new Padding(0, 6, 12, 0);

            this.cmbCategory.Font = AppTheme.FieldFont;
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.cmbCategory.Margin = new Padding(0, 6, 12, 0);

            this.cmbPeriod.Font = AppTheme.FieldFont;
            this.cmbPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPeriod.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.cmbPeriod.Margin = new Padding(0, 6, 12, 0);

            this.cmbSort.Font = AppTheme.FieldFont;
            this.cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSort.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.cmbSort.Margin = new Padding(0, 6, 12, 0);

            this.pnlSearchButtons.Dock = DockStyle.Fill;
            this.pnlSearchButtons.FlowDirection = FlowDirection.LeftToRight;
            this.pnlSearchButtons.WrapContents = false;
            this.pnlSearchButtons.Controls.Add(this.btnSearch);
            this.pnlSearchButtons.Controls.Add(this.btnClear);

            this.btnSearch.Text = "\U0001F50D  Search";
            this.btnSearch.Font = AppTheme.ButtonFont;
            this.btnSearch.BackColor = AppTheme.AccentColor;
            this.btnSearch.ForeColor = Color.White;
            this.btnSearch.FlatStyle = FlatStyle.Flat;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.Size = new Size(112, 36);
            this.btnSearch.Margin = new Padding(0, 4, 8, 0);
            this.btnSearch.Cursor = Cursors.Hand;

            this.btnClear.Text = "Clear";
            this.btnClear.Font = AppTheme.FieldFont;
            this.btnClear.BackColor = AppTheme.PanelColor;
            this.btnClear.ForeColor = AppTheme.TextColor;
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.FlatAppearance.BorderColor = AppTheme.BorderColor;
            this.btnClear.Size = new Size(64, 36);
            this.btnClear.Margin = new Padding(0, 4, 0, 0);
            this.btnClear.Cursor = Cursors.Hand;

            this.tblSearch.Controls.Add(this.chipSearch, 0, 0);
            this.tblSearch.Controls.Add(this.chipStatus, 2, 0);
            this.tblSearch.Controls.Add(this.chipCategory, 3, 0);
            this.tblSearch.Controls.Add(this.chipPeriod, 4, 0);
            this.tblSearch.Controls.Add(this.chipSort, 5, 0);
            this.tblSearch.Controls.Add(this.txtSearch, 0, 1);
            this.tblSearch.Controls.Add(this.chkStartsWith, 1, 1);
            this.tblSearch.Controls.Add(this.cmbStatus, 2, 1);
            this.tblSearch.Controls.Add(this.cmbCategory, 3, 1);
            this.tblSearch.Controls.Add(this.cmbPeriod, 4, 1);
            this.tblSearch.Controls.Add(this.cmbSort, 5, 1);
            this.tblSearch.Controls.Add(this.pnlSearchButtons, 6, 1);

            // =============================================================
            //  BODY: request list (left) and tabs (right)
            // =============================================================
            this.tblBody.Dock = DockStyle.Fill;
            this.tblBody.BackColor = AppTheme.BackgroundColor;
            this.tblBody.Padding = new Padding(24, 12, 24, 12);
            this.tblBody.ColumnCount = 2;
            this.tblBody.RowCount = 1;
            this.tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            this.tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            this.tblBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tblBody.Controls.Add(this.pnlLeft, 0, 0);
            this.tblBody.Controls.Add(this.tabs, 1, 0);

            this.pnlLeft.Dock = DockStyle.Fill;
            this.pnlLeft.Margin = new Padding(0, 0, 12, 0);
            this.pnlLeft.Controls.Add(this.lvRequests);      
            this.pnlLeft.Controls.Add(this.lblResults);      

            this.lblResults.UseMnemonic = false;
            this.lblResults.Dock = DockStyle.Top;
            this.lblResults.Height = 28;
            this.lblResults.Font = AppTheme.EngagementFont;
            this.lblResults.ForeColor = AppTheme.TextColor;
            this.lblResults.TextAlign = ContentAlignment.MiddleLeft;
            this.lblResults.Text = "Service requests";

            ConfigureList(this.lvRequests);
            this.lvRequests.Columns.Add("Reference", 78);
            this.lvRequests.Columns.Add("Title", 220);
            this.lvRequests.Columns.Add("Category", 112);
            this.lvRequests.Columns.Add("Area", 96);
            this.lvRequests.Columns.Add("Priority", 64);
            this.lvRequests.Columns.Add("Status", 82);
            this.lvRequests.Columns.Add("Submitted", 86);

            this.tabs.Dock = DockStyle.Fill;
            this.tabs.Font = AppTheme.LabelFont;
            this.tabs.Margin = new Padding(0);
            this.tabs.Controls.Add(this.tabProgress);
            this.tabs.Controls.Add(this.tabUrgent);
            this.tabs.Controls.Add(this.tabAreas);
            this.tabs.Controls.Add(this.tabDepartments);
            this.tabs.Controls.Add(this.tabStructures);

            // ---- Tab: progress -------------------------------------------------
            this.tabProgress.Text = "Progress";
            this.tabProgress.BackColor = Color.White;
            this.tabProgress.UseVisualStyleBackColor = false;
            // Fill first, then Bottom, then the Top items from the bottom of the screen upwards.
            this.tabProgress.Controls.Add(this.lstHistory);
            this.tabProgress.Controls.Add(this.btnAdvance);
            this.tabProgress.Controls.Add(this.lblHistoryHeading);
            this.tabProgress.Controls.Add(this.lblDescription);
            this.tabProgress.Controls.Add(this.lblMeta);
            this.tabProgress.Controls.Add(this.tracker);
            this.tabProgress.Controls.Add(this.flpTags);
            this.tabProgress.Controls.Add(this.lblRefTitle);

            this.lblRefTitle.UseMnemonic = false;
            this.lblRefTitle.Dock = DockStyle.Top;
            this.lblRefTitle.Height = 44;
            this.lblRefTitle.Padding = new Padding(10, 10, 10, 0);
            this.lblRefTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            this.lblRefTitle.ForeColor = AppTheme.TextColor;
            this.lblRefTitle.AutoEllipsis = true;
            this.lblRefTitle.Text = "Select a request";

            this.flpTags.Dock = DockStyle.Top;
            this.flpTags.Height = 36;
            this.flpTags.Padding = new Padding(10, 2, 0, 0);
            this.flpTags.WrapContents = false;
            this.flpTags.Controls.Add(this.tagStatus);
            this.flpTags.Controls.Add(this.tagPriority);
            this.flpTags.Controls.Add(this.tagCategory);

            UiHelpers.StyleAsTag(this.tagStatus, AppTheme.TagBlueTint, AppTheme.TagBlueText);
            UiHelpers.StyleAsTag(this.tagPriority, AppTheme.TagAmberTint, AppTheme.TagAmberText);
            UiHelpers.StyleAsTag(this.tagCategory, AppTheme.TagPurpleTint, AppTheme.TagPurpleText);
            this.tagStatus.Margin = new Padding(0, 2, 8, 0);
            this.tagPriority.Margin = new Padding(0, 2, 8, 0);
            this.tagCategory.UseMnemonic = false;
            this.tagCategory.Margin = new Padding(0, 2, 8, 0);
            this.tagStatus.Text = "";
            this.tagPriority.Text = "";
            this.tagCategory.Text = "";

            this.tracker.Dock = DockStyle.Top;
            this.tracker.Height = 84;

            this.lblMeta.UseMnemonic = false;
            this.lblMeta.Dock = DockStyle.Top;
            this.lblMeta.Height = 126;
            this.lblMeta.Padding = new Padding(10, 6, 10, 0);
            this.lblMeta.Font = AppTheme.LabelFont;
            this.lblMeta.ForeColor = AppTheme.TextColor;

            this.lblDescription.UseMnemonic = false;
            this.lblDescription.Dock = DockStyle.Top;
            this.lblDescription.Height = 66;
            this.lblDescription.Padding = new Padding(10, 4, 10, 0);
            this.lblDescription.Font = AppTheme.LabelFont;
            this.lblDescription.ForeColor = AppTheme.MutedTextColor;

            this.lblHistoryHeading.Dock = DockStyle.Top;
            this.lblHistoryHeading.Height = 26;
            this.lblHistoryHeading.Padding = new Padding(10, 4, 0, 0);
            this.lblHistoryHeading.Font = AppTheme.EngagementFont;
            this.lblHistoryHeading.Text = "Progress history";

            this.lstHistory.Dock = DockStyle.Fill;
            this.lstHistory.BorderStyle = BorderStyle.None;
            this.lstHistory.Font = AppTheme.LabelFont;
            this.lstHistory.IntegralHeight = false;
            this.lstHistory.HorizontalScrollbar = true;

            this.btnAdvance.Dock = DockStyle.Bottom;
            this.btnAdvance.Height = 42;
            this.btnAdvance.Text = "Move to next stage (demo)";
            this.btnAdvance.Font = AppTheme.ButtonFont;
            this.btnAdvance.BackColor = AppTheme.ServiceGreenColor;
            this.btnAdvance.ForeColor = Color.White;
            this.btnAdvance.FlatStyle = FlatStyle.Flat;
            this.btnAdvance.FlatAppearance.BorderSize = 0;
            this.btnAdvance.Cursor = Cursors.Hand;

            // ---- Tab: urgent queue (heap) ----------------------------------------
            this.tabUrgent.Text = "Urgent queue";
            this.tabUrgent.BackColor = Color.White;
            this.tabUrgent.UseVisualStyleBackColor = false;
            this.tabUrgent.Controls.Add(this.lvUrgent);
            this.tabUrgent.Controls.Add(this.lblUrgentInfo);

            this.lblUrgentInfo.Dock = DockStyle.Top;
            this.lblUrgentInfo.Height = 64;
            this.lblUrgentInfo.Padding = new Padding(10, 8, 10, 0);
            this.lblUrgentInfo.Font = AppTheme.LabelFont;
            this.lblUrgentInfo.ForeColor = AppTheme.MutedTextColor;
            this.lblUrgentInfo.Text = "The ten open requests that need attention first. A binary heap keeps the most urgent request on top, so it is found instantly.";

            ConfigureList(this.lvUrgent);
            this.lvUrgent.Columns.Add("#", 34);
            this.lvUrgent.Columns.Add("Reference", 76);
            this.lvUrgent.Columns.Add("Title", 200);
            this.lvUrgent.Columns.Add("Priority", 66);
            this.lvUrgent.Columns.Add("Status", 86);

            // ---- Tab: areas and crews (graph) ------------------------------------
            this.tabAreas.Text = "Areas and crews";
            this.tabAreas.BackColor = Color.White;
            this.tabAreas.UseVisualStyleBackColor = false;
            this.tabAreas.Controls.Add(this.tblAreas);

            this.tblAreas.Dock = DockStyle.Fill;
            this.tblAreas.ColumnCount = 1;
            this.tblAreas.RowCount = 3;
            this.tblAreas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tblAreas.RowStyles.Add(new RowStyle(SizeType.Percent, 36F));
            this.tblAreas.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));
            this.tblAreas.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));
            this.tblAreas.Controls.Add(MakeTitledPanel(this.lblNearbyHeading, this.lvNearby,
                "Areas near the selected request (nearest first)"), 0, 0);
            this.tblAreas.Controls.Add(MakeTitledPanel(this.lblRouteHeading, this.lstRoute,
                "Crew route from the depot, following the roads"), 0, 1);
            this.tblAreas.Controls.Add(MakeTitledPanel(this.lblMstHeading, this.lstMst,
                "Cheapest road network that links every area"), 0, 2);

            ConfigureList(this.lvNearby);
            this.lvNearby.Columns.Add("Area", 130);
            this.lvNearby.Columns.Add("Roads", 52);
            this.lvNearby.Columns.Add("Distance", 76);
            this.lvNearby.Columns.Add("Open requests", 100);

            this.lstRoute.BorderStyle = BorderStyle.None;
            this.lstRoute.Font = AppTheme.LabelFont;
            this.lstRoute.IntegralHeight = false;

            this.lstMst.BorderStyle = BorderStyle.None;
            this.lstMst.Font = AppTheme.LabelFont;
            this.lstMst.IntegralHeight = false;

            // ---- Tab: departments (general tree) ---------------------------------
            this.tabDepartments.Text = "Departments";
            this.tabDepartments.BackColor = Color.White;
            this.tabDepartments.UseVisualStyleBackColor = false;
            this.tabDepartments.Controls.Add(this.tvDepartments);
            this.tabDepartments.Controls.Add(this.lblTreeInfo);

            this.lblTreeInfo.Dock = DockStyle.Top;
            this.lblTreeInfo.Height = 64;
            this.lblTreeInfo.Padding = new Padding(10, 8, 10, 0);
            this.lblTreeInfo.Font = AppTheme.LabelFont;
            this.lblTreeInfo.ForeColor = AppTheme.MutedTextColor;
            this.lblTreeInfo.Text = "Which department handles which kind of request. The number in brackets is how many requests are still open.";

            this.tvDepartments.Dock = DockStyle.Fill;
            this.tvDepartments.BorderStyle = BorderStyle.None;
            this.tvDepartments.Font = AppTheme.FieldFont;
            this.tvDepartments.ShowLines = true;
            this.tvDepartments.FullRowSelect = true;

            // ---- Tab: data structures --------------------------------------------
            this.tabStructures.Text = "Data structures";
            this.tabStructures.BackColor = Color.White;
            this.tabStructures.UseVisualStyleBackColor = false;
            this.tabStructures.Controls.Add(this.txtStructures);

            this.txtStructures.Dock = DockStyle.Fill;
            this.txtStructures.Multiline = true;
            this.txtStructures.ReadOnly = true;
            this.txtStructures.ScrollBars = ScrollBars.Vertical;
            this.txtStructures.BorderStyle = BorderStyle.None;
            this.txtStructures.BackColor = Color.White;
            this.txtStructures.Font = new Font("Consolas", 9.5F);
            this.txtStructures.WordWrap = false;

            // =============================================================
            //  FORM
            // =============================================================
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.MinimumSize = new Size(1000, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Service Request Status - Sol Plaatje Municipal Community Portal";
            this.BackColor = AppTheme.BackgroundColor;

            
            this.Controls.Add(this.tblBody);
            this.Controls.Add(this.tblSearch);
            this.Controls.Add(this.pnlTrack);
            this.Controls.Add(this.pnlHeader);

            this.ResumeLayout(false);

            UiHelpers.MakeRounded(this.pbLogo, 8);
            UiHelpers.MakeRounded(this.btnBack, 8);
            UiHelpers.MakeRounded(this.btnTrack, 8);
            UiHelpers.MakeRounded(this.btnSearch, 8);
            UiHelpers.MakeRounded(this.btnClear, 8);
            UiHelpers.MakeRounded(this.btnAdvance, 8);
        }

   
        private static void ConfigureList(ListView list)
        {
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.FullRowSelect = true;
            list.MultiSelect = false;
            list.HideSelection = false;
            list.GridLines = false;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.Font = AppTheme.LabelFont;
        }

       
        private static Panel MakeTitledPanel(Label heading, Control content, string text)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 4) };

            heading.UseMnemonic = false;
            heading.Dock = DockStyle.Top;
            heading.Height = 24;
            heading.Font = AppTheme.EngagementFont;
            heading.ForeColor = AppTheme.TagBlueText;
            heading.TextAlign = ContentAlignment.MiddleLeft;
            heading.Padding = new Padding(2, 0, 0, 0);
            heading.Text = text;

            content.Dock = DockStyle.Fill;

            panel.Controls.Add(content);    
            panel.Controls.Add(heading);    
            return panel;
        }
    }
}