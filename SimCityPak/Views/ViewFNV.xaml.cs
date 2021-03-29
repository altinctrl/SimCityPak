using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gibbed.Spore.Helpers;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for viewFNV.xaml
    /// </summary>
    public partial class ViewFNV : Window
    {
        public ViewFNV()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox2.Text = StringHelpers.FNV(textBox1.Text).ToHex();
            textBox3.Text = StringHelpers.FNV(textBox1.Text, true).ToHex();
        }

        private void Window_KeyUp_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.C && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !textBox1.IsFocused)
            {
                Clipboard.SetText(textBox2.Text);
            }
        }
    }
}
