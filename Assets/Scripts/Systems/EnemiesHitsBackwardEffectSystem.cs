
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(TransformSystemGroup))]
public partial struct EnemiesHitsBackwardEffectSystem : ISystem
{
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HitBufferElement>();
        state.RequireForUpdate<BackwardEffect>();
          state.RequireForUpdate<GamePlayingTag>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        foreach ( var hitBuffer in SystemAPI.Query<DynamicBuffer<HitBufferElement>>())
        {
            foreach (var hit in hitBuffer)
            {
                if (hit.IsHandled || !SystemAPI.HasComponent<HitBackwardEffectComponent>(hit.HitEntity))continue;
                var backwardEffect = SystemAPI.GetComponentRW<BackwardEffect>(hit.HitEntity);
                if (!backwardEffect.ValueRO.haveAbackwardEffect)continue;

               
                state.EntityManager.SetComponentEnabled<EnemyActionComponent>(hit.HitEntity,false);
                state.EntityManager.SetComponentEnabled<HitBackwardEffectComponent>(hit.HitEntity,true);
                var backEffect = SystemAPI.GetComponentRW<HitBackwardEffectComponent>(hit.HitEntity);
                backEffect.ValueRW.direction = hit.Normal;
               // transform.ValueRW.Position = hit.Normal * 2  ;
                //localTransform.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }
        }
        foreach (var (localTransform,hitEffect,physicsVelocity,physicsMass,entity) in SystemAPI.Query<RefRW<LocalTransform>,RefRW<HitBackwardEffectComponent>,RefRW<PhysicsVelocity>,RefRW<PhysicsMass>>().WithEntityAccess())
        {
            //if (!hitEffect.ValueRO.isGoingBackHit)continue;
            var deltaTime = SystemAPI.Time.DeltaTime;
            hitEffect.ValueRW.goingBackHitTimer += deltaTime;
            if (hitEffect.ValueRO.goingBackHitTimer < hitEffect.ValueRO.goingBackHitDuration) {
                localTransform.ValueRW.Position += hitEffect.ValueRO.direction * hitEffect.ValueRO.goingBackHitSpeed* deltaTime;
                //physicsVelocity.ValueRW.ApplyImpulse(physicsMass.ValueRO,localTransform.ValueRW.Position,localTransform.ValueRW.Rotation, new float3(0f,1f,0f), hitEffect.ValueRO.direction);
            }else{
                hitEffect.ValueRW.goingBackHitTimer = 0f;
                hitEffect.ValueRW.animationHasPlayed = false;
                state.EntityManager.SetComponentEnabled<HitBackwardEffectComponent>(entity,false);
                state.EntityManager.SetComponentEnabled<EnemyActionComponent>(entity,true);
                
            }
            

        }
    }
    
}
