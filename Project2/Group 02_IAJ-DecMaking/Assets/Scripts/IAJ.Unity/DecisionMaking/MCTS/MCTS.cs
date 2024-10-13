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
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        protected int MaxIterations { get; set; }
        protected int MaxIterationsPerFrame { get; set; }
        protected int NumberPlayouts { get; set; }
        protected int PlayoutDepthLimit { get; set; }
        public MCTSNode BestFirstChild { get; set; }

        public List<MCTSNode> BestSequence { get; set; }
        public WorldModel BestActionSequenceEndState { get; set; }
        public int CurrentIterations { get; protected set; }
        protected int CurrentDepth { get; set; }
        protected int FrameCurrentIterations { get; set; }
        protected WorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        
        //Information and Debug Properties
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; set; }
        
        public List<Action> BestActionSequence { get; set; }
        private int actionSequenceIndex;
        //Debug
         

        public MCTS(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
        {
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = maxIter;
            this.MaxIterationsPerFrame = maxIterFrame;
            this.NumberPlayouts = playouts;
            this.PlayoutDepthLimit = playoutDepthLimit;
            this.InProgress = false;
            this.RandomGenerator = new System.Random();
            this.actionSequenceIndex = 0; // Initialize the action sequence index
        }

        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0.0f;
 
            // create root node n0 for state s0
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

            while (this.CurrentIterations < this.MaxIterations && this.FrameCurrentIterations < this.MaxIterationsPerFrame)
            {
                selectedNode = Selection(this.InitialNode);
                
                reward = Playout(selectedNode.State);
                
                Backpropagate(selectedNode, reward);

                this.CurrentIterations++;
                this.FrameCurrentIterations++;
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
            int depth = 0;
            while (!node.State.IsTerminal())
            {
                depth++;
                if (node.ChildNodes.Count < node.State.GetExecutableActions().Length)
                {
                    if (depth > this.MaxSelectionDepthReached)
                    {
                        this.MaxSelectionDepthReached = depth;
                    }
                    return Expand(node);
                }
                else
                {
                    node = BestUCTChild(node);
                }
            }

            if (depth > this.MaxSelectionDepthReached)
            {
                this.MaxSelectionDepthReached = depth;
            }
            return node;
        }


        protected virtual float Playout(WorldModel state)
        {
            var currentState = state.GenerateChildWorldModel();

            while (!currentState.IsTerminal())
            {
                var actions = currentState.GetExecutableActions();
                if (actions.Length == 0) break;

                // Choose a random action
                var action = actions[this.RandomGenerator.Next(actions.Length)];
                action.ApplyActionEffects(currentState);
            }

            return currentState.GetScore(); // Return the reward (score) for the final state
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            while (node != null)
            {
                node.N++; // Increment the visit count
                node.Q += reward; // Update the Q-value (total reward)
                node = node.Parent; // Move up to the parent node
            }
        }


        protected MCTSNode Expand(MCTSNode node)
        {
            var actions = node.State.GetExecutableActions();
            var triedActions = node.ChildNodes.Select(c => c.Action).ToList();

            var untriedActions = actions.Except(triedActions).ToArray();
            if (untriedActions.Length == 0) return null; // Safety check

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

            // Perform multiple playouts for the new node to handle stochasticity
            float averageReward = MultiplePlayouts(newState, this.NumberPlayouts); 
            newNode.Q = averageReward; // Initialize the Q value with the average reward from the playouts
            newNode.N = 1; // Set the visit count to 1 after expansion

            return newNode;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
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
