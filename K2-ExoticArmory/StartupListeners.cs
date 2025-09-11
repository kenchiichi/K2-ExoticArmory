using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Items;
using K2CustomEquipment;
using System.Collections.Generic;
using UnityEngine;

namespace K2ExoticArmory
{
    internal class StartupListeners
    {
        public void AddSpriteListener()
        {

            Item.OnItemCloned.AddListener((newItem, oldItem) =>
            {
                newItem.DisplaySprite = oldItem.DisplaySprite;
                newItem.DisplaySpriteResource = oldItem.DisplaySpriteResource;
            });
        }
        public void AddRequirementListener(List<CustomApparel> K2AllApparel, List<CustomWeapon> K2AllWeapons)
        {
            K2CustomEquipment.CustomWeapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                List<Restrictions> itemRequirementName = new List<Restrictions>();
                CustomApparel customApparel = ScriptableObject.CreateInstance<K2CustomEquipment.CustomApparel>();
                CustomWeapon customWeapon = ScriptableObject.CreateInstance<K2CustomEquipment.CustomWeapon>();
                foreach (K2CustomEquipment.CustomWeapon item in K2AllWeapons)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                    {
                        itemRequirementName = item.restrictions;
                        customWeapon = item;
                    }
                }
                foreach (K2CustomEquipment.CustomApparel item in K2AllApparel)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                    {
                        itemRequirementName = item.restrictions;
                        customApparel = item;
                    }
                }
                foreach (K2CustomEquipment.Restrictions restriction in itemRequirementName)
                {
                    foreach (K2CustomEquipment.CustomWeapon itemRequirement in K2AllWeapons)
                    {
                        RequiredItemInInventory(restriction, itemRequirement.Name, equipAttemptInfo);
                    }
                    foreach (K2CustomEquipment.CustomApparel itemRequirement in K2AllApparel)
                    {
                        RequiredItemInInventory(restriction, itemRequirement.Name, equipAttemptInfo);
                    }
                }

                foreach (CustomWeapon item in K2AllWeapons)
                {
                    foreach (Restrictions restriction in item.restrictions)
                    {
                        Item newitem = null;
                        bool itemInEquipmentSlot = false;
                        foreach (var EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                        {
                            if (EquipmentSlot.Name.ToLower() == restriction.RequiredItemEquipped.ToLower())
                            {
                                itemInEquipmentSlot = true;
                            }
                            if (EquipmentSlot.Name.ToLower() == item.Name.ToLower())
                            {
                                newitem = EquipmentSlot;
                            }
                        }
                        if (equipAttemptInfo.Equipment.Name.ToLower() == restriction.RequiredItemEquipped.ToLower() && itemInEquipmentSlot)
                        {
                            Character.Player.UnequipItem((Equipment)newitem);
                        }
                    }
                }
                foreach (CustomApparel item in K2AllApparel)
                {
                    foreach (Restrictions restriction in item.restrictions)
                    {
                        Item newitem = null;
                        bool itemInEquipmentSlot = false;
                        foreach (var EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                        {
                            if (EquipmentSlot.Name.ToLower() == restriction.RequiredItemEquipped.ToLower())
                            {
                                itemInEquipmentSlot = true;
                            }
                            if (EquipmentSlot.Name.ToLower() == item.Name.ToLower())
                            {
                                newitem = EquipmentSlot;
                            }
                        }
                        if (equipAttemptInfo.Equipment.Name.ToLower() == restriction.RequiredItemEquipped.ToLower() && itemInEquipmentSlot)
                        {
                            Character.Player.UnequipItem((Equipment)newitem);
                        }
                    }
                }
            });
        }
        private void RequiredItemInInventory(Restrictions restriction, string itemRequirement, EquipAttemptInfo equipAttemptInfo)
        {
            bool canEquip = false;
            if (restriction.RequiredItemEquipped == itemRequirement)
            {
                Restraint restraint = new Restraint();
                foreach (var EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                {
                    if (EquipmentSlot.Name.ToLower() == itemRequirement.ToLower())
                    {
                        canEquip = true;
                    }
                }
                if (canEquip)
                {
                    restraint.Set("CanEquip", true);
                    equipAttemptInfo.CanEquip = restraint;
                }
                else
                {
                    restraint.Set("CanEquip", false);
                    equipAttemptInfo.CanEquip = restraint;
                    string knownItem = "another item";
                    if (Character.Player.Inventory.Contains(itemRequirement))
                    {
                        knownItem = itemRequirement;
                    }
                    Item.GenerateErrorDialogue(Character.Player, "I need <color=#00ffff>" + knownItem + "</color> equipped to equip this!", "Distressed");
                }
            }
        }
    }
}
