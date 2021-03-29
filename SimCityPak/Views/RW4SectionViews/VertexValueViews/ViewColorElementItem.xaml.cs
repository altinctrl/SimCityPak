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

namespace SimCityPak.Views.RW4SectionViews.VertexValueViews
{
    /// <summary>
    /// Interaction logic for ViewColorElementItem.xaml
    /// </summary>
    public partial class ViewColorElementItem : UserControl
    {



        public ViewColorElementItem()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.GetType() == typeof(byte) || e.NewValue.GetType() == typeof(Int32))
            {
                ViewVertexArray win = this.GetParent(typeof(ViewVertexArray)) as ViewVertexArray;
                if (win == null)
                {
                    win = (ViewVertexArray)((ItemsPresenter)this.GetParent(typeof(ItemsPresenter))).TemplatedParent.GetParent(typeof(ViewVertexArray));



                }
                if (win != null)
                {
                    if (win.ColorBitmap != null)
                    {


                        int val = (byte)e.NewValue;

                        ToolTipValue.Text = val.ToString();
                        C1.Fill = new SolidColorBrush(win.ColorBitmap.GetPixel((val * 2), 0));
                        C2.Fill = new SolidColorBrush(win.ColorBitmap.GetPixel((val * 2) + 1, 0));
                        C3.Fill = new SolidColorBrush(win.ColorBitmap.GetPixel((val * 2), 1));
                        C4.Fill = new SolidColorBrush(win.ColorBitmap.GetPixel((val * 2) + 1, 1));
                    }
                }
            }
        }
    }
}
