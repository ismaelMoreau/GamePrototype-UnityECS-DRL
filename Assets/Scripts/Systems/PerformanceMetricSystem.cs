using Unity.Entities;


[UpdateAfter(typeof(DrlRewardSystem))]
[UpdateInGroup(typeof(QlearningSystemGroup))]
public partial struct PerformanceMetricSystem : ISystem
{
       public void OnCreate(ref SystemState state)
    {
       state.EntityManager.CreateEntity(typeof(RewardMetricComponent));
       state.EntityManager.CreateEntity(typeof(LossMetricComponent));
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var currentTime = SystemAPI.Time.ElapsedTime;

       

        // Sum up the cumulative rewards of all entities
        int totalEnemies = 0;
        float totalCumulativeReward = 0;
        foreach (var rewardTracker in SystemAPI.Query<RefRO<EnemyRewardComponent>>())
        {
            totalCumulativeReward += rewardTracker.ValueRO.earnReward;
            totalEnemies+= 1;
        }

        
        var performanceMetricEntity  = SystemAPI.GetSingletonEntity<RewardMetricComponent>();
        // Update the performance metric component
        state.EntityManager.SetComponentData(performanceMetricEntity, new RewardMetricComponent
        {
            Time = (float)currentTime,
            EntityCount = totalEnemies,
            TotalCumulativeReward = totalCumulativeReward
        });
    }
}


