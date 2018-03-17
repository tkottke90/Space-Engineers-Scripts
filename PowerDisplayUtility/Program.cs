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
        // App:
        int REFRESH = 62;
        int Stage = 0;

        // DEBUG:
        StringBuilder debugSB = new StringBuilder();

        // Groups
        EnergyGroup POWER;        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            POWER = new EnergyGroup(this);
            getScriptBlocks();

            Me.CustomName = "Programmable Block [PowerDisplayUtility]";
            Echo(DrawApp());
        }

        public void Save()
        {
            
        }

        public void Main(string argument)
        {
            if(Stage == REFRESH)
            {
                POWER.clear();
                getScriptBlocks();
                Stage = 0;
            }

            POWER.terminalDisplay();
            

            Stage++;
            Me.CustomData = debugSB.ToString();
        }

        void getScriptBlocks()
        {
            List<IMyTerminalBlock> Script_Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Script_Blocks);

            foreach (IMyTerminalBlock b in Script_Blocks)
            {
                var BlockType = b.GetType().ToString().Split('.');

                switch (BlockType[BlockType.Length - 1])
                {
                    case "MyBatteryBlock":
                        if (b.CubeGrid.ToString() == Me.CubeGrid.ToString()) { POWER.addBattery((IMyBatteryBlock)b); }
                        break;
                    case "MyReactor":
                        if (b.CubeGrid.ToString() == Me.CubeGrid.ToString()) { POWER.addReactor((IMyReactor)b); }
                        break;
                    case "MySolarPanel":
                        if (b.CubeGrid.ToString() == Me.CubeGrid.ToString()) { POWER.addSolar((IMySolarPanel)b); }
                        break;
                    default:
                        
                        break;
                }
            }
        }

        string DrawApp()
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine("-- Power Display Utility --").AppendLine();
            output.AppendLine("Reporting:").AppendLine(" - Solar: " + POWER.solars.Count).AppendLine(" - Batteries: " + POWER.batteries.Count).AppendLine(" - Reactors: " + POWER.reactors.Count);

            output.AppendLine("Power Trend: " + ( POWER.mathConvertWatt(POWER.CurrentOutput)));

            return output.ToString();
        }
    }
}