using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Patroling : IState
    {
        public Monster Agent { get; set; }
        public Patroling(Monster agent) { this.Agent = agent; }

        public List<IAction> GetEntryActions() 
        { 
            return new List<IAction> { new Stop(Agent) };
        }

        public List<IAction> GetActions() { return new List<IAction>() { new Patrol(Agent, Agent.patrolPoints[0], Agent.patrolPoints[1]) }; }

        public List<IAction> GetExitActions() { return new List<IAction>(); }

        public List<Transition> GetTransitions()
        {
            return new List<Transition> {
                /*new Transition.WasAttacked,*/ 
                new EnemyDetected(Agent),
                new HeardShout(Agent)
            };
        }
    } 
}
