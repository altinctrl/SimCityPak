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
    public partial class ViewFloat4 : Window
    {
        public float X { get { return dataContextValue.X; } }
        public float Y { get { return dataContextValue.Y; } }
        public float Z { get { return dataContextValue.Z; } }
        public float W { get { return dataContextValue.W; } }

        VertexFloat4Value dataContextValue = null;

        public ViewFloat4()
        {
            InitializeComponent();
        }

        public ViewFloat4(VertexFloat4Value d3dColorValue)
        {
            dataContextValue = new VertexFloat4Value()
            { 
                X = d3dColorValue.X,
                Y = d3dColorValue.Y,
                Z = d3dColorValue.Z,
                W = d3dColorValue.W
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
