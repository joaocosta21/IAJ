using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class ShieldOfFaith : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana >= 5 && Character.baseStats.ShieldHP != 5;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var shieldHP = (int)worldModel.GetProperty(PropertiesName.ShieldHP);
            return mana >= 5 && shieldHP != 5;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.ShieldOfFaith(); // No target needed
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                // Shield of Faith improves survivability, lowering the insistence of the survive goal
                change -= 5.0f - Character.baseStats.ShieldHP; // The shield is 5 HP, directly contributing to survival
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            
            // Apply Shield of Faith - grant a shield of 5 HP
            worldModel.SetProperty(PropertiesName.ShieldHP, 5);

            // Deduct 5 mana points
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, mana - 5);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var shieldHP = (int)worldModel.GetProperty(PropertiesName.ShieldHP);
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);

            return -200;
        }
    }
}
