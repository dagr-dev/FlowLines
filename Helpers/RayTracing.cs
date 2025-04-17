using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using DHARTAPI.Geometry;
using DHARTAPI.RayTracing;
using System.Threading;
using System.Diagnostics;
using Rhino.Render.ChangeQueue;
using System.Threading.Tasks;
using Rhino.Commands;
using DHARTAPI.SpatialStructures;
using System.Linq;

namespace HLA_Toolbox
{
    public class RayTracing
    {
        public static bool[] Hits(Rhino.Geometry.Mesh analysisMesh, Rhino.Geometry.Mesh contextMesh)
        {
            MeshInfo _contextMesh = new MeshInfo(contextMesh.Faces.ToIntArray(true), contextMesh.Vertices.ToFloatArray());
            EmbreeBVH bvh = new EmbreeBVH(_contextMesh);

            // Deconstruct to individual float coordinates
            float[] analysisPoints = new float[analysisMesh.Vertices.Count * 3];

            for (int i = 0; i < analysisMesh.Vertices.Count; i++)
            {
                analysisPoints[i * 3] = analysisMesh.Vertices[i].X;
                analysisPoints[i * 3 + 1] = analysisMesh.Vertices[i].Y;
                analysisPoints[i * 3 + 2] = analysisMesh.Vertices[i].Z;
            }

            //One direction
            float[] direction_vector = new float[] { 0, 0, 1 };

            List<bool[]> hitResults = new List<bool[]>();

            var sw = new Stopwatch();
            sw.Start();

            hitResults.Add(EmbreeRaytracer.IntersectOccluded(bvh, analysisPoints, direction_vector));

            LogTime(ref sw, "Ray casting");

            bool[] hits = new bool[hitResults.Count * hitResults[0].Length];

            int index = 0;
            for (int i = 0; i < hitResults.Count; i++)
            {
                for (int j = 0; j < hitResults[i].Length; j++) hits[index++] = hitResults[i][j];
            }

            return hits;

        }
        public static float[] Distances(Rhino.Geometry.Mesh analysisMesh, Rhino.Geometry.Mesh contextMesh)
        {

            MeshInfo _contextMesh = new MeshInfo(contextMesh.Faces.ToIntArray(true), contextMesh.Vertices.ToFloatArray());
            EmbreeBVH bvh = new EmbreeBVH(_contextMesh);

            var sw = new Stopwatch();
            sw.Start();

            var analysisPoints = new DHARTAPI.Vector3D[analysisMesh.Vertices.Count];
            for (int i = 0; i < analysisMesh.Vertices.Count; i++) 
            analysisPoints[i] = new DHARTAPI.Vector3D(analysisMesh.Vertices[i].X, analysisMesh.Vertices[i].Y, analysisMesh.Vertices[i].Z);

            LogTime(ref sw, "Convert points");

            var direction_vector = new DHARTAPI.Vector3D(0, 0, 1);

            float[] hitDistances = new float[analysisPoints.Length];

            Parallel.For(0, analysisPoints.Length, i =>         // Iterate over all 
            {
                hitDistances[i] = EmbreeRaytracer.IntersectForDistance(bvh, analysisPoints[i], direction_vector).distance;
                //EmbreeRaytracer.IntersectForPoint()
            });

            LogTime(ref sw, "Ray casting");

            return hitDistances;
        }
        public static float[] Distances(Point3d[] points, Rhino.Geometry.Mesh contextMesh, out int hitsNum)
        {
            int hits = 0;
            MeshInfo _contextMesh = new MeshInfo(contextMesh.Faces.ToIntArray(true), contextMesh.Vertices.ToFloatArray());
            EmbreeBVH bvh = new EmbreeBVH(_contextMesh);

            //var sw = new Stopwatch();
            //sw.Start();

            var analysisPoints = new DHARTAPI.Vector3D[points.Length];
            for (int i = 0; i < points.Length; i++)
                analysisPoints[i] = new DHARTAPI.Vector3D((float)points[i].X, (float)points[i].Y, (float)points[i].Z);

            //LogTime(ref sw, "Convert points");

            var direction_vector = new DHARTAPI.Vector3D(0, 0, 1);

            float[] hitDistances = new float[points.Length];

            Parallel.For(0, points.Length, i =>         // Iterate over all 
            {
                hitDistances[i] = EmbreeRaytracer.IntersectForDistance(bvh, analysisPoints[i], direction_vector).distance;
                if (hitDistances[i] != 0) hits++;
            });

            hitsNum = hits;

            //LogTime(ref sw, "Ray casting");

            return hitDistances;
        }
        public static Point3d[] HitPoints(Point3d[] points, Rhino.Geometry.Mesh contextMesh)
        {
            Point3d[] points_ = new Point3d[points.Length];
            MeshInfo _contextMesh = new MeshInfo(contextMesh.Faces.ToIntArray(true), contextMesh.Vertices.ToFloatArray());
            EmbreeBVH bvh = new EmbreeBVH(_contextMesh);

            //var sw = new Stopwatch();
            //sw.Start();

            var analysisPoints = new DHARTAPI.Vector3D[points.Length];
            for (int i = 0; i < points.Length; i++)
                analysisPoints[i] = new DHARTAPI.Vector3D((float)points[i].X, (float)points[i].Y, (float)points[i].Z);

            //LogTime(ref sw, "Convert points");

            var direction_vector = new DHARTAPI.Vector3D(0, 0, 1);

            //float[] hitDistances = new float[points.Length];
            DHARTAPI.Vector3D[] hitPoints = new DHARTAPI.Vector3D[points.Length];

            Parallel.For(0, points.Length, i =>         // Iterate over all 
            {
                //hitDistances[i] = EmbreeRaytracer.IntersectForDistance(bvh, analysisPoints[i], direction_vector).distance;
                hitPoints[i] = EmbreeRaytracer.IntersectForPoint(bvh, analysisPoints[i], direction_vector);
                points_[i] = new Point3d(hitPoints[i].x, hitPoints[i].y, hitPoints[i].z); ;
            });

            //LogTime(ref sw, "Ray casting");

            return points_;
        }


        public static void LogTime(ref Stopwatch sw, string text)
        {
            sw.Stop();
            Rhino.RhinoApp.WriteLine($"{text}: {sw.ElapsedMilliseconds} ms");
            sw.Restart();
        }

    }
}
