using ANToolkit.PlayMakerExtension;
using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using static DynamicBoneColliderBase;
namespace K2ExoticArmory
{
    public class CustomEquipment : ModEquipment
    {
        public string Description;
        public string AbilityTooltip;
        public string AbilityID;
        public string AbilityName;
        public string OggAudioClip;
        public string WeaponAttackVFXSprite;
        public int Price;
        public int BurstCount;
        public List<StatModifierInfo> StatModifierInfos;
        private CustomWeapon _instance;
        public AttackVFXType WeaponAttackVFXType;
        //public List<StatModifierInfo> StatRequirements;
        public CustomWeapon CustomInitialize(ModManifest manifest)
        {
            var weapon = ScriptableObject.CreateInstance<CustomWeapon>();
            //ANResourceSprite weaponAttackVFXSprite = new ANResourceSprite();
            if (AbilityID != null)
            {
                Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(AbilityID).Clone();
                ability.Tooltip = AbilityTooltip;
                ability.DisplayName = AbilityName;
                ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
                weapon.AddAbility(ability, ability.RestraintID);
            }
            if (StatModifierInfos != null)
            {
                //weapon.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
            }
            ANResourceSprite previewImage = manifest.SpriteResolver.ResolveAsResource(PreviewImage);
            weapon.DisplaySpriteResource = previewImage;

            

            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.Price = Price;
            weapon.AttackVFXType = WeaponAttackVFXType;

            //var musicDir = Path.Combine(manifest.ModPath, Sound);
            //var audioLoader = new WWW("file://" + musicDir);
            //var audioClip = audioLoader.GetAudioClip(false, false, AudioType.WAV);
            
            _instance = weapon;

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }
            else
            {
                Debug.LogError("Did not register Item \"" + Name + "\", because an item with the same name already exists.");
            }

            if (BurstCount > 0)
            {
                ANResourceSprite weaponAttackVFXSprite = manifest.SpriteResolver.ResolveAsResource(WeaponAttackVFXSprite);
                AddShootingVFXHook(weapon, (Sprite)weaponAttackVFXSprite, BurstCount, WeaponAttackVFXType.ToString());
            }
            return weapon;
        }

        private void AddShootingVFXHook(Weapon weapon, Sprite sprite, int burstCount, String WeaponAttackVFXType)
        {
            //Debug.Log("Adding ability hook to Turn Start");
            CombatTurnManager.OnTurnStart.AddListener(() =>
            {
                var jenna = Character.Get("Jenna");

                // Make sure that Jenna has this weapon equipped
                if (!jenna.EquippedItems.Contains(weapon)) return;
                var ability = jenna.GetAbilities().First(x => x.DisplayName == "Attack");

                // Create dummy gameobject to make duplicates of when shooting
                var vfxGameObject = new GameObject("VFX_Custom");
                // move out of level so that the prefab isn't visible
                vfxGameObject.transform.position = new Vector3(9999999999, 0);
                var vfxSpriteRenderer = vfxGameObject.AddComponent<SpriteRenderer>();

                vfxSpriteRenderer.sprite = Sprite.Create(sprite.texture,
                    new Rect(0, 0, sprite.texture.width, sprite.texture.height), new Vector2(0.5f, 0.5f),
                    800f);

                // get the logic from the Abl_Attack FIXME
                var cachedFSM = typeof(Ability)
                    .GetField("CachedFsm", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(ability) as Fsm;

                // edit the playmaker action that does the spawning of the visual effects
                var energyGunBurstState = cachedFSM.States.First(x => x.Name == WeaponAttackVFXType);

                var combatVFX = energyGunBurstState.Actions[0] as PMA_CombatVisualEffect;
                combatVFX.Prefab = vfxGameObject;
                combatVFX.AmountToSpawn = burstCount;
                
                var combatSFX = energyGunBurstState.Actions[1] as PMA_PlaySound;
                // This needs to be an AudioClip
                //combatSFX.ClipToPlay = sfxGameObject;
            });
        }
    }
}
