using ANToolkit.Controllers;
using ANToolkit.Debugging;
using ANToolkit.UI;
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

        private StartupListeners listener = new StartupListeners();

        public List<K2Items.K2Weapon> K2AllWeapons = new List<K2Items.K2Weapon>();

        public List<K2Items.K2Apparel> K2AllApparel = new List<K2Items.K2Apparel>();

        public List<Equipment> K2ItemList = new List<Equipment>();

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnFrame(float deltaTime) { }
        public void OnModUnLoaded()
        {
            if (MenuManager.InGame)
            {
                foreach (var item in K2ItemList)
                {
                    Item.All.Remove(item.Name.ToLower());
                    RemoveItemFromJenna(item.Name);
                }
                Item.GenerateErrorDialogue(Character.Player, "I should remember to check to see if I have weapons to defend myself.", "Think");
            }
            UnityEngine.Events.UnityEvent unityEvent = new UnityEngine.Events.UnityEvent();
            unityEvent.RemoveAllListeners();
            Debug.Log("K2-ExoticArmory uninstalled");
        }
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            K2Weapon k2Weapon = new K2Weapon();
            k2Weapon.LoadWorldItems(newLevel, K2AllWeapons);

            K2Apparel k2Apparel = new K2Apparel();
            k2Apparel.LoadWorldItems(newLevel, K2AllApparel);

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
            Debug.Log("K2-ExoticArmory installed");

            _manifest = manifest;

            listener.AddSpriteListener();

            listener.AddRequirementListener(K2AllApparel, K2AllWeapons);

            WeaponSerialStreamReader(manifest, "data\\WeaponData.xml", K2AllWeapons);

            ApparelSerialStreamReader(manifest, "data\\ApparelData.xml", K2AllApparel);

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
                GiveItemToJenna();
            });
        }

        public void RemoveItemFromJenna(string itemName)
        {
            foreach (Item EquipmentSlot in Character.Player.EquippedItems.GetAll<Item>())
            {
                if (EquipmentSlot.Name.ToLower() == itemName.ToLower())
                {
                    Character.Get("Jenna").EquippedItems.Remove(itemName);
                }
            }
            foreach (Item inventoryItem in Character.Player.Inventory.GetAll<Item>())
            {
                if (inventoryItem.Name.ToLower() == itemName.ToLower())
                {
                    Character.Get("Jenna").Inventory.Remove(itemName);
                }
            }
        }
        public void GiveItemToJenna(string itemName = "AllItems")
        {
            foreach (var weapon in K2AllWeapons)
            {
                if (weapon.Name.ToLower() == itemName.ToLower() || itemName == "AllItems")
                {
                    GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, weapon);
                }
            }
            foreach (var apparel in K2AllApparel)
            {
                if (apparel.Name.ToLower() == itemName.ToLower() || itemName == "AllItems")
                {
                    GiveItems.GiveToCharacter(Character.Get("Jenna"), false, false, apparel);
                }
            }
        }
        private void WeaponSerialStreamReader(ModManifest manifest, string xmlpath, List<K2Items.K2Weapon> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Weapon> k2Weapons = Deserialize<List<K2Weapon>>(reader.ReadToEnd());

            foreach (K2Weapon k2Weapon in k2Weapons)
            {
                var item = k2Weapon.CustomInitialize(manifest);
                list.Add(item);
                K2ItemList.Add(item);
            }
        }
        private void ApparelSerialStreamReader(ModManifest manifest, string xmlpath, List<K2Items.K2Apparel> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Apparel> k2Apparels = Deserialize<List<K2Apparel>>(reader.ReadToEnd());

            foreach (K2Apparel k2Apparel in k2Apparels)
            {
                var item = k2Apparel.CustomInitialize(manifest);
                list.Add(item);
                K2ItemList.Add(item);
            }
        }
        private static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xmlString);
            return (T)serializer.Deserialize(reader);
        }
    }
}
