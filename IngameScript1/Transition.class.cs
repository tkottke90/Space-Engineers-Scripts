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

        public enum ProgramTransitions
        {
            boot,
            error,
            reboot,
            start,
            stop,
            ignition,
            release,
            done,
            config
        }


        public abstract class Transition {
            public Program _program;
            public virtual bool action() { return false; }
            public virtual bool action(string message) { return false; }
        }


        public class ErrorTransition : Transition {
            public ErrorTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Error];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Error Transition: " + e.ToString()); return false; }
            }

            public override bool action(string message)
            {
                try
                {
                    _program.debugLog.Add("Error: " + message);
                    _program._currentState = _program.states[ProgramStates.Error];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Error Transition: " + e.ToString()); return false; }
            }
        }

        public class BootTransition : Transition {
            public BootTransition(Program p) { _program = p;  }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Ready];
                    _program._currentState.Init();
                    return true;
                } catch (Exception e) { _program.debugLog.Add("Exception in Boot Transition: " + e.ToString()); return false; }
            }
        }

        public class RebootTransition : Transition
        {
            public RebootTransition(Program p) { _program = p; }
            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Startup];
                    _program._currentState.Init();
                    _program.debugLog.Clear();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Reboot Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }

        public class StartTransition : Transition {
            public StartTransition(Program p) { _program = p; }
            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Countdown];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Start Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }

        }

        public class StopTransition : Transition {
            public StopTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Ready];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Stop Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }

        }

        public class IgnitionTransition : Transition {
            public IgnitionTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Stage_One];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Ignition Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }

        public class ReleaseTransition : Transition {
            public ReleaseTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Stage_Two];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Release Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }

        public class CompleteTransition : Transition {
            public CompleteTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Idle];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Complete Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }

        public class ConfigTransition : Transition
        {
            public ConfigTransition(Program p) { _program = p; }

            public override bool action()
            {
                try
                {
                    _program._currentState = _program.states[ProgramStates.Config];
                    _program._currentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugLog.Add("Exception in Config Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }
    }
}
