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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using SporeMaster.RenderWare4;
using System.IO;
using Gibbed.Spore.Package;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewHex.xaml
    /// </summary>
    public partial class ViewHex : UserControl
    {
        public ViewHex()
        {
            InitializeComponent();
        }

        public event EventHandler DataChangedHandler;
        private void DataChanged()
        {
            if (DataChangedHandler != null) DataChangedHandler(this, new EventArgs());
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext.GetType() == typeof(DatabaseIndexData))
                {
                    DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                    ByteArrayToIndexedHexStringConverter converter = new ByteArrayToIndexedHexStringConverter();
                    textBoxHex.Text = (string)converter.Convert(index.Data, typeof(string), "8", CultureInfo.CurrentCulture);
                    textBoxRawData.Text = System.Text.ASCIIEncoding.ASCII.GetString(index.Data);
                }
                else if (this.DataContext.GetType() == typeof(byte[]))
                {
                    byte[] data = (byte[])this.DataContext;
                    ByteArrayToIndexedHexStringConverter converter = new ByteArrayToIndexedHexStringConverter();
                    textBoxHex.Text = (string)converter.Convert(data, typeof(string), "8", CultureInfo.CurrentCulture);
                    textBoxRawData.Text = System.Text.ASCIIEncoding.ASCII.GetString(data);
                }
                else if (this.DataContext.GetType() == typeof(RW4Section))
                {
                   // RW4Section section = (RW4Section)this.DataContext;
                   // ByteArrayToIndexedHexStringConverter converter = new ByteArrayToIndexedHexStringConverter();
                   // textBoxHex.Text = (string)converter.Convert(section.obj.Data, typeof(string), "8", CultureInfo.CurrentCulture);
                   // textBoxRawData.Text = System.Text.ASCIIEncoding.ASCII.GetString(section.obj);

                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext.GetType() == typeof(DatabaseIndexData))
                { 
                    DatabaseIndexData index = (DatabaseIndexData)this.DataContext;
                   
                    int i = 0;
                    StringReader reader = new StringReader(textBoxHex.Text);
                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < 8; j++)
                        {
                            (index.Data as byte[])[i + j] = byte.Parse(values[j + 1].ToString(), NumberStyles.AllowHexSpecifier);
                        }
                        i += 8;
                    }


                    index.Index.ModifiedData = new ModifiedGenericFile() { FileData = index.Data };
                    index.Index.IsModified = true;
                   // DataChanged();

                  //
                   // ByteArrayToIndexedHexStringConverter converter = new ByteArrayToIndexedHexStringConverter();
                  //  textBoxHex.Text = (string)converter.Convert(index.Data, typeof(string), "8", CultureInfo.CurrentCulture);
                  //  textBoxRawData.Text = System.Text.ASCIIEncoding.ASCII.GetString(index.Data);
                }
                if (this.DataContext.GetType() == typeof(byte[]))
                {
                    byte[] data = this.DataContext as byte[];

                    int i = 0;
                    StringReader reader = new StringReader(textBoxHex.Text);
                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(new char[]{ ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < 8; j++)
                        {
                            if (j + 1 < values.Length)
                            {
                                (this.DataContext as byte[])[i + j] = byte.Parse(values[j + 1].ToString(), NumberStyles.AllowHexSpecifier);
                            }
                        }
                        i += 8;
                    }

                    DataChanged();

                }
            }
        }

        private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxHex.Text, TextDataFormat.Text);
        }
    }
}
