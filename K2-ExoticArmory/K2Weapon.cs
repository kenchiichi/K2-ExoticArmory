using ANToolkit.Controllers;
using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.Missions;
using Asuna.NewCombat;
using Asuna.NewMissions;
using HutongGames.PlayMaker;
using K2Items;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class K2Weapon : K2Items.K2Equipment
    {
        public CustomVFX customVFX;

        private K2Items.K2Weapon _instance;

        private ModManifest _manifest;

        public K2Items.K2Weapon CustomInitialize(ModManifest manifest)
        {
            K2Items.K2Weapon weapon = ScriptableObject.CreateInstance<K2Items.K2Weapon>();

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
            weapon.ModHitpoints = ModHitpoints;

            AddAbilitiesToEquipment(weapon, _manifest);

            return weapon;
        }
        private void AddShootingVFXHook(Weapon weapon, Sprite sprite, int burstCount)
        {
            CombatTurnManager.OnTurnStart.AddListener(() =>
            {
                if (!Character.Get("Jenna").EquippedItems.Contains(weapon)) return;
                var ability = Character.Get("Jenna").GetAbilities().First(x => x.DisplayName == "Attack");

                var vfxGameObject = new GameObject("VFX_Custom");
                vfxGameObject.transform.position = new Vector3(9999999999, 0);

                var vfxSpriteRenderer = vfxGameObject.AddComponent<SpriteRenderer>();
                vfxSpriteRenderer.sprite = Sprite.Create(sprite.texture, new Rect(0, 0, sprite.texture.width, sprite.texture.height), new Vector2(0.5f, 0.5f), 800f);

                var cachedFSM = typeof(Ability)
                    .GetField("CachedFsm", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(ability) as Fsm;

                var energyGunBurstState = cachedFSM.States.First(x => x.Name == "EnergyGunBurst");

                var combatVFX = energyGunBurstState.Actions[0] as PMA_CombatVisualEffect;
                combatVFX.Prefab = vfxGameObject;
                combatVFX.AmountToSpawn = burstCount;
            });
        }
        public void LoadWorldItems(string newLevel, List<K2Items.K2Weapon> K2AllWeapons)
        {
            K2ExoticArmory k2ExoticArmory = new K2ExoticArmory();

            foreach (K2Items.K2Weapon item in K2AllWeapons)
            {
                K2Items.K2Weapon itemRequired = null;
                foreach (K2Items.K2Weapon item2 in K2AllWeapons)
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
                    if (item.LocationCoordinates.MapName == newLevel && !Character.Player.Inventory.Contains(item))
                    {
                        if (itemRequired != null)
                        {
                            var itemEquiped = false;
                            foreach (Item EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                            {
                                if (EquipmentSlot.Name.ToLower() == itemRequired.Name.ToLower())
                                {
                                    itemEquiped = true;
                                }
                            }
                            if (Character.Player.Inventory.Contains(itemRequired) || itemEquiped)
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
                                        k2ExoticArmory.RemoveItemFromJenna(itemRequired.Name);
                                    }
                                    else
                                    {
                                        GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, item);
                                    }
                                    Item.GenerateErrorDialogue(Character.Player, "I found <color=#00ffff>" + item.Name + "</color> laying here!", "Happy");
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
                                Item.GenerateErrorDialogue(Character.Player, "I found <color=#00ffff>" + item.Name + "</color> laying here!", "Happy");
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
