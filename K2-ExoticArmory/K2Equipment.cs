using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using Modding;
using System.Collections.Generic;

namespace K2ExoticArmory
{
    public class K2Equipment
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<K2CustomEquipment.CustomAbility> CustomAbilityItems;

        public List<StatModifierInfo> StatRequirements;

        public List<K2CustomEquipment.Restrictions> restrictions;

        public K2CustomEquipment.MapCoordinate LocationCoordinates;

        public void AddAbilitiesToEquipment(Equipment equipment, ModManifest manifest)
        {
            if (CustomAbilityItems != null)
            {
                foreach (var item in CustomAbilityItems)
                {
                    Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(item.AbilityID).Clone();
                    ability.name = item.AbilityName.Replace(' ', '_');
                    ability.Tooltip = item.AbilityTooltip;
                    ability.DisplayName = item.AbilityName;
                    ability.DisplaySprite = manifest.SpriteResolver.ResolveAsResource(item.DisplaySprite);
                    ability.EnergyCost = item.AbilityEnergyCost;
                    ability.CooldownOnUse = item.AbilityCooldown;
                    ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
                    equipment.AddAbility(ability, "Ability");
                }
            }
        }
    }
}
