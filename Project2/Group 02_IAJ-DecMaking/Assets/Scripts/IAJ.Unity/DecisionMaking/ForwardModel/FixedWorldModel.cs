using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEditor;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    //Implementation of a WorldModel Class using a recursive dictionary
    public class FixedWorldModel : WorldModel
    {

        private Properties Properties { get; set; }
        //private bool CurrentWorld { get; set; }
        //private List<Action> Actions { get; set; }
        //protected IEnumerator<Action> ActionEnumerator { get; set; } 
        protected FixedWorldModel Parent { get; set; }
        //protected GameManager GameManager { get; set; }
        //protected AutonomousCharacter Character { get; set; }
        //protected int NextPlayer { get; set; }
        //protected Action NextEnemyAction { get; set; }
        //protected Action[] NextEnemyActions { get; set; }

        //This constructor is called to create the first world model,
        //that corresponds to the character's perceptions of the current "real world"
        public FixedWorldModel(GameManager gameManager, AutonomousCharacter character,  List<Action> actions, List<Goal> goals)
        {
            this.Properties = new Properties(character);
            this.GoalValues = new Dictionary<string, float>();
            this.Actions = new List<Action>(actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.GameManager = gameManager;
            this.Character = character;
            this.NextPlayer = 0;

            foreach (var goal in goals)
            {
                this.GoalValues.Add(goal.Name, goal.InsistenceValue);
            }
        }

        public FixedWorldModel(FixedWorldModel parent)
        {
            this.Properties = new Properties(parent.Properties);
            this.GoalValues = new Dictionary<string, float>(parent.GoalValues);
            this.Actions = new List<Action>(parent.Actions);
            this.Actions.Shuffle();
            this.Parent = parent;
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.GameManager = parent.GameManager;
            this.Character = parent.Character;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new FixedWorldModel(this);
        }

        public override object GetProperty(string propertyName)
        {
            object result;
            result = this.Properties.GetProperty(propertyName);
            if(result == null)
            {
                result = this.GameManager.disposableObjects.ContainsKey(propertyName);
            }
            return result;
        }

        public override void SetProperty(string propertyName, object value)
        {
            this.Properties.SetProperty(propertyName, value);
        }

        public override float GetGoalValue(string goalName)
        {
            return GoalValues[goalName];
        }

        public override void SetGoalValue(string goalName, float value)
        {
            if (this.Parent != null) 
                this.GoalValues[goalName] = value;
        }

        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(PropertiesName.HP);
            float time = (float)this.GetProperty(PropertiesName.TIME);
            int money = (int)this.GetProperty(PropertiesName.MONEY);

            return HP <= 0 || time >= GameManager.GameConstants.TIME_LIMIT || (this.NextPlayer == 0 && money == 25);
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(PropertiesName.MONEY);
            int HP = (int)this.GetProperty(PropertiesName.HP);
            float time = (float)this.GetProperty(PropertiesName.TIME);

            if (HP <= 0 || time >= GameManager.GameConstants.TIME_LIMIT) //lose
                return 0.0f;
            else if (this.NextPlayer == 0 && money == 25 && HP > 0) //win
                return 1.0f;
            else
            { // non-terminal state
                return 0.0f;
                //return timeAndMoneyScore(time, money) * levelScore() * hpScore(HP) * timeScore(time);
            }
        }

        private float timeAndMoneyScore(float time, int money)
        {
            float relationTimeMoney = time - 6 * money;

            if (relationTimeMoney > 30)
                return 0;
            else if (relationTimeMoney < 0)
                return 0.6f;
            else
                return 0.3f;
        }

        private float timeScore(float time)
        {
            return (1 - time / GameManager.GameConstants.TIME_LIMIT);
        }

        private float levelScore()
        {
            int level = (int)this.GetProperty(PropertiesName.LEVEL);
            if (level == 2)
                return 1f;
            else if (level == 1)
                return 0.4f;
            else
                return 0;
        }

        private float hpScore(int hp)
        {
            if (hp > 18) //survives orc and dragon
                return 1f;
            if (hp > 12) //survives dragon or two orcs
                return 0.6f;
            else if (hp > 6) //survives orc
                return 0.1f;
            else
                return 0.01f;

        }
    }
}