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

        // --- Block Groups ---
        // =======================================================================================
        // Enter names of block groups to be handed by script
        // Example: string[] groups = { "DockingBay1", "TruckBayA" };
        string[] groups = { "Truck Bay 1 Status", "Truck Bay 2 Status" };

        // =======================================================================================

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================

        List<DockBayGroup> myGroups = new List<DockBayGroup>();


        StringBuilder errors = new StringBuilder();
        StringBuilder notify = new StringBuilder();

        public Program()
        {


            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            getBlockGroups();

            Me.CustomName = "Programmable Block [DBM]";
        }

        [Flags]
        public enum Test {
            off = 0,
            on = 1,
            ready = 2
        }

        public void Main(string argument, UpdateType updateSource)
        {             
            errors = new StringBuilder();
            notify = new StringBuilder();
            
            foreach(DockBayGroup g in myGroups)
            {
                g.update();
            }
            
            Echo(DrawApp());
        }

        void getBlockGroups()
        {
            foreach (string group in groups)
            {
                IMySensorBlock s = null;
                IMyShipConnector c = null;
                List<IMyTextPanel> t = new List<IMyTextPanel>();

                IMyBlockGroup bGroup = GridTerminalSystem.GetBlockGroupWithName(group);
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                bGroup.GetBlocks(blocks);

                foreach (IMyTerminalBlock b in blocks)
                {
                    string[] blockName = b.GetType().ToString().Split('.');
                    switch (blockName[blockName.Length - 1])
                    {
                        case "MyTextPanel":
                            t.Add((IMyTextPanel)b);
                            break;
                        case "MySensorBlock":
                            s = (IMySensorBlock)b;
                            break;
                        case "MyShipConnector":
                            c = (IMyShipConnector)b;
                            break;
                    }
                }

                if (s != null && c != null)
                {
                    myGroups.Add(new DockBayGroup(s, c, t));
                } else if (s != null)
                {
                    myGroups.Add(new DockBayGroup(s, t));
                } else if (c != null)
                {
                    myGroups.Add(new DockBayGroup(c, t));
                } else
                {
                    errors.AppendLine("No Connector/Sensor in Block Group: " + group);
                }

            }
        }


        public string DrawApp()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--- Docking Bay Manager ---").AppendLine();

            if (errors.Length > 0)
            {
                sb.AppendLine(errors.ToString()).AppendLine();
            }

            sb.AppendLine("Bays Managed: " + myGroups.Count);
            

            return sb.ToString();
        }

    }


}