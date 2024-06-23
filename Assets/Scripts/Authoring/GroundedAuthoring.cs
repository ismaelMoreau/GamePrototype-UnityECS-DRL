using UnityEngine;
using Unity.Entities;

public class  GroundedAuthoring : MonoBehaviour
{
    public bool IsGrounded = false;
    private class Baker : Baker<GroundedAuthoring>
    {
        public override void Bake(GroundedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Grounded
            {
                IsGrounded = authoring.IsGrounded
            });
        }
    }
}