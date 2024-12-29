using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

[UpdateBefore(typeof(DrlActionSelectionSystem))]
[UpdateInGroup(typeof(QlearningSystemGroup))]
public partial struct DrlEnemyStateSystem : ISystem
{
    private const float MaxPlayerDistance = 200f;
    private const float MaxHealth = 100f;
    private const float MaxSpeed = 10f;
    private const float RockNormalizationFactor = 10f;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
        state.RequireForUpdate<PlayerMovementComponent>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float3 playerPosition = float3.zero;
        float3 playerDirection = float3.zero;
        quaternion playerRotation = quaternion.identity;
        float playerHealth = 0;
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        // Cache player data
        foreach (var (localTransform, healthComponent) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
            playerHealth = healthComponent.ValueRO.currentHealth;
        }
        playerDirection = math.mul(playerRotation, new float3(0, 0, 1));

        // Cache enemies data
        var enemies = new NativeList<(Entity entity, float3 position, float health, float reward)>(Allocator.Temp);
        foreach (var (localTransform, health, reward, entity) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthComponent>, RefRO<EnemyRewardComponent>>().WithEntityAccess())
        {
            enemies.Add((entity, localTransform.ValueRO.Position, health.ValueRO.currentHealth, reward.ValueRO.earnReward));
        }

        foreach (var (enemyTransform, stateComponent, prevStateComponent, actionComponent, velocityComponent, enemyMovement, entity) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<EnemyStateComponent>, RefRW<EnemyPreviousStateComponent>, RefRO<EnemyActionComponent>, RefRO<PhysicsVelocity>, RefRW<EnemyMovementComponent>>().WithEntityAccess())
        {
            if (actionComponent.ValueRO.isDoingAction)
                continue;

            prevStateComponent.ValueRW.previousState = stateComponent.ValueRO;

            float3 enemyPosition = enemyTransform.ValueRW.Position;

            // Normalize state values
            stateComponent.ValueRW.playerDistance = math.clamp(math.distance(playerPosition, enemyPosition) / MaxPlayerDistance, 0f, 1f);
            stateComponent.ValueRW.playerHealth = math.clamp(playerHealth / MaxHealth, 0f, 1f);
            stateComponent.ValueRW.ownPositionX = math.clamp(enemyPosition.x / MaxPlayerDistance, 0f, 1f);
            stateComponent.ValueRW.ownPositionY = math.clamp(enemyPosition.y / MaxPlayerDistance, 0f, 1f);
            stateComponent.ValueRW.velocity = math.clamp(math.length(velocityComponent.ValueRO.Linear) / MaxSpeed, 0f, 1f);
            stateComponent.ValueRW.playerOrientationX = playerDirection.x;
            stateComponent.ValueRW.playerOrientationZ = playerDirection.z;

            UpdateNearestEnemies(enemies, entity, enemyPosition, ref stateComponent.ValueRW);
            UpdateNearestRock(collisionWorld, enemyPosition, ref enemyMovement.ValueRW, ref stateComponent.ValueRW);
        }

        enemies.Dispose();
    }

    private void UpdateNearestEnemies(NativeList<(Entity entity, float3 position, float health, float reward)> enemies, Entity self, float3 position, ref EnemyStateComponent state)
    {
        float nearestDistance1 = float.MaxValue;
        float nearestDistance2 = float.MaxValue;
        float firstEnemyHealth = 0;
        float firstEnemyReward = 0;
        float secondEnemyHealth = 0;
        float secondEnemyReward = 0;

        foreach (var enemy in enemies)
        {
            if (enemy.entity == self) continue;

            float distance = math.distance(position, enemy.position);
            if (distance < nearestDistance1)
            {
                nearestDistance2 = nearestDistance1;
                secondEnemyHealth = firstEnemyHealth;
                secondEnemyReward = firstEnemyReward;

                nearestDistance1 = distance;
                firstEnemyHealth = enemy.health;
                firstEnemyReward = enemy.reward;
            }
            else if (distance < nearestDistance2)
            {
                nearestDistance2 = distance;
                secondEnemyHealth = enemy.health;
                secondEnemyReward = enemy.reward;
            }
        }

        state.firstEnemyHealth = math.clamp(firstEnemyHealth / MaxHealth, 0f, 1f);
        state.secondEnemyHealth = math.clamp(secondEnemyHealth / MaxHealth, 0f, 1f);
        state.enemiesSharedReward = math.clamp((firstEnemyReward + secondEnemyReward) / 3f, 0f, 1f);
    }

    private void UpdateNearestRock(CollisionWorld collisionWorld, float3 position, ref EnemyMovementComponent movement, ref EnemyStateComponent state)
    {
        float searchRadius = 10f;
        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

        if (collisionWorld.OverlapSphere(position, searchRadius, ref hits, new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1 << 10,
            GroupIndex = 0
        }))
        {
            float nearestDistance = float.MaxValue;
            foreach (var hit in hits)
            {
                float distance = math.distance(position, hit.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    movement.neareasRockPosition = hit.Position;
                }
            }
            state.nearestRockDistance = math.clamp(nearestDistance / RockNormalizationFactor, 0f, 1f);
        }
        else
        {
            state.nearestRockDistance = 1f;
        }

        hits.Dispose();
    }
}
