using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Random= Unity.Mathematics.Random;
using Unity.Physics;

[UpdateAfter(typeof(QlearningInitSystem))]
public partial struct QlearningRewardSystem : ISystem
{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<ConfigQlearn>();
    }
    public void OnUpdate(ref SystemState state){
        
        var configQlearn =  SystemAPI.GetSingleton<ConfigQlearn>();

        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        float3 playerPosition = new float3(0, 0, 0);
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed,var entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>().WithEntityAccess())
        {
            
            playerPosition = localTransform.ValueRO.Position; 
             bool isHits= physicsWorld.SphereCastAll(localTransform.ValueRO.Position,0.7f,localTransform.ValueRO.Position,1,ref hits,new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.player, 
                    CollidesWith = (uint)CollisionLayer.enemies,  
                    GroupIndex = 0
                });
            //if(isHits){Debug.Log("is hit");}//TODO reduce player health}
        }
        foreach(var hit in hits)
        {
            if (state.EntityManager.HasComponent<DestroyTag>(hit.Entity))
            {
                state.EntityManager.SetComponentEnabled<DestroyTag>(hit.Entity,true);
            }
            // if (state.EntityManager.HasComponent<EnemyRewardComponent>(hit.Entity)){
            //     var reward = SystemAPI.GetComponentRW<EnemyRewardComponent>(hit.Entity);
            //     reward.ValueRW.earnReward = 10000;
            //     //TODO reduce enemy health and move back and change color 
            // }
        }
        hits.Dispose();
    }
  
}
