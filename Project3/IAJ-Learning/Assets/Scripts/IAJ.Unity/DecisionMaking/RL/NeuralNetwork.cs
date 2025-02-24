using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Learning
{
    public class NeuralNetwork
    {
        private List<Layer> layers;
        private System.Random random;

        public NeuralNetwork(int[] neuronsPerLayer, string[] activationFunctions)
        {
            random = new System.Random();
            layers = new List<Layer>();

            // Create layers
            for (int i = 0; i < neuronsPerLayer.Length - 1; i++)
            {
                layers.Add(new Layer(neuronsPerLayer[i], neuronsPerLayer[i + 1], activationFunctions[i]));
            }
        }

        // Forward Pass
        public float[] Forward(float[] inputs)
        {
            float[] outputs = inputs;
            foreach (var layer in layers)
            {
                outputs = layer.Forward(outputs);
            }
            return outputs;
        }

        // Backpropagation (Gradient Descent)
        public void Backpropagation(float[] inputs, float[] expectedOutput, float learningRate)
        {
            // Forward pass to get outputs
            float[] predictedOutput = Forward(inputs);

            // Calculate the error at the output layer
            float[] error = new float[predictedOutput.Length];
            for (int i = 0; i < predictedOutput.Length; i++)
            {
                error[i] = predictedOutput[i] - expectedOutput[i];
            }

            // Propagate the error back through the layers
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                error = layers[i].Backpropagate(error, learningRate);
            }
        }
        public void Save(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var layer in layers)
                {
                    float[,] weights = layer.GetWeights();
                    float[] biases = layer.GetBiases();

                    writer.WriteLine(weights.GetLength(0)); // number of neurons in the current layer
                    writer.WriteLine(weights.GetLength(1)); // number of neurons in the next layer

                    // Save weights
                    for (int i = 0; i < weights.GetLength(0); i++)
                    {
                        for (int j = 0; j < weights.GetLength(1); j++)
                        {
                            writer.Write(weights[i, j] + " ");
                        }
                        writer.WriteLine();
                    }

                    // Save biases
                    foreach (float bias in biases)
                    {
                        writer.Write(bias + " ");
                    }
                    writer.WriteLine();
                }
            }
        }

        // Load the model from a file
        public void Load(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                foreach (var layer in layers)
                {
                    int neuronsInCurrentLayer = int.Parse(reader.ReadLine());
                    int neuronsInNextLayer = int.Parse(reader.ReadLine());

                    float[,] weights = new float[neuronsInCurrentLayer, neuronsInNextLayer];
                    float[] biases = new float[neuronsInNextLayer];

                    // Load weights
                    for (int i = 0; i < neuronsInCurrentLayer; i++)
                    {
                        var weightLine = reader.ReadLine().Split(' ');
                        for (int j = 0; j < neuronsInNextLayer; j++)
                        {
                            weights[i, j] = float.Parse(weightLine[j]);
                        }
                    }

                    // Load biases
                    var biasLine = reader.ReadLine().Split(' ');
                    for (int i = 0; i < neuronsInNextLayer; i++)
                    {
                        biases[i] = float.Parse(biasLine[i]);
                    }

                    layer.SetWeights(weights);
                    layer.SetBiases(biases);
                }
            }
        }
    }
}
