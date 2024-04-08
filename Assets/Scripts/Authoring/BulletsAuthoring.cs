using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BulletsAuthoring : MonoBehaviour
{
   
    public float speed = 10f;




    // The Baker class is responsible for converting the authoring component into ECS components
    private class Baker : Baker<BulletsAuthoring>
    {
        public override void Bake(BulletsAuthoring authoring)
        {
            // Create the entity with the necessary components for the enemy
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Add the EnemyMovementComponent with values from the authoring component
            AddComponent(entity, new BulletsMouvementComponent
            {
                
                speed = authoring.speed,
                // Map other fields accordingly
            });
            AddComponent(entity,new DestroyTag{});
            SetComponentEnabled<DestroyTag>(entity, false);
            // You can also add other components here, such as components for rendering, physics, etc.
        }
    }
}
public struct BulletsMouvementComponent : IComponentData
{
    
    public float speed;
}
public struct BulletsDamageComponent : IComponentData
{
    
    public float damage;
}
