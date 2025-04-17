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
using static HLA_Toolbox.HelperFunctions;
using System.Drawing;

namespace HLA_Toolbox
{
    public class Hydrology
    {
        public int[] FlowDir { get; set; }
        public int[] NIDP { get; set; }
        public int[] FlowAccu { get; set; }
        public int[] SlopeIndex { get; set; }
        public Dictionary<int, Line> Lines { get; set; }

        public void Calculate(Point3d[] pointArray, double size)
        {
            double deltaX = size;
            double deltaY = size;
            double deltaXY = Math.Sqrt(2 * Math.Pow(deltaX, 2));

            double maxDistance = 2*size; // Necessary for checks around the borders

            //Transfer the witdh value from Helpers class in a different way?
            int xStride = HelperFunctions.Width;

            int[] slopeIndex = new int[pointArray.Length];
            int[] flowDir = new int[pointArray.Length];
            int[] nidp = new int[pointArray.Length];

            for (int i = 0; i < pointArray.Length; i++)
            {
                if (pointArray[i] == null) continue;

                double maxSlope = 0.0;
                double _slope = 0.0;

                //int i = Endvertex;
                int maxSlopeIndex = i;
                int finalDir = 0;

                int _sw = i - xStride - 1;//SW pixel
                int sw = 32;
                int _s = i - xStride; //S pixel
                int s = 64;
                int _se = i - xStride + 1; //SE pixel
                int se = 128;
                int _w = i - 1; //W pixel
                int w = 16;
                int _e = i + 1; //E pixel
                int e = 1;
                int _nw = i + xStride - 1; //NW pixel
                int nw = 8;
                int _n = i + xStride; //N pixel
                int n = 4;
                int _ne = i + xStride + 1; //NE pixel
                int ne = 2;

                if (_sw >= 0)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_sw].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_sw].Z) / deltaXY;
                        CheckForMaxSlope(_slope, ref maxSlope, _sw, ref maxSlopeIndex, sw, ref finalDir);
                    }
                }

                if (_s >= 0)
                {
                    _slope = (pointArray[i].Z - pointArray[_s].Z) / deltaY;
                    CheckForMaxSlope(_slope, ref maxSlope, _s, ref maxSlopeIndex, s, ref finalDir);
                }

                if (_se >= 0)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_se].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_se].Z) / deltaXY;
                        CheckForMaxSlope(_slope, ref maxSlope, _se, ref maxSlopeIndex, se, ref finalDir);

                    }
                }

                if (_w >= 0)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_w].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_w].Z) / deltaX;
                        CheckForMaxSlope(_slope, ref maxSlope, _w, ref maxSlopeIndex, w, ref finalDir);
                    }
                }

                if (_e <= pointArray.Length - 1)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_e].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_e].Z) / deltaX;
                        CheckForMaxSlope(_slope, ref maxSlope, _e, ref maxSlopeIndex, e, ref finalDir);
                    }
                }

                if (_nw <= pointArray.Length - 1)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_nw].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_nw].Z) / deltaXY;
                        CheckForMaxSlope(_slope, ref maxSlope, _nw, ref maxSlopeIndex, nw, ref finalDir);
                    }
                }

                if (_n <= pointArray.Length - 1)
                {
                    _slope = (pointArray[i].Z - pointArray[_n].Z) / deltaY;
                    CheckForMaxSlope(_slope, ref maxSlope, _n, ref maxSlopeIndex, n, ref finalDir);
                }

                if (_ne <= pointArray.Length - 1)
                {
                    deltaX = Math.Abs(pointArray[i].X - pointArray[_ne].X);

                    if (deltaX < maxDistance)
                    {
                        _slope = (pointArray[i].Z - pointArray[_ne].Z) / deltaXY;
                        CheckForMaxSlope(_slope, ref maxSlope, _ne, ref maxSlopeIndex, ne, ref finalDir);
                    }
                }

                if (maxSlope > 0)
                {
                    slopeIndex[i] = maxSlopeIndex;
                    flowDir[i] = finalDir;
                    nidp[maxSlopeIndex]++;
                }
            }

            FlowDir = flowDir;
            NIDP = nidp;
            SlopeIndex = slopeIndex;


            int[] flowAccu = new int[flowDir.Length];
            for (int i = 0; i < flowAccu.Length; i++) flowAccu[i] = 1;
            Lines = new Dictionary<int, Line>();

            for (int i = 0; i < flowAccu.Length; i++)
            {
                if (nidp[i] != 0) continue;
                int nAccu = 1;
                int c = i;

                for (int j = 0; j < flowAccu.Length; j++)
                {
                    int n = SlopeIndex[c];
                    Line line = new Line(pointArray[c], pointArray[n]);
                    if (n > 0 && n < flowAccu.Length)
                    {
                        Lines.Add(c, line);
                        flowAccu[n] += nAccu;
                        nAccu = flowAccu[n];
                        if (nidp[n] >= 2)
                        {
                            nidp[n]--;
                            break;
                        }
                        c = n;
                    }
                    else break;
                }
            }
            for (int i = 0; i < flowAccu.Length; i++) flowAccu[i]--;

            FlowAccu = flowAccu;


        }

        //public List<Line> drawLines(Point3d[] pointArray, int[] flowDir, int[] nidp)
        //{
        //    int[] flowAccu = new int[flowDir.Length];
        //    for (int i = 0; i < flowAccu.Length; i++) flowAccu[i] = 1;
        //    List<Line> lines = new List<Line>();    

        //    for (int i = 0; i < flowAccu.Length; i++)
        //    {
        //        if (nidp[i] != 0) continue;
        //        int nAccu = 1;
        //        int c = i;

        //        for (int j = 0; j < flowAccu.Length; j++)
        //        {
        //            int n = SlopeIndex[c];
        //            Line line = new Line(pointArray[c], pointArray[n]);
        //            if (n > 0 && n < flowAccu.Length)
        //            {
        //                lines.Add(line);
        //                flowAccu[n] += nAccu;
        //                nAccu = flowAccu[n];
        //                if (nidp[n] >= 2)
        //                {
        //                    nidp[n]--;
        //                    break;
        //                }
        //                c = n;
        //            }
        //            else break;
        //        }
        //    }
        //    for (int i = 0; i < flowAccu.Length; i++) flowAccu[i]--;

        //    FlowAccu = flowAccu;

        //    return lines;
        //}
            public static void CheckForMaxSlope(double currentSlope, ref double maxSlope, int currentIndex, ref int maxSlopeIndex, int currentDir, ref int finalDir)
        {
            if (currentSlope > maxSlope)
            {
                maxSlope = currentSlope;
                maxSlopeIndex = currentIndex;
                finalDir = currentDir;
            }
        }


    }
}
