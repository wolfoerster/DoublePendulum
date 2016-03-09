//******************************************************************************************
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
using System.ComponentModel;
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

			data = new DataModel();
			data.NewPoincarePoint += Data_NewPoincarePoint;
		}
		DataModel data;
		BackgroundWorker worker;

		/// <summary>
		/// 
		/// </summary>
		public DataModel Data
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
			int count = 0;
			data.InitMove();

			while (true)
			{
				data.Move(1000);

				if (++count > 4000)//check energy and update computing time every second (nearly)
				{
					count = 0;
					data.CheckMove();
				}

				if (worker.CancellationPending == true)
				{
					e.Cancel = true;
					break;
				}
			}
		}
	}
}
