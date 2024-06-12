using System.Threading;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct DetectTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EnemyActionComponent>();
        state.RequireForUpdate<PlayerMovementComponent>();
        state.RequireForUpdate<HitTriggerConfigComponent>();
        
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<HitTriggerConfigComponent>(); 
        var detectTriggerJob = new DetectTriggerJob
        {
            
            HitBufferLookup = SystemAPI.GetBufferLookup<HitBufferElement>(),
            enemyHitPointsLookup = SystemAPI.GetComponentLookup<EnemyActionComponent>(),
            playerMouvement = SystemAPI.GetComponentLookup<PlayerMovementComponent>(),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            cooldownTimer =  config.triggerCooldownTimer,
            cooldownDuration = config.hitTriggerCooldownDuration

        };
        
        var simSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = detectTriggerJob.Schedule(simSingleton, state.Dependency);
    }
}

public struct DetectTriggerJob : ITriggerEventsJob
{
    public BufferLookup<HitBufferElement> HitBufferLookup;
    public float cooldownTimer ;
    public float cooldownDuration ;
    [ReadOnly] public ComponentLookup<EnemyActionComponent> enemyHitPointsLookup;
    [ReadOnly] public ComponentLookup<PlayerMovementComponent> playerMouvement;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity triggerEntity;
        Entity hitEntity;

        // Figure out which entity is the trigger and which entity is the hit entity
        // If there are not exactly 1 of each, this is not a valid trigger event for this case, return from job
        if (HitBufferLookup.HasBuffer(triggerEvent.EntityA) && enemyHitPointsLookup.HasComponent(triggerEvent.EntityB))
        {
            triggerEntity = triggerEvent.EntityA;
            hitEntity = triggerEvent.EntityB;
        }
        else if (HitBufferLookup.HasBuffer(triggerEvent.EntityB) &&
                    enemyHitPointsLookup.HasComponent(triggerEvent.EntityA))
        {
            triggerEntity = triggerEvent.EntityB;
            hitEntity = triggerEvent.EntityA;
        }
        else if(HitBufferLookup.HasBuffer(triggerEvent.EntityA) && playerMouvement.HasComponent(triggerEvent.EntityB)){
            triggerEntity = triggerEvent.EntityA;
            hitEntity = triggerEvent.EntityB;
        }
        else if(HitBufferLookup.HasBuffer(triggerEvent.EntityB) && playerMouvement.HasComponent(triggerEvent.EntityA)){
            triggerEntity = triggerEvent.EntityB;
            hitEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        //Determine if the hit entity is already added to the trigger entity's hit buffer 
        var hitBuffer = HitBufferLookup[triggerEntity];
        foreach (var hit in hitBuffer)
        {
            
            if (hit.HitEntity == hitEntity) return;
        }
        
        // Need to estimate position and normal as TriggerEvent does not have these details unlike CollisionEvent
        var triggerEntityPosition = TransformLookup[triggerEntity].Position;
        var hitEntityPosition = TransformLookup[hitEntity].Position;

        var hitPosition = math.lerp(triggerEntityPosition, hitEntityPosition, 0.5f);
        var hitNormal = math.normalizesafe(hitEntityPosition - triggerEntityPosition);

        var newHitElement = new HitBufferElement
        {
            IsHandled = false,
            HitEntity = hitEntity,
            triggerEntity = triggerEntity,
            Position = hitPosition,
            Normal = hitNormal,
            cooldownTimer = cooldownTimer,
            cooldownDuration = cooldownDuration 
        };

        HitBufferLookup[triggerEntity].Add(newHitElement);
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial class HitTriggerSystemGroup : ComponentSystemGroup
{
    
}
[UpdateInGroup(typeof(HitTriggerSystemGroup), OrderLast = true)]
public partial struct HandleHitBufferSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var hitBufferLookup = SystemAPI.GetBufferLookup<HitBufferElement>();
        var triggerEntities = SystemAPI.QueryBuilder().WithAll<HitBufferElement>().Build().ToEntityArray(state.WorldUpdateAllocator);
        
        foreach (var triggerEntity in triggerEntities)
        {
            var hitBuffer = hitBufferLookup[triggerEntity];
            for (var i = 0; i < hitBuffer.Length; i++)
            {
                
                hitBuffer.ElementAt(i).IsHandled = true;
                hitBuffer.ElementAt(i).cooldownTimer += SystemAPI.Time.DeltaTime;
                if(hitBuffer.ElementAt(i).cooldownTimer >= hitBuffer.ElementAt(i).cooldownDuration){
                    hitBuffer.RemoveAt(i);
                }
            }
            
        }
    }
}  

 

