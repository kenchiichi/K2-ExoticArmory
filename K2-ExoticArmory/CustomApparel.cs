using Asuna.CharManagement;
using System.Collections.Generic;

namespace K2ExoticArmoryApparel
{
    public class CustomApparel : Asuna.Items.Apparel
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

        public MapCoordinate LocationCoordinates;

        public CustomVFX customVFX;

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
