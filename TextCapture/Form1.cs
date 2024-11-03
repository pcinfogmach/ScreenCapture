namespace TextCapture
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    public partial class Form1 : Form
    {
        private Rectangle selectionRectangle;
        private Point startPoint;
        private bool isDragging = false;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.Opacity = 0.5;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Cross;
            this.Capture.

            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = e.Location;
                selectionRectangle = new Rectangle(e.X, e.Y, 0, 0);
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                selectionRectangle.Width = e.X - startPoint.X;
                selectionRectangle.Height = e.Y - startPoint.Y;
                this.Invalidate(); // Redraw the form to show the updated rectangle
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;

                // Calculate capture area and perform screen capture
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(e.X - startPoint.X);
                int height = Math.Abs(e.Y - startPoint.Y);

                if (width > 0 && height > 0)
                {
                    CaptureScreen(x, y, width, height);
                }

                this.Close(); // Close form after capture
            }
        }

        private void CaptureScreen(int x, int y, int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Capture the area of the screen
                    g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
                }

                // Save the screenshot
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = System.IO.Path.Combine(desktopPath, $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                bitmap.Save(filePath);
                MessageBox.Show($"Screenshot saved to {filePath}");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isDragging)
            {
                // Draw the selection rectangle
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRectangle);
                }
            }
        }
    }
}