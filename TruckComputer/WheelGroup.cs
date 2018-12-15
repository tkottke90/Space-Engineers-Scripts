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
        public class WheelGroup
        {
            public List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();

            public float defaultHeight = 0.2f;

            public WheelGroup(float height = 0.2f) {
                this.defaultHeight = height;
            }

            public void setHeight() { }

            public void setPower() { }

            public void setStrength() { }


        }
    }
}
