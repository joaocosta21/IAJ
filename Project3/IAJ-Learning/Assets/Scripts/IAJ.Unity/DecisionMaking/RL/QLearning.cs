using RL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using System.IO;
using System.Text;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking
{
    public class QLearning
    {
        // Parameters
        public bool InProgress { get; set; }
        private float alpha; // Learning rate
        private float gamma; // Discount factor
        public float epsilon; // Exploration factor
        private List<Action> actions { get; set; }
        private List<Action> validActions { get; set; }

        private int maxEpisodes; // Maximum number of episodes
        private Dictionary<TQLState, Dictionary<Action, float>> QTable; // Q-value table
        // File path to save the QTable
        private string QTableFilePath;

        // Updated constructor
        public QLearning(List<Action> _actions, int maxEpisodes, float alpha, float gamma = 0.9f, float epsilon = 0.0f)
        {
            this.actions = _actions;
            this.maxEpisodes = maxEpisodes; // New parameter
            this.alpha = alpha; // Learning rate
            this.gamma = gamma; // Discount factor (optional, default 0.9)
            this.epsilon = epsilon; // Exploration factor (optional, default 0.1)
            this.QTable = new Dictionary<TQLState, Dictionary<Action, float>>();
            this.InProgress = true;
        }

        // Choose an action based on the epsilon-greedy policy
        public Action ChooseAction(TQLState currentState, int episode)
        {
            this.InProgress = true;
            float dynamicEpsilon = epsilon * (1 - ((float)episode / maxEpisodes));

            // Get a list of valid actions for the current state
            validActions = GetValidActions();
            if (validActions == null || validActions.Count == 0)
            {
                Debug.LogError("No valid actions available for the current state.");
                return null;
            }
            
            if (UnityEngine.Random.value < dynamicEpsilon)
            {
                return GetRandomAction(validActions, currentState);
            }
            else
            {
                return GetBestAction(validActions, currentState);
            }
        }

        private List<Action> GetValidActions()
        {
            return actions.Where(action => action.CanExecute()).OrderBy(action => UnityEngine.Random.value)
                  .ToList();
        }

        private Action GetBestAction(List<Action> validActions, TQLState state)
        {
            // Check if the state exists in the Q-table
            if (!QTable.ContainsKey(state))
            {
                QTable[state] = new Dictionary<Action, float>();
            }

            // Find the best action based on the Q-values, considering only valid actions
            var bestAction = validActions.OrderByDescending(action => 
                QTable[state].ContainsKey(action) ? QTable[state][action] : 0.0f).FirstOrDefault();

            // Ensure only the selected action is initialized
            if (!QTable[state].ContainsKey(bestAction))
            {
                QTable[state][bestAction] = 0.0f; // Initialize only the best action
            }

            this.InProgress = false;
            return bestAction;
        }

        private Action GetRandomAction(List<Action> validActions, TQLState state)
        {
            // Select a random valid action
            var randomAction = validActions[UnityEngine.Random.Range(0, validActions.Count)];

            // Ensure the Q-table has an entry for the current state and action
            if (!QTable.ContainsKey(state))
            {
                QTable[state] = new Dictionary<Action, float>();
            }
            if (!QTable[state].ContainsKey(randomAction))
            {
                QTable[state][randomAction] = 0.0f; // Initialize with a Q-value of 0.0f
            }

            this.InProgress = false;
            return randomAction;
        }


        // Update Q-Table after an action
        public void UpdateQValue(TQLState state, Action action, float reward, TQLState nextState)
        {
            // Ensure the current state exists in the QTable
            if (!QTable.ContainsKey(state))
            {
                QTable[state] = new Dictionary<Action, float>();
            }
            // Ensure the action exists in the current state
            if (!QTable[state].ContainsKey(action))
            {
                QTable[state][action] = 0.0f; // Initialize Q-value if not present
            }

            float maxQValueNextState = 0.0f;
            // Get the max Q-value for the next state
            if (nextState == null)
            {
                maxQValueNextState = 0.0f;
            }
            else
            {
                maxQValueNextState = QTable.ContainsKey(nextState) ? QTable[nextState].Values.DefaultIfEmpty(0.0f).Max() : 0.0f;
            }
            // Update the Q-value for the current state and action using the Q-learning formula
            QTable[state][action] += alpha * (reward + gamma * maxQValueNextState - QTable[state][action]);
        }

        public void SaveQTable()
        {
            StringBuilder csvBuilder = new StringBuilder();

            // Add headers
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            QTableFilePath = Application.persistentDataPath + $"/qtable_{date}.csv";
            Debug.Log("Saving QTable to " + QTableFilePath);
            csvBuilder.AppendLine("State,Action,QValue");

            foreach (var stateEntry in QTable)
            {
                var state = stateEntry.Key;
                foreach (var actionEntry in stateEntry.Value)
                {
                    var action = actionEntry.Key;
                    var qValue = actionEntry.Value;

                    // Convert state and action to strings, assuming ToString() can represent them adequately
                    csvBuilder.AppendLine($"{state.ToString()},{action.Name},{qValue}");
                }
            }

            // Write to file
            File.WriteAllText(QTableFilePath, csvBuilder.ToString());
            Debug.Log("QTable saved successfully.");
        }

        public void LoadQTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"QTable file not found at: {filePath}");
                return;
            }

            try
            {
                // Clear existing QTable
                QTable.Clear();

                // Read all lines from the file
                var lines = File.ReadAllLines(filePath);

                // Skip the header line
                foreach (var line in lines.Skip(1))
                {
                    var values = line.Split(',');

                    if (values.Length != 3)
                    {
                        Debug.LogError("Invalid line in QTable file: " + line);
                        continue;
                    }

                    // Extract values from the line
                    string stateString = values[0];
                    string actionName = values[1];
                    float qValue = float.Parse(values[2]);

                    // Reconstruct the state (assuming TQLState has a suitable method for parsing)
                    TQLState state = TQLState.Parse(stateString);

                    // Reconstruct the action based on its name
                    Action action = actions.FirstOrDefault(a => a.Name == actionName);

                    if (action == null)
                    {
                        Debug.LogError("Action not found: " + actionName);
                        continue;
                    }

                    // Check if the state is already present in the QTable
                    if (!QTable.ContainsKey(state))
                    {
                        QTable[state] = new Dictionary<Action, float>();
                    }

                    // Set the Q-value
                    QTable[state][action] = qValue;
                }

                Debug.Log("QTable loaded successfully from " + filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading QTable: " + ex.Message);
            }
        }

    }
}
