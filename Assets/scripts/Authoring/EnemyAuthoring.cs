using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoring : MonoBehaviour
{
    public float health = 100f;
    public float speed = 2f;

    public bool isDead= false;
    // Add other fields as necessary

    // The Baker class is responsible for converting the authoring component into ECS components
    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            // Create the entity with the necessary components for the enemy
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Add the EnemyComponent with values from the authoring component
            AddComponent(entity, new EnemyComponent
            {
                health = authoring.health,
                speed = authoring.speed
                // Map other fields accordingly
            });
        
            // You can also add other components here, such as components for rendering, physics, etc.
        }
    }
}
public struct EnemyComponent : IComponentData
{
    public float health;
    public float speed;
    // Add other enemy attributes here
}
public struct DestroyTag : IComponentData { 
  
}