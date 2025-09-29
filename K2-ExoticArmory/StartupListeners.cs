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
                K2Items.K2Weapon equippedWeapon = ScriptableObject.CreateInstance<K2Items.K2Weapon>();
                equippedWeapon = equippedWeapon.GetItemByName(equipAttemptInfo.Equipment.Name, K2AllWeapons);

                K2Items.K2Apparel equippedApparel = ScriptableObject.CreateInstance<K2Items.K2Apparel>();
                equippedApparel = equippedApparel.GetItemByName(equipAttemptInfo.Equipment.Name, K2AllApparel);

                List<K2Items.Restrictions> itemRestrictions = new List<K2Items.Restrictions>();
                if (equippedWeapon != null || equippedApparel != null)
                {
                    if (equippedWeapon != null)
                    {
                        itemRestrictions.AddRange(equippedWeapon.restrictions);
                    }
                    if (equippedApparel != null)
                    {
                        itemRestrictions.AddRange(equippedApparel.restrictions);
                    }
                    foreach (K2Items.Restrictions restriction in itemRestrictions)
                    {
                        K2Items.K2Weapon restrictedWeapon = ScriptableObject.CreateInstance<K2Items.K2Weapon>();
                        restrictedWeapon = restrictedWeapon.GetItemByName(restriction.RequiredItemEquipped, K2AllWeapons);

                        if (restrictedWeapon != null)
                        {
                            RequiredItemInInventory((Item)restrictedWeapon, equipAttemptInfo);
                        }

                        K2Items.K2Apparel restrictedApparel = ScriptableObject.CreateInstance<K2Items.K2Apparel>();
                        restrictedApparel = restrictedApparel.GetItemByName(restriction.RequiredItemEquipped, K2AllApparel);

                        if (restrictedApparel != null)
                        {
                            RequiredItemInInventory((Item)restrictedApparel, equipAttemptInfo);
                        }
                    }
                }

                UpdateHealth(K2AllApparel, K2AllWeapons, equipAttemptInfo);
            });

            Character.Get("Jenna").OnItemUnequipped.AddListener(unEquipInfo =>
            {
                foreach (var equippedItem in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                {
                    K2Items.K2Weapon equippedWeapon = ScriptableObject.CreateInstance<K2Items.K2Weapon>();
                    equippedWeapon = equippedWeapon.GetItemByName(equippedItem.Name, K2AllWeapons);

                    if (equippedWeapon != null)
                    {
                        if (equippedWeapon.restrictions != null)
                        {
                            foreach (var restriction in equippedWeapon.restrictions)
                            {
                                if (restriction.RequiredItemEquipped == unEquipInfo.Name)
                                {
                                    Character.Get("Jenna").UnequipItem((Equipment)equippedItem);
                                }
                            }
                        }
                    }

                    K2Items.K2Apparel equippedApparel = ScriptableObject.CreateInstance<K2Items.K2Apparel>();
                    equippedApparel = equippedApparel.GetItemByName(equippedItem.Name, K2AllApparel);

                    if (equippedApparel != null)
                    {
                        if (equippedApparel.restrictions != null)
                        {
                            foreach (var restriction in equippedApparel.restrictions)
                            {
                                if (restriction.RequiredItemEquipped == unEquipInfo.Name)
                                {
                                    Character.Get("Jenna").UnequipItem((Equipment)equippedItem);
                                }
                            }
                        }
                    }
                }
            });

            Character.Get("Jenna").OnItemEquipped.AddListener(equipInfo =>
            {
                foreach (K2Items.K2Weapon item in K2AllWeapons)
                {
                    if (equipInfo.Name == item.Name && item.questModifiers != null && item.questModifiers.BaseWeapon)
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
                }
            });
        }
        private void RequiredItemInInventory(Item itemRequirement, EquipAttemptInfo equipAttemptInfo)
        {
            bool canEquip = false;
            Restraint restraint = new Restraint();
            foreach (var EquipmentSlot in Character.Get("Jenna").EquippedItems.GetAll<Item>())
            {
                if (EquipmentSlot.Name == itemRequirement.Name)
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
                if (Character.Get("Jenna").Inventory.Contains(itemRequirement))
                {
                    knownItem = itemRequirement.Name;
                }
                Item.GenerateErrorDialogue(Character.Get("Jenna"), "I need <color=#00ffff>" + knownItem + "</color> equipped to equip this!", "Distressed");
            }
        }
        private void UpdateHealth(List<K2Items.K2Apparel> K2AllApparel, List<K2Items.K2Weapon> K2AllWeapons, EquipAttemptInfo equipAttemptInfo)
        {
            Restraint restraint = new Restraint();
            bool replace = false;
            bool canEquip = true;
            int healthModifier = Character.Get("Jenna").GetStat("stat_hitpoints").BaseMax;
            foreach (K2Items.K2Apparel item in K2AllApparel)
            {
                foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                {
                    if (item.Name == equipped.Name)
                    {
                        if (item.Name != equipAttemptInfo.Equipment.Name)
                        {
                            foreach (var slot in item.Slots)
                            {
                                if (equipAttemptInfo.Equipment.Slots.Contains(slot))
                                {
                                    replace = true;
                                    break;
                                }
                            }
                        }
                        if (replace)
                        {
                            if (healthModifier - item.ModHitpoints > -19 && canEquip)
                            {
                                healthModifier -= item.ModHitpoints;
                            }
                            else
                            {
                                canEquip = false;
                            }
                        }
                    }
                }
                if (item.ModHitpoints != 0 && canEquip && equipAttemptInfo.Equipment.Name == item.Name)
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
            foreach (K2Items.K2Weapon item in K2AllWeapons)
            {
                foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                {
                    if (item.Name == equipped.Name)
                    {
                        if (item.Name != equipAttemptInfo.Equipment.Name)
                        {
                            foreach (var slot in item.Slots)
                            {
                                if (equipAttemptInfo.Equipment.Slots.Contains(slot))
                                {
                                    replace = true;
                                    break;
                                }
                            }
                        }
                        if (replace)
                        {
                            if (healthModifier - item.ModHitpoints > -19 && canEquip)
                            {
                                healthModifier -= item.ModHitpoints;
                            }
                            else
                            {
                                canEquip = false;
                            }
                        }
                    }
                }
                if (item.ModHitpoints != 0 && canEquip && equipAttemptInfo.Equipment.Name == item.Name)
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
            if (healthModifier > -19 && canEquip)
            {
                Character.Get("Jenna").GetStat("stat_hitpoints").BaseMax = healthModifier;
            }
            else
            {
                restraint.Set("CanEquip", false);
                equipAttemptInfo.CanEquip = restraint;
                Item.GenerateErrorDialogue(Character.Get("Jenna"), "I can't Equip <color=#00ffff>" + equipAttemptInfo.Equipment.Name + "</color> right now...  I should increase my max HP.", "Sad");
            }
        }
    }
}
