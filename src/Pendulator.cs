//******************************************************************************************
// Copyright © 2016 - 2022 Wolfgang Foerster (wolfoerster@gmx.de)
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
    using System;
    using System.ComponentModel;

    public class Pendulator
    {
        private readonly Pendulum pendulum;
        private readonly BackgroundWorker worker = new BackgroundWorker
        {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true,
        };

        public Pendulator(Pendulum pendulum)
        {
            this.pendulum = pendulum;
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        }

        public Pendulum Pendulum => pendulum;

        public bool IsBusy => worker.IsBusy;

        public int CalcsPerSecond { get; private set; }

        public double Elapsed { get; private set; }

        public Action Finished;

        public bool Start()
        {
            if (worker.IsBusy)
                return false;

            worker.RunWorkerAsync();
            return true;
        }

        public bool Stop()
        {
            if (!worker.IsBusy)
                return false;

            worker.CancelAsync();
            return true;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Elapsed = 0;
            var count = 0;
            var numSteps = 20000;
            DateTime utcThen = DateTime.UtcNow;
            var t0 = pendulum.SimulationTime;

            while (true)
            {
                if (++count % 10 == 1)
                {
                    pendulum.UpdateTrajectory();
                }

                pendulum.Move(numSteps);

                if ((DateTime.UtcNow - utcThen).TotalSeconds > 1)
                {
                    Elapsed = pendulum.SimulationTime - t0;
                    pendulum.CheckEnergy();
                    utcThen = DateTime.UtcNow;
                    CalcsPerSecond = count * numSteps;
                    count = 0;
                }

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // evtl. rueckmeldung geben
            Finished?.Invoke();
        }
    }
}
