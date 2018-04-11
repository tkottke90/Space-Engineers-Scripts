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
        *	N00bKeper Script Template
        *
        *	version: 1.0
        *	last update: 3/19/2018
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
        Color run_light_Color = new Color(255f, 128f, 0f);

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

        public Program()
        {
            // Runtime.UpdateFrequency = UpdateFrequency

            RunningLights = new LightGroup();
            ReverseLights = new LightGroup();
            TailLights = new LightGroup();

            Power = new EnergyGroup(this);

            checkOptions();
        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource) {


            List<IMyCockpit> cockpit = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpit);

            List<IMyTextPanel> lcd = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcd);

            List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(wheels);



            Vector3D prev = new Vector3D(0, 0, 0);

            IMyCockpit c = cockpit[0];
            double prevspeed = 0.0;
            // -------------------------------
            if ((runcount - updatecount) > 200) { 
                    MyBlockOrientation or = c.Orientation;

                double curspeed = c.GetShipSpeed();

                IMyMotorSuspension x;

                

                Vector3D cur = c.GetPosition();

                Vector3D delta = cur - prev;

                StringBuilder sb = new StringBuilder();

                string[] values = {
                        (delta.X/delta.Normalize()).ToString("N2"),
                        (delta.Y/delta.Normalize()).ToString("N2"),
                        (delta.Z / delta.Normalize()).ToString("N2")
                    };

                sb.AppendFormat(" Delta:\n  X  |  Y  |  Z  |\n{0,5}|{1,5}|{2,5}|", values).AppendLine();
                sb.AppendFormat("Speed:\n  P  |  C  |  D  |\n{0,5}|{1,5}|{2,5}", prevspeed.ToString("N2"), curspeed.ToString("N2"), (curspeed - prevspeed).ToString("N2")).AppendLine();

                if ((curspeed - prevspeed) < -1.0) sb.AppendLine("BRAKE");

                prev = cur;
                prevspeed = curspeed;
                LCD.WritePublicText(sb.ToString(), false);
                LCD.ShowPublicTextOnScreen();
                updatecount = runcount;
            }
            Echo("Script Running");

            runcount++;

            OnUpdate(argument, updateSource);
            checkOptions();
        }

        public void OnUpdate(string arg, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Terminal:

                    break;
                case UpdateType.Update1:

                    break;
            }
        }

        public void getBlocks() {

            if (useTag)
            {
                List<IMyInteriorLight> blocks = new List<IMyInteriorLight>();
                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => RLTAG_match.IsMatch(i.CustomName));
                foreach(IMyInteriorLight i in blocks) { ReverseLights.AddLight(i); }

                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => RNTAG_match.IsMatch(i.CustomName));
                foreach (IMyInteriorLight i in blocks) { RunningLights.AddLight(i); }

                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks, (IMyInteriorLight i) => TLTAG_match.IsMatch(i.CustomName));
                foreach (IMyInteriorLight i in blocks) { TailLights.AddLight(i); }
            }
            else
            {
                foreach(string blk in reverse_lights)
                {
                    

                }
            }

            List<IMyTerminalBlock> tblocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tblocks);

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
            sb.AppendLine("Managing: ");
            sb.AppendFormat("Running Lights: {0}", RunningLights.lights.Count);
            sb.AppendFormat("Reverse Lights: {0}", ReverseLights.lights.Count);
            sb.AppendFormat("Tail Lights: {0}", ReverseLights.lights.Count);

            return sb.ToString();
        }
    }
}