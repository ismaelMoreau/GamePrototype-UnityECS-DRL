using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Rendering;
using Random = Unity.Mathematics.Random;
using Unity.Physics;
using System.Runtime.InteropServices;

public partial struct JumpSkillSystem : ISystem
{
   
    //PlayerTargetPosition priorMousePositionAfterButtonPress;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state){
         state.RequireForUpdate<SkillsConfig>();
         state.RequireForUpdate<PlayerTargetPosition>();
         state.RequireForUpdate<Target>();
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var skillsConfig = SystemAPI.GetSingleton<SkillsConfig>();   
        var playerTargetPosition = SystemAPI.GetSingleton<PlayerTargetPosition>();
        Entity targetPrefab = SystemAPI.GetSingletonEntity<Target>();
        Target target = state.EntityManager.GetComponentData<Target>(targetPrefab);
        
        if (!target.isJumpTarget){
            return;
        }      
          
        if (playerTargetPosition.isWaitingForClick){
            state.EntityManager.SetComponentData(targetPrefab, new LocalTransform
            {
                Position = playerTargetPosition.targetMousePosition,
                Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                Rotation = quaternion.identity
            });
           return;
        }
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityQuery query = SystemAPI.QueryBuilder().WithAll<Target>().Build();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        foreach ((RefRW<LocalTransform> localTransform, RefRW<PlayerMovementComponent> player, RefRW<PlayerTargetPosition> targetPosition,RefRW<PhysicsVelocity> velocity) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerMovementComponent>, RefRW<PlayerTargetPosition>,RefRW<PhysicsVelocity>>())
        {  
            float3 currentPosition = localTransform.ValueRW.Position; 
            float3 direction = math.normalize(targetPosition.ValueRO.targetMousePosition-currentPosition);
            if(player.ValueRW.isGrounded) {
                player.ValueRW.isGrounded = false;  
        
                float leapStrength = 6.0f; 
                direction.y = math.max(direction.y, 2f);
                velocity.ValueRW.Linear = direction * leapStrength ;
                player.ValueRW.JumpStartTime = SystemAPI.Time.ElapsedTime;
            }
            else{
                velocity.ValueRW.Linear.y -= 9.81f * SystemAPI.Time.DeltaTime; //
          
                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
              
            
                bool isOnGroundNow = physicsWorld.BoxCastAll(localTransform.ValueRO.Position,localTransform.ValueRO.Rotation,localTransform.ValueRO.Scale/2,
                math.mul(localTransform.ValueRO.Rotation, new float3(0,0,1)),1,ref hits,new CollisionFilter
                    {
                        BelongsTo = ~0u, // Ray belongs to all layers (use a specific mask if needed)
                        CollidesWith = (uint)CollisionLayer.ground,  // Only collide with ground layer
                        GroupIndex = 0
                    });
        
                if (isOnGroundNow && (SystemAPI.Time.ElapsedTime-player.ValueRO.JumpStartTime)>1)
                {
                    ecb.DestroyEntity(query,EntityQueryCaptureMode.AtPlayback);
                    player.ValueRW.isGrounded = true; 
                    
                }
                hits.Dispose();
            }
        }
    
        ecb.Playback(state.EntityManager);
        //state.EntityManager.DestroyEntity(query);
        // You are responsible for disposing of any ECB you create.
        ecb.Dispose();// Jump is complete
    }

}
public enum CollisionLayer{
    ground = 1 << 6,
    enemies = 1 << 7,

    player= 1 << 8,
    bullets = 1<< 9
}