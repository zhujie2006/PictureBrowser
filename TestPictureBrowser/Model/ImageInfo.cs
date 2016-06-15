using System;
namespace TestPictureBrowser.Model
{
    /// <summary>
    /// Image infomation
    /// </summary>
    public class ImageInfo
    {
        public WeakReference ImageReference { get; set; }

        public string Url { get; set; }
    }
}