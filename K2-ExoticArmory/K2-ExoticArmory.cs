using ANToolkit;
using ANToolkit.Cam;
using ANToolkit.Debugging;
using ANToolkit.Effects;
using ANToolkit.ResourceManagement;
using ANToolkit.Save;
using ANToolkit.UI;
using ANToolkit.UI.Themes;
using ANToolkit.Utility;
using Asuna.CharManagement;
using Asuna.Combat;
using Asuna.Dialogues;
using Asuna.Items;
using Asuna.NewCombat;
using Asuna.UI;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Unity;
using UnityEngine;
using static DynamicBoneColliderBase;
using CombatCharacter = Asuna.NewCombat.CombatCharacter;

namespace K2ExoticArmory
{
    public class K2ExoticArmory : ITCMod
    {
        public ModManifest modManifest;
        public List<string> NewItemNames = new List<string>();
        private Weapon _instance;
        public List<StatModifierInfo> StatModifierInfos;
        public ItemVendor vendor;
        public K2SimpleZoom.K2SZ k2SZ = new K2SimpleZoom.K2SZ(); 

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnFrame(float deltaTime) 
        {
            if (Input.GetKeyDown("i") && k2SZ.DetectMenu())
            {
                vendor.Catalogue.OpenShop();
            }
        }
        public void OnLevelChanged(string oldLevel, string newLevel) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnModUnLoaded()
        {
            foreach (string itemName in NewItemNames)
            {
                Item.All.Remove(itemName.ToLower());
            }
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
        public void OnModLoaded(ModManifest manifest)
        {
            modManifest = manifest;
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\ItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);
                List<ShopItemInfo> shopItems = new List<ShopItemInfo>();

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize(manifest.SpriteResolver);
                    CustomWeapon item = CustomWeapon.CreateWeapon(customEquipment.Name);
                    NewItemNames.Add(item.Name);

                    shopItems.Add(
                        new ShopItemInfo()
                        {
                            Item = item,
                            Cost = item.Price,
                        }
                    );
                }
                ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();
                catalogue.Items = shopItems;


                vendor = new ItemVendor()
                {
                    Catalogue = catalogue,
                };
            }
        }
    }
}
