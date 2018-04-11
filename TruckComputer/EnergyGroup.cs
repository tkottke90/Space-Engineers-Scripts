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
        public class EnergyGroup
        {
            const float KILOWATT = 1000.0f;
            const float MEGAWATT = 1000000.0f;

            Program _program;
            public List<IMySolarPanel> solars = new List<IMySolarPanel>();
            public List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            public List<IMyReactor> reactors = new List<IMyReactor>();

            public float CurrentStoredPower = 0.0f;
            public float MaxStoredPower = 0.0f;
            public string batteryStatus = "off";

            public float CurrentOutput = 0.0f;
            public float MaxOutput = 0.0f;

            public EnergyGroup(Program p)
            {
                _program = p;
            }

            public void clear() {
                this.solars = new List<IMySolarPanel>();
                this.batteries = new List<IMyBatteryBlock>();
                this.reactors = new List<IMyReactor>();

            }

            public bool addReactor(IMyReactor _Reactor)
            {
                if (!reactors.Contains(_Reactor))
                {
                    reactors.Add(_Reactor);
                    MaxOutput += (_Reactor.MaxOutput * MEGAWATT);
                    CurrentOutput += (_Reactor.CurrentOutput * MEGAWATT);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool addBattery(IMyBatteryBlock _Battery)
            {
                if (!batteries.Contains(_Battery))
                {
                    batteries.Add(_Battery);
                    MaxOutput += (_Battery.MaxOutput * MEGAWATT);
                    CurrentOutput += (_Battery.CurrentOutput * MEGAWATT);
                    MaxStoredPower += (_Battery.MaxStoredPower * MEGAWATT);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool addSolar(IMySolarPanel _SPanel)
            {
                if (!solars.Contains(_SPanel))
                {
                    solars.Add(_SPanel);
                    MaxOutput += (_SPanel.MaxOutput * MEGAWATT);
                    CurrentOutput += (_SPanel.CurrentOutput * MEGAWATT);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool updateBatteries()
            {
                string bStatus = "";
                float output = CurrentOutput / MaxOutput * 100.0f;
                CurrentStoredPower = 0.0f;
                if (reactors.Count > 0 || solars.Count > 0)
                {
                    if (output > 75.0f) { bStatus = "discharge"; }
                    else if (CurrentStoredPower == MaxStoredPower) { bStatus = "off"; }
                    else { bStatus = "recharge"; }
                }
                else
                {
                    bStatus = "Semi";
                }
                batteryStatus = bStatus;

                try
                {
                    foreach (IMyBatteryBlock b in batteries)
                    {
                        switch (bStatus)
                        {
                            case "off":
                                b.ApplyAction("OnOff_Off");
                                break;
                            case "discharge":
                                b.ApplyAction("OnOff_On");
                                b.OnlyDischarge = true;
                                b.OnlyRecharge = false;
                                b.SemiautoEnabled = false;
                                break;
                            case "recharge":
                                b.ApplyAction("OnOff_On");
                                b.OnlyDischarge = false;
                                b.OnlyRecharge = true;
                                b.SemiautoEnabled = false;
                                break;
                            case "Semi":
                                b.ApplyAction("OnOff_On");
                                b.OnlyDischarge = false;
                                b.OnlyRecharge = false;
                                b.SemiautoEnabled = true;
                                break;
                        }

                        CurrentStoredPower += b.CurrentStoredPower;

                    }


                    return true;
                }
                catch (Exception e)
                {
                    _program.debugSB.AppendLine("Error in Energy Group: UpdateBatteries(): " + e);
                    return false;
                }

            }

            public bool updateOutput()
            {
                try
                {
                    CurrentOutput = 0.0f;

                    foreach (IMySolarPanel s in solars)
                    {
                        CurrentOutput += s.CurrentOutput * MEGAWATT;
                    }

                    foreach (IMyReactor r in reactors)
                    {
                        CurrentOutput += r.CurrentOutput * MEGAWATT;
                    }

                    foreach (IMyBatteryBlock b in batteries)
                    {
                        CurrentOutput += b.CurrentOutput * MEGAWATT;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    _program.debugSB.AppendLine("Error in Energy Group: UpdateOutput(): " + e);
                    return false;
                }
            }

            public string mathConvertWatt(float watts)
            {
                if (watts < KILOWATT)
                {
                    return watts.ToString() + " W";
                }
                else if (watts > KILOWATT && watts < MEGAWATT)
                {
                    return (watts / KILOWATT) + " KW";
                }
                return (watts / MEGAWATT) + " MW";

            }

            public void terminalDisplay() {
                for(int i = 0; i < batteries.Count; i++) {
                    string name = "*Power: Battery " + i + " ( " + Math.Round((batteries[i].CurrentStoredPower/batteries[i].MaxStoredPower) * 100,2) + "% " + (batteries[i].IsCharging ? "+" : "-")  + " )";
                    batteries[i].CustomName = name;
                }

                for(int i = 0; i < solars.Count; i++)
                {
                    
                    string name = "*Power: Solar Panel " + i + " ( " + mathConvertWatt((float)Math.Round(solars[i].CurrentOutput * MEGAWATT,3)) + " )";
                    solars[i].CustomName = name;
                }
            }

            public void terminalHidden() {
                for (int i = 0; i < batteries.Count; i++) {
                    string name = "Battery " + i;
                    batteries[i].CustomName = name;
                }

                for (int i = 0; i < solars.Count; i++) {
                    string name = "Solar Panel " + i;
                    solars[i].CustomName = name;
                }
            }
        }
    }
}
