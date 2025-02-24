using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class FinishedInvestigation : Transition
    {
        public Monster agent;

        public FinishedInvestigation(Monster agent)
        {
            this.agent = agent;
            agent.HeardShout = false;
            TargetState = new Patroling(agent);
            Actions = new List<IAction>();
        }

        public override bool IsTriggered()
        {
            var x = agent.transform.position - agent.ShoutPosition;
            return (Mathf.Sqrt(x.x*x.x +x.z*x.z) <= 0.1);
        }
    }
}