using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{
    public float health = 100f;
    public float speed = 2f;


    public int numberOfSteps = 0;
    public float earnReward= 0;
    public float actionTimer= 0;
    public float actionDuration= 0;
    public float epsilon= 0.3f;
    
    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            
            AddComponent(entity, new EnemyMovementComponent
            {
                
                speed = authoring.speed,
               
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
            
        }
    }
}
public struct EnemyMovementComponent : IComponentData
{
    public float speed;
}
public struct EnemyHealthComponent : IComponentData
{
    public float health;
}
public struct EnemyActionComponent : IComponentData, IEnableableComponent
{
    public int gridFlatenPosition;
    
    public bool IsReadyToUpdateQtable;//not use

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