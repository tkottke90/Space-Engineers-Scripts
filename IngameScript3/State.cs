using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public enum ProgramStates {
            Startup,
            Idle,
            Error
        }

        public abstract class State
        {
            public Program _program { get; set; }
            public virtual ProgramStates state { get; set; }
            public virtual bool TaskComplete { get; set; } = false;
            public virtual int CycleStartTime { get; set; } = -1;


            public virtual void Init() { }
            public virtual void Init(string args) { }
            public virtual void Init(ProgramTransitions transition) { }
            public abstract bool Run(string args);
            public abstract bool Run(string args, UpdateType update);
            public abstract void Next();


            public void ThrowErrorState(string message, Func<string, bool> myErrorMethod)
            {
                myErrorMethod(message);
            }
        }
    }
}
