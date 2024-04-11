using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{
    public float health = 100f;
    public float speed = 2f;


    public float numberOfSteps = 0;

    
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

            AddComponent(entity,new EnemyGridPositionComponent{
                isDoingAction = false,
                numberOfSteps = authoring.numberOfSteps
            });
            SetComponentEnabled<EnemyGridPositionComponent>(entity, false);
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
public struct EnemyGridPositionComponent : IComponentData, IEnableableComponent
{
    public float gridFlatenPosition;
    public bool isDoingAction;

    public float chosenAction;
    public float numberOfSteps;
}

public struct DestroyTag : IComponentData, IEnableableComponent{ 
  
}