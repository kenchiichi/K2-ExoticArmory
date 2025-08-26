using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.CharManagement.CharacterCreator;
using Asuna.Items;
using Asuna.NewCombat;
using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;


namespace K2ExoticArmory
{
    public class CustomEquipment : ModEquipment
    {
        public string Description;
        public string AbilityTooltip;
        public string AbilityID;
        public string AbilityName;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;
        private CustomWeapon _instance;
        //public bool IsLocked = false;
        //public AttackVFXType WeaponAttackVFXType;
        //public List<StatModifierInfo> StatRequirements;
        //public string Sound;
        public void CustomInitialize(ModSpriteResolver modSpriteResolver)
        {
            CustomWeapon weapon = ScriptableObject.CreateInstance<CustomWeapon>();
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
                weapon.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
            }

            ANResourceSprite aNResourceSprite = modSpriteResolver.ResolveAsResource(PreviewImage);
            weapon.DisplaySpriteResource = aNResourceSprite;

            weapon.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(modSpriteResolver)));

            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.Price = Price;
            //AudioClip sound = Resources.Load<AudioClip>(Sound);
            //weapon.AttackVFXType = WeaponAttackVFXType;
            //weapon.IsLocked = IsLocked;

            _instance = weapon;
            
            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }
            else
            {
                Debug.LogError("Did not register Item \"" + Name + "\", because an item with the same name already exists.");
            }
        }
    }

}
