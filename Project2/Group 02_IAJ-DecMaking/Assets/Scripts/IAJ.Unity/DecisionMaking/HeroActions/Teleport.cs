using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class Teleport : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public Teleport(AutonomousCharacter character) : base("Teleport")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 5;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var level = (int)worldModel.GetProperty(PropertiesName.LEVEL);
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);

            return level >= 2 && mana >= 5;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.Teleport();
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += -1.5f;
            }
            else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change += -0.2f;
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, mana - 5);

            worldModel.SetProperty(PropertiesName.POSITION, GameManager.Instance.initialPosition);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);
            return - ((maxHP - currentHP) / maxHP) * 10 - ((10 - mana) / 10) * 5;
        }
    }
}