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
        public class LCDGroup
        {
            Program _program;
            public string group_name = "";
            public List<IMyTextPanel> group { get; } = new List<IMyTextPanel>();

            public LCDGroup(string groupName, Program p)
            {
                group_name = groupName;
                _program = p;
            }

            public bool Add(IMyTextPanel txt)
            {
                int groupCount = group.Count;
                group.Add(txt);
                return group.Count == (groupCount + 1);
            }

            public void writeToLCD(string output)
            {
                foreach (IMyTextPanel lcd in group)
                {
                    ((IMyTextPanel)lcd).WritePublicText(output, false);
                    ((IMyTextPanel)lcd).ShowPublicTextOnScreen();
                }
            }

            public void writeToLine(string output)
            {
                foreach (IMyTextPanel lcd in group)
                {
                    string txtOut = output + "\n";
                    ((IMyTextPanel)lcd).WritePublicText(output, true);
                    ((IMyTextPanel)lcd).ShowPublicTextOnScreen();
                }
            }
        }
    }
}
