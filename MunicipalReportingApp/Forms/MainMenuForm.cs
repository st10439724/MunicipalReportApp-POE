//using System;
//using System.Windows.Forms;
//using MunicipalReportingApp.Services;



using System;
using System.Windows.Forms;
using MunicipalReportingApp.Services;

namespace MunicipalReportingApp.Forms
{
    /// 
    /// The application's landing screen. Presents the three top-level
    /// citizen services: "Report Issues" (Part 1), "Local Events and
    /// Announcements" (Part 2) and "Service Request Status" (Task 3).
    /// 
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            ConfigureTooltips();
            RefreshFooter();
        }

        private void ConfigureTooltips()
        {
            toolTipDisabled.SetToolTip(btnServiceStatus, "Track your service requests by their unique reference.");
        }

        
        /// Engagement strategy (part 2): the main menu shows a running
        /// community counter, reinforcing that other citizens are
        /// actively participating and encouraging the user to do the same.
        
        public void RefreshFooter()
        {
            int count = IssueRepository.Instance.Count;

            lblFooterBadge.Text = count == 0
                ? "Be the first to report an issue!"
                : $"{count} issue(s) reported by citizens like you";
        }

        private void BtnReportIssues_Click(object? sender, EventArgs e)
        {
            using var reportForm = new ReportIssueForm();
            this.Hide();
            reportForm.ShowDialog(this);
            this.Show();
            RefreshFooter();
        }

        private void BtnEvents_Click(object? sender, EventArgs e)
        {
            using var eventsForm = new EventsForm();
            this.Hide();
            eventsForm.ShowDialog(this);
            this.Show();
        }

        private void BtnServiceStatus_Click(object? sender, EventArgs e)
        {
            using var statusForm = new ServiceStatusForm();
            this.Hide();
            statusForm.ShowDialog(this);
            this.Show();
        }
    }
}











































































































//namespace MunicipalReportingApp.Forms
//{
//    /// <summary>
//    /// The application's landing screen. Presents the three top-level
//    /// citizen services. "Report Issues" (Part 1) and "Local Events and
//    /// Announcements" (Part 2) are functional; "Service Request Status"
//    /// is visibly present but disabled, ready for Task 3.
//    /// </summary>
//    public partial class MainMenuForm : Form
//    {
//        public MainMenuForm()
//        {
//            InitializeComponent();
//            ConfigureUpcomingFeatures();
//            RefreshFooter();
//        }

//        /// <summary>
//        /// "Service Request Status" is not built yet (Task 3), so it stays
//        /// visible but disabled, with a hint explaining why.
//        /// </summary>
//        private void ConfigureUpcomingFeatures()
//        {
//            btnServiceStatus.Enabled = false;

//            toolTipDisabled.SetToolTip(btnServiceStatus, "This feature is coming in a future release.");
//        }

//        /// <summary>
//        /// Engagement strategy (part 2): the main menu shows a running
//        /// community counter, reinforcing that other citizens are
//        /// actively participating and encouraging the user to do the same.
//        /// </summary>
//        public void RefreshFooter()
//        {
//            int count = IssueRepository.Instance.Count;

//            lblFooterBadge.Text = count == 0
//                ? "Be the first to report an issue!"
//                : $"{count} issue(s) reported by citizens like you";
//        }

//        private void BtnReportIssues_Click(object? sender, EventArgs e)
//        {
//            using var reportForm = new ReportIssueForm();
//            this.Hide();
//            reportForm.ShowDialog(this);
//            this.Show();
//            RefreshFooter();
//        }

//        private void BtnEvents_Click(object? sender, EventArgs e)
//        {
//            using var eventsForm = new EventsForm();
//            this.Hide();
//            eventsForm.ShowDialog(this);
//            this.Show();
//        }
//    }
//}
