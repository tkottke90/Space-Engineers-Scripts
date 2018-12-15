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
        /*
        *
        *	N00bKeper Truck Script
        *
        *	version: 1.01
        / 	last update: 3/19/2018/ 10:35
        *
        *	description: Template for creating new Space Engineers Scripts
        *
        */

        // =======================================================================================
        //                                                                            --- Setup Instructions ---
        // =======================================================================================


            // Script Commands:
            // Script commands can be run in the terminal or by setting your cockpit/remote control 
            //    toolbar and run by adding the command.  The application will update information 
            //    in the script.

            // 1) useTag:true - change useTag Preference
            //    useTag:[bool]
            // 2) Add:Tail:Interior Light 5 - add light
            //   Add:[Type]:[Block Name]
            // 3) Remove:Tail:Interior Light 4 - remove light
            //   Remove:[Type]:[Block Name]
            // 4) AllOff - Turns off All Lights
            // 

        // =======================================================================================
        //                                                                            --- Configuration ---
        // =======================================================================================

        // --- Block Reference ---
        // =======================================================================================
        // Description: Selects whether the blocks need a block tag or if the block needs to be added to a list to be managed
        // Example with Tag: bool useTag = true  => Interior Light [NKTC:TailLight]
        bool useTag = true;

        // --- Default Ride Height ---
        // =======================================================================================
        // Description: Sets the default wheel height for the 
        // Example with Tag: bool useTag = true  => Interior Light [NKTC:TailLight]
        float defaultWheelHeight = 0.2f;


        // --- Tail Lights ---
        // =======================================================================================
        // Description: Manages Tail Lights.  Changes the Intensity when a deceleration is detected
        // Example with Tag: Interior Light [NKTC:TailLight]
        // Example with list: string[] tail_lights = { "Interior Light", "Interior Light 1" };
        string tail_light_TAG = "TailLight";
        string[] tail_lights = { };

        // --- Tail Light Color ---
        // =======================================================================================
        // Description: Sets Tail Light Color
        Color tail_light_Color = new Color(255f, 0f, 0f);
     
        // --- Running Lights ---
        // =======================================================================================
        // Description: Manages Running Lights.
        // Example with Tag: Interior Light [NKTC:RunLight]
        // Example with list: string[] tail_lights = { "Interior Light", "Interior Light 1" };
        string run_light_TAG = "RunLight";
        string[] run_lights = { };

        // --- Running Light Color ---
        // =======================================================================================
        // Description: Manages Running Lights Color.
        Color run_light_Color = new Color(255f, 128.0f, 0f);

        // --- Reverse Lights ---
        // =======================================================================================
        // Description: Manages Running Lights.
        // Example with Tag: Interior Light [NKTC:RunLight]
        // Example with list: string[] tail_lights = { "Interior Light", "Interior Light 1" };
        string reverse_light_TAG = "ReverseLight";
        string[] reverse_lights = { };

        // --- Running Light Color ---
        // =======================================================================================
        // Description: Manages Tail Lights Color.
        Color reverse_light_Color = new Color(255f, 255f, 255f);

        // --- Preferred Controler ---
        // =======================================================================================
        // Description: Allows the selection of a specific cockpit or remote control that will be used to detect speed
        // Example with Tag: Remote Control [NKTC]
        // Example with list: string[] controler = { "Remote Control" };
        string controler = "";

        // --- In Terminal Power Display ---
        // =======================================================================================
        // Description: Included in this script is an Energy Group script that watches the power
        //              systems.  This group has the ability to display the current power level and
        //              trend of the power in the terminal of the blocks custom name
        //  
        //              *Power: Battery 0 (54.47% -)
        //
        // Example: bool TerminalPowerDisplay = true;
        bool TerminalPowerDisplay = false;

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================

        // System
        int runcount = 0;
        int updateCount = 0;

        int TEN_SECONDS = 60 * 10;

        // Block Tag (Regex)
        string TLTAG_pattern;
            string RNTAG_pattern;
            string RLTAG_pattern;

            System.Text.RegularExpressions.Regex RLTAG_match;
            System.Text.RegularExpressions.Regex RNTAG_match;
            System.Text.RegularExpressions.Regex TLTAG_match;   

            // Loggers
            MyLogger debug = new MyLogger("Debug");
            MyLogger notification = new MyLogger("Notify");
            MyLogger warning = new MyLogger("Warn");

            // Light Groups
            LightGroup RunningLights;
            LightGroup ReverseLights;
            LightGroup TailLights;

            // Speed Detection
            IMyRemoteControl Remote;
            IMyShipController Cockpit;

            // Power Detection
            EnergyGroup Power;

            // Wheel Configuration
            WheelGroup Wheels;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            Me.CustomName = "PB [NKTC]";

            Power = new EnergyGroup(this);


            List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(wheels);

            List<ITerminalProperty> wheelProp = new List<ITerminalProperty>();
            wheels[0].GetProperties(wheelProp);

            List<ITerminalAction> wheelActions = new List<ITerminalAction>();
            wheels[0].GetActions(wheelActions);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Properties: ");
            foreach(ITerminalProperty t in wheelProp)
            {
                sb.AppendFormat("\t{0}", t.Id).AppendLine();
            }

            sb.AppendLine("Actions: ");
            foreach(ITerminalAction a in wheelActions)
            {
                sb.AppendFormat("\t{0} ({1})", a.Name, a.Id).AppendLine();
                
            }

            sb.AppendFormat("\nHeight: {0}", wheels[0].Height).AppendLine();

            Me.CustomData = sb.ToString();

            checkOptions();
            getBlocks();
            Echo(DrawApp());
        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource) {


            OnUpdate(argument, updateSource);
            checkOptions();

            // Draw Methods
            Echo(DrawApp());
        }

        public void OnUpdate(string arg, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Terminal:

                    string[] argList = arg.Split(':');

                    switch (argList[0])
                    {
                        case "pause":
                            Runtime.UpdateFrequency &= ~UpdateFrequency.Update1;
                            break;
                        case "resume":
                            Runtime.UpdateFrequency |= UpdateFrequency.Update1;
                            break;
                        case "wheels":
                            switch (argList[1])
                            {
                                case "min":

                                   break;
                            }
                            break;
                    }

                    break;
                case UpdateType.Update1:
                    if (updateCount >= TEN_SECONDS)
                    {
                        updateCount = 0;
                        checkOptions();
                        getBlocks();
                    }

                    updateCount++;
                    runcount++;
                    break;
            }
        }

        public void getBlocks() {

            if (useTag)
            {
                StringBuilder sb = new StringBuilder();
                List<IMyInteriorLight> blocks = new List<IMyInteriorLight>();
                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => RLTAG_match.IsMatch(i.CustomName));
                ReverseLights = new LightGroup(reverse_light_Color);
                foreach(IMyInteriorLight i in blocks) { ReverseLights.AddLight(i); }
                ReverseLights.SetIntensity();

                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => RNTAG_match.IsMatch(i.CustomName));
                RunningLights = new LightGroup(run_light_Color);
                foreach (IMyInteriorLight i in blocks) { RunningLights.AddLight(i); }
                RunningLights.SetIntensity();

                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => TLTAG_match.IsMatch(i.CustomName));
                TailLights = new LightGroup(tail_light_Color);
                foreach (IMyInteriorLight i in blocks) { TailLights.AddLight(i); }
                TailLights.SetIntensity();
            }

            List<IMyTerminalBlock> tblocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tblocks);

            Wheels = new WheelGroup(defaultWheelHeight);

            foreach (IMyTerminalBlock t in tblocks)
            {
                string[] BlockType = t.GetType().ToString().Split('.');

                switch (BlockType[BlockType.Length - 1])
                {
                    case "MyRemoteControl":
                        break;
                    case "MyCockpit":
                        

                        break;
                    case "MyBatteryBlock":
                        Power.addBattery((IMyBatteryBlock)t);
                        break;
                    case "MySolarPanel":
                        Power.addSolar((IMySolarPanel)t);
                        break;
                    case "MyReactor":
                        Power.addReactor((IMyReactor)t);
                        break;
                    case "MyMotorSuspension":

                        break;
                    default:

                        break;
                }
            }
        }

        public void checkOptions()
        {
            if (useTag)
            {
                RLTAG_pattern = generateTag(reverse_light_TAG);
                RLTAG_match = new System.Text.RegularExpressions.Regex(RLTAG_pattern);
                RNTAG_pattern = generateTag(run_light_TAG);
                RNTAG_match = new System.Text.RegularExpressions.Regex(RNTAG_pattern);
                TLTAG_pattern = generateTag(tail_light_TAG);
                TLTAG_match = new System.Text.RegularExpressions.Regex(TLTAG_pattern);
            }
        }

        public string generateTag(string tag)
        {
           return @"(\[NKTC:" + tag + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        }


        // =======================================================================================
        //                                                                            --- Draw Methods ---
        // =======================================================================================

        public string DrawApp() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- NoobKeper Truck Control --").AppendLine();

            int runtime = runcount / 60;
            int runRemainder = runcount % 60;
            sb.AppendFormat("Uptime: {0}:{1}", runtime, runRemainder).AppendLine();

            int updateTime = 10 - (updateCount / 60);
            int updateRemainder = updateCount % 60;
            sb.AppendFormat("Next Update: {0}:{1} Sec", updateTime, updateRemainder).AppendLine().AppendLine();

            sb.AppendLine("Managing: ");
            sb.AppendFormat("Running Lights: {0}\n", RunningLights.lights.Count);
            sb.AppendFormat("Reverse Lights: {0}\n", ReverseLights.lights.Count);
            sb.AppendFormat("Tail Lights: {0}\n", TailLights.lights.Count);

            return sb.ToString();
        }
    }
}