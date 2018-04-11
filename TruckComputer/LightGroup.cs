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
        public class LightGroup
        {
            public List<IMyInteriorLight> lights = new List<IMyInteriorLight>();

            public Color groupColor = new Color(0, 0, 0);

            Dictionary<string, MyLogger> loggers = new Dictionary<string,MyLogger>();

            public LightGroup() { }

            public LightGroup( MyLogger[] logs ) {
                foreach(MyLogger l in logs)
                {
                    loggers.Add(l.name, l);
                }
            }

            public void AddLight(IMyInteriorLight IL) {
                lights.Add(IL);
                IL.SetValue("Offset", 0.5f);
                IL.Color = groupColor; 
            }

            public void SetColor(float red, float green, float blue)
            {
                groupColor = new Color(red, green, blue);
                foreach(IMyInteriorLight i in lights)
                {
                    i.Color = groupColor;
                }
            }

            public void SetBlinkInterval(float interval = 1) {
                foreach(IMyInteriorLight i in lights)
                {
                    i.BlinkIntervalSeconds = interval;
                }
            }

            public void SetIntensity(float intensity = 0.5f) {
                foreach (IMyInteriorLight i in lights)
                {
                    i.Intensity = intensity;
                }
            }   
        }
    }
}
