using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
//[InternalBufferCapacity(32)]
public struct NeuralNetworkReplayBufferElement : IBufferElementData
{
    public EnemyStateComponent state;
    public int action;
    public float reward;
    public EnemyStateComponent nextState;
    public bool done;
}
public struct DrlConfigComponent : IComponentData
{
    public float learningRate;
    public float discountFactor;
}
public struct NeuralNetworkParametersComponent : IComponentData
{
    public NativeArray<float> inputWeights;
    public NativeArray<float> hiddenWeights;
    public NativeArray<float> hiddenBiases;
    public NativeArray<float> outputBiases;
}
public struct TargetNeuralNetworkParametersComponent : IComponentData 
{
    public NativeArray<float> inputWeights;
    public NativeArray<float> hiddenWeights;
    public NativeArray<float> hiddenBiases;
    public NativeArray<float> outputBiases;

}

public struct NeuralNetworkComponent : IComponentData
{
    public int inputSize;
    public int hiddenSize;
    public int outputSize;

}