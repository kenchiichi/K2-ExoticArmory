using Asuna.CharManagement;
using Asuna.Items;
using System.Collections.Generic;
using UnityEngine;

namespace K2ExoticArmory
{
    public class CustomWeapon : Asuna.Items.Weapon
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<MapCoordinate> LocationCoordinates;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

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
