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
        public class CameraGroup
        {
            Program _program;
            public List<IMyCameraBlock> group { get; } = new List<IMyCameraBlock>();

            public CameraGroup(Program p) { _program = p; }

            public void Add(IMyCameraBlock cam)
            {
                cam.ApplyAction("OnOff_On");
                cam.EnableRaycast = true;
                group.Add(cam);
            }

            public bool hasCamera() {
                return group.Count > 0 || group[0] == null;
            }

            public GPSlocation scan(IMyCameraBlock cam)
            {
                if (cam.CanScan(_program.SCAN_DISTANCE))
                {
                    MyDetectedEntityInfo info = cam.Raycast(_program.SCAN_DISTANCE, _program.PITCH, _program.YAW);
                    if (info.HitPosition.HasValue)
                    {
                        GPSlocation ent = new GPSlocation(info.EntityId.ToString(), info.Position);
                        ent.setCustomInfo("Type", info.Type.ToString(), true);
                        ent.setCustomInfo("Size", (info.BoundingBox.Size.ToString("0.00")), true);
                        
                        ent.setCustomInfo("DisplayName", (info.Name), true);
                        if (info.Relationship.ToString() != "NoOwnership")
                        {
                            ent.setCustomInfo("Owner", (info.Relationship.ToString()), true);
                        }
                        if (info.Velocity != new Vector3(0.0f, 0.0f, 0.0f))
                        {
                            ent.setCustomInfo("Velocity", info.Velocity.ToString("0.000"), true);
                        }
                        return ent;
                    }
                }
                return null;
            }
        }
    }
}
