using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Weapons;
using StardewValley.Tools;

namespace Dopamine.StardewValley
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public static IManifest Mod { get; private set; } = null!;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Data/Weapons"))
            {
                e.Edit(asset => { 
                    var dict = asset.AsDictionary<string, WeaponData>();
                    dict.Data["Blood_Fang"] = new WeaponData
                    {
                        Name = "Blood Fang",
                        Description = "A fang that has absorbed the blood of its victims.",
                        Type = 0,
                        Speed = 4,
                        Knockback = 1.5f,
                        CritChance = 0.05f,
                        CritMultiplier = 1.5f,
                    };
                });
                //DateTime dateTime = DateTime.Now.Day;
            }
        }
    }
}