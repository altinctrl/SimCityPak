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
using System.ComponentModel;

namespace SimCityPak.Views.RW4SectionViews.VertexValueViews
{
    /// <summary>
    /// Interaction logic for ViewColorElement.xaml
    /// </summary>
    public partial class ViewColorElement : UserControl
    {
        public ViewColorElement()
        {
            InitializeComponent();
            byte[] ints = new byte[256];
            for (byte i = 0; i < 255; i++)
            {
                ints[i] = i;
            }
            cbColorPicker.ItemsSource = ints;
        }

        public static readonly DependencyProperty SelectedIndexProperty =
    DependencyProperty.Register("SelectedIndex",
        typeof(byte),
        typeof(ViewColorElement),
        new PropertyMetadata((byte)0));

        [Bindable(true)]
        public byte SelectedIndex
        {
            get { return (byte)this.GetValue(SelectedIndexProperty); }
            set { this.SetValue(SelectedIndexProperty, value); }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           // if (sender != this)
           // {
           ///  cbColorPicker.SelectedValue = (byte)e.NewValue;

          //  }
        }

        

        private void cbColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if (cbColorPicker.SelectedValue != null)
            // {
            //     this.DataContext = (byte)cbColorPicker.SelectedValue;
            // }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            //DataContext = this;
        }
    }
}
