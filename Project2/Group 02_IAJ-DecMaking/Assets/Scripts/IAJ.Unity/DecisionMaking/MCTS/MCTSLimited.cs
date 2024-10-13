using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEditor.Animations;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSLimited
    {
        public const float C = 1.4f;
        protected int MaxIterations { get; set; }
        protected int MaxIterationsPerFrame { get; set; }
        protected int NumberPlayouts { get; set; }
        protected int PlayoutDepthLimit { get; set; }
        public bool InProgress { get; private set; }

        public MCTSNode BestFirstChild { get; private set; }
        public List<MCTSNode> BestSequence { get; private set; }
        public WorldModel BestActionSequenceEndState { get; set; }
        public int CurrentIterations { get; private set; }
        protected WorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected int FrameCurrentIterations { get; set; }

        protected System.Random RandomGenerator { get; set; }

        // Debugging Info
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public List<Action> BestActionSequence { get; set; }
        private int actionSequenceIndex;
         
        public MCTSLimited(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
        {
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = maxIter;
            this.MaxIterationsPerFrame = maxIterFrame;
            this.NumberPlayouts = playouts;
            this.PlayoutDepthLimit = playoutDepthLimit;
            this.RandomGenerator = new System.Random();
            this.InProgress = false;
            this.actionSequenceIndex = 0;
        }

        public void InitializeMCTLimitedSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0f;
            this.InitialNode = new MCTSNode(this.InitialState)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
            this.actionSequenceIndex = 0;
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;
            MCTSNode selectedNode;
            InProgress = true;
            float reward;
            this.FrameCurrentIterations = 0;

            while (this.CurrentIterations < this.MaxIterations && FrameCurrentIterations < this.MaxIterationsPerFrame)
            {
                selectedNode = Selection(this.InitialNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);

                this.CurrentIterations++;
                FrameCurrentIterations++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            if (this.CurrentIterations >= this.MaxIterations)
            {
                var bestAction = BestAction(this.InitialNode);
                InProgress = false;
                return bestAction;
            }
            return null;
        }

        protected MCTSNode Selection(MCTSNode node)
        {
            int depthsel = 0;
            while (!node.State.IsTerminal())
            {
                depthsel++;
                if (node.ChildNodes.Count < node.State.GetExecutableActions().Length)
                {
                    if (depthsel > this.MaxSelectionDepthReached)
                    {
                        this.MaxSelectionDepthReached = depthsel;
                    }
                    return Expand(node);
                }
                else
                {
                    node = BestUCTChild(node);
                }
            }

            if (depthsel > this.MaxSelectionDepthReached)
            {
                this.MaxSelectionDepthReached = depthsel;
            }
            return node;
        }

        protected MCTSNode Expand(MCTSNode node)
        {
            var actions = node.State.GetExecutableActions();
            var triedActions = node.ChildNodes.Select(c => c.Action).ToList();
            var untriedActions = actions.Except(triedActions).ToArray();

            if (untriedActions.Length == 0) return null;

            var action = untriedActions[this.RandomGenerator.Next(untriedActions.Length)];
            var newState = node.State.GenerateChildWorldModel();
            action.ApplyActionEffects(newState);
            newState.CalculateNextPlayer();

            var newNode = new MCTSNode(newState)
            {
                Parent = node,
                Action = action,
                N = 0,
                Q = 0
            };
            node.ChildNodes.Add(newNode);

            // Perform multiple playouts
            float averageReward = MultiplePlayouts(newState, this.NumberPlayouts);
            newNode.Q = averageReward;
            newNode.N = 1;

            return newNode;
        }

        protected MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestUCTValue = float.MinValue;

            foreach (var child in node.ChildNodes)
            {
                float exploitation = child.Q / child.N;
                float exploration = C * Mathf.Sqrt(Mathf.Log(node.N) / child.N);
                float uctValue = exploitation + exploration;

                if (uctValue > bestUCTValue)
                {
                    bestUCTValue = uctValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        protected float Playout(WorldModel state)
        {
            var currentState = state.GenerateChildWorldModel();
            int depthplay = 0;
            
            // Initialize actionHeuristicValues to keep track of the last heuristic values computed
            List<Tuple<Action, float>> actionHeuristicValues = new List<Tuple<Action, float>>();

            while (!currentState.IsTerminal() && depthplay < this.PlayoutDepthLimit)
            {
                var actions = currentState.GetExecutableActions();
                if (actions.Length == 0) break;

                // Update actionHeuristicValues with the current set of actions and their heuristic values
                actionHeuristicValues = actions.Select(action => 
                    new Tuple<Action, float>(action, action.GetHValue(currentState))
                ).ToList();

                var selectedAction = BiasedActionSelection(actionHeuristicValues);

                selectedAction.ApplyActionEffects(currentState);
                depthplay++;
            }

            this.MaxPlayoutDepthReached = Mathf.Max(this.MaxPlayoutDepthReached, depthplay);

            return currentState.GetScore();
        }

        private Action BiasedActionSelection(List<System.Tuple<Action, float>> actionHeuristicValues)
        {
            float minHeuristic = actionHeuristicValues.Min(a => a.Item2);

            var bestActions = actionHeuristicValues
                .Where(a => Mathf.Approximately(a.Item2, minHeuristic))
                .Select(a => a.Item1)
                .ToList();

            return bestActions[this.RandomGenerator.Next(bestActions.Count)];
        }

        protected void Backpropagate(MCTSNode node, float reward)
        {
            while (node != null)
            {
                node.N++;
                node.Q += reward;
                node = node.Parent;
            }
        }

        protected MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestValue = float.MinValue;

            foreach (var child in node.ChildNodes)
            {
                float value = child.Q / child.N;
                if (value > bestValue)
                {
                    bestValue = value;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        protected Action BestAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            // Build the best sequence for execution
            this.BestSequence = new List<MCTSNode> { bestChild };
            this.BestActionSequence = new List<Action> { bestChild.Action }; // Store action sequence
            node = bestChild;

            while (!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null)
                {
                    break;
                }
                this.BestSequence.Add(bestChild);
                this.BestActionSequence.Add(bestChild.Action); // Add to sequence
                node = bestChild;
            }
            this.BestActionSequenceEndState = node.State;
            return this.BestFirstChild.Action;
        }

        protected float MultiplePlayouts(WorldModel state, int numberOfPlayouts)
        {
            float totalReward = 0.0f;
            for (int i = 0; i < numberOfPlayouts; i++)
            {
                totalReward += Playout(state.GenerateChildWorldModel());
            }
            return totalReward / numberOfPlayouts;
        }
    }
}