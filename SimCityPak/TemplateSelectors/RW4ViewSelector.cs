using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using SporeMaster.RenderWare4;

namespace SimCityPak
{
    public class RW4ViewSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is RW4ModelSectionView)
            {

                RW4ModelSectionView sectionView = item as RW4ModelSectionView;

                switch (sectionView.Section.TypeCode)
                {
                    case SectionTypeCodes.Texture: return element.FindResource("viewTexture") as DataTemplate;
                    case SectionTypeCodes.Mesh: return element.FindResource("viewMesh") as DataTemplate;
                    case SectionTypeCodes.VertexArray: return element.FindResource("viewVertexArray") as DataTemplate;
                    case SectionTypeCodes.VertexFormat: return element.FindResource("viewVertexFormat") as DataTemplate;
                    case SectionTypeCodes.Material: return element.FindResource("viewMaterial") as DataTemplate; 
                    default: return element.FindResource("viewData") as DataTemplate;
                }
            }

            return null;
        }
    }
}
