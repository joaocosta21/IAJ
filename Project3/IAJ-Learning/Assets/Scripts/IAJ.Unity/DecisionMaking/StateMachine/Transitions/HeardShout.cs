using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class HeardShout : Transition
    {
        private AutonomousCharacter enemy;
        public Monster agent;

        public HeardShout(Monster agent)
        {
            this.agent = agent;
            this.enemy = GameManager.Instance.Character;
            TargetState = new InvestigateShout(agent,enemy);
            Actions = new List<IAction>();
        }

        public override bool IsTriggered()
        {
            return (agent.HeardShout);
        }
    }
}