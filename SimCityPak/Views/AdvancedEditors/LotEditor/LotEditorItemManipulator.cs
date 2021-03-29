using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows;

namespace SimCityPak.Views.AdvancedEditors
{
    public class LotEditorItemManipulator : Manipulator
    {
        public LotEditorItemManipulator(LotEditorItemMarker parent)
        {
            Parent = parent;
        }

public LotEditorItemMarker Parent { get; set; }

        private Point3D? GetNearestPoint(Point position, Point3D hitPlaneOrigin, Vector3D hitPlaneNormal)
        {
            var hpp = this.GetHitPlanePoint(position, hitPlaneOrigin, this.Direction);
            if (hpp == null)
            {
                return null;
            }

            // var ray = new Ray3D(this.ToWorld(this.Position), this.ToWorld(this.Direction));
            return hpp.Value; //ray.GetNearest(hpp.Value);
        }

        
        

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var direction = this.ToWorld(this.Direction);

            var up = Vector3D.CrossProduct(this.Camera.LookDirection, direction);
            var hitPlaneOrigin = this.ToWorld(this.Position);
            this.HitPlaneNormal = Vector3D.CrossProduct(up, direction);
            var p = e.GetPosition(this.ParentViewport);

            var np = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
            if (np == null)
            {
                return;
            }

            var lp = this.ToLocal(np.Value);

            this.lastPoint = lp;
            this.CaptureMouse();
        }

        private Vector3D Direction
        {
            get
            {

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    return new Vector3D(1, 0, 0);
                }
                else
                {
                    return new Vector3D(0, 0, 1);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            base.OnMouseMove(e);
            if (this.IsMouseCaptured)
            {

                var hitPlaneOrigin = this.ToWorld(this.Position);
                var p = e.GetPosition(this.ParentViewport);
                var nearestPoint = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
                if (nearestPoint == null)
                {
                    return;
                }

                var delta = this.ToLocal(nearestPoint.Value) - this.lastPoint;
                this.Value += Vector3D.DotProduct(delta, this.Direction);

                if (this.TargetTransform != null)
                {
                    var translateTransform = new TranslateTransform3D(delta);
                    this.TargetTransform = Transform3DHelper.CombineTransform(translateTransform, this.TargetTransform);
                }
                else
                {
                    //  this.Position += delta;
                    var transform = new TranslateTransform3D(delta);
                    this.Transform = Transform3DHelper.CombineTransform(this.Transform, transform);
                }

                nearestPoint = this.GetNearestPoint(p, hitPlaneOrigin, this.HitPlaneNormal);
                if (nearestPoint != null)
                {
                    this.lastPoint = this.ToLocal(nearestPoint.Value);
                }
            }
        }

        private Point3D lastPoint;

        new public Model3D Model
        {
            get
            {
                return this.Visual3DModel;
            }
            set
            {
                this.Visual3DModel = value;
            }
        }




        protected override void OnGeometryChanged()
        {
            throw new NotImplementedException();
        }
    }
}
