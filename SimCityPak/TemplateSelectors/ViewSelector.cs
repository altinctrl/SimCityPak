using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Gibbed.Spore.Package;

namespace SimCityPak
{
    public class ViewSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is DatabaseIndexData)
            {
                DatabaseIndexData index = item as DatabaseIndexData;

                string viewer = TGIRegistry.Instance.FileTypes.GetViewer(index.Index.TypeId);
                if (index.Index.InstanceType == (uint)PropertyFileTypeIds.DecalAtlas || index.Index.InstanceType == (uint)PropertyFileTypeIds.DecalAtlas2 || index.Index.InstanceType == (uint)PropertyFileTypeIds.DecalAtlas3)
                {
                    if (element.TryFindResource("viewDecalDictionary") != null)
                    {
                        return element.FindResource("viewDecalDictionary") as DataTemplate;
                    }
                }
                if (index.Index.InstanceType == (uint)PropertyFileTypeIds.Path)
               {
                   if (element.TryFindResource("viewPath") != null)
                  {
                      return element.FindResource("viewPath") as DataTemplate;
                   }
                }
                if (viewer != "")
                {
                    return element.FindResource(viewer) as DataTemplate;
                }
                else
                {
                    return element.FindResource("viewData") as DataTemplate;
                }
            }
            return null;
          
        }
    }
}
