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
        public void EquipmentListeners(List<K2CustomApparel> K2AllApparel, List<K2CustomWeapon> K2AllWeapons)
        {
            K2CustomWeapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                K2CustomWeapon equippedWeapon = ScriptableObject.CreateInstance<K2CustomWeapon>();
                equippedWeapon = equippedWeapon.GetItemByName(equipAttemptInfo.Equipment.Name, K2AllWeapons);

                K2CustomApparel equippedApparel = ScriptableObject.CreateInstance<K2CustomApparel>();
                equippedApparel = equippedApparel.GetItemByName(equipAttemptInfo.Equipment.Name, K2AllApparel);

                List<Restrictions> itemRestrictions = new List<Restrictions>();
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
                    foreach (Restrictions restriction in itemRestrictions)
                    {
                        K2CustomWeapon restrictedWeapon = ScriptableObject.CreateInstance<K2CustomWeapon>();
                        restrictedWeapon = restrictedWeapon.GetItemByName(restriction.RequiredItemEquipped, K2AllWeapons);

                        if (restrictedWeapon != null)
                        {
                            CheckForRequiredItem((Item)restrictedWeapon, equipAttemptInfo);
                        }

                        K2CustomApparel restrictedApparel = ScriptableObject.CreateInstance<K2CustomApparel>();
                        restrictedApparel = restrictedApparel.GetItemByName(restriction.RequiredItemEquipped, K2AllApparel);

                        if (restrictedApparel != null)
                        {
                            CheckForRequiredItem((Item)restrictedApparel, equipAttemptInfo);
                        }
                    }
                }

                int healthModifier = Character.Get("Jenna").GetStat("stat_hitpoints").BaseMax;
                foreach (K2CustomApparel item in K2AllApparel)
                {
                    healthModifier = HealthCheck((Equipment)item, equipAttemptInfo, healthModifier, item.ModHitpoints);
                }
                foreach (K2CustomWeapon item in K2AllWeapons)
                {
                    healthModifier = HealthCheck((Equipment)item, equipAttemptInfo, healthModifier, item.ModHitpoints);
                }
                if (healthModifier > -19)
                {
                    Character.Get("Jenna").GetStat("stat_hitpoints").BaseMax = healthModifier;
                }
                else
                {
                    Restraint restraint = new Restraint();
                    restraint.Set("CanEquip", false);
                    equipAttemptInfo.CanEquip = restraint;
                    Item.GenerateErrorDialogue(Character.Get("Jenna"), "I can't Equip <color=#00ffff>" + equipAttemptInfo.Equipment.Name + "</color> right now...  I should increase my max HP.", "Sad");
                }
            });

            Character.Get("Jenna").OnItemUnequipped.AddListener(unEquipInfo =>
            {
                foreach (var equippedItem in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                {
                    K2CustomWeapon equippedWeapon = ScriptableObject.CreateInstance<K2CustomWeapon>();
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

                    K2CustomApparel equippedApparel = ScriptableObject.CreateInstance<K2CustomApparel>();
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
                foreach (K2CustomWeapon item in K2AllWeapons)
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
        private void CheckForRequiredItem(Item itemRequirement, EquipAttemptInfo equipAttemptInfo)
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
        private int HealthCheck(Equipment equipment, EquipAttemptInfo equipAttemptInfo, int healthModifier, int modHitopints)
        {
            bool replace = false;
            bool canEquip = true;
            foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
            {
                if (equipment.Name == equipped.Name)
                {
                    if (equipment.Name != equipAttemptInfo.Equipment.Name)
                    {
                        foreach (var slot in equipment.Slots)
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
                        if (healthModifier - modHitopints > -19 && canEquip)
                        {
                            healthModifier -= modHitopints;
                        }
                        else
                        {
                            canEquip = false;
                        }
                    }
                }
            }
            if (modHitopints != 0 && canEquip && equipAttemptInfo.Equipment.Name == equipment.Name)
            {
                bool isEquipped = false;
                foreach (var equipped in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                {
                    if (equipped.Name == equipAttemptInfo.Equipment.Name)
                    {
                        healthModifier -= modHitopints;
                        isEquipped = true;
                        break;
                    }
                }
                if (!isEquipped)
                {
                    if (healthModifier + modHitopints > -19)
                    {
                        healthModifier += modHitopints;
                    }
                }
            }
            return healthModifier;
        }
    }
}
