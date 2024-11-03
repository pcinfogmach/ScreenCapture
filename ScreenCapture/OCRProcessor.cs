//using System;
//using System.IO;
//using System.Windows.Media.Imaging;
//using Tesseract;

//namespace ScreenCapture
//{
//    public class OCRProcessor
//    {
//        public async Task<string> ExtractTextAsync(BitmapImage bitmapImage, string language = "eng+heb")
//        {
//            string tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

//            try
//            {
//                // Initialize Tesseract with the specified languages
//                using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
//                using (var img = PixConverter.ToPix(bitmapImage))
//                using (var page = engine.Process(img))
//                {
//                    return page.GetText();
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error during OCR: {ex.Message}");
//                return string.Empty;
//            }
//        }
//    }

//}
