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

        public List<ApparelExtraSpriteInfo> ExtraSpriteInfos = ScriptableObject.CreateInstance<Apparel>().ExtraSpriteInfos;

        public List<MapCoordinate> LocationCoordinates;

        public List<CustomAbility> CustomAbilityItems;
    }

    public class MapCoordinate
    {
        public string MapName = "";

        public double xCoordinate;

        public double yCoordinate;
    };

}
