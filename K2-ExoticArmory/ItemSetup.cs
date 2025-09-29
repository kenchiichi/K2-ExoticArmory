using Modding;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace K2ExoticArmory
{
    public class ItemSetup
    {
        public List<K2CustomWeapon> K2AllWeapons = new List<K2CustomWeapon>();

        public List<K2CustomApparel> K2AllApparel = new List<K2CustomApparel>();
        public void WeaponSerialStreamReader(ModManifest manifest, string xmlpath, List<K2CustomWeapon> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Weapon> k2Weapons = Deserialize<List<K2Weapon>>(reader.ReadToEnd());

            foreach (K2Weapon k2Weapon in k2Weapons)
            {
                var item = k2Weapon.CustomInitialize(manifest);
                list.Add(item);
            }
        }
        public void ApparelSerialStreamReader(ModManifest manifest, string xmlpath, List<K2CustomApparel> list)
        {
            using StreamReader reader = new StreamReader(Path.Combine(manifest.ModPath, xmlpath));
            List<K2Apparel> k2Apparels = Deserialize<List<K2Apparel>>(reader.ReadToEnd());

            foreach (K2Apparel k2Apparel in k2Apparels)
            {
                var item = k2Apparel.CustomInitialize(manifest);
                list.Add(item);
            }
        }
        private static T Deserialize<T>(string xmlString)
        {
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xmlString);
            return (T)serializer.Deserialize(reader);
        }
    }
}
