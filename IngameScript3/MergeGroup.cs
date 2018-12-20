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
        public class MergeBlockGroup
        {
            List<IMyShipMergeBlock> group = new List<IMyShipMergeBlock>();

            public MergeBlockGroup() { }

            public void Add(IMyShipMergeBlock merge)
            {
                group.Add(merge);
                merge.Enabled = true;
            }

            public void Add(List<IMyShipMergeBlock> merge)
            {
                group = merge;
            }


            // Group should only contain available merge blocks.  Function checks if any of the current block connections status' have changed
            public bool newConnection()
            {
                foreach (IMyShipMergeBlock b in group)
                {
                    if (b.IsConnected) return true;
                }

                return false;
            }

            public string getBlockList()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Merge Blocks:");
                foreach (IMyShipMergeBlock b in group)
                {
                    sb.AppendFormat("   {0} - Enabled [{1}]", b.CustomName, b.Enabled ? "X" : " ").AppendLine();
                }

                return sb.ToString();
            }

            public static Vector3D getPointInFront(Vector3D directionalVector, Vector3D blockPos, int distance)
            {
                return directionalVector * distance + blockPos;
            }
        }
    }
}
