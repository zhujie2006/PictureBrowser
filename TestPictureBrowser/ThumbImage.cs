using System;
using System.Windows;
using System.IO;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TestPictureBrowser
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TestPictureBrowser"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TestPictureBrowser;assembly=TestPictureBrowser"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:ThumbImage/>
    ///
    /// </summary>
    /// <summary>
    /// Thrumb Image.
    /// </summary>
    public class ThrumbImage : System.Windows.Controls.Image
    {
        #region ... Variables ...
        /// <summary>
        /// Set ImageSource Handler.
        /// </summary>
        private static SetImageSourceHandler setImageSourceHandler;
        #endregion ... Variables ...

        #region ... Properties ...
        /// <summary>
        /// Gets or sets ThrumbImageSource.
        /// </summary>
        public string ThrumbImageSource { get; set; }
        /// <summary>
        /// ThrumbImageSourceProperty.
        /// </summary>
        public static DependencyProperty ThrumbImageSourceProperty
            = DependencyProperty.Register("ThrumbImageSource", typeof(string), typeof(System.Windows.Controls.Image),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThrumbImageSourcePropertyChanged)));
        #endregion ... Properties ...

        #region ... Methods ...
        /// <summary>
        /// OnThrumbImageSourcePropertyChanged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnThrumbImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is System.Windows.Controls.Image && args.NewValue is string)
            {
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                string fileName = args.NewValue as string;
                setImageSourceHandler = new SetImageSourceHandler(SetImageSource);
                image.Dispatcher.BeginInvoke(setImageSourceHandler, DispatcherPriority.ApplicationIdle, new object[] { image, fileName });
            }
        }
        /// <summary>
        /// Set Image.Source.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="fileName"></param>
        private static void SetImageSource(System.Windows.Controls.Image image, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    System.Drawing.Image sourceImage = System.Drawing.Image.FromFile(fileName);
                    int imageWidth = 0, imageHeight = 0;
                    InitializeImageSize(sourceImage, image, out imageWidth, out imageHeight);
                    Bitmap sourceBmp = new Bitmap(sourceImage, imageWidth, imageHeight);
                    IntPtr hBitmap = sourceBmp.GetHbitmap();
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    WriteableBitmap writeableBmp = new WriteableBitmap(bitmapSource);
                    sourceImage.Dispose();
                    sourceBmp.Dispose();
                    image.Source = writeableBmp;

                }
                catch (Exception)
                {
                }
            }
        }
        /// <summary>
        /// Initialize ImageSize.
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="image"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        private static void InitializeImageSize(System.Drawing.Image sourceImage, System.Windows.Controls.Image image,
            out int imageWidth, out int imageHeight)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;
            float aspect = (float)width / (float)height;
            if (image.Height != double.NaN)
            {
                imageHeight = Convert.ToInt32(image.Height);
                imageWidth = Convert.ToInt32(aspect * imageHeight);
            }
            else if (image.Width != double.NaN)
            {
                imageWidth = Convert.ToInt32(image.Width);
                imageHeight = Convert.ToInt32(image.Width / aspect);
            }
            else
            {
                imageHeight = 100;
                imageWidth = Convert.ToInt32(aspect * imageHeight);
            }
        }
        #endregion ... Methods ...
    }
    /// <summary>
    /// Set ImageSource Handler.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="fileName"></param>
    public delegate void SetImageSourceHandler(System.Windows.Controls.Image image, string fileName);
}
