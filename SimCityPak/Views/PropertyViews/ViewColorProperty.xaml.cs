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
using Gibbed.Spore.Properties;
using Gibbed.Spore.Package;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewColorProperty.xaml
    /// </summary>
    public partial class ViewColorProperty : UserControl
    {
        public ViewColorProperty()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is ColorRGBProperty)
                {
                    ColorRGBProperty prop = (ColorRGBProperty)this.DataContext;
                    Color color = Color.FromRgb((byte)(prop.R * 255), (byte)(prop.G * 255), (byte)(prop.B * 255));
                    rectColorSample.Fill = new SolidColorBrush(color);
                }
                else if (this.DataContext is ColorRGBAProperty)
                {
                    ColorRGBAProperty prop = (ColorRGBAProperty)this.DataContext;
                    Color color = Color.FromRgb((byte)(prop.R * 255), (byte)(prop.G * 255), (byte)(prop.B * 255));
                    rectColorSample.Fill = new SolidColorBrush(color);
                }
            }
        }

        private void rectColorSample_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ColorRGBProperty)
            {
                ColorRGBProperty prop = (ColorRGBProperty)this.DataContext;
                Color color = Color.FromRgb((byte)(prop.R * 255), (byte)(prop.G * 255), (byte)(prop.B * 255));

                ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
                dlg.InitialColor = color;
                if (dlg.ShowDialog().GetValueOrDefault(false))
                {
                    color = dlg.SelectedColor;
                }
                prop.R = (float)color.R / 255;
                prop.G = (float)color.G / 255;
                prop.B = (float)color.B / 255;

                rectColorSample.Fill = new SolidColorBrush(color);

                prop.OnPropertyChanged(new EventArgs());
            }
            else if (this.DataContext is ColorRGBAProperty)
            {
                ColorRGBAProperty prop = (ColorRGBAProperty)this.DataContext;
                Color color = Color.FromArgb((byte)(prop.A * 255),(byte)(prop.R * 255), (byte)(prop.G * 255), (byte)(prop.B * 255));

                ColorPickerControls.Dialogs.ColorPickerFullWithAlphaDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullWithAlphaDialog();
                dlg.InitialColor = color;
                if (dlg.ShowDialog().GetValueOrDefault(false))
                {
                    color = dlg.SelectedColor;
                }
                prop.A = (float)color.A / 255;
                prop.R = (float)color.R / 255;
                prop.G = (float)color.G / 255;
                prop.B = (float)color.B / 255;

                rectColorSample.Fill = new SolidColorBrush(color);

                prop.OnPropertyChanged(new EventArgs());

            
            }

            var expression = txtColor.GetBindingExpression(TextBlock.TextProperty);
            if (expression != null) expression.UpdateSource();
        }

    }
}
