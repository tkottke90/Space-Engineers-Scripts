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
        public class TankGroup
        {
            public List<IMyGasTank> group = new List<IMyGasTank>();
            public string group_name = "";


            public TankGroup(string name) {
                group_name = name;
            }

            public bool Add(IMyGasTank tank)
            {
                int groupCount = group.Count;
                group.Add(tank);
                return group.Count == (groupCount + 1);
            }

            public void Clear() {
                group.Clear();
            }

            public double getFill()
            {
                if (group.Count == 0)
                {
                    return 0.0d;
                }

                double fill = 0.0d;
                foreach (IMyGasTank t in group)
                {
                    fill += t.FilledRatio;
                }

                fill = fill / group.Count;

                return fill;
            }

        }
    }
}
