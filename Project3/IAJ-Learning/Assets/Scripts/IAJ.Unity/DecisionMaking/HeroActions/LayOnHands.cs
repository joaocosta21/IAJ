using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class LayOnHands : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public LayOnHands(AutonomousCharacter character) : base("LayOnHands")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 7 && this.Character.baseStats.HP < this.Character.baseStats.MaxHP;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var level = (int)worldModel.GetProperty(PropertiesName.LEVEL);
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);

            return level >= 2 && mana >= 7 && currentHP < maxHP;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.LayOnHands(); // Executes LayOnHands in the GameManager
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            // Heal to full HP
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);
            worldModel.SetProperty(PropertiesName.HP, maxHP);

            // Deduct 7 Mana
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, mana - 7);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += -goal.InsistenceValue * 1.2f; 
            }
            else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change += -0.2f;
            }

            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            return - ((maxHP - currentHP) / maxHP) * 200;
        }
    }
}