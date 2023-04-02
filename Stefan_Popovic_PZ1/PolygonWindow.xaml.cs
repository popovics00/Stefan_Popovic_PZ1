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
    /// Interaction logic for PolygonWindow.xaml
    /// </summary>
    public partial class PolygonWindow : Window
    {
        public PolygonWindow()
        {
            InitializeComponent();

            if (MainWindow.chngd && !MainWindow.leftClick)
            {
                if (MainWindow.polygonObj.Fill == Brushes.Red)
                    cbFillColor.SelectedIndex = 0;
                else if (MainWindow.polygonObj.Fill == Brushes.Blue)
                    cbFillColor.SelectedIndex = 1;
                else if (MainWindow.polygonObj.Fill == Brushes.Green)
                    cbFillColor.SelectedIndex = 2;
                else if (MainWindow.polygonObj.Fill == Brushes.Orange)
                    cbFillColor.SelectedIndex = 3;
                else if (MainWindow.polygonObj.Fill == Brushes.Yellow)
                    cbFillColor.SelectedIndex = 4;
                else if (MainWindow.polygonObj.Fill == Brushes.Purple)
                    cbFillColor.SelectedIndex = 5;
                else if (MainWindow.polygonObj.Fill == Brushes.Purple)
                    cbFillColor.SelectedItem = 6;

                if (MainWindow.polygonObj.Stroke == Brushes.Red)
                    cbBorderColor.SelectedIndex = 0;
                else if (MainWindow.polygonObj.Stroke == Brushes.Blue)
                    cbBorderColor.SelectedIndex = 1;
                else if (MainWindow.polygonObj.Stroke == Brushes.Green)
                    cbBorderColor.SelectedIndex = 2;
                else if (MainWindow.polygonObj.Stroke == Brushes.Orange)
                    cbBorderColor.SelectedIndex = 3;
                else if (MainWindow.polygonObj.Stroke == Brushes.Yellow)
                    cbBorderColor.SelectedIndex = 4;
                else if (MainWindow.polygonObj.Stroke == Brushes.Purple)
                    cbBorderColor.SelectedIndex = 5;
                else if (MainWindow.polygonObj.Stroke == Brushes.Gray)
                    cbBorderColor.SelectedIndex = 6;

                tbThick.Text = MainWindow.polygonObj.StrokeThickness.ToString();
                MainWindow.chngd = false;

            }
        }

        public static bool Closed { get; set; }
        public static SolidColorBrush fillColor;
        public static SolidColorBrush borderColor;
        public static int borderThick = 0;
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            borderThick = Int32.Parse(tbThick.Text);

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
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
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
