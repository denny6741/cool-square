using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace borissquare
{
    public class MainForm : Form
    {
        private readonly RotatingSquareControl square;
        private readonly Timer timer;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly Timer changeTextTimer;
        private bool showingFirstText = true;

        private string textOptionA1 = "im just a bebeh";
        private string textOptionA2 = "taking mah chips";

        private string textOptionB1 = "smit";
        private string textOptionB2 = "stinks";

        private string currentText1 = "";
        private string currentText2 = "";

        private readonly int baseSize = 320;
        private readonly Color colorA = Color.LightCyan;
        private readonly Color colorB = Color.LightPink;

        // Keep a reference to the start panel so we can hide it after selection
        private readonly Panel startPanel;

        // Panels inside the startPanel
        private readonly Panel optionsPanel;
        private readonly Panel customPanel;

        // Random generator for the random option
        private readonly Random rng = new Random();

        // Custom text inputs
        private readonly TextBox customTextBox1;
        private readonly TextBox customTextBox2;

        public MainForm()
        {
            Text = "Flashing Square";
            BackColor = Color.FromArgb(15, 23, 32);
            ClientSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            // Square custom control (hidden until user selects an option)
            square = new RotatingSquareControl
            {
                Size = new Size(baseSize, baseSize),
                FillColor = colorA,
                LabelText = "", // will be set after selection
                LabelForeColor = Color.Black,
                LabelFont = new Font("Times New Roman", 24, FontStyle.Bold),
                Visible = false // HIDE THE SQUARE UNTIL SELECTION
            };

            Controls.Add(square);
            CenterSquare();

            // Timer for animation (~60 FPS) - don't start until user selects option
            timer = new Timer { Interval = 16 };
            timer.Tick += Timer_Tick;

            // Timer for changing text - don't start until selection
            changeTextTimer = new Timer { Interval = 5000 }; // 5 seconds
            changeTextTimer.Tick += ChangeTextTimer_Tick;

            // Build the start panel UI and store it in the field so we can hide it later
            startPanel = new Panel
            {
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(220, 20, 20, 20), // semi-transparent overlay
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var startLabel = new Label
            {
                Text = "Choose the text set to display",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Black
            };
            startPanel.Controls.Add(startLabel);

            // Options panel with preset buttons and random + custom
            optionsPanel = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill };
            startPanel.Controls.Add(optionsPanel);

            var btnOptionA = new Button
            {
                Text = $"Option 1:\n\"{textOptionA1}\" / \"{textOptionA2}\"",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnOptionA.Click += (s, e) => StartWithTextsSafe(textOptionA1, textOptionA2);

            var btnOptionB = new Button
            {
                Text = $"Option 2:\n\"{textOptionB1}\" / \"{textOptionB2}\"",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnOptionB.Click += (s, e) => StartWithTextsSafe(textOptionB1, textOptionB2);

            var btnRandom = new Button
            {
                Text = "Random Option",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnRandom.Click += (s, e) =>
            {
                if (rng.Next(2) == 0)
                    StartWithTextsSafe(textOptionA1, textOptionA2);
                else
                    StartWithTextsSafe(textOptionB1, textOptionB2);
            };

            var btnCustom = new Button
            {
                Text = "Custom Text",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnCustom.Click += (s, e) => ShowCustomPanel();

            // Add option buttons to optionsPanel
            optionsPanel.Controls.Add(btnOptionA);
            optionsPanel.Controls.Add(btnOptionB);
            optionsPanel.Controls.Add(btnRandom);
            optionsPanel.Controls.Add(btnCustom);

            // Custom panel (hidden by default) with two textboxes and Start/Back buttons
            customPanel = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill, Visible = false };
            startPanel.Controls.Add(customPanel);

            var lblCustomIntro = new Label
            {
                Text = "Enter up to two lines of text to alternate (leave second empty for single-line).",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.Transparent
            };
            customPanel.Controls.Add(lblCustomIntro);

            customTextBox1 = new TextBox
            {
                Multiline = false,
                Font = new Font("Times New Roman", 18, FontStyle.Regular),
                Size = new Size(520, 36),
            };
            customTextBox2 = new TextBox
            {
                Multiline = false,
                Font = new Font("Times New Roman", 18, FontStyle.Regular),
                Size = new Size(520, 36),
            };

            var lbl1 = new Label { Text = "Line 1:", ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent };
            var lbl2 = new Label { Text = "Line 2:", ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent };

            var btnStartCustom = new Button { Text = "Start", AutoSize = false, Size = new Size(140, 44), BackColor = Color.White };
            btnStartCustom.Click += (s, e) => StartCustomTexts();

            var btnBack = new Button { Text = "Back", AutoSize = false, Size = new Size(140, 44), BackColor = Color.White };
            btnBack.Click += (s, e) => ShowOptionsPanel();

            // Add controls to customPanel
            customPanel.Controls.Add(lbl1);
            customPanel.Controls.Add(customTextBox1);
            customPanel.Controls.Add(lbl2);
            customPanel.Controls.Add(customTextBox2);
            customPanel.Controls.Add(btnStartCustom);
            customPanel.Controls.Add(btnBack);

            // Add the startPanel after adding the square so it overlays
            Controls.Add(startPanel);

            // Layout the start panel controls on resize/initial
            Layout += (s, e) =>
            {
                startPanel.Size = ClientSize;
                startLabel.Width = startPanel.Width;

                int gap = 20;

                // Layout options buttons in the center row
                int totalWidth = btnOptionA.Width + gap + btnOptionB.Width + gap + btnRandom.Width + gap + btnCustom.Width;
                int startX = (startPanel.Width - totalWidth) / 2;
                int y = startPanel.Height / 2 - btnOptionA.Height / 2 + 20;

                btnOptionA.Location = new Point(startX, y);
                btnOptionB.Location = new Point(startX + btnOptionA.Width + gap, y);
                btnRandom.Location = new Point(startX + btnOptionA.Width + gap + btnOptionB.Width + gap, y);
                btnCustom.Location = new Point(startX + btnOptionA.Width + gap + btnOptionB.Width + gap + btnRandom.Width + gap, y);

                // Layout custom panel elements roughly centered
                int cpY = startPanel.Height / 2 - 80;
                lbl1.Location = new Point((startPanel.Width - customTextBox1.Width) / 2 - 60, cpY);
                customTextBox1.Location = new Point((startPanel.Width - customTextBox1.Width) / 2, cpY - 4);
                lbl2.Location = new Point((startPanel.Width - customTextBox2.Width) / 2 - 60, cpY + 44);
                customTextBox2.Location = new Point((startPanel.Width - customTextBox2.Width) / 2, cpY + 40);
                btnStartCustom.Location = new Point((startPanel.Width / 2) - btnStartCustom.Width - 8, cpY + 100);
                btnBack.Location = new Point((startPanel.Width / 2) + 8, cpY + 100);

                // Keep square centered
                CenterSquare();
            };

            // Keyboard: Esc to close, Space to pause/resume (works after start)
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
                if (e.KeyCode == Keys.Space)
                {
                    if (timer.Enabled) { timer.Stop(); stopwatch.Stop(); }
                    else { stopwatch.Start(); timer.Start(); }
                }
            };

            // Start with timers stopped; user chooses option first
            stopwatch.Reset();
        }

        private void ShowCustomPanel()
        {
            optionsPanel.Visible = false;
            customPanel.Visible = true;
        }

        private void ShowOptionsPanel()
        {
            customPanel.Visible = false;
            optionsPanel.Visible = true;
        }

        // Wrapper that catches exceptions and shows them
        private void StartWithTextsSafe(string first, string second)
        {
            try
            {
                StartWithTexts(first, second);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartCustomTexts()
        {
            string a = customTextBox1.Text?.Trim() ?? "";
            string b = customTextBox2.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                // nothing entered — use a simple default
                a = "custom";
                b = "";
            }

            // If only one line entered, alternate with empty second string (will just show one)
            StartWithTextsSafe(a, b);
        }

        private void StartWithTexts(string first, string second)
        {
            currentText1 = first;
            currentText2 = second;
            showingFirstText = true;
            square.LabelText = currentText1;
            square.Invalidate();

            // hide the start UI and show the square
            if (startPanel != null)
            {
                startPanel.Visible = false;
            }
            square.Visible = true;
            square.BringToFront();

            // start animation and text timer
            stopwatch.Restart();
            timer.Start();
            changeTextTimer.Start();
        }

        private void CenterSquare()
        {
            square.Location = new Point(
                (ClientSize.Width - square.Width) / 2,
                (ClientSize.Height - square.Height) / 2
            );
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            int alpha = (int)(a.A + (b.A - a.A) * t);
            return Color.FromArgb(alpha, r, g, bl);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double t = stopwatch.ElapsedMilliseconds / 1000.0;

            // Flashing color (sine between 0..1)
            double sine = (Math.Sin(t * 2.0 * Math.PI * 1.0) + 1.0) / 2.0;
            Color bg = Lerp(colorA, colorB, sine);
            square.FillColor = bg;

            // Slight scale pulsing
            double scale = 1.0 + 0.06 * Math.Sin(t * 2.0 * Math.PI * 1.0);
            int newSize = (int)(baseSize * scale);
            newSize = Math.Max(80, newSize);
            square.Size = new Size(newSize, newSize);
            CenterSquare();

            // Keep text color black
            square.LabelForeColor = Color.Black;

            // Request repaint
            square.Invalidate();
        }

        private void ChangeTextTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentText1) && string.IsNullOrEmpty(currentText2))
                return;

            if (showingFirstText)
                square.LabelText = currentText2;
            else
                square.LabelText = currentText1;

            showingFirstText = !showingFirstText;
            square.Invalidate();
        }

        // Custom control that draws a filled square and centered text. (No rotation)
        private class RotatingSquareControl : Control
        {
            public Color FillColor { get; set; } = Color.LightCyan;
            public string LabelText { get; set; } = "";
            public Color LabelForeColor { get; set; } = Color.Black;
            public Font LabelFont { get; set; } = SystemFonts.DefaultFont;

            public RotatingSquareControl()
            {
                // Enable double buffering and custom painting
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                UpdateStyles();
                BackColor = Color.Black;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                int w = Width;
                int h = Height;

                // Draw rounded rectangle in the control bounds
                var rect = new RectangleF(0, 0, w, h);
                float radius = Math.Min(rect.Width, rect.Height) * 0.06f; // corner radius

                using (var path = RoundedRect(rect, radius))
                using (var brush = new SolidBrush(FillColor))
                using (var pen = new Pen(Color.FromArgb(40, Color.Black), 0.5f))
                {
                    g.FillPath(brush, path);
                    // subtle border for definition
                    g.DrawPath(pen, path);
                }

                // Draw text centered (supports newlines)
                if (!string.IsNullOrEmpty(LabelText))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    using (var font = AdjustFontToFit(g, LabelFont, LabelText, rect.Width * 0.8f, rect.Height * 0.8f))
                    using (var textBrush = new SolidBrush(LabelForeColor))
                    {
                        g.DrawString(LabelText, font, textBrush, rect, sf);
                    }
                }
            }

            // Create a rounded rectangle GraphicsPath
            private static GraphicsPath RoundedRect(RectangleF baseRect, float radius)
            {
                var path = new GraphicsPath();
                float diameter = radius * 2f;

                if (radius <= 0f)
                {
                    path.AddRectangle(baseRect);
                    path.CloseFigure();
                    return path;
                }

                var arc = new RectangleF(baseRect.Location, new SizeF(diameter, diameter));

                // top-left arc
                path.AddArc(arc, 180, 90);

                // top edge
                arc.X = baseRect.Right - diameter;
                path.AddArc(arc, 270, 90);

                // right edge
                arc.Y = baseRect.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                // bottom edge
                arc.X = baseRect.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
                return path;
            }

            // Adjust font size to fit into the available rectangle area (simple heuristic)
            private static Font AdjustFontToFit(Graphics g, Font baseFont, string text, float maxWidth, float maxHeight)
            {
                if (string.IsNullOrEmpty(text))
                    return baseFont;

                float emSize = baseFont.Size;
                var style = baseFont.Style;
                Font testFont = new Font(baseFont.FontFamily, emSize, style);
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

                // Reduce font size until it fits or reaches a small threshold
                for (float size = emSize; size > 6f; size -= 1f)
                {
                    testFont.Dispose();
                    testFont = new Font(baseFont.FontFamily, size, style);
                    var sizeF = g.MeasureString(text, testFont, new SizeF(maxWidth, maxHeight), sf);
                    if (sizeF.Width <= maxWidth && sizeF.Height <= maxHeight)
                        return testFont;
                }

                // fallback to smallest
                testFont.Dispose();
                return new Font(baseFont.FontFamily, 6f, style);
            }
        }
    }
}