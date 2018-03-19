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
            // Sets UpdateFrequency property of the PB Runtime to run once
            Runtime.UpdateFrequency = UpdateFrequency.Once;

            // Builds the Pattern used by the script to identify which blocks to grab
            tag_pattern = @"(\[" + SCRIPT_TAG + @")(\s\b[a-zA-z-]*\b)*(\s?\])";

            // Creates the Regular Expression that will be used to check if a blocks name (Me.CustomName) has the tag
            tag_match = new System.Text.RegularExpressions.Regex(tag_pattern);

        }

        public void Main()

        {
            // Create a list that will store the interior lights that have the matching tag
            List<IMyInteriorLight> intLights = new List<IMyInteriorLight>();

            // Method of the Grid the PB is attached to.  The GetBlocksofType allows for the selection of a specific block type which
            //   allows us to be specific about the blocks we want to work with.
            //
            // GetBlocksOfType takes 1-2 parameters, the first is a list of the type of blocks that will be pulled.  This is what we created
            //   the List<IMyInteriorLight> intLights for.  The second is an optional parameter that allows us to pass a function which will
            //   allow us to evalue each item (block) found by the method.  

            //  We cast the block found as a IMyInteriorLight and then use the lamba expression '=>' which is similar to using blocks '{ }', 
            //    then give the method an evaluator.  In this case we are using the Regular Expression we setup in the Construction (Program()) to 
            //    determine if the block's CustomName includes the tag for the script.  Example:
            //
            //      Interior Light 1
            //      Interior Light 2 [AVLIGHT]
            //      Interior Light Base [AVLIGHT]
            //      Interior Light Hanger1
            //
            //  In the above list, only Interior Light 2 and Interior Light Base will be selected because they have the tag which means the IsMatch function
            //    will evaluate true.
            //
            GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(intLights, (IMyInteriorLight i) => tag_match.IsMatch(i.CustomName));

            // Check if there were any interior lights found
            if (intLights.Count > 0)
            {
                // If lights were found loop through all blocks in the intLights list and set the Offset Value to the Offset Value setup in the config.
                foreach (IMyInteriorLight light in intLights)
                {
                    // Since IMyInteriorLight does not currently have a Offset property available, the SetValue method has to be used.
                    light.SetValue("Offset", OFFSET);
                }

                // Draw to the DetailedInfo space - will return successful run of the script
                Echo(DrawApp());
            }
            else
            {
                // Draw to the DetailedInfo space - will return error since no Interior Lights were found
                Echo(DrawApp("No Lights Found!!\nPlease Tag Managed Lights and Click Run"));
            }

        }

        /// <summary>
        /// Method is designed to draw information into the DetailedInfo field of the Programmable Block using the Echo() method.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns string formatted for the Echo Method</returns>
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