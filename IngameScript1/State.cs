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
            Ready,
            Idle,
            Countdown,
            Stage_One,
            Stage_Two,
            Adjust,
            Error,
            Config
        }

        public void createStates()
        {
            states.Add(ProgramStates.Startup, new StartupState(this, ProgramStates.Startup));
            states.Add(ProgramStates.Error, new ErrorState(this, ProgramStates.Error));
            states.Add(ProgramStates.Ready, new ReadyState(this, ProgramStates.Ready));
            states.Add(ProgramStates.Countdown, new CountdownState(this, ProgramStates.Countdown));
            states.Add(ProgramStates.Stage_One, new StageOneState(this, ProgramStates.Stage_One));
            states.Add(ProgramStates.Stage_Two, new StageTwoState(this, ProgramStates.Stage_Two));
            states.Add(ProgramStates.Idle, new IdleState(this, ProgramStates.Idle));
            states.Add(ProgramStates.Config, new ConfigState(this, ProgramStates.Config));
            

            transitions.Add(ProgramTransitions.error, new ErrorTransition(this));
            transitions.Add(ProgramTransitions.boot, new BootTransition(this));
            transitions.Add(ProgramTransitions.reboot, new RebootTransition(this));
            transitions.Add(ProgramTransitions.start, new StartTransition(this));
            transitions.Add(ProgramTransitions.stop, new StopTransition(this));
            transitions.Add(ProgramTransitions.ignition, new IgnitionTransition(this));
            transitions.Add(ProgramTransitions.release, new ReleaseTransition(this));
            transitions.Add(ProgramTransitions.done, new CompleteTransition(this));
            transitions.Add(ProgramTransitions.config, new ConfigTransition(this));


            _currentState = states[ProgramStates.Startup];
            _currentState.Init();
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
                _program.debugLog.Add("State Error: " + (state.ToString()) + ": " + errorMessage);
                _program.transitions[ProgramTransitions.error].action();
            }
        }

        public class StartupState : State
        {
            public StartupState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                this.TaskComplete = this.Run("");
            }

            public override bool Run(string args)
            {
                _program.Runtime.UpdateFrequency = UpdateFrequency.Update1;
                try
                {
                    _program.Me.CustomData = "";
                    foreach (string s in _program.debugLog) { _program.Me.CustomData += s + "\n"; }
                    return getScriptBlocks();
                }
                catch (Exception e)
                {
                    ThrowErrorState(e.Message);
                    return false;
                }
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.boot].action();
                this.TaskComplete = false;
            }

            public bool getScriptBlocks()
            {
                _program.debugLog.Add("-- Boot --");
                _program.EventLog.Add(String.Format("{0} - Boot Started", _program.EventLog.Count));
                List<IMyTerminalBlock> tBlocks = new List<IMyTerminalBlock>();
                _program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tBlocks, (IMyTerminalBlock b) => _program.Me.CubeGrid == b.CubeGrid);
                _program.EventLog.Add(String.Format("{0} - Terminal Block Count: {1}", _program.EventLog.Count, tBlocks.Count));


                _program.EventLog.Add(String.Format("{0} - Reset Block Groups", _program.EventLog.Count));
                _program.remote = null;
                _program.cargoTanks.group.Clear();
                _program.rocketTanks.group.Clear();
                _program.boosterTanks.group.Clear();
                _program.debug.group.Clear();
                _program.display.group.Clear();
                _program.rocketRelease = null;
                _program.cargoRelease = null;
                _program.control = null;
                _program.radio = null;
                _program.laser = null;
                _program.boosters.Clear();

                foreach (IMyTerminalBlock b in tBlocks)
                {
                    _program.EventLog.Add(String.Format("{0} - Add Block: {1}", _program.EventLog.Count, b.CustomName));
                    string[] BlockType = b.GetType().ToString().Split('.');
                    string[] BlockTags = _program.tag_match.Match(b.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(' ');
                    BlockTags.DefaultIfEmpty("");

                    switch (BlockType[BlockType.Length - 1])
                    {
                        case "MyRemoteControl":
                            string remoteName = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                            b.CustomName = "@Remote Control [NKRS]";
                            _program.remote = ((IMyRemoteControl)b);
                            break;
                        case "MyGyro":
                            if (_program.control == null)
                            {
                                string gyroName = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                                b.CustomName = "@Gyro [NKRS]";
                                _program.control = ((IMyGyro)b);
                                _program.control.GyroOverride = true;
                            }
                            break;
                        case "MyThrust":
                            if (!checkTag(b.CustomName))
                            {
                                _program.debugLog.Add(String.Format("Invalid Thruster Tag: {0}", b.CustomName));
                                continue;
                            }
                            string thrustArgs = BlockTags[1];
                            string thrustName = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                            thrustName += "[" + _program.scriptName + " " + thrustArgs + "]";
                            b.CustomName = thrustName;
                            switch (thrustArgs)
                            {
                                case "CARGO":
                                    _program.cargoThrusters.Add((IMyThrust)b);
                                    break;
                                case "ROCKET":
                                    _program.rocketThrusters.Add((IMyThrust)b);
                                    break;
                                case "BOOSTER":
                                    _program.boosterThursters.Add((IMyThrust)b);
                                    break;
                            }
                            break;
                        case "MyTextPanel":
                            if (!checkTag(b.CustomName))
                            {
                                _program.debugLog.Add(String.Format("Invalid Text Panel Tag: {0}", b.CustomName));
                                continue;
                            }
                            string args1 = BlockTags[1];
                            string name = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                            name += "[" + _program.scriptName + " " + args1 + "]";
                            b.CustomName = name;
                            switch (args1)
                            {
                                case "DEBUG":
                                    _program.debug.Add((IMyTextPanel)b);
                                    break;
                                case "DISPLAY":
                                    _program.display.Add((IMyTextPanel)b);
                                    ((IMyTextPanel)b).SetValue<Int64>("alignment", (Int64)2);
                                    break;
                            }

                            break;
                        case "MyGasTank":

                            if (!checkTag(b.CustomName))
                            {
                                _program.debugLog.Add(String.Format("Invalid Tank Tag: {0}", b.CustomName));
                                continue;
                            }
                            string tankArgs = BlockTags[1];
                            string tankName = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                            tankName += "[" + _program.scriptName + " " + tankArgs + "]";
                            b.CustomName = tankName;
                            switch (tankArgs)
                            {
                                case "ROCKET":
                                    _program.rocketTanks.Add((IMyGasTank)b);
                                    break;
                                case "CARGO":
                                    _program.cargoTanks.Add((IMyGasTank)b);
                                    break;
                                case "BOOSTER":
                                    _program.boosterTanks.Add((IMyGasTank)b);
                                    break;
                                default:
                                    b.CustomName = "Hydrogen Tank (" + b.EntityId + ")";
                                    break;
                            }

                            break;

                        case "MyShipMergeBlock":
                            if (!checkTag(b.CustomName))
                            {
                                _program.EventLog.Add(String.Format("Invalid Merge Block Tag: {0}", b.CustomName));
                                continue;
                            }
                            string mergeArgs = BlockTags[1];
                            string mergeName = b.CustomName.Replace(_program.tag_match.Match(b.CustomName).Value, "");
                            mergeName += "[" + _program.scriptName + " " + mergeArgs;
                            b.CustomName = mergeName;
                            switch (mergeArgs)
                            {
                                case "ROCKET":
                                    if (_program.rocketRelease == null)
                                    {
                                        _program.rocketRelease = ((IMyShipMergeBlock)b);
                                        b.CustomName += "]";
                                    }
                                    else
                                    {
                                        b.CustomName += " Disabled]";
                                        ((IMyShipMergeBlock)b).Enabled = false;
                                    }
                                    break;
                                case "CARGO":
                                    if (_program.cargoRelease == null)
                                    {
                                        _program.cargoRelease = ((IMyShipMergeBlock)b);
                                        b.CustomName += "]";
                                    }
                                    else
                                    {
                                        b.CustomName += " Disabled]";
                                        ((IMyShipMergeBlock)b).Enabled = false;
                                    }
                                    break;
                                case "BOOSTER":
                                    b.CustomName += "]";
                                    _program.boosters.Add((IMyShipMergeBlock)b);
                                    break;
                                case "NA":
                                    b.CustomName += "]";
                                    continue;
                            }
                            break;

                        case "MyParachute":
                            _program.chutes.Add((IMyParachute)b);
                            ((IMyParachute)b).SetValue<Single>("AutoDeployHeight", 2000.0f);

                            ((IMyParachute)b).CustomName = String.Format("Parachue Hatch [NKRS] {0}", b.EntityId);

                            break;

                        case "MyRadioAntenna":
                            if (_program.radio == null)
                            {
                                _program.radio = ((IMyRadioAntenna)b);
                            } else
                            {
                                _program.debugLog.Add("Duplicate Radio Antenna Found");
                            }
                            break;
                        case "MyLaserAntenna":
                            if (!checkTag(b.CustomName))
                            {
                                _program.debugLog.Add(String.Format("Invalid Laser Antenna Tag: {0}", b.CustomName));
                                continue;
                            }

                            if (_program.laser == null)
                            {
                                _program.laser = ((IMyLaserAntenna)b);
                                _program.laser.CustomName = "[NKRS] Laser Antenna";

                            }
                            else
                            {
                                _program.debugLog.Add("Duplicate Laser Antenna Found");
                            }
                            break;
                        default:
                            // _program.Me.CustomData += String.Format("\n\nBlock: {0}\n    - Type: {1}\n\n", b.CustomName, BlockType[BlockType.Length - 1]);
                            break;
                    }
                }

                // Set Initial Group Values
                _program.display.writeToLCD("Loading....N00bKeper Rocket Script Debug Display");
                _program.debug.writeToLCD("Loading.... N00bKeper Rocket Script Debug Display");
                _program.rocketThrusters.Disable();
                _program.cargoThrusters.Disable();

                // Log Group Status'
                _program.debugLog.Add(String.Format("Remote Control: [{0}", _program.remote != null ? "X]" : " ] !Required"));
                _program.debugLog.Add(String.Format("Gyro Control: [{0}", _program.control != null ? "X]" : " ] !Required"));
                _program.debugLog.Add(String.Format("Hydrogen Tanks: [{0}", _program.rocketTanks.group.Count > 0 ? "X]" : "] !Required"));
                _program.debugLog.Add(String.Format("Launch Thrusters: [{0}", _program.rocketThrusters.getThrusterCount() > 0 ? "X]" : "] !Required"));

                if (_program.remote == null) {
                    ThrowErrorState("No Valid Remote Control Found");
                    return false;
                }

                if (_program.control == null)
                {
                    ThrowErrorState("No Valid Gyroscope Found");
                    return false;
                }

                if (_program.rocketTanks.group.Count == 0)
                {
                    ThrowErrorState("No Hydrogen Tanks Found");
                    return false;
                }

                if (_program.rocketThrusters.getThrusterCount() == 0)
                {
                    ThrowErrorState("No Thrusters Found");
                    return false;
                }

                _program.debugLog.Add("-- Boot Complete --\n");
            
                return true;
            }

            private bool checkTag(string tag)
            {
                return _program.tag_match.Match(tag).Success;
            }
        }


        public class ErrorState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.error;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public ErrorState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;

            }

            public override bool Run(string args)
            {
                args = args.ToUpper();
                switch (args)
                {
                    case "REBOOT":
                        this.nextTransition = ProgramTransitions.reboot;
                        return true;
                default:
                        _program.Me.CustomData = _program.debugLog.ToString();
                         
                        _program.Echo(_program.DrawError());
                        _program.debug.writeToLCD(_program.DrawDebug());
                        _program.Runtime.UpdateFrequency = UpdateFrequency.None;
                        
                        break;
                }
                return false;
            }

            public override void Next()
            {
                _program.transitions[nextTransition].action();
                this.TaskComplete = false;
            }
        }

        public class ReadyState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.boot;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public ReadyState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
            }

            public override bool Run(string args)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-- N00bKeper Rocket Script --");
                sb.AppendLine("Status: Ready").AppendLine();

                sb.AppendLine("-- Configuration --");
                sb.AppendFormat("Broadcasting Status: [{0}]\n", _program.broadcastStatus ? "X" : " ").AppendLine();

                sb.AppendLine("-- Block Information --").AppendLine();
                sb.AppendFormat("Remote Control: [{0}]\n", _program.remote != null ? "X" : " " );
                sb.AppendFormat("Gyroscope: [{0}]   - Enabled: [{1}]\n", _program.control != null ? "X" : " ", _program.control.Enabled ? "X" : " ");
                sb.AppendFormat("                  - Locked: [{0}]\n", _program.control.GyroOverride ? "X" : " ");
                sb.AppendFormat("Radio Antenna: [{0}]", _program.radio != null ? "X" : " ");
                if (_program.radio != null)
                {
                    sb.AppendFormat("  - Enabled: [{0}]\n", _program.radio.Enabled ? "X" : " ");
                    sb.AppendFormat("                     - Range: {0}\n", _program.radio.Radius);
                } else { sb.AppendLine();  }

                sb.AppendFormat("Laser Antenna: [{0}]", _program.laser != null ? "X" : " ");

                if (_program.laser != null)
                {
                    sb.AppendFormat("  - Enabled: [{0}]\n", _program.laser.Enabled ? "X" : " ");
                    sb.AppendFormat("                     - Connected: [{0}]\n", _program.laser.Status == MyLaserAntennaStatus.Connected ? "X" : " ");
                    sb.AppendFormat("                     - Range: {0}\n", _program.laser.Range);
                } else { sb.AppendLine();  }

                sb.AppendLine().AppendLine("-- Fuel Information --").AppendLine();
                if (_program.boosterTanks.group.Count > 0)
                {
                    sb.AppendFormat("Booster Fuel Level: {0}% {1}\n", Math.Round((_program.boosterTanks.getFill() * 100), 2), Math.Round((_program.boosterTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
                }
                sb.AppendFormat("Rocket Fuel Level: {0}%\n", Math.Round((_program.rocketTanks.getFill() * 100),2));
                sb.AppendFormat("Cargo Fuel Level: {0}%\n", Math.Round((_program.cargoTanks.getFill() * 100), 2));

                _program.display.setFontSize(0.65f);
                _program.display.writeToLCD(sb.ToString());
                _program.debug.writeToLCD(_program.DrawDebug());

                
                string[] a = args.ToUpper().Split(' ');
                switch(a[0])
                {
                    case "REBOOT":
                        this.nextTransition = ProgramTransitions.reboot;
                        return true;
                    case "LAUNCH":
                        this.nextTransition = ProgramTransitions.start;
                        return true;
                    case "CONFIG":
                        this.nextTransition = ProgramTransitions.config;
                        return true;
                    case "ERROR":
                        this.nextTransition = ProgramTransitions.error;
                        return true;
                    default: return false;
                }

            }

            public override void Next()
            {
                _program.transitions[nextTransition].action();
                this.TaskComplete = false;
            }
        }

        public class CountdownState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.ignition;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public int countdownTime = 60;
            public CountdownState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                countdownTime = _program.countdown;
                _program.debugLog.Add(String.Format("{0} - Launch Sequence Started", _program.runcount));
            }

            public override bool Run(string args)
            {
                if (countdownTime < 60)
                {
                    return true;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-- N00bKeper Rocket Script --");
                sb.AppendLine("Status: Countdown Started").AppendLine();

                if (_program.boosterTanks.group.Count > 0)
                {
                    sb.AppendFormat("Booster Fuel Level: {0}% {1}\n", Math.Round((_program.boosterTanks.getFill() * 100), 2), Math.Round((_program.boosterTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
                }
                sb.AppendFormat("Rocket Fuel Level: {0}%\n", Math.Round((_program.rocketTanks.getFill() * 100), 2));
                sb.AppendFormat("Cargo Fuel Level: {0}%\n", Math.Round((_program.cargoTanks.getFill() * 100), 2));

                sb.AppendFormat("Lanunching in: {0} seconds", (countdownTime / 60)).AppendLine();
                sb.AppendFormat("Enter \'STOP\' argument to PB to stop launch").AppendLine();
                _program.display.writeToLCD(sb.ToString());
                _program.debug.writeToLCD(_program.DrawDebug());

                string[] a = args.ToUpper().Split(' ');
                switch (a[0])
                {
                    case "STOP":
                        this.nextTransition = ProgramTransitions.stop;
                        return true;
                    case "GO":
                        this.nextTransition = ProgramTransitions.ignition;
                        return true;
                    default:
                        countdownTime--;
                        return false;
                }
            }

            public override void Next()
            {
                _program.transitions[nextTransition].action();
                this.TaskComplete = false;
            }
        }

        public class StageOneState : State
        {
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public StageOneState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                _program.debugLog.Add(String.Format("{0} - Launch", _program.runcount));
                _program.display.setFontSize(1.0f);
                _program.rocketThrusters.setThrust(_program.rocketThrusters.getMaxThrust());
                _program.rocketThrusters.Enable();
                _program.boosterThursters.setThrust(_program.boosterThursters.getMaxThrust());
                _program.boosterThursters.Enable();
            }

            public override bool Run(string args)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-- N00bKeper Rocket Script --");
                sb.AppendLine("Status: Stage One").AppendLine().AppendLine();

                sb.AppendFormat("[{0}] Boosters Enabled", _program.boosterThursters.isEnabled() ? "X" : " ").AppendLine();
                sb.AppendFormat("[{0}] Rockets Enabled", _program.rocketThrusters.isEnabled() ? "X" : " ").AppendLine().AppendLine();

                if (_program.boosterTanks.group.Count > 0)
                {
                    sb.AppendFormat("Booster Fuel Level: {0}% {1}\n", Math.Round((_program.boosterTanks.getFill() * 100), 2), Math.Round((_program.boosterTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
                }

                sb.AppendFormat("Rocket Fuel Level: {0}% {1}\n", Math.Round((_program.rocketTanks.getFill() * 100), 2), Math.Round((_program.rocketTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
                sb.AppendFormat("Cargo Fuel Level: {0}% {1}\n\n", Math.Round((_program.cargoTanks.getFill() * 100), 2), Math.Round((_program.cargoTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");

                Vector3D gravity = _program.remote.GetNaturalGravity();
                double magnitude = Math.Sqrt(Math.Pow(gravity.X, 2) + Math.Pow(gravity.Y, 2) + Math.Pow(gravity.Z, 2));
                sb.AppendFormat("Gravity (G): {0}\n", Math.Round(magnitude / 9.81, 2));

                bool threshold = Math.Round(magnitude / 9.81, 2) <= _program.transitionGrav || Math.Round((_program.rocketTanks.getFill() * 100), 2) == 0.0;
                _program.gravity = magnitude;

                _program.display.writeToLCD(sb.ToString());
                _program.debug.writeToLCD(_program.DrawDebug());

                // Monitor Booster Tanks - Drop if Empty
                if (_program.boosterTanks.group.Count > 0 && _program.boosterTanks.getFill() <= 0.1)
                {
                    _program.debugLog.Add(String.Format("{0} - Booster Tanks Depleted", _program.runcount));
                    _program.boosterTanks.Clear();   
                    foreach (IMyShipMergeBlock m in _program.boosters)
                    {
                        m.Enabled = false;
                    }
                }

                // Monitor Speed, fire rocket thrusters if below 80 m/s
                if (_program.remote.GetShipSpeed() > 80.0)
                {
                    _program.rocketThrusters.Disable();
                } else
                {
                    _program.rocketThrusters.Enable();
                }

                _program.rocketThrusters.logEnabled();
                _program.boosterThursters.logEnabled();

                if (threshold)
                {
                    return true;
                }

                return false;
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.release].action();
                _program.debugLog.Add(String.Format("{0} - Stage One Complete ({1} sec)", _program.runcount, ((_program.runcount - CycleStartTime) / 60) ));
                _program.debugLog.Add(String.Format("- Report: ", _program.runcount));
                _program.debugLog.Add(String.Format("  - Booster Uptime: {0} sec", (_program.boosterThursters.overideTime / 60)));
                _program.debugLog.Add(String.Format("  - Booster Fuel Used: {0}%", Math.Round((100 - (_program.boosterTanks.getFill() * 100)), 2)));
                _program.debugLog.Add(String.Format("  - Rocket Uptime: {0} sec", (_program.rocketThrusters.overideTime / 60)));
                _program.debugLog.Add(String.Format("  - Rocket Fuel Used {0}%", Math.Round((100 - (_program.rocketTanks.getFill() * 100)), 2)));
                this.TaskComplete = false;
            }
        }

        public class StageTwoState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.done;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public StageTwoState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                _program.rocketThrusters.Disable();
                _program.boosterThursters.Disable();
                _program.cargoThrusters.EnableForward(_program.remote.Orientation.Forward);
                _program.cargoThrusters.setThrust(_program.cargoThrusters.getMaxThrust());
                
                _program.remote.DampenersOverride = false;

                foreach(IMyParachute p in _program.chutes)
                {
                    p.SetValueBool("AutoDeploy", true);
                }

                _program.rocketRelease.Enabled = false;
            }

            public override bool Run(string args)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-- N00bKeper Rocket Script --");
                sb.AppendLine("Status: Stage Two\n\n\n");

                sb.AppendFormat("Cargo Fuel Level: {0}%\n", Math.Round((_program.cargoTanks.getFill() * 100), 2));

                Vector3D gravity = _program.remote.GetNaturalGravity();
                double magnitude = Math.Sqrt(Math.Pow(gravity.X, 2) + Math.Pow(gravity.Y, 2) + Math.Pow(gravity.Z, 2));
                sb.AppendFormat("Gravity (G): {0}\n", Math.Round(magnitude / 9.81, 2));
                _program.gravity = magnitude;

                Vector3D position = _program.remote.GetPosition();
                sb.AppendFormat("Position: \n    X: {0}\n    Y:{1}\n    Z:{2}\n",Math.Round(position.X,2), Math.Round(position.Y, 2), Math.Round(position.Z, 2));

                _program.display.writeToLCD(sb.ToString());
                _program.debug.writeToLCD(_program.DrawDebug());

                if (_program.gravity == 0)
                {
                    return true;
                }

                return false;
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.done].action();
                _program.debugLog.Add(String.Format("{0} - Stage Two Complete ({1} sec)", _program.runcount, ((_program.runcount - CycleStartTime) / 60)));
                this.TaskComplete = false;
            }
        }

        public class IdleState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.done;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public IdleState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                _program.cargoThrusters.setThrust(0.0f);
                _program.cargoThrusters.Enable();
                _program.remote.DampenersOverride = true;
                _program.control.GyroOverride = false;
            }

            public override bool Run(string args)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("-- N00bKeper Rocket Script --");
                sb.AppendLine("Status: Idle\n\n\n");

                sb.AppendFormat("Cargo Fuel Level: {0}%\n", Math.Round((_program.cargoTanks.getFill() * 100), 2));

                Vector3D position = _program.remote.GetPosition();
                sb.AppendFormat("Position: \n    X: {0}\n    Y:{1}\n    Z:{2}\n", Math.Round(position.X, 2), Math.Round(position.Y, 2), Math.Round(position.Z, 2));

                _program.display.writeToLCD(sb.ToString());
                _program.debug.writeToLCD(_program.DrawDebug());

                _program.Me.CustomData = _program.DrawDebug();
                _program.Runtime.UpdateFrequency = UpdateFrequency.None;

                string[] a = args.ToUpper().Split(' ');
                switch (a[0])
                {
                    case "STATUS":
                        sb.Clear();
                        sb.AppendLine("-- Cargo Status --");
                        sb.AppendLine("Status: Idle\n\n\n");
                        sb.AppendFormat("").AppendLine();
                        return false;
                    default: return false;
                }
            }

            public override void Next()
            {
                _program.transitions[ProgramTransitions.done].action();
                _program.debugLog.Add(String.Format("{0} - Stage Two Complete ({1} sec)", _program.runcount, ((_program.runcount - CycleStartTime) / 60)));
                this.TaskComplete = false;
            }
        }

        public class ConfigState : State
        {
            public ProgramTransitions nextTransition = ProgramTransitions.done;
            public override int CycleStartTime { get; set; } = -1;
            public override bool TaskComplete { get; set; } = false;
            public ConfigState(Program p, ProgramStates s)
            {
                this.state = s;
                _program = p;
            }

            public override void Init()
            {
                if (CycleStartTime == -1) CycleStartTime = _program.runcount;
                _program.Me.CustomData = writeConfig();
                if (_program.display.group.Count > 0)
                {
                    _program.display.writeToLCD(writeConfig());
                } else
                {
                    _program.Me.CustomData = writeConfig();
                }
                _program.Runtime.UpdateFrequency = UpdateFrequency.None;
            }

            public override bool Run(string args)
            {
                string[] a = args.ToUpper().Split(' ');
                switch (a[0])
                {
                    case "EXIT":
                        return true;
                    case "SAVE-EXIT":
                        readConfig();
                        return true;
                    case "SAVE":
                        readConfig();
                        if (_program.display.group.Count > 0)
                        {
                            _program.display.writeToLCD(writeConfig());
                        }
                        else
                        {
                            _program.Me.CustomData = writeConfig();
                        }
                        return false;
                    default: return false;
                }
            }

            public override void Next()
            {
                _program.Runtime.UpdateFrequency = UpdateFrequency.Update1;
                _program.transitions[ProgramTransitions.boot].action();
                this.TaskComplete = false;
            }

            public string writeConfig() {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("// -- N00bKeper Rocket Script Config --");
                sb.AppendLine("//  - To update a value, place an \'[X]\' in the selection box.").AppendLine("//  - Toggleable settings can be changed by adding check \'[X]\'").AppendLine("//  - Setting with value require value after \':\'").AppendLine();

                sb.AppendLine("// -- Countdown Timer --").AppendLine("// Description: Length of countdown timer before launch. Time is expressed in seconds").AppendLine("[ ] Coutdown-Time: " + (_program.countdown / 60)).AppendLine();
                sb.AppendLine("// -- Change Cargo Names --").AppendLine("// Description: Update the block tag of blocks labeled CARGO, after the script has completed").AppendLine("Note: Prefence is a toggle, placing an \'[X]\' switches the preference").AppendLine("[ ] Change-Cargo-Name: " + _program.changeCargoNames).AppendLine();
                sb.AppendLine("// -- Updated Cargo Name --").AppendLine("// Description: Updates CARGO block names if changeCaroNames is true").AppendLine("// Example: string updatedCargoName = \"Satelite\" => [NKRS CARGO] -> [Satelite]").AppendLine("[ ] Updated-Cargo-Name: " + _program.updatedCargoName).AppendLine();
                sb.AppendLine("// -- Broadcast Status --").AppendLine("// Description: Should script broadcast status for other antenna to listen to?").AppendLine("Note: Prefence is a toggle, placing an \'[X]\' switches the preference").AppendLine("[ ] Broadcast-Status: " + _program.broadcastStatus).AppendLine();
                sb.AppendLine("// -- Transition Point --").AppendLine("// Description: Specific gravity ratio that the rocket should transition from using the main thrusters to the cargo thrusters.").AppendLine("[ ] Transition-Gavtity: " + _program.transitionGrav).AppendLine();
                return sb.ToString();
            }

            public void readConfig()
            {
                string config = "";

                if (_program.display.group.Count > 0)
                {
                    config = _program.display.readLCD();
                }
                else
                {
                    config = _program.Me.CustomData;
                    _program.Me.CustomData = "";
                }

                string[] configLines = config.Split('\n').Where(s => s.StartsWith("[")).ToArray();

                // Check if Countdown Timer Pref Changed
                if (configLines[0].StartsWith("[X]"))
                {
                    int newCountdown = 10;
                    bool result = int.TryParse(configLines[0].Split(' ')[2], out newCountdown);
                    
                    if (result)
                    {
                        _program.countdown = newCountdown * 60;
                        _program.debugLog.Add(" - Updated Countdown Pref");
                    } else
                    {
                        _program.debugLog.Add(String.Format("Invalid Setting Value (Countdown): \"{0}\"", configLines[0].Split(' ')[2]));
                    }
                }

                // Check if Change Cargo Name Pref Changed:
                if (configLines[1].StartsWith("[X]")) {
                    _program.changeCargoNames = !_program.changeCargoNames;
                    _program.debugLog.Add(" - Updated Change Cargo Name Pref");
                }

                // Check if Cargo Name Pref Changed:
                if (configLines[2].StartsWith("[X]")) {
                    _program.updatedCargoName = configLines[0].Split(' ')[2];
                    _program.debugLog.Add(" - Updated Cargo Name Pref");
                }

                // Check if Broadcast Pref Changed:
                if (configLines[3].StartsWith("[X]")) {
                    _program.broadcastStatus = !_program.broadcastStatus;
                    _program.debugLog.Add(" - Updated Broadcast Status Pref");
                }

                // Check if Transition Grav Pref Changed:
                if (configLines[4].StartsWith("[X]"))
                {
                    _program.debugLog.Add(double.TryParse(configLines[4].Split(' ')[2], out _program.transitionGrav) ? "- Updated Transition Gravity" : " - Error Updating Transition Gravity" );
                }
            }
        }
    }



}
