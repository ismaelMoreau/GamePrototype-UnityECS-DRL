using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

[UpdateInGroup(typeof(TransformSystemGroup))]
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigQlearnGrid>();
        state.RequireForUpdate<EnemyMovementComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<ConfigQlearnGrid>();
        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;
        
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position; 
            playerRotation = localTransform.ValueRO.Rotation;
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.Dependency.Complete();

        GridIndexer gridIndexer = new GridIndexer(config.width, config.height, config.cellSize, playerPosition, playerRotation);

        new EnemyMoveTowardsPlayerJob
        {
            GridIndexer = gridIndexer,
            DeltaTime = SystemAPI.Time.DeltaTime,
            PlayerPosition = playerPosition,
            PlayerRotation = playerRotation,
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
        }.ScheduleParallel();
        
        new EnemyDisableInGridJob
        {
            GridIndexer = gridIndexer,
            DeltaTime = SystemAPI.Time.DeltaTime,
            PlayerPosition = playerPosition,
            PlayerRotation = playerRotation,
        }.ScheduleParallel();

        state.Dependency.Complete();
    }

    public static bool IsPositionInSquare(float3 position, float3 center, quaternion playerRotation, int width, int height, float cellSize)
    {
        // Transform the position to the player's local coordinate space
        float3 localPosition = math.rotate(math.inverse(playerRotation), position - center);

        // Define the half size of the square
        float halfWidth = width * cellSize / 2;
        float halfHeight = height * cellSize / 2;

        // Check if the local position is within the square boundaries
        return math.abs(localPosition.x) <= halfWidth && math.abs(localPosition.z) <= halfHeight;
    }
}

[WithNone(typeof(EnemyActionComponent))]
[WithNone(typeof(HitBackwardEffectComponent))]
[BurstCompile]
public partial struct EnemyMoveTowardsPlayerJob : IJobEntity
{
    public GridIndexer GridIndexer;
    public float DeltaTime;
    public float3 PlayerPosition;
    public quaternion PlayerRotation;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(Entity entity, [ChunkIndexInQuery] int index, ref LocalTransform localTransform, in EnemyMovementComponent enemy)
    {
        if (!EnemyMovementSystem.IsPositionInSquare(localTransform.Position, PlayerPosition, PlayerRotation, GridIndexer.width, GridIndexer.height, GridIndexer.cellSize))
        {
            float3 direction = math.normalize(PlayerPosition - localTransform.Position);
            localTransform.Position += direction * enemy.speed * DeltaTime;
            localTransform.Rotation = quaternion.LookRotationSafe(direction, math.up());
        }
        else
        {
            ECB.SetComponentEnabled<EnemyActionComponent>(index, entity, true);
        }
    }
}

[BurstCompile]
[WithNone(typeof(HitBackwardEffectComponent))]
public partial struct EnemyDisableInGridJob : IJobEntity
{
    public GridIndexer GridIndexer;
    public float DeltaTime;
    public float3 PlayerPosition;
    public quaternion PlayerRotation;

    public void Execute(Entity entity, [ChunkIndexInQuery] int index, ref LocalTransform localTransform, in EnemyMovementComponent enemy,
        EnabledRefRW<EnemyActionComponent> enemyGridEnableRef, ref EnemyActionComponent EnemyActionComponent, ref EnemyActionTimerComponent enemyActionTimerComponent,
        ref EnemyRewardComponent enemyRewardComponent, ref PhysicsVelocity velocity)
    {
        velocity.Linear = float3.zero;
        velocity.Angular = float3.zero;

        if (!EnemyMovementSystem.IsPositionInSquare(localTransform.Position, PlayerPosition, PlayerRotation, GridIndexer.width, GridIndexer.height, GridIndexer.cellSize))
        {
            EnemyActionComponent.isDoingAction = false;
            enemyGridEnableRef.ValueRW = false;
        }
        else
        {
            int actualPosition = GridIndexer[localTransform.Position];

            float3 playerDirection = math.normalize(PlayerPosition - localTransform.Position);
            localTransform.Rotation = quaternion.LookRotationSafe(playerDirection, math.up());

            EnemyActionComponent.gridFlatenPosition = actualPosition;

            if (EnemyActionComponent.isDoingAction)
            {
                enemyActionTimerComponent.actionTimer += DeltaTime;

                if (enemyActionTimerComponent.actionTimer >= enemyActionTimerComponent.actionDuration)
                {
                    EnemyActionComponent.isDoingAction = false;

                    if (actualPosition != EnemyActionComponent.nextActionGridFlatenPosition)
                    {
                        float3 enemyNextActionWorldPosition = GridIndexer[EnemyActionComponent.nextActionGridFlatenPosition];
                        float3 enemyActualWorldPosition = GridIndexer[actualPosition];
                        enemyRewardComponent.earnReward -= math.distance(enemyNextActionWorldPosition, enemyActualWorldPosition);
                    }
                    EnemyActionComponent.IsReadyToUpdateQtable = true;
                    enemyActionTimerComponent.actionTimer = 0f;
                }
                else
                {
                    float3 moveDirection = float3.zero;
                    float speedMultiplier = 1.0f;

                    switch (EnemyActionComponent.chosenAction)
                    {
                        case 0:
                            moveDirection = playerDirection;
                            break;
                        case 1:
                            moveDirection = -playerDirection;
                            break;
                        case 2:
                            moveDirection = math.cross(playerDirection, math.up());
                            break;
                        case 3:
                            moveDirection = math.cross(math.up(), playerDirection);
                            break;
                        case 4:
                            moveDirection = playerDirection;
                            speedMultiplier = 2.0f;
                            break;
                        case 5: // Block
                            // Implement block logic here
                            break;
                        case 6: // Heal
                            // here its become something else where i got to implement sense of health and other enemy health  
                            break;
                        case 7: // Jump
                            // Implement jump logic here
                            break;
                        case 8: // Stay
                            // Implement stay logic here
                            break;
                    }

                    if (math.lengthsq(moveDirection) > 0.01f)
                    {
                        moveDirection = math.normalize(moveDirection);
                        localTransform.Position += moveDirection * enemy.speed * speedMultiplier * DeltaTime;
                    }
                }
            }
        }
    }
}
