//******************************************************************************************
// Copyright © 2016 - 2024 Wolfgang Foerster (wolfoerster@gmx.de)
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

namespace DoublePendulum
{
    using System.Collections.Generic;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    internal static class Extensions
    {
        public static IEnumerable<T> PickUp<T>(this List<T> list)
        {
            var count = list.Count;
            for (int i = 0; i < count; ++i)
                yield return list[i];
        }

        public static Vector3D FindAnyPerpendicular(this Vector3D direction)
        {
            var unitD = direction;
            unitD.Normalize();

            var n = unitD.Cross(Math3D.UnitX);
            return n.LengthSquared > 1e-3 ? n : unitD.Cross(Math3D.UnitY);
        }
    }
}
