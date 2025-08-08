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
        public void OnLevelChanged(string oldLevel, string newLevel) {
            StartCombat();
        }
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
        public void StartCombat()
        {
            // Note, that shouldClone: true is important here, so that each enemy has their own health pool instead of a shared one.
            var soldier1 = CharacterHandler.Create(Character.Get("Soldier"), shouldClone: true);
            var soldier2 = CharacterHandler.Create(Character.Get("Soldier"), shouldClone: true);
            var soldier3 = CharacterHandler.Create(Character.Get("Soldier"), shouldClone: true);

            var combatData = new Asuna.NewCombat.CombatData()
            {
                InvolvedCharacters = new List<CharacterHandler>()
                {
                    soldier1, soldier2, soldier3, Character.Get("Jenna").Handlers.First()
                },
                PathfindingData = new ANToolkit.Pathfinding.ANPathfindingData()
                {
                    Center = new Vector3(2f, 10f),
                    // This defines the size that the involved characters can traverse
                    Width = 50,
                    Depth = 50
                },
                EndCondition = EndCondition.DefeatEnemies,
                InvolveParty = true
            };

            var combatGameObject = new GameObject();
            var starter = combatGameObject.AddComponent<Asuna.NewCombat.CombatStarter>();
            starter.data = combatData;
            // This instantly starts the combat
            starter.Fire();

            Asuna.NewCombat.Combat.OnCombatEnd.AddSingleTimeListener(result =>
            {
                if (result == Asuna.NewCombat.CombatResult.Victory)
                {
                    Debug.Log("Player won the fight!");
                }
            });
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
}
