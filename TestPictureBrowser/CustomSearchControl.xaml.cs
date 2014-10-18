using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestPictureBrowser
{
    /// <summary>
    /// CustomSearchControl.xaml 的交互逻辑
    /// </summary>
    public partial class CustomSearchControl : UserControl
    {
        public CustomSearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 上一条查询记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_OnClick(object sender, RoutedEventArgs e)
        {
            SearchUserControl.RaiseEvent(new RoutedEventArgs(OnSearchPreviousEvent, SearchUserControl));
        }

        /// <summary>
        /// 下一条记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            SearchUserControl.RaiseEvent(new RoutedEventArgs(OnSearchNextEvent, SearchUserControl));
        }

        /// <summary>
        /// 关闭隐藏窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonHide_OnClick(object sender, RoutedEventArgs e)
        {
            SearchUserControl.RaiseEvent(new RoutedEventArgs(OnStopSearchEvent, SearchUserControl));
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 响应键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomSearchControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            bool isShift = false;
            var kd = e.KeyboardDevice;
            if ((kd.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0 ||
                (kd.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0)
            {
                isShift = true;
            }

            if (isShift)
            {
                if (e.Key == Key.F3)
                {
                    SearchUserControl.RaiseEvent(new RoutedEventArgs(OnSearchPreviousEvent, SearchUserControl));
                }
            }
            else
            {
                if (e.Key == Key.F3)
                {
                    SearchUserControl.RaiseEvent(new RoutedEventArgs(OnSearchNextEvent, SearchUserControl));
                }
                else if (e.Key == Key.Escape)
                {
                    SearchUserControl.RaiseEvent(new RoutedEventArgs(OnStopSearchEvent, SearchUserControl));
                    SearchUserControl.Visibility = Visibility.Collapsed;
                }
            }
        }


        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
            typeof (CustomSearchControl), new PropertyMetadata("搜索文本"));
        
        [Description ("搜索文本")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value);}
        }


        public static readonly RoutedEvent OnSearchNextEvent = EventManager.RegisterRoutedEvent("OnSearchNextEvent",
            RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CustomSearchControl));

        [Description ("搜索下一条开始")]
        public event RoutedEventHandler OnSearchNext
        {
            add
            {
                if (OnSearchNextEvent != null)
                    AddHandler(OnSearchNextEvent, value);
            }

            remove
            {
                if (OnSearchNextEvent != null)
                    RemoveHandler(OnSearchNextEvent, value);
            }
        }

        public static readonly RoutedEvent OnSearchPreviousEvent = EventManager.RegisterRoutedEvent("OnSearchPreviousEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomSearchControl));

        [Description("搜索上一条开始")]
        public event RoutedEventHandler OnSearchPrevious
        {
            add
            {
                if (OnSearchPreviousEvent != null)
                    AddHandler(OnSearchPreviousEvent, value);
            }

            remove
            {
                if (OnSearchPreviousEvent != null)
                    RemoveHandler(OnSearchPreviousEvent, value);
            }
        }

        public static readonly RoutedEvent OnStopSearchEvent = EventManager.RegisterRoutedEvent("OnStopSearchEvent",
            RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CustomSearchControl));
        
        [Description ("停止搜索")]
        public event RoutedEventHandler OnStopSearch
        {
            add
            {
                if (OnStopSearchEvent != null)
                    AddHandler(OnStopSearchEvent, value);
            }

            remove
            {
                if (OnStopSearchEvent != null)
                    RemoveHandler(OnStopSearchEvent, value);
            }
        }

        public static readonly RoutedEvent OnTextChangeEvent = EventManager.RegisterRoutedEvent("OnTextChangeEvent",
            RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CustomSearchControl));

        [Description("搜索文本改变")]
        public event RoutedEventHandler OnTextChange
        {
            add
            {
                if (OnTextChangeEvent != null)
                    AddHandler(OnTextChangeEvent, value);
            }

            remove
            {
                if (OnTextChangeEvent != null)
                    RemoveHandler(OnTextChangeEvent, value);
            }
        }

        private void TextBoxTrue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Text = TextBoxTrue.Text;
            SearchUserControl.RaiseEvent(new RoutedEventArgs(OnTextChangeEvent, SearchUserControl));
        }
    }
}
