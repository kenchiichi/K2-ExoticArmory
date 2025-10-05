using ANToolkit.Controllers;
using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.Missions;
using Asuna.NewCombat;
using Asuna.NewMissions;
using HutongGames.PlayMaker;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class K2Weapon : K2Equipment
    {
        public CustomVFX customVFX;

        private K2CustomWeapon _instance;

        private ModManifest _manifest;

        public K2CustomWeapon CustomInitialize(ModManifest manifest)
        {
            K2CustomWeapon weapon = ScriptableObject.CreateInstance<K2CustomWeapon>();

            _manifest = manifest;

            _instance = weapon;

            if (StatModifierInfos != null)
            {
                weapon.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);

                DamageFractionInfo damageFractionInfo = new DamageFractionInfo();

                float physicalDamageStat = 0;
                float lustDamageStat = 0;

                foreach (var info in weapon.StatModifierInfos)
                {
                    if (info.StatName == "stat_physical_power")
                    {
                        physicalDamageStat = info.ModifyAmount;
                    }
                    if (info.StatName == "stat_lust_power")
                    {
                        lustDamageStat = info.ModifyAmount;
                    }
                }
                if (physicalDamageStat != 0)
                {
                    damageFractionInfo.DamageType = (DamageType)1;
                    damageFractionInfo.Fraction = physicalDamageStat / (physicalDamageStat + lustDamageStat);
                    weapon.DamageTypesDealt.Add(damageFractionInfo);
                }
                if (lustDamageStat != 0)
                {
                    damageFractionInfo.DamageType = (DamageType)2;
                    damageFractionInfo.Fraction = lustDamageStat / (physicalDamageStat + lustDamageStat);
                    weapon.DamageTypesDealt.Add(damageFractionInfo);
                }
            }
            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }

            if (customVFX.BurstCount > 0)
            {
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                ANResourceSprite weaponAttackVFXSprite = manifest.SpriteResolver.ResolveAsResource(customVFX.WeaponAttackVFXSprite);
                AddShootingVFXHook(weapon, (Sprite)weaponAttackVFXSprite, customVFX.BurstCount);
            }
            else
            {
                weapon.AttackVFXType = AttackVFXType.UnarmedMelee;
            }

            if (questModifiers != null)
            {
                weapon.questModifiers = questModifiers;
                CreateSearchQuest(questModifiers.DisplayName, questModifiers.Description, questModifiers.name, questModifiers.Repeatable, questModifiers.TaskDisplay);
            }

            if (Sprites != null)
            {
                weapon.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(manifest.SpriteResolver)));
            }

            weapon.DisplaySpriteResource = manifest.SpriteResolver.ResolveAsResource(PreviewImage);
            weapon.LocationCoordinates = LocationCoordinates;
            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = (ItemCategory)weapon.K2ItemCategory;
            weapon.Description = Description;
            weapon.Price = Price;
            weapon.StatRequirements = StatRequirements;
            weapon.restrictions = restrictions;
            weapon.StatRequirements.AddRange(StatRequirements);

            AddAbilitiesToEquipment(weapon, _manifest);

            return weapon;
        }
        private void AddShootingVFXHook(Weapon weapon, Sprite sprite, int burstCount)
        {
            CombatTurnManager.OnTurnStart.AddListener(() =>
            {
                if (!Character.Get("Jenna").EquippedItems.Contains(weapon)) return;

                string tem = "abilities: \n";
                foreach (var item in Character.Get("Jenna").GetAbilities())
                {
                    tem += item.DisplayName += "\n";
                }
                Debug.Log(tem);

                Debug.Log(1);
                var ability = Character.Get("Jenna").GetAbilities().First(x => x.DisplayName == "Attack");
                Debug.Log(ability.DisplayName);

                var vfxGameObject = new GameObject("VFX_Custom");
                Debug.Log(3);
                vfxGameObject.transform.position = new Vector3(9999999999, 0);
                Debug.Log(4);

                var vfxSpriteRenderer = vfxGameObject.AddComponent<SpriteRenderer>();
                Debug.Log(5);
                vfxSpriteRenderer.sprite = Sprite.Create(sprite.texture, new Rect(0, 0, sprite.texture.width, sprite.texture.height), new Vector2(0.5f, 0.5f), 800f);
                Debug.Log(6);

                var cachedFSM = typeof(Ability)
                    .GetField("CachedFsm", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(ability) as Fsm;
                Debug.Log(7);

                var energyGunBurstState = cachedFSM.States.First(x => x.Name == "EnergyGunBurst");
                Debug.Log(8);

                var combatVFX = energyGunBurstState.Actions[0] as PMA_CombatVisualEffect;
                Debug.Log(9);
                combatVFX.Prefab = vfxGameObject;
                Debug.Log(10);
                combatVFX.AmountToSpawn = burstCount;
                Debug.Log(11);
            });
        }
        public void LoadWorldItems(string newLevel, List<K2CustomWeapon> K2AllWeapons)
        {
            K2ExoticArmory k2ExoticArmory = new K2ExoticArmory();

            foreach (K2CustomWeapon item in K2AllWeapons)
            {
                K2CustomWeapon itemRequired = null;
                foreach (K2CustomWeapon item2 in K2AllWeapons)
                {
                    if (item.LocationCoordinates != null)
                    {
                        if (item2.Name == item.LocationCoordinates.PrerequisiteQuestWeapon)
                        {
                            itemRequired = item2;
                        }
                    }
                }
                if (item.LocationCoordinates != null)
                {
                    if (item.LocationCoordinates.MapName == newLevel && !Character.Get("Jenna").Inventory.Contains(item))
                    {
                        if (itemRequired != null)
                        {
                            var itemEquiped = false;
                            foreach (Item EquipmentSlot in Character.Get("Jenna").EquippedItems.GetAll<Item>())
                            {
                                if (EquipmentSlot.Name.ToLower() == itemRequired.Name.ToLower())
                                {
                                    itemEquiped = true;
                                }
                            }
                            if (Character.Get("Jenna").Inventory.Contains(itemRequired) || itemEquiped)
                            {
                                var interactableGameObject = new GameObject();
                                interactableGameObject.transform.position = new Vector3((float)item.LocationCoordinates.xCoordinate, (float)item.LocationCoordinates.yCoordinate);

                                var boxCollider = interactableGameObject.AddComponent<BoxCollider>();
                                boxCollider.size = new Vector3((float)0.25, (float)0.25);

                                var interactable = interactableGameObject.AddComponent<Interactable>();
                                interactable.TypeOfInteraction = InteractionType.Talk;
                                interactable.OnInteracted.AddListener(x =>
                                {
                                    if (itemRequired != null)
                                    {
                                        if (item.questModifiers != null)
                                        {
                                            if (item.questModifiers.next != null)
                                            {
                                                var oldMission = MissionContainer.GetMission(item.questModifiers.name + "_Quest");
                                                oldMission.Completion = TaskCompletion.Complete;

                                                var oldTask = oldMission.StartTask(item.questModifiers.name + "_Task");

                                                MissionContainer.AddMissionToLookup(oldMission);
                                                MissionContainer.AddTaskToLookup(oldTask);

                                                if (item.questModifiers.next != "" && item.questModifiers.next != null)
                                                {
                                                    var newMission = NewMission.StartMissionByID(item.questModifiers.next + "_Quest");
                                                    newMission.Completion = TaskCompletion.InProgress;

                                                    var newTask = newMission.StartTask(item.questModifiers.next + "_Task");

                                                    MissionContainer.AddMissionToLookup(newMission);
                                                    MissionContainer.AddTaskToLookup(newTask);
                                                }
                                            }
                                        }
                                        GiveItems.GiveToCharacter(Character.Get("Jenna"), itemEquiped, false, item);
                                        k2ExoticArmory.K2Equipment.RemoveItemFromJenna(itemRequired.Name);
                                    }
                                    else
                                    {
                                        GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, item);
                                    }
                                    Item.GenerateErrorDialogue(Character.Get("Jenna"), "I found <color=#00ffff>" + item.Name + "</color> laying here!", "Happy");
                                    interactable.gameObject.SetActive(false);
                                });
                            }
                        }
                        else
                        {
                            var interactableGameObject = new GameObject();
                            interactableGameObject.transform.position = new Vector3((float)item.LocationCoordinates.xCoordinate, (float)item.LocationCoordinates.yCoordinate);

                            var boxCollider = interactableGameObject.AddComponent<BoxCollider>();
                            boxCollider.size = new Vector3((float)0.25, (float)0.25);

                            var interactable = interactableGameObject.AddComponent<Interactable>();
                            interactable.TypeOfInteraction = InteractionType.Talk;
                            interactable.OnInteracted.AddListener(x =>
                            {
                                Item.GenerateErrorDialogue(Character.Get("Jenna"), "I found <color=#00ffff>" + item.Name + "</color> laying here!", "Happy");
                                GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, item);
                                interactable.gameObject.SetActive(false);
                            });
                        }
                    }
                }
            }
        }
    }
}
