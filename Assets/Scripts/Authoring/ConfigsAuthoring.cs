using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ConfigsAuthoring : MonoBehaviour
{   

    
    public float EffectRadius;
    public double LastUsedTime;
    public float Cooldown;
    public float Damage;
    public GameObject PrefabAoe;

    public float height= 20;
    public float width= 20;

    public float cellSize= 1.0f;

    public float epsilon = 1;
    public float alpha = 0.1f;
    public float gamma = 0.8f;
    private class Baker : Baker<ConfigsAuthoring>{
        public override void Bake(ConfigsAuthoring authoring)
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
            AddComponentObject(entity ,new ConfigManaged{});
            AddComponent(entity ,new ConfigQlearnGrid{
                height= authoring.height,
                width = authoring.width,
                cellSize = authoring.cellSize
            });
            AddComponent(entity ,new ConfigQlearn{
                epsilon= authoring.epsilon,
                alpha = authoring.alpha,
                gamma = authoring.gamma
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
public class ConfigManaged : IComponentData
{
    public UIController UIController;
}
public struct ConfigQlearnGrid : IComponentData
{
    public float height;
    public float width;

    public float cellSize;
}
public struct ConfigQlearn : IComponentData
{
    public float epsilon;
    public float alpha;
    public float gamma;

}