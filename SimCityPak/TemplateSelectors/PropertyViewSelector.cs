using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;

namespace SimCityPak
{
    public class PropertyViewSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is PropertyModel)
            {
                Property prop = (Property)((PropertyModel)item).Value;
                object[] attributes = prop.GetType().GetCustomAttributes(typeof(PropertyDefinitionAttribute), false);

                if(attributes.Length > 0)
                {
                    PropertyDefinitionAttribute attribute = (PropertyDefinitionAttribute)attributes[0];
                    return(element.FindResource(attribute.ViewName) as DataTemplate);
                }
            }

            return null;
        }
    }

    public class PropertyEditViewSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is PropertyModel)
            {
                Property prop = (Property)((PropertyModel)item).Value;
                object[] attributes = prop.GetType().GetCustomAttributes(typeof(PropertyDefinitionAttribute), false);

                if (attributes.Length > 0)
                {
                    PropertyDefinitionAttribute attribute = (PropertyDefinitionAttribute)attributes[0];
                    return (element.FindResource(attribute.ViewName) as DataTemplate);
                }
            }

            return null;
        }
    }
}
