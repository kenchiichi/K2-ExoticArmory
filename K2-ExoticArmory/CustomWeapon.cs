using ANToolkit.ResourceManagement;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Items;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace K2ExoticArmory
{
    public class CustomWeapon : Asuna.Items.Apparel
    {
        public bool IsLocked;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;
        public static CustomWeapon CreateWeapon(CustomEquipment customEquipment)
        {
            Item item = CreateItem(customEquipment.Name);
            if (item is CustomWeapon)
            {
                typeof(Equipment)
                   .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                   .SetValue(item, (item as CustomWeapon).StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(item, (item as CustomWeapon).StatModifierInfos);
                return (item as CustomWeapon);
            }
            return null;
        }

        private static Item CreateItem(string name)
        {
            if (All.TryGetValue(name.ToLower(), out var value))
            {
                if (value == null)
                {
                    return null;
                }
            }
            return value;
        }
    }
}
