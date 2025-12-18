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

        private string textOptionA1 = "I miss you Boris";
        private string textOptionA2 = "Come back";

        private string textOptionB1 = "Ay mi gatito";
        private string textOptionB2 = "Meow meow";

        private string currentText1 = "";
        private string currentText2 = "";

        private readonly int baseSize = 320;
        private Color colorA = Color.LightCyan;
        private Color colorB = Color.LightPink;

        private readonly Panel startPanel;
        private readonly Panel optionsPanel;
        private readonly Panel customPanel;
        private readonly Panel colorPanel;

        private readonly Random rng = new Random();

        private readonly TextBox customTextBox1;
        private readonly TextBox customTextBox2;

        private string selectedText1 = "";
        private string selectedText2 = "";

        private Button btnRandomColor;
        private Button btnBackFromColor;

        private Tuple<string, Color, Color>[] colorPresets;

        public MainForm()
        {
            Text = "Flashing Square";
            BackColor = Color.FromArgb(15, 23, 32);
            ClientSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            square = new RotatingSquareControl
            {
                Size = new Size(baseSize, baseSize),
                FillColor = colorA,
                LabelText = "",
                LabelForeColor = Color.Black,
                LabelFont = new Font("Times New Roman", 24, FontStyle.Bold),
                Visible = false
            };

            Controls.Add(square);
            CenterSquare();

            timer = new Timer { Interval = 16 };
            timer.Tick += Timer_Tick;

            changeTextTimer = new Timer { Interval = 5000 };
            changeTextTimer.Tick += ChangeTextTimer_Tick;

            startPanel = new Panel
            {
                Size = ClientSize,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(220, 20, 20, 20),
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

            optionsPanel = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill };
            startPanel.Controls.Add(optionsPanel);

            var btnOptionA = new Button
            {
                Text = "Option 1:\n\"" + textOptionA1 + "\"\n\"" + textOptionA2 + "\"",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnOptionA.Click += (s, e) => ShowColorPanel(textOptionA1, textOptionA2);

            var btnOptionB = new Button
            {
                Text = "Option 2:\n\"" + textOptionB1 + "\"\n\"" + textOptionB2 + "\"",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnOptionB.Click += (s, e) => ShowColorPanel(textOptionB1, textOptionB2);

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
                    ShowColorPanel(textOptionA1, textOptionA2);
                else
                    ShowColorPanel(textOptionB1, textOptionB2);
            };

            var btnCustom = new Button
            {
                Text = "Custom Text",
                AutoSize = false,
                Size = new Size(220, 80),
                BackColor = Color.White,
            };
            btnCustom.Click += (s, e) => ShowCustomPanel();

            optionsPanel.Controls.Add(btnOptionA);
            optionsPanel.Controls.Add(btnOptionB);
            optionsPanel.Controls.Add(btnRandom);
            optionsPanel.Controls.Add(btnCustom);

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

            var btnStartCustom = new Button { Text = "Next", AutoSize = false, Size = new Size(140, 44), BackColor = Color.White };
            btnStartCustom.Click += (s, e) => StartCustomTexts();

            var btnBack = new Button { Text = "Back", AutoSize = false, Size = new Size(140, 44), BackColor = Color.White };
            btnBack.Click += (s, e) => ShowOptionsPanel();

            customPanel.Controls.Add(lbl1);
            customPanel.Controls.Add(customTextBox1);
            customPanel.Controls.Add(lbl2);
            customPanel.Controls.Add(customTextBox2);
            customPanel.Controls.Add(btnStartCustom);
            customPanel.Controls.Add(btnBack);

            colorPanel = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill, Visible = false };
            startPanel.Controls.Add(colorPanel);

            var lblColorIntro = new Label
            {
                Text = "Choose colors for the square",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Transparent
            };
            colorPanel.Controls.Add(lblColorIntro);

            colorPresets = new[]
            {
                new Tuple<string, Color, Color>("Light Cyan / Light Pink", Color.LightCyan, Color.LightPink),
                new Tuple<string, Color, Color>("Blue / Purple", Color.DeepSkyBlue, Color.MediumPurple),
                new Tuple<string, Color, Color>("Green / Yellow", Color.LightGreen, Color.LightYellow),
                new Tuple<string, Color, Color>("Orange / Red", Color.Orange, Color.Tomato),
                new Tuple<string, Color, Color>("Pink / Violet", Color.HotPink, Color.Violet),
                new Tuple<string, Color, Color>("Aqua / Lime", Color.Aqua, Color.Lime)
            };

            foreach (var preset in colorPresets)
            {
                var btn = new Button
                {
                    Text = preset.Item1,
                    AutoSize = false,
                    Size = new Size(200, 60),
                    BackColor = Color.White,
                    Tag = preset
                };
                btn.Click += (s, e) =>
                {
                    var colors = (Tuple<string, Color, Color>)((Button)s).Tag;
                    colorA = colors.Item2;
                    colorB = colors.Item3;
                    StartWithTextsSafe(selectedText1, selectedText2);
                };
                colorPanel.Controls.Add(btn);
            }

            btnRandomColor = new Button { Text = "Random Colors", AutoSize = false, Size = new Size(200, 60), BackColor = Color.White };
            btnRandomColor.Click += (s, e) =>
            {
                var randomPreset = colorPresets[rng.Next(colorPresets.Length)];
                colorA = randomPreset.Item2;
                colorB = randomPreset.Item3;
                StartWithTextsSafe(selectedText1, selectedText2);
            };
            colorPanel.Controls.Add(btnRandomColor);

            btnBackFromColor = new Button { Text = "Back", AutoSize = false, Size = new Size(140, 44), BackColor = Color.White };
            btnBackFromColor.Click += (s, e) => ShowOptionsPanel();
            colorPanel.Controls.Add(btnBackFromColor);

            Controls.Add(startPanel);

            Layout += (s, e) =>
            {
                startPanel.Size = ClientSize;
                startLabel.Width = startPanel.Width;

                int gap = 20;

                int totalWidth = btnOptionA.Width + gap + btnOptionB.Width + gap + btnRandom.Width + gap + btnCustom.Width;
                int startX = (startPanel.Width - totalWidth) / 2;
                int y = startPanel.Height / 2 - btnOptionA.Height / 2 + 20;

                btnOptionA.Location = new Point(startX, y);
                btnOptionB.Location = new Point(startX + btnOptionA.Width + gap, y);
                btnRandom.Location = new Point(startX + btnOptionA.Width + gap + btnOptionB.Width + gap, y);
                btnCustom.Location = new Point(startX + btnOptionA.Width + gap + btnOptionB.Width + gap + btnRandom.Width + gap, y);

                int cpY = startPanel.Height / 2 - 80;
                lbl1.Location = new Point((startPanel.Width - customTextBox1.Width) / 2 - 60, cpY);
                customTextBox1.Location = new Point((startPanel.Width - customTextBox1.Width) / 2, cpY - 4);
                lbl2.Location = new Point((startPanel.Width - customTextBox2.Width) / 2 - 60, cpY + 44);
                customTextBox2.Location = new Point((startPanel.Width - customTextBox2.Width) / 2, cpY + 40);
                btnStartCustom.Location = new Point((startPanel.Width / 2) - btnStartCustom.Width - 8, cpY + 100);
                btnBack.Location = new Point((startPanel.Width / 2) + 8, cpY + 100);

                int colorY = 120;
                int colorX = (startPanel.Width - (3 * 200 + 2 * gap)) / 2;
                int colorBtnIndex = 0;
                foreach (Control ctrl in colorPanel.Controls)
                {
                    if (ctrl is Button && ctrl != btnBackFromColor && ctrl != btnRandomColor)
                    {
                        int row = colorBtnIndex / 3;
                        int col = colorBtnIndex % 3;
                        ctrl.Location = new Point(colorX + col * (200 + gap), colorY + row * (60 + gap));
                        colorBtnIndex++;
                    }
                }
                btnRandomColor.Location = new Point((startPanel.Width - btnRandomColor.Width) / 2, colorY + 180);
                btnBackFromColor.Location = new Point((startPanel.Width - btnBackFromColor.Width) / 2, colorY + 260);

                CenterSquare();
            };

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

            stopwatch.Reset();
        }

        private void ShowCustomPanel()
        {
            optionsPanel.Visible = false;
            customPanel.Visible = true;
            colorPanel.Visible = false;
        }

        private void ShowOptionsPanel()
        {
            customPanel.Visible = false;
            optionsPanel.Visible = true;
            colorPanel.Visible = false;
        }

        private void ShowColorPanel(string text1, string text2)
        {
            selectedText1 = text1;
            selectedText2 = text2;
            optionsPanel.Visible = false;
            customPanel.Visible = false;
            colorPanel.Visible = true;
        }

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
                a = "custom";
                b = "";
            }

            ShowColorPanel(a, b);
        }

        private void StartWithTexts(string first, string second)
        {
            currentText1 = first;
            currentText2 = second;
            showingFirstText = true;
            square.LabelText = currentText1;
            square.Invalidate();

            if (startPanel != null)
            {
                startPanel.Visible = false;
            }
            square.Visible = true;
            square.BringToFront();

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

            double sine = (Math.Sin(t * 2.0 * Math.PI * 1.0) + 1.0) / 2.0;
            Color bg = Lerp(colorA, colorB, sine);
            square.FillColor = bg;

            double scale = 1.0 + 0.06 * Math.Sin(t * 2.0 * Math.PI * 1.0);
            int newSize = (int)(baseSize * scale);
            newSize = Math.Max(80, newSize);
            square.Size = new Size(newSize, newSize);
            CenterSquare();

            square.LabelForeColor = Color.Black;

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

        private class RotatingSquareControl : Control
        {
            public Color FillColor { get; set; } = Color.LightCyan;
            public string LabelText { get; set; } = "";
            public Color LabelForeColor { get; set; } = Color.Black;
            public Font LabelFont { get; set; } = SystemFonts.DefaultFont;

            public RotatingSquareControl()
            {
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

                var rect = new RectangleF(0, 0, w, h);
                float radius = Math.Min(rect.Width, rect.Height) * 0.06f;

                using (var path = RoundedRect(rect, radius))
                using (var brush = new SolidBrush(FillColor))
                using (var pen = new Pen(Color.FromArgb(40, Color.Black), 0.5f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

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

                path.AddArc(arc, 180, 90);

                arc.X = baseRect.Right - diameter;
                path.AddArc(arc, 270, 90);

                arc.Y = baseRect.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                arc.X = baseRect.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
                return path;
            }

            private static Font AdjustFontToFit(Graphics g, Font baseFont, string text, float maxWidth, float maxHeight)
            {
                if (string.IsNullOrEmpty(text))
                    return baseFont;

                float emSize = baseFont.Size;
                var style = baseFont.Style;
                Font testFont = new Font(baseFont.FontFamily, emSize, style);
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

                for (float size = emSize; size > 6f; size -= 1f)
                {
                    testFont.Dispose();
                    testFont = new Font(baseFont.FontFamily, size, style);
                    var sizeF = g.MeasureString(text, testFont, new SizeF(maxWidth, maxHeight), sf);
                    if (sizeF.Width <= maxWidth && sizeF.Height <= maxHeight)
                        return testFont;
                }

                testFont.Dispose();
                return new Font(baseFont.FontFamily, 6f, style);
            }
        }
    }
}