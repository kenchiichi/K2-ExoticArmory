using ANToolkit.Debugging;
using ANToolkit.UI;
using Asuna.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace K2ExoticArmory
{
    public class DetectMenus
    {
        private bool GameObjNotActive(List<String> objects)
        {
            bool objectNotActive = true;
            foreach (String obj in objects)
            {
                if (!(GameObject.Find(obj) == null))
                {
                    objectNotActive = false; break;
                }
            }
            return objectNotActive;
        }
        public bool DetectMenu()
        {
            bool menuNotOpen = false;
            if (                                             // Checks if the game's state is not in the following:
                GameObjNotActive(new List<string> {
                    "ModMenu(Clone)",                        // ModMenu
                    "ShopUICanvas",                          // Clothing Shop Menu
                    "DialogueCanvas",                        // Dialogue
                    "Gallery_Scenes",                        // Gallery Scenes Tab
                    "Gallery_CharacterViewer",               // Gallery Character Viewer
                    "Gallery_Character",                     // Gallery Character Tab
                    "Gallery_ImageViewer",                   // Gallery Image Viewer
                    "Gallery_Images",                        // Gallery Image Tab
                    "Gallery_Animations",                    // Gallery Animations Tab and Viewer
                    "MatchMinigame(Clone)",                  // Hacking Minigame
                    "Wrestling Minigame Prefab",             // Wrestling Minigame
                    "WorkoutMinigame",                       // Workout Minigame
                    "DancingMinigame",                       // Dancing Minigame
                    "BarMixing",                             // Bar Mixing Minigame
                    "Jenna Gloryhole",                       // Gloryhole Minigame
                    "SDT Minigame",                          // Peitho Blowjob Minigame
                    "PeithOS Computer UI",                   // Peitho Blowjob Minigame Upgrade shop Menu
                    "SDT Selector",                          // Peitho Blowjob Minigame Upgrade selector Menu
                    "Slave Training UI",                     // Peitho Slave Training Minigame Menu
                }) &&
                !MenuManager.IsPaused &&                     // Pause Menu
                !TabMenu.IsOpen &&                           // Phone Menu 
                !ConsoleUI.IsOpen &&                         // Dev Console
                MenuManager.InGame                           // TitleScreen
                )
            {
                if (Asuna.Minimap.MinimapPlayerIcon.Instance != null) // Checks if the Minimap PlayerIcon exists
                {
                    if (!Asuna.Minimap.MinimapUI.Instance.Maximized) // Checks if the Minimap is fullscreened
                    {
                        menuNotOpen = true;
                    }
                }
                else
                {
                    menuNotOpen = true;
                }
            }
            return menuNotOpen;
        }
    }
}
