using Asuna.CharManagement;
using Asuna.Items;
using System.Collections.Generic;
using System.Reflection;

namespace K2ExoticArmory
{
    public class CustomWeapon : Asuna.Items.Weapon
    {
        public bool IsLocked;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;
        public static CustomWeapon CreateWeapon(string name)
        {
            Item item = Create(name);
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
    }
}
