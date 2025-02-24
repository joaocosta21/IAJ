using Assets.Scripts.IAJ.Unity.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using RL;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using System.Runtime.InteropServices;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class DeepQLearning
    {
        // Parameters
        public bool InProgress { get; set; }
        private float learningRate;
        private float gamma; // Discount factor
        private float epsilon; // Exploration factor
        public float epsilonDecay; // Epsilon decay rate
        private float minEpsilon; // Minimum epsilon
        private int batchSize;
        private int maxEpisodes;

        private List<Action> actions { get; set; }
        private List<Action> validActions { get; set; }

        // Q-network and Target Network
        private NeuralNetwork qNetwork;
        private NeuralNetwork targetNetwork;

        // Experience Replay Memory
        private List<Experience> replayMemory;
        private int memoryCapacity;

        // File path to save QTable (used for experience replay visualization)
        private string replayMemoryFilePath;

        // Constructor
        public DeepQLearning(List<Action> _actions, int[] neuronsPerLayer, string[] activationFunctions, int memoryCapacity = 10000, int batchSize = 10, float gamma = 0.99f, float epsilon = 0.0f, float epsilonDecay = 0.95f, float minEpsilon = 0.0f, float learningRate = 0.01f, int maxEpisodes = 1000)
        {
            this.memoryCapacity = memoryCapacity;
            this.batchSize = batchSize;
            this.gamma = gamma;
            this.epsilon = epsilon;
            this.epsilonDecay = epsilonDecay;
            this.minEpsilon = minEpsilon;
            this.learningRate = learningRate;
            this.maxEpisodes = maxEpisodes;

            this.actions = _actions;

            // Initialize Q-network and Target network
            qNetwork = new NeuralNetwork(neuronsPerLayer, activationFunctions);
            targetNetwork = new NeuralNetwork(neuronsPerLayer, activationFunctions);

            // Initialize replay memory
            replayMemory = new List<Experience>();
            this.InProgress = true;
        }

        public int GetActionIndex(Action action)
        {
            for (int i = 0; i < actions.Count(); i++)
            {
                if (actions[i].Equals(action))
                {
                    return i; // Return the index if the action is found
                }
            }
            return UnityEngine.Random.Range(0, actions.Count);
        }

        private List<Action> GetValidActions()
        {

            return actions.Where(action => action.CanExecute()).OrderBy(action => UnityEngine.Random.value)
                  .ToList();
        }

        // Choose an action based on the epsilon-greedy policy
        public Action ChooseAction(float[] state, int episode)
        {
            this.InProgress = true;
            this.validActions = GetValidActions();
            float dynamicEpsilon = Mathf.Max(minEpsilon, epsilon * Mathf.Pow(epsilonDecay, episode));

            if (UnityEngine.Random.value < dynamicEpsilon)
            {
                this.InProgress = false;
                return validActions[UnityEngine.Random.Range(0, validActions.Count)]; ; // Random action
            }
            else
            {
                this.InProgress = false;
                return GetBestAction(state);
            }
        }

        private Action GetBestAction(float[] state)
        {
            float[] qValues = qNetwork.Forward(state);
            int bestActionIndex = -1;
            float maxQValue = float.MinValue;
            this.validActions = GetValidActions();

            foreach (var action in validActions)
            {
                int actionIndex = GetActionIndex(action);
                if (qValues[actionIndex] > maxQValue)
                {
                    maxQValue = qValues[actionIndex];
                    bestActionIndex = actionIndex;
                }
            }

            return bestActionIndex != -1 ? actions[bestActionIndex] : null;
        }

        // Store experience in replay memory
        public void StoreExperience(float[] state, int action, float reward, float[] nextState, bool done)
        {
            if (replayMemory.Count >= memoryCapacity)
            {
                replayMemory.RemoveAt(0); // Remove oldest experience
            }
            replayMemory.Add(new Experience(state, action, reward, nextState, done));
        }

        // Sample random experiences from memory and train the Q-network
        public void Train()
        {
            Debug.Log("Training");
            if (replayMemory.Count < batchSize) return;

            var batch = replayMemory.OrderBy(x => UnityEngine.Random.value).Take(batchSize).ToList();

            foreach (var experience in batch)
            {
                float[] targetQValues = qNetwork.Forward(experience.State);
                float target;

                if (experience.Done)
                {
                    target = experience.Reward;
                }
                else
                {
                    float[] nextQValues = targetNetwork.Forward(experience.NextState);
                    target = experience.Reward + gamma * nextQValues.Max();
                }
                targetQValues[experience.Action] = target;
                qNetwork.Backpropagation(experience.State, targetQValues, learningRate);
            }

            // Decay epsilon after training
            if (epsilon > minEpsilon)
            {
                epsilon *= epsilonDecay;
            }
        }

        // Update the target network weights
        public void UpdateTargetNetwork()
        {
            targetNetwork = qNetwork; // Copy weights to target network
        }

        public void saveNN()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filePath = Application.persistentDataPath + $"/NeuralN_{date}.txt";
            qNetwork.Save(filePath);
        }

        public void loadNN(string filePath)
        {
            qNetwork.Load(filePath);
        }

        public float[] ConvertStateToNNInput(DeepQLState state)
        {
            return new float[]
            {
                state.HPState,
                state.ManaState,
                state.LevelState,
                state.XPState,
                state.GoldState
            };
        }

        public void SaveReplayMemory()
        {
            StringBuilder csvBuilder = new StringBuilder();
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            replayMemoryFilePath = Application.persistentDataPath + $"/replayMemory_{date}.csv";
            Debug.Log("Saving Replay Memory to " + replayMemoryFilePath);
            csvBuilder.AppendLine("State,Action,Reward,NextState,Done");

            foreach (var experience in replayMemory)
            {
                if (experience != null)
                {
                    if(experience.State != null && experience.NextState != null)
                        csvBuilder.AppendLine($"{experience.State.ToString()},{experience.Action},{experience.Reward},{experience.NextState.ToString()},{experience.Done}");
                    else
                    {
                        csvBuilder.AppendLine($"{"null"},{experience.Action},{experience.Reward},{"null"},{experience.Done}");
                    }
                
                }
            }

            File.WriteAllText(replayMemoryFilePath, csvBuilder.ToString());
            Debug.Log("Replay Memory saved successfully.");
        }

        public void SaveNN()
        {

        }

        private class Experience
        {
            public float[] State;
            public int Action;
            public float Reward;
            public float[] NextState;
            public bool Done;

            public Experience(float[] state, int action, float reward, float[] nextState, bool done)
            {
                State = state;
                Action = action;
                Reward = reward;
                NextState = nextState;
                Done = done;
            }
        }
    }
}
