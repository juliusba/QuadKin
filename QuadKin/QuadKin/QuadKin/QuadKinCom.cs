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
        private static readonly int waitInterval = 3000;
        private static readonly int validInterval = 100;
        
        private Stopwatch swInit = new Stopwatch();
        private Stopwatch swValid = new Stopwatch();

        private static QuadKinCom quadKinCom;
        private static object syncRoot = new Object();

        public delegate void CommandHandler(Command cmd);
        public event CommandHandler CommandReady;

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
            if(CommandReady != null) CommandReady(c);

            if (c.valid)
            {
                switch (State)
                {
                    case State.NoConnection:
                        this.State = State.Initializing;
                        QuadCom.instance.takeOff();
                        this.swInit.Restart();
                        break;
                    case State.Initializing:
                        QuadCom.instance.sendNullCommand();
                        long timeLeft = waitInterval - this.swInit.ElapsedMilliseconds;
                        if (timeLeft <= 0)
                        {
                            this.State = State.Ready;
                            this.swInit.Reset();
                        }
                        if(swValid.IsRunning) swValid.Reset();
                        break;
                    case State.Ready:
                        QuadCom.instance.sendCommand(c);
                        if(swValid.IsRunning) swValid.Reset();
                        break;
                }
            }
            else
            {
                if (State != State.NoConnection)
                {
                    if (swValid.IsRunning)
                    {
                        if (validInterval - this.swValid.ElapsedMilliseconds <= 0)
                        {
                            swValid.Reset();
                            State = State.NoConnection;
                        }
                    }
                    else
                    {
                        swValid.Start();
                    }
                }
            }
        }
    }
}