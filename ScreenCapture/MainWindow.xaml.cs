using System;
using System.Drawing; // For screen capturing (Bitmap, Graphics)
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScreenCapture
{
    public partial class MainWindow : Window
    {
        private System.Windows.Shapes.Rectangle DragRectangle = null;
        private System.Windows.Point StartPoint, LastPoint;

        public MainWindow()
        {
            InitializeComponent();
            Cursor = Cursors.Cross;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.Close(); App.Current.Shutdown(); e.Handled = true; }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint = Mouse.GetPosition(canvas);
            LastPoint = StartPoint;

            DragRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 1,
                Height = 1,
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 1,
                Cursor = Cursors.Cross
            };

            canvas.Children.Add(DragRectangle);
            Canvas.SetLeft(DragRectangle, StartPoint.X);
            Canvas.SetTop(DragRectangle, StartPoint.Y);

            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseUp += canvas_MouseUp;
            canvas.CaptureMouse();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragRectangle == null) return;

            LastPoint = Mouse.GetPosition(canvas);

            DragRectangle.Width = Math.Abs(LastPoint.X - StartPoint.X);
            DragRectangle.Height = Math.Abs(LastPoint.Y - StartPoint.Y);
            Canvas.SetLeft(DragRectangle, Math.Min(LastPoint.X, StartPoint.X));
            Canvas.SetTop(DragRectangle, Math.Min(LastPoint.Y, StartPoint.Y));
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DragRectangle == null) return;
            canvas.ReleaseMouseCapture();

            CaptureScreen();

            canvas.Children.Remove(DragRectangle);
            DragRectangle = null;
            this.Close();
        }

        private void CaptureScreen()
        {
            var screenStart = PointToScreen(StartPoint);
            var screenEnd = PointToScreen(LastPoint);

            int x = (int)Math.Min(screenStart.X, screenEnd.X);
            int y = (int)Math.Min(screenStart.Y, screenEnd.Y);
            int width = (int)Math.Abs(screenEnd.X - screenStart.X);
            int height = (int)Math.Abs(screenEnd.Y - screenStart.Y);

            this.Closed += (s, e) =>
            {
                if (width > 0 && height > 0)
                {
                    using (Bitmap bitmap = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                        }

                        MemoryStream memory = new MemoryStream();
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        PreviewWindow previewWindow = new PreviewWindow(bitmapImage, memory);
                        previewWindow.ShowDialog();
                    }
                }
            };
        }
    }
}
