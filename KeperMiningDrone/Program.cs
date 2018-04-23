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
        // =======================================================================================
        //                                                                            --- Setup Instructions ---
        // =======================================================================================


        // =======================================================================================
        //                                                                            --- Configuration ---
        // =======================================================================================

        // --- <Configurable Variable> ---
        // =======================================================================================
        // Description: 
        // Example: 

        // --- Script Tag ---
        // =======================================================================================
        // Description: Tag used to identify blocks that will be managed by the script
        // Example: Remote Control [KEP]
        const string SCRIPT_TAG = "KEPAI";

        // --- Scan Distance ---
        // =======================================================================================
        // Description: Operating Radius of Drone 
        // Example: double SCAN_DISTANCE = 10000;
        double SCAN_DISTANCE = 10000;

        // --- Origin GPS Location ---
        // =======================================================================================
        // Description: Origin of Drone.  Will typically be a connector.  This will be automatically set if the 
        // Boolean ManualOrigin is set to false
        // Example: bool ManualOrigin = false;
        bool ManualOrigin = false;
        // Example: GPSlocation origin = new GPSlocation("Origin", new Vector3D(0,0,0));
        GPSlocation origin = new GPSlocation("Origin", new Vector3D(0,0,0));

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================

        // App Variables
        const string VERSION = "v0.5";
        int runtime_count = 0;
        DateTime scriptStartTime;

        // Block Tags
        System.Text.RegularExpressions.Regex script_tag;

        // Block Variables
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

        Vector3D current;
        Dictionary<string, GPSlocation> asteriods = new Dictionary<string, GPSlocation>();
        Dictionary<string, GPSlocation> ships = new Dictionary<string, GPSlocation>();
        Dictionary<string, GPSlocation> stations = new Dictionary<string, GPSlocation>();

        // State Machine
        State _CurrentState;
        Dictionary<ProgramStates, State> states = new Dictionary<ProgramStates, State>();
        Dictionary<ProgramTransitions, Transition> transitions = new Dictionary<ProgramTransitions, Transition>();

        public Program()
        {
            // Init Variables:
            scriptStartTime = DateTime.Now;
            script_tag = new System.Text.RegularExpressions.Regex(generateTag(SCRIPT_TAG));

            // Init Block Groups:
            initBlockGroups();

            // Init State Machine:
            createStates();

            // Runtime.UpdateFrequency = UpdateFrequency

        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource) {

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

            Me.CustomData = "";
            foreach (IMyTerminalBlock b in blocks) {
                Me.CustomData += String.Format("{0} Damage Value: {1}\n", b.CustomName, b.CubeGrid.GetCubeBlock(b.Position).CurrentDamage);
            }


            Me.CustomData = "";

            List <IMyShipDrill> drills = new List<IMyShipDrill>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drills);
            foreach(IMyShipDrill d in drills)
            {
                float CurrentDamage = d.CubeGrid.GetCubeBlock(d.Position).CurrentDamage;
                float MaxIntegrity = d.CubeGrid.GetCubeBlock(d.Position).MaxIntegrity;
                float BuildIntegrity = d.CubeGrid.GetCubeBlock(d.Position).BuildIntegrity;

                Me.CustomData += String.Format("Drill Damage ({0}): {1}\n ", d.CustomName, ((BuildIntegrity - CurrentDamage) / MaxIntegrity));
            }


            runtime_count++;
        }

        // Utility Methods

        public void initBlockGroups() {
            LCDDisplay = new LCDGroup("Display",this);
            LCDDebug = new LCDGroup("Debug",this);
            LCDScan = new LCDGroup("Scan", this);
            LCDRemote = new LCDGroup("Remote", this);
            Cameras = new CameraGroup(this);
            SG = new ScanGyro(this);
            Power = new EnergyGroup(this);
        }

        public void createStates() {
            states.Add(ProgramStates.Startup, new StartupState(this, ProgramStates.Startup));
            states.Add(ProgramStates.Error, new ErrorState(this, ProgramStates.Error));
            states.Add(ProgramStates.Idle, new IdleState(this, ProgramStates.Idle));

            transitions.Add(ProgramTransitions.error, new ErrorTransition(this));
            transitions.Add(ProgramTransitions.boot, new BootTransition(this));

            _CurrentState = states[ProgramStates.Startup];
        }

        public string generateTag(string tag)
        {
            return @"(\[" + tag + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        }


        // Draw Methods

        public string DrawApp()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Keper AI Script");
            output.AppendLine("Script Version: " + VERSION);
            output.AppendLine("Script Start Time: " + scriptStartTime.ToString());
            output.AppendLine("Script Runcount: " + runtime_count);
            output.AppendLine("");
            output.AppendLine("Status: " + Enum.GetName(typeof(ProgramStates), _CurrentState.state));


            return output.ToString();
        }

        public string DrawAppErrors()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("### Script Error ###").AppendLine();
            //output.AppendLine(notifySB.ToString());
            output.AppendLine().AppendLine("####################");
            output.AppendLine().AppendLine(DrawApp());
            return output.ToString();
        }
    }
}