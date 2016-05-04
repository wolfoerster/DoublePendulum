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
using System.ComponentModel;
using System.Diagnostics;
using WFTools3D;

namespace DoublePendulum
{
	public class Simulator
	{
		/// <summary>
		/// 
		/// </summary>
		public Simulator()
		{
			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;

			worker.DoWork += Worker_DoWork;
			worker.ProgressChanged += Worker_ProgressChanged;

			data = new PendulumData();
			data.NewPoincarePoint += Data_NewPoincarePoint;
		}
		PendulumData data;
		BackgroundWorker worker;

		/// <summary>
		/// 
		/// </summary>
		public PendulumData Data
		{
			get { return data; }
		}

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler NewPoincarePoint;

		/// <summary>
		/// 
		/// </summary>
		void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (NewPoincarePoint != null)
				NewPoincarePoint(this, new EventArgs());
		}

		/// <summary>
		/// 
		/// </summary>
		void Data_NewPoincarePoint(object sender, EventArgs e)
		{
			worker.ReportProgress(0);
		}

		public bool Start()
		{
			worker.RunWorkerAsync();
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			worker.CancelAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsBusy
		{
			get { return worker.IsBusy; }
		}

		/// <summary>
		/// 
		/// </summary>
		void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			count = 0;
			while (true)
			{
				count++;
				data.Move(1000);

				if (worker.CancellationPending == true)
				{
					e.Cancel = true;
					break;
				}
			}
		}

		public long Count
		{
			get { return count; }
		}
		long count;
	}
}
