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

namespace SimCityPak.Views.AdvancedEditors.LotEditor
{
    /// <summary>
    /// Interaction logic for ViewUnitDecal.xaml
    /// </summary>
    public partial class ViewUnitEffect : UserControl
    {
        //public static Dictionary<int, string> Categories = new Dictionary<int,string>() { { 0, "Default" }, {1, "Vandalism"}, {2, "Vacant"} };
        //public static Dictionary<uint, string> RenderGroups = new Dictionary<uint, string>() { { 0x2ea8fb98, "Default" }, { 0x96b84350, "Ground" } };

        public ViewUnitEffect()
        {
            InitializeComponent();

            //cbDecalCategory.ItemsSource = Categories;
            //cbRenderGroup.ItemsSource = RenderGroups;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
           // UnitDecal decal = (UnitDecal)this.DataContext;
//decal.Transform.
        }
    }
}
