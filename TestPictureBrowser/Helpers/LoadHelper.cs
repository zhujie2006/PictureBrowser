using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Windows.Media.Imaging;
using TestPictureBrowser.Controls;
using TestPictureBrowser.Model;

namespace TestPictureBrowser.Helpers
{
    /// <summary>
    /// Image loading queue
    /// </summary>
    static class LoadQueue
    {
        /// <summary>
        /// Fail loading delegate
        /// </summary>
        /// <param name="reference">WeakReference of imageinfo</param>
        /// <param name="url">url</param>
        public delegate void FailDelegate(WeakReference reference, string url);

        /// <summary>
        /// Compelete loading delegate
        /// </summary>
        /// <param name="reference">WeakReference of imageinfo</param>
        /// <param name="url">url</param>
        /// <param name="bmp">image content</param>
        public delegate void CompleteDelegate(WeakReference reference, string url, BitmapImage bmp);

        /// <summary>
        /// Compelete load event
        /// </summary>
        public static event CompleteDelegate OnComplete;

        /// <summary>
        /// Failed load event
        /// </summary>
        public static event FailDelegate OnFail;

        /// <summary>
        /// Signal of image source change
        /// </summary>
        private static AutoResetEvent autoResetEvent;

        /// <summary>
        /// static imageinfo queue
        /// </summary>
        private static Queue<ImageInfo> Stacks;

        static LoadQueue()
        {
            Stacks = new Queue<ImageInfo>();

            autoResetEvent = new AutoResetEvent(true);

            var thread = new Thread(DownloadImage) { Name = "Loading Thread", IsBackground = true };

            thread.Start();
        }

        /// <summary>
        /// Download http image
        /// </summary>
        /// <param name="uri">http url</param>
        /// <returns></returns>
        private static BitmapImage DownloadHttpImage(Uri uri)
        {
            BitmapImage image;

            var wc = new WebClient
            {
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable)
            };
            using (var ms = new MemoryStream(wc.DownloadData(uri)))
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }

            return image;
        }

        /// <summary>
        /// Load local image
        /// </summary>
        /// <param name="strPath">local path</param>
        /// <returns></returns>
        private static BitmapImage LoadLocalImage(string strPath)
        {
            BitmapImage image;

            using (var fs = new FileStream(strPath, FileMode.Open))
            {
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = fs;
                image.EndInit();
            }

            return image;
        }

        /// <summary>
        /// Get next image to load from queue
        /// </summary>
        /// <returns></returns>
        private static ImageInfo GetNextImageInfo()
        {
            ImageInfo t = null;

            lock (Stacks)
            {
                if (Stacks.Count > 0)
                {
                    t = Stacks.Dequeue();
                }
            }

            return t;
        }

        /// <summary>
        /// Call compelete event within AsyncImage.Dispatcher
        /// </summary>
        /// <param name="info">Image info</param>
        /// <param name="image">Image content</param>
        private static void CompleteCallbackFunc(ImageInfo info, BitmapImage image)
        {
            if (info == null || info.ImageReference == null)
                return;

            var tImage = info.ImageReference.Target as AsyncImage;
            if (tImage == null)
                return;

            tImage.Dispatcher.BeginInvoke(new Action<WeakReference, string, BitmapImage>((refrense, url, bmp) =>
            {
                if (OnComplete != null)
                {
                    OnComplete(refrense, url, bmp);
                }
            }), info.ImageReference, info.Url, image);
        }

        /// <summary>
        /// Call failed event within AsyncImage.Dispatcher
        /// </summary>
        /// <param name="info">Image info</param>
        /// <param name="url">Image content</param>
        private static void ErrorCallbackFunc(ImageInfo info, string url)
        {
            if (info == null || info.ImageReference == null)
                return;

            var tImage = info.ImageReference.Target as AsyncImage;
            if (tImage == null)
                return;

            tImage.Dispatcher.BeginInvoke(new Action<WeakReference, string>((refrense, path) =>
            {
                if (OnFail != null)
                {
                    OnFail(refrense, path);
                }
            }), info.ImageReference, url);
        }

        /// <summary>
        /// Image load
        /// </summary>
        private static void DownloadImage()
        {
            while (true)
            {
                // 1. Get next image to load from queue
                var t = GetNextImageInfo();

                if (t == null || t.ImageReference == null)
                {
                    // No more image to load, just wait
                    if (Stacks.Count > 0)
                        continue;

                    autoResetEvent.WaitOne();
                    continue;
                }

                try
                {
                    if (t.Url.StartsWith(".") || t.Url.StartsWith("Image"))
                    {
                        // Convert relative path to absolute path
                        t.Url = AppHelper.GetCurrentPath() + t.Url.TrimStart('.').Trim('\\');
                    }

                    // 2. Download or load file, gain image content
                    var uri = new Uri(t.Url);
                    BitmapImage image = null;

                    // uri with http protocol, download image
                    if ("http".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                    {
                        image = DownloadHttpImage(uri);
                    }
                    // uri with file protocol, read image file
                    else if ("file".Equals(uri.Scheme, StringComparison.CurrentCultureIgnoreCase))
                    {
                        image = LoadLocalImage(t.Url);
                    }

                    if (image != null)
                    {
                        if (image.CanFreeze)
                            image.Freeze();

                        // 3.1 Image content not empty, call complete function
                        CompleteCallbackFunc(t, image);
                    }
                    else
                    {
                        throw new Exception("Image url format error!");
                    }
                }
                catch (Exception)
                {
                    // 3.2 Catch exception, call failed function
                    ErrorCallbackFunc(t, t.Url);
                }

                if (Stacks.Count > 0)
                    continue;

                // wait for next signal
                autoResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// Enqueue image info
        /// </summary>
        /// <param name="img">control</param>
        /// <param name="url">url</param>
        public static void Queue(AsyncImage img, String url)
        {
            if (String.IsNullOrEmpty(url)) return;

            lock (Stacks)
            {
                Stacks.Enqueue(new ImageInfo
                {
                    Url = url,
                    ImageReference = new WeakReference(img)
                });

                // Set signal, start to load
                autoResetEvent.Set();
            }
        }
    }
}