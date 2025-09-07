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
    public class K2Apparel
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<K2ExoticArmoryApparel.CustomAbility> CustomAbilityItems;

        public List<StatModifierInfo> StatRequirements;

        public List<K2ExoticArmoryApparel.Restrictions> restrictions;

        public K2ExoticArmoryApparel.MapCoordinate LocationCoordinates;

        public K2ExoticArmoryApparel.CustomVFX customVFX;

        private K2ExoticArmoryApparel.CustomApparel _instance;

        private ModManifest _manifest;

        public K2ExoticArmoryApparel.CustomApparel CustomInitialize(ModManifest manifest)
        {
            K2ExoticArmoryApparel.CustomApparel Apparel = ScriptableObject.CreateInstance<K2ExoticArmoryApparel.CustomApparel>();

            ANResourceSprite previewImage = manifest.SpriteResolver.ResolveAsResource(PreviewImage);

            _manifest = manifest;

            _instance = Apparel;

            if (StatModifierInfos != null)
            {
                Apparel.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(Apparel, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(Apparel, StatModifierInfos);
            }

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }

            Apparel.DisplaySpriteResource = previewImage;
            Apparel.LocationCoordinates = LocationCoordinates; 
            Apparel.Name = Name;
            Apparel.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            Apparel.Category = ItemCategory.Clothing;
            Apparel.Description = Description;
            Apparel.Price = Price;
            Apparel.StatRequirements = StatRequirements;
            Apparel.restrictions = restrictions;

            AddAbilitiesToApparel(Apparel);

            return Apparel;
        }

        private void AddAbilitiesToApparel(K2ExoticArmoryApparel.CustomApparel Apparel)
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
                    Apparel.AddAbility(ability, "Ability");
                }
            }
        }
    }
}
