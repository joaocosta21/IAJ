using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion",character,target)
        {
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana < Character.baseStats.MaxMana;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);
            return currentMana < maxMana;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.GetManaPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (Character.baseStats.Money == 25)
            {
                change += 1000;
            }
            else
            {
                if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
                {
                    change -= goal.InsistenceValue * 0.5f;
                }
                else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
                {
                    change -= goal.InsistenceValue * 0.1f;
                }
            }

 
            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, mana + 10);

            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);

            return - ((maxMana - currentMana) / maxMana) * 50;
        }
    }
}
