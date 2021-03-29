using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Gibbed.Spore.Package;
using SimCityPak.Views.AdvancedEditors;

namespace SimCityPak
{
    public class LotEditorViewSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item is UnitLight)
                {
                    return element.FindResource("viewLight") as DataTemplate;
                }
                if (item is UnitBinDrawSlot)
                {
                    return element.FindResource("viewBinDraw") as DataTemplate;
                }
                if (item is UnitDecal)
                {
                    return element.FindResource("viewDecal") as DataTemplate;
                }
                if (item is LightGroup)
                {
                    return element.FindResource("viewLightGroup") as DataTemplate;
                }
                if (item is UnitEffect)
                {
                    return element.FindResource("viewUnitEffect") as DataTemplate;
                }
            }
            return null;
          
        }
    }
}
