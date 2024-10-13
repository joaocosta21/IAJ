using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Pursuit : IState
    {

        public Monster Agent { get; set; }
        public AutonomousCharacter Target { get; set; }

        public float maximumRange { get; set; }

        public Pursuit(Monster agent, AutonomousCharacter target)
        {
            this.Agent = agent;
            this.Target = target;
        }

        public List<IAction> GetEntryActions() { return new List<IAction>(); }

        public List<IAction> GetActions()
        {
            return new List<IAction> { new MoveTo(Agent, Target.transform.position) };
            
        }

        public List<IAction> GetExitActions() { return new List<IAction>(); }

        public List<Transition> GetTransitions()
        {
            return new List<Transition>
            {
                new ToMeleeCombat(Agent,Target),
                new LostEnemy(Agent, Target),
                new LostEnemySleep(Agent,Target),
            };
        }
    } 
}
