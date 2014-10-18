using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Configuration;
using TestPictureBrowser.Utils;

namespace TestPictureBrowser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region 常量

        /// <summary>
        /// 图片所在文件夹路径
        /// </summary>
        private string _imageFolderPath = @"C:\Users\Administrator\Pictures\WallPaper";
	
	    private string[] _picFilters;

        /// <summary>
        /// 缩略图区宽度
        /// </summary>
        private const double Expanderwidth = 200;

        /// <summary>
        /// 2倍边距
        /// </summary>
        private const double Doublemargin = 40;

        private bool _isSearching;

        #endregion

        /// <summary>
        /// 用于绑定的图片对象，在控件加载时，就开始绑定缩略图
        /// </summary>
        private readonly ObservableCollection<PictureInfo> _pictureInfos = new ObservableCollection<PictureInfo>();


        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 图片切换事件，通知父窗体改变标题
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="currentPicture">当前图片信息</param>
        public delegate void PictureChangedHandle(object sender, PictureInfo currentPicture);


        /// <summary>
        /// 图片切换时发生
        /// </summary>
        public event PictureChangedHandle OnPictureChanged;

        /// <summary>
        /// 图片列表选择改变
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

                        BitmapImage bitmapImage;
                        //利用下面的方法进行图片加载至内存中，防止文件被锁定
                        //将文件打开方式改为只读方式防文件占用
                        using (var reader = new BinaryReader(File.Open(pic.PicturePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            var fi = new FileInfo(pic.PicturePath);
                            byte[] bytes = reader.ReadBytes((int)fi.Length);

                            bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = new MemoryStream(bytes);
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                            bitmapImage.EndInit();
                        }
                        //计算图片的实际展示宽度。
                        //WPF每个像素是1/96。因此，实际宽度|高度的计算公式是:图片像素宽度|高度*96/图片的X方向DPI|图片的Y方向DPI
                        //这里解决的是当加载的图片DPI不是1/96的情况
                        double height = bitmapImage.PixelHeight * 96 / bitmapImage.DpiY;
                        double width = bitmapImage.PixelWidth * 96 / bitmapImage.DpiX;
                        if (height > ActualHeight - Doublemargin || width > ActualWidth - Doublemargin - Expanderwidth)
                        {
                            ImgMain.Stretch = Stretch.Uniform;
                        }
                        else
                        {
                            ImgMain.Stretch = Stretch.None;
                        }

                        ImgMain.Source = bitmapImage;
                    }
                    
                    if (OnPictureChanged != null)
                    {
                        //通知图片标题改变
                        OnPictureChanged(this, pic);
                    }
                }
            }
            catch (Exception ex)//异常图片导致，图片无法加载播放，导致异常 yuxiaohui 20120924
            {
                MessageBox.Show(ex.Message);
            }

            //ListBox滚动到当前图片可见
            ListPics.ScrollIntoView(ListPics.SelectedItem);
        }

        /// <summary>
        /// 加载图片信息
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
                    MessageBox.Show("指定的图片路径不存在!");
                    return;
                }

                var arrFiles = Directory.GetFiles(_imageFolderPath);
                if (!arrFiles.Any())
                {
                    MessageBox.Show("指定的图片路径下不存在任何文件!");
                    return;
                }

                var iNum = 0;
                foreach (var fileName in arrFiles)
                {
                    var strExt = FileHelper.GetFileExt(fileName);

                    if (!string.IsNullOrEmpty(strSearch))       // 搜索时
                    {
                        _isSearching = true;
                        if (!fileName.Contains(strSearch))          // 如果文件中不包含搜索条件直接跳出
                            continue;
                    }

                    if (_picFilters.Contains(strExt))
                    {
                        _pictureInfos.Add(new PictureInfo
                        {
                            IsSelected = false,
                            OrderId = iNum++,
                            PicturePath = fileName,
                            PictureTitle = GetFileName(fileName),
                            UniqueId = new Guid().ToString(),
                            PictureSuffix = strExt,
                        });
                    }
                }
                
                ListPics.SelectedIndex = 0;
            }));
            
        }

        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadPictures();
        }


        /// <summary>
        /// 搜索下一条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnSearchNext(object sender, RoutedEventArgs e)
        {
            var iTotal = _pictureInfos.Count;
            ListPics.SelectedIndex = ListPics.SelectedIndex < (iTotal - 1) ? ListPics.SelectedIndex + 1 : ListPics.SelectedIndex;
        }

        /// <summary>
        /// 搜索上一条
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
        /// 根据
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        private string GetFileName(string strFilePath)
        {
            if (string.IsNullOrEmpty(strFilePath))
                return null;

            var iPos = 0;
            for (int i = strFilePath.Length - 1; i > 0; i--)
            {
                if (strFilePath[i] == '\\')
                {
                    iPos = i;
                    break;
                }
            }
            return strFilePath.Substring(iPos + 1);
        }

        /// <summary>
        /// 图片信息类
        /// </summary>
        public class PictureInfo : INotifyPropertyChanged
        {
            public PictureInfo()
            {
                UniqueId = Guid.NewGuid().ToString();
            }

            /// <summary>
            /// 图片唯一性ID，一般采用Guid
            /// </summary>
            public string UniqueId { get; set; }

            /// <summary>
            /// 图片后缀
            /// </summary>
            public string PictureSuffix { get; set; }

            private string _pictureTitle;

            /// <summary>
            /// 图片标题
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
            /// 图片路径
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
            /// 从0开始的序号
            /// </summary>
            public int OrderId { get; set; }

            private bool _isSelected;

            /// <summary>
            /// 是否被选中
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

            //实现INotifyPropertyChanged的接口
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(PropertyChangedEventArgs e)
            {

                /*判断event是否为空，如果该属性未指定绑定则为空*/
                if (PropertyChanged != null)
                {

                    PropertyChanged(this, e);
                }
            }
        }


        /// <summary>
        /// 搜索文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnTextChange(object sender, RoutedEventArgs e)
        {
            LoadPictures(Searcher.Text);
        }

        /// <summary>
        /// 停止搜索事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Searcher_OnOnStopSearch(object sender, RoutedEventArgs e)
        {
            if (_isSearching)
                LoadPictures();
        }

        /// <summary>
        /// 上一幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPre_OnClick(object sender, RoutedEventArgs e)
        {
            ListPics.SelectedIndex = ListPics.SelectedIndex > 0 ? ListPics.SelectedIndex - 1 : ListPics.SelectedIndex;
        }

        /// <summary>
        /// 下一幅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            var iTotal = _pictureInfos.Count;
            ListPics.SelectedIndex = ListPics.SelectedIndex < (iTotal - 1) ? ListPics.SelectedIndex + 1 : ListPics.SelectedIndex;
        }
    }
}
