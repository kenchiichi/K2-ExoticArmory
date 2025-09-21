using ANToolkit.ResourceManagement;
using Asuna.Items;
using Modding;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class K2Apparel : K2Items.K2Equipment
    {
        private K2Items.K2Apparel _instance;

        private ModManifest _manifest;
        public K2Items.K2Apparel CustomInitialize(ModManifest manifest)
        {
            K2Items.K2Apparel apparel = ScriptableObject.CreateInstance<K2Items.K2Apparel>();

            ANResourceSprite previewImage = manifest.SpriteResolver.ResolveAsResource(PreviewImage);

            _manifest = manifest;

            _instance = apparel;

            if (StatModifierInfos != null)
            {
                apparel.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(apparel, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(apparel, StatModifierInfos);
            }

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }

            if (questModifiers != null)
            {
                apparel.questModifiers = questModifiers;
            }
            apparel.DisplaySpriteResource = previewImage;
            apparel.LocationCoordinates = LocationCoordinates;
            apparel.Name = Name;
            apparel.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            apparel.Category = (ItemCategory)apparel.K2ItemCategory;
            apparel.Description = Description;
            apparel.Price = Price;
            apparel.StatRequirements = StatRequirements;
            apparel.restrictions = restrictions;
            apparel.questModifiers = questModifiers;

            AddAbilitiesToEquipment(apparel, _manifest);

            return apparel;
        }
    }
}
