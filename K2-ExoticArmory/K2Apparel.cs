using ANToolkit.ResourceManagement;
using Asuna.Items;
using Modding;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace K2ExoticArmory
{
    public class K2Apparel : K2Equipment
    {
        private K2CustomEquipment.CustomApparel _instance;

        private ModManifest _manifest;
        public K2CustomEquipment.CustomApparel CustomInitialize(ModManifest manifest)
        {
            K2CustomEquipment.CustomApparel Apparel = ScriptableObject.CreateInstance<K2CustomEquipment.CustomApparel>();

            ANResourceSprite previewImage = manifest.SpriteResolver.ResolveAsResource(PreviewImage);

            _manifest = manifest;

            _instance = Apparel;

            if (StatModifierInfos != null)
            {
                Apparel.StatModifierInfos = StatModifierInfos;
                typeof(Equipment)
                    .GetField("_dynamicStatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(Apparel, StatModifierInfos);
                typeof(Equipment)
                    .GetField("StatModifiers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(Apparel, StatModifierInfos);
            }

            if (!Item.All.ContainsKey(Name.ToLower()))
            {
                Item.All.Add(Name.ToLower(), _instance);
            }
            Apparel.DisplaySpriteResource = previewImage;
            Apparel.LocationCoordinates = LocationCoordinates;
            Apparel.Name = Name;
            Apparel.Slots.AddRange(Slots.Select((string x) => (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), x)));
            Apparel.Category = (ItemCategory)Apparel.K2ItemCategory;
            Apparel.Description = Description;
            Apparel.Price = Price;
            Apparel.StatRequirements = StatRequirements;
            Apparel.restrictions = restrictions;

            AddAbilitiesToEquipment(Apparel, _manifest);

            return Apparel;
        }
    }
}
