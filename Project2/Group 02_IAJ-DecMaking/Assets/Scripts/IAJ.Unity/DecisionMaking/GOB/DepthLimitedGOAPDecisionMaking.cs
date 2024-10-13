using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 3;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public WorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] LevelAction { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(WorldModel currentStateWorldModel, AutonomousCharacter character)
        {
            this.ActionCombinationsProcessedPerFrame = 2000;
            this.Goals = character.Goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.LevelAction = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;
            var actionsFrame = 0;

            while (this.CurrentDepth >= 0)
            {
                
                if(actionsFrame>= this.ActionCombinationsProcessedPerFrame)
                {
                    this.InProgress = true;
                    return null;
                }
                TotalActionCombinationsProcessed += 1;
                actionsFrame += 1;
                
                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    var current_val = Models[CurrentDepth].Character.CalculateDiscontentment(Models[CurrentDepth]);
                    if (current_val <= BestDiscontentmentValue)
                    {
                        BestDiscontentmentValue = current_val;
                        BestAction = LevelAction[0];
                        this.BestActionSequence = (Action[]) LevelAction.Clone();
                    }
                    CurrentDepth -= 1;
                    continue;
                }
                var nextAction = Models[CurrentDepth].GetNextAction();
                //Debug.Log((nextAction, CurrentDepth, LevelAction[0], LevelAction[1]));
                while(nextAction != null && !nextAction.CanExecute(Models[CurrentDepth])) { nextAction = Models[CurrentDepth].GetNextAction(); }
                if (nextAction != null)
                {
                    Models[CurrentDepth + 1] = Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(Models[CurrentDepth + 1]);
                    Models[CurrentDepth + 1].Character.UpdateGoalsInsistence(Models[CurrentDepth + 1]);
                    LevelAction[CurrentDepth] = nextAction;
                    CurrentDepth += 1;
                }
                else
                {
                    CurrentDepth -= 1;
                }
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
