
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
 
 public partial struct DamageUISystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            // Initialize Damage bars for new entities that need them
            foreach (var (DamageComponent, transform, DamageBarOffset, entity) in SystemAPI.Query<DamageComponent, 
                         LocalTransform, damageUIOffset>().WithNone<DamageUI>().WithEntityAccess())
            {
                var DamageBarPrefab = SystemAPI.ManagedAPI.GetSingleton<DamageGameObjectPrefabs2>().damageUIPrefab;
                var spawnPosition = transform.Position + DamageBarOffset.Value;
                var newDamageBar = Object.Instantiate(DamageBarPrefab, spawnPosition, quaternion.identity);
                newDamageBar.GetComponent<Canvas>().gameObject.SetActive(false);
                ecb.AddComponent(entity, new DamageUI { Value = newDamageBar });
                
            }

            // Cleanup Damage bars for destroyed entities
            foreach (var (DamageBarUI, entity) in SystemAPI.Query<DamageUI>().WithNone<damageUIOffset>()
                         .WithEntityAccess())
            {
                Object.Destroy(DamageBarUI.Value);
                ecb.RemoveComponent<DamageUI>(entity);
            }

            ecb.Playback(state.EntityManager);

            // Update the transform position of all Damage bars to be above each entity
            foreach (var (DamageBarUI, transform, DamageBarOffset) in SystemAPI.Query<DamageUI, LocalTransform, 
                         damageUIOffset>().WithNone<DamageShowUpdate>())
            {
                DamageBarUI.Value.transform.position = transform.Position + DamageBarOffset.Value;
                DamageBarUI.Value.transform.LookAt(Camera.main.transform);
                DamageBarUI.Value.transform.Rotate(0, 180, 0);
            }
            
            // Update the values of the Damage bar for entities that need it updated
            foreach (var (DamageBarUI, DamageComponent, entity) in SystemAPI.Query<DamageUI, RefRW<DamageComponent>>().WithAll<DamageShowUpdate>().WithEntityAccess())
            {
                var Damage = DamageBarUI.Value.GetComponent<Canvas>();
                
                Damage.gameObject.SetActive(true);
                var text = Damage.GetComponentInChildren<TMP_Text>();
                
                
                DamageComponent.ValueRW.damageShowTimer += Time.deltaTime;

                text.text = DamageComponent.ValueRO.currentDamage.ToString();
                float remainingTime = DamageComponent.ValueRO.damageShowDuration - DamageComponent.ValueRO.damageShowTimer;
                float ratio = Mathf.Clamp01(remainingTime / DamageComponent.ValueRO.damageShowDuration);

                // Set color with fading alpha
                Color coolColor = new Color(0.2f, 0.6f, 1.0f, ratio); 
                text.color = coolColor;

                // Calculate and set scale based on remaining time
                float scale = ratio * 2.0f; 
                text.transform.localScale = new Vector3(scale, scale, scale);

                // Calculate and set position based on remaining time
                float moveDistance = 1.0f; 
                text.transform.localPosition += new Vector3(0, moveDistance * Time.deltaTime, 0);

                text.color = coolColor;
                if (DamageComponent.ValueRO.damageShowTimer >= DamageComponent.ValueRO.damageShowDuration)
                {
                    DamageComponent.ValueRW.damageShowTimer = 0;
                    Damage.gameObject.SetActive(false);
                    state.EntityManager.SetComponentEnabled<DamageShowUpdate>(entity, false);
                    DamageComponent.ValueRW.currentDamage = 0;
                }
            }
            
            ecb.Dispose();
        }
    }
