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
         *   CONFIG:
         *   -------
         *   
         *   Edit these valiables to change how the script interacts with other blocks on your grid
         */

        // Grid Name - Used to let other grids know who is transmitting
        string GRID_NAME = "";

        // Script Tag - Used to label blocks used by this script (Recompile Required to Update)
        const string SCRIPT_TAG = "RADIO";


        // DO NOT EDIT BELOW THIS LINE!
        /*----------------------------------------------------------------------------------------------------------------------------*/


        // System
        const string VERSION = "v0.1";
        DateTime scriptStartTime;
        int runTimeCount = 0;

        CustomDataStatus status;
        public enum CustomDataStatus
        {
            Settings,
            Message
        }

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

        // Data
        Queue<string> messages;

        

        public Program()
        {
            // Setup System:
            scriptStartTime = DateTime.Now;
            status = CustomDataStatus.Settings;

            logs.AddLog("Debug");
            logs.AddLog("Notify");
            logs.AddLog("Event");

            // Init Script Tags
            tag_match = new System.Text.RegularExpressions.Regex(tag_pattern);

            // Init Block Groups

            // Init Data
            messages = new Queue<string>();

            // Draw Screens
            
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

        public void Main(string argument, UpdateType updateSource)
        {
            // Check Update Source:
            if ((updateSource & UpdateType.Antenna) != 0)
            {
                messages.Enqueue(argument);
            } else if((updateSource & UpdateType.Terminal) != 0)
            {
                handleCommand(argument);
            } else if ((updateSource & (UpdateType.Update1 | UpdateType.Update10)) != 0)
            {

            }

            if (logs.LogSize("Notify") == 0 & OddBlocks.Count == 0) { Echo(DrawApp()); } else { Echo(DrawAppErrors()); }
            Echo(DrawDev());

            runTimeCount++;
        }

        public void handleMessage(string message)
        {

        }

        public void handleCommand(string argument)
        {
            eventLog.Add(eventLog.Count, argument);

            string[] args = argument.Split(':');
            switch (args[0])
            {
                case "CONFIG":
                    status = CustomDataStatus.Settings;
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
                                    //LCDDisplay.Add((IMyTextPanel)b);
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