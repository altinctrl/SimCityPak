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
using SporeMaster.RenderWare4;

namespace SimCityPak.Views.RW4SectionViews.VertexValueViews
{
    /// <summary>
    /// Interaction logic for ViewD3DColor.xaml
    /// </summary>
    public partial class ViewD3DColor : Window
    {
        public byte A { get { return dataContextValue.A; } }
        public byte R { get { return dataContextValue.R; } }
        public byte G { get { return dataContextValue.G; } }
        public byte B { get { return dataContextValue.B; } }

        VertexD3DColorValue dataContextValue = null;

        public ViewD3DColor()
        {
            InitializeComponent();
        }

        public ViewD3DColor(VertexD3DColorValue d3dColorValue)
        {
            dataContextValue = new VertexD3DColorValue()
            { 
                A = d3dColorValue.A,
                R = d3dColorValue.R,
                G = d3dColorValue.G,
                B = d3dColorValue.B
            };
            DataContext = dataContextValue;
            InitializeComponent();
       
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
