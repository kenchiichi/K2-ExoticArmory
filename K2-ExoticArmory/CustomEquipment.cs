using ANToolkit.PlayMakerExtension;
using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using HutongGames.PlayMaker;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class CustomEquipment
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<MapCoordinate> LocationCoordinates;

        public List<CustomAbility> CustomAbilityItems;

        public List<CustomVFX> customVFX;

        public List<StatModifierInfo> StatRequirements;

        public List<Restrictions> restrictions;

        private CustomWeapon _instance;

        private ModManifest _manifest;

        public CustomWeapon CustomInitialize(ModManifest manifest)
        {
            CustomWeapon weapon = ScriptableObject.CreateInstance<CustomWeapon>();

            ANResourceSprite previewImage = manifest.SpriteResolver.ResolveAsResource(PreviewImage);

            _manifest = manifest;

            _instance = weapon;

            if (StatModifierInfos != null)
            {
                weapon.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
            }

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }

            if (customVFX[0].BurstCount > 0)
            {
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                ANResourceSprite weaponAttackVFXSprite = manifest.SpriteResolver.ResolveAsResource(customVFX[0].WeaponAttackVFXSprite);
                AddShootingVFXHook(weapon, (Sprite)weaponAttackVFXSprite, customVFX[0].BurstCount);
            }
            else
            {
                weapon.AttackVFXType = AttackVFXType.UnarmedMelee;
            }

            weapon.DisplaySpriteResource = previewImage;
            weapon.LocationCoordinates = LocationCoordinates; weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.Price = Price;
            weapon.StatRequirements = StatRequirements;

            AddAbilitiesToWeapon(weapon);
            //var musicDir = Path.Combine(manifest.ModPath, Sound);
            //var audioLoader = new WWW("file://" + musicDir);
            //var audioClip = audioLoader.GetAudioClip(false, false, AudioType.WAV);

            return weapon;
        }

        private void AddAbilitiesToWeapon(CustomWeapon weapon)
        {
            if (CustomAbilityItems != null)
            {
                foreach (var item in CustomAbilityItems)
                {
                    Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(item.AbilityID).Clone();
                    ability.name = item.AbilityName.Replace(' ', '_');
                    ability.Tooltip = item.AbilityTooltip;
                    ability.DisplayName = item.AbilityName;
                    ability.DisplaySprite = _manifest.SpriteResolver.ResolveAsResource(item.DisplaySprite);
                    ability.EnergyCost = 1;
                    ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
                    weapon.AddAbility(ability, "Ability");
                }
            }
        }

        private void AddShootingVFXHook(Weapon weapon, Sprite sprite, int burstCount)
        {
            string WeaponAttackVFXType = "UnarmedMelee";
            if (burstCount > 0) 
            {
                WeaponAttackVFXType = "EnergyGunBurst";
            }
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
