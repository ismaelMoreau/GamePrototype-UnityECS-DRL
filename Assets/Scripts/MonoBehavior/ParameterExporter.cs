using System.IO;
using UnityEngine;

public static class ParameterExporter
{
    public static void ExportParameters(NeuralNetworkParametersComponent parameters, string filePath)
    {
        var data = new NeuralNetworkData
        {
            InputWeights = parameters.inputWeights.ToArray(),
            HiddenWeights = parameters.hiddenWeights.ToArray(),
            OutputBiases = parameters.outputBiases.ToArray(),
            HiddenBiases = parameters.hiddenBiases.ToArray()
        };

        var json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public static void ExportParameters(TargetNeuralNetworkParametersComponent parameters, string filePath)
    {
        var data = new NeuralNetworkData
        {
            InputWeights = parameters.inputWeights.ToArray(),
            HiddenWeights = parameters.hiddenWeights.ToArray(),
            OutputBiases = parameters.outputBiases.ToArray(),
            HiddenBiases = parameters.hiddenBiases.ToArray()
        };

        var json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    [System.Serializable]
    private class NeuralNetworkData
    {
        public float[] InputWeights;
        public float[] HiddenWeights;
        public float[] OutputBiases;
        public float[] HiddenBiases;
    }
}
