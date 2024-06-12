
using UnityEngine;
using Unity.Entities;
public class TargetAuthoring : MonoBehaviour

    // Start is called before the first frame update
    {
        public bool isJumpTarget = false;
        public bool isAoeTarget = false;
            private class Baker : Baker<TargetAuthoring>{
                public override void Bake(TargetAuthoring authoring)
                {
                    var entity = GetEntity(TransformUsageFlags.None);
                    AddComponent(entity ,new Target
                    {        
                        isJumpTarget= authoring.isJumpTarget,
                        isAoeTarget = authoring.isAoeTarget            
                    });
                }
            }
    }