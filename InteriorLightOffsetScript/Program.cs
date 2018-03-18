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

        // --- Script Tag ---
        // =======================================================================================
        // Add tag to Interior Light Blocks you wish for this script to control.
        // Example: Interior Light 1 [AVLIGHT]
        string SCRIPT_TAG = "AVLIGHT";

        // =======================================================================================

        // --- Light Offset ---
        // =======================================================================================
        // Enter the offset value as a number plus 'f' (sets the number as a float)
        // Example: float OFFSET = 5f;
        const float OFFSET = 5f;

        // =======================================================================================

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================


        string tag_pattern;
        System.Text.RegularExpressions.Regex tag_match;

        public Program()

        {
            Runtime.UpdateFrequency = UpdateFrequency.Once;
            tag_pattern = @"(\[" + SCRIPT_TAG + @")(\s\b[a-zA-z-]*\b)*(\s?\])";
            tag_match = new System.Text.RegularExpressions.Regex(tag_pattern);

        }

        public void Main()

        {
            List<IMyInteriorLight> intLights = new List<IMyInteriorLight>();
            GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(intLights, (IMyInteriorLight i) => tag_match.IsMatch(i.CustomName));

            if (intLights.Count > 0)
            {
                ITerminalProperty<float> x = intLights[0].GetProperty("Offset").AsFloat();

                foreach (IMyInteriorLight light in intLights)
                {
                    x.SetValue(light, OFFSET);
                }

                Echo(DrawApp());
            }
            else
            {
                Echo(DrawApp("No Lights Found!!\nPlease Tag Managed Lights and Click Run"));
            }

        }

        public string DrawApp(string message = "")
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine("-- NK Light Update Script --").AppendLine();

            if (message.Length == 0)
            {
                output.AppendLine("Run Successful - Lights Updated");
            }
            else
            {
                output.AppendLine(message);
            }
            return output.ToString();
        }
    }
}