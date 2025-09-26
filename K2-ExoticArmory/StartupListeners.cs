using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.Missions;
using Asuna.NewMissions;
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
        public void EquipAttemptListener(List<K2Items.K2Apparel> K2AllApparel, List<K2Items.K2Weapon> K2AllWeapons)
        {
            K2Items.K2Weapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                List<K2Items.Restrictions> itemRequirementName = new List<K2Items.Restrictions>();
                K2Items.K2Apparel customApparel = ScriptableObject.CreateInstance<K2Items.K2Apparel>();
                K2Items.K2Weapon customWeapon = ScriptableObject.CreateInstance<K2Items.K2Weapon>();
                foreach (K2Items.K2Weapon item in K2AllWeapons)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                    {
                        if (item.questModifiers != null && item.questModifiers.BaseWeapon)
                        {
                            var missionInstance = MissionContainer.GetMission(item.questModifiers.next + "_Quest");
                            if (missionInstance.Completion == TaskCompletion.None && item.questModifiers.next != "")
                            {
                                missionInstance = NewMission.StartMissionByID(item.questModifiers.next + "_Quest");
                                missionInstance.Completion = TaskCompletion.InProgress;

                                var taskInstance = missionInstance.StartTask(item.questModifiers.next + "_Task");
                                taskInstance.Completion = TaskCompletion.InProgress;

                                MissionContainer.AddMissionToLookup(missionInstance);
                                MissionContainer.AddTaskToLookup(taskInstance);
                            }
                        }

                        itemRequirementName = item.restrictions;
                        customWeapon = item;
                    }
                }
                foreach (K2Items.K2Apparel item in K2AllApparel)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                    {
                        itemRequirementName = item.restrictions;
                        customApparel = item;
                    }
                }
                foreach (K2Items.Restrictions restriction in itemRequirementName)
                {
                    foreach (K2Items.K2Weapon itemRequirement in K2AllWeapons)
                    {
                        RequiredItemInInventory(restriction, itemRequirement.Name, equipAttemptInfo);

                    }
                    foreach (K2Items.K2Apparel itemRequirement in K2AllApparel)
                    {
                        RequiredItemInInventory(restriction, itemRequirement.Name, equipAttemptInfo);
                    }
                }

                //Unequip Required Weapon
                foreach (K2Items.K2Weapon item in K2AllWeapons)
                {
                    foreach (K2Items.Restrictions restriction in item.restrictions)
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

                //Hitpoints
                Restraint restraint = new Restraint();
                bool replace = false;
                bool canEquip = true;
                int healthModifier = 0;
                foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                {
                    if (stats.Name == "Hitpoints")
                    {
                        healthModifier = stats.BaseMax;
                    }
                }
                foreach (K2Items.K2Apparel item in K2AllApparel)
                {
                    foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                    {
                        if (item.Name != equipAttemptInfo.Equipment.Name && item.Name == equipped.Name)
                        {
                            foreach (var slot in item.Slots)
                            {
                                foreach (var slot2 in equipAttemptInfo.Equipment.Slots)
                                {
                                    if (slot == slot2)
                                    {
                                        replace = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (equipped.Name.ToLower() == item.Name.ToLower() && replace)
                        {
                            foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                            {
                                if (stats.Name == "Hitpoints")
                                {
                                    if (healthModifier - item.ModHitpoints > -19 && canEquip)
                                    {
                                        healthModifier -= item.ModHitpoints;
                                        break;
                                    }
                                    else
                                    {
                                        canEquip = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (item.ModHitpoints != 0 && canEquip)
                    {
                        if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                        {
                            foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                            {
                                if (stats.Name == "Hitpoints")
                                {
                                    bool isEquipped = false;
                                    foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                                    {
                                        if (equipped.Name == equipAttemptInfo.Equipment.Name)
                                        {
                                            healthModifier -= item.ModHitpoints;
                                            isEquipped = true;
                                            break;
                                        }
                                    }
                                    if (!isEquipped)
                                    {
                                        if (healthModifier + item.ModHitpoints > -19)
                                        {
                                            healthModifier += item.ModHitpoints;
                                        }
                                        else
                                        {
                                            canEquip = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (K2Items.K2Weapon item in K2AllWeapons)
                {
                    foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                    {
                        if (item.Name != equipAttemptInfo.Equipment.Name && item.Name == equipped.Name)
                        {
                            foreach (var slot in item.Slots)
                            {
                                foreach (var slot2 in equipAttemptInfo.Equipment.Slots)
                                {
                                    if (slot == slot2)
                                    {
                                        replace = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (equipped.Name.ToLower() == item.Name.ToLower() && replace)
                        {
                            foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                            {
                                if (stats.Name == "Hitpoints")
                                {
                                    if (healthModifier - item.ModHitpoints > -19 && canEquip)
                                    {
                                        healthModifier -= item.ModHitpoints;
                                        break;
                                    }
                                    else
                                    {
                                        canEquip = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (item.ModHitpoints != 0 && canEquip)
                    {
                        if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower())
                        {
                            foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                            {
                                if (stats.Name == "Hitpoints")
                                {
                                    bool isEquipped = false;
                                    foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                                    {
                                        if (equipped.Name == equipAttemptInfo.Equipment.Name)
                                        {
                                            healthModifier -= item.ModHitpoints;
                                            isEquipped = true;
                                            break;
                                        }
                                    }
                                    if (!isEquipped)
                                    {
                                        if (healthModifier + item.ModHitpoints > -19)
                                        {
                                            healthModifier += item.ModHitpoints;
                                        }
                                        else
                                        {
                                            canEquip = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var stats in Character.Get("Jenna").Stats.GetAll<Stat>())
                {
                    if (stats.Name == "Hitpoints")
                    {
                        if (healthModifier > -19 && canEquip)
                        {
                            stats.BaseMax = healthModifier;
                        }
                        else
                        {
                            restraint.Set("CanEquip", false);
                            equipAttemptInfo.CanEquip = restraint;
                            Item.GenerateErrorDialogue(Character.Player, "I can't Equip <color=#00ffff>" + equipAttemptInfo.Equipment.Name + "</color> right now...  I should increase my max HP.", "Sad");
                        }
                    }
                }
            });
        }
        private void RequiredItemInInventory(K2Items.Restrictions restriction, string itemRequirement, EquipAttemptInfo equipAttemptInfo)
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
