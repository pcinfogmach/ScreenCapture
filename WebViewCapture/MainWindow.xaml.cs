using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;

namespace WebViewCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private void InitializeWebView()
        {
            webView.EnsureCoreWebView2Async();
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                webView.CoreWebView2.DOMContentLoaded += (s, e) => 
                {
                    // Capturing the preview as a JPEG image
                    using (var stream = new FileStream("webviewpreview.jpg", FileMode.Create))
                    {
                        webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Jpeg, stream);
                    }
                };
                webView.Source = new Uri("https://www.google.com/");
            };
           
        }
    }
}
