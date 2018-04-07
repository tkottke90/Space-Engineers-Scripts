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
        public class MyLogger
        {
            public string name = "";

            List<string> logs = new List<string>();

            public MyLogger(string n) {
                name = n;
            }

            public string DrawLog() {
                StringBuilder output = new StringBuilder();
                output.AppendFormat("-- Logger ({0}) Output --", name);

                for (int i = 0; i < logs.Count; i++)
                {
                    output.AppendFormat("{0}) {1}", i, logs[i]);
                }

                return output.ToString();
            }

            public string DumpLog() {
                StringBuilder output = new StringBuilder();

                return output.ToString();
            }
            
        }
    }
}
