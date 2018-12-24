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
    partial class Program : MyGridProgram
    {
        // =======================================================================================
        //                                                                            --- Setup Instructions ---
        // =======================================================================================


        // =======================================================================================
        //                                                                            --- Configuration ---
        // =======================================================================================

        // --- Script Name ---
        // =======================================================================================
        // Description: Name added to blocks the script has access to.  *Updating this value requires a recompile*
        // Example: Remote Control [NKRS]
        string scriptName = "NKRS";

        // --- Countdown Timer ---
        // =======================================================================================
        // Description: How long should the launch countdown be
        int countdown = 600;

        // --- Change Cargo Names ---
        // =======================================================================================
        // Description: Should script update tag of cargo blocks after it has reached orbit?
        bool changeCargoNames = false;

        // --- Updated Cargo Name ---
        // =======================================================================================
        // Description: If changeCaroNames is true, update cargo blocks to this value
        // Example: string updatedCargoName = "Satelite" = [NKRS CARGO] -> [Satelite]
        string updatedCargoName = "";

        // --- Broadcast Status ---
        // =======================================================================================
        // Description: Should script broadcast status for other antenna to listen to?
        bool broadcastStatus = false;

        // --- Transition Gravity ---
        // =======================================================================================
        // Description: Gravity at which to transition to rockets of cargo
        // Example: double transitionGrav = 0.5
        double transitionGrav = 0.5;

        // --- <Configurable Variable> ---
        // =======================================================================================
        // Description: 
        // Example: 

        // DO NOT EDIT BELOW THIS LINE!!
        // =======================================================================================
        //                                                                            --- Script ---
        // =======================================================================================

        // System
        int runcount = 0;
        int updateCount;
        int broadcastTimer = 0;
        string key = "";

        // Block Selection
        System.Text.RegularExpressions.Regex tag_match;

        // Block Groups
        LCDGroup debug;
        LCDGroup display;
        TankGroup rocketTanks;
        TankGroup cargoTanks;
        TankGroup boosterTanks;
        ThrusterGroup rocketThrusters;
        ThrusterGroup cargoThrusters;
        ThrusterGroup boosterThursters;

        IMyRemoteControl remote;
        IMyGyro control;
        IMyRadioAntenna radio;
        IMyLaserAntenna laser;

        IMyShipMergeBlock rocketRelease;
        IMyShipMergeBlock cargoRelease;

        List<IMyParachute> chutes = new List<IMyParachute>();
        List<IMyShipMergeBlock> boosters = new List<IMyShipMergeBlock>();


        // Sensor Data:
        double gravity = 0.0d;

        // Log Lists
        List<string> debugLog = new List<string>();
        List<string> EventLog = new List<string>();
        MyData log = null;

        // Config
        // MyData userConfig = null;

        // State Machine
        State _currentState;
        Dictionary<ProgramStates, State> states = new Dictionary<ProgramStates, State>();
        Dictionary<ProgramTransitions, Transition> transitions = new Dictionary<ProgramTransitions, Transition>();

        public Program()
        {
            Storage = "";

            // Init tag
            tag_match = new System.Text.RegularExpressions.Regex(generateTag(scriptName));
            

            if (!tag_match.IsMatch(Me.CustomName))
            {
                // Generate Instance Key:
                GenerateInstanceKey();

                // Name Block Running Script
                Me.CustomName = $"PB [NKRS {key}]";
            } else
            {
                string[] BlockTags = tag_match.Match(Me.CustomName).Value.Replace("[", "").Replace("]", "").ToUpper().Split(' ');
                key = BlockTags[1];
            }
            Echo($"Key: {key}");

            // Get Stored Data
            if (log == null) {
                getLogInformation($"Log-{key}", out log);
            }


            // --------------

            // Init Block Groups
            debug = new LCDGroup("debug", this);
            display = new LCDGroup("display", this);
            rocketTanks = new TankGroup("Rocket");
            cargoTanks = new TankGroup("Cargo");
            boosterTanks = new TankGroup("Booster");
            rocketThrusters = new ThrusterGroup("RocketThrust");
            cargoThrusters = new ThrusterGroup("CargoThrust");
            boosterThursters = new ThrusterGroup("BoosterThrust");

            // Init State Machine:
            createStates();
        }


        public void getLogInformation(string dataName, out MyData foundData) {
            if (!MyData.FindDataInstance(Me.CustomData, dataName, out foundData))
            {
                foundData = new MyData($"Log-{key}");
                foundData.AddData($"{CurrentLogTime()} - Boot", "New Data Object Created");
                Echo("New Log Created");
                Me.CustomData += log.ToString();
            }
            else
            {
                Echo("Stored Log Found");
                Echo($"Log: {log.name}");
                foreach(var keys in log.data.Keys)
                {
                    Echo($"  - {keys}");
                }

                foundData.AddData($"{CurrentLogTime()}-Boot", "Script Recompiled");
                string output = "";
                if (MyData.UpdateDataInstance(Me.CustomData, log, out output))
                {
                    Me.CustomData = output;
                }
            }
        }


        public void Save() {
            string output;
            if (MyData.UpdateDataInstance(Me.CustomData, log, out output))
            {
                Me.CustomData = output;
            } else {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Echo("Error Updating Storage");
            }
        }

        public void Main(string argument, UpdateType updateSource) {
            OnUpdate(argument, updateSource);

            if (!_currentState.TaskComplete)
            {
                _currentState.TaskComplete = _currentState.Run("");
            }
            else
            {
                _currentState.Next();
            }

            // Echo(log.ToString());

            Me.CustomData = Storage;

            // Draw displays
            Echo(DrawApp());
            // debug.writeToLCD(DrawDebug());
            sendStatus();

            // Update Runtime variables
            runcount++;
            updateCount++;
        }

        public void OnUpdate(string arg, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Terminal:
                    debugLog.Add(String.Format("{0} - Terminal Command: {1}", (runcount / 60), arg.ToUpper()));
                    _currentState.TaskComplete = _currentState.Run(arg);
                    break;
                case UpdateType.Antenna:
                    string[] radioCommands = arg.Split(' ');
                    if (radioCommands[0] == key)
                    {
                        _currentState.TaskComplete = _currentState.Run(radioCommands[1].ToUpper());
                        debugLog.Add(String.Format("Command: {0}", radioCommands[1].ToUpper()));
                    }                    
                    break;
                case UpdateType.Update1:
                    
                    break;
            }
        }

        private void GenerateInstanceKey()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] charsArr = chars.ToCharArray();
            DateTime start = DateTime.Now;
            Random r = new Random();
            string charKey = "";
            for (int i = 0; i < 5; i++)
            {
                int next = r.Next(0, (chars.Length - 1));
                charKey += charsArr[next];
            }

            key = String.Format("{0}{1}-{2}-{3}{4}", start.Year, start.Month, charKey, start.Day, start.Hour);
        }

        public string getProperites(IMyTerminalBlock block) {
            List<ITerminalProperty> props = new List<ITerminalProperty>();
            block.GetProperties(props);

            StringBuilder sb = new StringBuilder();

            foreach(ITerminalProperty t in props)
            {
                sb.AppendFormat(" - {0} ({1})", t.Id, t.TypeName);
            }

            return sb.ToString();
        }

        public long CurrentLogTime()
        {
            return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public string generateTag(string tag)
        {
            return @"(\[" + tag + @")(\s\b[a-zA-z0-9-]*\b)*(\s?\])";
        }

        // Broadcast Methods

        public void sendStatus()
        {

            if (!broadcastStatus) return;

            if (broadcastTimer != 0) { broadcastTimer--; return;  }
            broadcastTimer = 60;

            if (radio == null && laser == null) {
                broadcastStatus = false;
                debugLog.Add("No Antennas Found - Disabling Broadcast");
                return;
            }

            StringBuilder status = new StringBuilder();
            
            status.AppendFormat("Rocket {0} Status", key).AppendLine().AppendLine();
            status.AppendFormat("  - Rocket Status: {0}", Enum.GetName(typeof(ProgramStates), _currentState.state)).AppendLine();
            if (boosterTanks.group.Count > 0)
            {
                status.AppendFormat("  - Booster Fuel Level: {0}% {1}\n", Math.Round((boosterTanks.getFill() * 100), 2), Math.Round((boosterTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
            }
            status.AppendFormat("  - Rocket Fuel Level: {0}% {1}\n", Math.Round((rocketTanks.getFill() * 100), 2), Math.Round((rocketTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
            status.AppendFormat("  - Cargo Fuel Level: {0}% {1}\n\n", Math.Round((cargoTanks.getFill() * 100), 2), Math.Round((cargoTanks.getFill() * 100), 2) > 10 ? "" : "! Low Fuel !");
            Vector3D gravity = remote.GetNaturalGravity();
            double magnitude = Math.Sqrt(Math.Pow(gravity.X, 2) + Math.Pow(gravity.Y, 2) + Math.Pow(gravity.Z, 2));
            status.AppendFormat("  - Measured Gravity (G): {0}\n", Math.Round(magnitude / 9.81, 2));

            double altitude;
            remote.TryGetPlanetElevation(MyPlanetElevation.Surface,out altitude);
            status.AppendFormat("  - Altitude(m): {0}", altitude).AppendLine();

            double speed = remote.GetShipSpeed();
            status.AppendFormat("  - Rocket Speed: {0}", speed <= 1 ? "Stopped" : (Math.Round(speed, 2) + " m\\s")).AppendLine();

            if (radio != null && radio.Enabled)
            {
                status.AppendLine("  - Transmittion Type: Radio");
                if (radio.TransmitMessage(status.ToString(), MyTransmitTarget.Default)) {
                    EventLog.Add(String.Format("{0} - Status Transmitted over Radio Antenna to Owner/Ally [ Length: {1} ]", runcount, status.ToString().Length));
                } else
                {
                    EventLog.Add(String.Format("{0} - Failed to Transmit Status over Radio Antenna", runcount));
                }
            }

            if (laser != null && laser.Enabled && laser.Status == MyLaserAntennaStatus.Connected)
            {
                status.AppendLine("Transmittion Type: Laser");
                if (laser.TransmitMessage(status.ToString()))
                {
                    EventLog.Add(String.Format("{0} - Status Transmitted over Laser Antenna to {2} [ Length: {1} ]", runcount, status.ToString().Length, laser.TargetCoords.ToString()));
                }
                else
                {
                    EventLog.Add(String.Format("{0} - Failed to Transmit Status over Radio Antenna", runcount));
                }

            }
        }

        // Draw Methods
        public string DrawApp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- NoobKeper Rocket Control --").AppendLine();

            sb.AppendFormat("Instance Key: {0}\n", key);

            int runtime = runcount / 60;
            int runRemainder = runcount % 60;
            sb.AppendFormat("Uptime: {0}:{1}", runtime, runRemainder).AppendLine();

            // int updateTime = 10 - (updateCount / 60);
            // int updateRemainder = updateCount % 60;
            // sb.AppendFormat("Next Update: {0}:{1} Sec", updateTime, updateRemainder).AppendLine().AppendLine();

            sb.AppendLine("-- App Data -- ");
            sb.AppendFormat("  - State: {0}", Enum.GetName(typeof(ProgramStates), _currentState.state)).AppendLine();
            sb.AppendFormat("  - State Complete: {0}", _currentState.TaskComplete).AppendLine();

            sb.AppendFormat("  - Logs({1}): {0}", log.data.Count, log.name).AppendLine();

            return sb.ToString();
        }

        public string DrawError() {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-- Breaking Error -- ");

            sb.AppendLine("Error");

            return sb.ToString();
        }

        public string DrawDisplay(string display = "")
        {
            if (display == "") {
                return display;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-- N00bKeper Rocket Script --");

            if (remote != null)
            {
                sb.AppendLine("  -- Remote Control --");
                Vector3D gravity = remote.GetNaturalGravity();

                sb.AppendFormat("Planet Gravity:\n    X: {0}\n    Y:{1}\n    Z:{2}\n", Math.Round(gravity.X, 2), Math.Round(gravity.Y, 2), Math.Round(gravity.Z, 2));

                double magnitude = Math.Sqrt(Math.Pow(gravity.X, 2) + Math.Pow(gravity.Y, 2) + Math.Pow(gravity.Z, 2));

                sb.AppendFormat("Magnitude: {0}\n", Math.Round(magnitude, 2));

                sb.AppendFormat("Gravity (G): {0}\n", Math.Round(magnitude/9.81,2));

            }

            return sb.ToString();
        }

        public string DrawDebug()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-- N00bKeper Debug -- ").AppendLine();

            foreach (string s in debugLog) {
                sb.AppendLine(s);
            }

            return sb.ToString();
        }

        public string DrawConfig()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// -- N00bKeper Rocket Script Config --");
            sb.AppendLine("//  - To update a value, place an \'[X]\' in the selection box.").AppendLine("//  - Toggleable settings can be changed by adding check \'[X]\'").AppendLine("//  - Setting with value require value after \':\'").AppendLine();

            sb.AppendLine("// Script Name:").AppendLine("// Description: Name added to blocks the script has access to.").AppendLine("// Example: Remote Control [NKRS]").AppendLine("[ ] Script-Name: " + scriptName).AppendLine();
            sb.AppendLine("// Countdown Timer:").AppendLine("// Description: Length of countdown timer before launch. Time is expressed in seconds").AppendLine("[ ] Coutdown-Time:  " + (countdown / 60)).AppendLine();
            return sb.ToString();
        }

        public string DrawStatus()
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }
    }
}