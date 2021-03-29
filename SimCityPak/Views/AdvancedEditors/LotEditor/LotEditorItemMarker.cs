using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows.Data;

namespace SimCityPak.Views.AdvancedEditors
{
    public class LotEditorItemMarker : ModelVisual3D
    {
        public LotEditorItemMarker(ILotEditorItem parent)
        {
            Parent = parent;
            LotManipulator = new LotEditorItemManipulator(this);
            BindingOperations.SetBinding(
                this.LotManipulator,
                Manipulator.TargetTransformProperty,
                new Binding("Transform") { Source = this });
            this.Children.Add(LotManipulator);
        }

        public LotEditorItemManipulator LotManipulator
        { get; set; }

        public ILotEditorItem Parent { get; set; }

        public Model3D ManipulatorModel
        {
            get
            {
                return LotManipulator.Model;
            }
            set
            {
                LotManipulator.Model = value;
            }
        }
    }
}
