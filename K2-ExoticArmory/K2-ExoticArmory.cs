using ANToolkit;
using ANToolkit.Debugging;
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
        public void OnModUnLoaded()
        {
            foreach (string itemName in NewItemNames)
            {
                Debug.Log("Remove " + itemName);
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

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize(manifest.SpriteResolver);
                    CustomWeapon item = CustomWeapon.CreateWeapon(customEquipment.Name);
                    NewItemNames.Add(item.Name);

                }
            }
        }
        public class CustomWeapon : Asuna.Items.Weapon
        {
            public List<StatModifierInfo> StatModifierInfos;
            public static CustomWeapon CreateWeapon(string name)
            {
                Item item = Create(name);
                if (item is CustomWeapon)
                {
                    typeof(Equipment)
                       .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                       .SetValue(item, (item as CustomWeapon).StatModifierInfos);
                    typeof(Equipment)
                        .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(item, (item as CustomWeapon).StatModifierInfos);
                    return (item as CustomWeapon);
                }
                return null;
            }
        }

        public class CustomEquipment
        {
            public string Name;
            public string PreviewImage;
            public string Description;
            public string AbilityTooltip;
            public string AbilityID;
            public List<string> Slots;
            public List<StatModifierInfo> StatModifierInfos;
            private CustomWeapon _instance;

            public void CustomInitialize(ModSpriteResolver modSpriteResolver)
            {
                CustomWeapon weapon = ScriptableObject.CreateInstance<CustomWeapon>();
                Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(AbilityID);
                ANResourceSprite aNResourceSprite = modSpriteResolver.ResolveAsResource(PreviewImage);
                
                ability.Tooltip = AbilityTooltip;

                aNResourceSprite.MOD_ONLY_USE = true;

                weapon.Name = Name;
                weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                weapon.Category = ItemCategory.Weapon;
                weapon.Description = Description;
                weapon.AddAbility(ability, ability.RestraintID);
                weapon.StatModifierInfos = StatModifierInfos;

                weapon.DisplaySprite = aNResourceSprite;
                weapon.DisplaySpriteResource = aNResourceSprite;

                _instance = weapon;
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
