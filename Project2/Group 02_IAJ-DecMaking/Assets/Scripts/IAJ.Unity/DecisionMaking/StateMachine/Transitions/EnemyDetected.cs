using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class EnemyDetected : Transition
    {
        private AutonomousCharacter enemy;
        public Monster agent;

        public EnemyDetected(Monster agent)
        {
            this.agent = agent;
            this.enemy = GameManager.Instance.Character;
            if (agent.tag == "Orc")
            {
                Actions = new List<IAction>() { new Shout(agent) };
            }
            else
            {
                Actions = new List<IAction>();
            }
            TargetState = new Pursuit(agent, enemy);
            
        }

        public override bool IsTriggered()
        {
            return (Vector3.Distance(agent.transform.position, enemy.transform.position) <= agent.stats.AwakeDistance);
        }
    }
}