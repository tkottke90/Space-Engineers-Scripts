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
        public class ThrusterGroup
        {
            public List<IMyThrust> group = new List<IMyThrust>();
            string group_name;

            public int overideTime = 0;

            public ThrusterGroup(string name)
            {
                this.group_name = name;
            }

            public int getThrusterCount() { return group.Count; }

            public void Add(IMyThrust t)
            {
                this.group.Add(t);
                t.Enabled = false;
            }

            public void Clear() {
                this.group.Clear();
            }

            public bool isEnabled() {
                if (group.Count > 0)
                {
                    return group[0].Enabled;
                } else { return false; }
            }

            public float getMaxThrust()
            {
                if (group.Count > 0)
                {
                    return group[0].MaxThrust;
                } else { return 0.0f; }
            }

            public float getMinThrust()
            {
                return 0.0f;
            }

            public void Enable()
            {
                foreach(IMyThrust t in group)
                {
                    t.Enabled = true;
                }
            }

            public void Disable()
            {
                foreach(IMyThrust t in group)
                {
                    t.Enabled = false;
                }
            }

            public void setThrust(float thrust)
            {
                foreach(IMyThrust t in group)
                {
                    t.ThrustOverride = thrust;
                }
            }

            public void EnableForward(Base6Directions.Direction remote) {
                foreach(IMyThrust t in group)
                {
                    if (t.Orientation.Forward == Base6Directions.GetFlippedDirection(remote))
                    {
                        t.Enabled = true;
                    } else
                    {
                        t.Enabled = false;
                    }
                }
            }

            public void logEnabled() {
                if (group[1].Enabled)
                {
                    overideTime++;
                }
            }
        }
    }
}
