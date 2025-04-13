using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tema1.ViewModel;

namespace Tema1.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as CalculatorViewModel).OperationsOrder == false)
            {
                string buttonLabel = (sender as Button).Content.ToString();
                (DataContext as CalculatorViewModel).OnButtonPressed(buttonLabel);
            }
            else
            {
                string buttonLabel = (sender as Button).Content.ToString();
                (DataContext as CalculatorViewModel).OnButtonPressedOrder(buttonLabel);
            }
        }
        private void MemoryButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonLabel = (sender as Button).Content.ToString();
            (DataContext as CalculatorViewModel).OnMemoryButtonPressed(buttonLabel);
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vasile Marina\n10LF234");
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.Key.ToString();
            if (e.Key == Key.Add) key = "+";
            else if (e.Key == Key.Subtract) key = "-";
            else if (e.Key == Key.Multiply) key = "*";
            else if (e.Key == Key.Divide) key = "/";
            else if (e.Key == Key.Return) key = "=";
            else if (e.Key == Key.Back) key = "<=";
            else if (e.Key == Key.Decimal) key = ".";
            else if (e.Key == Key.Escape || e.Key == Key.C) key = "c";
            else if (e.Key >= Key.D0 && e.Key <= Key.D9) key = (e.Key - Key.D0).ToString();
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) key = (e.Key - Key.NumPad0).ToString();

            if ((DataContext as CalculatorViewModel).OperationsOrder == false)
                (DataContext as CalculatorViewModel).OnButtonPressed(key);           
            else           
                (DataContext as CalculatorViewModel).OnButtonPressedOrder(key);
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as CalculatorViewModel).CutText();
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as CalculatorViewModel).CopyText();
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as CalculatorViewModel).PasteText();
        }
        private void DigitGrouping_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as CalculatorViewModel).DigitGroupingOn = !(DataContext as CalculatorViewModel).DigitGroupingOn;
        }
        private void OperationsOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The changes will apply after restarting the application.");
            (DataContext as CalculatorViewModel).OperationsOrder = !(DataContext as CalculatorViewModel).OperationsOrder;
        }
        private void MemoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as CalculatorViewModel).GetOperandFromMemory(decimal.Parse(MemoryListBox.SelectedItem.ToString()));
        }
    }
}
