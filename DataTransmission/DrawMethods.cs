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


    }
}
