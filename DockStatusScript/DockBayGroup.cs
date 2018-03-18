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

        

        public class DockBayGroup
        {
            IMySensorBlock sensor;
            IMyShipConnector connector;
            List<IMyTextPanel> LCDPanels;

            [Flags]
            public enum States {
                none = 0,
                sensor = 1,
                connector = 2,
            }

            bool prevState = false;
            public States state = States.none;

            public DockBayGroup(IMySensorBlock s, List<IMyTextPanel> p) {
                sensor = s;
                LCDPanels = p;
                Setup();
            }

            public DockBayGroup(IMyShipConnector c, List<IMyTextPanel> p) {
                connector = c;
                LCDPanels = p;
                Setup();
            }

            public DockBayGroup(IMySensorBlock s, IMyShipConnector c, List<IMyTextPanel> p) {
                sensor = s;
                connector = c;
                LCDPanels = p;
                Setup();
            }

            private void Setup() {
                foreach (IMyTextPanel t in LCDPanels)
                {
                    t.ClearImagesFromSelection();
                    t.SetShowOnScreen(0);
                }
            }

            public void update() {

                if ( sensor != null && sensor.IsActive)
                {
                    state |= States.sensor;
                } else
                {
                    state &= ~States.sensor;
                }

                if ( connector != null && connector.Status == MyShipConnectorStatus.Connected)
                {
                    state |= States.connector;
                } else
                {
                    state &= ~States.connector;
                }

                bool currentState = (state & States.sensor) != 0 || (state & States.connector) != 0;

                if (prevState != currentState)
                {
                    DrawLCD(currentState);
                    
                }

                prevState = currentState;

            }

            public void DrawLCD(bool occupied) {
                if (occupied)
                {
                    foreach(IMyTextPanel t in LCDPanels)
                    {
                        t.ClearImagesFromSelection();
                        t.AddImageToSelection("Cross");
                        
                    }
                } else
                {
                    foreach (IMyTextPanel t in LCDPanels)
                    {
                        t.ClearImagesFromSelection();
                        t.AddImageToSelection("Arrow");
                       
                    }
                }
            }
        }
    }
}
