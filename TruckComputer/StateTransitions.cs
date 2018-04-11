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

        public enum ProgramStates
        {
            Startup,
            Running,
            Pause,
            Error
        }

        public abstract class State
        {
            public abstract

            public abstract bool Run();
            public virtual void Init() { }
            public virtual void Init(string arg) { }
            public abstract void Next();
        }

        public enum ProgramTransitions { }

        public abstract class Transition
        {

        }
    }
}
