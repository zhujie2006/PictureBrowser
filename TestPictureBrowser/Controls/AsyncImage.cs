using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TestPictureBrowser.Helpers;

namespace TestPictureBrowser.Controls
{
    [TemplatePart(Name = ImgLoaderName, Type = typeof(Image))]
    [TemplatePart(Name = ImgContentName, Type = typeof(Image))]
    public class AsyncImage : Control
    {
        #region .tor

        static AsyncImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AsyncImage), new FrameworkPropertyMetadata(typeof(AsyncImage)));

            SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(string),
                typeof(AsyncImage), new PropertyMetadata(OnSourceWithSourceChanged));

            LoadQueue.OnComplete += LoadImage_OnComplate;
            LoadQueue.OnFail += LoadImage_OnOnFail;
        }

        #endregion

        #region Fields

        private const string ImgLoaderName = "LoadingImg";
        private const string ImgContentName = "ContentImg";

        /// <summary>
        /// Rotate story
        /// </summary>
        private Storyboard _story;

        /// <summary>
        /// Loading image
        /// </summary>
        private Image _imgLoader;

        /// <summary>
        /// Content image
        /// </summary>
        private Image _imgContent;
        
        #endregion

        #region DependencyProperty

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Stretch DependencyProperty
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(AsyncImage), new PropertyMetadata(Stretch.UniformToFill));

        /// <summary>
        /// Attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SourceProperty;

        /// <summary>
        /// Queue to loading queue when source changed
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void OnSourceWithSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LoadQueue.Queue((AsyncImage)o, (string)e.NewValue);
        }

        #endregion
        
        #region Static Methods 

        /// <summary>
        /// Get image source property
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string GetSource(AsyncImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            return (string)image.GetValue(SourceProperty);
        }

        /// <summary>
        /// Set source property of image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="value"></param>
        public static void SetSource(AsyncImage image, string value)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            image.SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Check whether image is legal
        /// </summary>
        /// <param name="img"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        private static bool CheckImage(AsyncImage img, string u)
        {
            try
            {
                string source = GetSource(img);

                if (source.StartsWith(".") || source.StartsWith("Image"))
                {
                    source = AppHelper.GetCurrentPath() + source.TrimStart('.').Trim('\\');
                }

                if (source != u)
                    return false;

                if (img._imgContent == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Show content of image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="u"></param>
        /// <param name="bNeedCheck"></param>
        private static void ShowContent(AsyncImage img, string u, bool bNeedCheck = true)
        {
            if (bNeedCheck)
            {
                if (!CheckImage(img, u))
                    return;
            }

            var storyboard = new Storyboard();
            var doubleAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(500.0)));

            Storyboard.SetTarget(doubleAnimation, img._imgContent);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();

            img.StopLoading();
        }

        /// <summary>
        /// Image load complete
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="u"></param>
        /// <param name="bmp"></param>
        private static void LoadImage_OnComplate(WeakReference reference, string u, BitmapImage bmp)
        {
            if (reference == null || reference.Target == null)
                    return;

            var img = reference.Target as AsyncImage;

            if (img == null || !CheckImage(img, u))
                return;

            img._imgContent.Stretch = img.Stretch;
            // Set Source of content image
            img._imgContent.Source = bmp;

            // Already checked, show
            ShowContent(img, u, false);
        }

        /// <summary>
        /// Load image failed
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="url"></param>
        private static void LoadImage_OnOnFail(WeakReference reference, string url)
        {
            var img = reference.Target as AsyncImage;
            ShowContent(img, url);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            _imgLoader = GetTemplateChild<Image>(ImgLoaderName);
            _imgContent = GetTemplateChild<Image>(ImgContentName);

            _story = (Storyboard) TryFindResource("StoryboardLoading");
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Begin loading story when control onloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _story.Begin(_imgLoader, true);
        }

        /// <summary>
        /// Stop loading state
        /// </summary>
        private void StopLoading()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _story.Pause(_imgLoader);
                _imgLoader.Visibility = Visibility.Collapsed;
            }));
        }
        
        private T GetTemplateChild<T>(string childName) where T : FrameworkElement, new()
        {
            return (GetTemplateChild(childName) as T) ?? new T();
        }

        #endregion
    }
}
