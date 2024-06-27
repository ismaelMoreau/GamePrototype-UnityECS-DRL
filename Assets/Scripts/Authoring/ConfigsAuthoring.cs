using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Random= Unity.Mathematics.Random;
using UnityEditor.Rendering;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms.Impl;
using Google.Protobuf.WellKnownTypes;

public class ConfigsAuthoring : MonoBehaviour
{   

    
    public float EffectRadius;
    public double LastUsedTime;
    public float Cooldown;
    public float Damage;
    public GameObject PrefabAoe;

    // public int height= 20;
    // public int width= 20;

    // public float cellSize= 1.0f;

    //public float sartingEpsilon = 0.5f;
    // public float alpha = 0.1f;
    // public float gamma = 0.8f;

    //public bool isInitQlearning = false;
    public float triggerCooldownTimer = 0f;
    public float hitTriggerCooldownDuration =1.5f;

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
            // AddComponent(entity ,new ConfigQlearnGrid{
            //     height= authoring.height,
            //     width = authoring.width,
            //     cellSize = authoring.cellSize
            // });
            // AddComponent(entity ,new ConfigQlearn{
            //     sartingEpsilon= authoring.sartingEpsilon,
            //     alpha = authoring.alpha,
            //     gamma = authoring.gamma,
            //     random = new Random(123),
            //     isInitQlearning = authoring.isInitQlearning
            // });
            AddComponent(entity, new HitTriggerConfigComponent{
                triggerCooldownTimer = authoring.triggerCooldownTimer,
                hitTriggerCooldownDuration = authoring.hitTriggerCooldownDuration
            });
            AddComponent(entity, new ScoreComponent{Value=0});
            
        }
    }
}


