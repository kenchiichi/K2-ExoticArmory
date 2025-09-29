using ANToolkit.Controllers;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.Missions;
using Asuna.NewMissions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class K2Apparel : K2Equipment
    {
        public List<ModEquipmentSprite> Sprites;

        private K2CustomApparel _instance;

        private ModManifest _manifest;
        public K2CustomApparel CustomInitialize(ModManifest manifest)
        {

            K2CustomApparel apparel = ScriptableObject.CreateInstance<K2CustomApparel>();

            _manifest = manifest;

            _instance = apparel;

            if (StatModifierInfos != null)
            {
                apparel.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(apparel, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(apparel, StatModifierInfos);
            }

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }

            if (questModifiers != null)
            {
                apparel.questModifiers = questModifiers;
            }

            if (Sprites != null)
            {
                apparel.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(manifest.SpriteResolver)));
            }

            apparel.DisplaySpriteResource = manifest.SpriteResolver.ResolveAsResource(PreviewImage);
            apparel.LocationCoordinates = LocationCoordinates;
            apparel.Name = Name;
            apparel.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            apparel.Category = (ItemCategory)apparel.K2ItemCategory;
            apparel.Description = Description;
            apparel.Price = Price;
            apparel.StatRequirements = StatRequirements;
            apparel.restrictions = restrictions;
            apparel.questModifiers = questModifiers;
            apparel.ModHitpoints = ModHitpoints;

            AddAbilitiesToEquipment(apparel, _manifest);

            return apparel;
        }

        public void LoadWorldItems(string newLevel, List<K2CustomApparel> K2AllApparel)
        {
            K2ExoticArmory k2ExoticArmory = new K2ExoticArmory();

            foreach (K2CustomApparel item in K2AllApparel)
            {
                K2CustomApparel itemRequired = null;
                foreach (K2CustomApparel item2 in K2AllApparel)
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
                                        k2ExoticArmory.k2Equipment.RemoveItemFromJenna(itemRequired.Name);
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
