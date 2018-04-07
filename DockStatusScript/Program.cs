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
        // --- N00bKeper's Dock Bay Display Manager Script ---
        /*
         *   R e a d m e
         *   -----------
         * 
         *   Script designed to manage docking bays in a base using Sensors, LCD Panels, and Connectors.  Using the Cross and Arrow
         *   images, it provides a quick way to check if a ship is docked in a bay.
         *    
         *   To setup script:
         *   
         *      1) Build PB on base, station, or large ship with docking ports for smaller ships/rovers
         *      2) Build LCD Panels around bay so they are visible to engineers when they enter the docking space
         *      3) Setup one of the following sensor configurations:
         *          3a) Setup a sensor that encompasses the size of the bay
         *              ** Note that if the ship connects to a connector, it is concidered part of the grid it is attached to which means that a small
         *              ship attached to a large grid is no longer sensed by a sensor looking for small ships
         *          3b) Setup a connector in the bay that the ship can connect to
         *              ** Note that without a senor a ship could sit in a bay without being connected to the connector
         *          3c) Setup 1 sensor and 1 connector in the bay.  Configure the the sensor as in 3a.
         *      4) Assign Sensor, Connector, and any LCDs related to this bay to a unique group
         *      5) Add that group name to the list in the configuration section
         *          
         */

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

            Echo(DrawApp());

            foreach (DockBayGroup g in myGroups)
            {
                g.update();
                Echo(g.DrawDebug());
            }
            
            
            
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