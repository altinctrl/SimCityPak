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
using SimCityPak.Views;
using SimCityPak.Views.PropertyViews.PropertyEditViews;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewBasicProperty.xaml
    /// </summary>
    public partial class ViewBasicProperty : UserControl
    {
        public ViewBasicProperty()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is Vector2Property)
            {
                Vector2Property prop = this.DataContext as Vector2Property;

                ViewEditVector2Property editWindow = new ViewEditVector2Property(prop);
                if(editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }
            if (this.DataContext is Vector3Property)
            {
                Vector3Property prop = this.DataContext as Vector3Property;

                ViewEditVector3Property editWindow = new ViewEditVector3Property(prop);
                if (editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }
            
            if (this.DataContext is Vector4Property)
            {
                Vector4Property prop = this.DataContext as Vector4Property;

                ViewEditVector4Property editWindow = new ViewEditVector4Property(prop);
                if (editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }

            if (this.DataContext is BoundingBoxProperty)
            {
                BoundingBoxProperty prop = this.DataContext as BoundingBoxProperty;

                ViewEditBoundingBoxProperty editWindow = new ViewEditBoundingBoxProperty(prop);
                if (editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }

            if (this.DataContext is TextProperty)
            {
                TextProperty prop = this.DataContext as TextProperty;

                ViewEditTextProperty editWindow = new ViewEditTextProperty(prop);
                if (editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }

            if (this.DataContext is TransformProperty)
            {
                TransformProperty prop = this.DataContext as TransformProperty;

                ViewEditTransformProperty editWindow = new ViewEditTransformProperty(prop);
                if (editWindow.ShowDialog().GetValueOrDefault(false))
                {
                    prop = editWindow.EditProperty;
                    prop.OnPropertyChanged(new EventArgs());
                }
            }

            var expression = txtValue.GetBindingExpression(TextBlock.TextProperty);
            if (expression != null) expression.UpdateTarget();
            
        }
    }
}
