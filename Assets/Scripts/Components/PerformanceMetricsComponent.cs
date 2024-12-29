using Unity.Entities;
public struct LossMetricComponent : IComponentData
{
    public float Time;
    public int EntityCount;
    public float totalLoss;
}

public struct RewardMetricComponent : IComponentData
{
    public float Time;
    public int EntityCount;
    public float TotalCumulativeReward;
}