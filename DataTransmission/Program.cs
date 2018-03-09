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

        // Script Tag - Used to label blocks used by this script
        const string SCRIPT_TAG = "RADIO";


        // DO NOT EDIT BELOW THIS LINE!

        // Logging
        StringBuilder debugSB = new StringBuilder();
        StringBuilder notifySB = new StringBuilder();
        List<IMyTerminalBlock> OddBlocks = new List<IMyTerminalBlock>();

        // Script Tags
        string tag_pattern = @"(\[" + SCRIPT_TAG + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        System.Text.RegularExpressions.Regex tag_match;

        // Block Groups

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
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
                        break;
                    case "MyLaserAntenna":
                        break;
                    case "MyRadioAntenna":
                        break;
                    default:
                        OddBlocks.Add(b);
                        debugSB.AppendLine(b.CustomName + " is not used by this script");
                        break;
                }
            }
            if (OddBlocks.Count > 0) { notifySB.AppendLine(OddBlocks.Count + " invalid blocks with tag"); }
        }
    }
}