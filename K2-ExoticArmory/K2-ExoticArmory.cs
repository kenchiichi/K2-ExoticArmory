using ANToolkit.Controllers;
using ANToolkit.Debugging;
using ANToolkit.UI;
using Asuna.CharManagement;
using Asuna.Dialogues;
using Asuna.Items;
using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace K2ExoticArmory
{
    public class K2ExoticArmory : ITCMod
    {
        public ItemVendor vendor;

        private ModManifest _manifest;

        private readonly StartupListeners listener = new StartupListeners();

        public K2Equipment K2Equipment = new K2Equipment();

        public ItemSetup itemSetup = new ItemSetup();

        public void OnDialogueStarted(Dialogue dialogue) { }
        public void OnLineStarted(DialogueLine line) { }
        public void OnFrame(float deltaTime) { }
        public void OnModUnLoaded()
        {
            if (MenuManager.InGame)
            {
                foreach (var item in itemSetup.K2AllWeapons)
                {
                    Item.All.Remove(item.Name.ToLower());
                    K2Equipment.RemoveItemFromJenna(item.Name);
                }
                foreach (var item in itemSetup.K2AllApparel)
                {
                    Item.All.Remove(item.Name.ToLower());
                    K2Equipment.RemoveItemFromJenna(item.Name);
                }
                Item.GenerateErrorDialogue(Character.Get("Jenna"), "I should remember to check to see if I have weapons to defend myself.", "Think");
                Character.Get("Jenna").GetStat("stat_hitpoints").BaseMax = 1;
            }
            Debug.Log("K2-ExoticArmory uninstalled");
        }
        public void OnLevelChanged(string oldLevel, string newLevel)
        {
            K2Weapon k2Weapon = new K2Weapon();
            k2Weapon.LoadWorldItems(newLevel, itemSetup.K2AllWeapons);

            K2Apparel k2Apparel = new K2Apparel();
            k2Apparel.LoadWorldItems(newLevel, itemSetup.K2AllApparel);

            if (newLevel == "Motel_UpperFloor")
            {
                double xPos = -6.00;
                double yPos = 16.30;

                GameObject adaNPC = new GameObject();
                adaNPC.transform.position = new Vector3((float)xPos, (float)yPos);

                BoxCollider adaNPCCollider = adaNPC.AddComponent<BoxCollider>();
                adaNPCCollider.size = new Vector3(1f, 2f);

                GameObject adaNPCSprite = new GameObject();
                adaNPCSprite.transform.position = new Vector3((float)(xPos - .7), (float)(yPos - .65));

                SpriteRenderer adaNPCSpriteRenderer = adaNPCSprite.AddComponent<SpriteRenderer>();
                adaNPCSpriteRenderer.sprite = _manifest.SpriteResolver.ResolveAsResource("assets\\sprites\\npc\\ada_overworld.png");
                adaNPCSpriteRenderer.transform.localScale = new Vector3(1f, 1f);

                Interactable adaNPCInteractable = adaNPC.AddComponent<Interactable>();
                adaNPCInteractable.TypeOfInteraction = InteractionType.Talk;
                adaNPCInteractable.OnInteracted.AddListener(x =>
                {
                    vendor.Catalogue.OpenShop();
                });
            }
        }

        public void OnModLoaded(ModManifest manifest)
        {
            Debug.Log("K2-ExoticArmory installed");

            _manifest = manifest;

            itemSetup.WeaponSerialStreamReader(manifest, "data\\WeaponData.xml", itemSetup.K2AllWeapons);

            itemSetup.ApparelSerialStreamReader(manifest, "data\\ApparelData.xml", itemSetup.K2AllApparel);

            List<ShopItemInfo> shopItems = new List<ShopItemInfo>();

            ItemShopCatalogue catalogue = ScriptableObject.CreateInstance<ItemShopCatalogue>();

            foreach (var item in itemSetup.K2AllApparel)
            {
                if (item.Price > 0)
                {
                    shopItems.Add(
                    new ShopItemInfo()
                    {
                        Item = item,
                        Cost = item.Price,
                    }
                );
                }
            }
            foreach (var item in itemSetup.K2AllWeapons)
            {
                if (item.Price > 0)
                {
                    shopItems.Add(
                        new ShopItemInfo()
                        {
                            Item = item,
                            Cost = item.Price,
                        }
                    );
                }
            }
            catalogue.Items = shopItems;
            vendor = new ItemVendor()
            {
                Catalogue = catalogue,
            };
            ConCommand.Add("GiveArmory", delegate
            {
                K2Equipment.GiveItemToJenna(itemSetup.K2AllWeapons, itemSetup.K2AllApparel);
            });

            listener.AddSpriteListener();

            listener.EquipmentListeners(itemSetup.K2AllApparel, itemSetup.K2AllWeapons);
        }
    }
}
