using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TestPictureBrowser.Helpers;
using TestPictureBrowser.Model;

namespace TestPictureBrowser
{
    public partial class MainWindow
    {
        #region .tor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Fields

        /// <summary>
        /// Folder path of pictures
        /// </summary>
        private string _imageFolderPath = @"C:\Users\Administrator\Pictures\WallPaper";

	    /// <summary>
	    /// Suffix filters
	    /// </summary>
	    private string[] _picFilters;

        /// <summary>
        /// Is searching
        /// </summary>
        private bool _isSearching;

        #endregion

        /// <summary>
        /// Picture info
        /// </summary>
        private readonly ObservableCollection<PictureInfo> _pictureInfos = new ObservableCollection<PictureInfo>();
        
        /// <summary>
        /// Picture selection changed delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currentPicture">Current picture</param>
        public delegate void PictureChangedHandle(object sender, PictureInfo currentPicture);
        
        /// <summary>
        /// Picture selection changed event
        /// </summary>
        public event PictureChangedHandle OnPictureChanged;
        
        /// <summary>
        /// Load picture infos
        /// </summary>
        private void LoadPictures(string strSearch = null)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isSearching = false;
                ListPics.DataContext = null;
                _pictureInfos.Clear();
                ListPics.DataContext = _pictureInfos;

                GifImageMain.Visibility = Visibility.Collapsed;
                GifImageMain.DataContext = null;

                _imageFolderPath = ConfigurationManager.AppSettings["Path"];

                var strFilter = ConfigurationManager.AppSettings["Filter"];
                _picFilters = strFilter.Split(',');

                if (!Directory.Exists(_imageFolderPath))
                {
                    MessageBox.Show("Image path not exist!");
                    return;
                }

                var arrFiles = Directory.GetFiles(_imageFolderPath);
                if (!arrFiles.Any())
                {
                    MessageBox.Show("Image path not exist!");
                    return;
                }

                var iNum = 0;
                foreach (var fileName in arrFiles)
                {
                    var strExt = FileHelper.GetFileExt(fileName);

                    // During searching
                    if (!string.IsNullOrEmpty(strSearch)) 
                    {
                        _isSearching = true;
                        // File name not contain searching text, don't add to obs list
                        if (!fileName.Contains(strSearch))
                            continue;
                    }

                    if (_picFilters.Contains(strExt))
                    {
                        _pictureInfos.Add(new PictureInfo
                        {
                            IsSelected = false,
                            OrderId = iNum++,
                            PicturePath = fileName,
                            PictureTitle = FileHelper.GetFileName(fileName),
                            UniqueId = new Guid().ToString(),
                            PictureSuffix = strExt,
                        });
                    }
                }

                ListPics.SelectedIndex = 0;
            }));

        }
        
        #region Events

        /// <summary>
        /// Picture selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPics_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pic = ListPics.SelectedItem as PictureInfo;
            try
            {
                if (pic != null)
                {
                    var index = 0;
                    foreach (var pictureInfo in _pictureInfos)
                    {
                        pictureInfo.IsSelected = pictureInfo.Equals(pic);
                        if (pictureInfo.IsSelected)
                        {
                            index = _pictureInfos.IndexOf(pic);
                        }
                    }
                    ListPics.SelectedIndex = index;

                    if (!File.Exists(pic.PicturePath))
                        return;

                    if (pic.PictureSuffix == ".gif")
                    {
                        GifImageMain.Visibility = Visibility.Visible;
                        ImgMain.Visibility = Visibility.Collapsed;

                        GifImageMain.DataContext = pic;
                        GifImageMain.Source = pic.PicturePath;
                    }
                    else
                    {
                        GifImageMain.Visibility = Visibility.Collapsed;
                        ImgMain.Visibility = Visibility.Visible;
                        ImgMain.DataContext = pic;
                        Title = pic.PictureTitle;
                    }
                    
                    if (OnPictureChanged != null)
                    {
                        // Call event
                        OnPictureChanged(this, pic);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // ListBox scroll to current picture
            ListPics.ScrollIntoView(ListPics.SelectedItem);
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadPictures();
        }
        
        /// <summary>
        /// Search for next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnSearchNext(object sender, RoutedEventArgs e)
        {
            var iTotal = _pictureInfos.Count;
            ListPics.SelectedIndex = ListPics.SelectedIndex < (iTotal - 1) ? ListPics.SelectedIndex + 1 : ListPics.SelectedIndex;
        }

        /// <summary>
        /// Goto previous
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnSearchPrevious(object sender, RoutedEventArgs e)
        {
            ListPics.SelectedIndex = ListPics.SelectedIndex > 0 ? ListPics.SelectedIndex - 1 : ListPics.SelectedIndex;
        }

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                Searcher.Visibility = Searcher.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        
        /// <summary>
        /// Searching text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnTextChange(object sender, RoutedEventArgs e)
        {
            LoadPictures(Searcher.Text);
        }

        /// <summary>
        /// Stop searching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnStopSearch(object sender, RoutedEventArgs e)
        {
            if (_isSearching)
                LoadPictures();
        }

        /// <summary>
        /// Goto previous picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPre_OnClick(object sender, RoutedEventArgs e)
        {
            ListPics.SelectedIndex = ListPics.SelectedIndex > 0 ? ListPics.SelectedIndex - 1 : ListPics.SelectedIndex;
        }

        /// <summary>
        /// Goto next picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            var iTotal = _pictureInfos.Count;
            ListPics.SelectedIndex = ListPics.SelectedIndex < (iTotal - 1) ? ListPics.SelectedIndex + 1 : ListPics.SelectedIndex;
        }

        #endregion
    }
}
