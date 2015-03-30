using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Audiotica.Controls.Chart
{
    namespace RayGraphComponent
    {
        /// <summary>
        ///     Interpret input data to Point objects for rendering
        /// </summary>
        internal static class DataInterpreter
        {
            /// <summary>
            ///     Convert a text to a usable data format
            /// </summary>
            /// <param name="input"></param>
            /// <returns>List of usable data</returns>
            public static List<Point> TextToPointList(string input)
            {
                throw new NotImplementedException();

                // TODO: TextToPointList
            }

            /// <summary>
            ///     Convert double array [y] to a usable data format
            /// </summary>
            /// <param name="input"></param>
            /// <returns>List of usable data</returns>
            public static List<Point> SingleDoubleArrayToPointList(double[] input)
            {
                return input.Select((t, i) => new Point(i, t)).ToList();
            }

            /// <summary>
            ///     Convert double array [x, y] to a usable data format
            /// </summary>
            /// <param name="input"></param>
            /// <returns>List of usable data</returns>
            public static List<Point> DoubleDoubleArrayToPointList(double[][] input)
            {
                return input.Select(d => new Point(d[0], d[1])).ToList();
            }
        }
    }
}