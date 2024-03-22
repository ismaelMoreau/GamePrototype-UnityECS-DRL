
using UnityEngine;
using Unity.Entities;
public class TargetAuthoring : MonoBehaviour

    // Start is called before the first frame update
    {
            private class Baker : Baker<TargetAuthoring>{
                public override void Bake(TargetAuthoring authoring)
                {
                    var entity = GetEntity(TransformUsageFlags.None);
                    AddComponent(entity ,new Target
                    {                    
                    });
                }
            }
    }

    public struct Target: IComponentData
    {

    }
