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
    /// Interaction logic for ViewUnitLightGroup.xaml
    /// </summary>
    public partial class ViewUnitLightGroup : UserControl
    {
        public static Dictionary<uint, string> TransitionTypes = new Dictionary<uint, string>() { { 0xf5aaf85f, "Flicker" }, { 0x9dbffe5b, "Fade" }, { 0xb6b969a4, "Flip" } };

        public ViewUnitLightGroup()
        {


            InitializeComponent();

            cbTransitionType.ItemsSource = TransitionTypes;
        }
    }
}
