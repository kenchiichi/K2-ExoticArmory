using ANToolkit.PlayMakerExtension;
using ANToolkit.ResourceManagement;
using Asuna.CharManagement;
using Asuna.Items;
using Asuna.NewCombat;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace K2ExoticArmory
{
    public class CustomEquipment : ModEquipment
    {
        public string Description;
        public string AbilityTooltip;
        public string AbilityID;
        public string AbilityName;
        public int Price;
        public List<StatModifierInfo> StatModifierInfos;
        private CustomWeapon _instance;
        public AttackVFXType WeaponAttackVFXType;
        //public bool IsLocked = false;
        //public List<StatModifierInfo> StatRequirements;
        //public string Sound;
        public CustomWeapon CustomInitialize(ModSpriteResolver modSpriteResolver)
        {
            var weapon = ScriptableObject.CreateInstance<CustomWeapon>();
            if (AbilityID != null)
            {
                Ability ability = ANToolkit.ScriptableManagement.ScriptableManager.Get<Asuna.NewCombat.Ability>(AbilityID).Clone();
                ability.Tooltip = AbilityTooltip;
                ability.DisplayName = AbilityName;
                ANToolkit.ScriptableManagement.ScriptableManager.Add(ability);
                weapon.AddAbility(ability, ability.RestraintID);
            }
            if (StatModifierInfos != null)
            {
                //weapon.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(weapon, StatModifierInfos);
            }

            ANResourceSprite aNResourceSprite = modSpriteResolver.ResolveAsResource(PreviewImage);
            weapon.DisplaySpriteResource = aNResourceSprite;

            //weapon.DurabilityDisplayLayers.AddRange(Sprites.Select((ModEquipmentSprite x) => x.Get(modSpriteResolver)));

            weapon.Name = Name;
            weapon.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            weapon.Category = ItemCategory.Weapon;
            weapon.Description = Description;
            weapon.Price = Price;
            //AudioClip sound = Resources.Load<AudioClip>(Sound);
            weapon.AttackVFXType = WeaponAttackVFXType;
            //weapon.IsLocked = IsLocked;

            _instance = weapon;

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }
            else
            {
                Debug.LogError("Did not register Item \"" + Name + "\", because an item with the same name already exists.");
            }

            //AddAbility(weapon, (Sprite)aNResourceSprite);

            return weapon;
        }

        private void AddAbility(CustomWeapon weapon, Sprite sprite)
        {
            // Create your VFX gameobject with a spriterenderer
            var vfxGameObject = new GameObject("VFX_Custom");
            var sfxGameObject = new GameObject("SFX_Custom");
            var vfxSpriteRenderer = vfxGameObject.AddComponent<SpriteRenderer>();
            //var sfxClipLoader = sfxGameObject.GetComponent<AudioSource>();
            vfxSpriteRenderer.sprite = sprite;
            //sfxClipLoader.clip = sound; // FIXME



            // Get the attack ability and clone it
            var fakeAttackAbility = ANToolkit.ScriptableManagement.ScriptableManager.Get<Ability>("Abl_Attack").Clone();
            ANToolkit.ScriptableManagement.ScriptableManager.Add(fakeAttackAbility);

            // get the logic from the Abl_Attack FIXME
            var logicTemplate = typeof(Ability)
                .GetField("logic", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(fakeAttackAbility) as FsmTemplate;

            // make a copy of it
            logicTemplate = UnityEngine.Object.Instantiate(logicTemplate);

            // edit the playmaker action that does the spawning of the visual effects
            var energyGunBurstState = logicTemplate.fsm.States.First(x => x.Name == "EnergyGunBurst");
            Debug.Log("Is initialized? " + energyGunBurstState.IsInitialized);


            energyGunBurstState.Fsm.Reinitialize();

            var combatVFX = energyGunBurstState.Actions[0] as PMA_CombatVisualEffect;
            Debug.Log("combatVFX");
            var combatSFX = energyGunBurstState.Actions[0] as PMA_PlaySound;
            Debug.Log("combatSFX");
            combatVFX.Prefab = vfxGameObject;
            Debug.Log("combatVFX.Prefab");
            combatSFX.ClipToPlay = sfxGameObject;
            Debug.Log("combatSFX.ClipToPlay");

            // apply the custom logic
            typeof(Equipment)
                .GetField("logic", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(weapon, logicTemplate);
        }
    }

}
