using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random= Unity.Mathematics.Random;
using Unity.Physics;
using Unity.Rendering;
using Unity.VisualScripting;

[UpdateAfter(typeof(QlearningActionSelectionSystem))]
public partial struct QlearningRewardSystem : ISystem
{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<ConfigQlearn>();
        state.RequireForUpdate<ConfigQlearnGrid>();
    }
    public void OnUpdate(ref SystemState state){
        
        var configQlearn =  SystemAPI.GetSingleton<ConfigQlearn>();
        var configQlearnGrid =  SystemAPI.GetSingleton<ConfigQlearnGrid>();

        // var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        // NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        // float3 playerPosition = new float3(0, 0, 0);
        // foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed,var PlayerHealth, var entity) 
        //     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<PlayerHealth>>().WithEntityAccess())
        // {
            
        //     playerPosition = localTransform.ValueRO.Position; 
        //      bool isHits= collisionWorld.SphereCastAll(localTransform.ValueRO.Position,0.7f,localTransform.ValueRO.Position,0.5f,ref hits,new CollisionFilter
        //         {
        //             BelongsTo = (uint)CollisionLayer.player, 
        //             CollidesWith = (uint)CollisionLayer.enemies,  
        //             GroupIndex = 0
        //         });
        //     if(isHits){
        //             Debug.Log("player is hit");
        //             PlayerHealth.ValueRW.currentHealth -= 10;
        //         }
        // }
        // foreach(var hit in hits)
        // {
            
        //     if (state.EntityManager.HasComponent<EnemyRewardComponent>(hit.Entity)){
        //         var ennemyHealth = SystemAPI.GetComponentRW<EnemyHealthComponent>(hit.Entity);
        //         var enemyActionComponent = SystemAPI.GetComponentRW<EnemyActionComponent>(hit.Entity);
        //         var enemyPosition = SystemAPI.GetComponentRW<LocalTransform>(hit.Entity);
        //         var enemyReward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.Entity);
        //         var enemyColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(hit.Entity);
        //         enemyColor.ValueRW.Value = (Vector4)Color.red;
        //         if (EnemyMovementSystem.CalculateFlattenedGridPosition(playerPosition,enemyPosition.ValueRO.Position,configQlearnGrid.cellSize,configQlearnGrid.width,configQlearnGrid.height)
        //             != enemyActionComponent.ValueRO.nextActionGridFlatenPosition)
        //             {                  
        //                 var enemyNextActionWorldPosition = EnemyMovementSystem.CalculateWorldPositionFromFlattenedGrid(enemyActionComponent.ValueRO.nextActionGridFlatenPosition,playerPosition,configQlearnGrid.cellSize,configQlearnGrid.width,configQlearnGrid.height);      
                              
        //                 var distancePLannedActionPostionAndActualPostion =  math.distance(enemyPosition.ValueRO.Position, enemyNextActionWorldPosition);
        //                 // Far from the next action position, give negative reward proportional to the distance
        //                 enemyReward.ValueRW.earnReward += 1000/distancePLannedActionPostionAndActualPostion;
        //             }
        //         else{
        //             enemyReward.ValueRW.earnReward = 1000;
        //         }
            
        //         enemyActionComponent.ValueRW.IsReadyToUpdateQtable = true;
        //         ennemyHealth.ValueRW.currentEnnemyHealth -= 5; 
            
        //         if (ennemyHealth.ValueRO.currentEnnemyHealth <= 0)
        //         {
        //             state.EntityManager.SetComponentEnabled<DestroyTag>(hit.Entity,true);
        //         }
        //         if (SystemAPI.ManagedAPI.HasComponent<HealthBarUI>(hit.Entity))
        //         {
        //             SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.Entity, true);
        //         }
        //     }
        // }
        // hits.Dispose();
         float3 playerPosition = new float3(0, 0, 0);
        foreach ((RefRO<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed,var PlayerHealth ,var entity) 
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<PlayerHealth>>().WithEntityAccess())
        {
            playerPosition = localTransform.ValueRO.Position;
        }
            
            foreach ( var hitBuffer in SystemAPI.Query<DynamicBuffer<HitBufferElement>>())
            {
                foreach (var hit in hitBuffer)
                {
                    if (hit.IsHandled) continue;
                     var ennemyHealth = SystemAPI.GetComponentRW<EnemyHealthComponent>(hit.HitEntity);
                    var enemyActionComponent = SystemAPI.GetComponentRW<EnemyActionComponent>(hit.HitEntity);
                    var enemyPosition = SystemAPI.GetComponentRW<LocalTransform>(hit.HitEntity);
                    var enemyReward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.HitEntity);
                    var enemyColor = SystemAPI.GetComponentRW<URPMaterialPropertyBaseColor>(hit.HitEntity);
                    enemyColor.ValueRW.Value = (Vector4)Color.red;
                    if (EnemyMovementSystem.CalculateFlattenedGridPosition(playerPosition,enemyPosition.ValueRO.Position,configQlearnGrid.cellSize,configQlearnGrid.width,configQlearnGrid.height)
                        != enemyActionComponent.ValueRO.nextActionGridFlatenPosition)
                        {                  
                            var enemyNextActionWorldPosition = EnemyMovementSystem.CalculateWorldPositionFromFlattenedGrid(enemyActionComponent.ValueRO.nextActionGridFlatenPosition,playerPosition,configQlearnGrid.cellSize,configQlearnGrid.width,configQlearnGrid.height);      
                                
                            var distancePLannedActionPostionAndActualPostion =  math.distance(enemyPosition.ValueRO.Position, enemyNextActionWorldPosition);
                            // Far from the next action position, give negative reward proportional to the distance
                            enemyReward.ValueRW.earnReward += 1000/distancePLannedActionPostionAndActualPostion;
                        }
                    else{
                        enemyReward.ValueRW.earnReward = 1000;
                    }
                
                    enemyActionComponent.ValueRW.IsReadyToUpdateQtable = true;
                    ennemyHealth.ValueRW.currentEnnemyHealth -= 25; 
                    // TODO create damagebuffercomponent and damages system 
                    if (SystemAPI.ManagedAPI.HasComponent<HealthBarUI>(hit.HitEntity))
                    {
                        SystemAPI.SetComponentEnabled<UpdateHealthBarUI>(hit.HitEntity, true);
                    }

                    //TODO death system
                    if (ennemyHealth.ValueRO.currentEnnemyHealth <= 0)
                    {
                        state.EntityManager.SetComponentEnabled<DestroyTag>(hit.HitEntity,true);
                    }
                }
            }
    }
  
}
