using ANToolkit.Controllers;
using ANToolkit.Debugging;
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
        public ItemVendor vendor;

        private ModManifest _manifest;

        public List<K2CustomEquipment.CustomWeapon> K2AllWeapons = new List<K2CustomEquipment.CustomWeapon>();

        public List<K2CustomEquipment.CustomApparel> K2AllApparel = new List<K2CustomEquipment.CustomApparel>();

        public List<Item> K2Items = new List<Item>();

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnFrame(float deltaTime) { }
        public void OnModUnLoaded()
        {
            string removedItems = "Items removed: \n";
            foreach (var item in K2Items)
            {
                Item.All.Remove(item.Name.ToLower());
                removedItems += item.Name + "\n";
                foreach (Item EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
                {
                    if (EquipmentSlot.Name.ToLower() == item.Name.ToLower())
                    {
                        Character.Player.EquippedItems.Remove(item.Name);
                    }
                }
                foreach (Item inventoryItem in Character.Player.Inventory.GetAll<Item>())
                {
                    if (inventoryItem.Name == item.Name)
                    {
                        Character.Get("Jenna").Inventory.Remove(item.Name);
                    }
                }
            }
            Debug.Log(removedItems);
            Item.GenerateErrorDialogue(Character.Player, "I should remember to check to see if I have weapons to defend myself.", "Think");
        }
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            foreach (K2CustomEquipment.CustomWeapon item in K2AllWeapons)
            {
                if (item.LocationCoordinates != null)
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

            if (newLevel == "Motel_UpperFloor")
            {
                double xPos = -6.00;
                double yPos = 16.30;

                GameObject adaNPC = new GameObject();
                adaNPC.transform.position = new Vector3((float)xPos, (float)yPos);
                BoxCollider adaNPCCollider = adaNPC.AddComponent<BoxCollider>();
                adaNPCCollider.size = new Vector3(1f, 2f);
                GameObject adaNPCSprite = new GameObject();
                adaNPCSprite.transform.position = new Vector3((float)(xPos - .7), (float)(yPos - .65));
                SpriteRenderer adaNPCSpriteRenderer = adaNPCSprite.AddComponent<SpriteRenderer>();
                adaNPCSpriteRenderer.sprite = _manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\npc\\ada_overworld.png");
                adaNPCSpriteRenderer.transform.localScale = new Vector3(1f, 1f);

                Interactable adaNPCInteractable = adaNPC.AddComponent<Interactable>();
                adaNPCInteractable.TypeOfInteraction = InteractionType.Talk;
                adaNPCInteractable.OnInteracted.AddListener(x =>
                {
                    vendor.Catalogue.OpenShop();
                });
            }
        }
        public void OnModLoaded(ModManifest manifest)
        {
            _manifest = manifest;

            AddSpriteListener();

            AddRequirementListener();

            WeaponSerialStreamReader(manifest, "data\\StoreWeaponData.xml", K2AllWeapons);

            ApparelSerialStreamReader(manifest, "data\\StoreApparelData.xml", K2AllApparel);

            WeaponSerialStreamReader(manifest, "data\\WorldWeaponData.xml", K2AllWeapons);

            ApparelSerialStreamReader(manifest, "data\\WorldApparelData.xml", K2AllApparel);

            List<ShopItemInfo> shopItems = new List<ShopItemInfo>();

            ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();

            foreach (var item in K2AllApparel)
            {
                if (item.Price > 0)
                {
                    shopItems.Add(
                    new ShopItemInfo()
                    {
                        Item = item,
                        Cost = item.Price,
                    }
                );
                }
            }
            foreach (var item in K2AllWeapons)
            {
                if (item.Price > 0)
                {
                    shopItems.Add(
                        new ShopItemInfo()
                        {
                            Item = item,
                            Cost = item.Price,
                        }
                    );
                }
            }
            catalogue.Items = shopItems;
            vendor = new ItemVendor()
            {
                Catalogue = catalogue,
            };

            ConCommand.Add("GiveArmory", delegate
            {
                string items = "K2-ExoticAromory items: \n";
                foreach (var item in K2Items)
                {
                    items += item.Name + "\n";
                    GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, item);
                }
                Debug.Log(items);
            });
        }

        private void WeaponSerialStreamReader(ModManifest manifest, string xmlpath, List<K2CustomEquipment.CustomWeapon> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Weapon> k2Weapons = Deserialize<List<K2Weapon>>(reader.ReadToEnd());

            foreach (K2Weapon k2Weapon in k2Weapons)
            {
                var item = k2Weapon.CustomInitialize(manifest);
                list.Add(item);
                K2Items.Add(item);
            }
        }
        private void ApparelSerialStreamReader(ModManifest manifest, string xmlpath, List<K2CustomEquipment.CustomApparel> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Apparel> k2Apparels = Deserialize<List<K2Apparel>>(reader.ReadToEnd());

            foreach (K2Apparel k2Apparel in k2Apparels)
            {
                var item = k2Apparel.CustomInitialize(manifest);
                list.Add(item);
                K2Items.Add(item);
            }
        }

        private static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xmlString);
            return (T)serializer.Deserialize(reader);
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
            K2CustomEquipment.CustomWeapon.OnEquipAttempt.AddListener(equipAttemptInfo =>
            {
                foreach (K2CustomEquipment.CustomWeapon item in K2AllWeapons)
                {
                    if (equipAttemptInfo.Equipment.Name.ToLower() == item.Name.ToLower() && item.restrictions != null)
                    {
                        foreach (K2CustomEquipment.Restrictions restriction in item.restrictions)
                        {
                            foreach (K2CustomEquipment.CustomWeapon itemRequirement in K2AllWeapons)
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
                    foreach (K2CustomEquipment.Restrictions restriction in item.restrictions)
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
