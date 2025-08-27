using ANToolkit.ResourceManagement;
using ANToolkit.UI;
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

        private List<StatModifierInfo> dynamicStatModifiers;

        public List<ApparelExtraSpriteInfo> ExtraSpriteInfos = ScriptableObject.CreateInstance<Apparel>().ExtraSpriteInfos;

        public List<MapCoordinate> LocationCoordinates;
    }

    public class MapCoordinate
    {
        public string MapName = "";

        public double xCoordinate;

        public double yCoordinate;
    };

}
