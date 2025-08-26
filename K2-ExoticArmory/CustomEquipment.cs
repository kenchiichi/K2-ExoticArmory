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
        public string Name;
        public string Sound;
        public string PreviewImage;
        public string Description;
        public string AbilityTooltip;
        public string AbilityID;
        public string AbilityName;
        public bool IsLocked = false;
        public int Price;
        public AttackVFXType WeaponAttackVFXType;
        public List<string> Slots;
        public List<StatModifierInfo> StatRequirements;
        public List<StatModifierInfo> StatModifierInfos;
        private CustomWeapon _instance;

        public void CustomInitialize(ModSpriteResolver modSpriteResolver)
        {
            CustomWeapon weapon = ScriptableObject.CreateInstance<CustomWeapon>();
            

            Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(AbilityID).Clone();
            ANResourceSprite aNResourceSprite = modSpriteResolver.ResolveAsResource(PreviewImage);

            //AudioClip sound = Resources.Load<AudioClip>(Sound);

            ability.Tooltip = AbilityTooltip;
            ability.DisplayName = AbilityName;
            ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
            weapon.DisplaySpriteResource = aNResourceSprite;
            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));

            weapon.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(modSpriteResolver)));
            //weapon.AttackVFXType = WeaponAttackVFXType;
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.AddAbility(ability, ability.RestraintID);
            weapon.StatModifierInfos = StatModifierInfos;
            weapon.Price = Price;
            weapon.IsLocked = IsLocked;

            _instance = weapon;
            typeof(Equipment)
                .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(weapon, weapon.StatModifierInfos);
            typeof(Equipment)
                .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(weapon, weapon.StatModifierInfos);

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
