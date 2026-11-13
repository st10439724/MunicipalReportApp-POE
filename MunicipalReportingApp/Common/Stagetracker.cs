using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalReportingApp.Common
{
    /// <summary>
    /// A small progress tracker drawn as four connected circles:
    /// Submitted - Assigned - In progress - Resolved.
    /// Finished stages are green, the current stage is amber, later stages are grey.
    /// </summary>
    public sealed class StageTracker : Panel
    {
        private static readonly string[] StageNames = { "Submitted", "Assigned", "In progress", "Resolved" };
        private static readonly Font NameFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        private static readonly Font NumberFont = new Font("Segoe UI", 10F, FontStyle.Bold);

        private int _stage = -1;

        public StageTracker()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.White;
        }

        /// <summary>0 = Submitted ... 3 = Resolved. -1 shows all stages as not started.</summary>
        public int Stage
        {
            get => _stage;
            set
            {
                _stage = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int count = StageNames.Length;
            int margin = 60;
            int radius = 15;
            int lineY = 30;
            float step = (this.Width - 2 * margin) / (float)(count - 1);

            // Connecting lines first, so the circles sit on top of them.
            for (int i = 0; i < count - 1; i++)
            {
                float x1 = margin + i * step + radius;
                float x2 = margin + (i + 1) * step - radius;
                bool done = i < _stage;

                using var pen = new Pen(done ? AppTheme.SuccessColor : AppTheme.BorderColor, 4F);
                g.DrawLine(pen, x1, lineY, x2, lineY);
            }

            using var centred = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < count; i++)
            {
                float cx = margin + i * step;
                var circle = new RectangleF(cx - radius, lineY - radius, radius * 2, radius * 2);

                bool finished = i < _stage || (_stage == count - 1 && i == _stage);
                bool current = i == _stage && !finished;

                Color fill = finished ? AppTheme.SuccessColor : current ? AppTheme.InProgressColor : Color.White;
                Color outline = finished || current ? fill : AppTheme.BorderColor;

                using (var brush = new SolidBrush(fill))
                {
                    g.FillEllipse(brush, circle);
                }
                using (var pen = new Pen(outline, 2F))
                {
                    g.DrawEllipse(pen, circle);
                }

                string mark = finished ? "✓" : (i + 1).ToString();
                Color markColor = finished || current ? Color.White : AppTheme.MutedTextColor;
                using (var brush = new SolidBrush(markColor))
                {
                    g.DrawString(mark, NumberFont, brush, circle, centred);
                }

                var nameBox = new RectangleF(cx - step / 2, lineY + radius + 6, step, 24);
                Color nameColor = (finished || current) ? AppTheme.TextColor : AppTheme.DisabledTextColor;
                using (var brush = new SolidBrush(nameColor))
                {
                    g.DrawString(StageNames[i], NameFont, brush, nameBox, centred);
                }
            }
        }
    }
}