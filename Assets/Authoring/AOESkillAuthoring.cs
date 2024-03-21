using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class AOESkillAuthoring : MonoBehaviour
{   
    public float EffectRadius;
    public float3 targetClickPosition;
    public double LastUsedTime;
    public float Cooldown;
    public float Damage;
    public GameObject Prefab;
    private class Baker : Baker<AOESkillAuthoring>{
        public override void Bake(AOESkillAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity ,new AOESkill
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                EffectRadius = authoring.EffectRadius,
                targetClickPosition = authoring.transform.position,
                Damage = authoring.Damage,
                LastUsedTime = authoring.LastUsedTime,
                Cooldown = authoring.Cooldown
            });
        }
    }
}

public struct AOESkill : IComponentData
{
    public Entity Prefab;
    public float EffectRadius;
    public float3 targetClickPosition;
    public float Damage;
    public double LastUsedTime;
    public float Cooldown;
}