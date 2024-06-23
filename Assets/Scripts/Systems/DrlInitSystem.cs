using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(QlearningSystemGroup), OrderFirst = true)]

public partial struct DrlInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

        state.RequireForUpdate<NeuralNetworkComponent>();

    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;// one time update

        var neuralNetworkComponent = SystemAPI.GetSingleton<NeuralNetworkComponent>();
        var networkEntity = SystemAPI.GetSingletonEntity<NeuralNetworkComponent>();

        var inputSize = neuralNetworkComponent.inputSize;
        var hiddenSize = neuralNetworkComponent.hiddenSize;
        var outputSize = neuralNetworkComponent.outputSize;

        var inputWeightCount = inputSize * hiddenSize;
        var hiddenWeightCount = hiddenSize * outputSize;
        var weightCount = inputWeightCount + hiddenWeightCount;
        var biasCount = inputSize + outputSize;

        var weights = new NativeArray<float>(weightCount, Allocator.Temp);
        var hiddenBiases = new NativeArray<float>(hiddenSize, Allocator.Temp);
        var outputBiases = new NativeArray<float>(outputSize, Allocator.Temp);

        Random random = new Random(1);

        for (int i = 0; i < inputWeightCount + hiddenWeightCount; i++)
        {
            weights[i] = random.NextFloat(-1f, 1f);
        }

        for (int i = 0; i < hiddenSize; i++)
        {
            hiddenBiases[i] = random.NextFloat(-1f, 1f);
        }

        for (int i = 0; i < outputSize; i++)
        {
            outputBiases[i] = random.NextFloat(-1f, 1f);
        }



        state.EntityManager.AddComponentData(networkEntity, new NeuralNetworkParametersComponent
        {
            inputWeights = new NativeArray<float>(weights.GetSubArray(0, inputWeightCount), Allocator.Persistent),

            hiddenBiases = new NativeArray<float>(hiddenBiases, Allocator.Persistent),

            hiddenWeights = new NativeArray<float>(weights.GetSubArray(inputWeightCount, hiddenWeightCount), Allocator.Persistent),

            //outputWeights = new NativeArray<float>(weights.GetSubArray(inputWeightCount + hiddenWeightCount, hiddenWeightCount), Allocator.Persistent),
            outputBiases = new NativeArray<float>(outputBiases, Allocator.Persistent),

        });
        state.EntityManager.AddComponentData(networkEntity, new TargetNeuralNetworkParametersComponent
        {
            inputWeights = new NativeArray<float>(weights.GetSubArray(0, inputWeightCount), Allocator.Persistent),

            hiddenBiases = new NativeArray<float>(hiddenBiases, Allocator.Persistent),

            hiddenWeights = new NativeArray<float>(weights.GetSubArray(inputWeightCount, hiddenWeightCount), Allocator.Persistent),

            outputBiases = new NativeArray<float>(outputBiases, Allocator.Persistent)

        });

        weights.Dispose();
        hiddenBiases.Dispose();
        outputBiases.Dispose();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        foreach (var param in SystemAPI.Query<RefRW<NeuralNetworkParametersComponent>>())
        {
            param.ValueRW.inputWeights.Dispose();
            param.ValueRW.hiddenBiases.Dispose();
            param.ValueRW.hiddenWeights.Dispose();
            //param.ValueRW.outputWeights.Dispose();
            param.ValueRW.outputBiases.Dispose();
        }
        foreach (var param in SystemAPI.Query<RefRW<TargetNeuralNetworkParametersComponent>>())
        {
            param.ValueRW.inputWeights.Dispose();
            param.ValueRW.hiddenBiases.Dispose();
            param.ValueRW.hiddenWeights.Dispose();
            param.ValueRW.outputBiases.Dispose();
        }
    }
}
