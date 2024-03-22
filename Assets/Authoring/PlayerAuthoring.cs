using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class PlayerAuthoring : MonoBehaviour
{
    public float initialSpeed = 5f;
    public GameObject targetPrefab;
    private class Baker : Baker<PlayerAuthoring>{
      public override void Bake(PlayerAuthoring authoring)
        {
          Entity entity = GetEntity(TransformUsageFlags.Dynamic);
          AddComponent(entity ,new PlayerMovementComponent{
              speed = authoring.initialSpeed
          });
          AddComponent(entity ,new PlayerTargetPosition{
               isWaitingForClick = false,
               targetPrefab = GetEntity(authoring.targetPrefab, TransformUsageFlags.Dynamic),
          });
        }
    }
}
public struct PlayerMovementComponent : IComponentData
{
    public float speed;
}

public struct PlayerTargetPosition : IComponentData
{
    public float3 targetClickPosition;

    public float3 targetMousePosition;
    public bool isWaitingForClick;
    public Entity targetPrefab;
}