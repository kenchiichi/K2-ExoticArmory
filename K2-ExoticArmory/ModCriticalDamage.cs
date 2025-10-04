using System.Collections.Generic;
using ANToolkit.Save;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.NewCombat;
using UnityEngine;
namespace K2ExoticArmory
{
    public class ModCriticalDamage
    {
        public float CritMultiplier = (float)3;

        private static List<DamageType> critableDamageTypes = new List<DamageType>
        {
            DamageType.Physical,
            DamageType.Lust
        };
        public static Restraint IsEnabled => SaveManager.GetKey("__CriticalSystem_Enabled", new Restraint());

        public void OnDamageCalculated(DamageInfo info)
        {
            if (!critableDamageTypes.Contains(info.Type))
            {
                return;
            }

            if (!IsEnabled)
            {
                info.IsCritical = true;
            }

            Stat stat = info.Origin?.GetStat("stat_crit_chance");
            if (stat != null && info.IsCritical == true)
            {
                int value = stat.Value;
                if (value > 0 && Mathf.FloorToInt(Random.value * 100f) <= value)
                {
                    info.Amount = Mathf.RoundToInt((float)info.Amount * CritMultiplier);
                }
            }
        }
    }
}