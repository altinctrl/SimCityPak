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
    public partial class ViewShort4N : Window
    {
        public double X { get { return dataContextValue.X; } }
        public double Y { get { return dataContextValue.Y; } }
        public double Z { get { return dataContextValue.Z; } }
        public double W { get { return dataContextValue.W; } }

        VertexShort4NValue dataContextValue = null;

        public ViewShort4N()
        {
            InitializeComponent();
        }

        public ViewShort4N(VertexShort4NValue vertexComponentValue)
        {
            dataContextValue = new VertexShort4NValue()
            { 
                X = vertexComponentValue.X,
                Y = vertexComponentValue.Y,
                Z = vertexComponentValue.Z,
                W = vertexComponentValue.W
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
