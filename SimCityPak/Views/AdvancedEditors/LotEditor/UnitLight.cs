using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using HelixToolkit.Wpf;
using Gibbed.Spore.Properties;

namespace SimCityPak.Views.AdvancedEditors
{
    public class UnitLight : PropertyFileObject, ILotEditorItem
    {
        public event EventHandler LightTypeChanged;

        public void OnLightTypeChanged(object sender, EventArgs data)
        {
            if (LightTypeChanged != null)
            {
                LightTypeChanged(this, data);
            }
        }

        public LotEditorItemMarker ModelRepresentation
        { get; set; }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightIDs)]
        public KeyProperty LightId { get; set; }
        public uint LightIdProperty
        { get { return LightId.InstanceId; } set { LightId.InstanceId = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightTypes)]
        public KeyProperty LightType
        { get; set; }
        public uint LightTypeProperty
        { get { return LightType.InstanceId; } set { LightType.InstanceId = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightTransforms)]
        public TransformProperty LightTransform
        { get; set; }
        
        public ColorRGBProperty _lightColor;
        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightColors)]
        public ColorRGBProperty LightColor
        { get { return _lightColor; } set { _lightColor = value; OnPropertyChanged("LightColor"); } }

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightRadii)]
        public FloatProperty LightOuterRadius
        { get; set; }
        public float LightOuterRadiusProperty
        { get { return LightOuterRadius.Value; } set { LightOuterRadius.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightInnerRadii)]
        public FloatProperty LightInnerRadius
        { get; set; }
        public float LightInnerRadiusProperty
        { get { return LightInnerRadius.Value; } set { LightInnerRadius.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightDiffuseLevels)]
        public FloatProperty LightDiffuseLevel
        { get; set; }
        public float LightDiffuseLevelProperty
        { get { return LightDiffuseLevel.Value; } set { LightDiffuseLevel.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightDebugNames)]
        public String8Property LightDebugName { get; set; }
        public string LightDebugNameProperty
        { get { return LightDebugName.Value; } set { LightDebugName.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightSpecLevels)]
        public FloatProperty LightSpecLevels
        { get; set; }
        public float LightSpecLevelsProperty
        { get { return LightSpecLevels.Value; } set { LightSpecLevels.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightLength)]
        public FloatProperty LightLength
        { get; set; }
        public float LightLengthProperty
        { get { return LightLength.Value; } set { LightLength.Value = value; } } 


        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightCullDistance)]
        public KeyProperty LightCullDistance
        { get; set; }
        public uint LightCullDistanceProperty
        { get { return LightCullDistance.InstanceId; } set { LightCullDistance.InstanceId = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightFalloffStart)]
        public FloatProperty LightFalloffStart
        { get; set; }
        public float LightFalloffStartProperty
        { get { return LightFalloffStart.Value; } set { LightFalloffStart.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightIsVolumetric)]
        public BoolProperty IsVolumetric
        { get; set; }
        public bool IsVolumetricProperty
        { get { return IsVolumetric.Value; } set { IsVolumetric.Value = value; } } 

        [PropertyFilePropertyAttribute(PropertyID = PropertyConstants.scLightVolStrength)]
        public FloatProperty LightVolStrength
        { get; set; }
        public float LightVolStrengthProperty
        { get { return LightVolStrength.Value; } set { LightVolStrength.Value = value; } } 

        public void UpdateTransform()
        {
            if (ModelRepresentation != null)
            {
                if (ViewLotEditor.LightTypes[this.LightType.InstanceId] == "Spot")
                {
                    Transform3DGroup transforms = new Transform3DGroup();
                    transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)));
                    transforms.Children.Add(new MatrixTransform3D(this.ModelRepresentation.Transform.Value));

                    LightTransform.SetMatrix(transforms.Value);
                }
                else if (ViewLotEditor.LightTypes[this.LightType.InstanceId] == "Point")
                {
                    LightTransform.SetMatrix(this.ModelRepresentation.Transform.Value);
                }
                else if (ViewLotEditor.LightTypes[this.LightType.InstanceId] == "Line")
                {
                    Transform3DGroup transforms = new Transform3DGroup();

                    transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));
                    transforms.Children.Add(new MatrixTransform3D(this.ModelRepresentation.Transform.Value));


                    LightTransform.SetMatrix(transforms.Value);
                }
            }

        }

        public void UpdateGeometry()
        {
            this.UpdateTransform();
            this.CreateGeometry();
        }

        public LotEditorItemMarker CreateGeometry()
        {
            LotEditorItemMarker marker;
            if (this.ModelRepresentation == null)
            {
                marker = new LotEditorItemMarker(this);
            }
            else
            {
                marker = this.ModelRepresentation;
            }
           
           if (ViewLotEditor.LightTypes[LightType.InstanceId] == "Spot")
                {
                    
                    TruncatedConeVisual3D cone = new TruncatedConeVisual3D();
                    cone.Height = LightLength.Value;
                    cone.BaseRadius = 0;
                    cone.TopRadius = LightOuterRadius.Value;
                    Transform3DGroup transforms = new Transform3DGroup();
                    transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));
                    Transform3D coneTransform = new MatrixTransform3D(LightTransform.GetAsMatrix3D());
                    transforms.Children.Add(coneTransform);
                   // cone.Transform = transforms;

                    cone.Material = MaterialHelper.CreateMaterial(Brushes.Black, new SolidColorBrush(LightColor.Color), opacity: 0.6);
                    //ModelRepresentation.Model = cone.Model;

                    marker.Transform = transforms;
                    marker.ManipulatorModel = cone.Model;
                    ModelRepresentation = marker;
                    return marker;
                }
            else if (ViewLotEditor.LightTypes[LightType.InstanceId] == "Point")
                {
                    HelixToolkit.Wpf.SphereVisual3D sphere = new HelixToolkit.Wpf.SphereVisual3D();
                    sphere.Radius = 1;
                    
                    sphere.Material = MaterialHelper.CreateMaterial(Brushes.Black, new SolidColorBrush(LightColor.Color));

                    marker.Transform = new MatrixTransform3D(LightTransform.GetAsMatrix3D());
                    marker.ManipulatorModel = sphere.Model;
                     ModelRepresentation = marker;
                    return marker;
                }
            else if (ViewLotEditor.LightTypes[LightType.InstanceId] == "Line")
                {
                    BoxVisual3D lineLight = new BoxVisual3D();
                    lineLight.Height = LightLength.Value;

                    lineLight.Center = new Point3D(0, 0, -(LightLength.Value / 2));

                    Transform3DGroup transforms = new Transform3DGroup();
                    // transforms.Children.Add(new TranslateTransform3D(0, 0, -(light.LightLength / 2)));
                    transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)));
                    transforms.Children.Add(new MatrixTransform3D(LightTransform.GetAsMatrix3D()));

                    lineLight.Material = MaterialHelper.CreateMaterial(Brushes.Black, new SolidColorBrush(LightColor.Color));

                    marker.Transform = transforms;
                    marker.ManipulatorModel = lineLight.Model;
                    ModelRepresentation = marker;
                    return marker;
                }

            return new LotEditorItemMarker(this);
        }
    }

    public class LightGroup
    {
        public uint GroupContainer
        { get; set; }

        public bool HasTransitionType
        {
            get
            {
                return TransitionType.HasValue;
            }
            set
            {
                if (value)
                {
                    TransitionType = 0;
                }
                else
                {
                    TransitionType = null;
                }
            }
        }

        public uint? TransitionType
        { get; set; }

        public float? TransitionOnDuration
        { get; set; }

        public float? TransitionOffDuration
        { get; set; }

        public uint? TransitionOnAudio
        { get; set; }

        public uint? TransitionOffAudio
        { get; set; }

        public bool HasTimeRange
        {
            get
            {
                return TimeRangeID.HasValue;
            }
            set
            {
                if (value)
                {
                    TimeRangeID = 0;
                }
                else
                {
                    TimeRangeID = null;
                }
            }
        }

        public uint? TimeRangeID
        { get; set; }

        public float? TimeRangeBegin
        { get; set; }

        public float? TimeRangeEnd
        { get; set; }

        public float? TimeRangeVaryBegin
        { get; set; }

        public float? TimeRangeVaryEnd
        { get; set; }

        public bool HasFlags
        {
            get
            {
                return UnitControlFlagIndex.HasValue;
            }
            set
            {
                if (value)
                {
                    UnitControlFlagIndex = 0;
                }
                else
                {
                    UnitControlFlagIndex = null;
                }
            }
        }

        public int? UnitControlFlagIndex
        { get; set; }

        public bool? InvertFlag
        { get; set; }

        public uint? ResourceBinId
        { get; set; }

        public float? ResourceBinThreshold
        { get; set; }
    }



}
