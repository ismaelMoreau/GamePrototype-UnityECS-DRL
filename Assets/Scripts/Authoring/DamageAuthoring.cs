using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class DamageAuthoring : MonoBehaviour

    
    {
    public float currentDamage = 0f;
    public float damageShowTimer = 0f;
    public float damageShowDuration = 2f;
     public Vector3 damageUIOffset;
            private class Baker : Baker<DamageAuthoring>{
                public override void Bake(DamageAuthoring authoring)
                {
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, new damageUIOffset { Value = authoring.damageUIOffset });
                    AddComponent<DamageShowUpdate>(entity);
                    SetComponentEnabled<DamageShowUpdate>(entity, false);
                    AddComponent(entity, new DamageComponent
                    {
                        currentDamage = authoring.currentDamage,
                        damageShowTimer = authoring.damageShowTimer,
                        damageShowDuration = authoring.damageShowDuration
                    });
                }
            }
    }