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
        //                                                                            --- Configuration ---
        // =======================================================================================

        // --- Tempate Setting ---
        // =======================================================================================
        // Description: 
        // Example:

        // --- Update Frequency ---
        // =======================================================================================
        // Description: How many ticks should the script wait before updating
        int update = 600;

        // --- LCDs ---
        // =======================================================================================
        // Description: Manages Tail Lights.  Changes the Intensity when a deceleration is detected
        // Example with Tag: Interior Light [NKTC:TailLight]
        // Example with list: string[] tail_lights = { "Interior Light", "Interior Light 1" };
        string LCD_TAG = "LCD";
        string[] tail_lights = { };


        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================

        // System
        string scriptName = "NKCD";
        int runcount = 0;
        int updateCount = 0;

        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        LCDGroup lcd;
        LCDGroup debug;

        System.Text.RegularExpressions.Regex tag_match;

        public Program()
        {
            Me.CustomName = "Programmable Block [Block Name Utility]";
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            updateCount = update;

            lcd = new LCDGroup("data", this);
            debug = new LCDGroup("debug", this);

            tag_match = new System.Text.RegularExpressions.Regex(generateTag(LCD_TAG));

            getBlocks();
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            OnUpdate(argument, updateSource);


            lcd.writeToLCD(DrawBlocks());
            debug.writeToLCD(DrawDebug());
            Echo(DrawApp());
        }

        public void OnUpdate(string arg, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Terminal:

                    break;
                case UpdateType.Update1:
                    if (updateCount >= update)
                    {
                        updateCount = 0;
                        lcd = new LCDGroup("lcd", this);
                        getBlocks();
                    }

                    updateCount++;
                    runcount++;
                    break;
            }
        }

        public void getBlocks()
        {

            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (IMyTerminalBlock t) => t.CubeGrid == Me.CubeGrid);

            foreach (IMyTerminalBlock t in blocks)
            {

                string[] BlockTags = tag_match.Match(t.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(':');
                string[] BlockType = t.GetType().ToString().Split('.');

                switch (BlockType[BlockType.Length - 1])
                {
                    case "MyTextPanel":

                        string args1 = BlockTags[1];
                        string name = t.CustomName.Replace(tag_match.Match(t.CustomName).Value, "");
                        name += "[" + scriptName + ":" + args1 + "]";
                        t.CustomName = name;
                        switch (args1)
                        {
                            case "DEBUG":
                                debug.Add((IMyTextPanel)t);
                                break;
                            case "LCD":
                                lcd.Add((IMyTextPanel)t);
                                break;
                        }

                        break;

                }
            }
        }

        public string generateTag(string tag)
        {
            return @"(\[NKCD:" + tag + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        }

        public class LCDGroup
        {
            Program _program;
            public string group_name = "";
            public List<IMyTextPanel> group { get; } = new List<IMyTextPanel>();

            public LCDGroup(string groupName, Program p)
            {
                group_name = groupName;
                _program = p;
            }

            public bool Add(IMyTextPanel txt)
            {
                int groupCount = group.Count;
                group.Add(txt);
                return group.Count == (groupCount + 1);
            }

            public void writeToLCD(string output)
            {
                foreach (IMyTextPanel lcd in group)
                {
                    ((IMyTextPanel)lcd).WritePublicText(output, false);
                    ((IMyTextPanel)lcd).ShowPublicTextOnScreen();
                }
            }

            public void writeToLine(string output)
            {
                foreach (IMyTextPanel lcd in group)
                {
                    string txtOut = output + "\n";
                    ((IMyTextPanel)lcd).WritePublicText(output, true);
                    ((IMyTextPanel)lcd).ShowPublicTextOnScreen();
                }
            }
        }


        // Draw Methods:

        public string DrawApp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- NoobKeper Construction Display --").AppendLine();

            int runtime = runcount / 60;
            int runRemainder = runcount % 60;
            sb.AppendFormat("Uptime: {0}:{1}", runtime, runRemainder).AppendLine();

            int updateTime = 10 - (updateCount / 60);
            int updateRemainder = updateCount % 60;
            sb.AppendFormat("Next Update: {0}:{1} Sec", updateTime, updateRemainder).AppendLine().AppendLine();

            sb.AppendFormat("Blocks:\t{0}\n", blocks.Count);
            sb.AppendFormat("LCDs:\t{0}", lcd.group.Count);
      

            return sb.ToString();
        }

        public string DrawDebug() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- N00bKeper Debug -- ").AppendLine();

            return sb.ToString();
        }

        public string DrawBlocks() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Count\t|\tName");
            for (var i = 0; i < blocks.Count; i++) {
                sb.AppendFormat("{0}\t|\t{1}\n", i, blocks[i].CustomName);
            }
            return sb.ToString();
        }
    }
}