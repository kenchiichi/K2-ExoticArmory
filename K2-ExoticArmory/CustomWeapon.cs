using ANToolkit.ResourceManagement;
using ANToolkit.UI;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Items;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using ANToolkit.ScriptableManagement;
using Asuna.NewCombat;
using UnityEngine.Serialization;

namespace K2ExoticArmory
{
    public class CustomWeapon : Asuna.Items.Weapon
    {
        public bool IsLocked;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;
        private List<StatModifierInfo> dynamicStatModifiers;
        public List<ApparelExtraSpriteInfo> ExtraSpriteInfos = ScriptableObject.CreateInstance<Apparel>().ExtraSpriteInfos;

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


        protected override void Initialize()
        {
            foreach (ApparelExtraSpriteInfo extraSpriteInfo in ExtraSpriteInfos)
            {
                extraSpriteInfo.DisplaySpritesWhenNoSlot.ForEach(delegate (VisualLayerInfo visual)
                {
                    AddVisualLayer(visual);
                });
                extraSpriteInfo.DisplaySpritesWhenSlot.ForEach(delegate (VisualLayerInfo visual)
                {
                    AddVisualLayer(visual);
                });
                extraSpriteInfo.DisplayDurabilitySpritesWhenNoSlot.ForEach(delegate (DurabilityLayerInfo visual)
                {
                    AddVisualLayer(visual);
                });
                extraSpriteInfo.DisplayDurabilitySpritesWhenSlot.ForEach(delegate (DurabilityLayerInfo visual)
                {
                    AddVisualLayer(visual);
                });
            }

            base.Initialize();
            UseText = "Equip";
            Equipment.OnDurabilitySet.AddListener(AnyDurabilitySet);
        }

        public override void Equipped(Character User)
        {
            base.Equipped(User);
            UseText = "Unequip";
            User.EquippedItems.OnSlotChanged.AddListener(OnSlotChanged);
            OnSlotChanged(null);

            CheckExtraSprites();
        }

        public virtual void UnEquipped(Character User)
        {
            OnUnEquipped.Invoke(User);
            OnEquipToggled.Invoke(arg0: false);
            RemoveStats();
            UseText = "Equip";
            if (cachedFsm != null)
            {
                cachedFsm.GetFsmObject("User").Value = User;
                cachedFsm.GetFsmObject("Owner").Value = User;
                cachedFsm.ProcessEventByName("OnUnEquipped");
            }
        }

        private void RemoveStats()
        {
            if (!base.Owner)
            {
                return;
            }

            foreach (StatModifierInfo dynamicStatModifier in dynamicStatModifiers)
            {
                Stat stat = base.Owner.GetStat(dynamicStatModifier.StatName);
                if (stat != null)
                {
                    dynamicStatModifier.ModifierID = "Equipment_" + base.name;
                    stat.RemoveModifier(dynamicStatModifier);
                }
            }
        }

        public void OnSlotChanged(SlotEventInfo info)
        {
            CheckExtraSprites();
        }

        private void AnyDurabilitySet(DurabilityEventInfo info)
        {
            if ((bool)info.Equipment && info.Equipment.Owner == base.Owner)
            {
                CheckExtraSprites();
            }
        }

        private void CheckExtraSprites()
        {
            if (!base.Owner)
            {
                return;
            }

            SlottedInventory equippedItems = base.Owner.EquippedItems;
            foreach (ApparelExtraSpriteInfo extraSpriteInfo in ExtraSpriteInfos)
            {
                bool flag = true;
                foreach (EquipmentSlot item in extraSpriteInfo.SlotsToCheck)
                {
                    Equipment slot = equippedItems.GetSlot(item);
                    if (!slot || slot.AreSlotsExposed())
                    {
                        flag = false;
                        break;
                    }
                }

                foreach (EquipmentSlot item2 in extraSpriteInfo.SlotsToNotMatch)
                {
                    Equipment slot2 = equippedItems.GetSlot(item2);
                    if ((bool)slot2 && !slot2.AreSlotsExposed())
                    {
                        flag = false;
                        break;
                    }
                }

                string restrainer = extraSpriteInfo.Name + "_ApparelExtraSprite";
                foreach (VisualLayerInfo item3 in extraSpriteInfo.DisplaySpritesWhenNoSlot)
                {
                    item3.DisableRestraint.Set(restrainer, (bool)item3.DisplaySprite && !flag);
                }

                foreach (VisualLayerInfo item4 in extraSpriteInfo.DisplaySpritesWhenSlot)
                {
                    item4.DisableRestraint.Set(restrainer, (bool)item4.DisplaySprite && flag);
                }

                foreach (DurabilityLayerInfo item5 in extraSpriteInfo.DisplayDurabilitySpritesWhenNoSlot)
                {
                    item5.DisableRestraint.Set(restrainer, !flag);
                }

                foreach (DurabilityLayerInfo item6 in extraSpriteInfo.DisplayDurabilitySpritesWhenSlot)
                {
                    item6.DisableRestraint.Set(restrainer, flag);
                }
            }
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
