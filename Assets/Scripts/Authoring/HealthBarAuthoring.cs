using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HealthBarAuthoring : MonoBehaviour

    // Start is called before the first frame update
    {
     public Vector3 HealthBarUIOffset;
            private class Baker : Baker<HealthBarAuthoring>{
                public override void Bake(HealthBarAuthoring authoring)
                {
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, new HealthBarOffset { Value = authoring.HealthBarUIOffset });
                    AddComponent<UpdateHealthBarUI>(entity);
                    SetComponentEnabled<UpdateHealthBarUI>(entity, false);
                }
            }
    }