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
    partial class Program
    {
        public string DrawApp() {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Keper AI Script");
            output.AppendLine("Script Version: " + VERSION);
            output.AppendLine("Script Start Time: " + scriptStartTime.ToString());
            output.AppendLine("Script Runcount: " + runtime_count);
            output.AppendLine("");
            output.AppendLine("Status: " + Enum.GetName(typeof(ProgramStates), _CurrentState.state));


            return output.ToString();
        }

        public string DrawAppErrors() {
            StringBuilder output = new StringBuilder();
            output.AppendLine("### Script Error ###").AppendLine();
            output.AppendLine(notifySB.ToString());
            output.AppendLine().AppendLine("####################");
            output.AppendLine().AppendLine(DrawApp());
            return output.ToString();
        }

        public string DrawDisplay() {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Keper AI Script");
            output.AppendLine("");
            output.AppendLine("Blocks Assigned: ");
            output.AppendLine("   [" + (Remote != null ? "X" : " ") + "] - Remote Control");
            output.AppendLine("   [" + (Cameras.hasCamera() ? "X" : " ") + "] - Cameras");
            output.AppendLine("   [" + (SG.activeGyro != null ? "X" : " ") + "] - Script Gyro");
            return output.ToString();
        }

        public string DrawDebug() {
            StringBuilder output = new StringBuilder();
            output.AppendLine(debugSB.ToString());
            return output.ToString();
        }

        public string DrawScan() {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Keper AI Raycast Scanner").AppendLine();
            if (Cameras.hasCamera()) {
                output.AppendLine("Status: " + Enum.GetName(typeof(ProgramStates), _CurrentState.state));
                output.AppendLine("Scan Distance: " + SCAN_DISTANCE);
                output.AppendLine("Time Till Next Scan: " + (Cameras.group[0].TimeUntilScan(SCAN_DISTANCE) / 1000) + " sec");
            } else {
                output.AppendLine("Status: No Cameras Available").AppendLine().AppendLine();
            }
            output.AppendLine("");
            output.AppendLine().AppendLine("Located Asteriods: " + asteriods.Count);
            output.AppendLine("Located Ships: " + ships.Count);
            output.AppendLine("Located Stations: " + stations.Count);

            

            return output.ToString();
        }

        public string DrawRemote() {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Keper AI Remote Control Status").AppendLine();
            output.AppendLine("Remote Block Name: " + Remote.CustomName);
            output.AppendLine("Remote Autopilot Status: " + Remote.IsAutoPilotEnabled);
            return output.ToString();
        }

        public string DrawEnergy() {
            StringBuilder output = new StringBuilder();

            output.AppendLine("Keper AI Power Status")
                  .AppendLine("Batteries: " + Power.batteries.Count)
                  .AppendLine("Reactors: " + Power.reactors.Count)
                  .AppendLine("Solar Panels: " + Power.solars.Count);



            return output.ToString();
        }

        public string DrawDev() {
            StringBuilder output = new StringBuilder();
            output.AppendLine().AppendLine("Dev Values:");
            output.AppendLine("Instruction Count:" + Runtime.CurrentInstructionCount);
            output.AppendLine("Last Run Time (milliseconds): " + Runtime.LastRunTimeMs);
            output.AppendLine("Current State Task Complete: " + _CurrentState.TaskComplete);
            return output.ToString();
        }
    }
}
