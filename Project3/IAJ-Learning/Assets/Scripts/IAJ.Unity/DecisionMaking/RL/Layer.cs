using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Learning
{
    public class Layer
    {
        public int NeuronCount { get; private set; }
        public int NextLayerNeuronCount { get; private set; }
        private float[,] weights;
        private float[] biases;
        private string activationFunction;

        public Layer(int neuronCount, int nextLayerNeuronCount, string activationFunction)
        {
            NeuronCount = neuronCount;
            NextLayerNeuronCount = nextLayerNeuronCount;
            this.activationFunction = activationFunction;

            // Initialize weights and biases
            weights = new float[neuronCount, nextLayerNeuronCount];
            biases = new float[nextLayerNeuronCount];
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            for (int i = 0; i < NeuronCount; i++)
            {
                for (int j = 0; j < NextLayerNeuronCount; j++)
                {
                    weights[i, j] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
            for (int j = 0; j < NextLayerNeuronCount; j++)
            {
                biases[j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }

        public float[] Forward(float[] inputs)
        {
            float[] outputs = new float[NextLayerNeuronCount];

            for (int j = 0; j < NextLayerNeuronCount; j++)
            {
                float sum = biases[j];
                for (int i = 0; i < NeuronCount; i++)
                {
                    if (inputs != null)
                        sum += weights[i, j] * inputs[i];
                }
                outputs[j] = Activate(sum);
            }

            return outputs;
        }

        private float Activate(float value)
        {
            switch (activationFunction.ToLower())
            {
                case "relu":
                    return Mathf.Max(0, value);
                case "sigmoid":
                    return 1f / (1f + Mathf.Exp(-value));
                case "tanh":
                    return (float)Math.Tanh(value);
                default:
                    return value; // Linear activation as default
            }
        }

        public float[] Backpropagate(float[] error, float learningRate)
        {
            float[] newError = new float[NeuronCount];

            // Adjust weights and biases
            for (int i = 0; i < NeuronCount; i++)
            {
                for (int j = 0; j < NextLayerNeuronCount; j++)
                {
                    // Derivative of activation function (assuming ReLU for simplicity)
                    float derivative = (weights[i, j] > 0) ? 1 : 0;

                    newError[i] += error[j] * weights[i, j] * derivative;

                    // Gradient descent update
                    weights[i, j] -= learningRate * error[j] * derivative;
                }
            }

            return newError;
        }

        // GetWeights method to retrieve the weights
        public float[,] GetWeights()
        {
            return weights;
        }

        // GetBiases method to retrieve the biases
        public float[] GetBiases()
        {
            return biases;
        }

        // SetWeights method to set new weights
        public void SetWeights(float[,] newWeights)
        {
            if (newWeights.GetLength(0) != NeuronCount || newWeights.GetLength(1) != NextLayerNeuronCount)
                throw new ArgumentException("Dimensions of new weights do not match layer dimensions.");

            weights = newWeights;
        }

        // SetBiases method to set new biases
        public void SetBiases(float[] newBiases)
        {
            if (newBiases.Length != NextLayerNeuronCount)
                throw new ArgumentException("Length of new biases does not match the layer's next layer neuron count.");

            biases = newBiases;
        }
    }
}
