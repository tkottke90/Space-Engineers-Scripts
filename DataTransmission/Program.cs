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
        *	N00bKeper Data Transmission Script
        *
        *	version: 1.0
        *	last update: 3/19/2018
        *
        *	description: Script to allow the transmission of data to and from grid
        *
        */


        // =======================================================================================
        //                                                                            --- Configuration ---
        // =======================================================================================

        // --- Grid Name ---
        // =======================================================================================
        // Description: Used to give grid special name.  If blank will use CubeGrid ID
        // Example: string GRID_NAME = "Gypsy Danger";
        string GRID_NAME = "";

        // --- Script Tag ---
        // =======================================================================================
        // Description: Tag added to blocks controlled by this script.  By Default the value is RADIO.
        // Example: const string SCRIPT_TAG = "TRANSMIT"
        const string SCRIPT_TAG = "RADIO";


        

        // System
        const string VERSION = "v0.1";
        DateTime scriptStartTime;
        int runTimeCount = 0;

        // Logging
        //StringBuilder debugSB = new StringBuilder();
        //StringBuilder notifySB = new StringBuilder();
        Dictionary<int, string> eventLog = new Dictionary<int, string>();
        List<IMyTerminalBlock> OddBlocks = new List<IMyTerminalBlock>();

        MyLogger logs = new MyLogger();

        // Script Tags
        string tag_pattern = @"(\[" + SCRIPT_TAG + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        System.Text.RegularExpressions.Regex tag_match;

        // Block Groups
        LCDGroup status;

        // Data
        System.Text.RegularExpressions.Regex dataFlag = new System.Text.RegularExpressions.Regex(@"(<[a-zA-Z0-9]*>)([\sa-zA-Z0-9:\n\-\%]*)");

        Queue<string> inbound;
        Queue<string> outbound;

        

        public Program()
        {
            // Setup System:
            scriptStartTime = DateTime.Now;

            logs.AddLog("Debug");
            logs.AddLog("Notify");
            logs.AddLog("Event");

            // Init Script Tags
            tag_match = new System.Text.RegularExpressions.Regex(tag_pattern);

            // Init Block Groups
            status = new LCDGroup("Display", this);

            // Init Data
            inbound = new Queue<string>();
            outbound = new Queue<string>();
            
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {

            MyData test = new MyData("test");

            Vector3D beta = new Vector3D(268, 21, 4660);
            test.AddData("GPS-Location", beta.ToString());

            Echo(test.ToString());

            Me.CustomData = Storage;

            Storage += test.ToString();



            // Check Update Source:
            if ((updateSource & UpdateType.Antenna) != 0)
            {
                inbound.Enqueue(argument);
            } else if((updateSource & UpdateType.Terminal) != 0)
            {
                // handleCommand(argument);

                
            } else if ((updateSource & (UpdateType.Update1 | UpdateType.Update10)) != 0)
            {

            }

            if (logs.LogSize("Notify") == 0 & OddBlocks.Count == 0) { Echo(DrawApp()); } else { Echo(DrawAppErrors()); }
            Echo(DrawDev());
            
            runTimeCount++; 
        }

        public void handleMessage(string message)
        {
            string data = Storage;
            Echo(data);
            Dictionary<string, string> test = new Dictionary<string, string>();

            System.Text.RegularExpressions.Regex start = new System.Text.RegularExpressions.Regex(@"(<[a-zA-Z0-9]*>)");
            string endTag = @"";
            System.Text.RegularExpressions.Regex dat;

            while (data.Length != 0)
            {
                // Find Next Tag
                string tag = start.Match(data).Value;
                // Generate End Tag
                endTag = @"(</" + tag.Replace("<", "").Replace(">", "") + @">)";
                // Build Regex for Data
                dat = new System.Text.RegularExpressions.Regex(tag + @"[a-zA-Z0-9:\s<>/!?]*" + endTag);

                string result = dat.Match(data).Value;

                Me.CustomData += "Result: \n   " + result + "\n";
                try
                {
                    Storage = data.Replace(result, "");
                } catch (Exception e) { Echo("Empty Result"); }
                Echo(runTimeCount.ToString());
                Echo("Pattern: " + dat.ToString());
                Echo("Tag: " + tag);
                Echo(Storage);
            }
            
            

        }

        public void handleCommand(string argument)
        {
            eventLog.Add(eventLog.Count, argument);

            string[] args = argument.ToUpper().Split(':');
            switch (args[0])
            {
                case "CONFIG":
                    //status = CustomDataStatus.Settings;
                    Me.CustomData = DrawConfig();
                    break;
                case "Save":
                    ReadConfig();
                    break;
                case "NEW":

                    break;
                case "TRANSMIT":
                    
                    break;
                
                default:
                    eventLog.Add(eventLog.Count, "**** NO COMMAND FOUND: " + argument + " ****");
                    break;  
            }
        }

        public void getScriptBlocks()
        {
            List<IMyTerminalBlock> Script_Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Script_Blocks, (IMyTerminalBlock t) => tag_match.IsMatch(t.CustomName));

            foreach (IMyTerminalBlock b in Script_Blocks)
            {
                var BlockType = b.GetType().ToString().Split('.');
                string[] BlockTags = tag_match.Match(b.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(' ');
                BlockTags.DefaultIfEmpty("");

                switch (BlockType[BlockType.Length - 1])
                {
                    case "MyTextPanel":
                        try
                        {
                            string args1 = BlockTags[1];
                            string name = b.CustomName.Replace(tag_match.Match(b.CustomName).Value, "");
                            name += "[" + SCRIPT_TAG + " " + args1 + "]";
                            b.CustomName = name;
                            switch (args1)
                            {
                                case "DEBUG":
                                    //LCDDebug.Add((IMyTextPanel)b);
                                    break;
                                case "DISPLAY":
                                    status.Add((IMyTextPanel)b);
                                    break;
                            }
                        }
                        catch (Exception e) { logs.WriteLog( "Debug", "Error Adding Text Panel(" + (b.CustomName != null ? b.CustomName : "gyro") + ") : " + e.Message); }
                        break;
                    default:
                        OddBlocks.Add(b);
                        logs.WriteLog("Debug", b.CustomName + " is not used by this script");
                        break;
                }
            }
            if (OddBlocks.Count > 0) { logs.WriteLog("Notify", OddBlocks.Count + " invalid blocks with tag"); }
        }
    }
}