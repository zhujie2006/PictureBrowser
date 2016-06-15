using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestPictureBrowser
{
    /// <summary>
    /// Search Control
    /// </summary>
    public partial class CustomSearchControl
    {
        public CustomSearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Previous button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnSearchPreviousEvent, this));
        }

        /// <summary>
        /// Next button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnSearchNextEvent, this));
        }

        /// <summary>
        /// Close button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonHide_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnStopSearchEvent, this));
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handle F3 & Shift+F3 hot key
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
                    RaiseEvent(new RoutedEventArgs(OnSearchPreviousEvent, this));
                }
            }
            else
            {
                if (e.Key == Key.F3)
                {
                    RaiseEvent(new RoutedEventArgs(OnSearchNextEvent, this));
                }
                else if (e.Key == Key.Escape)
                {
                    RaiseEvent(new RoutedEventArgs(OnStopSearchEvent, this));
                    Visibility = Visibility.Collapsed;
                }
            }
        }


        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
            typeof(CustomSearchControl), new PropertyMetadata("Search Text"));

        [Description("Search text")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value);}
        }


        public static readonly RoutedEvent OnSearchNextEvent = EventManager.RegisterRoutedEvent("OnSearchNextEvent",
            RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CustomSearchControl));

        [Description ("Next")]
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

        [Description("Previous")]
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
        
        [Description ("Stop search")]
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

        [Description("Search text changed")]
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
            RaiseEvent(new RoutedEventArgs(OnTextChangeEvent, this));
        }
    }
}
