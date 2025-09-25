using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using Asuna.NewMissions;
using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace K2Items
{
    public class K2Equipment
    {
        public string Name;

        public int Price;

        public string PreviewImage;

        public string Description;

        public List<string> Slots;

        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<StatModifierInfo> StatRequirements;

        public List<Restrictions> restrictions;

        public QuestModifiers questModifiers;

        public MapCoordinate LocationCoordinates;

        public void AddAbilitiesToEquipment(Equipment equipment, ModManifest manifest)
        {
            if (CustomAbilityItems != null)
            {
                foreach (var item in CustomAbilityItems)
                {
                    Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(item.AbilityID).Clone();
                    ability.name = item.AbilityName.Replace(' ', '_');
                    ability.Tooltip = item.AbilityTooltip;
                    ability.DisplayName = item.AbilityName;
                    ability.DisplaySprite = manifest.SpriteResolver.ResolveAsResource(item.DisplaySprite);
                    ability.EnergyCost = item.AbilityEnergyCost;
                    ability.CooldownOnUse = item.AbilityCooldown;

                    ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);

                    equipment.AddAbility(ability, "Ability");
                }
            }
        }

        public void CreateSearchQuest(string DisplayName, string Description, string name, bool Repeatable, string TaskDisplay)
        {
            var moddedMission = ScriptableObject.CreateInstance<NewMission>();
            moddedMission.DisplayName = DisplayName;
            moddedMission.Description = Description;
            moddedMission.name = name + "_Quest";
            moddedMission.Repeatable = Repeatable;

            var moddedTask1 = ScriptableObject.CreateInstance<NewTask>();
            moddedTask1.DefaultDisplayName = TaskDisplay;
            //moddedTask1.TargetCharacter = Character.Get("Klaus");
            moddedTask1.name = name + "_Task";

            MissionContainer.AddMissionToLookup(moddedMission);
            MissionContainer.AddTaskToLookup(moddedTask1);
        }
    }
    public class K2Weapon : Asuna.Items.Weapon
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

        public QuestModifiers questModifiers;

        public MapCoordinate LocationCoordinates;

        public CustomVFX customVFX;

        public int K2ItemCategory = 2;

        public int Price;
    }
    public class K2Apparel : Asuna.Items.Apparel
    {
        public List<StatModifierInfo> StatModifierInfos;

        public List<CustomAbility> CustomAbilityItems;

        public List<Restrictions> restrictions;

        public QuestModifiers questModifiers;

        public MapCoordinate LocationCoordinates;

        public int K2ItemCategory = 1;

        public int Price;
    }
    public class MapCoordinate
    {
        public string MapName = "";

        public double xCoordinate;

        public double yCoordinate;

        public string PrerequisiteQuestWeapon = "";
    }
    public class CustomAbility
    {
        public string AbilityID = "";

        public string AbilityName = "";

        public string AbilityTooltip = "";

        public string DisplaySprite = "";

        public int AbilityCooldown = 0;

        public int AbilityEnergyCost = 0;
    }
    public class CustomVFX
    {
        public int BurstCount;

        public string WeaponAttackVFXType = "UnarmedMelee";

        public string wavAudioClip = "";

        public string WeaponAttackVFXSprite = "";
    }
    public class Restrictions
    {
        public string RequiredItemEquipped = "";

        public string PrerequisiteQuestWeapon = "";
    }
    public class QuestModifiers
    {
        public string DisplayName = "";

        public string Description = "";

        public string TaskDisplay = "";

        public string name = "";

        public string next = "";

        public bool Repeatable = false;

        public bool BaseWeapon = false;
    }
}
