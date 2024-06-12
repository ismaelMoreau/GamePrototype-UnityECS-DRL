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

[UpdateAfter(typeof(QlearningActionSelectionSystem))]
public partial struct QlearningRewardSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigQlearn>();
        state.RequireForUpdate<ConfigQlearnGrid>();
        state.RequireForUpdate<HitBufferElement>();
        state.RequireForUpdate<EnemyMovementComponent>();
        state.RequireForUpdate<PlayerMovementComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var configQlearn = SystemAPI.GetSingleton<ConfigQlearn>();
        var configQlearnGrid = SystemAPI.GetSingleton<ConfigQlearnGrid>();

        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;

        foreach ((RefRO<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed, var PlayerHealth, var entity)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<HealthComponent>>().WithEntityAccess())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
        }

        GridIndexer gridIndexer = new GridIndexer(configQlearnGrid.width, configQlearnGrid.height, configQlearnGrid.cellSize, playerPosition, playerRotation);

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

                    
                    if(enemyActionComponent.ValueRO.chosenAction == 5 )//blocked
                    {
                        enemyReward.ValueRW.earnReward += 100;
                    }else{
                        enemyReward.ValueRW.earnReward -= 100;
                        if (trigger.ValueRO.playerSword)
                            enemyHealth.ValueRW.currentHealth -= 50;
                        else if (trigger.ValueRO.playerShield)
                            enemyHealth.ValueRW.currentHealth -= 10;
                    }
                    enemyActionComponent.ValueRW.IsReadyToUpdateQtable = true;


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
                    var enemyActionComponent = SystemAPI.GetComponentRW<EnemyActionComponent>(hit.triggerEntity);
                    var enemyPosition = SystemAPI.GetComponentRW<LocalTransform>(hit.triggerEntity);
                    var enemyReward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.triggerEntity);
                    var enemyColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(hit.triggerEntity);
                    enemyColor.ValueRW.Value = (Vector4)Color.red;

                    if (gridIndexer[enemyPosition.ValueRO.Position] != enemyActionComponent.ValueRO.nextActionGridFlatenPosition)
                    {
                        var enemyNextActionWorldPosition = gridIndexer[enemyActionComponent.ValueRO.nextActionGridFlatenPosition];

                        var distancePlannedActionPositionAndActualPosition = math.distance(enemyPosition.ValueRO.Position, enemyNextActionWorldPosition);
                        // Far from the next action position, give negative reward proportional to the distance
                        enemyReward.ValueRW.earnReward += 200 / distancePlannedActionPositionAndActualPosition;
                    }
                    else
                    {
                        enemyReward.ValueRW.earnReward = 200;
                    }

                    enemyActionComponent.ValueRW.IsReadyToUpdateQtable = true;
                    enemyHealth.ValueRW.currentHealth -= 5;

                    SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.triggerEntity, true);
                    SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.HitEntity, true);
                }
            }
        }
    }
}
