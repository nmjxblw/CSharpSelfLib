using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SpaceShared.APIs;

public interface IFashionSenseApi
{
	public enum Type
	{
		Unknown,
		Hair,
		Accessory,
		[Obsolete("No longer maintained. Use Accessory instead.")]
		AccessorySecondary,
		[Obsolete("No longer maintained. Use Accessory instead.")]
		AccessoryTertiary,
		Hat,
		Shirt,
		Pants,
		Sleeves,
		Shoes,
		Player
	}

	public record RawTextureData(int Width, int Height, Color[] Data) : IRawTextureData;

	public interface IDrawTool
	{
		Farmer Farmer { get; init; }

		SpriteBatch SpriteBatch { get; init; }

		FarmerRenderer FarmerRenderer { get; init; }

		Texture2D BaseTexture { get; init; }

		Rectangle FarmerSourceRectangle { get; init; }

		AnimationFrame AnimationFrame { get; init; }

		bool IsDrawingForUI { get; init; }

		Color OverrideColor { get; init; }

		Color AppearanceColor { get; set; }

		Vector2 Position { get; init; }

		Vector2 Origin { get; init; }

		Vector2 PositionOffset { get; init; }

		int FacingDirection { get; init; }

		int CurrentFrame { get; init; }

		float Scale { get; init; }

		float Rotation { get; init; }

		float LayerDepthSnapshot { get; set; }
	}

	event EventHandler SetSpriteDirtyTriggered;

	KeyValuePair<bool, string> SetAppearance(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);

	KeyValuePair<bool, string> SetAccessorySlot(string accessoryId, int accessorySlot);

	KeyValuePair<bool, string> SetAppearanceColor(Type appearanceType, Color color, IManifest callerManifest);

	KeyValuePair<bool, string> SetAccessoryColor(Color color, int accessorySlot);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
	KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
	KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
	KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
	KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

	KeyValuePair<bool, string> ClearAppearance(Type appearanceType, IManifest callerManifest);

	KeyValuePair<bool, string> ClearAccessorySlot(int accessorySlot, IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
	KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
	KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
	KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest);

	[Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
	KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest);

	KeyValuePair<bool, string> GetCurrentAppearanceId(Type appearanceType, Farmer target = null);

	KeyValuePair<bool, Color> GetAppearanceColor(Type appearanceType, Farmer target = null);

	KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, bool getOriginalTexture = false);

	KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(string appearanceId, bool getOriginalTexture = false);

	KeyValuePair<bool, string> SetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);

	KeyValuePair<bool, string> SetAppearanceTexture(string appearanceId, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);

	KeyValuePair<bool, string> ResetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);

	KeyValuePair<bool, string> ResetAppearanceTexture(string appearanceId, IManifest callerManifest);

	KeyValuePair<bool, List<string>> GetOutfitIds();

	KeyValuePair<bool, string> GetCurrentOutfitId();

	KeyValuePair<bool, string> SetCurrentOutfitId(string outfitId, IManifest callerManifest);

	KeyValuePair<bool, string> SetAppearanceLockState(Type appearanceType, string targetPackId, string targetAppearanceName, bool isLocked, IManifest callerManifest);

	KeyValuePair<bool, string> RegisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest, Func<IDrawTool, bool> appearanceDrawOverride);

	KeyValuePair<bool, string> UnregisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest);

	KeyValuePair<bool, string> IsDrawOverrideActive(Type appearanceType, IManifest callerManifest);
}
