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
using System.IO;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewEffectsDirectory.xaml
    /// </summary>
    public partial class ViewEffectsDirectory : UserControl
    {
        public ViewEffectsDirectory()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(DatabaseIndexData))
            {
                DatabaseIndexData index = (DatabaseIndexData)this.DataContext;

                MemoryStream byteStream = new MemoryStream(index.Data);
                try
                {

                    EffectsDirectory effDir = EffectsDirectory.CreateFromStream(byteStream);
                }
                catch
                {
                }

            }

        }
    }
}
