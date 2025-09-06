using ANToolkit.Controllers;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Dialogues;
using Asuna.Items;
using Modding;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace K2ExoticArmory
{
    public class K2ExoticArmory : ITCMod
    {
        public List<string> NewItemNames = new List<string>();

        public List<CustomWeapon> EarnableWeapons = new List<CustomWeapon>();

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnFrame(float deltaTime) { }
        public void OnModUnLoaded()
        {
            foreach (string itemName in NewItemNames)
            {
                Item.All.Remove(itemName.ToLower());
            }
        }
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            foreach (CustomWeapon item in EarnableWeapons)
            {
                foreach (MapCoordinate location in item.LocationCoordinates)
                {
                    if (location.MapName == newLevel && !Character.Player.Inventory.Contains(item))
                    {
                        var interactableGameObject = new GameObject();
                        interactableGameObject.transform.position = new Vector3((float)location.xCoordinate, (float)location.yCoordinate);
                        var boxCollider = interactableGameObject.AddComponent<BoxCollider>();
                        boxCollider.size = new Vector3((float)0.25, (float)0.25);

                        var interactable = interactableGameObject.AddComponent<Interactable>();
                        interactable.TypeOfInteraction = InteractionType.Talk;
                        interactable.OnInteracted.AddListener(x =>
                        {
                            GiveItems.GiveToCharacter(Character.Get("Jenna"), false, true, item);
                            interactable.gameObject.SetActive(false);
                        });
                    }
                }
            }
        }
        public void OnModLoaded(ModManifest manifest)
        {
            AddSpriteListener();
            AddRequirementListener();
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\StoreItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    var item = customEquipment.CustomInitialize(manifest);
                    NewItemNames.Add(item.Name);
                }
            }

            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\WorldItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    var item = customEquipment.CustomInitialize(manifest);
                    NewItemNames.Add(item.Name);
                    EarnableWeapons.Add(item);
                }
            }
        }
        private static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        private void AddSpriteListener()
        {

            Item.OnItemCloned.AddListener((newItem, oldItem) =>
            {
                newItem.DisplaySprite = oldItem.DisplaySprite;
                newItem.DisplaySpriteResource = oldItem.DisplaySpriteResource;
            });
        }
        private void AddRequirementListener()
        {
            CustomWeapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                foreach (CustomWeapon item in EarnableWeapons)
                {
                    List<Restrictions> restrictions = item.restrictions;
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower() && item.restrictions != null)
                    {
                        foreach (Restrictions restriction in restrictions)
                        {
                            foreach (CustomWeapon itemRequirement in EarnableWeapons)
                            {
                                if (restriction.RequiredItemEquipped == itemRequirement.Name)
                                {
                                    Restraint restraint = new Restraint();
                                    bool itemInEquipmentSlot = false;
                                    foreach (var EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                                    {
                                        if (EquipmentSlot.Name.ToLower() == itemRequirement.Name.ToLower())
                                        {
                                            itemInEquipmentSlot = true;
                                        }
                                    }
                                    if (itemInEquipmentSlot)
                                    {
                                        restraint.Set("CanEquip", true);
                                        equipAttemptInfo.CanEquip = restraint;
                                    }
                                    else
                                    {
                                        restraint.Set("CanEquip", false);
                                        equipAttemptInfo.CanEquip = restraint;
                                        string knownItem = "another item";
                                        if (Character.Player.Inventory.Contains(itemRequirement.Name))
                                        {
                                            knownItem = itemRequirement.Name;
                                        }
                                        Item.GenerateErrorDialogue(Character.Player, "I need " + knownItem + " equipped to equip this!", "Distressed");
                                    }
                                }
                            }
                        }
                    }
                    foreach (Restrictions restriction in restrictions)
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
                            Debug.Log("Item unequipped: " + item.Name);
                        }
                    }
                }
            });
        }
    }
}
