using ANToolkit;
using ANToolkit.Debugging;
using ANToolkit.ResourceManagement;
using ANToolkit.Save;
using ANToolkit.UI;
using ANToolkit.UI;
using ANToolkit.UI.Themes;
using Asuna.CharManagement;
using Asuna.Combat;
using Asuna.Dialogues;
using Asuna.Items;
using Asuna.NewCombat;
using Asuna.UI;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using Unity;
using UnityEngine;

using System.Collections.Generic;
using System.IO;
using ANToolkit.ResourceManagement;
using UnityEngine;

namespace K2ExoticArmory
{
    public class K2ExoticArmory : ITCMod
    {
        public ModManifest modManifest;
        public List<string> NewItemNames = new List<string>();
        private Weapon _instance;
        public List<StatModifierInfo> StatModifierInfos;

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnFrame(float deltaTime) { }
        public void OnLevelChanged(string oldLevel, string newLevel) { }
        public void OnLineStarted(DialogueLine line) { }
        public static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public void OnModUnLoaded()
        {
            foreach (string itemName in NewItemNames)
            {
                Debug.Log("Remove " + itemName);
                Item.All.Remove(itemName.ToLower());
            }
        }
        public void OnModLoaded(ModManifest manifest)
        {
            modManifest = manifest;
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\ItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize(manifest.SpriteResolver);
                    Weapon item = Weapon.CreateWeapon(customEquipment.Name);
                    NewItemNames.Add(item.Name);

                }
            }
        }
        public class Weapon : Asuna.Items.Weapon
        {
            public List<StatModifierInfo> StatModifierInfos;
            public static Weapon CreateWeapon(string name)
            {
                Item item = Create(name);
                if (item is Weapon)
                {
                    typeof(Equipment)
                       .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                       .SetValue(item, (item as Weapon).StatModifierInfos);
                    typeof(Equipment)
                        .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(item, (item as Weapon).StatModifierInfos);
                    return (item as Weapon);
                }
                return null;
            }
        }

        public class CustomEquipment : ModEquipment
        {
            public string Description;
            public List<StatModifierInfo> StatModifierInfos;
            private Weapon _instance;

            public void CustomInitialize(ModSpriteResolver modSpriteResolver)
            {

                Weapon weapon = ScriptableObject.CreateInstance<Weapon>();
                weapon.Name = Name;
                weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                weapon.Category = ItemCategory.Weapon;
                weapon.Description = Description;
                ANResourceSprite aNResourceSprite = modSpriteResolver.ResolveAsResource(PreviewImage);
                weapon.DisplaySpriteResource = aNResourceSprite;
                PopupData popupData = new PopupData
                {
                    Image = aNResourceSprite,
                    YesLabel = "Ok",
                    NoLabel = null
                };
                Popup.CreateBanner(popupData);
                weapon.StatModifierInfos = StatModifierInfos;
                // ability Abl_Akimbo
                // ANToolkit.ScriptableManagement.ScriptableManager.GetAll<Asuna.NewCombat.Ability>()


                _instance = weapon;
                Debug.Log(weapon.DisplaySpriteResource);
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
                
                if (!Item.All.ContainsKey(Name.ToLower()))
                {
                    Item.All.Add(Name.ToLower(), _instance);
                }
                else
                {
                    Debug.LogError("Did not register Item \"" + Name + "\", because an item with the same name already exists.");
                }
            }
        }
    }
}
