using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public static class NeuralNetworkUtility
{ 
    [BurstCompile]
    public static void ForwardPass(in NeuralNetworkComponent network,
    in NeuralNetworkParametersComponent parameters,
    in EnemyStateComponent inputs,
    out NativeArray<float> qvalues)
    {
        var inputWeights = parameters.inputWeights;
        var inputBiases = parameters.hiddenBiases;
        var hiddenWeights = parameters.hiddenWeights;
        var outputBiases = parameters.outputBiases;

        int inputSize = network.inputSize;
        int hiddenSize = network.hiddenSize;
        int outputSize = network.outputSize;

        // Compute hidden layer activations
        NativeArray<float> hiddenActivations = new NativeArray<float>(hiddenSize, Allocator.Temp);
        for (int i = 0; i < hiddenSize; i++)
        {
            float activation = inputBiases[i];
            for (int j = 0; j < inputSize; j++)
            {
                activation += GetInputValue(inputs, j) * inputWeights[j * hiddenSize + i];
            }
            hiddenActivations[i] = math.max(0, activation); // ReLU activation function
        }

        // Compute output layer activations
        NativeArray<float> outputActivations = new NativeArray<float>(outputSize, Allocator.Temp);
        for (int i = 0; i < outputSize; i++)
        {
            float activation = outputBiases[i];
            for (int j = 0; j < hiddenSize; j++)
            {
                activation += hiddenActivations[j] * hiddenWeights[j * outputSize + i];
            }
            outputActivations[i] = activation;
        }

        hiddenActivations.Dispose(); 

        qvalues = outputActivations;
    }  
    [BurstCompile]
    public static void ForwardPassWithIntermediate(in NeuralNetworkComponent network, 
        in NativeArray<float> inputWeights,
        in NativeArray<float> hiddenWeights,
        in NativeArray<float> hiddenBiases,
        in NativeArray<float> outputBiases,
        in EnemyStateComponent inputs,
        out NativeArray<float> outhiddenActivations,
        out NativeArray<float> outoutputActivations)
    {
        int inputSize = network.inputSize;
        int hiddenSize = network.hiddenSize;
        int outputSize = network.outputSize;

        // Compute hidden layer activations
        NativeArray<float> hiddenActivations = new NativeArray<float>(hiddenSize, Allocator.Temp);
        for (int i = 0; i < hiddenSize; i++)
        {
            float activation = hiddenBiases[i];
            for (int j = 0; j < inputSize; j++)
            {
                activation += GetInputValue(inputs, j) * inputWeights[j * hiddenSize + i];
            }
            hiddenActivations[i] = math.max(0, activation); // ReLU activation function
        }

        // Compute output layer activations
        NativeArray<float> outputActivations = new NativeArray<float>(outputSize, Allocator.Temp);
        for (int i = 0; i < outputSize; i++)
        {
            float activation = outputBiases[i];
            for (int j = 0; j < hiddenSize; j++)
            {
                activation += hiddenActivations[j] * hiddenWeights[j * outputSize + i];
            }
            outputActivations[i] = activation;
        }

        outhiddenActivations =  hiddenActivations;
        outoutputActivations =  outputActivations;
    }
    private static float GetInputValue(in EnemyStateComponent inputs, int index)
    {
        return index switch
        {
            0 => inputs.playerDistance,
            1 => inputs.playerHealth,
            2 => inputs.ownPositionX,
            3 => inputs.ownPositionY,
            4 => inputs.firstEnemyHealth,
            5 => inputs.secondEnemyHealth,
            6 => inputs.playerOrientationX,
            7 => inputs.playerOrientationZ,
            8 => inputs.enemiesSharedReward,
            9 => inputs.velocity,
            _ => 0f,
        };
    }
    public static float Max(NativeArray<float> array)
    {
        float max = float.MinValue;
        for (int i = 0; i < array.Length; i++)
        {
            max = math.max(max, array[i]);
        }
        return max;
    }
    public static (int,float) ArgMax(NativeArray<float> array)
    {
        float max = float.MinValue;
        int argMax = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                max = array[i];
                argMax = i;
            }
        }
        return (argMax, max);
    } 

//     private static void SetOutputValue(ref QValueComponent outputActivations, int index, float value)
//     {
//         switch (index)
//         {
//             case 0:
//                 outputActivations.forward = value;
//                 break;
//             case 1:
//                 outputActivations.backward = value;
//                 break;
//             case 2:
//                 outputActivations.stepRight = value;
//                 break;
//             case 3:
//                 outputActivations.stepLeft = value;
//                 break;
//             case 4:
//                 outputActivations.dash = value;
//                 break;
//             case 5:
//                 outputActivations.block = value;
//                 break;
//             case 6:
//                 outputActivations.heal = value;
//                 break;
//             case 7:
//                 outputActivations.jump = value;
//                 break;
//             case 8:
//                 outputActivations.stay = value;
//                 break;
//         }
//     }
}
