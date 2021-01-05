//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the DoublePendulum project which can be found on github.com
//
// DoublePendulum is free software: you can redistribute it and/or modify it under the terms 
// of the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.
// 
// DoublePendulum is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//******************************************************************************************
using System;
using System.Windows;
using System.Windows.Media;

namespace DoublePendulum
{
    public class ArcGeometry
    {
        /// <summary>
        /// Creates the specified arc.
        /// </summary>
        /// <param name="center">The center in pixel.</param>
        /// <param name="radius">The radius in pixel.</param>
        /// <param name="startAngle">The start angle in radians.</param>
        /// <param name="stopAngle">The stop angle in radians.</param>
        /// <param name="segmentLength">The length of a circle segment in pixel.</param>
        static public Geometry Create(Point center, double radius,
            double startAngle, double stopAngle,
            double segmentLength)
        {
            if (startAngle == stopAngle)
                return Geometry.Empty;

            StreamGeometry streamGeometry = new StreamGeometry();

            using (StreamGeometryContext ctx = streamGeometry.Open())
            {
                //double phi = MathUtils.NormalizeRadians(stopAngle - startAngle);//in radians
                double phi = stopAngle - startAngle;//in radians
                double totalLength = Math.Abs(phi * radius);//in DIP

                int iSteps = (int)(totalLength / segmentLength + 1.5);
                double dPhi = phi / iSteps;//in radians

                ctx.BeginFigure(GetPoint(center, radius, startAngle), false, false);

                for (int i = 0; i < iSteps; i++)
                {
                    double angle = startAngle + (i + 1) * dPhi;
                    ctx.LineTo(GetPoint(center, radius, angle), true, false);
                }
            }

            return streamGeometry;
        }

        /// <summary>
        /// Gets the point which is defined by center, radius and phi. 
        /// Per definition phi = 0 points to the right, phi = PI / 2 points to the top.
        /// </summary>
        private static Point GetPoint(Point center, double radius, double phi)
        {
            double x = radius * Math.Cos(phi);
            double y = radius * Math.Sin(phi);
            return new Point(center.X + x, center.Y - y);
        }
    }
}
