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
    partial class Program : MyGridProgram
    {
        // Config

        const string SCRIPT_TAG = "KEP";
        double SCAN_DISTANCE = 10000;

        // System
        const string VERSION = "v0.4";
        int runtime_count = 0;
        DateTime scriptStartTime;

        string tag_pattern = @"(\[" + SCRIPT_TAG + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        System.Text.RegularExpressions.Regex tag_match;

        // State Machine
        State _CurrentState;
        Dictionary<ProgramStates, State> states = new Dictionary<ProgramStates, State>();
        Dictionary<ProgramTransitions, Transition> transitions = new Dictionary<ProgramTransitions, Transition>();

        // Block Groups
        IMyRemoteControl Remote = null;
        LCDGroup LCDDisplay;
        LCDGroup LCDDebug;
        LCDGroup LCDScan;
        LCDGroup LCDRemote;
        CameraGroup Cameras;
        ScanGyro SG;
        EnergyGroup Power;

        // Scan Variables
        float PITCH = 0;
        float YAW = 0;
        const double SCAN_DISTANCE_PER_TICK = 0.032;
        int SCAN_RATE;

        Vector3D origin;
        Vector3D current;
        Dictionary<string, GPSlocation> asteriods = new Dictionary<string, GPSlocation>();
        Dictionary<string, GPSlocation> ships = new Dictionary<string, GPSlocation>();
        Dictionary<string, GPSlocation> stations = new Dictionary<string, GPSlocation>();

        // Debugger
        StringBuilder debugSB = new StringBuilder();
        StringBuilder notifySB = new StringBuilder();
        List<IMyTerminalBlock> OddBlocks = new List<IMyTerminalBlock>();
        

        public Program()
        {
            scriptStartTime = DateTime.Now; 
            tag_match = new System.Text.RegularExpressions.Regex(tag_pattern);
            SCAN_RATE = Convert.ToInt32(SCAN_DISTANCE * SCAN_DISTANCE_PER_TICK);


            LCDDisplay = new LCDGroup("Display", this);
            LCDDebug = new LCDGroup("Debug", this);
            LCDScan = new LCDGroup("Scan", this);
            LCDRemote = new LCDGroup("Remote", this);
            SG = new ScanGyro(this);
            Cameras = new CameraGroup(this);
            Power = new EnergyGroup(this);

            states.Add(ProgramStates.Startup, new StartupState(this, ProgramStates.Startup));
            states.Add(ProgramStates.Error, new ErrorState(this, ProgramStates.Error));
            states.Add(ProgramStates.Idle, new IdleState(this, ProgramStates.Idle));

            transitions.Add(ProgramTransitions.error, new ErrorTransition(this));
            transitions.Add(ProgramTransitions.boot, new BootTransition(this));

            _CurrentState = states[ProgramStates.Startup];
            _CurrentState.Init();
            
            //Runtime.UpdateFrequency = UpdateFrequency.Update10;
            try { Echo(DrawApp()); Echo(DrawDev()); } catch(Exception e) { Echo("Exception in DrawApp() " + e.Message); }

            LCDDebug.writeToLine("Booting...");
            LCDDisplay.writeToLine("Booting...");
            LCDScan.writeToLine("Booting...");
            LCDRemote.writeToLine("Booting...");

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument)
        {


            

            if (!_CurrentState.TaskComplete)
            {
                _CurrentState.TaskComplete = _CurrentState.Run(argument);
            }
            else
            {
                _CurrentState.Next();
            }

            if (notifySB.Length == 0 & OddBlocks.Count == 0) { Echo(DrawApp()); } else { Echo(DrawAppErrors()); }
            Echo(DrawDev());
            LCDDebug.writeToLCD(DrawDebug());
            LCDDisplay.writeToLCD(DrawDisplay());
            LCDScan.writeToLCD(DrawScan());
            LCDRemote.writeToLCD(DrawRemote());
            runtime_count++;
        }

        
        public bool getScriptBlocks() {
            List<IMyTerminalBlock> Script_Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Script_Blocks, (IMyTerminalBlock t) => tag_match.IsMatch(t.CustomName));

            foreach (IMyTerminalBlock b in Script_Blocks) {
                var BlockType = b.GetType().ToString().Split('.');
                string[] BlockTags = tag_match.Match(b.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(' ');
                BlockTags.DefaultIfEmpty("");

                switch (BlockType[BlockType.Length - 1]) {
                    case "MyCameraBlock":
                        try {
                            try {
                                Cameras.Add((IMyCameraBlock)b);
                            } catch(Exception e) {
                                debugSB.AppendLine("Error Adding Camera(" + (b.CustomName != null ? b.CustomName : "gyro") + ") : " + e.Message);
                            }
                        } catch(Exception e) { debugSB.AppendLine("Error Adding Camera: " + e.Message); }
                        break;
                    case "MyTextPanel":
                        try
                        {
                            string args1 = BlockTags[1];
                            string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                            name += "[" + SCRIPT_TAG + " " + args1 + "]";
                            b.CustomName = name;
                            switch (args1)
                            {
                                case "SCAN":
                                    LCDScan.Add((IMyTextPanel)b);
                                    break;
                                case "DEBUG":
                                    LCDDebug.Add((IMyTextPanel)b);
                                    break;
                                case "DISPLAY":
                                    LCDDisplay.Add((IMyTextPanel)b);
                                    break;
                                case "REMOTE":
                                    LCDRemote.Add((IMyTextPanel)b);
                                    break;
                            }
                        } catch(Exception e) { debugSB.AppendLine("Error Adding Text Panel(" + (b.CustomName != null ? b.CustomName : "gyro") + ") : " + e.Message); }
                        break;

                    case "MyRemoteControl":
                        Echo("Remote Control Found");
                        try { 
                            if (Remote == null) {
                                Remote = (IMyRemoteControl)b;
                                string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                name += "[" + SCRIPT_TAG + " ACTIVE]";
                                b.CustomName = name;

                                
                            } else {
                                if (!Remote.IsFunctional)
                                {
                                    Remote = (IMyRemoteControl)b;
                                    string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                    name += "[" + SCRIPT_TAG + " ACTIVE]";
                                    b.CustomName = name;
                                
                                } else
                                {
                                    string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                    name += "[" + SCRIPT_TAG + " INACTIVE]";
                                    b.CustomName = name;
                                
                                }
                            }
                        } catch(Exception e) { debugSB.AppendLine("Error Adding Remote Control(" + (b.CustomName != null ? b.CustomName : "gyro") + ") : " + e.Message); }
                        break;

                    case "MyGyro":
                        try { 
                            if(SG.activeGyro == null) {
                                string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                name += "[" + SCRIPT_TAG + " ACTIVE]";
                                b.CustomName = name;
                                b.ApplyAction("OnOff_On");
                                SG.Add((IMyGyro)b);
                            } else {
                                if (!SG.activeGyro.IsFunctional) {
                                    string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                    name += "[" + SCRIPT_TAG + " ACTIVE]";
                                    b.CustomName = name;
                                    b.ApplyAction("OnOff_On");
                                    SG.Add((IMyGyro)b);
                                } else {
                                    string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                                    name += "[" + SCRIPT_TAG + " INACTIVE]";
                                    b.CustomName = name;
                                    b.ApplyAction("OnOff_Off");
                                }
                            }
                        } catch(Exception e) { debugSB.AppendLine("Error Adding Gyro(" + (b.CustomName != null ? b.CustomName : "gyro") + ") : " + e.Message); }
                        break;
                    case "MyBatteryBlock":
                        Power.addBattery((IMyBatteryBlock) b);
                        break;
                    case "MyReactor":
                        Power.addReactor((IMyReactor)b);
                        break;
                    case "MySolarPanel":
                        Power.addSolar((IMySolarPanel)b);
                        break;
                    default:
                        OddBlocks.Add(b);
                        debugSB.AppendLine(b.CustomName + " is not used by this script");
                        break;
                }
            }
            if(OddBlocks.Count > 0) { notifySB.AppendLine(OddBlocks.Count + " invalid blocks with tag"); }
            if(Remote == null) { notifySB.AppendLine("No Remote Control Found"); }
            if(SG.activeGyro == null) { notifySB.AppendLine("No Gyro Found"); }
            if(!Cameras.hasCamera()) { notifySB.AppendLine("No Cameras Found"); }
            return (Remote != null && SG.activeGyro != null && Cameras.hasCamera());
        }
       


        /*
        public bool HandleCommand(string command) {
            string[] parts = command.ToUpper().Split(' ');
            parts.DefaultIfEmpty("");

            var arg0 = parts.ElementAtOrDefault(0);
            var arg1 = parts.ElementAtOrDefault(1);

            try {
                Echo("arg0: " + arg0);
                Echo("arg1: " + arg1);

                switch (arg0) {
                    case "SCAN":

                        break;
                    case "PAUSE":
                        Runtime.UpdateFrequency &= ~UpdateFrequency.Update10;
                        break;
                    case "RESUME":
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        break;
                    default:
                        return false;
                }

                return true;

            } catch (Exception e) { Echo("Error in Handle Command: " + e.Message); return false; }
        }
        */
    }
}