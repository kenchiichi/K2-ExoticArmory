using Asuna.CharManagement;
using Asuna.Items;
using System.Collections.Generic;
using UnityEngine;

namespace K2ExoticArmory
{
    public class CustomWeapon : Asuna.Items.Weapon
    {
        public int Price;

        public List<StatModifierInfo> StatModifierInfos;

        public List<MapCoordinate> LocationCoordinates;

        public List<CustomAbility> CustomAbilityItems;
        
        public CustomVFX customVFX;
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

        public string OggAudioClip = "";

        public string WeaponAttackVFXSprite = "";
    }

}
