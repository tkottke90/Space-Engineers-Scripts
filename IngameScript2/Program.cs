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

        // --- LCD Name ---
        // =======================================================================================
        // Description: 
        // Example: 
        string panel_name = "LCD Panel";


        // --- <Configurable Variable> ---
        // =======================================================================================
        // Description: 
        // Example: 

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================


        public Program()
        {

            // Runtime.UpdateFrequency = UpdateFrequency



        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource)
        {
            OnUpdate(argument, updateSource);
        }

        public void OnUpdate(string arg, UpdateType updateSource)
        {

            IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(panel_name) as IMyTextPanel;

            if (updateSource == UpdateType.Terminal)
            {

                
                IMyRadioAntenna ant = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Transmission Status: {0}\n", ant.TransmitMessage(arg, MyTransmitTarget.Everyone));
                Echo(sb.ToString());
            }

            if (updateSource == UpdateType.Antenna)
            {
                Echo("Message Recieved, See LCD: \n [" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] -" + panel_name);

                lcd.WritePublicText(arg);
                lcd.ShowPublicTextOnScreen();
            }
        }

        public string generateTag(string tag)
        {
            return @"(\[" + tag + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
        }

        // Draw Methods
    }
}