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
using System.Globalization;
using System.Collections;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewHexDiff.xaml
    /// </summary>
    public partial class ViewHexDiff : Window
    {
        public ViewHexDiff(byte[] data1, byte[] data2)
        {
            InitializeComponent();

            ByteArrayToIndexedHexStringConverter converter = new ByteArrayToIndexedHexStringConverter();


            string original = (string)converter.Convert(data1, typeof(string), "8", CultureInfo.CurrentCulture);
            string modified = (string)converter.Convert(data2, typeof(string), "8", CultureInfo.CurrentCulture);

            Run currentRun = new Run();
            Run currentRun2 = new Run();

            textBoxHex1.Text = original;
            textBoxHex2.Text = modified;
           /*
            bool isDifferent = false;
            for (int i = 0; i < Math.Min(original.Length, modified.Length); i++)
            {
                if (original[i] != modified[i])
                {
                    if (!isDifferent)
                    {
                        //end the old run and start a new one
                        textBoxHex1.Inlines.Add(currentRun);
                        textBoxHex2.Inlines.Add(currentRun2);
                        currentRun = new Run();
                        currentRun2 = new Run();
                        currentRun.Background = Brushes.Red;
                        currentRun2.Background = Brushes.Red;
                    }
                    isDifferent = true;
                }
                else
                {
                    if (isDifferent)
                    {
                        //end the old run and start a new one
                        textBoxHex1.Inlines.Add(currentRun);
                        textBoxHex2.Inlines.Add(currentRun2);
                        currentRun = new Run();
                        currentRun2 = new Run();
                        currentRun.Background = Brushes.Transparent;
                        currentRun2.Background = Brushes.Transparent;
                    }
                    isDifferent = false;
                }

                currentRun.Text += original[i];
                currentRun2.Text += modified[i];

            }
            textBoxHex1.Inlines.Add(currentRun);
            textBoxHex2.Inlines.Add(currentRun2);
            */



            IStructuralEquatable eqa1 = data1;
            bool theSame = eqa1.Equals(data2, StructuralComparisons.StructuralEqualityComparer);

            if (theSame)
            {
                txtMessage.Text = "These files are identical!";
            }
            else
            {
                txtMessage.Text = "There are differences!";
            }
        }

        private void textBoxHex1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(textBoxHex1.Text);
        }

        private void textBoxHex2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(textBoxHex2.Text);
        }


    }
}
