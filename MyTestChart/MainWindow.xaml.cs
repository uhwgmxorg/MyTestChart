using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MyTestChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int _listSize = 100;

        DispatcherTimer DispatcherTimer;
        int _timerTick = 200;
        int _counter = 0;

        EnumerableDataSource<Measurement> _eds = null;

        /// <summary>
        /// Properties
        /// </summary>
        ObservableCollection<Measurement> _measurements = new ObservableCollection<Measurement>();
        public ObservableCollection<Measurement> Measurements { get { return _measurements; } set { value = _measurements; } }

        /// <summary>
        /// DependencyProperties
        /// </summary>
        public static DependencyProperty MinProperty = DependencyProperty.Register("MinProperty", typeof(double), typeof(MainWindow));
        public double Min { get { return (double)GetValue(MinProperty); } set { SetValue(MinProperty, value); } }
        public static DependencyProperty MaxProperty = DependencyProperty.Register("MaxProperty", typeof(double), typeof(MainWindow));
        public double Max { get { return (double)GetValue(MaxProperty); } set { SetValue(MaxProperty, value); } }

        public MainWindow()
        {
            InitializeComponent();
            label_TimerTickValue.Content = _timerTick.ToString();
            slider_TimerTick.Value = _timerTick;

            DataContext = this;

            // Binding of Min
            Binding B1 = new Binding();
            B1.ElementName = slider_Min.Name;
            B1.Path = new PropertyPath("Value");
            B1.Mode = BindingMode.TwoWay;
            this.SetBinding(MinProperty, B1);
            // Binding of Max
            Binding B2 = new Binding();
            B2.ElementName = slider_Max.Name;
            B2.Path = new PropertyPath("Value");
            B2.Mode = BindingMode.TwoWay;
            this.SetBinding(MaxProperty, B2);

            Measurements = new ObservableCollection<Measurement>();
            Max = 50.0;
            Min = 7.0;

            slider_ListSize.Value = _listSize;
        }

        /******************************/
        /*       Button Events        */
        /******************************/
        #region Button Events

        /// <summary>
        /// #1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            // Test RandomFloat(...)
            float F = RandomFloat((float)Min, (float)Max);
            label_RandomFloat.Content = F.ToString();

            label_Debug.Text = "MinX: " + chartPlotter1.Visible.XMin.ToString() + " MaxX: " + chartPlotter1.Visible.XMax.ToString();
            chartPlotter1.Viewport.Domain = DataRect.Create(Double.MinValue, 0, Double.MaxValue, 100);
        }

        /// <summary>
        /// #2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            CreateRandomDataList();
        }

        /// <summary>
        ///  #3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            AddRandomData();
        }

        /// <summary>
        /// #4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (chartPlotter1.LegendVisibility == System.Windows.Visibility.Visible)
                chartPlotter1.LegendVisibility = System.Windows.Visibility.Hidden;
            else
                chartPlotter1.LegendVisibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Clear the Chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ClearChart_Click(object sender, RoutedEventArgs e)
        {
            ClearChart();
        }

        /// <summary>
        /// Start the simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer.Start();
        }

        /// <summary>
        /// Stop the simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer.Stop();
        }

        /// <summary>
        /// Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        /******************************/
        /*      Menu Events          */
        /******************************/
        #region Menu Events

        #endregion

        /******************************/
        /*      Other Events          */
        /******************************/
        #region Other Events

        /// <summary>
        /// Windows is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Windows_Main_Loaded(object sender, RoutedEventArgs e)
        {
            // necessary for chart the context menu
            chartPlotter1.Loaded += new RoutedEventHandler(ChartMeasurement_Loaded);
            chartPlotter1.MouseWheel += new MouseWheelEventHandler(ChartMeasurement_MouseWheel);


            InitChart();

            // Set Clock Timer
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, _timerTick);
        }

        /// <summary>
        /// This is necessary for edit the context menu (deleing some items)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartMeasurement_Loaded(object sender, RoutedEventArgs e)
        {
            RemoveD3ContextMenuItemContainig("feed");
            RemoveD3ContextMenuItemContainig("Quick");
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartMeasurement_MouseWheel(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Timer for the chart update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            AddRandomData();
        }

        /// <summary>
        /// Slider ListSize has new value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ListSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (label_ListSize != null)
            {
                _listSize = Convert.ToInt32(slider_ListSize.Value);
                label_ListSize.Content = _listSize.ToString();
            }
        }

        /// <summary>
        /// Slider has new value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_TimerTick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DispatcherTimer != null && label_TimerTickValue != null)
            {
                _timerTick = Convert.ToInt32(slider_TimerTick.Value);
                DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, _timerTick);
                label_TimerTickValue.Content = _timerTick.ToString();
            }
        }

        #endregion

        /******************************/
        /*      Other Functions       */
        /******************************/
        #region Other Functions

        /// <summary>
        /// Init the Chart
        /// </summary>
        public void InitChart()
        {
            // Init the Chart
            chartPlotter1.LegendVisibility = System.Windows.Visibility.Hidden;
            //chartPlotter1.NewLegendVisible = false;
            Color C = new Color(); C.A = 255; C.R = 136; C.G = 5; C.B = 42;
            _eds = new EnumerableDataSource<Measurement>(Measurements);
            _eds.SetXMapping(ci => timeAxis.ConvertToDouble(ci.Time));
            _eds.SetYMapping(ci => ci.Value);
            chartPlotter1.AddLineGraph(_eds, C, 4,"Prediction");
            chartPlotter1.Viewport.FitToView();
            chartPlotter1.NewLegendVisible = false;
        }

        /// <summary>
        /// Clear the Chart
        /// </summary>
        public void ClearChart()
        {
            // Clear the Chart
            chartPlotter1.Children.RemoveAll<LineGraph>();
            chartPlotter1.Children.RemoveAll<MarkerPointsGraph>();
            Measurements.Clear();
            InitChart();
        }

        /// <summary>
        /// Make the List
        /// </summary>
        public void CreateRandomDataList()
        {
            Measurements.Clear();
            for (int i = 0; i < _listSize; i++)
            {
                AddRandomData();
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Add one random data to list
        /// </summary>
        public void AddRandomData()
        {
            // Add random data to list
            Measurement M = new Measurement(++_counter, RandomFloat((float)Min, (float)Max), DateTime.Now,Measurements,10);
            if (Measurements.Count <= _listSize)
            {
                Measurements.Add(M);
                listView_Values.ScrollIntoView(listView_Values.Items[listView_Values.Items.Count - 1]);
            }
            else
            {
                int RecordsToDelete = Measurements.Count - _listSize;
                for (int i = 0; i < RecordsToDelete; i++)
                    Measurements.Remove(Measurements[0]);
                Measurements.Add(M);
                listView_Values.ScrollIntoView(listView_Values.Items[listView_Values.Items.Count - 1]);
            }
            label_Number.Content = _counter.ToString();
            label_Date.Content = String.Format(String.Format("{0:dd-MM-yyyy HH:mm:ss ms}", M.Time));
            label_RandomFloat.Content = String.Format("{0:00.00}",M.Value).Replace(".",",");
        }

        /// <summary>
        /// Get a random float betwen min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float RandomFloat(float min, float max)
        {
            float F;
            Random random = new Random();
            F = (float)random.NextDouble() * (max - min) + min;
            return F;
        }

        /// <summary>
        /// Delete an menu item in the D3 ChartPlotter
        /// </summary>
        /// <param name="Text"></param>
        public void RemoveD3ContextMenuItemContainig(string text)
        {
            MenuItem Mi = null;
            // Clear all menu items
            foreach (Object o in chartPlotter1.DefaultContextMenu.StaticMenuItems)
            {
                string StrHelp = o.ToString();
                if (StrHelp.Contains(text))
                {
                    Mi = (MenuItem)o;
                }
            }
            // clear my menu item 'give feedback'
            if (Mi != null)
                chartPlotter1.DefaultContextMenu.StaticMenuItems.Remove(Mi);
        }

        #endregion
    }

    /// <summary>
    /// Help class
    /// </summary>
    public class Measurement
    {
        public int Number { get; set; }
        public DateTime Time { get; set; }
        public float Value { get; set; }
        private float TempValue { get; set; }
        private ObservableCollection<Measurement> List { get; set; }
        private int AverageRange { get; set; }

        /// <summary>
        /// inserts an item
        /// </summary>
        /// <param name="number"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        public Measurement(int number,float value, DateTime time)
        {
            Number = number;
            Time = time;
            Value = value;
        }

        /// <summary>
        /// inserts the average of the item after 
        /// the last averageRange items
        /// </summary>
        /// <param name="number"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <param name="list"></param>
        /// <param name="averageRange"></param>
        public Measurement(int number, float value, DateTime time, ObservableCollection<Measurement> list, int averageRange)
        {
            Number = number;
            Time = time;
            List = list;
            AverageRange = averageRange;
            Value = value;
            TempValue = value;
            Value = GetAverageValue();
        }

        /// <summary>
        /// Calculate the average after the last 
        /// AverageRange elements in the list
        /// </summary>
        /// <returns></returns>
        private float GetAverageValue()
        {
            int c = 0;
            int i = 0;
            float Total = 0;
            float Average = Value;

            if (List.Count == 0)
                return Value;

            if (List.Count < AverageRange)
            {
                for (i = List.Count - 1; i >= 0; i--)
                {
                    Total += List[i].Value;
                }
                Average = (Total + TempValue) / (List.Count - i);
                return Average;
            }

            if (List.Count >= AverageRange)
            {
                for (c = 0, i = List.Count - 1; c < AverageRange-1; i--)
                {
                    c++;
                    Total += List[i].Value;
                }
                Average = (Total + TempValue) / (List.Count - i);
                return Average;
            }

            return -1;
        }
    }

    /// <summary>
    /// Converter Class for data binding
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)((int)(double)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)String.Format(String.Format("{0:dd-MM-yyyy HH:mm:ss ms}", (DateTime)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
