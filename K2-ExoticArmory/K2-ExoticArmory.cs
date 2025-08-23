using Asuna.CharManagement;
using Asuna.Combat;
using Asuna.Dialogues;
using Asuna.Items;
using Asuna.NewCombat;
using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace K2ExoticArmory
{
    public class K2ExoticArmory : ITCMod
    {
        public List<string> NewItemNames = new List<string>();
        private CustomWeapons _instance;
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
        public void OnModLoaded(ModManifest manifest)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, "data\\ItemData.xml")))
            {
                string xml = reader.ReadToEnd();
                List<CustomEquipment> customEquipments = Deserialize<List<CustomEquipment>>(xml);

                foreach (CustomEquipment customEquipment in customEquipments)
                {
                    customEquipment.CustomInitialize();
                    CustomWeapons item = CustomWeapons.CreateWeapon(customEquipment.Name);
                    NewItemNames.Add(item.Name);

                }
            }
        }
        public class CustomWeapons : Asuna.Items.Weapon
        {
            public List<StatModifierInfo> StatModifierInfos;
            public static CustomWeapons CreateWeapon(string name)
            {
                Item item = Create(name);
                if (item is CustomWeapons)
                {
                    typeof(Equipment)
                       .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                       .SetValue(item, (item as CustomWeapons).StatModifierInfos);
                    typeof(Equipment)
                        .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(item, (item as CustomWeapons).StatModifierInfos);
                    return (item as CustomWeapons);
                }
                return null;
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

        public class CustomEquipment : ModEquipment
        {
            public string Description;
            public List<StatModifierInfo> StatModifierInfos;
            private CustomWeapons _instance;
            public void CustomInitialize()
            {
                CustomWeapons weapon = ScriptableObject.CreateInstance<CustomWeapons>();
                weapon.Name = Name;
                weapon.AttackVFXType = AttackVFXType.EnergyGunBurst;
                weapon.Category = ItemCategory.Weapon;
                weapon.Description = Description;
                weapon.StatModifierInfos = StatModifierInfos;
                weapon.SetData(PreviewImage, "true");
                Ability ability = new Ability();
                ability.ID = ("akimbo_id");
                ability.name = "Akimbo_Name";
                ability.Tooltip = "This is a test tooltip";
                ability.Data.Print();
                ability.DisplayName = "this is the display name!";
                ability.RestraintID = "akimbo_id";
                ability.Owner = new Character();





                Debug.Log(ability.ID);
                Debug.Log(ability.name);
                Debug.Log(ability.Tooltip);
                Debug.Log(ability.DisplayName);
                Debug.Log(ability.RestraintID);



                weapon.SetAbility(ability, ability.RestraintID, true);
                weapon.AddAbility(ability, "akimbo_id");
                weapon.Abilities.Add(ability);


                //weapon.;
                //weapon.Data.Add(Asuna.NewCombat.CombatStats.Range, 2);
                //Stat.Get(Asuna.NewCombat.);
                weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, weapon.StatModifierInfos);
                _instance = weapon;
                
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
