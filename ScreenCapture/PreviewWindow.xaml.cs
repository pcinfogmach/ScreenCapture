using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tesseract;

namespace ScreenCapture
{
    public partial class PreviewWindow : Window
    {
        private readonly BitmapImage _bitmapImage;
        private readonly MemoryStream _imageStream;
        bool doNotShutDown;

        public PreviewWindow(BitmapImage bitmapImage, MemoryStream imageStream)
        {
            InitializeComponent();
            ApplyTheme();
            SetButtonContentBasedOnLanguage();
            _bitmapImage = bitmapImage;
            _imageStream = imageStream;
            PreviewImage.Source = _bitmapImage;
            ExtractTextFromImage();
            this.Closed += (s, e) => { if (!doNotShutDown) App.Current.Shutdown(); };
        }

        private void ApplyTheme()
        {
            bool isDarkTheme = IsDarkThemeEnabled();
            if (!isDarkTheme)
            {
                this.Background = Brushes.WhiteSmoke;
                this.Foreground = Brushes.Black;
            }
        }

        private bool IsDarkThemeEnabled()
        {
            const string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string registryValueName = "AppsUseLightTheme";

            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryKeyPath))
            {
                if (key?.GetValue(registryValueName) is int value)
                {
                    return value == 0; // 0 means Dark Theme is enabled, 1 means Light Theme
                }
            }
            return false; // Default to light theme if the registry value is not found
        }

        private void SetButtonContentBasedOnLanguage()
        {
            var info = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            // Check if the system language is set to Hebrew
            if (info == "he")
            {
                this.Title = "צילום מסך";
                this.FlowDirection = FlowDirection.RightToLeft;
                ExtractedTextBox.Text = "מחלץ טקסט. אנא המתן...";
                // Change button content to Hebrew
                SaveImageButton.Content = "שמור תמונה";
                SaveTextButton.Content = "שמור טקסט";
                CopyImageButton.Content = "העתק תמונה";
                CopyTextButton.Content = "העתק טקסט";
                RestartButton.Content = "לכידה חדשה";
                GoogleTranslateButton.ToolTip = "תרגום גוגל";
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.Close(); App.Current.Shutdown(); e.Handled = true; }
        }

        private async void ExtractTextFromImage()
        {
            ExtractedTextBox.Text = await Task.Run(() =>
            {
                try
                {
                    string tessDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                    List<string> files = Directory.GetFiles(tessDataFolder, "*.traineddata").ToList();
                    string tessLang = Path.GetFileNameWithoutExtension(files[0]);
                    if (files.Count > 1)
                    {
                        for (int i = 1; i < files.Count; i++) { tessLang += "+" + Path.GetFileNameWithoutExtension(files[i]); }
                    }

                    // Use the existing MemoryStream with Tesseract
                    _imageStream.Seek(0, SeekOrigin.Begin); // Reset position
                    using (var engine = new TesseractEngine(@"./tessdata", tessLang, EngineMode.Default))
                    using (var img = Pix.LoadFromMemory(_imageStream.ToArray()))
                    using (var page = engine.Process(img))
                    {
                        // Get the extracted text
                        var text = page.GetText().Trim();

                        // Replace single newlines with spaces and keep paragraph breaks
                        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<!\n)\n(?!\n)", " ");

                        return text;
                    }
                }
                catch (Exception ex)
                {
                    return $"Failed to extract text: {ex.Message}";
                }
            });
        }


        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                Filter = "PNG Image|*.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(_bitmapImage));
                    encoder.Save(fileStream);
                }
                Close();
                MessageBox.Show("Screenshot saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveTextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"ExtractedText_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Filter = "Text File|*.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, ExtractedTextBox.Text);
                Close();
                MessageBox.Show("Text saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CopyImageButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(_bitmapImage);
            Close();
            MessageBox.Show("Image copied to clipboard!", "Copy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyTextButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ExtractedTextBox.Text);
            Close();
            MessageBox.Show("Text copied to clipboard!", "Copy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            doNotShutDown = true;
            this.Close();
            new MainWindow().ShowDialog();
        }

        private async void GoogleTranslateButton_Click(object sender, RoutedEventArgs e)
        {
            string textToTranslate = ExtractedTextBox.Text;          
            string targetLanguage = Regex.Match(textToTranslate, @"\p{IsHebrew}").Success ? "en" : "he";

            var browser = new WebBrowser();
            string allowedUrl = $"https://translate.google.com/?sl=auto&tl={targetLanguage}&text={Uri.EscapeDataString(textToTranslate)}&op=translate";

            browser.Navigate(allowedUrl);
            browser.Navigating += (s, args) =>
            {
                if (!args.Uri.AbsoluteUri.StartsWith("https://translate.google.com"))
                {
                    args.Cancel = true; // Cancel navigation if URL is not allowed
                }
            };

            Window window = new Window { Content = browser, Title = "Google Translate" };
            doNotShutDown = true;
            this.Close();
            window.Closing += (s, e) => { App.Current.Shutdown(); };
            window.Show();
        }
    }
}
