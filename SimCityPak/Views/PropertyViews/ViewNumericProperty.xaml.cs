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

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewFloatProperty.xaml
    /// </summary>
    public partial class ViewNumericProperty : UserControl
    {
        public ViewNumericProperty()
        {
            InitializeComponent();
        }

        private void txtFloat_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.DataContext is FloatProperty)
            {
                FloatProperty prop = this.DataContext as FloatProperty;
                float value;
                if (float.TryParse(txtFloat.Text, out value))
                {
                    prop.Value = value;
                }
            }
            if (this.DataContext is Int32Property)
            {
                Int32Property prop = this.DataContext as Int32Property;
                int value;
                if (Int32.TryParse(txtFloat.Text, out value))
                {
                    prop.Value = value;
                }
            }
            if (this.DataContext is UInt32Property)
            {
                UInt32Property prop = this.DataContext as UInt32Property;
                try
                {
                    prop.Value = prop.Value = Convert.ToUInt32(txtFloat.Text.Substring(2, 8), 16);
                }
                catch
                {

                }
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is FloatProperty)
            {
                FloatProperty prop = this.DataContext as FloatProperty;
                txtFloat.Text = prop.Value.ToString();
            }
            if (this.DataContext is Int32Property)
            {
                Int32Property prop = this.DataContext as Int32Property;
                txtFloat.Text = prop.Value.ToString();
            }
            if (this.DataContext is UInt32Property)
            {
                UInt32Property prop = this.DataContext as UInt32Property;



                // txtFloat.Text = string.Format("{0} {1}", prop.Value.ToHex(), InstanceRegistry.Instance.Find(prop.Value)).Trim();

                txtFloat.Text = string.Format("{0}", prop.Value.ToHex()).Trim();
            }
            


        }
    }
}
