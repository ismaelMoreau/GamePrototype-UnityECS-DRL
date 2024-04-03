using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
public partial struct EnemyMovementSystem : ISystem
{
   

    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile] 
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

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
            DeltaTime = SystemAPI.Time.DeltaTime,
            //CollisionRadius = 1.0f, // Define your collision radius
            
        };
        
        
        state.Dependency = job.Schedule(state.Dependency);
        state.Dependency.Complete();

     
    }
}
// Job to move enemies towards the player
public partial struct EnemyMoveTowardsPlayerJob : IJobEntity
{
    public float3 PlayerPosition;
    public float DeltaTime;
    //public float CollisionRadius;

    //[NativeDisableParallelForRestriction]
    public EntityCommandBuffer.ParallelWriter CommandBuffer; 

    public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in EnemyComponent enemy)
    {
        float3 direction = math.normalize(PlayerPosition - localTransform.Position);
        float distance = math.distance(PlayerPosition, localTransform.Position);
        localTransform.Position += direction * enemy.speed * DeltaTime;
      
       
    }
} 
