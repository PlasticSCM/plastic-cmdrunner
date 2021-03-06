﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CmdRunnerExamples
{
    class CMboxHelper
    {
        public static void StartSimulation()
        {
            mThread = new Thread(new ThreadStart(UpdateLocal));
            mThread.Start();
        }

        public static void StopSimulation()
        {
            mStopped = true;
            mThread.Join();
        }

        private static void UpdateLocal()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            while (!mStopped)
                SampleHelper.RandomlyUpdateWorkspace(r);
        }

        private static bool mStopped;
        private static Thread mThread;
    }
}
