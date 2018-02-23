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
            Off,
            Startup,
            Idle,
            Working,
            Mining,
            Scanning,
            Traveling,
            Refresh,
            Error
        }

        public enum ScanStates {
            Pitch,
            Yaw,
            Roll,
            Off
        }

        public abstract class State
        {
            public Program _program { get; set; }
            public virtual ProgramStates state { get; set; }
            public virtual bool TaskComplete { get; set; } = false;
            public virtual int CycleStartTime { get; set; } = -1;
            

            public virtual void Init() { }
            public virtual void Init(string args) { }
            public abstract bool Run(string args);
            public abstract void Next();
            

            public void ThrowErrorState(string errorMessage)
            {
                _program.debugSB.AppendLine("State Error: " + errorMessage);
                _program.transitions[ProgramTransitions.error].action();
            }
        }

        public class StartupState : State {
            public StartupState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runtime_count;
                this.TaskComplete = this.Run("");
            }

            public override bool Run(string args)
            {
                try
                {
                    return _program.getScriptBlocks();
                } catch (Exception e)
                {
                    ThrowErrorState(e.Message);
                    return false;
                }
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.boot].action();
            }

            
        }

        public class IdleState: State {
            
            public IdleState(Program p, ProgramStates s) {
                this.state = s;
                _program = p;
            }

            Queue<ProgramTransitions> queue = new Queue<ProgramTransitions>();

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runtime_count;
            }

            public override bool Run(string args)
            {
                switch (args) {
                    case "SCAN":
                        
                        break;
                    case "COLLECT":

                        break;
                    default:
                        


                        break;
                }


                return false;
            }

            public override void Next()
            {
                
            }
        }

        public class ErrorState : State {
            
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public ErrorState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runtime_count;
            }

            public override bool Run(string args)
            {
                args = args.ToUpper();
                switch (args)
                {
                    case "REBOOT":
                        return true;
                    default:

                        _program.Echo(_program.DrawAppErrors());
                        if (_program.Runtime.UpdateFrequency == UpdateFrequency.Update1)
                        {
                            _program.Runtime.UpdateFrequency = ~UpdateFrequency.Update1;
                        }
                        break;
                }
                return false;
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.reboot].action();
            }
    }


        public class ScanState : State {

            public ScanState(Program p, ProgramStates s)
            {
                _program = p;
                state = s;
            }

            public override void Init()
            {
                
            }

            public override bool Run(string args) { }

            public override void Next()
            {
                
            }

        }

        public class PitchState : State
        {

            public PitchState(Program p, ProgramStates s)
            {
                _program = p;
                state = s;
            }

            public override void Init()
            {
                CycleStartTime = _program.runtime_count;
                TaskComplete = false;

                try {
                    _program.SG.setSpin(_program.SG.activeGyro, "Pitch", 0.1f);
                }
            }

            public override bool Run(string args) {

            }

            public override void Next()
            {

            }

        }

        public class YawState : State
        {

            public YawState(Program p, ProgramStates s)
            {
                _program = p;
                state = s;
            }

            public override void Init()
            {

            }

            public override bool Run(string args) { }

            public override void Next()
            {

            }

        }
    }
}
