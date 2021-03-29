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
    public partial class ViewUByte4 : Window
    {
        public byte X { get { return dataContextValue.X; } }
        public byte Y { get { return dataContextValue.Y; } }
        public byte Z { get { return dataContextValue.Z; } }
        public byte W { get { return dataContextValue.W; } }

        VertexUByte4Value dataContextValue = null;

        public ViewUByte4()
        {
            InitializeComponent();
        }

        public ViewUByte4(VertexUByte4Value d3dColorValue)
        {
            dataContextValue = new VertexUByte4Value()
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
