using System;
using System.ComponentModel;

namespace TestPictureBrowser.Model
{
    /// <summary>
    /// Picture info model (for display)
    /// </summary>
    public class PictureInfo : INotifyPropertyChanged
    {
        #region .tor

        public PictureInfo()
        {
            UniqueId = Guid.NewGuid().ToString();
        }

        #endregion

        #region Fields

        /// <summary>
        /// Guid
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Suffix
        /// </summary>
        public string PictureSuffix { get; set; }

        private string _pictureTitle;
        /// <summary>
        /// Title
        /// </summary>
        public string PictureTitle
        {
            get { return _pictureTitle; }
            set
            {
                _pictureTitle = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PictureTitle"));
            }
        }

        private string _picturePath;
        /// <summary>
        /// Path
        /// </summary>
        public string PicturePath
        {
            get { return _picturePath; }
            set
            {
                _picturePath = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PicturePath"));
            }
        }

        /// <summary>
        /// Order number (begin with 0)
        /// </summary>
        public int OrderId { get; set; }

        private bool _isSelected;
        /// <summary>
        /// Is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsSelected"));
            }
        }
        
        #endregion

        #region Implematation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}