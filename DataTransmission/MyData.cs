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
        public class MyData
        {
            string name = "";
            Dictionary<string, string> data = new Dictionary<string, string>();

            public MyData(string dataName)
            {
                this.name = dataName;
            }



            override
            public string ToString()
            {
                StringBuilder output = new StringBuilder();

                // Add Data Name:
                output.AppendFormat("<{0}>", name);

                output.AppendFormat("</{0}>", name);
                return output.ToString();
            }
        }
    }
}
