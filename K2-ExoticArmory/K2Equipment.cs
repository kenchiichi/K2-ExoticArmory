using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using Modding;
using System.Collections.Generic;

namespace K2Items
{
    public class K2Equipment
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<K2Items.CustomAbility> CustomAbilityItems;

        public List<StatModifierInfo> StatRequirements;

        public List<K2Items.Restrictions> restrictions;

        public K2Items.MapCoordinate LocationCoordinates;

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
    public class K2Weapon : Asuna.Items.Weapon
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

        public MapCoordinate LocationCoordinates;

        public CustomVFX customVFX;

        public int K2ItemCategory = 2;

        public int Price;
    }
    public class K2Apparel : Asuna.Items.Apparel
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

        public MapCoordinate LocationCoordinates;

        public int K2ItemCategory = 1;

        public int Price;
    }
    public class MapCoordinate
    {
        public string MapName = "";

        public double xCoordinate;

        public double yCoordinate;
    }
    public class CustomAbility
    {
        public string AbilityID = "";

        public string AbilityName = "";

        public string AbilityTooltip = "";

        public string DisplaySprite = "";

        public int AbilityCooldown = 0;

        public int AbilityEnergyCost = 0;
    }
    public class CustomVFX
    {
        public int BurstCount;

        public string WeaponAttackVFXType = "UnarmedMelee";

        public string wavAudioClip = "";

        public string WeaponAttackVFXSprite = "";
    }
    public class Restrictions
    {
        public string RequiredItemEquipped = "";
    }
}
