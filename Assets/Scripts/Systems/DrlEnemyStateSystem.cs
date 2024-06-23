

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random = Unity.Mathematics.Random;
using Unity.Physics;
using Unity.Rendering;
using Unity.VisualScripting;

[UpdateBefore(typeof(DrlActionSelectionSystem))]
[UpdateInGroup(typeof(QlearningSystemGroup))]
public partial struct DrlEnemyStateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
        state.RequireForUpdate<PlayerMovementComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Cache player data
        float3 playerPosition = float3.zero;
        float3 playerDirection = float3.zero;
        quaternion playerRotation = quaternion.identity;
        float playerHealth = 0;
        
        foreach (var (localTransform, playerMovement, playerHealthComponent)
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>, RefRO<HealthComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position; 
            playerRotation = localTransform.ValueRO.Rotation;
            playerHealth = playerHealthComponent.ValueRO.currentHealth;
        }
        playerDirection = math.mul(playerRotation, new float3(0, 0, 1));
        // Cache enemies data
        var enemies = new NativeList<(Entity entity, float3 position, float health, float earnReward ,int action)>(Allocator.Temp);
        foreach (var (localTransform, health, reward,action,entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthComponent>, RefRO<EnemyRewardComponent>,RefRO<EnemyActionComponent>>().WithEntityAccess())
        {
            enemies.Add((entity, localTransform.ValueRO.Position, health.ValueRO.currentHealth, reward.ValueRO.earnReward ,action.ValueRO.chosenAction));
        }

        // Update each enemy's EnemyStateComponent
        foreach (var (enemyMovement, EnemyStateComponent,previousState,enemyActionComponent,velocity,entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRW<EnemyStateComponent>,RefRW<EnemyPreviousStateComponent> ,RefRO<EnemyActionComponent>,RefRO<PhysicsVelocity>>().WithEntityAccess())
        {
            if(enemyActionComponent.ValueRO.isDoingAction){continue;};
            
            previousState.ValueRW.previousState = EnemyStateComponent.ValueRO;
            float3 enemyPosition = enemyMovement.ValueRW.Position;

            // Calculate distance to player
            EnemyStateComponent.ValueRW.playerDistance = math.min(1000,math.distance(playerPosition, enemyPosition))/1000;
            EnemyStateComponent.ValueRW.playerHealth = math.min(playerHealth/100,0f);
            EnemyStateComponent.ValueRW.playerOrientationX = playerDirection.x;
            EnemyStateComponent.ValueRW.playerOrientationZ = playerDirection.z;
            EnemyStateComponent.ValueRW.ownPositionX = enemyMovement.ValueRW.Position.x;
            EnemyStateComponent.ValueRW.ownPositionY = enemyMovement.ValueRW.Position.y;
            const float maxSpeed = 10f;
            EnemyStateComponent.ValueRW.velocity = math.clamp(math.length(velocity.ValueRO.Linear) / maxSpeed, 0f, 1f);


            // Find the two nearest enemies
            float nearestDistance1 = float.MaxValue;
            float nearestDistance2 = float.MaxValue;
            Entity nearestEnemy1 = Entity.Null;
            Entity nearestEnemy2 = Entity.Null;
            float firstEnemyHealth = 0;
            float firstEnemyearnReward = 0;
            float secondEnemyHealth = 0;
            float secondEnemyearnReward = 0;
            foreach (var enemy in enemies)
            {
                if (enemy.entity == entity)
                    continue;

                float distance = math.distance(enemyPosition, enemy.position);
                if (distance < nearestDistance1)
                {
                    nearestDistance2 = nearestDistance1;
                    nearestEnemy2 = nearestEnemy1;

                    nearestDistance1 = distance;
                    nearestEnemy1 = enemy.entity;

                    firstEnemyHealth = enemy.health;
                    firstEnemyearnReward = enemy.earnReward;

                   
                }
                else if (distance < nearestDistance2)
                {
                    nearestDistance2 = distance;
                    nearestEnemy2 = enemy.entity;

                    secondEnemyHealth = enemy.health;
                    secondEnemyearnReward = enemy.earnReward;
                }
            }
            float sharedReward = 0f;
            // Update nearest enemies info
            if (nearestEnemy1 != Entity.Null)
            {
                //EnemyStateComponent.ValueRW.firstNearestEnemyDistance = math.min(100,nearestDistance1)/100;
                EnemyStateComponent.ValueRW.firstEnemyHealth = firstEnemyHealth/100;
                //EnemyStateComponent.ValueRW.firstEnemyAction = nearestEnemy1Action.chosenAction;
                sharedReward += firstEnemyearnReward;
            }
            else
            {
                //EnemyStateComponent.ValueRW.firstNearestEnemyDistance = 0;
                EnemyStateComponent.ValueRW.firstEnemyHealth = 0;
                //EnemyStateComponent.ValueRW.firstEnemyAction = 0;
            }

            if (nearestEnemy2 != Entity.Null)
            {

                //EnemyStateComponent.ValueRW.secondNearestEnemyDistance = math.min(100,nearestDistance2)/100;
                EnemyStateComponent.ValueRW.secondEnemyHealth = secondEnemyHealth/100;
                sharedReward += secondEnemyearnReward;
                //EnemyStateComponent.ValueRW.secondEnemyAction = nearestEnemy2Action.chosenAction;
            }
            else
            {
                //EnemyStateComponent.ValueRW.secondNearestEnemyDistance = 0;
                EnemyStateComponent.ValueRW.secondEnemyHealth = 0;
                //EnemyStateComponent.ValueRW.secondEnemyAction = 0;
            }

            if (sharedReward!=0)
            {
                EnemyStateComponent.ValueRW.enemiesSharedReward = sharedReward/3;
            }
            // Optionally, update enemiesSharedReward if needed
            EnemyStateComponent.ValueRW.enemiesSharedReward = CalculateEnemiesSharedReward();
        }
        enemies.Dispose();
    }

    private float CalculateEnemiesSharedReward()
    {
        // Implement your logic to calculate the shared reward for enemies
        return 0f;
    }
   
}
