﻿using Sandbox.Game.EntityComponents;
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
        public string DrawApp()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Info Transmission Script");
            output.AppendLine("Script Version: " + VERSION);
            output.AppendLine("Script Start Time: " + scriptStartTime.ToString());
            output.AppendLine("Script Runcount: " + runTimeCount);

            return output.ToString();
        }

        public string DrawAppErrors()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("### Script Error ###").AppendLine();
            output.AppendLine(notifySB.ToString());
            output.AppendLine().AppendLine("####################");
            output.AppendLine().AppendLine(DrawApp());
            return output.ToString();
        }

        public string DrawDev()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine().AppendLine("Dev Values:");
            output.AppendLine("Instruction Count:" + Runtime.CurrentInstructionCount);
            output.AppendLine("Last Run Time (milliseconds): " + Runtime.LastRunTimeMs);
            return output.ToString();
        }

        public string DrawMessageTemplate()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("From: " + GRID_NAME).AppendLine();
            output.AppendLine("Message: (Start On Next Line Down)").AppendLine();

            return output.ToString();
        }

        public string DrawConfig()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("--- Data Transmit Script Config ---");
            output.AppendLine("Grid Name: " + GRID_NAME);

            return output.ToString();
        }

        public void ReadConfig()
        {
            string[] config = Me.CustomData.Split('\n');

            // Grid Name
            GRID_NAME = config[1].Split(':')[1]; 
        }
    }
}
