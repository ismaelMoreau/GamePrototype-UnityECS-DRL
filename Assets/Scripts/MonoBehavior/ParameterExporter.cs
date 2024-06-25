using System.IO;
using UnityEngine;
using Unity.Collections;
using System.Text;

public static class ParameterExporter
{
    public static void ExportParameters(NeuralNetworkParametersComponent parameters, NeuralNetworkComponent neuralNetworkComponent, string filePath)
    {
        var inputWeights = ConvertTo2DArray(parameters.inputWeights, neuralNetworkComponent.inputSize, neuralNetworkComponent.hiddenSize);
        var hiddenWeights = ConvertTo2DArray(parameters.hiddenWeights, neuralNetworkComponent.hiddenSize, neuralNetworkComponent.outputSize);

        // Debugging: Print inputWeights and hiddenWeights
        // Debug.Log("Input Weights:");
        // Print2DArray(inputWeights);
        // Debug.Log("Hidden Weights:");
        // Print2DArray(hiddenWeights);

        var data = new NeuralNetworkData
        {
            layers = new LayerData[]
            {
                new LayerData
                {
                    weights = inputWeights,
                    biases = parameters.hiddenBiases.ToArray()
                },
                new LayerData
                {
                    weights = hiddenWeights,
                    biases = parameters.outputBiases.ToArray()
                }
            }
        };

        var json = SerializeToJson(data);
        File.WriteAllText(filePath, json);
    }

    public static void ExportParameters(TargetNeuralNetworkParametersComponent parameters, NeuralNetworkComponent neuralNetworkComponent, string filePath)
    {
        var inputWeights = ConvertTo2DArray(parameters.inputWeights, neuralNetworkComponent.inputSize, neuralNetworkComponent.hiddenSize);
        var hiddenWeights = ConvertTo2DArray(parameters.hiddenWeights, neuralNetworkComponent.hiddenSize, neuralNetworkComponent.outputSize);

        // Debugging: Print inputWeights and hiddenWeights
        // Debug.Log("Input Weights:");
        // Print2DArray(inputWeights);
        // Debug.Log("Hidden Weights:");
        // Print2DArray(hiddenWeights);

        var data = new NeuralNetworkData
        {
            layers = new LayerData[]
            {
                new LayerData
                {
                    weights = inputWeights,
                    biases = parameters.hiddenBiases.ToArray()
                },
                new LayerData
                {
                    weights = hiddenWeights,
                    biases = parameters.outputBiases.ToArray()
                }
            }
        };

        var json = SerializeToJson(data);
        File.WriteAllText(filePath, json);
    }

    private static float[][] ConvertTo2DArray(NativeArray<float> flatArray, int rows, int cols)
    {
        float[][] array2D = new float[rows][];
        for (int i = 0; i < rows; i++)
        {
            array2D[i] = new float[cols];
            for (int j = 0; j < cols; j++)
            {
                array2D[i][j] = flatArray[i * cols + j];
            }
        }
        return array2D;
    }

    private static void Print2DArray(float[][] array2D)
    {
        for (int i = 0; i < array2D.Length; i++)
        {
            string row = "Row " + i + ": ";
            for (int j = 0; j < array2D[i].Length; j++)
            {
                row += array2D[i][j] + " ";
            }
            Debug.Log(row);
        }
    }

    private static string SerializeToJson(NeuralNetworkData data)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{\n  \"layers\": [\n");

        for (int i = 0; i < data.layers.Length; i++)
        {
            LayerData layer = data.layers[i];
            json.Append("    {\n");
            json.Append("      \"weights\": [\n");

            for (int j = 0; j < layer.weights.Length; j++)
            {
                json.Append("        [");
                for (int k = 0; k < layer.weights[j].Length; k++)
                {
                    json.Append(layer.weights[j][k]);
                    if (k < layer.weights[j].Length - 1) json.Append(", ");
                }
                json.Append("]");
                if (j < layer.weights.Length - 1) json.Append(",\n");
            }
            json.Append("\n      ],\n");

            json.Append("      \"biases\": [");
            for (int j = 0; j < layer.biases.Length; j++)
            {
                json.Append(layer.biases[j]);
                if (j < layer.biases.Length - 1) json.Append(", ");
            }
            json.Append("]\n");

            json.Append("    }");
            if (i < data.layers.Length - 1) json.Append(",\n");
        }

        json.Append("\n  ]\n}");
        return json.ToString();
    }

    [System.Serializable]
    private class NeuralNetworkData
    {
        public LayerData[] layers;
    }

    [System.Serializable]
    private class LayerData
    {
        public float[][] weights;
        public float[] biases;
    }
}
