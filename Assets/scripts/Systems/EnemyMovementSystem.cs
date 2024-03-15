using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile] // Optional, for performance
    public void OnUpdate(ref SystemState state)
    {
        float3 playerPosition = new float3(0, 0, 0);
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position; 
        }

        // Schedule the job
        var job = new EnemyMoveTowardsPlayerJob
        {
            PlayerPosition = playerPosition,
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        
        state.Dependency = job.Schedule(state.Dependency);
    }
}
// Job to move enemies towards the player
public partial struct EnemyMoveTowardsPlayerJob : IJobEntity
{
    public float3 PlayerPosition;
    public float DeltaTime;

    public void Execute(ref LocalTransform localTransform, in EnemyComponent enemy)
    {
        float3 direction = math.normalize(PlayerPosition - localTransform.Position);
        localTransform.Position += direction * enemy.speed * DeltaTime;
    }
}
