using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedHPChange;
        private float expectedXPChange;
        private int xpChange;
        //how do you like lambda's in c#?
        private Func<int> dmgRoll;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite",character,target)
        {
            if (target.tag.Equals("Skeleton"))
            {
                this.dmgRoll = () => RandomHelper.RollD6();
                this.xpChange = 3;
                this.expectedXPChange = 2.7f;
            }
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana >= 2 && this.Target.tag.Equals("Skeleton");
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            return mana >= 2 && this.Target.tag.Equals("Skeleton") && (bool)worldModel.GetProperty(this.Target.name);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += -1.5f;
            }
            else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)//ToDo You can add here something...
            {
                change += -this.expectedXPChange;
            }

            return change;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            var xp = (int)worldModel.GetProperty(PropertiesName.XP);

            // Instantly destroy the skeleton and gain XP
            worldModel.SetProperty(this.Target.name, false);
            worldModel.SetProperty(PropertiesName.XP, xp + this.xpChange);

            // Deduct 2 mana points
            var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, mana - 2);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);
            
            return - level * 10/this.expectedXPChange * 0.5f + 0.2f;
        }
    }
}
