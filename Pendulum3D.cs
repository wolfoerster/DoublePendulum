﻿//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the EquationOfTime project which can be found on github.com
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
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using WFTools3D;

namespace DoublePendulum
{
	public class Pendulum3D : Object3D
	{
		public Pendulum3D(Brush brush1, Brush brush2)
		{
			MaterialGroup material = new MaterialGroup();
			material.Children.Add(new DiffuseMaterial(brush1));
			material.Children.Add(new SpecularMaterial(Brushes.Gray, 100));

			Cylinder ground = new Cylinder { Radius = 0.5, ScaleZ = 0.1, IsClosed = true };
			ground.Position = new Point3D(0, 0, -ground.ScaleZ);
			ground.DiffuseMaterial.Brush = brush2;
			Children.Add(ground);

			Cylinder mast = new Cylinder(4) { ScaleX = 0.1, ScaleY = 0.2, ScaleZ = 2.2, IsClosed = true, Material = material };
			Children.Add(mast);

			arm = new Arm(material);
			arm.Position = new Point3D(0.3, 0, 2);
			Children.Add(arm);
		}
		Arm arm;
		static double r = 0.03;

		/// <summary>
		/// 
		/// </summary>
		public DataModel Data { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			arm.Rotation1 = Math3D.RotationX(Data.Q1, false);
			arm.ForeArm.Rotation1 = Math3D.RotationX(Data.Q2 - Data.Q1, false);
		}

		/// <summary>
		/// 
		/// </summary>
		class Arm : Object3D
		{
			public LowerArm ForeArm;

			public Arm(MaterialGroup material)
			{
				UpperArm upperArm = new UpperArm(material);
				Children.Add(upperArm);

				ForeArm = new LowerArm(material);
				ForeArm.Position = new Point3D(0, 0, -1);
				Children.Add(ForeArm);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		class UpperArm : Object3D
		{
			public UpperArm(MaterialGroup material)
			{
				Cylinder axis = new Cylinder { Radius = r, IsClosed = true, Material = material };
				axis.From = new Point3D(-0.6, 0, 0);
				axis.To = new Point3D(0.21, 0, 0);
				Children.Add(axis);

				Sphere shoulder = new Sphere { Radius = 0.2, Material = material };
				shoulder.Position = new Point3D(-0.6, 0, 0);
				Children.Add(shoulder);

				ArmBone bone1 = new ArmBone(material);
				bone1.Position = new Point3D(-0.15, 0, 0);
				Children.Add(bone1);

				ArmBone bone2 = new ArmBone(material);
				bone2.Position = new Point3D(0.15, 0, 0);
				Children.Add(bone2);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		class ArmBone : Object3D
		{
			public ArmBone(MaterialGroup material)
			{
				Cylinder disk1 = new Cylinder { Radius = 0.1, IsClosed = true, Material = material };
				disk1.From = new Point3D(-r, 0, 0);
				disk1.To = new Point3D(r, 0, 0);
				Children.Add(disk1);

				Cylinder stick = new Cylinder { Radius = r, IsClosed = true, Material = material };
				stick.From = new Point3D(0, 0, 0);
				stick.To = new Point3D(0, 0, -1);
				Children.Add(stick);

				Cylinder disk2 = new Cylinder { Radius = 0.05, IsClosed = true, Material = material };
				disk2.From = new Point3D(-r, 0, -1);
				disk2.To = new Point3D(r, 0, -1);
				Children.Add(disk2);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		class LowerArm : Object3D
		{
			public LowerArm(MaterialGroup material)
			{
				Cylinder axis = new Cylinder { Radius = r, IsClosed = true, Material = material };
				axis.From = new Point3D(-0.2, 0, 0);
				axis.To = new Point3D(0.2, 0, 0);
				Children.Add(axis);

				Cylinder disk = new Cylinder { Radius = 0.1, IsClosed = true, Material = material };
				disk.From = new Point3D(-r, 0, 0);
				disk.To = new Point3D(r, 0, 0);
				Children.Add(disk);

				Sphere ball = new Sphere { Radius = 0.1, Material = material };
				ball.Position = new Point3D(0, 0, ball.Radius + r - 1);
				Children.Add(ball);

				Cylinder stick = new Cylinder { Radius = r, IsClosed = true, Material = material };
				stick.From = new Point3D(0, 0, 0);
				stick.To = ball.Position;
				Children.Add(stick);
			}
		}
	}
}
