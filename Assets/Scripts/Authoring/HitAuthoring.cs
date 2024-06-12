using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HitAuthoring : MonoBehaviour
{
    public float goingBackHitSpeed = 7f;
    public float goingBackHitTimer = 0f;
    public float goingBackHitDuration = 2f;
    public bool haveAbackwardEffect;
    // The Baker class is responsible for converting the authoring component into ECS components
    private class Baker : Baker<HitAuthoring>
    {
        public override void Bake(HitAuthoring authoring)
        {
            // Create the entity with the necessary components for the enemy
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<HitBufferElement>(entity);
            AddComponent(entity, new HitBackwardEffectComponent{
               
                goingBackHitSpeed = authoring.goingBackHitSpeed,
                goingBackHitTimer = authoring.goingBackHitTimer,
                goingBackHitDuration = authoring.goingBackHitDuration,
            });
            SetComponentEnabled<HitBackwardEffectComponent>(entity,false);
            AddComponent(entity, new BackwardEffect{
                haveAbackwardEffect = authoring.haveAbackwardEffect
            });
        }
    }
}