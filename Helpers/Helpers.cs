using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace HLA_Toolbox
{
    public class HelperFunctions
    {
        public static int Width { get; set; }
        public static int Height { get; set; }

        public static Rhino.Geometry.Mesh Remesh(IGH_GeometricGoo goo)
        {
            Guid id = goo.ReferenceID;
            var rhinoObj = new RhinoObject[] {RhinoDoc.ActiveDoc.Objects.Find(id)};
            if (rhinoObj == null) return null;

            ObjRef[] getMesh = Rhino.DocObjects.RhinoObject.GetRenderMeshesWithUpdatedTCs(rhinoObj, false, false, false, false); //hidden objects won't be ignored
            if (getMesh.Length == 0) return null;

            return getMesh[0].Mesh();
        }
        public static Rectangle3d GetBase(Rhino.Geometry.Mesh mesh)
        {
            BoundingBox box = mesh.GetBoundingBox(false);
            Rectangle3d rect = new Rectangle3d(new Plane(box.Min, Vector3d.ZAxis), box.Min, box.Max);

            //Add scale units later
            var offsetVector = new Vector3d(0, 0, -1);

            Transform transform1 = Transform.Translation(offsetVector);
            rect.Transform(transform1);

            Transform transform2 = Transform.Scale(rect.Center, 0.999);
            rect.Transform(transform2);

            return rect;
        }
        public static Point3d[] GetPoints(Rectangle3d rect, double treshold)
        {

            double deltaY = treshold / rect.Height;
            double deltaX = treshold / rect.Width;

            //resolution
            int width = (int)(Math.Ceiling(rect.Width / treshold)) + 1;
            int height = (int)(Math.Ceiling(rect.Height / treshold)) + 1;

            Width = width;

            int arrayLength = width * height;

            Point3d[] points = new Point3d[arrayLength];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Point3d pt = rect.PointAt(deltaX * j, 1 - deltaY * i);
                    int refer = (i * width) + j;
                    points[refer] = pt;
                }
            }

            return points;
        }


    }
}
