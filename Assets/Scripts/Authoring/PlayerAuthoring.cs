using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

public class PlayerAuthoring : MonoBehaviour
{
    public float initialSpeed = 5f;
    public GameObject targetPrefab;
    public bool isGrounded = true;

    public bool isRunning = false;
    public bool isWalking = false; 
    private class Baker : Baker<PlayerAuthoring>{
      public override void Bake(PlayerAuthoring authoring)
        {
          Entity entity = GetEntity(TransformUsageFlags.Dynamic);
          AddComponent(entity ,new PlayerMovementComponent{
            speed = authoring.initialSpeed,
            isGrounded = authoring.isGrounded,
            isRunning = authoring.isRunning,
            isWalking = authoring.isWalking,
            IsAttacking = false
          });
          AddComponent(entity ,new PlayerTargetPosition{
            isWaitingForClick = false,
            targetPrefab = GetEntity(authoring.targetPrefab, TransformUsageFlags.Dynamic),
          });
         
         
        }
    }
}