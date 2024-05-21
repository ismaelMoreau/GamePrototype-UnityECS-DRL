using Unity.Entities;
using UnityEngine;


public class DamageGameObjectPrefabsAuthoring : MonoBehaviour
    {
        public GameObject HealthBarUIPrefab;

        public class DamageGameObjectPrefabsBaker : Baker<DamageGameObjectPrefabsAuthoring>
        {
            public override void Bake(DamageGameObjectPrefabsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new DamageGameObjectPrefabs
                {
                    HealthBarUIPrefab = authoring.HealthBarUIPrefab
                });
            }
        }
    }

public class DamageGameObjectPrefabs : IComponentData
    {
        public GameObject HealthBarUIPrefab;
    }