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
    }

    public void OnUpdate(ref SystemState state)
    {
        var detectTriggerJob = new DetectTriggerJob
        {
            HitBufferLookup = SystemAPI.GetBufferLookup<HitBufferElement>(),
            enemyHitPointsLookup = SystemAPI.GetComponentLookup<EnemyHealthComponent>(),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
        };

        var simSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = detectTriggerJob.Schedule(simSingleton, state.Dependency);
    }
}

public struct DetectTriggerJob : ITriggerEventsJob
{
    public BufferLookup<HitBufferElement> HitBufferLookup;
    [ReadOnly] public ComponentLookup<EnemyHealthComponent> enemyHitPointsLookup;
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
        else
        {
            return;
        }

        // Determine if the hit entity is already added to the trigger entity's hit buffer 
        // var hitBuffer = HitBufferLookup[triggerEntity];
        // foreach (var hit in hitBuffer)
        // {
        //     if (hit.HitEntity == hitEntity) return;
        // }
        
        // Need to estimate position and normal as TriggerEvent does not have these details unlike CollisionEvent
        var triggerEntityPosition = TransformLookup[triggerEntity].Position;
        var hitEntityPosition = TransformLookup[hitEntity].Position;

        var hitPosition = math.lerp(triggerEntityPosition, hitEntityPosition, 0.5f);
        var hitNormal = math.normalizesafe(hitEntityPosition - triggerEntityPosition);

        var newHitElement = new HitBufferElement
        {
            IsHandled = false,
            HitEntity = hitEntity,
            Position = hitPosition,
            Normal = hitNormal
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
            }
        }
    }
}  

 

