using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Random= Unity.Mathematics.Random;

public class ConfigsAuthoring : MonoBehaviour
{   

    
    public float EffectRadius;
    public double LastUsedTime;
    public float Cooldown;
    public float Damage;
    public GameObject PrefabAoe;

    public int height= 20;
    public int width= 20;

    public float cellSize= 1.0f;

    public float sartingEpsilon = 1;
    public float alpha = 0.1f;
    public float gamma = 0.8f;

    public bool isInitQlearning = false;

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
                sartingEpsilon= authoring.sartingEpsilon,
                alpha = authoring.alpha,
                gamma = authoring.gamma,
                random = new Random(123),
                isInitQlearning = authoring.isInitQlearning
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
    public int height;
    public int width;

    public float cellSize;
}
public struct ConfigQlearn : IComponentData
{
    public float sartingEpsilon;
    public float alpha;
    public float gamma;

    public Random random;

    public bool isInitQlearning;

}