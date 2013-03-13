using Microsoft.Kinect;
using QuadKin.Kinect;
using QuadKin.Quad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuadKin
{
    class QuadKinCom : StateClass
    {
        private static readonly int waitInterwal = 3000;
        
        private Stopwatch stopwatch = new Stopwatch();

        private static QuadKinCom quadKinCom;
        private static object syncRoot = new Object();

        private QuadKinCom()
        {
            Init();
        }

        public static QuadKinCom instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (quadKinCom == null)
                    {
                        quadKinCom = new QuadKinCom();
                    }
                    return quadKinCom;
                }
            }
        }

        private void Init()
        {
            if (KinCom.instance.State == State.Ready && QuadCom.instance.State == State.Ready)
            {
                KinCom.instance.skeletonReady += skeletonReady;
            }
            KinCom.instance.stateChanged += kinStateChanged;
            QuadCom.instance.stateChanged += quadStateChanged;
        }

        private void quadStateChanged(State state)
        {
            if (state == State.Ready && QuadCom.instance.State == State.Ready)
            {
                KinCom.instance.skeletonReady += skeletonReady;
            }
        }

        private void kinStateChanged(State state)
        {
            if (state == State.Ready && QuadCom.instance.State == State.Ready)
            {
                KinCom.instance.skeletonReady += skeletonReady;
            }
        }

        private void skeletonReady(Skeleton skel)
        {
            Command c = new Command(skel);

            if (c.valid)
            {
                switch (State)
                {
                    case State.NoConnection:
                        this.State = State.Initializing;
                        QuadCom.instance.takeOff();
                        this.stopwatch.Restart();
                        break;
                    case State.Initializing:
                        QuadCom.instance.sendNullCommand();
                        long timeLeft = waitInterwal - this.stopwatch.ElapsedMilliseconds;
                        if (timeLeft <= 0)
                        {
                            this.State = State.Ready;
                            this.stopwatch.Reset();
                        }
                        break;
                    case State.Ready:
                        QuadCom.instance.sendCommand(c);
                        break;
                }
            }
            else
            {
                this.State = State.NoConnection;
            }
        }
    }
}
