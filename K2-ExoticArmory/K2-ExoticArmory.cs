using ANToolkit.Controllers;
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

        public List<StatModifierInfo> StatModifierInfos;

        public CustomWeapon customWeapon;

        public ItemVendor vendor;
        public void OnDialogueStarted(Dialogue dialogue)
        {
            return;
        }
        public void OnLineStarted(DialogueLine line)
        {
            return;
        }
        public void OnFrame(float deltaTime)
        {
            if (Input.GetKeyDown("i") && detectMenus.DetectMenu())
            {
                //vendor.Catalogue.OpenShop();
            }
        }
        public void OnModUnLoaded()
        {
            foreach (string itemName in NewItemNames)
            {
                Item.All.Remove(itemName.ToLower());
            }
        }

        public DetectMenus detectMenus = new DetectMenus();
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            foreach (CustomWeapon item in EarnableWeapons)
            {
                foreach (MapCoordinate location in item.LocationCoordinates)
                {
                    //Debug.Log(item.Name + "\n" + location.MapName + ": x = " + location.xCoordinate + ", y = " + location.yCoordinate + " ");
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
            Item.OnItemCloned.AddListener((newItem, oldItem) =>
            {
                newItem.DisplaySprite = oldItem.DisplaySprite;
                newItem.DisplaySpriteResource = oldItem.DisplaySpriteResource;
            });

            List<ShopItemInfo> shopItems = new List<ShopItemInfo>();
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\StoreItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    var item = customEquipment.CustomInitialize(manifest);
                    NewItemNames.Add(item.Name);
                    shopItems.Add(
                        new ShopItemInfo()
                        {
                            Item = item,
                            Cost = item.Price,
                        }
                    );
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
            ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();
            catalogue.Items = shopItems;

            vendor = new ItemVendor()
            {
                Catalogue = catalogue,
            };
        }
        public static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
