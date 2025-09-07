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
    public class K2Weapon
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<StatModifierInfo> StatRequirements;

        public List<Restrictions> restrictions;

        public MapCoordinate LocationCoordinates;

        public CustomVFX customVFX;

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

            if (customVFX.BurstCount > 0)
            {
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                ANResourceSprite weaponAttackVFXSprite = manifest.SpriteResolver.ResolveAsResource(customVFX.WeaponAttackVFXSprite);
                AddShootingVFXHook(weapon, (Sprite)weaponAttackVFXSprite, customVFX.BurstCount);
            }
            else
            {
                weapon.AttackVFXType = AttackVFXType.UnarmedMelee;
            }

            weapon.DisplaySpriteResource = previewImage;
            weapon.LocationCoordinates = LocationCoordinates; 
            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.Price = Price;
            weapon.StatRequirements = StatRequirements;
            weapon.restrictions = restrictions;

            AddAbilitiesToWeapon(weapon);

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
                    ability.EnergyCost = item.AbilityEnergyCost;
                    ability.CooldownOnUse = item.AbilityCooldown;
                    ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
                    weapon.AddAbility(ability, "Ability");
                }
            }
        }

        private void AddShootingVFXHook(Weapon weapon, Sprite sprite, int burstCount)
        {
            CombatTurnManager.OnTurnStart.AddListener(() =>
            {
                if (!Character.Get("Jenna").EquippedItems.Contains(weapon)) return;
                var ability = Character.Get("Jenna").GetAbilities().First(x => x.DisplayName == "Attack");

                var vfxGameObject = new GameObject("VFX_Custom");
                vfxGameObject.transform.position = new Vector3(9999999999, 0);
                var vfxSpriteRenderer = vfxGameObject.AddComponent<SpriteRenderer>();

                vfxSpriteRenderer.sprite = Sprite.Create(sprite.texture, new Rect(0, 0, sprite.texture.width, sprite.texture.height), new Vector2(0.5f, 0.5f), 800f);

                var cachedFSM = typeof(Ability)
                    .GetField("CachedFsm", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(ability) as Fsm;

                var energyGunBurstState = cachedFSM.States.First(x => x.Name == "EnergyGunBurst");

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
