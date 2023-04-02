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
    /// Interaction logic for AddTextWindow.xaml
    /// </summary>
    public partial class AddTextWindow : Window
    {
        public AddTextWindow()
        {
            InitializeComponent();

            if (MainWindow.chngd && !MainWindow.leftClick)
            {
                if (MainWindow.polygonObj.Fill == Brushes.Red)
                    cbColor.SelectedIndex = 0;
                else if (MainWindow.polygonObj.Fill == Brushes.Blue)
                    cbColor.SelectedIndex = 1;
                else if (MainWindow.polygonObj.Fill == Brushes.Green)
                    cbColor.SelectedIndex = 2;
                else if (MainWindow.polygonObj.Fill == Brushes.Orange)
                    cbColor.SelectedIndex = 3;
                else if (MainWindow.polygonObj.Fill == Brushes.Yellow)
                    cbColor.SelectedIndex = 4;
                else if (MainWindow.polygonObj.Fill == Brushes.Purple)
                    cbColor.SelectedIndex = 5;
                else if (MainWindow.polygonObj.Fill == Brushes.Purple)
                    cbColor.SelectedItem = 6;

                tbText.Text = MainWindow.block.Text.ToString();
                tbSize.Text = MainWindow.block.FontSize.ToString();
                MainWindow.chngd = false;
            }
        }

        public static bool Closed { get; set; }
        public static SolidColorBrush textColor;
        public static int textSize = 0;
        public static string text;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textSize = Int32.Parse(tbSize.Text);
            text = tbText.Text;

            if (cbColor.SelectedIndex == 0)
                textColor = Brushes.Red;
            else if (cbColor.SelectedIndex == 1)
                textColor = Brushes.Blue;
            else if (cbColor.SelectedIndex == 2)
                textColor = Brushes.Green;
            else if (cbColor.SelectedIndex == 3)
                textColor = Brushes.Orange;
            else if (cbColor.SelectedIndex == 4)
                textColor = Brushes.Yellow;
            else if (cbColor.SelectedIndex == 5)
                textColor = Brushes.Purple;
            else if (cbColor.SelectedIndex == 6)
                textColor = Brushes.Gray;

            this.Close();
            Closed = false;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Closed = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Validation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
