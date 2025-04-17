using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using Grasshopper.Kernel.Types;
using static HLA_Toolbox.HelperFunctions;
using static HLA_Toolbox.RayTracing;
using static HLA_Toolbox.Hydrology;
using DHARTAPI.NativeUtils;
using Rhino.UI;
using System.Collections;

namespace HLA_Toolbox.Components
{
    public class FlowLines : GH_Component
    {
        public IGH_GeometricGoo goo;
        public double size;
        public double treshold;
        public double[] remappedArray;
        Hydrology flow;
        
        /// <summary>
        /// Initializes a new instance of the Grid class.
        /// </summary>
        public FlowLines()
          : base("FlowLines", "FlowLines",
              "Component to calculate flow directions and flow accumulation and drawing flow lines based on them.",
              "HLA Toolbox", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Terrain", "Terrain", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Treshold", "Treshold", "", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Mesh", "Mesh", "", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Rectangle", "Rectangle", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "Points", "", GH_ParamAccess.list);
            pManager.AddPointParameter("RayPoints", "RayPoints", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("FlowDir", "FlowDir", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("NIDP", "NIDP", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("FlowAccu", "FlowAccu", "", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Lines", "Lines", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Keys", "Keys", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //Inputs
            DA.GetData(0, ref goo);
            DA.GetData(1, ref size);
            DA.GetData(2, ref treshold);

            //HelperFunctions helper = new HelperFunctions();
            var mesh = Remesh(goo);
            if (mesh == null) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Geometry is not valid. Please input a Surface, Brep or Mesh.");
            var rect = GetBase(mesh);
            var points = GetPoints(rect, size);
            if (size <= 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Grid size should be bigger than 0.");

            var rayPoints = HitPoints(points, mesh);

            flow = new Hydrology();
            flow.Calculate(rayPoints, size);
            var flowDir = flow.FlowDir;
            var NIDP = flow.NIDP;
            var flowAccu = flow.FlowAccu;
            var lines = flow.Lines.Values;
            var keys1 = flow.Lines.Keys;

            double minValue = flowAccu.Min();
            double maxValue = flowAccu.Max();

            remappedArray = new double[flowAccu.Length];
            for (int i = 0; i < flowAccu.Length; i++)
            {
                double remappedValue = ((flowAccu[i] - minValue) / (maxValue - minValue)) * 100;
                remappedArray[i] = remappedValue;
            }

            //Output
            //DA.SetData(0, mesh);
            //DA.SetData(1, rect);
            //DA.SetDataList(2, points);
            //DA.SetDataList(3, rayPoints);
            DA.SetDataList(4, flowDir);
            DA.SetDataList(5, NIDP);
            DA.SetDataList(6, flowAccu);
            //DA.SetDataList(7, lines);
            DA.SetDataList(8, remappedArray);
        }

        
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (flow != null)
            {
                double pixel;
                Point3d point = Point3d.Unset;
                args.Viewport.GetWorldToScreenScale(point, out pixel);

                for (int i = 0; i < flow.FlowAccu.Length; i++)
                {
                    if (flow.Lines.ContainsKey(i) && flow.FlowAccu[i]>=treshold)
                        //args.Display.DrawLine(flow.Lines[i], System.Drawing.Color.Blue, (int)(Math.Sqrt(pixel) + flow.FlowAccu[i]));
                        //args.Display.DrawLine(flow.Lines[i], System.Drawing.Color.Blue, (int)(Math.Sqrt(pixel/50) + remappedArray[i]));
                        args.Display.DrawLine(flow.Lines[i], System.Drawing.Color.Blue, (int)(Math.Sqrt((pixel/50 + remappedArray[i]))));
                }

                base.DrawViewportWires(args);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AF99A4A2-DA78-47A2-AE3E-45B0C756F06E"); }
        }
    }
}