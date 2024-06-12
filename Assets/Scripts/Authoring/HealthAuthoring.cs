using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HealthAuthoring : MonoBehaviour
{
    public float currentHealth = 100;
    public float maxHealth = 100; 
    private class Baker : Baker<HealthAuthoring>{
        public override void Bake(HealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HealthComponent{
                currentHealth = authoring.currentHealth,
                maxHealth = authoring.maxHealth
            });
        }
    }
}