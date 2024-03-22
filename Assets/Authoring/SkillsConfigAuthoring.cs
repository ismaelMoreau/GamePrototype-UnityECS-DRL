using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SkillConfigAuthoring : MonoBehaviour
{   

    
    public float EffectRadius;
    public double LastUsedTime;
    public float Cooldown;
    public float Damage;
    public GameObject PrefabAoe;
    private class Baker : Baker<SkillConfigAuthoring>{
        public override void Bake(SkillConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity ,new SkillsConfig
            {
                PrefabAoe = GetEntity(authoring.PrefabAoe, TransformUsageFlags.Dynamic),
                EffectRadius = authoring.EffectRadius,
                Damage = authoring.Damage,
                LastUsedTime = authoring.LastUsedTime,
                Cooldown = authoring.Cooldown
            });
        }
    }
}

public struct SkillsConfig: IComponentData
{
    public Entity PrefabAoe;
    public float EffectRadius;
    public float Damage;
    public double LastUsedTime;
    public float Cooldown;

}