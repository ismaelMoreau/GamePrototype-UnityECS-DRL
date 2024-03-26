using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using JetBrains.Annotations;
public partial struct BulletstSystem : ISystem
{
   

    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile] 
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
     
        float3 playerPosition = new float3(0, 0, 0);
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position; 
        }
        float nearestDistanceEnemy = 10000;
        float3 nearestEnemyPosition = new float3(0, 0, 0);
        foreach ((RefRW<LocalTransform> localTransform, RefRO<EnemyComponent> enemy,Entity e) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>>().WithEntityAccess())
        {
            float distance = math.distance(playerPosition, localTransform.ValueRW.Position);
            if (distance < nearestDistanceEnemy) {
                nearestDistanceEnemy = distance;
                nearestEnemyPosition = localTransform.ValueRW.Position;
            }
            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
            bool isHits= physicsWorld.SphereCastAll(localTransform.ValueRO.Position,1,localTransform.ValueRO.Position,1,ref hits,new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.enemies, // Ray belongs to all layers (use a specific mask if needed)
                    CollidesWith = (uint)CollisionLayer.bullets,  // Only collide with ground layer
                    GroupIndex = 0
                });
            if(isHits)
            {
                ecb.AddComponent<DestroyTag>(e);
            }
            hits.Dispose();
        }

        // Schedule the job
        var job = new BulletMoveTowardsEnnemyJob
        {
            physicsWorld = physicsWorld ,
            nearestEnemyPosition = nearestEnemyPosition,
            DeltaTime = SystemAPI.Time.DeltaTime,
            CollisionRadius = 1.0f, // Define your collision radius
            CommandBuffer = ecb.AsParallelWriter()
        };
        
        
        state.Dependency = job.Schedule(state.Dependency);
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
// Job to move enemies towards the player
public partial struct BulletMoveTowardsEnnemyJob : IJobEntity
{
    public PhysicsWorldSingleton physicsWorld;
    public float3 nearestEnemyPosition;
    public float DeltaTime;
    public float CollisionRadius;

    //[NativeDisableParallelForRestriction]
    internal EntityCommandBuffer.ParallelWriter CommandBuffer; 

    public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in BulletsMouvementComponent bullets)
    {
        float3 direction = math.normalize(nearestEnemyPosition - localTransform.Position);
        float distance = math.distance(nearestEnemyPosition, localTransform.Position);
        localTransform.Position += direction * bullets.speed * DeltaTime;
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        bool isHits= physicsWorld.SphereCastAll(localTransform.Position,1,localTransform.Position,1,ref hits,new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.player,
                CollidesWith = (uint)CollisionLayer.enemies,  // Only collide with layer
                GroupIndex = 0
            });
        if(isHits)
        {
            CommandBuffer.AddComponent<DestroyTag>(index,entity);
        }
        hits.Dispose();
    }
} 
