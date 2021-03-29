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
using SporeMaster.RenderWare4;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewVertexFormat.xaml
    /// </summary>
    public partial class ViewVertexFormat : UserControl
    {
        public ViewVertexFormat()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext.GetType() == typeof(RW4Section))
            {
                RW4Section section = this.DataContext as RW4Section;
                SporeMaster.RenderWare4.VertexFormat vertexFormatSection = section.obj as SporeMaster.RenderWare4.VertexFormat;

                dataGridVertices.ItemsSource = vertexFormatSection.VertexElements;
            }
        }
    }
}
