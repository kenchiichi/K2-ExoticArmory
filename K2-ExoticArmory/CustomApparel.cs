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

    public class Restrictions
    {
        public string RequiredItemEquipped = "";
    }
}
