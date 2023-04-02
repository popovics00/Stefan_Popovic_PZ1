using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stefan_Popovic_PZ1
{
    /// <summary>
    /// Interaction logic for EllipseWindow.xaml
    /// </summary>
    public partial class EllipseWindow : Window
    {
        public EllipseWindow()
        {
            InitializeComponent();

            if (MainWindow.chngd && !MainWindow.leftClick)
            {
                xValue.Text = MainWindow.ellipseObj.Width.ToString();
                xValue.IsReadOnly = true;
                yValue.Text = MainWindow.ellipseObj.Height.ToString();
                yValue.IsReadOnly = true;

                if (MainWindow.ellipseObj.Fill == Brushes.Red)
                    cbFillColor.SelectedIndex = 0;
                else if (MainWindow.ellipseObj.Fill == Brushes.Blue)
                    cbFillColor.SelectedIndex = 1;
                else if (MainWindow.ellipseObj.Fill == Brushes.Green)
                    cbFillColor.SelectedIndex = 2;
                else if (MainWindow.ellipseObj.Fill == Brushes.Orange)
                    cbFillColor.SelectedIndex = 3;
                else if (MainWindow.ellipseObj.Fill == Brushes.Yellow)
                    cbFillColor.SelectedIndex = 4;
                else if (MainWindow.ellipseObj.Fill == Brushes.Purple)
                    cbFillColor.SelectedIndex = 5;
                else if (MainWindow.ellipseObj.Fill == Brushes.Gray)
                    cbFillColor.SelectedIndex = 6;

                if (MainWindow.ellipseObj.Stroke == Brushes.Red)
                    cbBorderColor.SelectedIndex = 0;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Blue)
                    cbBorderColor.SelectedIndex = 1;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Green)
                    cbBorderColor.SelectedIndex = 2;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Orange)
                    cbBorderColor.SelectedIndex = 3;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Yellow)
                    cbBorderColor.SelectedIndex = 4;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Purple)
                    cbBorderColor.SelectedIndex = 5;
                else if (MainWindow.ellipseObj.Stroke == Brushes.Gray)
                    cbBorderColor.SelectedIndex = 6;

                thickValue.Text = MainWindow.ellipseObj.StrokeThickness.ToString();
                MainWindow.chngd = false;
            }

        }
        
        BrushConverter brushConverter = new BrushConverter();
        public static bool Closed { get; set; }
        public static int x = 0;
        public static int y = 0;
        public static int borderThickness = 0;
        public static SolidColorBrush borderColor;
        public static SolidColorBrush fillColor;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            x = Int32.Parse(xValue.Text);
            y = Int32.Parse(yValue.Text);
            borderThickness = Int32.Parse(thickValue.Text);

            if (cbFillColor.SelectedIndex == 0)
                fillColor = Brushes.Red;
            else if (cbFillColor.SelectedIndex == 1)
                fillColor = Brushes.Blue;
            else if (cbFillColor.SelectedIndex == 2)
                fillColor = Brushes.Green;
            else if (cbFillColor.SelectedIndex == 3)
                fillColor = Brushes.Orange;
            else if (cbFillColor.SelectedIndex == 4)
                fillColor = Brushes.Yellow;
            else if (cbFillColor.SelectedIndex == 5)
                fillColor = Brushes.Purple;
            else if (cbFillColor.SelectedIndex == 6)
                fillColor = Brushes.Gray;

            if (cbBorderColor.SelectedIndex == 0)
                borderColor = Brushes.Red;
            else if (cbBorderColor.SelectedIndex == 1)
                borderColor = Brushes.Blue;
            else if (cbBorderColor.SelectedIndex == 2)
                borderColor = Brushes.Green;
            else if (cbBorderColor.SelectedIndex == 3)
                borderColor = Brushes.Orange;
            else if (cbBorderColor.SelectedIndex == 4)
                borderColor = Brushes.Yellow;
            else if (cbBorderColor.SelectedIndex == 5)
                borderColor = Brushes.Purple;
            else if (cbBorderColor.SelectedIndex == 6)
                borderColor = Brushes.Gray;

            this.Close();
            Closed = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Closed = true;
        }
        private void Validation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
