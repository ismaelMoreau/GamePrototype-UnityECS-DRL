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
using Unity.Barracuda;

[UpdateBefore(typeof(DrlEnemyStateSystem))]
[UpdateInGroup(typeof(QlearningSystemGroup))]
public partial struct DrlRewardSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {


        state.RequireForUpdate<HitBufferElement>();
        state.RequireForUpdate<EnemyMovementComponent>();
        state.RequireForUpdate<PlayerMovementComponent>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {



        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;

        foreach ((RefRO<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed, var PlayerHealth, var entity)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<HealthComponent>>().WithEntityAccess())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
        }

        // Cache enemies data
        var enemiesCached = new NativeList<(Entity entity, float3 position, float health, float earnReward, int action)>(Allocator.Temp);
        foreach (var (localTransform, health, reward, action, entity)
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthComponent>, RefRO<EnemyRewardComponent>, RefRO<EnemyActionComponent>>().WithEntityAccess())
        {
            enemiesCached.Add((entity, localTransform.ValueRO.Position, health.ValueRO.currentHealth, reward.ValueRO.earnReward, action.ValueRO.chosenAction));
        }
        foreach (var (localTransform, enemyActionComponent, enemyReward, enemyActionTimerComponent, enemyActionsCooldown,enemyMovementComponent, entity) in
            SystemAPI.Query<RefRO<LocalTransform>,
            RefRO<EnemyActionComponent>,
            RefRW<EnemyRewardComponent>,
            RefRW<EnemyActionTimerComponent>,
            RefRO<EnemyActionsCooldownComponent>,
            RefRO<EnemyMovementComponent>>().WithEntityAccess())
        {

            if (enemyActionComponent.ValueRO.isDoingAction) continue;
            var Distance = math.distance(playerPosition, localTransform.ValueRO.Position);
            var penalty = enemyActionComponent.ValueRO.numberOfSteps * -0.3f;
            
            //Reduce the penalty based on the distance
            var maxDistance = 50.0f;
            var distanceFactor =  math.clamp(Distance / maxDistance, 0.0f, 0.95f); 
            var proximityFactor = math.pow(distanceFactor, 2);  // Quadratic fall-off
            penalty *= proximityFactor;
            //Debug.Log("Penalty: " + penalty);
            enemyReward.ValueRW.earnReward += penalty;
            
            //nearestRock penalty
            if (math.distance(enemyMovementComponent.ValueRO.neareasRockPosition, localTransform.ValueRO.Position) < 0.5) {
                enemyReward.ValueRW.earnReward -= 1000;
                //Debug.Log($"Nearest Rock Penalty distance: {math.distance(enemyMovementComponent.ValueRO.neareasRockPosition, localTransform.ValueRO.Position)}");
            };

            //heal reward
            if (enemyActionComponent.ValueRO.chosenAction != 6 ) { continue; };

            foreach (var enemy in enemiesCached)
            {
                var distance = math.distance(enemy.position, localTransform.ValueRO.Position);
               
                if (distance > 5 || enemy.entity == entity) { continue; }

                var health = SystemAPI.GetComponentRW<HealthComponent>(enemy.entity);

                if (health.ValueRO.currentHealth < 100)
                {
                    health.ValueRW.currentHealth += 15;//TODO: buffer for damage and healing
                    enemyReward.ValueRW.earnReward += 5;
                    SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(enemy.entity, true);
                }
            }

        }
        enemiesCached.Dispose();
        foreach (var hitBuffer in SystemAPI.Query<DynamicBuffer<HitBufferElement>>())
        {
            foreach (var hit in hitBuffer)
            {
                if (hit.IsHandled) continue;
                if (SystemAPI.HasComponent<EnemyMovementComponent>(hit.HitEntity))
                {
                    var enemyHealth = SystemAPI.GetComponentRW<HealthComponent>(hit.HitEntity);
                    var enemyActionComponent = SystemAPI.GetComponentRW<EnemyActionComponent>(hit.HitEntity);
                    var enemyReward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.HitEntity);
                    var trigger = SystemAPI.GetComponentRW<PlayerWeaponTag>(hit.triggerEntity);


                    if (enemyActionComponent.ValueRO.chosenAction == 5)//blocked
                    {
                        enemyReward.ValueRW.earnReward += 40;
                    }
                    else
                    {
                        enemyReward.ValueRW.earnReward -= 10;
                        if (trigger.ValueRO.playerSword)
                            enemyHealth.ValueRW.currentHealth -= 50;//TODO: buffer for damage and healing
                        else if (trigger.ValueRO.playerShield)
                            enemyHealth.ValueRW.currentHealth -= 10;
                    }
                    //enemyActionComponent.ValueRW.IsReadyToUpdateQtable = true;


                    if (SystemAPI.ManagedAPI.HasComponent<HealthBarUI>(hit.HitEntity))
                    {
                        SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.HitEntity, true);
                    }
                }
                if (SystemAPI.HasComponent<PlayerMovementComponent>(hit.HitEntity))
                {
                    var playerHealth = SystemAPI.GetComponentRW<HealthComponent>(hit.HitEntity);
                    playerHealth.ValueRW.currentHealth -= 10;
                    var enemyHealth = SystemAPI.GetComponentRW<HealthComponent>(hit.triggerEntity);
                    // var enemyActionComponent = SystemAPI.GetComponentRW<EnemyActionComponent>(hit.triggerEntity);
                    // var enemyPosition = SystemAPI.GetComponentRW<LocalTransform>(hit.triggerEntity);
                    var enemyReward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.triggerEntity);
                    //var enemyColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(hit.triggerEntity);
                    //enemyColor.ValueRW.Value = (Vector4)Color.red;

                    // if (gridIndexer[enemyPosition.ValueRO.Position] != enemyActionComponent.ValueRO.nextActionGridFlatenPosition)
                    // {
                    //     var enemyNextActionWorldPosition = gridIndexer[enemyActionComponent.ValueRO.nextActionGridFlatenPosition];

                    //     var distancePlannedActionPositionAndActualPosition = math.distance(enemyPosition.ValueRO.Position, enemyNextActionWorldPosition);
                    //     // Far from the next action position, give negative reward proportional to the distance
                    //     enemyReward.ValueRW.earnReward += 200 / distancePlannedActionPositionAndActualPosition;
                    // }
                    // else
                    // {
                    enemyReward.ValueRW.earnReward = 100;
                    //}

                    //.ValueRW.IsReadyToUpdateQtable = true;
                    enemyHealth.ValueRW.currentHealth -= 5;//TODO: buffer for damage and healing

                    SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.triggerEntity, true);
                    SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.HitEntity, true);
                }
            }
        }
    }
}
