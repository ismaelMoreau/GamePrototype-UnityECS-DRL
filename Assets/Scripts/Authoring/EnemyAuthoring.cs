using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{
    public float currentEnnemyHealth = 100;
    public float MaxEnnemyHealth = 100;
    public float speed = 2f;


    public int numberOfSteps = 0;
    public float earnReward= 0;
    public float actionTimer= 0;
    public float actionDuration= 0;
    public float epsilon= 0.3f;
    public Vector3 HealthBarUIOffset;
    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyMovementComponent
            {
                speed = authoring.speed
            });
            AddComponent(entity,new DestroyTag{});
            SetComponentEnabled<DestroyTag>(entity, false);
            AddComponent(entity,new EnemyActionComponent{
                isDoingAction = false,
                numberOfSteps = authoring.numberOfSteps,
                IsReadyToUpdateQtable = false
            });
            SetComponentEnabled<EnemyActionComponent>(entity, false);
            AddComponent(entity,new EnemyRewardComponent{
                earnReward= authoring.earnReward
            });
            AddComponent(entity,new EnemyActionTimerComponent{
                actionTimer = authoring.actionTimer,
                actionDuration = authoring.actionDuration
            });
            AddComponent(entity,new EnemyEpsilonComponent{
                epsilon= authoring.epsilon
            });
            AddComponent(entity, new EnemyHealthComponent{
                currentEnnemyHealth = authoring.currentEnnemyHealth,
                MaxEnnemyHealth = authoring.MaxEnnemyHealth
            });
            AddComponent(entity, new HealthBarOffset { Value = authoring.HealthBarUIOffset });
            AddComponent<UpdateHealthBarUI>(entity);
            SetComponentEnabled<UpdateHealthBarUI>(entity, false);
        }
    }
}
public struct EnemyMovementComponent : IComponentData
{
    public float speed;
}
public struct EnemyHealthComponent : IComponentData
{
    public float currentEnnemyHealth;
    public float MaxEnnemyHealth;
}
public struct EnemyActionComponent : IComponentData, IEnableableComponent
{
    public int gridFlatenPosition;
    
    public bool IsReadyToUpdateQtable;
    public bool isDoingAction;

    public int chosenAction;
    public int numberOfSteps;

    public float chosenActionQvalue;

    public int chosenNextAction;
    public float chosenNextActionQvalue;

    public int nextActionGridFlatenPosition;

    
}
public struct EnemyRewardComponent : IComponentData, IEnableableComponent
{
    public float earnReward;
}
public struct EnemyActionTimerComponent : IComponentData 
{
    
    public float actionTimer;
    public float actionDuration;
}
public struct EnemyEpsilonComponent : IComponentData 
{
    public float epsilon;
}
public struct DestroyTag : IComponentData, IEnableableComponent{ 
  
}
// Managed Cleanup Component used to reference the world-space Unity UI health bar slider associated with an entity
public class HealthBarUI : ICleanupComponentData
{
    public GameObject Value;
}
public struct HealthBarOffset : IComponentData
{
    public float3 Value;
}

public struct UpdateHealthBarUI : IComponentData, IEnableableComponent {}