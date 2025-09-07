using ANToolkit.Controllers;
using ANToolkit.FPS.Weapons;
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
        public List<K2ExoticArmoryWeapon.CustomWeapon> PurchasableWeapons = new List<K2ExoticArmoryWeapon.CustomWeapon>();

        public List<K2ExoticArmoryWeapon.CustomWeapon> EarnableWeapons = new List<K2ExoticArmoryWeapon.CustomWeapon>();

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnFrame(float deltaTime) { }
        public void OnModUnLoaded()
        {
            foreach (var item in PurchasableWeapons)
            {
                Item.All.Remove(item.Name.ToLower());
            }
            foreach (var item in EarnableWeapons)
            {
                Item.All.Remove(item.Name.ToLower());
            }
        }
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            foreach (K2ExoticArmoryWeapon.CustomWeapon item in EarnableWeapons)
            {
                if (item.LocationCoordinates.MapName == newLevel && !Character.Player.Inventory.Contains(item))
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
        public void OnModLoaded(ModManifest manifest)
        {
            AddSpriteListener();

            AddRequirementListener();

            WeaponSerialStreamReader(manifest, "data\\StoreWeaponData.xml", PurchasableWeapons);

            WeaponSerialStreamReader(manifest, "data\\WorldWeaponData.xml", EarnableWeapons);
        }

        private void WeaponSerialStreamReader(ModManifest manifest, string xmlpath, List<K2ExoticArmoryWeapon.CustomWeapon> list)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath)))
            {
                List<K2Weapon> k2Weapons = Deserialize<List<K2Weapon>>(reader.ReadToEnd());

                foreach (K2Weapon k2Weapon in k2Weapons)
                {
                    var item = k2Weapon.CustomInitialize(manifest);
                    list.Add(item);
                }
            }
        }
        private void ApparelSerialStreamReader(ModManifest manifest, string xmlpath, List<K2ExoticArmoryApparel.CustomApparel> list)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath)))
            {
                List<K2Apparel> k2Apparels = Deserialize<List<K2Apparel>>(reader.ReadToEnd());

                foreach (K2Apparel k2Apparel in k2Apparels)
                {
                    var item = k2Apparel.CustomInitialize(manifest);
                    list.Add(item);
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
            K2ExoticArmoryWeapon.CustomWeapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                foreach (K2ExoticArmoryWeapon.CustomWeapon item in EarnableWeapons)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower() && item.restrictions != null)
                    {
                        foreach (K2ExoticArmoryWeapon.Restrictions restriction in item.restrictions)
                        {
                            foreach (K2ExoticArmoryWeapon.CustomWeapon itemRequirement in EarnableWeapons)
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
                                        Item.GenerateErrorDialogue(Character.Player, "I need <color=#00ffff>" + knownItem + "</color> equipped to equip this!", "Distressed");
                                    }
                                }
                            }
                        }
                    }
                    foreach (K2ExoticArmoryWeapon.Restrictions restriction in item.restrictions)
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
    }
}
