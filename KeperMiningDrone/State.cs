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
                //_program.debugSB.AppendLine("State Error: " + errorMessage);
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
                    return getScriptBlocks();
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

            public bool getScriptBlocks()
            {
                List<IMyTerminalBlock> tBlocks = new List<IMyTerminalBlock>();
                _program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tBlocks, (IMyTerminalBlock b) => _program.script_tag.IsMatch(b.CustomName));

                foreach (IMyTerminalBlock b in tBlocks) {
                    string[] BlockType = b.GetType().ToString().Split('.');
                    string[] BlockTags = _program.script_tag.Match(b.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(' ');
                    BlockTags.DefaultIfEmpty("");

                    switch (BlockType[BlockType.Length - 1]) {
                        case "MyCameraBlock":
                            break;
                        case "MyTextPanel":
                            break;
                        case "MyRemoteControl":
                            break;
                        case "MyGyro":
                            try {
                                if (_program.SG.activeGyro == null)
                                {
                                    string name = b.CustomName.Replace(_program.script_tag.Match(b.CustomName).Value, "");
                                    name += "[" + SCRIPT_TAG + " ACTIVE]";
                                    b.CustomName = name;
                                    b.ApplyAction("OnOff_On");
                                    _program.SG.Add((IMyGyro)b);
                                }
                                else
                                {
                                    if (!_program.SG.activeGyro.IsFunctional)
                                    {
                                        string name = b.CustomName.Replace(_program.script_tag.Match(b.CustomName).Value, "");
                                        name += "[" + SCRIPT_TAG + " ACTIVE]";
                                        b.CustomName = name;
                                        b.ApplyAction("OnOff_On");
                                        _program.SG.Add((IMyGyro)b);
                                    }
                                    else
                                    {
                                        string name = b.CustomName.Replace(_program.script_tag.Match(b.CustomName).Value, "");
                                        name += "[" + SCRIPT_TAG + " INACTIVE]";
                                        b.CustomName = name;
                                        b.ApplyAction("OnOff_Off");
                                    }
                                }
                            } catch (Exception e) {
                                // TODO - Build Logger
                                _program.Echo(e.ToString());
                            }
                            break;
                        case "MyBatteryBlock":
                            _program.Power.addBattery((IMyBatteryBlock)b);
                            break;
                        case "MyReactor":
                            _program.Power.addReactor((IMyReactor)b);
                            break;
                        case "MySolarPanel":
                            _program.Power.addSolar((IMySolarPanel)b);
                            break;
                        default:

                            break;
                    }
                }
                return false;
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
                        if ((_program.Runtime.UpdateFrequency & UpdateFrequency.Update1) == 0)
                        {
                            _program.Runtime.UpdateFrequency = UpdateFrequency.None;
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

            String scanType = "ALL";

            public ScanState(Program p, ProgramStates s)
            {
                _program = p;
                state = s;
            }

            public override void Init()
            {
                
            }

            public override bool Run(string args) { return false; }

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

                try
                {
                    _program.SG.setSpin(_program.SG.activeGyro, "Pitch", 0.1f);
                }
                catch (Exception e) { _program.Echo(e.ToString()); }
            }

            public override bool Run(string args) {
                return false;
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

            public override bool Run(string args) { return false; }

            public override void Next()
            {

            }

        }

        public class RollState : State
        {

            public RollState(Program p, ProgramStates s)
            {
                _program = p;
                state = s;
            }

            public override void Init()
            {

            }

            public override bool Run(string args) { return false; }

            public override void Next()
            {

            }

        }
    }
}
