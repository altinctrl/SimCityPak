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
    /// Interaction logic for ViewUnitLight.xaml
    /// </summary>
    public partial class ViewUnitLight : UserControl
    {
        public ViewUnitLight()
        {
            InitializeComponent();

            cbLightType.ItemsSource = ViewLotEditor.LightTypes;
            cbCullDistance.ItemsSource = ViewLotEditor.LightCullDistances;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            UnitLight light = ((UnitLight)this.DataContext);
            dlg.InitialColor = light.LightColor.Color;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                light.LightColor.Color = dlg.SelectedColor;
            }

            var expression = rectangle1.GetBindingExpression(Border.BackgroundProperty);
             if (expression != null) expression.UpdateTarget();

            light.UpdateGeometry();

        }

        private void tbLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.DataContext is UnitLight)
            {
                UnitLight light = ((UnitLight)this.DataContext);

               light.UpdateGeometry();
            }
        }

        private void cbLightType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is UnitLight)
            {
                UnitLight light = ((UnitLight)this.DataContext);

          
               // light.ModelRepresentation = light.CreateGeometry();

                light.OnLightTypeChanged(light, new EventArgs());
               
            }
        }
    }
}
