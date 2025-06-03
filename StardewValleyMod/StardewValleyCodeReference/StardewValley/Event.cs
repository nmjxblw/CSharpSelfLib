using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

public class Event
{
	/// <summary>The low-level event commands defined by the base game. Most code should use <see cref="T:StardewValley.Event" /> methods instead.</summary>
	/// <remarks>Every method within this class is an event command whose name matches the method name. All event commands must be static, public, and match <see cref="T:StardewValley.Delegates.EventCommandDelegate" />.</remarks>
	public static class DefaultCommands
	{
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void IgnoreEventTileOffset(Event @event, string[] args, EventContext context)
		{
			@event.ignoreTileOffsets = true;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Move(Event @event, string[] args, EventContext context)
		{
			bool? continueAfterMove = null;
			int fieldsAfterMoves = (args.Length - 1) % 4;
			if (fieldsAfterMoves == 1)
			{
				if (!ArgUtility.TryGetOptionalBool(args, args.Length - 1, out var rawValue, out var error, defaultValue: false, "bool rawValue"))
				{
					context.LogErrorAndSkip(error);
					return;
				}
				continueAfterMove = rawValue;
			}
			else if (fieldsAfterMoves > 1)
			{
				context.LogErrorAndSkip("invalid number of arguments, expected sets of [actor x y direction] fields plus an optional continue-after-move boolean field");
				return;
			}
			if (!continueAfterMove.HasValue || args.Length > 2)
			{
				for (int i = 1; i < args.Length && ArgUtility.HasIndex(args, i + 3); i += 4)
				{
					if (!ArgUtility.TryGet(args, i, out var actorName, out var error2, allowBlank: true, "string actorName") || !ArgUtility.TryGetPoint(args, i + 1, out var tile, out error2, "Point tile") || !ArgUtility.TryGetDirection(args, i + 3, out var facingDirection, out error2, "int facingDirection"))
					{
						context.LogError(error2);
						continue;
					}
					if (@event.IsFarmerActorId(actorName, out var farmerNumber))
					{
						if (!@event.actorPositionsAfterMove.ContainsKey(actorName))
						{
							Farmer farmer = @event.GetFarmerActor(farmerNumber);
							if (farmer != null)
							{
								farmer.canOnlyWalk = false;
								farmer.setRunning(isRunning: false, force: true);
								farmer.canOnlyWalk = true;
								farmer.convertEventMotionCommandToMovement(Utility.PointToVector2(tile));
								@event.actorPositionsAfterMove.Add(actorName, @event.getPositionAfterMove(farmer, tile.X, tile.Y, facingDirection));
							}
						}
						continue;
					}
					bool isOptionalNpc;
					NPC n = @event.getActorByName(actorName, out isOptionalNpc);
					if (n == null)
					{
						if (!isOptionalNpc)
						{
							context.LogErrorAndSkip("no NPC found with name '" + actorName + "'");
							return;
						}
					}
					else if (!@event.actorPositionsAfterMove.ContainsKey(n.Name))
					{
						n.convertEventMotionCommandToMovement(Utility.PointToVector2(tile));
						@event.actorPositionsAfterMove.Add(n.Name, @event.getPositionAfterMove(n, tile.X, tile.Y, facingDirection));
					}
				}
			}
			if (!continueAfterMove.HasValue)
			{
				return;
			}
			if (continueAfterMove == true)
			{
				@event.continueAfterMove = true;
				@event.CurrentCommand++;
				return;
			}
			@event.continueAfterMove = false;
			if (args.Length == 2 && @event.actorPositionsAfterMove.Count == 0)
			{
				@event.CurrentCommand++;
			}
		}

		/// <summary>Run an action string.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Action(Event @event, string[] args, EventContext context)
		{
			Exception ex;
			if (!ArgUtility.TryGetRemainder(args, 1, out var action, out var error, ' ', "string action"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (!TriggerActionManager.TryRunAction(action, out error, out ex))
			{
				if (ex != null)
				{
					error += $"\n{ex}";
				}
				context.LogErrorAndSkip(error);
			}
			else
			{
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Speak(Event @event, string[] args, EventContext context)
		{
			if (@event.skipped)
			{
				return;
			}
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var textOrTranslationKey, out error, allowBlank: true, "string textOrTranslationKey"))
			{
				context.LogErrorAndSkip(error);
			}
			else
			{
				if (Game1.dialogueUp)
				{
					return;
				}
				@event.timeAccumulator += context.Time.ElapsedGameTime.Milliseconds;
				if (@event.timeAccumulator < 500f)
				{
					return;
				}
				@event.timeAccumulator = 0f;
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc) ?? Game1.getCharacterFromName(actorName.TrimEnd('?'));
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					if (!isOptionalNpc)
					{
						Game1.eventFinished();
					}
					return;
				}
				Game1.player.NotifyQuests((Quest quest) => quest.OnNpcSocialized(n));
				if (n.CanSocialize && !Game1.player.friendshipData.ContainsKey(n.Name))
				{
					Game1.player.friendshipData.Add(n.Name, new Friendship(0));
				}
				Dialogue dialogue = (Game1.content.IsValidTranslationKey(textOrTranslationKey) ? new Dialogue(n, textOrTranslationKey) : new Dialogue(n, null, textOrTranslationKey));
				n.CurrentDialogue.Push(dialogue);
				Game1.drawDialogue(n);
			}
		}

		/// <summary>Try to execute all commands in one tick until <see cref="M:StardewValley.Event.DefaultCommands.EndSimultaneousCommand(StardewValley.Event,System.String[],StardewValley.EventContext)" /> is called.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void BeginSimultaneousCommand(Event @event, string[] args, EventContext context)
		{
			@event.simultaneousCommand = true;
			@event.CurrentCommand++;
		}

		/// <summary>If commands are being executed in one tick due to <see cref="M:StardewValley.Event.DefaultCommands.BeginSimultaneousCommand(StardewValley.Event,System.String[],StardewValley.EventContext)" />, stop doing so for the remaining commands.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void EndSimultaneousCommand(Event @event, string[] args, EventContext context)
		{
			@event.simultaneousCommand = false;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void MineDeath(Event @event, string[] args, EventContext context)
		{
			if (!Game1.dialogueUp)
			{
				Random r = Utility.CreateDaySaveRandom(Game1.timeOfDay);
				int moneyToLose = r.Next(Game1.player.Money / 40, Game1.player.Money / 8);
				moneyToLose = Math.Min(moneyToLose, 15000);
				moneyToLose -= (int)((double)Game1.player.LuckLevel * 0.01 * (double)moneyToLose);
				moneyToLose -= moneyToLose % 100;
				int numberOfItemsLost = Game1.player.LoseItemsOnDeath(r);
				Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
				Game1.player.Money = Math.Max(0, Game1.player.Money - moneyToLose);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1057") + " " + ((moneyToLose <= 0) ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1058", moneyToLose)) + ((numberOfItemsLost <= 0) ? ((moneyToLose <= 0) ? "" : ".") : ((moneyToLose <= 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1060") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1063") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))))));
				@event.InsertNextCommand("showItemsLost");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void HospitalDeath(Event @event, string[] args, EventContext context)
		{
			if (!Game1.dialogueUp)
			{
				int numberOfItemsLost = Game1.player.LoseItemsOnDeath();
				Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
				int moneyToLose = Math.Min(1000, Game1.player.Money);
				Game1.player.Money -= moneyToLose;
				Game1.drawObjectDialogue(((moneyToLose > 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1068", moneyToLose) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1070")) + ((numberOfItemsLost > 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1071") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : ""));
				@event.InsertNextCommand("showItemsLost");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ShowItemsLost(Event @event, string[] args, EventContext context)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new ItemListMenu(Game1.content.LoadString("Strings\\UI:ItemList_ItemsLost"), Game1.player.itemsLostLastDeath.ToList());
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void End(Event @event, string[] args, EventContext context)
		{
			@event.endBehaviors(args, context.Location);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void LocationSpecificCommand(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var command, out var error, allowBlank: true, "string command"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			string[] commandArgs = args.Skip(2).ToArray();
			if (context.Location.RunLocationSpecificEventCommand(@event, command, !@event._repeatingLocationSpecificCommand, commandArgs))
			{
				@event._repeatingLocationSpecificCommand = false;
				@event.CurrentCommand++;
			}
			else
			{
				@event._repeatingLocationSpecificCommand = true;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Unskippable(Event @event, string[] args, EventContext context)
		{
			@event.skippable = false;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Skippable(Event @event, string[] args, EventContext context)
		{
			@event.skippable = true;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SetSkipActions(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetRemainder(args, 1, out var skipActions, out var error, ' ', "string skipActions"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (string.IsNullOrWhiteSpace(skipActions))
			{
				@event.actionsOnSkip = null;
			}
			else
			{
				string[] actions = LegacyShims.SplitAndTrim(skipActions, '#');
				string[] array = actions;
				for (int i = 0; i < array.Length; i++)
				{
					if (!TriggerActionManager.TryValidateActionExists(array[i], out error))
					{
						context.LogErrorAndSkip(error);
						return;
					}
				}
				@event.actionsOnSkip = actions;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Emote(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out var emoteId, out error, "int emoteId") || !ArgUtility.TryGetOptionalBool(args, 3, out var nextCommandImmediate, out error, defaultValue: false, "bool nextCommandImmediate"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				@event.GetFarmerActor(farmerNumber)?.doEmote(emoteId, !nextCommandImmediate);
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (!n.isEmoting)
				{
					n.doEmote(emoteId, !nextCommandImmediate);
				}
			}
			if (nextCommandImmediate)
			{
				@event.CurrentCommand++;
				@event.Update(context.Location, context.Time);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopMusic(Event @event, string[] args, EventContext context)
		{
			Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.Event);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PlayPetSound(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var sound, out var error, allowBlank: true, "string sound"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Pet pet = null;
			foreach (NPC actor in @event.actors)
			{
				if (actor is Pet)
				{
					pet = actor as Pet;
					break;
				}
			}
			if (pet == null)
			{
				pet = Game1.player.getPet();
			}
			float pitch = 1200f;
			if (pet != null)
			{
				PetData pet_data = pet.GetPetData();
				PetBreed breed = pet_data?.GetBreedById(pet.whichBreed.Value);
				if (breed != null)
				{
					pitch *= breed.VoicePitch;
					if (sound == pet_data.BarkSound && breed.BarkOverride != null)
					{
						sound = breed.BarkOverride;
					}
				}
			}
			Game1.playSound(sound, (int)pitch);
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PlaySound(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var soundId, out var error, allowBlank: true, "string soundId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.playSound(soundId, out var sound);
			@event.TrackSound(sound);
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopSound(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var soundId, out var error, allowBlank: true, "string soundId") || !ArgUtility.TryGetOptionalBool(args, 2, out var immediate, out error, defaultValue: true, "bool immediate"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.StopTrackedSound(soundId, immediate);
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TossConcession(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var concessionId, out error, allowBlank: true, "string concessionId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			MovieConcession concession = MovieTheater.GetConcessionItem(concessionId);
			if (concession == null)
			{
				context.LogErrorAndSkip("no concession found with ID '" + concessionId + "'");
				return;
			}
			Texture2D texture = concession.GetTexture();
			int spriteIndex = concession.GetSpriteIndex();
			Game1.playSound("dwop");
			context.Location.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = texture,
				sourceRect = Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16),
				animationLength = 1,
				totalNumberOfLoops = 1,
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				interval = 1000f,
				scale = 4f,
				position = @event.OffsetPosition(new Vector2(actor.Position.X, actor.Position.Y - 96f)),
				layerDepth = (float)actor.StandingPixel.Y / 10000f
			});
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Pause(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var pauseTime, out var error, "int pauseTime"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = pauseTime;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PrecisePause(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var pauseTime, out var error, "int pauseTime"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.stopWatch == null)
			{
				@event.stopWatch = new Stopwatch();
			}
			if (!@event.stopWatch.IsRunning)
			{
				@event.stopWatch.Start();
			}
			if (@event.stopWatch.ElapsedMilliseconds >= pauseTime)
			{
				@event.stopWatch.Stop();
				@event.stopWatch.Reset();
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ResetVariable(Event @event, string[] args, EventContext context)
		{
			@event.specialEventVariable1 = false;
			@event.currentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void FaceDirection(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetDirection(args, 2, out var faceDirection, out error, "int faceDirection") || !ArgUtility.TryGetOptionalBool(args, 3, out var continueImmediate, out error, defaultValue: false, "bool continueImmediate"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.FarmerSprite.StopAnimation();
					f.completelyStopAnimatingOrDoingAction();
					f.faceDirection(faceDirection);
				}
			}
			else if (actorName.Contains("spouse"))
			{
				NPC spouse = @event.getActorByName(Game1.player.spouse);
				if (spouse != null && !Game1.player.hasRoommate())
				{
					spouse.faceDirection(faceDirection);
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.faceDirection(faceDirection);
			}
			if (continueImmediate)
			{
				@event.CurrentCommand++;
				@event.Update(context.Location, context.Time);
			}
			else if (Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = 500f;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Warp(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetVector2(args, 2, out var tile, out error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGetOptionalBool(args, 4, out var continueImmediate, out error, defaultValue: false, "bool continueImmediate"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.setTileLocation(@event.OffsetTile(tile));
					f.position.Y -= 16f;
					if (@event.farmerActors.Contains(f))
					{
						f.completelyStopAnimatingOrDoingAction();
					}
				}
			}
			else if (actorName.Contains("spouse"))
			{
				NPC spouse = @event.getActorByName(Game1.player.spouse);
				if (spouse != null && !Game1.player.hasRoommate())
				{
					@event.npcControllers?.RemoveAll((NPCController npcController) => npcController.puppet.Name == Game1.player.spouse);
					spouse.Position = @event.OffsetPosition(tile * 64f);
					spouse.IsWalkingInSquare = false;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.position.X = @event.OffsetPositionX(tile.X * 64f + 4f);
				n.position.Y = @event.OffsetPositionY(tile.Y * 64f);
				n.IsWalkingInSquare = false;
			}
			@event.CurrentCommand++;
			if (continueImmediate)
			{
				@event.Update(context.Location, context.Time);
			}
		}

		/// <summary>Change the event position for all connected farmers.</summary>
		/// <remarks>This expects at least four fields:
		///   1. zero or more [x y direction] triplets (one per possible farmer);
		///   2. an offset direction (up/down/left/right), which sets where each subsequent farmer is set when using the default triplet;
		///   3. and a default [x y direction] triplet which applies to any unlisted farmer.</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void WarpFarmers(Event @event, string[] args, EventContext context)
		{
			int nonWarpFields = (args.Length - 1) % 3;
			if (args.Length < 5 || nonWarpFields != 1)
			{
				context.LogErrorAndSkip("invalid number of arguments; expected zero or more [x y direction] triplets, one offset direction (up/down/left/right), and one triplet which applies to any other farmer");
				return;
			}
			int defaultsIndex = args.Length - 4;
			if (!ArgUtility.TryGetDirection(args, defaultsIndex, out var offsetDirection, out var error, "int offsetDirection") || !ArgUtility.TryGetPoint(args, defaultsIndex + 1, out var defaultPosition, out error, "Point defaultPosition") || !ArgUtility.TryGetDirection(args, defaultsIndex + 3, out var defaultFacingDirection, out error, "int defaultFacingDirection"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			List<Vector3> positions = new List<Vector3>();
			for (int i = 1; i < defaultsIndex; i += 3)
			{
				if (!ArgUtility.TryGetPoint(args, i, out var position, out error, "Point position") || !ArgUtility.TryGetDirection(args, i + 2, out var facingDirection, out error, "int facingDirection"))
				{
					context.LogErrorAndSkip(error);
					return;
				}
				positions.Add(new Vector3(position.X, position.Y, facingDirection));
			}
			Point offset;
			switch (offsetDirection)
			{
			case 3:
				offset = new Point(-1, 0);
				break;
			case 1:
				offset = new Point(1, 0);
				break;
			case 0:
				offset = new Point(0, -1);
				break;
			case 2:
				offset = new Point(0, 1);
				break;
			default:
				context.LogErrorAndSkip($"invalid offset direction '{offsetDirection}'; must be one of 'left', 'right', 'up', or 'down'");
				return;
			}
			int currentX = defaultPosition.X;
			int currentY = defaultPosition.Y;
			for (int j = 0; j < Game1.numberOfPlayers(); j++)
			{
				Farmer farmer = @event.GetFarmerActor(j + 1);
				float x;
				float y;
				int direction;
				if (j < positions.Count)
				{
					x = positions[j].X;
					y = positions[j].Y;
					direction = (int)positions[j].Z;
				}
				else
				{
					x = currentX;
					y = currentY;
					direction = defaultFacingDirection;
					currentX += offset.X;
					currentY += offset.Y;
					if (context.Location.map.GetLayer("Buildings")?.Tiles[currentX, currentY] != null && offset != Point.Zero)
					{
						currentX -= offset.X;
						currentY -= offset.Y;
						offset = Point.Zero;
					}
				}
				if (farmer != null)
				{
					farmer.setTileLocation(@event.OffsetTile(new Vector2(x, y)));
					farmer.faceDirection(direction);
					farmer.position.Y -= 16f;
					farmer.completelyStopAnimatingOrDoingAction();
				}
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Speed(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out var speed, out error, "int speed"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				if (@event.IsCurrentFarmerActorId(farmerNumber))
				{
					@event.farmerAddedSpeed = speed;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				actor.speed = speed;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopAdvancedMoves(Event @event, string[] args, EventContext context)
		{
			string option = ArgUtility.Get(args, 1);
			if (option != null)
			{
				if (!(option == "next"))
				{
					context.LogErrorAndSkip("unknown option " + option + ", must be 'next' or omitted");
					return;
				}
				foreach (NPCController npcController in @event.npcControllers)
				{
					npcController.destroyAtNextCrossroad();
				}
			}
			else
			{
				@event.npcControllers.Clear();
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void DoAction(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Location tileLocation = new Location(@event.OffsetTileX(tile.X), @event.OffsetTileY(tile.Y));
			Game1.hooks.OnGameLocation_CheckAction(context.Location, tileLocation, Game1.viewport, @event.farmer, () => context.Location.checkAction(tileLocation, Game1.viewport, @event.farmer));
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveTile(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGet(args, 3, out var layerId, out error, allowBlank: true, "string layerId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			context.Location.removeTile(@event.OffsetTileX(tile.X), @event.OffsetTileY(tile.Y), layerId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TextAboveHead(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var text, out error, allowBlank: true, "string text"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC n = @event.getActorByName(actorName, out isOptionalNpc);
			if (n == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			n.showTextAboveHead(text);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ShowFrame(Event @event, string[] args, EventContext context)
		{
			bool flip = false;
			string actorName;
			int frame;
			string error2;
			if (args.Length == 2)
			{
				actorName = "farmer";
				if (!ArgUtility.TryGetInt(args, 1, out frame, out var error, "frame"))
				{
					context.LogErrorAndSkip(error);
					return;
				}
			}
			else if (!ArgUtility.TryGet(args, 1, out actorName, out error2, allowBlank: true, "actorName") || !ArgUtility.TryGetInt(args, 2, out frame, out error2, "frame") || !ArgUtility.TryGetOptionalBool(args, 3, out flip, out error2, defaultValue: false, "flip"))
			{
				context.LogErrorAndSkip(error2);
				return;
			}
			if (!@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (actorName == "spouse" && n.Gender == Gender.Male && frame >= 36 && frame <= 38)
				{
					frame += 12;
				}
				n.Sprite.CurrentFrame = frame;
			}
			else
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.FarmerSprite.setCurrentAnimation(new FarmerSprite.AnimationFrame[1]
					{
						new FarmerSprite.AnimationFrame(frame, 100, secondaryArm: false, flip)
					});
					f.FarmerSprite.loop = true;
					f.FarmerSprite.loopThisAnimation = true;
					f.FarmerSprite.PauseForSingleAnimation = true;
					f.Sprite.currentFrame = frame;
				}
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void FarmerAnimation(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var animationId, out var error, "int animationId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.farmer.FarmerSprite.setCurrentSingleAnimation(animationId);
			@event.farmer.FarmerSprite.PauseForSingleAnimation = true;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void IgnoreMovementAnimation(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptionalBool(args, 2, out var ignore, out error, defaultValue: true, "bool ignore"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerId))
			{
				Farmer f = @event.GetFarmerActor(farmerId);
				if (f != null)
				{
					f.ignoreMovementAnimation = ignore;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc, legacyReplaceUnderscores: true);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.ignoreMovementAnimation = ignore;
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Animate(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetBool(args, 2, out var flip, out error, "bool flip") || !ArgUtility.TryGetBool(args, 3, out var loop, out error, "bool loop") || !ArgUtility.TryGetInt(args, 4, out var frameDuration, out error, "int frameDuration"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
			for (int i = 5; i < args.Length; i++)
			{
				if (!ArgUtility.TryGetInt(args, i, out var frame, out error, "int frame"))
				{
					context.LogErrorAndSkip(error);
					return;
				}
				animationFrames.Add(new FarmerSprite.AnimationFrame(frame, frameDuration, secondaryArm: false, flip));
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
					f.FarmerSprite.loop = true;
					f.FarmerSprite.loopThisAnimation = loop;
					f.FarmerSprite.PauseForSingleAnimation = true;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc, legacyReplaceUnderscores: true);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.Sprite.setCurrentAnimation(animationFrames);
				n.Sprite.loop = loop;
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopAnimation(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptionalInt(args, 2, out var endFrame, out error, -1, "int endFrame"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.completelyStopAnimatingOrDoingAction();
					f.Halt();
					f.FarmerSprite.CurrentAnimation = null;
					switch (f.FacingDirection)
					{
					case 0:
						f.FarmerSprite.setCurrentSingleFrame(12, 32000);
						break;
					case 1:
						f.FarmerSprite.setCurrentSingleFrame(6, 32000);
						break;
					case 2:
						f.FarmerSprite.setCurrentSingleFrame(0, 32000);
						break;
					case 3:
						f.FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
						break;
					}
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.Sprite.StopAnimation();
				if (endFrame > -1)
				{
					n.Sprite.currentFrame = endFrame;
					n.Sprite.UpdateSourceRect();
				}
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeLocation(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var locationName, out var error, allowBlank: true, "string locationName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Point playerTile = @event.farmer.TilePoint;
			@event.changeLocation(locationName, playerTile.X, playerTile.Y, delegate
			{
				Game1.currentLocation.ResetForEvent(@event);
				@event.CurrentCommand++;
			});
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Halt(Event @event, string[] args, EventContext context)
		{
			foreach (NPC actor in @event.actors)
			{
				actor.Halt();
			}
			@event.farmer.Halt();
			@event.CurrentCommand++;
			@event.continueAfterMove = false;
			@event.actorPositionsAfterMove.Clear();
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Message(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var dialogue, out var error, allowBlank: true, "string dialogue"))
			{
				context.LogError(error);
			}
			if (!Game1.dialogueUp && Game1.activeClickableMenu == null)
			{
				Game1.drawDialogueNoTyping(Game1.parseText(dialogue));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddCookingRecipe(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetRemainder(args, 1, out var recipeKey, out var error, ' ', "string recipeKey"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.cookingRecipes.TryAdd(recipeKey, 0);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ItemAboveHead(Event @event, string[] args, EventContext context)
		{
			string itemId = ArgUtility.Get(args, 1);
			bool showMessage = ArgUtility.GetBool(args, 2, defaultValue: true);
			switch (itemId?.ToLower())
			{
			case "pan":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(T)Pan"), showMessage);
				break;
			case "hero":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)116"), showMessage);
				break;
			case "sculpture":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(F)1306"), showMessage);
				break;
			case "samboombox":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(F)1309"), showMessage);
				break;
			case "joja":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)117"), showMessage);
				break;
			case "slimeegg":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(O)680"), showMessage);
				break;
			case "rod":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(T)BambooPole"), showMessage);
				break;
			case "sword":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(W)0"), showMessage);
				break;
			case "ore":
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(O)334"), showMessage);
				break;
			case "pot":
				showMessage = ArgUtility.GetBool(args, 2);
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)62"), showMessage);
				break;
			case "jukebox":
				showMessage = ArgUtility.GetBool(args, 2);
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create("(BC)209"), showMessage);
				break;
			case null:
				@event.farmer.holdUpItemThenMessage(null, showMessage: false);
				break;
			default:
				@event.farmer.holdUpItemThenMessage(ItemRegistry.Create(itemId), showMessage);
				break;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddCraftingRecipe(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetRemainder(args, 1, out var recipeKey, out var error, ' ', "string recipeKey"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.craftingRecipes.TryAdd(recipeKey, 0);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void HostMail(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: true, "string mailId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail(mailId))
			{
				Game1.addMailForTomorrow(mailId);
			}
			@event.CurrentCommand++;
		}

		/// <summary>Add a letter to the mailbox tomorrow.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Mail(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: true, "string mailId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (!Game1.player.hasOrWillReceiveMail(mailId))
			{
				Game1.addMailForTomorrow(mailId);
			}
			@event.CurrentCommand++;
		}

		/// <summary>Add a letter to the mailbox immediately.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void MailToday(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: true, "string mailId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (!Game1.player.hasOrWillReceiveMail(mailId))
			{
				Game1.addMail(mailId);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Shake(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out var duration, out error, "int duration"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			actor.shake(duration);
			@event.CurrentCommand++;
		}

		/// <remarks>Main format: <c>temporaryAnimatedSprite texture rect_x rect_y rect_width rect_height animation_interval animation_length number_of_loops tile_x tile_y flicker flipped layer_depth alpha_fade scale scale_change rotation rotation_change</c>. This also supports a number of extended options (like <c>color Green</c>).</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TemporaryAnimatedSprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var textureName, out var error, allowBlank: true, "string textureName") || !ArgUtility.TryGetRectangle(args, 2, out var sourceRect, out error, "Rectangle sourceRect") || !ArgUtility.TryGetFloat(args, 6, out var animationInterval, out error, "float animationInterval") || !ArgUtility.TryGetInt(args, 7, out var animationLength, out error, "int animationLength") || !ArgUtility.TryGetInt(args, 8, out var numberOfLoops, out error, "int numberOfLoops") || !ArgUtility.TryGetVector2(args, 9, out var tile, out error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGetBool(args, 11, out var flicker, out error, "bool flicker") || !ArgUtility.TryGetBool(args, 12, out var flip, out error, "bool flip") || !ArgUtility.TryGetFloat(args, 13, out var layerDepth, out error, "float layerDepth") || !ArgUtility.TryGetFloat(args, 14, out var alphaFade, out error, "float alphaFade") || !ArgUtility.TryGetInt(args, 15, out var scale, out error, "int scale") || !ArgUtility.TryGetFloat(args, 16, out var scaleChange, out error, "float scaleChange") || !ArgUtility.TryGetFloat(args, 17, out var rotation, out error, "float rotation") || !ArgUtility.TryGetFloat(args, 18, out var rotationChange, out error, "float rotationChange"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, animationInterval, animationLength, numberOfLoops, @event.OffsetPosition(tile * 64f), flicker, flip, @event.OffsetPosition(new Vector2(0f, layerDepth) * 64f).Y / 10000f, alphaFade, Color.White, 4 * scale, scaleChange, rotation, rotationChange);
			for (int i = 19; i < args.Length; i++)
			{
				switch (args[i])
				{
				case "color":
				{
					if (!ArgUtility.TryGet(args, i + 1, out var rawColor, out error, allowBlank: true, "string rawColor"))
					{
						context.LogError(error);
						break;
					}
					Color? color = Utility.StringToColor(rawColor);
					if (color.HasValue)
					{
						tempSprite.color = color.Value;
					}
					else
					{
						context.LogError($"index {i + 1} has value '{rawColor}', which can't be parsed as a color");
					}
					i++;
					break;
				}
				case "hold_last_frame":
					tempSprite.holdLastFrame = true;
					break;
				case "ping_pong":
					tempSprite.pingPong = true;
					break;
				case "motion":
				{
					if (!ArgUtility.TryGetVector2(args, i + 1, out var value2, out error, integerOnly: false, "Vector2 value"))
					{
						context.LogError(error);
						break;
					}
					tempSprite.motion = value2;
					i += 2;
					break;
				}
				case "acceleration":
				{
					if (!ArgUtility.TryGetVector2(args, i + 1, out var value3, out error, integerOnly: false, "Vector2 value"))
					{
						context.LogError(error);
						break;
					}
					tempSprite.acceleration = value3;
					i += 2;
					break;
				}
				case "acceleration_change":
				{
					if (!ArgUtility.TryGetVector2(args, i + 1, out var value, out error, integerOnly: false, "Vector2 value"))
					{
						context.LogError(error);
						break;
					}
					tempSprite.accelerationChange = value;
					i += 2;
					break;
				}
				default:
					context.LogError("unknown option '" + args[i] + "'");
					break;
				}
			}
			context.Location.TemporarySprites.Add(tempSprite);
			@event.CurrentCommand++;
		}

		/// <remarks>Format: <c>temporarySprite xTile yTile rowInAnimationSheet animationLength animationInterval=300 flipped=false layerDepth=-1</c>.</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TemporarySprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetVector2(args, 1, out var tile, out var error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGetInt(args, 3, out var rowInAnimationSheet, out error, "int rowInAnimationSheet") || !ArgUtility.TryGetInt(args, 4, out var animationLength, out error, "int animationLength") || !ArgUtility.TryGetOptionalFloat(args, 5, out var animationInterval, out error, 300f, "float animationInterval") || !ArgUtility.TryGetOptionalBool(args, 6, out var flipped, out error, defaultValue: false, "bool flipped") || !ArgUtility.TryGetOptionalFloat(args, 7, out var layerDepth, out error, -1f, "float layerDepth"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			context.Location.TemporarySprites.Add(new TemporaryAnimatedSprite(rowInAnimationSheet, @event.OffsetPosition(tile * 64f), Color.White, animationLength, flipped, animationInterval, 0, 64, layerDepth));
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveTemporarySprites(Event @event, string[] args, EventContext context)
		{
			context.Location.TemporarySprites.Clear();
			@event.CurrentCommand++;
		}

		/// <summary>A command that does nothing. Used just to wait for another event to finish.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Null(Event @event, string[] args, EventContext context)
		{
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SpecificTemporarySprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var spriteId, out var error, allowBlank: true, "string spriteId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.addSpecificTemporarySprite(spriteId, context.Location, args);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PlayMusic(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetRemainder(args, 1, out var musicId, out var error, ' ', "string musicId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (musicId == "samBand")
			{
				if (Game1.player.DialogueQuestionsAnswered.Contains("78"))
				{
					Game1.changeMusicTrack("shimmeringbastion", track_interruptable: false, MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains("79"))
				{
					Game1.changeMusicTrack("honkytonky", track_interruptable: false, MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains("77"))
				{
					Game1.changeMusicTrack("heavy", track_interruptable: false, MusicContext.Event);
				}
				else
				{
					Game1.changeMusicTrack("poppy", track_interruptable: false, MusicContext.Event);
				}
			}
			else if (Game1.options.musicVolumeLevel > 0f)
			{
				Game1.changeMusicTrack(musicId, track_interruptable: false, MusicContext.Event);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void MakeInvisible(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGetOptionalInt(args, 3, out var width, out error, 1, "int width") || !ArgUtility.TryGetOptionalInt(args, 4, out var height, out error, 1, "int height"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			GameLocation location = context.Location;
			int originX = @event.OffsetTileX(tile.X);
			int originY = @event.OffsetTileY(tile.Y);
			for (int y = originY; y < originY + height; y++)
			{
				for (int x = originX; x < originX + width; x++)
				{
					Object o = location.getObjectAtTile(x, y);
					TerrainFeature terrainFeature;
					if (o != null)
					{
						if (o is BedFurniture bed && bed.GetBoundingBox().Contains(Utility.Vector2ToPoint(Game1.player.mostRecentBed)))
						{
							@event.CurrentCommand++;
							return;
						}
						o.isTemporarilyInvisible = true;
					}
					else if (location.terrainFeatures.TryGetValue(new Vector2(x, y), out terrainFeature))
					{
						terrainFeature.isTemporarilyInvisible = true;
					}
				}
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddObject(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGet(args, 3, out var itemId, out error, allowBlank: true, "string itemId") || !ArgUtility.TryGetOptionalFloat(args, 4, out var layerDepth, out error, -1f, "float layerDepth"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Vector2 pixelPos = @event.OffsetPosition(new Vector2(tile.X * 64, tile.Y * 64));
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, Microsoft.Xna.Framework.Rectangle.Empty, pixelPos, flipped: false, 0f, Color.White)
			{
				layerDepth = ((layerDepth >= 0f) ? layerDepth : ((float)(@event.OffsetTileY(tile.Y) * 64) / 10000f))
			};
			sprite.CopyAppearanceFromItemId(itemId);
			context.Location.TemporarySprites.Add(sprite);
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddBigProp(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetVector2(args, 1, out var tile, out var error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGet(args, 3, out var itemId, out error, allowBlank: true, "string itemId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Object prop = ItemRegistry.Create<Object>("(BC)" + itemId);
			prop.TileLocation = @event.OffsetTile(tile);
			@event.props.Add(prop);
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddFloorProp(Event @event, string[] args, EventContext context)
		{
			DefaultCommands.AddProp(@event, args, context);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddProp(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 0, out var commandName, out var error, allowBlank: true, "string commandName") || !ArgUtility.TryGetInt(args, 1, out var index, out error, "int index") || !ArgUtility.TryGetPoint(args, 2, out var tile, out error, "Point tile") || !ArgUtility.TryGetOptionalInt(args, 4, out var drawWidth, out error, 1, "int drawWidth") || !ArgUtility.TryGetOptionalInt(args, 5, out var drawHeight, out error, 1, "int drawHeight") || !ArgUtility.TryGetOptionalInt(args, 6, out var boundingHeight, out error, drawHeight, "int boundingHeight") || !ArgUtility.TryGetOptionalInt(args, 7, out var tilesHorizontal, out error, 0, "int tilesHorizontal") || !ArgUtility.TryGetOptionalInt(args, 8, out var tilesVertical, out error, 0, "int tilesVertical"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			int tileX = @event.OffsetTileX(tile.X);
			int tileY = @event.OffsetTileY(tile.Y);
			bool solid = !commandName.EqualsIgnoreCase("AddFloorProp");
			@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, tileY, solid));
			if (tilesHorizontal != 0)
			{
				for (int x = tileX + tilesHorizontal; x != tileX; x -= Math.Sign(tilesHorizontal))
				{
					@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, x, tileY, solid));
				}
			}
			if (tilesVertical != 0)
			{
				for (int y = tileY + tilesVertical; y != tileY; y -= Math.Sign(tilesVertical))
				{
					@event.festivalProps.Add(new Prop(@event.festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, y, solid));
				}
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveObject(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			GameLocation location = context.Location;
			Vector2 position = @event.OffsetPosition(new Vector2(tile.X, tile.Y) * 64f);
			location.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.position == position);
			@event.CurrentCommand++;
			@event.Update(location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Glow(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(args, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out var blue, out error, "int blue") || !ArgUtility.TryGetOptionalBool(args, 4, out var hold, out error, defaultValue: false, "bool hold"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.screenGlowOnce(new Color(red, green, blue), hold);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopGlowing(Event @event, string[] args, EventContext context)
		{
			Game1.screenGlowUp = false;
			Game1.screenGlowHold = false;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddQuest(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var questId, out var error, allowBlank: true, "string questId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.addQuest(questId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveQuest(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var questId, out var error, allowBlank: true, "string questId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.removeQuest(questId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddSpecialOrder(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var orderId, out var error, allowBlank: true, "string orderId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.team.AddSpecialOrder(orderId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveSpecialOrder(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var orderId, out var error, allowBlank: true, "string orderId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.team.specialOrders.RemoveWhere((SpecialOrder order) => order.questKey.Value == orderId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddItem(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, allowBlank: true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out var count, out error, 1, "int count") || !ArgUtility.TryGetOptionalInt(args, 3, out var quality, out error, 0, "int quality"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Item i = ItemRegistry.Create(itemId, count, quality);
			if (i != null)
			{
				Game1.player.addItemByMenuIfNecessary(i);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
		{
			if (args.Length == 1)
			{
				string id = @event.id;
				if (id == "festival_spring13")
				{
					if (@event.festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
					{
						if (Game1.player.mailReceived.Add("Egg Festival"))
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(H)4"));
							}
							@event.CurrentCommand++;
							if (Game1.activeClickableMenu == null)
							{
								@event.CurrentCommand++;
							}
						}
						else
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)PrizeTicket"));
							}
							@event.CurrentCommand++;
							if (Game1.activeClickableMenu == null)
							{
								@event.CurrentCommand++;
							}
						}
					}
					else
					{
						@event.CurrentCommand += 2;
					}
					return;
				}
				if (id == "festival_winter8")
				{
					if (@event.festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
					{
						if (Game1.player.mailReceived.Add("Ice Festival"))
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.activeClickableMenu = new ItemGrabMenu(new Item[4]
								{
									ItemRegistry.Create("(H)17"),
									ItemRegistry.Create("(O)687"),
									ItemRegistry.Create("(O)691"),
									ItemRegistry.Create("(O)703")
								}, @event).setEssential(essential: true);
							}
							@event.CurrentCommand++;
						}
						else
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)PrizeTicket"));
							}
							@event.CurrentCommand++;
							if (Game1.activeClickableMenu == null)
							{
								@event.CurrentCommand++;
							}
						}
					}
					else
					{
						@event.CurrentCommand += 2;
					}
					return;
				}
			}
			if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, allowBlank: true, "string itemId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			switch (itemId.ToLower())
			{
			case "meowmere":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)65"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "birdiereward":
				Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
				{
					Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
				}
				@event.CurrentCommand++;
				@event.CurrentCommand++;
				break;
			case "memento":
			{
				Object o = ItemRegistry.Create<Object>("(O)864");
				o.specialItem = true;
				o.questItem.Value = true;
				Game1.player.addItemByMenuIfNecessary(o);
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			}
			case "emilyclothes":
			{
				Clothing pants = ItemRegistry.Create<Clothing>("(P)8");
				pants.Dye(new Color(0, 143, 239), 1f);
				Game1.player.addItemsByMenuIfNecessary(new List<Item>
				{
					ItemRegistry.Create("(B)804"),
					ItemRegistry.Create("(H)41"),
					ItemRegistry.Create("(S)1127"),
					pants
				});
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			}
			case "qimilk":
				if (Game1.player.mailReceived.Add("qiCave"))
				{
					Game1.player.maxHealth += 25;
				}
				@event.CurrentCommand++;
				break;
			case "pan":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)Pan"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "sculpture":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1306"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "samboombox":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1309"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "marniepainting":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1802"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "rod":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)BambooPole"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "pot":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)62"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "jukebox":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)209"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "sword":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)0"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "hero":
				Game1.getSteamAchievement("Achievement_LocalLegend");
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)116"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "joja":
				Game1.getSteamAchievement("Achievement_Joja");
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)117"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			case "slimeegg":
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)680"));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			default:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create(itemId));
				if (Game1.activeClickableMenu == null)
				{
					@event.CurrentCommand++;
				}
				@event.CurrentCommand++;
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AttachCharacterToTempSprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			TemporaryAnimatedSprite t = context.Location.temporarySprites.Last();
			if (t != null)
			{
				t.attachedCharacter = @event.getActorByName(actorName);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Fork(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var requiredId, out var error, allowBlank: true, "string requiredId") || !ArgUtility.TryGetOptional(args, 2, out var newKey, out error, null, allowBlank: true, "string newKey") || !ArgUtility.TryGetOptionalBool(args, 3, out var isTranslationKey, out error, defaultValue: false, "bool isTranslationKey"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (newKey == null)
			{
				newKey = requiredId;
				requiredId = null;
			}
			bool num;
			if (requiredId == null)
			{
				num = @event.specialEventVariable1;
			}
			else
			{
				if (Game1.player.mailReceived.Contains(requiredId))
				{
					goto IL_008f;
				}
				num = Game1.player.dialogueQuestionsAnswered.Contains(requiredId);
			}
			if (!num)
			{
				@event.CurrentCommand++;
				return;
			}
			goto IL_008f;
			IL_008f:
			string[] commands;
			if (isTranslationKey)
			{
				string raw = Game1.content.LoadStringReturnNullIfNotFound(newKey);
				if (raw == null)
				{
					context.LogErrorAndSkip("can't load new script from translation key '" + newKey + "' because that translation wasn't found");
					return;
				}
				commands = Event.ParseCommands(raw, context.Event.farmer);
			}
			else if (@event.isFestival)
			{
				if (!@event.TryGetFestivalDataForYear(newKey, out var raw2))
				{
					context.LogErrorAndSkip($"can't load new script from festival field '{newKey}' because there's no such key in the '{@event.id}' festival");
					return;
				}
				commands = Event.ParseCommands(raw2, context.Event.farmer);
			}
			else
			{
				string assetName = "Data\\Events\\" + Game1.currentLocation.Name;
				if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					context.LogErrorAndSkip("can't load new script from event asset '" + assetName + "' because it doesn't exist");
					return;
				}
				if (!Game1.content.Load<Dictionary<string, string>>(assetName).TryGetValue(newKey, out var raw3))
				{
					context.LogErrorAndSkip($"can't load new script from event asset '{assetName}' because it doesn't contain the required '{newKey}' key");
					return;
				}
				commands = Event.ParseCommands(raw3, context.Event.farmer);
			}
			@event.ReplaceAllCommands(commands);
			@event.forked = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SwitchEvent(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var newKey, out var error, allowBlank: true, "string newKey"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			string[] commands;
			if (@event.isFestival)
			{
				if (!@event.TryGetFestivalDataForYear(newKey, out var raw))
				{
					context.LogErrorAndSkip($"can't load new event from festival field '{newKey}' because there's no such key in the '{@event.id}' festival");
					return;
				}
				commands = Event.ParseCommands(raw, context.Event.farmer);
			}
			else
			{
				string assetName = "Data\\Events\\" + Game1.currentLocation.Name;
				if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					context.LogErrorAndSkip("can't load new event from asset '" + assetName + "' because it doesn't exist");
					return;
				}
				if (!Game1.content.Load<Dictionary<string, string>>(assetName).TryGetValue(newKey, out var raw2))
				{
					context.LogErrorAndSkip($"can't load new event from asset '{assetName}' because it doesn't contain the required '{newKey}' key");
					return;
				}
				commands = Event.ParseCommands(raw2, context.Event.farmer);
			}
			@event.ReplaceAllCommands(commands);
			@event.eventSwitched = true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GlobalFade(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetOptionalFloat(args, 1, out var fadeSpeed, out var error, 0.007f, "float fadeSpeed") || !ArgUtility.TryGetOptionalBool(args, 2, out var continueEventDuringFade, out error, defaultValue: false, "bool continueEventDuringFade"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (!Game1.globalFade)
			{
				if (continueEventDuringFade)
				{
					Game1.globalFadeToBlack(null, fadeSpeed);
					@event.CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToBlack(@event.incrementCommandAfterFade, fadeSpeed);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GlobalFadeToClear(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetOptionalFloat(args, 1, out var fadeSpeed, out var error, 0.007f, "float fadeSpeed") || !ArgUtility.TryGetOptionalBool(args, 2, out var continueEventDuringFade, out error, defaultValue: false, "bool continueEventDuringFade"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (!Game1.globalFade)
			{
				if (continueEventDuringFade)
				{
					Game1.globalFadeToClear(null, fadeSpeed);
					@event.CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToClear(@event.incrementCommandAfterFade, fadeSpeed);
				}
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Cutscene(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var cutsceneId, out var error, allowBlank: true, "string cutsceneId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			GameLocation location = context.Location;
			GameTime time = context.Time;
			if (@event.currentCustomEventScript != null)
			{
				if (@event.currentCustomEventScript.update(time, @event))
				{
					@event.currentCustomEventScript = null;
					@event.CurrentCommand++;
				}
			}
			else
			{
				if (Game1.currentMinigame != null)
				{
					return;
				}
				switch (cutsceneId)
				{
				case "greenTea":
					@event.currentCustomEventScript = new EventScript_GreenTea(new Vector2(-64000f, -64000f), @event);
					break;
				case "linusMoneyGone":
					foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
					{
						temporarySprite.alphaFade = 0.01f;
						temporarySprite.motion = new Vector2(0f, -1f);
					}
					@event.CurrentCommand++;
					return;
				case "marucomet":
					Game1.currentMinigame = new MaruComet();
					break;
				case "AbigailGame":
					Game1.currentMinigame = new AbigailGame(@event.getActorByName("Abigail") ?? Game1.RequireCharacter("Abigail"));
					break;
				case "robot":
					Game1.currentMinigame = new RobotBlastoff();
					break;
				case "haleyCows":
					Game1.currentMinigame = new HaleyCowPictures();
					break;
				case "boardGame":
					Game1.currentMinigame = new FantasyBoardGame();
					@event.CurrentCommand++;
					break;
				case "plane":
					Game1.currentMinigame = new PlaneFlyBy();
					break;
				case "balloonDepart":
				{
					TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(1);
					temporarySpriteByID.attachedCharacter = @event.farmer;
					temporarySpriteByID.motion = new Vector2(0f, -2f);
					TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(2);
					temporarySpriteByID2.attachedCharacter = @event.getActorByName("Harvey");
					temporarySpriteByID2.motion = new Vector2(0f, -2f);
					location.getTemporarySpriteByID(3).scaleChange = -0.01f;
					@event.CurrentCommand++;
					return;
				}
				case "clearTempSprites":
					location.temporarySprites.Clear();
					@event.CurrentCommand++;
					break;
				case "balloonChangeMap":
					@event.eventPositionTileOffset = Vector2.Zero;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, @event.OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(-23f, 0f) * 4f), flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						yStopCoordinate = (int)@event.OffsetPositionY(576f),
						reachedStopCoordinate = @event.balloonInSky,
						attachedCharacter = @event.farmer,
						id = 1
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, @event.OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(0f, 134f) * 4f), flicker: false, flipped: false, 0.2625f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						id = 2,
						attachedCharacter = @event.getActorByName("Harvey")
					});
					@event.CurrentCommand++;
					break;
				case "bandFork":
				{
					int whichBand = 76;
					if (Game1.player.dialogueQuestionsAnswered.Contains("77"))
					{
						whichBand = 77;
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains("78"))
					{
						whichBand = 78;
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains("79"))
					{
						whichBand = 79;
					}
					@event.answerDialogue("bandFork", whichBand);
					@event.CurrentCommand++;
					return;
				}
				case "eggHuntWinner":
					@event.eggHuntWinner();
					@event.CurrentCommand++;
					return;
				case "governorTaste":
					@event.governorTaste();
					@event.currentCommand++;
					return;
				case "addSecretSantaItem":
				{
					Item o = Utility.getGiftFromNPC(@event.mySecretSanta);
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(o);
					@event.currentCommand++;
					return;
				}
				case "iceFishingWinner":
					@event.iceFishingWinner();
					@event.currentCommand++;
					return;
				case "iceFishingWinnerMP":
					@event.iceFishingWinnerMP();
					@event.currentCommand++;
					return;
				}
				Game1.globalFadeToClear(null, 0.01f);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void WaitForTempSprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var spriteId, out var error, "int spriteId"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (Game1.currentLocation.getTemporarySpriteByID(spriteId) != null)
			{
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Cave(Event @event, string[] args, EventContext context)
		{
			if (Game1.activeClickableMenu == null)
			{
				Response[] responses = new Response[2]
				{
					new Response("Mushrooms", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1220")),
					new Response("Bats", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1222"))
				};
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1223"), responses, "cave");
				Game1.dialogueTyping = false;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void UpdateMinigame(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var eventData, out var error, "int eventData"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.currentMinigame?.receiveEventPoke(eventData);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StartJittering(Event @event, string[] args, EventContext context)
		{
			@event.farmer.jitterStrength = 1f;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Money(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var amount, out var error, "int amount"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.farmer.Money += amount;
			if (@event.farmer.Money < 0)
			{
				@event.farmer.Money = 0;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopJittering(Event @event, string[] args, EventContext context)
		{
			@event.farmer.stopJittering();
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddLantern(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var initialParentSheetIndex, out var error, "int initialParentSheetIndex") || !ArgUtility.TryGetVector2(args, 2, out var tile, out error, integerOnly: true, "Vector2 tile") || !ArgUtility.TryGetInt(args, 4, out var lightRadius, out error, "int lightRadius"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			context.Location.TemporarySprites.Add(new TemporaryAnimatedSprite(initialParentSheetIndex, 999999f, 1, 0, @event.OffsetPosition(tile * 64f), flicker: false, flipped: false)
			{
				lightId = @event.GenerateLightSourceId($"{"AddLantern"}_{tile.X}_{tile.Y}"),
				lightRadius = lightRadius
			});
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RustyKey(Event @event, string[] args, EventContext context)
		{
			Game1.player.hasRustyKey = true;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Swimming(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer farmer = @event.GetFarmerActor(farmerNumber);
				if (farmer != null)
				{
					farmer.bathingClothes.Value = true;
					farmer.swimming.Value = true;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				actor.swimming.Value = true;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopSwimming(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer farmer = @event.GetFarmerActor(farmerNumber);
				if (farmer != null)
				{
					farmer.bathingClothes.Value = context.Location is BathHousePool;
					farmer.swimming.Value = false;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				actor.swimming.Value = false;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TutorialMenu(Event @event, string[] args, EventContext context)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new TutorialMenu();
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AnimalNaming(Event @event, string[] args, EventContext context)
		{
			GameLocation currentLocation = Game1.currentLocation;
			AnimalHouse animalHouse = currentLocation as AnimalHouse;
			if (animalHouse == null)
			{
				context.LogErrorAndSkip("this command only works when run in an AnimalHouse location");
			}
			else if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new NamingMenu(delegate(string animalName)
				{
					animalHouse.addNewHatchedAnimal(animalName);
					@event.CurrentCommand++;
				}, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"));
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SplitSpeak(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var dialogue, out error, allowBlank: true, "string dialogue"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			string[] choiceAnswers = LegacyShims.SplitAndTrim(dialogue, '~');
			if (Game1.dialogueUp)
			{
				return;
			}
			@event.timeAccumulator += context.Time.ElapsedGameTime.Milliseconds;
			if (!(@event.timeAccumulator < 500f))
			{
				@event.timeAccumulator = 0f;
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc) ?? Game1.getCharacterFromName(actorName);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (!ArgUtility.HasIndex(choiceAnswers, @event.previousAnswerChoice))
				{
					@event.CurrentCommand++;
					return;
				}
				n.CurrentDialogue.Push(new Dialogue(n, null, choiceAnswers[@event.previousAnswerChoice]));
				Game1.drawDialogue(n);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void CatQuestion(Event @event, string[] args, EventContext context)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				PetData data;
				string petType = (Pet.TryGetData(Game1.player.whichPetType, out data) ? (TokenParser.ParseText(data.DisplayName) ?? "pet") : "pet");
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:AdoptPet", petType), Game1.currentLocation.createYesNoResponses(), "pet");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AmbientLight(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(args, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out var blue, out error, "int blue"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.ambientLight = new Color(red, green, blue);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void BgColor(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var red, out var error, "int red") || !ArgUtility.TryGetInt(args, 2, out var green, out error, "int green") || !ArgUtility.TryGetInt(args, 3, out var blue, out error, "int blue"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.setBGColor((byte)red, (byte)green, (byte)blue);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ElliottBookTalk(Event @event, string[] args, EventContext context)
		{
			if (!Game1.dialogueUp)
			{
				string speechKey = (Game1.player.dialogueQuestionsAnswered.Contains("958699") ? "Strings\\StringsFromCSFiles:Event.cs.1257" : (Game1.player.dialogueQuestionsAnswered.Contains("958700") ? "Strings\\StringsFromCSFiles:Event.cs.1258" : ((!Game1.player.dialogueQuestionsAnswered.Contains("9586701")) ? "Strings\\StringsFromCSFiles:Event.cs.1260" : "Strings\\StringsFromCSFiles:Event.cs.1259")));
				NPC n = @event.getActorByName("Elliott") ?? Game1.getCharacterFromName("Elliott");
				n.CurrentDialogue.Push(new Dialogue(n, speechKey));
				Game1.drawDialogue(n);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveItem(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, allowBlank: true, "string itemId") || !ArgUtility.TryGetOptionalInt(args, 2, out var count, out error, 1, "int count"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.removeFirstOfThisItemFromInventory(itemId, count);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Friendship(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out var friendshipChange, out error, "int friendshipChange"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			NPC character = Game1.getCharacterFromName(actorName);
			if (character == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'");
				return;
			}
			Game1.player.changeFriendship(friendshipChange, character);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SetRunning(Event @event, string[] args, EventContext context)
		{
			@event.farmer.setRunning(isRunning: true);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ExtendSourceRect(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var rawOption, out error, allowBlank: true, "string rawOption"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isReset = rawOption == "reset";
			int horizontal = -1;
			int vertical = -1;
			bool ignoreSourceRectUpdates = false;
			if (!isReset && (!ArgUtility.TryGetInt(args, 2, out horizontal, out error, "horizontal") || !ArgUtility.TryGetInt(args, 3, out vertical, out error, "vertical") || !ArgUtility.TryGetOptionalBool(args, 4, out ignoreSourceRectUpdates, out error, defaultValue: false, "ignoreSourceRectUpdates")))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			if (isReset)
			{
				actor.reloadSprite();
				actor.Sprite.SpriteWidth = 16;
				actor.Sprite.SpriteHeight = 32;
				actor.HideShadow = false;
			}
			else
			{
				actor.extendSourceRect(horizontal, vertical, ignoreSourceRectUpdates);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void WaitForOtherPlayers(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var gateId, out var error, allowBlank: true, "string gateId"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (Game1.IsMultiplayer)
			{
				Game1.netReady.SetLocalReady(gateId, ready: true);
				if (Game1.netReady.IsReady(gateId))
				{
					if (Game1.activeClickableMenu is ReadyCheckDialog)
					{
						Game1.exitActiveMenu();
					}
					@event.CurrentCommand++;
				}
				else if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new ReadyCheckDialog(gateId, allowCancel: false);
				}
			}
			else
			{
				@event.CurrentCommand++;
			}
		}

		/// <summary>Used in the movie theater, requests that the server end the movie.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RequestMovieEnd(Event @event, string[] args, EventContext context)
		{
			Game1.player.team.requestMovieEndEvent.Fire(Game1.player.UniqueMultiplayerID);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RestoreStashedItem(Event @event, string[] args, EventContext context)
		{
			Game1.player.TemporaryItem = null;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AdvancedMove(Event @event, string[] args, EventContext context)
		{
			@event.setUpAdvancedMove(args);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void StopRunning(Event @event, string[] args, EventContext context)
		{
			@event.farmer.setRunning(isRunning: false);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Eyes(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var eyes, out var error, "int eyes") || !ArgUtility.TryGetInt(args, 2, out var blinkTimer, out error, "int blinkTimer"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.farmer.currentEyes = eyes;
			@event.farmer.blinkTimer = blinkTimer;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		[OtherNames(new string[] { "mailReceived" })]
		public static void AddMailReceived(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: true, "string mailId") || !ArgUtility.TryGetOptionalBool(args, 2, out var add, out error, defaultValue: true, "bool add"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.mailReceived.Toggle(mailId, add);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddWorldState(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var worldStateId, out var error, allowBlank: true, "string worldStateId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.worldStateIDs.Add(worldStateId);
			Game1.netWorldState.Value.addWorldStateID(worldStateId);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Fade(Event @event, string[] args, EventContext context)
		{
			string option = ArgUtility.Get(args, 1);
			if (option == "unfade")
			{
				Game1.fadeIn = false;
				Game1.fadeToBlack = false;
				@event.CurrentCommand++;
				return;
			}
			Game1.fadeToBlack = true;
			Game1.fadeIn = true;
			if (Game1.fadeToBlackAlpha >= 0.97f)
			{
				if (option == null)
				{
					Game1.fadeIn = false;
				}
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeMapTile(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var layerId, out var error, allowBlank: true, "string layerId") || !ArgUtility.TryGetPoint(args, 2, out var tilePos, out error, "Point tilePos") || !ArgUtility.TryGetInt(args, 4, out var newTileIndex, out error, "int newTileIndex"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Layer layer = context.Location.map.GetLayer(layerId);
			if (layer == null)
			{
				context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId);
				return;
			}
			int tileX = @event.OffsetTileX(tilePos.X);
			int tileY = @event.OffsetTileY(tilePos.Y);
			Tile tile = layer.Tiles[tileX, tileY];
			if (tile == null)
			{
				context.LogErrorAndSkip($"the '{context.Location.NameOrUniqueName}' location doesn't have required tile ({tilePos.X}, {tilePos.Y})" + ((tileX != tilePos.X || tileY != tilePos.Y) ? $" (adjusted to ({tileX}, {tileY})" : "") + " on layer " + layerId);
			}
			else
			{
				tile.TileIndex = newTileIndex;
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeSprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptional(args, 2, out var spriteSuffix, out error, null, allowBlank: true, "string spriteSuffix"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			if (spriteSuffix != null)
			{
				actor.spriteOverridden = true;
				actor.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(actor.Name) + "_" + spriteSuffix);
			}
			else
			{
				actor.spriteOverridden = false;
				actor.ChooseAppearance();
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void WaitForAllStationary(Event @event, string[] args, EventContext context)
		{
			List<NPCController> npcControllers = @event.npcControllers;
			bool anyMoving = npcControllers != null && npcControllers.Count > 0;
			if (!anyMoving)
			{
				foreach (NPC actor in @event.actors)
				{
					if (actor.isMoving())
					{
						anyMoving = true;
						break;
					}
				}
			}
			if (!anyMoving)
			{
				foreach (Farmer farmerActor in @event.farmerActors)
				{
					if (farmerActor.isMoving())
					{
						anyMoving = true;
						break;
					}
				}
			}
			if (!anyMoving)
			{
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ProceedPosition(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Character character = @event.getCharacterByName(actorName);
			if (character == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'");
				return;
			}
			@event.continueAfterMove = true;
			try
			{
				if (character.isMoving())
				{
					List<NPCController> npcControllers = @event.npcControllers;
					if (npcControllers == null || npcControllers.Count != 0)
					{
						return;
					}
				}
				character.Halt();
				@event.CurrentCommand++;
			}
			catch
			{
				@event.CurrentCommand++;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangePortrait(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptional(args, 2, out var portraitSuffix, out error, null, allowBlank: true, "string portraitSuffix"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC n = @event.getActorByName(actorName, out isOptionalNpc) ?? Game1.getCharacterFromName(actorName);
			if (n == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			if (portraitSuffix != null)
			{
				n.portraitOverridden = true;
				n.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(n.Name) + "_" + portraitSuffix);
			}
			else
			{
				n.portraitOverridden = false;
				n.ChooseAppearance();
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeYSourceRectOffset(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetInt(args, 2, out var ySourceRectOffset, out error, "int ySourceRectOffset"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC n = @event.getActorByName(actorName, out isOptionalNpc);
			if (n == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			n.ySourceRectOffset = ySourceRectOffset;
			@event.CurrentCommand++;
		}

		/// <summary>Set the display name for an event actor to an exact value.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeName(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var newName, out error, allowBlank: true, "string newName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC n = @event.getActorByName(actorName, out isOptionalNpc);
			if (n == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			n.displayName = newName;
			@event.CurrentCommand++;
		}

		/// <summary>Set the display name for an event actor to a translation key.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void TranslateName(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var translationKey, out error, allowBlank: true, "string translationKey"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC n = @event.getActorByName(actorName, out isOptionalNpc);
			if (n == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			n.displayName = Game1.content.LoadString(translationKey);
			@event.CurrentCommand++;
		}

		/// <summary>Replace an NPC in the event with a temporary copy that only exists for the duration of the event. This allows changing the NPC in the event (e.g. renaming them) without affecting the real NPC.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ReplaceWithClone(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			@event.actors.Remove(actor);
			@event.actors.Add(new NPC(actor.Sprite.Clone(), actor.Position, actor.FacingDirection, actor.Name)
			{
				Birthday_Day = actor.Birthday_Day,
				Birthday_Season = actor.Birthday_Season,
				Gender = actor.Gender,
				Portrait = actor.Portrait,
				EventActor = true,
				displayName = actor.displayName,
				drawOffset = actor.drawOffset,
				TemporaryDialogue = new Stack<Dialogue>(actor.CurrentDialogue.Select((Dialogue p) => new Dialogue(p)))
			});
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PlayFramesAhead(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var framesToSkip, out var error, "int framesToSkip"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.CurrentCommand++;
			for (int i = 0; i < framesToSkip; i++)
			{
				@event.Update(context.Location, context.Time);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ShowKissFrame(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptionalBool(args, 2, out var flip, out error, defaultValue: false, "bool flip"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
			if (actor == null)
			{
				context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			CharacterData data = actor.GetData();
			int spouseFrame = data?.KissSpriteIndex ?? 28;
			bool facingRight = data?.KissSpriteFacingRight ?? true;
			if (flip)
			{
				facingRight = !facingRight;
			}
			actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(spouseFrame, 1000, secondaryArm: false, facingRight)
			});
			@event.CurrentCommand++;
		}

		/// <remarks>Format: <c>addTemporaryActor name spriteWidth spriteHeight xPosition yPosition facingDirection breather=true animal=false</c>.</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddTemporaryActor(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var spriteAssetName, out var error, allowBlank: true, "string spriteAssetName") || !ArgUtility.TryGetPoint(args, 2, out var spriteSize, out error, "Point spriteSize") || !ArgUtility.TryGetVector2(args, 4, out var tile, out error, integerOnly: false, "Vector2 tile") || !ArgUtility.TryGetDirection(args, 6, out var facingDirection, out error, "int facingDirection") || !ArgUtility.TryGetOptionalBool(args, 7, out var isBreather, out error, defaultValue: true, "bool isBreather") || !ArgUtility.TryGetOptional(args, 8, out var typeOrDisplayName, out error, null, allowBlank: true, "string typeOrDisplayName") || !ArgUtility.TryGetOptional(args, 9, out var overrideName, out error, null, allowBlank: false, "string overrideName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			string textureLocation = "Characters\\";
			bool hasValidTypeKey = true;
			switch (typeOrDisplayName?.ToLower())
			{
			case "animal":
				textureLocation = "Animals\\";
				break;
			case "monster":
				textureLocation = "Characters\\Monsters\\";
				break;
			default:
				hasValidTypeKey = false;
				break;
			case "character":
				break;
			}
			string fullSpriteAssetName = textureLocation + spriteAssetName;
			if (!Game1.content.DoesAssetExist<Texture2D>(fullSpriteAssetName))
			{
				string newSpriteAssetName = spriteAssetName.Replace('_', ' ');
				string newFullSpriteAssetName = textureLocation + newSpriteAssetName;
				if (newSpriteAssetName != spriteAssetName && Game1.content.DoesAssetExist<Texture2D>(newFullSpriteAssetName))
				{
					spriteAssetName = newSpriteAssetName;
					fullSpriteAssetName = newFullSpriteAssetName;
				}
			}
			NPC n = new NPC(new AnimatedSprite(@event.festivalContent, fullSpriteAssetName, 0, spriteSize.X, spriteSize.Y), @event.OffsetPosition(tile * 64f), facingDirection, spriteAssetName, @event.festivalContent);
			n.AllowDynamicAppearance = false;
			n.Breather = isBreather;
			n.HideShadow = n.Sprite.SpriteWidth >= 32;
			n.TemporaryDialogue = new Stack<Dialogue>();
			if (!hasValidTypeKey && typeOrDisplayName != null)
			{
				n.displayName = typeOrDisplayName;
			}
			if (@event.isFestival && @event.TryGetFestivalDialogueForYear(n, n.Name, out var dialogue))
			{
				n.CurrentDialogue.Push(dialogue);
			}
			if (overrideName != null)
			{
				n.Name = overrideName;
			}
			n.EventActor = true;
			@event.actors.Add(n);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ChangeToTemporaryMap(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var mapName, out var error, allowBlank: true, "string mapName") || !ArgUtility.TryGetOptionalBool(args, 2, out var shouldPan, out error, defaultValue: true, "bool shouldPan"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.temporaryLocation = ((mapName == "Town") ? new Town("Maps\\Town", "Temp") : ((@event.isFestival && mapName.Contains("Town")) ? new Town("Maps\\" + mapName, "Temp") : new GameLocation("Maps\\" + mapName, "Temp")));
			@event.temporaryLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Event e = Game1.currentLocation.currentEvent;
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation.currentEvent = null;
			Game1.currentLightSources.Clear();
			Game1.currentLocation = @event.temporaryLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.currentLocation.UpdateMapSeats();
			Game1.currentLocation.currentEvent = e;
			@event.CurrentCommand++;
			Game1.player.currentLocation = Game1.currentLocation;
			@event.farmer.currentLocation = Game1.currentLocation;
			Game1.currentLocation.ResetForEvent(@event);
			if (shouldPan)
			{
				Game1.panScreen(0, 0);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PositionOffset(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetPoint(args, 2, out var offset, out error, "Point offset") || !ArgUtility.TryGetOptionalBool(args, 4, out var continueImmediately, out error, defaultValue: false, "bool continueImmediately"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.position.X += offset.X;
					f.position.Y += offset.Y;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.position.X += offset.X;
				n.position.Y += offset.Y;
			}
			@event.CurrentCommand++;
			if (continueImmediately)
			{
				@event.Update(context.Location, context.Time);
			}
		}

		/// <remarks>Format: <c>question &lt;questionKey (forkN to make the nth answer fork)&gt; "question#answer1#answer2#...#answerN"</c>.</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Question(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var dialogueKey, out var error, allowBlank: true, "string dialogueKey") || !ArgUtility.TryGet(args, 2, out var rawQuestionsAndAnswers, out error, allowBlank: true, "string rawQuestionsAndAnswers"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string[] questionAndAnswers = LegacyShims.SplitAndTrim(rawQuestionsAndAnswers, '#');
				string question = questionAndAnswers[0];
				Response[] answers = new Response[questionAndAnswers.Length - 1];
				for (int i = 1; i < questionAndAnswers.Length; i++)
				{
					answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswers[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, answers, dialogueKey);
			}
		}

		/// <remarks>Format: <c>quickQuestion question#answer1#answer2#...#answerN(break)answerLogic1(break)answerLogic2(break)...(break)answerLogicN</c>. Use <c>\</c> instead of <c>/</c> inside the <c>answerLogic</c> sections.</remarks>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void QuickQuestion(Event @event, string[] args, EventContext context)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string currentCommand = @event.GetCurrentCommand();
				string[] questionAndAnswerSplit = LegacyShims.SplitAndTrim(LegacyShims.SplitAndTrim(currentCommand.Substring(currentCommand.IndexOf(' ') + 1), "(break)")[0], '#');
				string question = questionAndAnswerSplit[0];
				Response[] answers = new Response[questionAndAnswerSplit.Length - 1];
				for (int i = 1; i < questionAndAnswerSplit.Length; i++)
				{
					answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswerSplit[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, answers, "quickQuestion");
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void DrawOffset(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetVector2(args, 2, out var offset, out error, integerOnly: true, "Vector2 offset"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc = false;
			int farmerNumber;
			Character character = (@event.IsFarmerActorId(actorName, out farmerNumber) ? ((Character)@event.GetFarmerActor(farmerNumber)) : ((Character)@event.getActorByName(actorName, out isOptionalNpc)));
			if (character == null)
			{
				context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			character.drawOffset = offset * 4f;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void HideShadow(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetBool(args, 2, out var hideShadow, out error, "bool hideShadow"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			bool isOptionalNpc;
			NPC character = @event.getActorByName(actorName, out isOptionalNpc);
			if (character == null)
			{
				context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			character.HideShadow = hideShadow;
			@event.CurrentCommand++;
		}

		/// <summary>Animates properties of character "jumps". If any argument is set to "keep", it'll retain the current value.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AnimateHeight(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGet(args, 2, out var rawHeight, out error, allowBlank: true, "string rawHeight") || !ArgUtility.TryGet(args, 3, out var rawGravity, out error, allowBlank: true, "string rawGravity") || !ArgUtility.TryGet(args, 4, out var rawVelocity, out error, allowBlank: true, "string rawVelocity"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			int? height = null;
			float? jumpGravity = null;
			float? jumpVelocity = null;
			if (rawHeight != "keep")
			{
				if (!int.TryParse(rawHeight, out var parsed))
				{
					context.LogErrorAndSkip("required index 2 must be 'keep' or an integer height");
					return;
				}
				height = parsed;
			}
			if (rawGravity != "keep")
			{
				if (!float.TryParse(rawGravity, out var parsed2))
				{
					context.LogErrorAndSkip("required index 3 must be 'keep' or a float gravity value");
					return;
				}
				jumpGravity = parsed2;
			}
			if (rawVelocity != "keep")
			{
				if (!float.TryParse(rawVelocity, out var parsed3))
				{
					context.LogErrorAndSkip("required index 4 must be 'keep' or a float velocity value");
					return;
				}
				jumpVelocity = parsed3;
			}
			bool isOptionalNpc = false;
			int farmerNumber;
			Character character = (@event.IsFarmerActorId(actorName, out farmerNumber) ? ((Character)@event.GetFarmerActor(farmerNumber)) : ((Character)@event.getActorByName(actorName, out isOptionalNpc)));
			if (character == null)
			{
				context.LogErrorAndSkip("no actor found with name '" + actorName + "'", isOptionalNpc);
				return;
			}
			if (height.HasValue)
			{
				character.yJumpOffset = -height.Value;
			}
			if (jumpGravity.HasValue)
			{
				character.yJumpGravity = jumpGravity.Value;
			}
			if (jumpVelocity.HasValue)
			{
				character.yJumpVelocity = jumpVelocity.Value;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Jump(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetOptionalFloat(args, 2, out var jumpV, out error, 8f, "float jumpV") || !ArgUtility.TryGetOptionalBool(args, 3, out var noSound, out error, defaultValue: false, "bool noSound"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				@event.GetFarmerActor(farmerNumber)?.jump(jumpV);
			}
			else
			{
				bool isOptionalNpc;
				NPC actor = @event.getActorByName(actorName, out isOptionalNpc);
				if (actor == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				if (noSound)
				{
					actor.jumpWithoutSound(jumpV);
				}
				else
				{
					actor.jump(jumpV);
				}
			}
			@event.CurrentCommand++;
			@event.Update(context.Location, context.Time);
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void FarmerEat(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, allowBlank: true, "string itemId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Object toEat = ItemRegistry.Create<Object>("(O)" + itemId);
			@event.farmer.eatObject(toEat, overrideFullness: true);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void SpriteText(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var colorIndex, out var error, "int colorIndex") || !ArgUtility.TryGet(args, 2, out var text, out error, allowBlank: true, "string text"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			@event.int_useMeForAnything2 = colorIndex;
			@event.float_useMeForAnything += context.Time.ElapsedGameTime.Milliseconds;
			if (@event.float_useMeForAnything > 80f)
			{
				if (@event.int_useMeForAnything >= text.Length)
				{
					if (@event.float_useMeForAnything >= 2500f)
					{
						@event.int_useMeForAnything = 0;
						@event.float_useMeForAnything = 0f;
						@event.spriteTextToDraw = "";
						@event.CurrentCommand++;
					}
				}
				else
				{
					@event.int_useMeForAnything++;
					@event.float_useMeForAnything = 0f;
					Game1.playSound("dialogueCharacter");
				}
			}
			@event.spriteTextToDraw = text;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void IgnoreCollisions(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.IsFarmerActorId(actorName, out var farmerNumber))
			{
				Farmer f = @event.GetFarmerActor(farmerNumber);
				if (f != null)
				{
					f.ignoreCollisions = true;
				}
			}
			else
			{
				bool isOptionalNpc;
				NPC n = @event.getActorByName(actorName, out isOptionalNpc);
				if (n == null)
				{
					context.LogErrorAndSkip("no NPC found with name '" + actorName + "'", isOptionalNpc);
					return;
				}
				n.isCharging = true;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void ScreenFlash(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetFloat(args, 1, out var flashAlpha, out var error, "float flashAlpha"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.flashAlpha = flashAlpha;
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GrandpaCandles(Event @event, string[] args, EventContext context)
		{
			int candles = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
			Game1.getFarm().grandpaScore.Value = candles;
			for (int i = 0; i < candles; i++)
			{
				DelayedAction.playSoundAfterDelay("fireball", 100 * i);
			}
			Game1.getFarm().addGrandpaCandles();
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GrandpaEvaluation2(Event @event, string[] args, EventContext context)
		{
			switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
			{
			case 1:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1306") + "\"");
				break;
			case 2:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1307") + "\"");
				break;
			case 3:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1308") + "\"");
				break;
			case 4:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1309") + "\"");
				break;
			}
			Game1.player.eventsSeen.Remove("2146991");
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GrandpaEvaluation(Event @event, string[] args, EventContext context)
		{
			switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
			{
			case 1:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1315") + "\"");
				break;
			case 2:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1316") + "\"");
				break;
			case 3:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1317") + "\"");
				break;
			case 4:
				@event.ReplaceCurrentCommand("speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1318") + "\"");
				break;
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void LoadActors(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var layerId, out var error, allowBlank: true, "string layerId"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Layer layer = @event.temporaryLocation?.map.GetLayer(layerId);
			if (layer == null)
			{
				context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId);
				return;
			}
			@event.actors.Clear();
			@event.npcControllers?.Clear();
			Dictionary<int, string> actorNamesByIndex = new Dictionary<int, string>();
			foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
			{
				int index = entry.Value.FestivalVanillaActorIndex;
				if (index >= 0 && !actorNamesByIndex.TryAdd(index, entry.Key))
				{
					Game1.log.Warn($"NPC '{entry.Key}' has the same festival actor index as '{actorNamesByIndex[index]}' in Data/Characters, so it'll be ignored for festival placement.");
				}
			}
			HashSet<string> npcNames = new HashSet<string>();
			for (int x = 0; x < layer.LayerWidth; x++)
			{
				for (int y = 0; y < layer.LayerHeight; y++)
				{
					Tile tile = layer.Tiles[x, y];
					if (tile != null)
					{
						int tileIndex = tile.TileIndex;
						int actorIndex = tileIndex / 4;
						int actorFacingDirection = tileIndex % 4;
						if (actorNamesByIndex.TryGetValue(actorIndex, out var actorName) && Game1.getCharacterFromName(actorName) != null && (!(actorName == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved")))
						{
							@event.addActor(actorName, x, y, actorFacingDirection, @event.temporaryLocation);
							npcNames.Add(actorName);
						}
					}
				}
			}
			if (@event.festivalData != null && @event.TryGetFestivalDataForYear(layerId + "_additionalCharacters", out var data, out var keyName))
			{
				string[] array = Event.ParseCommands(data, context.Event.farmer);
				for (int i = 0; i < array.Length; i++)
				{
					string[] curArgs = ArgUtility.SplitBySpaceQuoteAware(array[i]);
					if (!ArgUtility.TryGet(curArgs, 0, out var actorName2, out error, allowBlank: true, "string actorName") || !ArgUtility.TryGetPoint(curArgs, 1, out var tile2, out error, "Point tile") || !ArgUtility.TryGetDirection(curArgs, 3, out var direction, out error, "int direction"))
					{
						context.LogError($"'{keyName}' festival field has invalid additional character entry '{string.Join(" ", curArgs)}': {error}");
					}
					else if (Game1.getCharacterFromName(actorName2) != null)
					{
						if (!(actorName2 == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
						{
							@event.addActor(actorName2, tile2.X, tile2.Y, direction, @event.temporaryLocation);
							npcNames.Add(actorName2);
						}
					}
					else
					{
						context.LogError($"'{keyName}' festival field has invalid additional character entry '{string.Join(" ", curArgs)}': no NPC found with name '{actorName2}'");
					}
				}
			}
			if (layerId == "Set-Up")
			{
				foreach (string npcName in npcNames)
				{
					NPC npc = Game1.getCharacterFromName(npcName);
					if (!npc.isMarried() || npc.getSpouse() == null || npc.getSpouse().getChildren().Count <= 0)
					{
						continue;
					}
					Farmer spouse = Game1.player;
					if (npc.getSpouse() != null)
					{
						spouse = npc.getSpouse();
					}
					List<Child> children = spouse.getChildren();
					npc = @event.getCharacterByName(npcName) as NPC;
					for (int childIndex = 0; childIndex < children.Count; childIndex++)
					{
						Child child = children[childIndex];
						if (child.Age < 3)
						{
							continue;
						}
						Child childActor = new Child(child.Name, child.Gender == Gender.Male, child.darkSkinned.Value, spouse);
						childActor.NetFields.CopyFrom(child.NetFields);
						childActor.Halt();
						Point[] directionOffsets = npc.FacingDirection switch
						{
							0 => new Point[4]
							{
								new Point(0, 1),
								new Point(-1, 0),
								new Point(1, 0),
								new Point(0, -1)
							}, 
							2 => new Point[4]
							{
								new Point(0, -1),
								new Point(1, 0),
								new Point(-1, 0),
								new Point(0, 1)
							}, 
							3 => new Point[4]
							{
								new Point(1, 0),
								new Point(0, -1),
								new Point(0, 1),
								new Point(-1, 0)
							}, 
							1 => new Point[4]
							{
								new Point(-1, 0),
								new Point(0, 1),
								new Point(0, -1),
								new Point(1, 0)
							}, 
							_ => new Point[4]
							{
								new Point(-1, 0),
								new Point(1, 0),
								new Point(0, -1),
								new Point(0, 1)
							}, 
						};
						Point spawnPoint = npc.TilePoint;
						List<Point> pointsToCheck = new List<Point>();
						Point[] array2 = directionOffsets;
						for (int i = 0; i < array2.Length; i++)
						{
							Point offset = array2[i];
							pointsToCheck.Add(new Point(spawnPoint.X + offset.X, spawnPoint.Y + offset.Y));
						}
						bool foundSpawn = false;
						for (int iteration = 0; iteration < 5; iteration++)
						{
							if (foundSpawn)
							{
								break;
							}
							int currentCheckCount = pointsToCheck.Count;
							for (int j = 0; j < currentCheckCount; j++)
							{
								Point currentPoint = pointsToCheck[0];
								pointsToCheck.RemoveAt(0);
								if (IsWalkableTileCheck(currentPoint))
								{
									if (HasClearanceCheck(currentPoint))
									{
										foundSpawn = true;
										spawnPoint = currentPoint;
										break;
									}
									array2 = directionOffsets;
									for (int i = 0; i < array2.Length; i++)
									{
										Point offset2 = array2[i];
										pointsToCheck.Add(new Point(currentPoint.X + offset2.X, currentPoint.Y + offset2.Y));
									}
								}
							}
						}
						if (foundSpawn)
						{
							childActor.setTilePosition(spawnPoint.X, spawnPoint.Y);
							childActor.DefaultPosition = npc.DefaultPosition;
							childActor.faceDirection(npc.FacingDirection);
							childActor.EventActor = true;
							childActor.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(spawnPoint.X * 64, spawnPoint.Y * 64, 64, 64);
							childActor.squareMovementFacingPreference = -1;
							childActor.walkInSquare(3, 3, 2000);
							childActor.controller = null;
							childActor.temporaryController = null;
							@event.actors.Add(childActor);
						}
					}
				}
			}
			@event.CurrentCommand++;
			bool HasClearanceCheck(Point point)
			{
				int clearance = 1;
				for (int k = point.X - clearance; k <= point.X + clearance; k++)
				{
					for (int l = point.Y - clearance; l <= point.Y + clearance; l++)
					{
						if (@event.temporaryLocation.IsTileBlockedBy(new Vector2(k, l)))
						{
							return false;
						}
						foreach (NPC actor in @event.actors)
						{
							if (!(actor is Child))
							{
								Point tile3 = actor.TilePoint;
								if (tile3.X == k && tile3.Y == l)
								{
									return false;
								}
							}
						}
					}
				}
				return true;
			}
			bool IsWalkableTileCheck(Point point)
			{
				return @event.temporaryLocation.isTilePassable(new Location(point.X, point.Y), Game1.viewport);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void PlayerControl(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var sequenceId, out var error, allowBlank: true, "string sequenceId"))
			{
				context.LogErrorAndSkip(error);
			}
			else if (!@event.playerControlSequence)
			{
				@event.setUpPlayerControlSequence(sequenceId);
			}
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void RemoveSprite(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetVector2(args, 1, out var tile, out var error, integerOnly: true, "Vector2 tile"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Vector2 tilePixel = @event.OffsetPosition(tile * 64f);
			Game1.currentLocation.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.position == tilePixel);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Viewport(Event @event, string[] args, EventContext context)
		{
			if (ArgUtility.Get(args, 1) == "move")
			{
				if (!ArgUtility.TryGetPoint(args, 2, out var direction, out var error, "Point direction") || !ArgUtility.TryGetInt(args, 4, out var duration, out error, "int duration"))
				{
					context.LogErrorAndSkip(error);
					return;
				}
				@event.viewportTarget = new Vector3(direction.X, direction.Y, duration);
			}
			else
			{
				Point position = Point.Zero;
				string action = null;
				bool shouldFade = false;
				string option = null;
				string error3;
				if (!int.TryParse(args[1], out var _) && ArgUtility.TryGet(args, 1, out var NPCTarget, out var error2, allowBlank: true, "string NPCTarget"))
				{
					position = ((!(NPCTarget == "player")) ? @event.getActorByName(NPCTarget).TilePoint : Game1.MasterPlayer.TilePoint);
					if (!ArgUtility.TryGetOptional(args, 2, out action, out error2, null, allowBlank: true, "action") || !ArgUtility.TryGetOptionalBool(args, (action == "clamp") ? 3 : 2, out shouldFade, out error2, defaultValue: false, "shouldFade") || !ArgUtility.TryGetOptional(args, (action == "clamp") ? 4 : 2, out option, out error2, null, allowBlank: true, "option"))
					{
						context.LogErrorAndSkip(error2);
					}
				}
				else if (!ArgUtility.TryGetPoint(args, 1, out position, out error3, "position") || !ArgUtility.TryGetOptional(args, 3, out action, out error3, null, allowBlank: true, "action") || !ArgUtility.TryGetOptionalBool(args, (action == "clamp") ? 4 : 3, out shouldFade, out error3, defaultValue: false, "shouldFade") || !ArgUtility.TryGetOptional(args, (action == "clamp") ? 5 : 4, out option, out error3, null, allowBlank: true, "option"))
				{
					context.LogErrorAndSkip(error3);
					return;
				}
				if (@event.aboveMapSprites != null && position.X < 0)
				{
					@event.aboveMapSprites.Clear();
					@event.aboveMapSprites = null;
				}
				Game1.viewportFreeze = true;
				int targetTileX = @event.OffsetTileX(position.X);
				int targetTileY = @event.OffsetTileY(position.Y);
				if (@event.id == "2146991")
				{
					Point grandpaShrinePosition = Game1.getFarm().GetGrandpaShrinePosition();
					targetTileX = grandpaShrinePosition.X;
					targetTileY = grandpaShrinePosition.Y;
				}
				Game1.viewport.X = targetTileX * 64 + 32 - Game1.viewport.Width / 2;
				Game1.viewport.Y = targetTileY * 64 + 32 - Game1.viewport.Height / 2;
				if (Game1.viewport.X > 0 && Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
				{
					Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
				}
				if (Game1.viewport.Y > 0 && Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
				{
					Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
				}
				if (action == "clamp")
				{
					if (Game1.currentLocation.map.DisplayWidth >= Game1.viewport.Width)
					{
						if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
						{
							Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
						}
						if (Game1.viewport.X < 0)
						{
							Game1.viewport.X = 0;
						}
					}
					else
					{
						Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth / 2 - Game1.viewport.Width / 2;
					}
					if (Game1.currentLocation.map.DisplayHeight >= Game1.viewport.Height)
					{
						if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
						{
							Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height;
						}
					}
					else
					{
						Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight / 2 - Game1.viewport.Height / 2;
					}
					if (Game1.viewport.Y < 0)
					{
						Game1.viewport.Y = 0;
					}
				}
				if (shouldFade)
				{
					Game1.fadeScreenToBlack();
					Game1.fadeToBlackAlpha = 1f;
					Game1.nonWarpFade = true;
				}
				if (option == "unfreeze")
				{
					Game1.viewportFreeze = false;
				}
				if (Game1.gameMode == 2)
				{
					Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
				}
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void BroadcastEvent(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGetOptionalBool(args, 1, out var useLocalFarmer, out var error, defaultValue: false, "bool useLocalFarmer"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.farmer == Game1.player)
			{
				if (@event.id == "558291" || @event.id == "558292")
				{
					useLocalFarmer = true;
				}
				Game1.multiplayer.broadcastEvent(@event, Game1.currentLocation, Game1.player.positionBeforeEvent, useLocalFarmer);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void AddConversationTopic(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var topicId, out var error, allowBlank: true, "string topicId") || !ArgUtility.TryGetOptionalInt(args, 2, out var daysDuration, out error, 4, "int daysDuration"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (@event.isMemory)
			{
				@event.CurrentCommand++;
				return;
			}
			Game1.player.activeDialogueEvents.TryAdd(topicId, daysDuration);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void Dump(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var which, out var error, allowBlank: true, "string which"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			if (!(which == "girls"))
			{
				if (!(which == "guys"))
				{
					context.LogErrorAndSkip("unknown ID '" + which + "', expected 'girls' or 'guys'");
					return;
				}
				Game1.player.activeDialogueEvents["dumped_Guys"] = 7;
				Game1.player.activeDialogueEvents["secondChance_Guys"] = 14;
			}
			else
			{
				Game1.player.activeDialogueEvents["dumped_Girls"] = 7;
				Game1.player.activeDialogueEvents["secondChance_Girls"] = 14;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void EventSeen(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var eventId, out var error, allowBlank: false, "string eventId") || !ArgUtility.TryGetOptionalBool(args, 2, out var seen, out error, defaultValue: true, "bool seen"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.eventsSeen.Toggle(eventId, seen);
			if (eventId == @event.id)
			{
				@event.markEventSeen = false;
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.DebugCommandHandlerDelegate" />
		public static void QuestionAnswered(Event @event, string[] args, EventContext context)
		{
			if (!ArgUtility.TryGet(args, 1, out var questionId, out var error, allowBlank: false, "string questionId") || !ArgUtility.TryGetOptionalBool(args, 2, out var seen, out error, defaultValue: true, "bool seen"))
			{
				context.LogErrorAndSkip(error);
				return;
			}
			Game1.player.dialogueQuestionsAnswered.Toggle(questionId, seen);
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void GainSkill(Event @event, string[] args, EventContext context)
		{
			int whichSkill = Farmer.getSkillNumberFromName(args[1]);
			int level = Convert.ToInt32(args[2]);
			if (Game1.player.GetUnmodifiedSkillLevel(whichSkill) < level)
			{
				Game1.player.setSkillLevel(args[1], level);
			}
			@event.CurrentCommand++;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.EventCommandDelegate" />
		public static void MoveToSoup(Event @event, string[] args, EventContext context)
		{
			if (Game1.year % 2 == 1)
			{
				@event.setUpAdvancedMove(new string[9] { "", "Gus", "false", "0", "-1", "5", "0", "4", "1000" });
				@event.setUpAdvancedMove(new string[5] { "", "Jodi", "false", "0", "-2" });
				@event.setUpAdvancedMove(new string[11]
				{
					"", "Clint", "false", "0", "1", "-1", "0", "0", "3", "-2",
					"0"
				});
				@event.setUpAdvancedMove(new string[5] { "", "Emily", "false", "3", "0" });
				@event.setUpAdvancedMove(new string[7] { "", "Pam", "false", "0", "2", "7", "0" });
			}
			else
			{
				@event.setUpAdvancedMove(new string[5] { "", "Pierre", "false", "3", "0" });
				@event.setUpAdvancedMove(new string[9] { "", "Pam", "false", "0", "2", "-4", "0", "0", "1" });
				@event.setUpAdvancedMove(new string[9] { "", "Abigail", "false", "4", "0", "0", "-3", "1", "4000" });
				@event.setUpAdvancedMove(new string[9] { "", "Alex", "false", "-5", "0", "0", "-1", "3", "2000" });
				@event.setUpAdvancedMove(new string[5] { "", "Gus", "false", "0", "-1" });
			}
			@event.CurrentCommand++;
		}
	}

	/// <summary>The event commands indexed by name.</summary>
	/// <remarks>Command names are case-insensitive.</remarks>
	protected static readonly Dictionary<string, EventCommandDelegate> Commands = new Dictionary<string, EventCommandDelegate>(StringComparer.OrdinalIgnoreCase);

	/// <summary>Alternate names for event commands.</summary>
	protected static readonly Dictionary<string, string> CommandAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The registered command names.</summary>
	protected static readonly HashSet<string> CommandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The event preconditions indexed by name.</summary>
	/// <remarks>Precondition names are case-<strong>sensitive</strong>.</remarks>
	protected static readonly Dictionary<string, EventPreconditionDelegate> Preconditions = new Dictionary<string, EventPreconditionDelegate>(StringComparer.OrdinalIgnoreCase);

	/// <summary>Alternate names for event preconditions (e.g. shorthand or acronyms).</summary>
	/// <remarks>Aliases are case-sensitive for compatibility with older preconditions like 'h' vs 'H'.</remarks>
	private static readonly Dictionary<string, string> PreconditionAliases = new Dictionary<string, string>();

	private const float timeBetweenSpeech = 500f;

	public const string festivalTextureName = "Maps\\Festivals";

	private string festivalDataAssetName;

	/// <summary>
	///   The unique identifier for the event, if available. This may be...
	///   <list type="bullet">
	///     <item><description>for a regular event, the unique event ID from its data file (i.e. the first number in its entry key);</description></item>
	///     <item><description>for a generated event, an <see cref="T:StardewValley.Constants.EventIds" /> constant;</description></item>
	///     <item><description>for a festival, <c>festival_{asset name}</c> (like <c>festival_fall16</c>);</description></item>
	///     <item><description>else <see cref="F:StardewValley.Constants.EventIds.Unknown" />.</description></item>
	///   </list>
	/// </summary>
	public string id = "-1";

	/// <summary>The data asset name from which the event script was taken, or <c>null</c> for a generated event.</summary>
	public string fromAssetName;

	public bool isFestival;

	public bool isWedding;

	public bool isMemory;

	/// <summary>Whether the player can skip the rest of the event.</summary>
	public bool skippable;

	/// <summary>The actions to perform when the event is skipped, if any.</summary>
	public string[] actionsOnSkip;

	public bool skipped;

	public bool forked;

	public bool eventSwitched;

	/// <summary>Whether we need to notify the dedicated server once the event finishes running.</summary>
	internal bool notifyWhenDone;

	/// <summary>The location name sent to the dedicated server to identify an event.</summary>
	internal string notifyLocationName;

	/// <summary>Whether the location name sent to the dedicated server corresponds to a structure.</summary>
	internal byte notifyLocationIsStructure;

	private readonly LocalizedContentManager festivalContent = Game1.content.CreateTemporary();

	public string[] eventCommands;

	public int currentCommand;

	private Dictionary<string, Vector3> actorPositionsAfterMove;

	private float timeAccumulator;

	private Vector3 viewportTarget;

	private Color previousAmbientLight;

	private HashSet<long> festivalWinners = new HashSet<long>();

	private GameLocation temporaryLocation;

	private Dictionary<string, string> festivalData;

	private Texture2D _festivalTexture;

	private bool drawTool;

	private string hostMessageKey;

	private int previousFacingDirection = -1;

	private int previousAnswerChoice = -1;

	private bool startSecretSantaAfterDialogue;

	private List<Farmer> iceFishWinners;

	protected static LocalizedContentManager FestivalReadContentLoader;

	protected bool _playerControlSequence;

	protected bool _repeatingLocationSpecificCommand;

	[NonInstancedStatic]
	public static HashSet<string> invalidFestivals = new HashSet<string>();

	public List<NPC> actors = new List<NPC>();

	public List<Object> props = new List<Object>();

	public List<Prop> festivalProps = new List<Prop>();

	public List<Farmer> farmerActors = new List<Farmer>();

	public Dictionary<string, Dictionary<ISalable, ItemStockInformation>> festivalShops;

	public List<NPCController> npcControllers;

	internal NPC festivalHost;

	public NPC secretSantaRecipient;

	public NPC mySecretSanta;

	public TemporaryAnimatedSpriteList underwaterSprites;

	public TemporaryAnimatedSpriteList aboveMapSprites;

	/// <summary>The custom sounds started during the event via <see cref="M:StardewValley.Event.DefaultCommands.PlaySound(StardewValley.Event,System.String[],StardewValley.EventContext)" />.</summary>
	public IDictionary<string, List<ICue>> CustomSounds = new Dictionary<string, List<ICue>>();

	public ICustomEventScript currentCustomEventScript;

	public bool simultaneousCommand;

	public int farmerAddedSpeed;

	public int int_useMeForAnything;

	public int int_useMeForAnything2;

	public float float_useMeForAnything;

	public string playerControlSequenceID;

	public string spriteTextToDraw;

	public bool showActiveObject;

	public bool continueAfterMove;

	public bool specialEventVariable1;

	public bool specialEventVariable2;

	public bool showGroundObjects = true;

	public bool doingSecretSanta;

	public bool showWorldCharacters;

	public bool ignoreObjectCollisions = true;

	public Point playerControlTargetTile;

	public List<Vector2> characterWalkLocations = new List<Vector2>();

	public Vector2 eventPositionTileOffset = Vector2.Zero;

	public int festivalTimer;

	public int grangeScore = -1000;

	public bool grangeJudged;

	/// <summary>Used to offset positions specified in events.</summary>
	public bool ignoreTileOffsets;

	private Stopwatch stopWatch;

	public LocationRequest exitLocation;

	public Action onEventFinished;

	/// <summary>Whether to add this event's ID to <see cref="F:StardewValley.Farmer.eventsSeen" /> when it ends, if it has a valid ID.</summary>
	/// <remarks>This has no effect on <see cref="F:StardewValley.Game1.eventsSeenSinceLastLocationChange" />, which is updated regardless (if it has a valid ID) to prevent event loops.</remarks>
	public bool markEventSeen = true;

	private bool eventFinished;

	private bool gotPet;

	public string FestivalName
	{
		get
		{
			if (!this.TryGetFestivalDataForYear("name", out var name))
			{
				return "";
			}
			return name;
		}
	}

	public bool playerControlSequence
	{
		get
		{
			return this._playerControlSequence;
		}
		set
		{
			if (this._playerControlSequence != value)
			{
				this._playerControlSequence = value;
				if (!this._playerControlSequence)
				{
					this.OnPlayerControlSequenceEnd(this.playerControlSequenceID);
				}
			}
		}
	}

	public Farmer farmer
	{
		get
		{
			if (this.farmerActors.Count <= 0)
			{
				return Game1.player;
			}
			return this.farmerActors[0];
		}
	}

	public Texture2D festivalTexture
	{
		get
		{
			if (this._festivalTexture == null)
			{
				this._festivalTexture = this.festivalContent.Load<Texture2D>("Maps\\Festivals");
			}
			return this._festivalTexture;
		}
	}

	public int CurrentCommand
	{
		get
		{
			return this.currentCommand;
		}
		set
		{
			this.currentCommand = value;
		}
	}

	/// <summary>Register an event command.</summary>
	/// <param name="name">The command name that can be used in event scripts. This is case-insensitive.</param>
	/// <param name="action">The handler to call when the command is used.</param>
	public static void RegisterCommand(string name, EventCommandDelegate action)
	{
		Event.SetupEventCommandsIfNeeded();
		if (Event.Commands.ContainsKey(name))
		{
			Game1.log.Warn("Warning: event command " + name + " is already defined and will be overwritten.");
		}
		Event.Commands[name] = action;
		Event.CommandNames.Add(name);
		Game1.log.Verbose("Registered event command: " + name);
	}

	/// <summary>Register an alternate name for an event command.</summary>
	/// <param name="alias">The alternate name. This is case-insensitive.</param>
	/// <param name="commandName">The original command name to alias. This is case-insensitive.</param>
	public static void RegisterCommandAlias(string alias, string commandName)
	{
		Event.SetupEventCommandsIfNeeded();
		string conflictingName;
		if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(commandName))
		{
			Game1.log.Error($"Can't register event command alias '{alias}' for '{commandName}' because the alias and command name must both be non-null and non-empty strings.");
		}
		else if (Event.Commands.ContainsKey(alias))
		{
			Game1.log.Error($"Can't register event command alias '{alias}' for command '{commandName}', because there's a command with that name.");
		}
		else if (Event.CommandAliases.TryGetValue(alias, out conflictingName))
		{
			Game1.log.Error($"Can't register event command alias '{alias}' for command '{commandName}', because that's already an alias for '{conflictingName}'.");
		}
		else if (!Event.Commands.ContainsKey(commandName))
		{
			Game1.log.Error($"Can't register event command alias '{alias}' for command '{commandName}', because there's no such command.");
		}
		else
		{
			Event.CommandAliases[alias] = commandName;
		}
	}

	/// <summary>Get the actual command name which matches an input command or alias name.</summary>
	/// <param name="name">The command or alias name to resolve.</param>
	/// <param name="actualName">The resolved command name.</param>
	/// <returns>Returns whether a matching command was found.</returns>
	/// <remarks>For example, this can be used to resolve an alias (like <c>mailReceived</c> → <c>AddMailReceived</c>) or normalize the capitalization (like <c>itemnamed</c> → <c>ItemNamed</c>).</remarks>
	public static bool TryResolveCommandName(string name, out string actualName)
	{
		Event.SetupEventCommandsIfNeeded();
		if (Event.CommandAliases.TryGetValue(name, out actualName))
		{
			return true;
		}
		if (Event.CommandNames.TryGetValue(name, out actualName))
		{
			return true;
		}
		actualName = null;
		return false;
	}

	/// <summary>Register an event precondition.</summary>
	/// <param name="name">The precondition key that can be used in event precondition strings. This is case-insensitive.</param>
	/// <param name="action">The handler to call when the precondition is used.</param>
	public static void RegisterPrecondition(string name, EventPreconditionDelegate action)
	{
		Event.SetupEventCommandsIfNeeded();
		if (Event.Preconditions.ContainsKey(name))
		{
			Game1.log.Warn("Warning: event precondition " + name + " is already defined and will be overwritten.");
		}
		if (Event.PreconditionAliases.Remove(name))
		{
			Game1.log.Warn("Warning: '" + name + "' was previously registered as a precondition alias. The alias was removed.");
		}
		Event.Preconditions[name] = action;
		Game1.log.Verbose("Registered precondition: " + name);
	}

	/// <summary>Register an alternate name for an event precondition.</summary>
	/// <param name="alias">The alternate name. This is <strong>case-sensitive</strong> for legacy reasons.</param>
	/// <param name="preconditionName">The original precondition name to alias. This is case-insensitive.</param>
	public static void RegisterPreconditionAlias(string alias, string preconditionName)
	{
		Event.SetupEventCommandsIfNeeded();
		string conflictingName;
		if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(preconditionName))
		{
			Game1.log.Error($"Can't register event precondition alias '{alias}' for '{preconditionName}' because the alias and precondition name must both be non-null and non-empty strings.");
		}
		else if (Event.Preconditions.ContainsKey(alias))
		{
			Game1.log.Error($"Can't register event precondition alias '{alias}' for precondition '{preconditionName}', because there's a precondition with that name.");
		}
		else if (Event.PreconditionAliases.TryGetValue(alias, out conflictingName))
		{
			Game1.log.Error($"Can't register event precondition alias '{alias}' for precondition '{preconditionName}', because that's already an alias for '{conflictingName}'.");
		}
		else if (!Event.Preconditions.ContainsKey(preconditionName))
		{
			Game1.log.Error($"Can't register event precondition alias '{alias}' for precondition '{preconditionName}', because there's no such precondition.");
		}
		else
		{
			Event.PreconditionAliases[alias] = preconditionName;
		}
	}

	/// <summary>Register the vanilla event commands and preconditions if they haven't already been registered.</summary>
	private static void SetupEventCommandsIfNeeded()
	{
		MethodInfo[] array;
		if (Event.Commands.Count == 0)
		{
			MethodInfo[] methods = typeof(DefaultCommands).GetMethods(BindingFlags.Static | BindingFlags.Public);
			array = methods;
			foreach (MethodInfo method in array)
			{
				EventCommandDelegate command = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), method);
				Event.Commands.Add(method.Name, command);
				Event.CommandNames.Add(method.Name);
			}
			array = methods;
			foreach (MethodInfo method2 in array)
			{
				OtherNamesAttribute attribute = method2.GetCustomAttribute<OtherNamesAttribute>();
				if (attribute != null)
				{
					string[] aliases = attribute.Aliases;
					for (int j = 0; j < aliases.Length; j++)
					{
						Event.RegisterCommandAlias(aliases[j], method2.Name);
					}
				}
			}
		}
		if (Event.Preconditions.Count != 0)
		{
			return;
		}
		MethodInfo[] methods2 = typeof(Preconditions).GetMethods(BindingFlags.Static | BindingFlags.Public);
		array = methods2;
		foreach (MethodInfo method3 in array)
		{
			EventPreconditionDelegate preconditionDelegate = (EventPreconditionDelegate)Delegate.CreateDelegate(typeof(EventPreconditionDelegate), method3);
			Event.Preconditions[method3.Name] = preconditionDelegate;
		}
		array = methods2;
		foreach (MethodInfo method4 in array)
		{
			OtherNamesAttribute attribute2 = method4.GetCustomAttribute<OtherNamesAttribute>();
			if (attribute2 != null)
			{
				string[] aliases = attribute2.Aliases;
				for (int j = 0; j < aliases.Length; j++)
				{
					Event.RegisterPreconditionAlias(aliases[j], method4.Name);
				}
			}
		}
	}

	/// <summary>Get the handler for a precondition key, if any.</summary>
	/// <param name="key">The precondition key, which can be either the case-insensitive canonical name (like <c>DaysPlayed</c>) or case-sensitive alias (like <c>j</c>).</param>
	/// <param name="handler">The precondition handler, if found.</param>
	/// <returns>Returns whether a handler was found for the precondition key.</returns>
	public static bool TryGetPreconditionHandler(string key, out EventPreconditionDelegate handler)
	{
		Event.SetupEventCommandsIfNeeded();
		if (Event.PreconditionAliases.TryGetValue(key, out var aliasTarget))
		{
			key = aliasTarget;
		}
		return Event.Preconditions.TryGetValue(key, out handler);
	}

	/// <summary>Get whether an event precondition matches the current context.</summary>
	/// <param name="location">The location which is checking the event.</param>
	/// <param name="eventId">The unique ID for the event being checked.</param>
	/// <param name="precondition">The event precondition string, including the precondition name.</param>
	public static bool CheckPrecondition(GameLocation location, string eventId, string precondition)
	{
		string[] preconditionSplit = ArgUtility.SplitBySpaceQuoteAware(precondition);
		string key = preconditionSplit[0];
		bool match = true;
		if (key.StartsWith('!'))
		{
			key = key.Substring(1);
			match = false;
		}
		if (!Event.TryGetPreconditionHandler(key, out var handler))
		{
			Game1.log.Warn("Unknown precondition for event " + eventId + ": " + precondition);
			return false;
		}
		try
		{
			return handler(location, eventId, preconditionSplit) == match;
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed checking precondition '{precondition}' for event {eventId}.", exception);
			return false;
		}
	}

	/// <summary>Get the handler for an event command key, if any.</summary>
	/// <param name="key">The event command key, which can be either the case-insensitive canonical name (like <c>AddMailReceived</c>) or case-sensitive alias (like <c>mailReceived</c>).</param>
	/// <param name="handler">The event command handler, if found.</param>
	/// <returns>Returns whether a handler was found for the event command key.</returns>
	public static bool TryGetEventCommandHandler(string key, out EventCommandDelegate handler)
	{
		if (Event.CommandAliases.TryGetValue(key, out var aliasTarget))
		{
			key = aliasTarget;
		}
		return Event.Commands.TryGetValue(key, out handler);
	}

	/// <summary>Try to run an event command for the current event.</summary>
	/// <param name="location">The location in which the event is running.</param>
	/// <param name="time">The current game execution time.</param>
	/// <param name="args">The space-delimited event command string, including the command name.</param>
	public virtual void tryEventCommand(GameLocation location, GameTime time, string[] args)
	{
		string commandName = ArgUtility.Get(args, 0);
		if (string.IsNullOrWhiteSpace(commandName))
		{
			this.LogCommandErrorAndSkip(args, "can't run an empty or null command");
			return;
		}
		if (!Event.TryGetEventCommandHandler(commandName, out var command))
		{
			this.LogCommandErrorAndSkip(args, "unknown command '" + commandName + "'");
			return;
		}
		try
		{
			EventContext context = new EventContext(this, location, time, args);
			command(this, args, context);
		}
		catch (Exception e)
		{
			this.LogErrorAndHalt(e);
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="eventString">The raw event script.</param>
	/// <param name="farmerActor">The player to add as an actor in the event script, or <c>null</c> to use <see cref="P:StardewValley.Game1.player" />.</param>
	public Event(string eventString, Farmer farmerActor = null)
		: this(eventString, null, "-1", farmerActor)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="eventString">The raw event script.</param>
	/// <param name="fromAssetName">The data asset name from which the event script was taken, or <c>null</c> for a generated event.</param>
	/// <param name="eventID">The event's unique ID from the event data files, if known. This may be a number matching one of the <see cref="T:StardewValley.Event" /> constants in <see cref="T:StardewValley.Constants.EventIds" /> for a generated event.</param>
	/// <param name="farmerActor">The player to add as an actor in the event script, or <c>null</c> to use <see cref="P:StardewValley.Game1.player" />.</param>
	public Event(string eventString, string fromAssetName, string eventID, Farmer farmerActor = null)
		: this()
	{
		this.fromAssetName = fromAssetName;
		this.id = eventID;
		this.eventCommands = Event.ParseCommands(eventString, farmerActor);
		this.actorPositionsAfterMove = new Dictionary<string, Vector3>();
		this.previousAmbientLight = Game1.ambientLight;
		if (farmerActor != null)
		{
			this.farmerActors.Add(farmerActor);
		}
		this.farmer.canOnlyWalk = true;
		this.farmer.showNotCarrying();
		this.drawTool = false;
		if (eventID == "-2")
		{
			this.isWedding = true;
		}
	}

	/// <summary>Construct an instance.</summary>
	public Event()
	{
		Event.SetupEventCommandsIfNeeded();
	}

	~Event()
	{
		this.notifyDone();
	}

	public static void OnNewDay()
	{
		Event.FestivalReadContentLoader?.Unload();
	}

	/// <summary>Load the raw data for a festival, if it exists and is valid.</summary>
	/// <param name="festival">The festival ID to load, matching the asset name under <c>Data/Festivals</c> (like <samp>spring13</samp>).</param>
	/// <param name="assetName">The asset name for the loaded festival data.</param>
	/// <param name="data">The loaded festival data.</param>
	/// <param name="locationName">The location name in which the festival takes place.</param>
	/// <param name="startTime">The time of day when the festival opens.</param>
	/// <param name="endTime">The time of day when the festival closes.</param>
	/// <returns>Returns whether the festival data was loaded successfully.</returns>
	public static bool tryToLoadFestivalData(string festival, out string assetName, out Dictionary<string, string> data, out string locationName, out int startTime, out int endTime)
	{
		assetName = "Data\\Festivals\\" + festival;
		data = null;
		locationName = null;
		startTime = 0;
		endTime = 0;
		if (Event.invalidFestivals.Contains(festival))
		{
			return false;
		}
		if (Event.FestivalReadContentLoader == null)
		{
			Event.FestivalReadContentLoader = Game1.content.CreateTemporary();
		}
		try
		{
			if (!Event.FestivalReadContentLoader.DoesAssetExist<Dictionary<string, string>>(assetName))
			{
				Event.invalidFestivals.Add(festival);
				return false;
			}
			data = Event.FestivalReadContentLoader.Load<Dictionary<string, string>>(assetName);
		}
		catch
		{
			Event.invalidFestivals.Add(festival);
			return false;
		}
		if (!data.TryGetValue("conditions", out var rawConditions))
		{
			Game1.log.Error("Festival '" + festival + "' doesn't have the required 'conditions' data field.");
			return false;
		}
		string[] fields = LegacyShims.SplitAndTrim(rawConditions, '/');
		if (!ArgUtility.TryGet(fields, 0, out locationName, out var error, allowBlank: false, "locationName") || !ArgUtility.TryGet(fields, 1, out var rawTimeSpan, out error, allowBlank: false, "string rawTimeSpan"))
		{
			Game1.log.Error($"Festival '{festival}' has preconditions '{rawConditions}' which couldn't be parsed: {error}.");
			return false;
		}
		string[] timeParts = ArgUtility.SplitBySpace(rawTimeSpan);
		if (!ArgUtility.TryGetInt(timeParts, 0, out startTime, out error, "startTime") || !ArgUtility.TryGetInt(timeParts, 1, out endTime, out error, "endTime"))
		{
			Game1.log.Error($"Festival '{festival}' has preconditions '{rawConditions}' with time range '{string.Join(" ", timeParts)}' which couldn't be parsed: {error}.");
			return false;
		}
		return true;
	}

	/// <summary>Load a festival if it exists and its preconditions match the current time and the local player's current location.</summary>
	/// <param name="festival">The festival ID to load, matching the asset name under <c>Data/Festivals</c> (like <samp>spring13</samp>).</param>
	/// <param name="ev">The loaded festival event, if it was loaded successfully.</param>
	/// <returns>Returns whether the festival was loaded successfully.</returns>
	public static bool tryToLoadFestival(string festival, out Event ev)
	{
		ev = null;
		if (!Event.tryToLoadFestivalData(festival, out var dataAssetName, out var data, out var locationName, out var startTime, out var endTime))
		{
			return false;
		}
		if (locationName != Game1.currentLocation.Name || Game1.timeOfDay < startTime || Game1.timeOfDay >= endTime)
		{
			return false;
		}
		ev = new Event
		{
			id = "festival_" + festival,
			isFestival = true,
			festivalDataAssetName = dataAssetName,
			festivalData = data,
			actorPositionsAfterMove = new Dictionary<string, Vector3>(),
			previousAmbientLight = Game1.ambientLight
		};
		ev.festivalData["file"] = festival;
		if (!ev.TryGetFestivalDataForYear("set-up", out var rawSetUp))
		{
			Game1.log.Error("Festival " + ev.id + " doesn't have the required 'set-up' data field.");
		}
		ev.eventCommands = Event.ParseCommands(rawSetUp, ev.farmer);
		Game1.player.festivalScore = 0;
		Game1.setRichPresence("festival", festival);
		return true;
	}

	/// <summary>Try to get an NPC dialogue from the festival data, automatically adjusted to use the closest <c>{key}_y{year}</c> variant if any.</summary>
	/// <param name="npc">The NPC for which to get a dialogue.</param>
	/// <param name="key">The base field key for the dialogue text.</param>
	/// <param name="data">The resulting dialogue instance, or <c>null</c> if the key wasn't found.</param>
	/// <returns>Returns whether a matching dialogue was found.</returns>
	public bool TryGetFestivalDialogueForYear(NPC npc, string key, out Dialogue dialogue)
	{
		if (this.TryGetFestivalDataForYear(key, out var text, out var actualKey))
		{
			dialogue = new Dialogue(npc, this.festivalDataAssetName + ":" + actualKey, text);
			return true;
		}
		dialogue = null;
		return false;
	}

	/// <summary>Try to get a value from the festival data, automatically adjusted to use the closest <c>{key}_y{year}</c> variant if any.</summary>
	/// <param name="key">The base field key.</param>
	/// <param name="data">The resolved data, or <c>null</c> if the key wasn't found.</param>
	/// <param name="actualKey">The resolved field key, including the variant suffix if applicable, or <c>null</c> if the key wasn't found.</param>
	/// <returns>Returns whether a matching field was found.</returns>
	public bool TryGetFestivalDataForYear(string key, out string data, out string actualKey)
	{
		if (this.festivalData == null)
		{
			data = null;
			actualKey = null;
			return false;
		}
		int years = 1;
		while (this.festivalData.ContainsKey($"{key}_y{years + 1}"))
		{
			years++;
		}
		int selected_year = Game1.year % years;
		if (selected_year == 0)
		{
			selected_year = years;
		}
		actualKey = ((selected_year > 1) ? $"{key}_y{selected_year}" : key);
		if (this.festivalData.TryGetValue(actualKey, out data))
		{
			return true;
		}
		actualKey = null;
		data = null;
		return false;
	}

	/// <summary>Get a value from the festival data, automatically adjusted to use the closest <c>{key}_y{year}</c> variant if any.</summary>
	/// <param name="key">The base field key.</param>
	/// <param name="data">The resolved data, or <c>null</c> if the key wasn't found.</param>
	/// <returns>Returns whether a matching field was found.</returns>
	public bool TryGetFestivalDataForYear(string key, out string data)
	{
		string actualKey;
		return this.TryGetFestivalDataForYear(key, out data, out actualKey);
	}

	/// <summary>Set the location and tile position at which to warp the player once the event ends.</summary>
	/// <param name="warp">The warp whose endpoint to use as the exit location.</param>
	public void setExitLocation(Warp warp)
	{
		this.setExitLocation(warp.TargetName, warp.TargetX, warp.TargetY);
	}

	/// <summary>Set the location and tile position at which to warp the player once the event ends.</summary>
	/// <param name="location">The location name.</param>
	/// <param name="x">The X tile position.</param>
	/// <param name="y">The Y tile position.</param>
	public void setExitLocation(string location, int x, int y)
	{
		if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value))
		{
			this.exitLocation = Game1.getLocationRequest(location);
			Game1.player.positionBeforeEvent = new Vector2(x, y);
		}
	}

	public void endBehaviors(GameLocation location = null)
	{
		this.endBehaviors(LegacyShims.EmptyArray<string>(), location ?? Game1.currentLocation);
	}

	public void endBehaviors(string[] args, GameLocation location)
	{
		if (Game1.getMusicTrackName().Contains(Game1.currentSeason) && ArgUtility.Get(this.eventCommands, 0) != "continue")
		{
			Game1.stopMusicTrack(MusicContext.Default);
		}
		switch (ArgUtility.Get(args, 1))
		{
		case "qiSummitCheat":
			Game1.playSound("death");
			Game1.player.health = -1;
			Game1.player.position.X = -99999f;
			Game1.background = null;
			Game1.viewport.X = -999999;
			Game1.viewport.Y = -999999;
			Game1.viewportHold = 6000;
			Game1.eventOver = true;
			this.CurrentCommand += 2;
			Game1.screenGlowHold = false;
			Game1.screenGlowOnce(Color.Black, hold: true, 1f, 1f);
			break;
		case "Leo":
			if (!this.isMemory)
			{
				Game1.addMailForTomorrow("leoMoved", noLetter: true, sendToEveryone: true);
				Game1.player.team.requestLeoMove.Fire();
			}
			break;
		case "bed":
			Game1.player.Position = Game1.player.mostRecentBed + new Vector2(0f, 64f);
			break;
		case "newDay":
			Game1.player.faceDirection(2);
			this.setExitLocation(Game1.player.homeLocation.Value, (int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
			if (!Game1.IsMultiplayer)
			{
				this.exitLocation.OnWarp += delegate
				{
					Game1.NewDay(0f);
					Game1.player.currentLocation.lastTouchActionLocation = new Vector2((int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
				};
			}
			Game1.player.completelyStopAnimatingOrDoingAction();
			if (Game1.player.bathingClothes.Value)
			{
				Game1.player.changeOutOfSwimSuit();
			}
			Game1.player.swimming.Value = false;
			Game1.player.CanMove = false;
			Game1.changeMusicTrack("none");
			break;
		case "invisibleWarpOut":
		{
			if (!ArgUtility.TryGet(args, 2, out var npcName2, out var error3, allowBlank: false, "string npcName"))
			{
				this.LogCommandError(args, error3);
				break;
			}
			NPC npc = Game1.getCharacterFromName(npcName2);
			if (npc == null)
			{
				this.LogCommandError(args, "NPC '" + npcName2 + "' not found");
				break;
			}
			npc.IsInvisible = true;
			npc.daysUntilNotInvisible = 1;
			this.setExitLocation(location.GetFirstPlayerWarp());
			Game1.fadeScreenToBlack();
			Game1.eventOver = true;
			this.CurrentCommand += 2;
			Game1.screenGlowHold = false;
			break;
		}
		case "invisible":
		{
			if (!ArgUtility.TryGet(args, 2, out var npcName4, out var error5, allowBlank: false, "string npcName"))
			{
				this.LogCommandError(args, error5);
			}
			else if (!this.isMemory)
			{
				NPC npc2 = Game1.getCharacterFromName(npcName4);
				if (npc2 == null)
				{
					this.LogCommandError(args, "NPC '" + npcName4 + "' not found");
					break;
				}
				npc2.IsInvisible = true;
				npc2.daysUntilNotInvisible = 1;
			}
			break;
		}
		case "warpOut":
			this.setExitLocation(location.GetFirstPlayerWarp());
			Game1.eventOver = true;
			this.CurrentCommand += 2;
			Game1.screenGlowHold = false;
			break;
		case "dialogueWarpOut":
		{
			if (!ArgUtility.TryGet(args, 2, out var npcName, out var error2, allowBlank: false, "string npcName") || !ArgUtility.TryGet(args, 3, out var dialogue, out error2, allowBlank: true, "string dialogue"))
			{
				this.LogCommandError(args, error2);
				break;
			}
			this.setExitLocation(location.GetFirstPlayerWarp());
			NPC n = Game1.getCharacterFromName(npcName);
			if (n == null)
			{
				this.LogCommandError(args, "NPC '" + npcName + "' not found");
				break;
			}
			n.CurrentDialogue.Clear();
			n.CurrentDialogue.Push(new Dialogue(n, null, dialogue));
			Game1.eventOver = true;
			this.CurrentCommand += 2;
			Game1.screenGlowHold = false;
			break;
		}
		case "Maru1":
			(Game1.getCharacterFromName("Demetrius") ?? this.getActorByName("Demetrius"))?.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1018");
			(Game1.getCharacterFromName("Maru") ?? this.getActorByName("Maru"))?.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1020");
			this.setExitLocation(location.GetFirstPlayerWarp());
			Game1.fadeScreenToBlack();
			Game1.eventOver = true;
			this.CurrentCommand += 2;
			break;
		case "wedding":
		{
			Game1.RequireCharacter("Lewis").CurrentDialogue.Push(new Dialogue(Game1.getCharacterFromName("Lewis"), "Strings\\StringsFromCSFiles:Event.cs.1025"));
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
			Point porch = homeOfFarmer.getPorchStandingSpot();
			if (homeOfFarmer is Cabin)
			{
				this.setExitLocation("Farm", porch.X + 1, porch.Y);
			}
			else
			{
				this.setExitLocation("Farm", porch.X - 1, porch.Y);
			}
			if (!Game1.IsMasterGame)
			{
				break;
			}
			NPC spouse = Game1.getCharacterFromName(this.farmer.spouse);
			if (spouse != null)
			{
				spouse.ClearSchedule();
				spouse.ignoreScheduleToday = true;
				spouse.shouldPlaySpousePatioAnimation.Value = false;
				spouse.controller = null;
				spouse.temporaryController = null;
				spouse.currentMarriageDialogue.Clear();
				Game1.warpCharacter(spouse, "Farm", Utility.getHomeOfFarmer(this.farmer).getPorchStandingSpot());
				spouse.faceDirection(2);
				if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + spouse.Name + "_AfterWedding") != null)
				{
					spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", spouse.Name + "_AfterWedding", false);
				}
				else
				{
					spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "Game1.cs.2782", false);
				}
			}
			break;
		}
		case "dialogue":
		{
			if (!ArgUtility.TryGet(args, 2, out var npcName3, out var error4, allowBlank: false, "string npcName") || !ArgUtility.TryGet(args, 3, out var dialogue2, out error4, allowBlank: true, "string dialogue"))
			{
				this.LogCommandError(args, error4);
				break;
			}
			NPC n2 = Game1.getCharacterFromName(npcName3);
			if (n2 == null)
			{
				this.LogCommandError(args, "NPC '" + npcName3 + "' not found");
				break;
			}
			n2.shouldSayMarriageDialogue.Value = false;
			n2.currentMarriageDialogue.Clear();
			n2.CurrentDialogue.Clear();
			n2.CurrentDialogue.Push(new Dialogue(n2, null, dialogue2));
			break;
		}
		case "beginGame":
			Game1.gameMode = 3;
			this.setExitLocation("FarmHouse", 9, 9);
			Game1.NewDay(1000f);
			this.exitEvent();
			Game1.eventFinished();
			return;
		case "credits":
			Game1.debrisWeather.Clear();
			Game1.isDebrisWeather = false;
			Game1.changeMusicTrack("wedding", track_interruptable: false, MusicContext.Event);
			Game1.gameMode = 10;
			this.CurrentCommand += 2;
			break;
		case "position":
		{
			if (!ArgUtility.TryGetVector2(args, 2, out var position, out var error, integerOnly: true, "Vector2 position"))
			{
				this.LogCommandError(args, error);
			}
			else if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value))
			{
				Game1.player.positionBeforeEvent = position;
			}
			break;
		}
		case "islandDepart":
		{
			Game1.player.orientationBeforeEvent = 2;
			string whereIsTodaysFest = Game1.whereIsTodaysFest;
			if (!(whereIsTodaysFest == "Beach"))
			{
				if (whereIsTodaysFest == "Town")
				{
					Game1.player.orientationBeforeEvent = 3;
					this.setExitLocation("BusStop", 43, 23);
				}
				else
				{
					this.setExitLocation("BoatTunnel", 6, 9);
				}
			}
			else
			{
				Game1.player.orientationBeforeEvent = 0;
				this.setExitLocation("Town", 54, 109);
			}
			GameLocation left_location = Game1.currentLocation;
			this.exitLocation.OnLoad += delegate
			{
				foreach (NPC actor in this.actors)
				{
					actor.shouldShadowBeOffset = true;
					actor.drawOffset.Y = 0f;
				}
				foreach (Farmer farmerActor in this.farmerActors)
				{
					farmerActor.shouldShadowBeOffset = true;
					farmerActor.drawOffset.Y = 0f;
				}
				Game1.player.drawOffset = Vector2.Zero;
				Game1.player.shouldShadowBeOffset = false;
				if (left_location is IslandSouth islandSouth)
				{
					islandSouth.ResetBoat();
				}
			};
			break;
		}
		case "tunnelDepart":
			if (Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
			{
				Game1.warpFarmer("IslandSouth", 21, 43, 0);
			}
			break;
		}
		this.exitEvent();
	}

	public void exitEvent()
	{
		this.eventFinished = true;
		if (!string.IsNullOrEmpty(this.id) && this.id != "-1")
		{
			if (this.markEventSeen)
			{
				Game1.player.eventsSeen.Add(this.id);
			}
			Game1.eventsSeenSinceLastLocationChange.Add(this.id);
		}
		this.notifyDone();
		Game1.stopMusicTrack(MusicContext.Event);
		this.StopTrackedSounds();
		if (this.id == "1039573")
		{
			Game1.addMail("addedParrotBoy", noLetter: true, sendToEveryone: true);
			Game1.player.team.requestAddCharacterEvent.Fire("Leo");
		}
		Game1.player.ignoreCollisions = false;
		Game1.player.canOnlyWalk = false;
		Game1.nonWarpFade = true;
		if (!Game1.fadeIn || Game1.fadeToBlackAlpha >= 1f)
		{
			Game1.fadeScreenToBlack();
		}
		Game1.eventOver = true;
		Game1.fadeToBlack = true;
		Game1.setBGColor(5, 3, 4);
		this.CurrentCommand += 2;
		Game1.screenGlowHold = false;
		if (this.isFestival)
		{
			Game1.timeOfDayAfterFade = 2200;
			if (this.festivalData != null && (this.isSpecificFestival("summer28") || this.isSpecificFestival("fall27")))
			{
				Game1.timeOfDayAfterFade = 2400;
			}
			int timePass = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, Game1.timeOfDayAfterFade);
			if (Game1.IsMasterGame)
			{
				Point house_entry = Game1.getFarm().GetMainFarmHouseEntry();
				this.setExitLocation("Farm", house_entry.X, house_entry.Y);
			}
			else
			{
				Point porchSpot = Utility.getHomeOfFarmer(Game1.player).getPorchStandingSpot();
				this.setExitLocation("Farm", porchSpot.X, porchSpot.Y);
			}
			Game1.player.toolOverrideFunction = null;
			this.isFestival = false;
			foreach (NPC n in this.actors)
			{
				if (n != null)
				{
					this.resetDialogueIfNecessary(n);
				}
			}
			if (Game1.IsMasterGame)
			{
				foreach (NPC n2 in Utility.getAllVillagers())
				{
					if (n2.getSpouse() != null)
					{
						Farmer spouse_farmer = n2.getSpouse();
						if (spouse_farmer.isMarriedOrRoommates())
						{
							n2.controller = null;
							n2.temporaryController = null;
							FarmHouse home_location = Utility.getHomeOfFarmer(spouse_farmer);
							n2.Halt();
							Game1.warpCharacter(n2, home_location, Utility.PointToVector2(home_location.getSpouseBedSpot(spouse_farmer.spouse)));
							if (home_location.GetSpouseBed() != null)
							{
								FarmHouse.spouseSleepEndFunction(n2, Utility.getHomeOfFarmer(spouse_farmer));
							}
							n2.ignoreScheduleToday = true;
							if (Game1.timeOfDayAfterFade >= 1800)
							{
								n2.currentMarriageDialogue.Clear();
								n2.checkForMarriageDialogue(1800, Utility.getHomeOfFarmer(spouse_farmer));
							}
							else if (Game1.timeOfDayAfterFade >= 1100)
							{
								n2.currentMarriageDialogue.Clear();
								n2.checkForMarriageDialogue(1100, Utility.getHomeOfFarmer(spouse_farmer));
							}
							continue;
						}
					}
					if (n2.currentLocation != null && n2.defaultMap.Value != null)
					{
						n2.doingEndOfRouteAnimation.Value = false;
						n2.nextEndOfRouteMessage = null;
						n2.endOfRouteMessage.Value = null;
						n2.controller = null;
						n2.temporaryController = null;
						n2.Halt();
						Game1.warpCharacter(n2, n2.defaultMap.Value, n2.DefaultPosition / 64f);
						n2.ignoreScheduleToday = true;
					}
				}
			}
			foreach (GameLocation l in Game1.locations)
			{
				foreach (Vector2 position in new List<Vector2>(l.objects.Keys))
				{
					if (l.objects[position].minutesElapsed(timePass))
					{
						l.objects.Remove(position);
					}
				}
				if (l is Farm farm)
				{
					farm.timeUpdate(timePass);
				}
			}
			Game1.player.freezePause = 1500;
		}
		else
		{
			Game1.player.forceCanMove();
		}
	}

	public void notifyDone()
	{
		if (!(this.id == "-1") && !string.IsNullOrEmpty(this.id) && this.notifyWhenDone && this.notifyLocationName != null && Game1.HasDedicatedHost && Game1.client != null)
		{
			Game1.client.sendMessage(33, (byte)0, this.notifyLocationName, this.notifyLocationIsStructure, this.id);
			this.notifyWhenDone = false;
		}
	}

	public void resetDialogueIfNecessary(NPC n)
	{
		if (!Game1.player.hasTalkedToFriendToday(n.Name))
		{
			n.resetCurrentDialogue();
		}
		else
		{
			n.CurrentDialogue?.Clear();
		}
	}

	public void incrementCommandAfterFade()
	{
		this.CurrentCommand++;
		Game1.globalFade = false;
	}

	public void cleanup()
	{
		Game1.ambientLight = this.previousAmbientLight;
		this._festivalTexture = null;
		this.festivalContent.Unload();
	}

	private void changeLocation(string locationName, int x, int y, Action onComplete = null)
	{
		Event e = Game1.currentLocation.currentEvent;
		Game1.currentLocation.currentEvent = null;
		LocationRequest locationRequest = Game1.getLocationRequest(locationName);
		locationRequest.OnLoad += delegate
		{
			if (!e.isFestival)
			{
				Game1.currentLocation.currentEvent = e;
			}
			this.temporaryLocation = null;
			onComplete?.Invoke();
			locationRequest.Location.ResetForEvent(this);
		};
		locationRequest.OnWarp += delegate
		{
			this.farmer.currentLocation = Game1.currentLocation;
			if (e.isFestival)
			{
				Game1.currentLocation.currentEvent = e;
			}
		};
		Game1.warpFarmer(locationRequest, x, y, this.farmer.FacingDirection);
	}

	/// <summary>Log an error indicating that an event command format is invalid.</summary>
	/// <param name="args">The space-delimited event command string, including the command name.</param>
	/// <param name="error">The error to log.</param>
	/// <param name="willSkip">Whether the event command will be skipped entirely. If false, the event command will be applied without the argument(s) that failed. This only affects the wording of the message logged.</param>
	public void LogCommandError(string[] args, string error, bool willSkip = false)
	{
		Game1.log.Error(willSkip ? $"Event '{this.id}' has command '{string.Join(" ", args)}' which couldn't be parsed: {error}." : $"Event '{this.id}' has command '{string.Join(" ", args)}' which reported errors: {error}.");
	}

	/// <summary>Log an error indicating that a command format is invalid and skip the current command.</summary>
	/// <param name="args">The space-delimited event command string, including the command name.</param>
	/// <param name="error">The error to log.</param>
	/// <param name="hideError">Whether to skip without logging an error message.</param>
	public void LogCommandErrorAndSkip(string[] args, string error, bool hideError = false)
	{
		if (!hideError)
		{
			this.LogCommandError(args, error, willSkip: true);
		}
		this.CurrentCommand++;
	}

	/// <summary>Log an error indicating that the entire event has failed, and immediately stop the event.</summary>
	/// <param name="error">An error message indicating why the event failed.</param>
	/// <param name="e">The exception which caused the error, if applicable.</param>
	public void LogErrorAndHalt(string error, Exception e = null)
	{
		string technicalError = "Error running event script " + this.fromAssetName + "#" + this.id;
		Game1.chatBox.addErrorMessage("Event script error: " + error);
		string commandText = this.GetCurrentCommand();
		if (commandText != null)
		{
			technicalError += $" on line #{this.CurrentCommand} ({commandText})";
			Game1.chatBox.addErrorMessage($"On line #{this.CurrentCommand}: {commandText}");
		}
		Game1.log.Error(technicalError + ".", e);
		this.skipEvent();
	}

	/// <summary>Log an error indicating that the entire event has failed, and immediately stop the event.</summary>
	/// <param name="e">The exception which caused the error.</param>
	public void LogErrorAndHalt(Exception e)
	{
		this.LogErrorAndHalt(e?.Message ?? "An unknown error occurred.", e);
	}

	/// <summary>Log an error indicating that an event precondition is invalid.</summary>
	/// <param name="location">The location containing the event.</param>
	/// <param name="eventId">The unique event ID whose preconditions are being checked.</param>
	/// <param name="args">The precondition arguments, including the precondition key at the zeroth index.</param>
	/// <param name="error">The error phrase indicating why the precondition is invalid.</param>
	/// <returns>Returns false to simplify failing the precondition.</returns>
	public static bool LogPreconditionError(GameLocation location, string eventId, string[] args, string error)
	{
		Game1.log.Error($"Event '{eventId}' in location '{location.NameOrUniqueName}' has invalid event precondition '{string.Join(" ", args)}': {error}.");
		return false;
	}

	/// <summary>Update the event state.</summary>
	/// <param name="location">The location in which the event is running.</param>
	/// <param name="time">The current game execution time.</param>
	public void Update(GameLocation location, GameTime time)
	{
		try
		{
			if (this.eventFinished)
			{
				return;
			}
			int num;
			if (this.CurrentCommand == 0 && !this.forked)
			{
				num = ((!this.eventSwitched) ? 1 : 0);
				if (num != 0)
				{
					this.InitializeEvent(location, time);
				}
			}
			else
			{
				num = 0;
			}
			bool runNextCommand = this.UpdateBeforeNextCommand(location, time);
			if (num == 0 && runNextCommand)
			{
				this.CheckForNextCommand(location, time);
			}
		}
		catch (Exception e)
		{
			this.LogErrorAndHalt(e);
		}
	}

	/// <summary>Initialize the event when it first starts.</summary>
	/// <param name="location">The location in which the event is running.</param>
	/// <param name="time">The current game execution time.</param>
	protected void InitializeEvent(GameLocation location, GameTime time)
	{
		this.farmer.speed = 2;
		this.farmer.running = false;
		Game1.eventOver = false;
		if (!ArgUtility.TryGet(this.eventCommands, 0, out var musicId, out var error, allowBlank: true, "string musicId") || !ArgUtility.TryGet(this.eventCommands, 1, out var rawCameraPosition, out error, allowBlank: false, "string rawCameraPosition") || !ArgUtility.TryGet(this.eventCommands, 2, out var rawCharacterPositions, out error, allowBlank: false, "string rawCharacterPositions") || !ArgUtility.TryGetOptional(this.eventCommands, 3, out var rawOption, out error, null, allowBlank: true, "string rawOption"))
		{
			Game1.log.Error($"Event '{this.id}' has initial fields '{string.Join("/", this.eventCommands.Take(3))}' which couldn't be parsed: {error}.");
			this.LogErrorAndHalt("event script is invalid");
			return;
		}
		if (string.IsNullOrWhiteSpace(musicId))
		{
			musicId = "none";
		}
		Point cameraPosition;
		if (rawCameraPosition != "follow")
		{
			string[] cameraParts = ArgUtility.SplitBySpace(rawCameraPosition);
			if (!ArgUtility.TryGetPoint(cameraParts, 0, out cameraPosition, out error, "cameraPosition"))
			{
				Game1.log.Error($"Event '{this.id}' has initial fields '{string.Join("/", this.eventCommands.Take(3))}' with camera value '{string.Join(" ", cameraParts)}' which couldn't be parsed (must be 'follow' or tile coordinates): {error}.");
				this.LogErrorAndHalt("event script is invalid");
				return;
			}
		}
		else
		{
			cameraPosition = new Point(-1000, -1000);
		}
		if (rawOption == "ignoreEventTileOffset")
		{
			this.ignoreTileOffsets = true;
		}
		if ((musicId != "none" || !Game1.isRaining) && musicId != "continue" && !musicId.Contains("pause"))
		{
			Game1.changeMusicTrack(musicId, track_interruptable: false, MusicContext.Event);
		}
		if (location is Farm && cameraPosition.X >= -1000 && this.id != "-2" && !this.ignoreTileOffsets)
		{
			Point p = Farm.getFrontDoorPositionForFarmer(this.farmer);
			p.X *= 64;
			p.Y *= 64;
			Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.graphics.GraphicsDevice.Viewport.Width)) : (p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2));
			Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.graphics.GraphicsDevice.Viewport.Height)) : (p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2));
		}
		else if (rawCameraPosition != "follow")
		{
			try
			{
				Game1.viewportFreeze = true;
				int centerX = this.OffsetTileX(cameraPosition.X) * 64 + 32;
				int centerY = this.OffsetTileY(cameraPosition.Y) * 64 + 32;
				if (centerX < 0)
				{
					Game1.viewport.X = centerX;
					Game1.viewport.Y = centerY;
				}
				else
				{
					Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerX - Game1.viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width)) : (centerX - Game1.viewport.Width / 2));
					Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerY - Game1.viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) : (centerY - Game1.viewport.Height / 2));
				}
				if (centerX > 0 && Game1.graphics.GraphicsDevice.Viewport.Width > Game1.currentLocation.Map.DisplayWidth)
				{
					Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
				}
				if (centerY > 0 && Game1.graphics.GraphicsDevice.Viewport.Height > Game1.currentLocation.Map.DisplayHeight)
				{
					Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
				}
			}
			catch (Exception)
			{
				this.forked = true;
				return;
			}
		}
		this.setUpCharacters(rawCharacterPositions, location);
		this.trySpecialSetUp(location);
		this.populateWalkLocationsList();
		this.CurrentCommand = 3;
	}

	/// <summary>Run any updates needed before checking for the next script command.</summary>
	/// <param name="location">The location in which the event is running.</param>
	/// <param name="time">The current game execution time.</param>
	/// <returns>Returns whether to run the next command.</returns>
	protected bool UpdateBeforeNextCommand(GameLocation location, GameTime time)
	{
		if (this.skipped || Game1.farmEvent != null)
		{
			return false;
		}
		foreach (NPC n in this.actors)
		{
			n.update(time, Game1.currentLocation);
			if (n.Sprite.CurrentAnimation != null)
			{
				n.Sprite.animateOnce(time);
			}
		}
		this.aboveMapSprites?.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		if (this.underwaterSprites != null)
		{
			foreach (TemporaryAnimatedSprite underwaterSprite in this.underwaterSprites)
			{
				underwaterSprite.update(time);
			}
		}
		if (!this.playerControlSequence)
		{
			this.farmer.setRunning(isRunning: false);
		}
		if (this.npcControllers != null)
		{
			for (int i = this.npcControllers.Count - 1; i >= 0; i--)
			{
				this.npcControllers[i].puppet.isCharging = !this.isFestival;
				if (this.npcControllers[i].update(time, location, this.npcControllers))
				{
					this.npcControllers.RemoveAt(i);
				}
			}
		}
		if (this.isFestival)
		{
			this.festivalUpdate(time);
		}
		if (this.temporaryLocation != null && !Game1.currentLocation.Equals(this.temporaryLocation))
		{
			this.temporaryLocation.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush: true);
		}
		if (!Game1.fadeToBlack || this.actorPositionsAfterMove.Count > 0 || this.CurrentCommand > 3 || this.forked)
		{
			if (this.eventCommands.Length <= this.CurrentCommand)
			{
				return false;
			}
			if (this.viewportTarget != Vector3.Zero)
			{
				int playerSpeed = this.farmer.speed;
				this.farmer.speed = (int)this.viewportTarget.X;
				int oldX = Game1.viewport.X;
				Game1.viewport.X += (int)this.viewportTarget.X;
				if (oldX > 0 && Game1.viewport.X <= 0 && location.IsOutdoors)
				{
					Game1.viewport.X = 0;
					this.viewportTarget.X = 0f;
				}
				else if (oldX < location.map.DisplayWidth - Game1.viewport.Width && Game1.viewport.X >= location.Map.DisplayWidth - Game1.viewport.Width)
				{
					Game1.viewport.X = location.Map.DisplayWidth - Game1.viewport.Width;
					this.viewportTarget.X = 0f;
				}
				if (this.viewportTarget.X != 0f)
				{
					Game1.updateRainDropPositionForPlayerMovement((!(this.viewportTarget.X < 0f)) ? 1 : 3, Math.Abs(this.viewportTarget.X + (float)((this.farmer.isMoving() && this.farmer.FacingDirection == 3) ? (-this.farmer.speed) : ((this.farmer.isMoving() && this.farmer.FacingDirection == 1) ? this.farmer.speed : 0))));
				}
				int oldY = Game1.viewport.Y;
				Game1.viewport.Y += (int)this.viewportTarget.Y;
				if (oldY > 0 && Game1.viewport.Y <= 0 && location.IsOutdoors)
				{
					Game1.viewport.Y = 0;
					this.viewportTarget.Y = 0f;
				}
				else if (oldY < location.map.DisplayHeight - Game1.viewport.Height && Game1.viewport.Y >= location.Map.DisplayHeight - Game1.viewport.Height)
				{
					Game1.viewport.Y = location.Map.DisplayHeight - Game1.viewport.Height;
					this.viewportTarget.Y = 0f;
				}
				this.farmer.speed = (int)this.viewportTarget.Y;
				if (this.viewportTarget.Y != 0f)
				{
					Game1.updateRainDropPositionForPlayerMovement((!(this.viewportTarget.Y < 0f)) ? 2 : 0, Math.Abs(this.viewportTarget.Y - (float)((this.farmer.isMoving() && this.farmer.FacingDirection == 0) ? (-this.farmer.speed) : ((this.farmer.isMoving() && this.farmer.FacingDirection == 2) ? this.farmer.speed : 0))));
				}
				this.farmer.speed = playerSpeed;
				this.viewportTarget.Z -= time.ElapsedGameTime.Milliseconds;
				if (this.viewportTarget.Z <= 0f)
				{
					this.viewportTarget = Vector3.Zero;
				}
			}
			if (this.actorPositionsAfterMove.Count > 0)
			{
				string[] array = this.actorPositionsAfterMove.Keys.ToArray();
				foreach (string s in array)
				{
					Microsoft.Xna.Framework.Rectangle targetTile = new Microsoft.Xna.Framework.Rectangle((int)this.actorPositionsAfterMove[s].X * 64, (int)this.actorPositionsAfterMove[s].Y * 64, 64, 64);
					targetTile.Inflate(-4, 0);
					NPC actor = this.getActorByName(s);
					if (actor != null)
					{
						Microsoft.Xna.Framework.Rectangle bounds = actor.GetBoundingBox();
						if (bounds.Width > 64)
						{
							targetTile.Inflate(4, 0);
							targetTile.Width = bounds.Width + 4;
							targetTile.Height = bounds.Height + 4;
							targetTile.X += 8;
							targetTile.Y += 16;
						}
					}
					if (this.IsFarmerActorId(s, out var farmerNumber))
					{
						Farmer f = this.GetFarmerActor(farmerNumber);
						if (f != null)
						{
							Microsoft.Xna.Framework.Rectangle bounds2 = f.GetBoundingBox();
							float moveSpeed = f.getMovementSpeed();
							if (targetTile.Contains(bounds2) && (((float)(bounds2.Y - targetTile.Top) <= 16f + moveSpeed && f.FacingDirection != 2) || ((float)(targetTile.Bottom - bounds2.Bottom) <= 16f + moveSpeed && f.FacingDirection == 2)))
							{
								f.showNotCarrying();
								f.Halt();
								f.faceDirection((int)this.actorPositionsAfterMove[s].Z);
								f.FarmerSprite.StopAnimation();
								f.Halt();
								this.actorPositionsAfterMove.Remove(s);
							}
							else if (f != null)
							{
								f.canOnlyWalk = false;
								f.setRunning(isRunning: false, force: true);
								f.canOnlyWalk = true;
								f.lastPosition = this.farmer.Position;
								f.MovePosition(time, Game1.viewport, location);
							}
						}
						continue;
					}
					foreach (NPC n2 in this.actors)
					{
						Microsoft.Xna.Framework.Rectangle bounds3 = n2.GetBoundingBox();
						if (n2.Name.Equals(s) && targetTile.Contains(bounds3) && bounds3.Y - targetTile.Top <= 16)
						{
							n2.Halt();
							n2.faceDirection((int)this.actorPositionsAfterMove[s].Z);
							this.actorPositionsAfterMove.Remove(s);
							break;
						}
						if (n2.Name.Equals(s))
						{
							if (n2 is Monster)
							{
								n2.MovePosition(time, Game1.viewport, location);
							}
							else
							{
								n2.MovePosition(time, Game1.viewport, null);
							}
							break;
						}
					}
				}
				if (this.actorPositionsAfterMove.Count == 0)
				{
					if (this.continueAfterMove)
					{
						this.continueAfterMove = false;
					}
					else
					{
						this.CurrentCommand++;
					}
				}
				if (!this.continueAfterMove)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected void CheckForNextCommand(GameLocation location, GameTime time)
	{
		string[] args = ArgUtility.SplitBySpaceQuoteAware(this.eventCommands[Math.Min(this.eventCommands.Length - 1, this.CurrentCommand)]);
		bool num = ArgUtility.Get(args, 0)?.StartsWith("--") ?? false;
		if (this.temporaryLocation != null && !Game1.currentLocation.Equals(this.temporaryLocation))
		{
			this.temporaryLocation.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush: true);
		}
		if (num)
		{
			this.CurrentCommand++;
		}
		else
		{
			this.tryEventCommand(location, time, args);
		}
	}

	/// <summary>Get the text of the current event command being executed.</summary>
	public string GetCurrentCommand()
	{
		return ArgUtility.Get(this.eventCommands, this.currentCommand);
	}

	/// <summary>Replace the command at the current index.</summary>
	/// <param name="command">The new command text to parse.</param>
	public void ReplaceCurrentCommand(string command)
	{
		if (ArgUtility.HasIndex(this.eventCommands, this.currentCommand))
		{
			this.eventCommands[this.currentCommand] = command;
		}
	}

	/// <summary>Replace the entire list of commands with the given values.</summary>
	/// <param name="commands">The new commands to parse.</param>
	public void ReplaceAllCommands(params string[] commands)
	{
		this.eventCommands = commands;
		this.CurrentCommand = 0;
	}

	/// <summary>Add a new event command to run after the current one.</summary>
	/// <param name="command">The new command text to parse.</param>
	public void InsertNextCommand(string command)
	{
		int index = this.currentCommand + 1;
		List<string> commands = this.eventCommands.ToList();
		if (index <= commands.Count)
		{
			commands.Insert(index, command);
		}
		else
		{
			commands.Add(command);
		}
		this.eventCommands = commands.ToArray();
	}

	/// <summary>Register a sound cue to remove when the event ends.</summary>
	/// <param name="cue">The audio cue to register.</param>
	public void TrackSound(ICue cue)
	{
		if (cue != null)
		{
			if (!this.CustomSounds.TryGetValue(cue.Name, out var sounds))
			{
				sounds = (this.CustomSounds[cue.Name] = new List<ICue>());
			}
			sounds.Add(cue);
		}
	}

	/// <summary>Stop a tracked sound registered via <see cref="M:StardewValley.Event.TrackSound(StardewValley.ICue)" />.</summary>
	/// <param name="cueId">The audio cue ID to stop.</param>
	/// <param name="immediate">Whether to stop the sound immediately, instead of letting it finish the current loop.</param>
	public void StopTrackedSound(string cueId, bool immediate)
	{
		if (cueId == null || !this.CustomSounds.TryGetValue(cueId, out var sounds))
		{
			return;
		}
		foreach (ICue item in sounds)
		{
			item.Stop(immediate ? AudioStopOptions.Immediate : AudioStopOptions.AsAuthored);
		}
		if (immediate)
		{
			this.CustomSounds.Remove(cueId);
		}
	}

	/// <summary>Stop all tracked sounds registered via <see cref="M:StardewValley.Event.TrackSound(StardewValley.ICue)" />.</summary>
	public void StopTrackedSounds()
	{
		foreach (List<ICue> value in this.CustomSounds.Values)
		{
			foreach (ICue item in value)
			{
				item.Stop(AudioStopOptions.Immediate);
			}
		}
		this.CustomSounds.Clear();
	}

	public bool isTileWalkedOn(int x, int y)
	{
		return this.characterWalkLocations.Contains(new Vector2(x, y));
	}

	private void populateWalkLocationsList()
	{
		this.characterWalkLocations.Add(this.farmer.Tile);
		foreach (NPC n in this.actors)
		{
			this.characterWalkLocations.Add(n.Tile);
		}
		for (int i = 2; i < this.eventCommands.Length; i++)
		{
			string[] args = ArgUtility.SplitBySpace(this.eventCommands[i]);
			if (ArgUtility.Get(args, 0) != "move" || (ArgUtility.Get(args, 1) == "false" && args.Length == 2))
			{
				continue;
			}
			if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetPoint(args, 2, out var position, out error, "Point position"))
			{
				this.LogCommandError(args, error);
				continue;
			}
			Character character = (this.IsCurrentFarmerActorId(actorName) ? ((Character)this.farmer) : ((Character)this.getActorByName(actorName)));
			if (character != null)
			{
				Vector2 pos = character.Tile;
				for (int x = 0; x < Math.Abs(position.X); x++)
				{
					pos.X += Math.Sign(position.X);
					this.characterWalkLocations.Add(pos);
				}
				for (int y = 0; y < Math.Abs(position.Y); y++)
				{
					pos.Y += Math.Sign(position.Y);
					this.characterWalkLocations.Add(pos);
				}
			}
		}
	}

	/// <summary>Get an NPC actor in the event by its name.</summary>
	/// <param name="name">The actor name.</param>
	/// <param name="legacyReplaceUnderscores">Whether to try replacing underscores with spaces in <paramref name="name" /> if an exact match wasn't found. This is only meant for backwards compatibility, for event commands which predate argument quoting.</param>
	/// <returns>Returns the matching actor, else <c>null</c>.</returns>
	public NPC getActorByName(string name, bool legacyReplaceUnderscores = false)
	{
		bool isOptionalNpc;
		return this.getActorByName(name, out isOptionalNpc, legacyReplaceUnderscores);
	}

	/// <summary>Get an NPC actor in the event by its name.</summary>
	/// <param name="name">The actor name.</param>
	/// <param name="isOptionalNpc">Whether the NPC is marked optional, so no error should be shown if they're missing.</param>
	/// <param name="legacyReplaceUnderscores">Whether to try replacing underscores with spaces in <paramref name="name" /> if an exact match wasn't found. This is only meant for backwards compatibility, for event commands which predate argument quoting.</param>
	/// <returns>Returns the matching actor, else <c>null</c>.</returns>
	public NPC getActorByName(string name, out bool isOptionalNpc, bool legacyReplaceUnderscores = false)
	{
		isOptionalNpc = name?.EndsWith('?') ?? false;
		if (isOptionalNpc)
		{
			name = name.Substring(0, name.Length - 1);
		}
		if (name != null)
		{
			if (name == "spouse")
			{
				name = this.farmer.spouse;
			}
			foreach (NPC n in this.actors)
			{
				if (n.Name == name)
				{
					return n;
				}
			}
			if (legacyReplaceUnderscores)
			{
				string newName = name.Replace('_', ' ');
				if (newName != name)
				{
					foreach (NPC n2 in this.actors)
					{
						if (n2.Name == newName)
						{
							return n2;
						}
					}
				}
			}
			return null;
		}
		return null;
	}

	private void addActor(string name, int x, int y, int facingDirection, GameLocation location)
	{
		bool isOptionalNpc;
		NPC duplicate = this.getActorByName(name, out isOptionalNpc);
		if (duplicate != null)
		{
			duplicate.Position = new Vector2(x * 64, y * 64);
			duplicate.FacingDirection = facingDirection;
			return;
		}
		if (isOptionalNpc)
		{
			name = name.Substring(0, name.Length - 1);
			if (!NPC.TryGetData(name, out var data) || !GameStateQuery.CheckConditions(data.UnlockConditions))
			{
				return;
			}
		}
		NPC n;
		try
		{
			string spriteName = NPC.getTextureNameForCharacter(name);
			Texture2D portrait = null;
			try
			{
				portrait = Game1.content.Load<Texture2D>("Portraits\\" + spriteName);
			}
			catch (Exception)
			{
			}
			int height = ((name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128);
			n = new NPC(new AnimatedSprite("Characters\\" + spriteName, 0, 16, height / 4), new Vector2(x * 64, y * 64), location.Name, facingDirection, name, portrait, eventActor: true);
			n.EventActor = true;
			if (this.isFestival)
			{
				try
				{
					if (this.TryGetFestivalDialogueForYear(n, n.Name, out var dialogue))
					{
						n.setNewDialogue(dialogue);
					}
				}
				catch (Exception)
				{
				}
			}
			if (n.name.Equals("MrQi"))
			{
				n.displayName = Game1.content.LoadString("Strings\\NPCNames:MisterQi");
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Event '{this.id}' has character '{name}' which couldn't be added.", exception);
			return;
		}
		n.EventActor = true;
		this.actors.Add(n);
	}

	/// <summary>Get the player in the event matching a farmer number, if found.</summary>
	/// <param name="farmerNumber">The farmer number. This can be -1 (current player), 1 (main player), or higher numbers for farmhands.</param>
	/// <returns>Returns the matching event actor or real farmer, or <c>null</c> if neither was found.</returns>
	public Farmer GetFarmerActor(int farmerNumber)
	{
		Farmer player = ((farmerNumber < 1) ? this.farmer : Utility.getFarmerFromFarmerNumber(farmerNumber));
		if (player == null)
		{
			return null;
		}
		foreach (Farmer actor in this.farmerActors)
		{
			if (actor.UniqueMultiplayerID == player.UniqueMultiplayerID)
			{
				return actor;
			}
		}
		return player;
	}

	/// <summary>Get whether an actor ID is the current player.</summary>
	/// <param name="actor">The actor ID to check.</param>
	public bool IsCurrentFarmerActorId(string actor)
	{
		if (this.IsFarmerActorId(actor, out var farmerNumber))
		{
			return this.IsCurrentFarmerActorId(farmerNumber);
		}
		return false;
	}

	/// <summary>Get whether an actor ID is the current player.</summary>
	/// <param name="farmerNumber">The farmer number to check.</param>
	public bool IsCurrentFarmerActorId(int farmerNumber)
	{
		if (farmerNumber >= 1)
		{
			return farmerNumber == Utility.getFarmerNumberFromFarmer(Game1.player);
		}
		return true;
	}

	/// <summary>Get whether an actor ID is a farmer like <c>farmer</c> (current player) or <samp>farmer3</samp> (player #3), regardless of whether that player is present.</summary>
	/// <param name="actor">The actor ID to check.</param>
	/// <param name="farmerNumber">The parsed farmer number, if applicable. This can be <samp>-1</samp> (current player), 1 (main player), or higher numbers for farmhands.</param>
	public bool IsFarmerActorId(string actor, out int farmerNumber)
	{
		if (actor == null || !actor.StartsWith("farmer"))
		{
			farmerNumber = -1;
			return false;
		}
		if (actor.Length == "farmer".Length)
		{
			farmerNumber = -1;
			return true;
		}
		return int.TryParse(actor.Substring("farmer".Length), out farmerNumber);
	}

	public Character getCharacterByName(string name)
	{
		if (this.IsFarmerActorId(name, out var farmerNumber))
		{
			return this.GetFarmerActor(farmerNumber);
		}
		foreach (NPC n in this.actors)
		{
			if (n.Name.Equals(name))
			{
				return n;
			}
		}
		return null;
	}

	public Vector3 getPositionAfterMove(Character c, int xMove, int yMove, int facingDirection)
	{
		Vector2 tileLocation = c.Tile;
		return new Vector3(tileLocation.X + (float)xMove, tileLocation.Y + (float)yMove, facingDirection);
	}

	private void trySpecialSetUp(GameLocation location)
	{
		string text = this.id;
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 7:
			switch (text[6])
			{
			case '0':
				if (text == "9333220" && location is FarmHouse { upgradeLevel: 1 })
				{
					this.farmer.Position = new Vector2(1920f, 400f);
					this.getActorByName("Sebastian").setTilePosition(31, 6);
				}
				break;
			case '3':
			{
				if (!(text == "4324303") || !(location is FarmHouse house3))
				{
					break;
				}
				Point bed_spot = house3.GetPlayerBedSpot();
				bed_spot.X--;
				this.farmer.Position = new Vector2(bed_spot.X * 64, bed_spot.Y * 64 + 16);
				this.getActorByName("Penny").setTilePosition(bed_spot.X - 1, bed_spot.Y);
				Microsoft.Xna.Framework.Rectangle room = new Microsoft.Xna.Framework.Rectangle(23, 12, 10, 10);
				if (house3.upgradeLevel == 1)
				{
					room = new Microsoft.Xna.Framework.Rectangle(20, 3, 8, 7);
				}
				Point room_center = room.Center;
				if (!room.Contains(Game1.player.TilePoint))
				{
					List<string> commands = new List<string>(this.eventCommands);
					int command_index = 56;
					commands.Insert(command_index, "globalFade 0.03");
					command_index++;
					commands.Insert(command_index, "beginSimultaneousCommand");
					command_index++;
					commands.Insert(command_index, "viewport " + room_center.X + " " + room_center.Y);
					command_index++;
					commands.Insert(command_index, "globalFadeToClear 0.03");
					command_index++;
					commands.Insert(command_index, "endSimultaneousCommand");
					command_index++;
					commands.Insert(command_index, "pause 2000");
					command_index++;
					commands.Insert(command_index, "globalFade 0.03");
					command_index++;
					commands.Insert(command_index, "beginSimultaneousCommand");
					command_index++;
					commands.Insert(command_index, "viewport " + Game1.player.TilePoint.X + " " + Game1.player.TilePoint.Y);
					command_index++;
					commands.Insert(command_index, "globalFadeToClear 0.03");
					command_index++;
					commands.Insert(command_index, "endSimultaneousCommand");
					command_index++;
					this.eventCommands = commands.ToArray();
				}
				for (int i = 0; i < this.eventCommands.Length; i++)
				{
					if (!this.eventCommands[i].StartsWith("makeInvisible"))
					{
						continue;
					}
					string[] args = ArgUtility.SplitBySpace(this.eventCommands[i]);
					if (!ArgUtility.TryGetPoint(args, 1, out var tile, out var error, "Point tile"))
					{
						this.LogCommandError(args, error);
						continue;
					}
					args[1] = (tile.X - 26 + bed_spot.X).ToString() ?? "";
					args[2] = (tile.Y - 13 + bed_spot.Y).ToString() ?? "";
					if (location.getObjectAtTile(tile.X, tile.Y) == house3.GetPlayerBed())
					{
						this.eventCommands[i] = "makeInvisible -1000 -1000";
					}
					else
					{
						this.eventCommands[i] = string.Join(" ", args);
					}
				}
				break;
			}
			case '4':
				if (text == "4325434" && location is FarmHouse { upgradeLevel: 1 })
				{
					this.farmer.Position = new Vector2(512f, 336f);
					this.getActorByName("Penny").setTilePosition(5, 5);
				}
				break;
			case '2':
			{
				if (!(text == "3912132") || !(location is FarmHouse house7))
				{
					break;
				}
				Point bed_spot2 = house7.GetPlayerBedSpot();
				bed_spot2.X--;
				if (!location.CanItemBePlacedHere(Utility.PointToVector2(bed_spot2) + new Vector2(-2f, 0f)))
				{
					bed_spot2.X++;
				}
				this.farmer.setTileLocation(Utility.PointToVector2(bed_spot2));
				this.getActorByName("Elliott").setTileLocation(Utility.PointToVector2(bed_spot2) + new Vector2(-2f, 0f));
				for (int i2 = 0; i2 < this.eventCommands.Length; i2++)
				{
					if (!this.eventCommands[i2].StartsWith("makeInvisible"))
					{
						continue;
					}
					string[] args2 = ArgUtility.SplitBySpace(this.eventCommands[i2]);
					if (!ArgUtility.TryGetPoint(args2, 1, out var tile2, out var error2, "Point tile"))
					{
						this.LogCommandError(args2, error2);
						continue;
					}
					args2[1] = (tile2.X - 26 + bed_spot2.X).ToString() ?? "";
					args2[2] = (tile2.Y - 13 + bed_spot2.Y).ToString() ?? "";
					if (location.getObjectAtTile(tile2.X, tile2.Y) == house7.GetPlayerBed())
					{
						this.eventCommands[i2] = "makeInvisible -1000 -1000";
					}
					else
					{
						this.eventCommands[i2] = string.Join(" ", args2);
					}
				}
				break;
			}
			case '1':
				if (!(text == "8675611"))
				{
					if (!(text == "3917601") || !(location is DecoratableLocation decoratableLocation))
					{
						break;
					}
					foreach (Furniture f in decoratableLocation.furniture)
					{
						if (f.furniture_type.Value == 14 && !location.IsTileBlockedBy(f.TileLocation + new Vector2(0f, 1f), CollisionMask.All, CollisionMask.All) && !location.IsTileBlockedBy(f.TileLocation + new Vector2(1f, 1f), CollisionMask.All, CollisionMask.All))
						{
							this.getActorByName("Emily").setTilePosition((int)f.TileLocation.X, (int)f.TileLocation.Y + 1);
							this.farmer.Position = new Vector2((f.TileLocation.X + 1f) * 64f, (f.tileLocation.Y + 1f) * 64f + 16f);
							f.isOn.Value = true;
							f.setFireplace(playSound: false);
							return;
						}
					}
					if (location is FarmHouse { upgradeLevel: 1 })
					{
						this.getActorByName("Emily").setTilePosition(4, 5);
						this.farmer.Position = new Vector2(320f, 336f);
					}
				}
				else if (location is FarmHouse { upgradeLevel: 1 })
				{
					this.getActorByName("Haley").setTilePosition(4, 5);
					this.farmer.Position = new Vector2(320f, 336f);
				}
				break;
			case '6':
				if (text == "3917666" && location is FarmHouse { upgradeLevel: 1 })
				{
					this.getActorByName("Maru").setTilePosition(4, 5);
					this.farmer.Position = new Vector2(320f, 336f);
				}
				break;
			case '5':
				break;
			}
			break;
		case 6:
			if (text == "739330")
			{
				if (!Game1.player.friendshipData.ContainsKey("Willy"))
				{
					Game1.player.friendshipData.Add("Willy", new Friendship(0));
				}
				NPC willy = Game1.getCharacterFromName("Willy");
				Game1.player.NotifyQuests((Quest quest) => quest.OnNpcSocialized(willy));
			}
			break;
		}
	}

	private void setUpCharacters(string description, GameLocation location)
	{
		this.farmer.Halt();
		if (string.IsNullOrEmpty(Game1.player.locationBeforeForcedEvent.Value) && !this.isMemory)
		{
			Game1.player.positionBeforeEvent = Game1.player.Tile;
			Game1.player.orientationBeforeEvent = Game1.player.FacingDirection;
		}
		string[] args = ArgUtility.SplitBySpace(description);
		for (int i = 0; i < args.Length; i += 4)
		{
			if (!ArgUtility.TryGet(args, i, out var actorName, out var error, allowBlank: true, "string actorName") || !ArgUtility.TryGetPoint(args, i + 1, out var tile, out error, "Point tile") || !ArgUtility.TryGetInt(args, i + 3, out var direction, out error, "int direction"))
			{
				Game1.log.Error($"Event '{this.id}' has character positions '{string.Join(" ", args)}' which couldn't be parsed: {error}.");
				continue;
			}
			int farmerNumber;
			bool isFarmerId = this.IsFarmerActorId(actorName, out farmerNumber);
			bool isCurrentFarmer = isFarmerId && this.IsCurrentFarmerActorId(farmerNumber);
			if (tile.X == -1 && !isCurrentFarmer)
			{
				foreach (NPC n in location.characters)
				{
					if (n.Name == actorName)
					{
						this.actors.Add(n);
					}
				}
			}
			else if (actorName != "farmer")
			{
				if (actorName == "otherFarmers")
				{
					int x = this.OffsetTileX(tile.X);
					int y = this.OffsetTileY(tile.Y);
					foreach (Farmer f in Game1.getOnlineFarmers())
					{
						if (f.UniqueMultiplayerID != this.farmer.UniqueMultiplayerID)
						{
							Farmer fake = f.CreateFakeEventFarmer();
							fake.completelyStopAnimatingOrDoingAction();
							fake.hidden.Value = false;
							fake.faceDirection(direction);
							fake.setTileLocation(new Vector2(x, y));
							fake.currentLocation = Game1.currentLocation;
							x++;
							this.farmerActors.Add(fake);
						}
					}
					continue;
				}
				if (isFarmerId)
				{
					int x2 = this.OffsetTileX(tile.X);
					int y2 = this.OffsetTileY(tile.Y);
					Farmer f2 = this.GetFarmerActor(farmerNumber);
					if (f2 != null)
					{
						Farmer fake2 = f2.CreateFakeEventFarmer();
						fake2.completelyStopAnimatingOrDoingAction();
						fake2.hidden.Value = false;
						fake2.faceDirection(direction);
						fake2.setTileLocation(new Vector2(x2, y2));
						fake2.currentLocation = Game1.currentLocation;
						fake2.isFakeEventActor = true;
						this.farmerActors.Add(fake2);
					}
					continue;
				}
				string name = ((!(actorName == "spouse")) ? actorName : this.farmer.spouse);
				switch (actorName)
				{
				case "cat":
				{
					Pet cat = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, "Cat");
					cat.Name = "Cat";
					cat.position.X -= 32f;
					this.actors.Add(cat);
					continue;
				}
				case "dog":
				{
					Pet dog = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, "Dog");
					dog.Name = "Dog";
					dog.position.X -= 42f;
					this.actors.Add(dog);
					continue;
				}
				case "pet":
				{
					Pet pet = new Pet(this.OffsetTileX(tile.X), this.OffsetTileY(tile.Y), Game1.player.whichPetBreed, Game1.player.whichPetType);
					pet.Name = "PetActor";
					if (Pet.TryGetData(Game1.player.whichPetType, out var data))
					{
						pet.Position = new Vector2(pet.Position.X + (float)data.EventOffset.X, pet.Position.Y + (float)data.EventOffset.Y);
					}
					this.actors.Add(pet);
					continue;
				}
				case "golem":
				{
					NPC golem = new NPC(new AnimatedSprite("Characters\\Monsters\\Wilderness Golem", 0, 16, 24), this.OffsetPosition(new Vector2(tile.X, tile.Y) * 64f), 0, "Golem");
					golem.AllowDynamicAppearance = false;
					this.actors.Add(golem);
					continue;
				}
				case "Junimo":
					this.actors.Add(new Junimo(this.OffsetPosition(new Vector2(tile.X * 64, tile.Y * 64 - 32)), Game1.currentLocation.Name.Equals("AbandonedJojaMart") ? 6 : (-1))
					{
						Name = "Junimo",
						EventActor = true
					});
					continue;
				}
				int xPos = this.OffsetTileX(tile.X);
				int yPos = this.OffsetTileY(tile.Y);
				int facingDir = direction;
				if (location is Farm && this.id != "-2" && !this.ignoreTileOffsets)
				{
					xPos = Farm.getFrontDoorPositionForFarmer(this.farmer).X;
					yPos = Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 2;
					facingDir = 0;
				}
				this.addActor(name, xPos, yPos, facingDir, location);
			}
			else if (tile.X != -1)
			{
				this.farmer.position.X = this.OffsetPositionX(tile.X * 64);
				this.farmer.position.Y = this.OffsetPositionY(tile.Y * 64 + 16);
				this.farmer.faceDirection(direction);
				if (location is Farm && this.id != "-2" && !this.ignoreTileOffsets)
				{
					this.farmer.position.X = Farm.getFrontDoorPositionForFarmer(this.farmer).X * 64;
					this.farmer.position.Y = (Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 1) * 64;
					this.farmer.faceDirection(2);
				}
				this.farmer.FarmerSprite.StopAnimation();
			}
		}
	}

	private void beakerSmashEndFunction(int extraInfo)
	{
		Game1.playSound("breakingGlass");
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.LightBlue, 10));
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(400, 3008, 64, 64), 99999f, 2, 0, new Vector2(9f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.LightBlue, 1f, 0f, 0f, 0f)
		{
			delayBeforeAnimationStart = 700
		});
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(9f, 16f) * 64f, Color.White * 0.75f, 10)
		{
			motion = new Vector2(0f, -1f)
		});
	}

	private void eggSmashEndFunction(int extraInfo)
	{
		Game1.playSound("slimedead");
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.White, 10));
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(177, 99999f, 9999, 0, new Vector2(6f, 5f) * 64f, flicker: false, flipped: false)
		{
			layerDepth = 1E-06f
		});
	}

	private void balloonInSky(int extraInfo)
	{
		TemporaryAnimatedSprite t = Game1.currentLocation.getTemporarySpriteByID(2);
		if (t != null)
		{
			t.motion = Vector2.Zero;
		}
		t = Game1.currentLocation.getTemporarySpriteByID(1);
		if (t != null)
		{
			t.motion = Vector2.Zero;
		}
	}

	private void marcelloBalloonLand(int extraInfo)
	{
		Game1.playSound("thudStep");
		Game1.playSound("dirtyHit");
		TemporaryAnimatedSprite t = Game1.currentLocation.getTemporarySpriteByID(2);
		if (t != null)
		{
			t.motion = Vector2.Zero;
		}
		t = Game1.currentLocation.getTemporarySpriteByID(3);
		if (t != null)
		{
			t.scaleChange = 0f;
		}
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(25f, 39f) + this.eventPositionTileOffset) * 64f + new Vector2(-32f, 32f), flicker: false, flipped: true, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
		Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(27f, 39f) + this.eventPositionTileOffset) * 64f + new Vector2(0f, 48f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
		{
			delayBeforeAnimationStart = 300
		});
		this.CurrentCommand++;
	}

	private void samPreOllie(int extraInfo)
	{
		this.getActorByName("Sam").Sprite.currentFrame = 27;
		this.farmer.faceDirection(0);
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
		temporarySpriteByID.xStopCoordinate = 1408;
		temporarySpriteByID.reachedStopCoordinate = samOllie;
		temporarySpriteByID.motion = new Vector2(2f, 0f);
	}

	private void samOllie(int extraInfo)
	{
		Game1.playSound("crafting");
		this.getActorByName("Sam").Sprite.currentFrame = 26;
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
		temporarySpriteByID.currentNumberOfLoops = 0;
		temporarySpriteByID.totalNumberOfLoops = 1;
		temporarySpriteByID.motion.Y = -9f;
		temporarySpriteByID.motion.X = 2f;
		temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
		temporarySpriteByID.animationLength = 1;
		temporarySpriteByID.interval = 530f;
		temporarySpriteByID.timer = 0f;
		temporarySpriteByID.endFunction = samGrind;
		temporarySpriteByID.destroyable = false;
	}

	private void samGrind(int extraInfo)
	{
		Game1.playSound("hammer");
		this.getActorByName("Sam").Sprite.currentFrame = 28;
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
		temporarySpriteByID.currentNumberOfLoops = 0;
		temporarySpriteByID.totalNumberOfLoops = 9999;
		temporarySpriteByID.motion.Y = 0f;
		temporarySpriteByID.motion.X = 2f;
		temporarySpriteByID.acceleration = new Vector2(0f, 0f);
		temporarySpriteByID.animationLength = 1;
		temporarySpriteByID.interval = 99999f;
		temporarySpriteByID.timer = 0f;
		temporarySpriteByID.xStopCoordinate = 1664;
		temporarySpriteByID.yStopCoordinate = -1;
		temporarySpriteByID.reachedStopCoordinate = samDropOff;
	}

	private void samDropOff(int extraInfo)
	{
		NPC actorByName = this.getActorByName("Sam");
		actorByName.Sprite.currentFrame = 31;
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
		temporarySpriteByID.currentNumberOfLoops = 9999;
		temporarySpriteByID.totalNumberOfLoops = 0;
		temporarySpriteByID.motion.Y = 0f;
		temporarySpriteByID.motion.X = 2f;
		temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
		temporarySpriteByID.animationLength = 1;
		temporarySpriteByID.interval = 99999f;
		temporarySpriteByID.yStopCoordinate = 5760;
		temporarySpriteByID.reachedStopCoordinate = samGround;
		temporarySpriteByID.endFunction = null;
		actorByName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
		{
			new FarmerSprite.AnimationFrame(29, 100),
			new FarmerSprite.AnimationFrame(30, 100),
			new FarmerSprite.AnimationFrame(31, 100),
			new FarmerSprite.AnimationFrame(32, 100)
		});
		actorByName.Sprite.loop = false;
	}

	private void samGround(int extraInfo)
	{
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
		Game1.playSound("thudStep");
		temporarySpriteByID.attachedCharacter = null;
		temporarySpriteByID.reachedStopCoordinate = null;
		temporarySpriteByID.totalNumberOfLoops = -1;
		temporarySpriteByID.interval = 0f;
		temporarySpriteByID.destroyable = true;
		this.CurrentCommand++;
	}

	private void catchFootball(int extraInfo)
	{
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
		Game1.playSound("fishSlap");
		temporarySpriteByID.motion = new Vector2(2f, -8f);
		temporarySpriteByID.rotationChange = (float)Math.PI / 24f;
		temporarySpriteByID.reachedStopCoordinate = footballLand;
		temporarySpriteByID.yStopCoordinate = 1088;
		this.farmer.jump();
	}

	private void footballLand(int extraInfo)
	{
		TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
		Game1.playSound("sandyStep");
		temporarySpriteByID.motion = new Vector2(0f, 0f);
		temporarySpriteByID.rotationChange = 0f;
		temporarySpriteByID.reachedStopCoordinate = null;
		temporarySpriteByID.animationLength = 1;
		temporarySpriteByID.interval = 999999f;
		this.CurrentCommand++;
	}

	private void parrotSplat(int extraInfo)
	{
		Game1.playSound("drumkit0");
		DelayedAction.playSoundAfterDelay("drumkit5", 100);
		Game1.playSound("slimeHit");
		foreach (TemporaryAnimatedSprite aboveMapSprite in this.aboveMapSprites)
		{
			aboveMapSprite.alpha = 0f;
		}
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 2f, (float)Math.PI / 64f)
		{
			motion = new Vector2(2f, -2f),
			acceleration = new Vector2(0f, 0.1f)
		});
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 4f, (float)Math.PI / 64f)
		{
			motion = new Vector2(-2f, -1f),
			acceleration = new Vector2(0f, 0.1f)
		});
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI, (float)Math.PI / 64f)
		{
			motion = new Vector2(1f, 1f),
			acceleration = new Vector2(0f, 0.1f)
		});
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, 0f, (float)Math.PI / 64f)
		{
			motion = new Vector2(-2f, -2f),
			acceleration = new Vector2(0f, 0.1f)
		});
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(148, 165, 25, 23), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
		{
			id = 666
		});
		this.CurrentCommand++;
	}

	public virtual Vector2 OffsetPosition(Vector2 original)
	{
		return new Vector2(this.OffsetPositionX(original.X), this.OffsetPositionY(original.Y));
	}

	public virtual Vector2 OffsetTile(Vector2 original)
	{
		return new Vector2(this.OffsetTileX((int)original.X), this.OffsetTileY((int)original.Y));
	}

	public virtual float OffsetPositionX(float original)
	{
		if (original < 0f || this.ignoreTileOffsets)
		{
			return original;
		}
		return original + this.eventPositionTileOffset.X * 64f;
	}

	public virtual float OffsetPositionY(float original)
	{
		if (original < 0f || this.ignoreTileOffsets)
		{
			return original;
		}
		return original + this.eventPositionTileOffset.Y * 64f;
	}

	public virtual int OffsetTileX(int original)
	{
		if (original < 0 || this.ignoreTileOffsets)
		{
			return original;
		}
		return (int)((float)original + this.eventPositionTileOffset.X);
	}

	public virtual int OffsetTileY(int original)
	{
		if (original < 0 || this.ignoreTileOffsets)
		{
			return original;
		}
		return (int)((float)original + this.eventPositionTileOffset.Y);
	}

	private void addSpecificTemporarySprite(string key, GameLocation location, string[] args)
	{
		if (key == null)
		{
			return;
		}
		switch (key.Length)
		{
		case 13:
			switch (key[9])
			{
			case 'n':
				if (!(key == "raccoondance2"))
				{
					if (key == "raccoondance1")
					{
						TemporaryAnimatedSprite temporarySpriteByID4 = location.getTemporarySpriteByID(9786);
						TemporaryAnimatedSprite mrs_raccoon2 = location.getTemporarySpriteByID(9785);
						temporarySpriteByID4.sourceRect.Y = 96;
						temporarySpriteByID4.sourceRectStartingPos.Y = 96f;
						temporarySpriteByID4.currentParentTileIndex = 1;
						temporarySpriteByID4.motion.X = 0.07f;
						temporarySpriteByID4.timer = 0f;
						mrs_raccoon2.sourceRect.Y = 32;
						mrs_raccoon2.sourceRectStartingPos.Y = 32f;
						mrs_raccoon2.currentParentTileIndex = 1;
						mrs_raccoon2.motion.X = -0.07f;
						mrs_raccoon2.timer = 0f;
					}
				}
				else
				{
					location.removeTemporarySpritesWithIDLocal(9786);
					TemporaryAnimatedSprite temporarySpriteByID5 = location.getTemporarySpriteByID(9785);
					temporarySpriteByID5.sourceRect.Y = 64;
					temporarySpriteByID5.sourceRectStartingPos.Y = 64f;
					temporarySpriteByID5.currentParentTileIndex = 0;
					temporarySpriteByID5.motion.X = 0f;
					temporarySpriteByID5.interval *= 2f;
					temporarySpriteByID5.timer = 0f;
					temporarySpriteByID5.sourceRect.X = 0;
					temporarySpriteByID5.position.X -= 32f;
					temporarySpriteByID5.position.Y += 8f;
				}
				break;
			case 'r':
				if (key == "raccoonCircle")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\raccoon", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 148f, 8, 999, new Vector2(54.5f, 7f) * 64f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.051840004f,
						usePreciseTiming = true,
						id = 9786
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\mrs_raccoon", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 148f, 8, 999, new Vector2(56.5f, 7f) * 64f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.0512f,
						usePreciseTiming = true,
						id = 9785
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), Vector2.Zero, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						vectorScale = new Vector2(3090f, 1052f),
						interval = 99999f,
						totalNumberOfLoops = 1,
						id = 997799
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(56.5f, 0f) * 64f + new Vector2(131.5f, 0f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						vectorScale = new Vector2(5536f, 1052f),
						interval = 99999f,
						totalNumberOfLoops = 1,
						id = 997799
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(0f, 876f), flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						vectorScale = new Vector2(7552f, 7488f),
						interval = 99999f,
						totalNumberOfLoops = 1,
						id = 997799
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 263, 263), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 44f) * 4f, flipped: false, 0f, Color.Black)
					{
						drawAboveAlwaysFront = true,
						interval = 297f,
						animationLength = 3,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f
					});
				}
				break;
			case 'T':
				if (!(key == "trashBearTown"))
				{
					if (key == "stopShakeTent")
					{
						location.getTemporarySpriteByID(999).shakeIntensity = 0f;
					}
					break;
				}
				this.aboveMapSprites = new TemporaryAnimatedSpriteList();
				this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(46, 80, 46, 56), new Vector2(43f, 64f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					motion = new Vector2(4f, 0f),
					scale = 4f,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 32f,
					id = 777,
					xStopCoordinate = 3392,
					reachedStopCoordinate = delegate
					{
						this.aboveMapSprites[0].xStopCoordinate = -1;
						this.aboveMapSprites[0].motion = new Vector2(4f, 0f);
						location.ApplyMapOverride("Town-TrashGone", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5));
						location.ApplyMapOverride("Town-DogHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6));
						Game1.flashAlpha = 0.75f;
						Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
						location.playSound("yoba");
						TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), new Vector2(3456f, 4160f), flipped: false, 0f, Color.White)
						{
							yStopCoordinate = 4372,
							motion = new Vector2(-0.5f, -10f),
							acceleration = new Vector2(0f, 0.25f),
							scale = 4f,
							alphaFade = 0f,
							extraInfoForEndBehavior = -777
						};
						temporaryAnimatedSprite4.reachedStopCoordinate = temporaryAnimatedSprite4.bounce;
						temporaryAnimatedSprite4.initialPosition.Y = 4372f;
						this.aboveMapSprites.Add(temporaryAnimatedSprite4);
						this.aboveMapSprites.AddRange(Utility.getStarsAndSpirals(location, 54, 69, 6, 5, 1000, 10, Color.Lime));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1,
							delayBeforeAnimationStart = 3000,
							startSound = "dogWhining"
						});
					}
				});
				break;
			case 'S':
				if (key == "shakeBushStop")
				{
					location.getTemporarySpriteByID(777).shakeIntensity = 0f;
				}
				break;
			case 'F':
				if (key == "sebastianFrog")
				{
					Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = crittersText3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
						animationLength = 4,
						sourceRectStartingPos = new Vector2(0f, 224f),
						interval = 120f,
						totalNumberOfLoops = 9999,
						position = new Vector2(45f, 36f) * 64f,
						scale = 4f,
						layerDepth = 0.00064f,
						motion = new Vector2(2f, 0f),
						xStopCoordinate = 3136,
						id = 777,
						reachedStopCoordinate = delegate
						{
							int num = this.CurrentCommand;
							this.CurrentCommand = num + 1;
							location.removeTemporarySpritesWithID(777);
						}
					});
				}
				break;
			case 'W':
				if (key == "haleyCakeWalk")
				{
					Texture2D tempTxture100 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture100,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 400, 144, 112),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 400f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(26f, 65f) * 64f,
						scale = 4f,
						layerDepth = 0.00064f
					});
				}
				break;
			case 'a':
				if (key == "pamYobaStatue")
				{
					location.objects.Remove(new Vector2(26f, 9f));
					location.objects.Add(new Vector2(26f, 9f), ItemRegistry.Create<Object>("(BC)34"));
					GameLocation gameLocation = Game1.RequireLocation("Trailer_Big");
					gameLocation.objects.Remove(new Vector2(26f, 9f));
					gameLocation.objects.Add(new Vector2(26f, 9f), ItemRegistry.Create<Object>("(BC)34"));
				}
				break;
			case 'p':
				if (key == "EmilySleeping")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(574, 1892, 11, 11), 1000f, 2, 99999, new Vector2(20f, 3f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
				}
				break;
			case 'i':
				if (!(key == "shaneHospital"))
				{
					if (key == "grandpaSpirit")
					{
						TemporaryAnimatedSprite p = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(555, 1956, 18, 35), 9999f, 1, 99999, new Vector2(-1000f, -1010f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							yStopCoordinate = -64128,
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							motion = new Vector2(0f, 1f),
							overrideLocationDestroy = true,
							id = 77777
						};
						location.temporarySprites.Add(p);
						for (int i16 = 0; i16 < 19; i16++)
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(32f, 32f), Color.White)
							{
								parentSprite = p,
								delayBeforeAnimationStart = (i16 + 1) * 500,
								overrideLocationDestroy = true,
								scale = 1f,
								alpha = 1f
							});
						}
					}
				}
				else
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 10), 99999f, 1, 99999, new Vector2(20f, 3f) * 64f + new Vector2(16f, 12f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
				}
				break;
			case 'w':
				if (key == "shaneThrowCan")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(103f, 95f) * 64f + new Vector2(0f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -4f),
						acceleration = new Vector2(0f, 0.25f),
						rotationChange = (float)Math.PI / 128f
					});
					Game1.playSound("shwip");
				}
				break;
			case 'm':
				if (key == "WizardPromise")
				{
					Utility.addSprinklesToLocation(location, 16, 15, 9, 9, 2000, 50, Color.White);
				}
				break;
			case 'f':
				if (key == "linusCampfire")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 99999, new Vector2(29f, 9f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 0.0576f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("linusCampfire"),
						lightRadius = 3f,
						lightcolor = Color.Black
					});
				}
				break;
			case 't':
				if (key == "ccCelebration")
				{
					this.aboveMapSprites = new TemporaryAnimatedSpriteList();
					for (int i17 = 0; i17 < 32; i17++)
					{
						Vector2 position2 = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + i17 * 64);
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position2, flicker: false, flipped: false, 1f, 0f, Utility.getRandomRainbowColor(), 4f, 0f, 0f, 0f)
						{
							local = true,
							motion = new Vector2(0.25f, -1.5f),
							acceleration = new Vector2(0f, -0.001f),
							id = 79797 + i17
						});
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position2 + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							local = true,
							motion = new Vector2(0.25f, -1.5f),
							acceleration = new Vector2(0f, -0.001f),
							id = 79797 + i17
						});
					}
					if (Game1.IsWinter)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\marnie_winter_dance", new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
					}
				}
				break;
			case 'g':
				if (key == "alexDiningDog")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(7f, 2f) * 64f + new Vector2(2f, -8f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 1
					});
				}
				break;
			case 'd':
				if (key == "skateboardFly")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1875, 16, 6), 9999f, 1, 999, new Vector2(26f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						rotationChange = (float)Math.PI / 24f,
						motion = new Vector2(-8f, -10f),
						acceleration = new Vector2(0.02f, 0.3f),
						yStopCoordinate = 5824,
						xStopCoordinate = 1024,
						layerDepth = 1f
					});
				}
				break;
			case 'R':
				if (key == "sebastianRide")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1843, 14, 9), 40f, 4, 999, new Vector2(19f, 8f) * 64f + new Vector2(0f, 28f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f)
					});
				}
				break;
			case 'D':
				if (key == "haleyRoomDark")
				{
					Game1.currentLightSources.Clear();
					Game1.ambientLight = new Color(200, 200, 100);
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(743, 999999f, 1, 0, new Vector2(4f, 1f) * 64f, flicker: false, flipped: false)
					{
						lightId = this.GenerateLightSourceId("haleyRoomDark"),
						lightcolor = new Color(0, 255, 255),
						lightRadius = 2f
					});
				}
				break;
			case 'c':
				if (key == "maruTelescope")
				{
					for (int i15 = 0; i15 < 9; i15++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(256, 1680, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 28), Game1.random.Next(1, 20)) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 8000 + i15 * Game1.random.Next(2000),
							motion = new Vector2(4f, 4f)
						});
					}
					if (this.id == "5183338")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(206, 1827, 15, 27), 80f, 4, 999, new Vector2(-2f, 13f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 1.2f, 0f)
						{
							delayBeforeAnimationStart = 7000,
							motion = new Vector2(2f, -0.5f),
							alpha = 0.01f,
							alphaFade = -0.005f
						});
					}
				}
				break;
			case 'y':
				if (key == "abbyGraveyard")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(736, 999999f, 1, 0, new Vector2(48f, 86f) * 64f, flicker: false, flipped: false));
				}
				break;
			}
			break;
		case 18:
			switch (key[10])
			{
			case 't':
				if (key == "raccoonbutterflies")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 336, 16, 16), new Vector2(52.5f, 0f) * 64f - new Vector2(131.5f, -60f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 2800f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 8f,
						yPeriodicLoopTime = 3800f,
						overrideLocationDestroy = true
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 336, 16, 16), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 0f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 2600f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 4f,
						yPeriodicLoopTime = 2900f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 288, 16, 16), new Vector2(53.5f, 0f) * 64f + new Vector2(263f, 24f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 3000f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 6f,
						yPeriodicLoopTime = 3100f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 288, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(131.5f, 220f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 2400f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 12f,
						yPeriodicLoopTime = 2800f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(64, 288, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(186.5f, 150f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 3400f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 4f,
						yPeriodicLoopTime = 3200f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 96, 16, 16), new Vector2(52.5f, 0f) * 64f + new Vector2(211.5f, 180f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 32f,
						xPeriodicLoopTime = 3500f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 4f,
						yPeriodicLoopTime = 2700f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(192, 112, 16, 16), new Vector2(52.5f, 0f) * 64f - new Vector2(126.5f, -120f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 16f,
						xPeriodicLoopTime = 2500f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 4f,
						yPeriodicLoopTime = 3300f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(128, 288, 16, 16), new Vector2(49.5f, 0f) * 64f - new Vector2(126.5f, -100f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 148f,
						animationLength = 4,
						pingPong = true,
						totalNumberOfLoops = 99999,
						id = 997799,
						scale = 4f,
						xPeriodic = true,
						xPeriodicRange = 16f,
						xPeriodicLoopTime = 2200f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						yPeriodic = true,
						yPeriodicRange = 4f,
						yPeriodicLoopTime = 3400f
					});
					TemporaryAnimatedSprite temporarySpriteByID3 = location.getTemporarySpriteByID(9786);
					TemporaryAnimatedSprite mrs_raccoon = location.getTemporarySpriteByID(9785);
					temporarySpriteByID3.sourceRect.Y = 224;
					temporarySpriteByID3.sourceRectStartingPos.Y = 224f;
					temporarySpriteByID3.currentParentTileIndex = 3;
					temporarySpriteByID3.timer = 0f;
					temporarySpriteByID3.sourceRect.X = 96;
					mrs_raccoon.sourceRect.Y = 224;
					mrs_raccoon.sourceRectStartingPos.Y = 224f;
					mrs_raccoon.currentParentTileIndex = 3;
					mrs_raccoon.timer = 0f;
					mrs_raccoon.sourceRect.X = 96;
				}
				break;
			case 'a':
			{
				if (!(key == "terraria_cat_leave"))
				{
					break;
				}
				TemporaryAnimatedSprite terraria_cat = location.getTemporarySpriteByID(777);
				if (terraria_cat == null)
				{
					break;
				}
				terraria_cat.sourceRect.Y = 0;
				terraria_cat.sourceRect.X = terraria_cat.currentParentTileIndex * 16;
				terraria_cat.paused = false;
				terraria_cat.motion = new Vector2(1f, 0f);
				terraria_cat.xStopCoordinate = 1152;
				terraria_cat.flipped = true;
				Microsoft.Xna.Framework.Rectangle warpRect2 = new Microsoft.Xna.Framework.Rectangle(1024, 120, 144, 272);
				terraria_cat.reachedStopCoordinate = delegate
				{
					terraria_cat.position.X = -4000f;
					location.removeTemporarySpritesWithID(888);
					Game1.playSound("terraria_warp");
					for (int l = 0; l < 80; l++)
					{
						Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(warpRect2, Game1.random);
						Vector2 vector = randomPositionInThisRectangle - Utility.PointToVector2(warpRect2.Center);
						vector.Normalize();
						vector *= (float)(Game1.random.Next(10, 21) / 10);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(113 + Game1.random.Next(3) * 5, 123, 5, 5), 999f, 1, 9999, randomPositionInThisRectangle, flicker: false, flipped: false, 0.8f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
						{
							layerDepth = 0.99f,
							rotationChange = (float)Game1.random.Next(-10, 10) / 100f,
							motion = vector,
							acceleration = -vector / 150f,
							scaleChange = (float)Game1.random.Next(-10, 0) / 500f,
							delayBeforeAnimationStart = l * 5
						});
					}
				};
				break;
			}
			case 'm':
				if (key == "trashBearUmbrella1")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 80, 46, 56), new Vector2(102f, 94.5f) * 64f, flipped: false, 0f, Color.White)
					{
						animationLength = 1,
						interval = 999999f,
						motion = new Vector2(0f, -9f),
						acceleration = new Vector2(0f, 0.4f),
						scale = 4f,
						layerDepth = 1f,
						id = 777,
						yStopCoordinate = 6144,
						reachedStopCoordinate = delegate(int param)
						{
							location.getTemporarySpriteByID(777).yStopCoordinate = -1;
							location.getTemporarySpriteByID(777).motion = new Vector2(0f, (float)param * 0.75f);
							location.getTemporarySpriteByID(777).acceleration = new Vector2(0.04f, -0.19f);
							location.getTemporarySpriteByID(777).accelerationChange = new Vector2(0f, 0.0015f);
							location.getTemporarySpriteByID(777).sourceRect.X += 46;
							location.playSound("batFlap");
							location.playSound("tinyWhip");
						}
					});
				}
				break;
			case 'e':
				if (key == "movieTheater_setup")
				{
					Game1.currentLightSources.Add(new LightSource("Event_MovieProjector", 7, new Vector2(192f, 64f) + new Vector2(64f, 80f) * 4f, 4f, LightSource.LightContext.None, 0L));
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("Maps\\MovieTheaterScreen_TileSheet"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(224, 0, 96, 112),
						sourceRectStartingPos = new Vector2(224f, 0f),
						animationLength = 1,
						interval = 5000f,
						totalNumberOfLoops = 9999,
						scale = 4f,
						position = new Vector2(4f, 4f) * 64f,
						layerDepth = 1f,
						id = 999,
						delayBeforeAnimationStart = 7950
					});
				}
				break;
			case 'i':
				if (key == "missingJunimoStars")
				{
					location.removeTemporarySpritesWithID(999);
					Texture2D tempTxture98 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					for (int i14 = 0; i14 < 48; i14++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture98,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(477, 306, 28, 28),
							sourceRectStartingPos = new Vector2(477f, 306f),
							animationLength = 1,
							interval = 5000f,
							totalNumberOfLoops = 10,
							scale = Game1.random.Next(1, 5),
							position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 84, 84) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)),
							rotationChange = (float)Math.PI / (float)Game1.random.Next(16, 128),
							motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(20, 90) * -0.1f),
							acceleration = new Vector2(0f, 0.05f),
							local = true,
							layerDepth = (float)i14 / 100f,
							color = (Game1.random.NextBool() ? Color.White : Utility.getRandomRainbowColor())
						});
					}
				}
				break;
			case 'r':
				if (key == "sebastianFrogHouse")
				{
					Point frog_spot = (location as FarmHouse).GetSpouseRoomCorner();
					frog_spot.X++;
					frog_spot.Y += 6;
					Vector2 spot = Utility.PointToVector2(frog_spot);
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(641f, 1534f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2(0f, -5f) * 4f,
						scale = 4f,
						layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
					});
					Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = crittersText2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 224f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2(25f, 2f) * 4f,
						scale = 4f,
						flipped = true,
						layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
						id = 777
					});
				}
				break;
			case 'h':
				if (!(key == "harveyKitchenFlame"))
				{
					if (key == "harveyKitchenSetup")
					{
						Texture2D tempTxture99 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture99,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(379, 251, 31, 13),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(379f, 251f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(22f, 22f) * 64f + new Vector2(-2f, 6f) * 4f,
							scale = 4f,
							layerDepth = 0.15551999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture99,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(391, 235, 5, 13),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(391f, 235f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(21f, 22f) * 64f + new Vector2(8f, 4f) * 4f,
							scale = 4f,
							layerDepth = 0.15551999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = tempTxture99,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(399, 229, 11, 21),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(399f, 229f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(19f, 22f) * 64f + new Vector2(8f, -5f) * 4f,
							scale = 4f,
							layerDepth = 0.15551999f
						});
						location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(21f, 22f) * 64f + new Vector2(0f, -5f) * 4f, Color.White, 10)
						{
							totalNumberOfLoops = 999,
							layerDepth = 0.15616f
						});
						location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(21f, 22f) * 64f + new Vector2(24f, -5f) * 4f, Color.White, 10)
						{
							totalNumberOfLoops = 999,
							flipped = true,
							delayBeforeAnimationStart = 400,
							layerDepth = 0.15616f
						});
					}
				}
				else
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
						animationLength = 4,
						sourceRectStartingPos = new Vector2(276f, 1985f),
						interval = 100f,
						totalNumberOfLoops = 6,
						position = new Vector2(22f, 22f) * 64f + new Vector2(8f, 5f) * 4f,
						scale = 4f,
						layerDepth = 0.15584001f
					});
				}
				break;
			case 'P':
				if (key == "farmerHoldPainting")
				{
					Texture2D tempTxture10 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.getTemporarySpriteByID(888).sourceRect.X += 15;
					location.getTemporarySpriteByID(888).sourceRectStartingPos.X += 15f;
					location.removeTemporarySpritesWithID(444);
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture10,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(476, 394, 25, 22),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(476f, 394f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(75f, 40f) * 64f + new Vector2(-4f, -33f) * 4f,
						scale = 4f,
						layerDepth = 1f,
						id = 777
					});
				}
				break;
			case 's':
			{
				if (!(key == "farmerForestVision"))
				{
					break;
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(393, 1973, 1, 1), 9999f, 1, 999999, new Vector2(0f, 0f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.LimeGreen * 0.85f, Game1.viewport.Width * 2, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.002f,
					id = 1
				});
				Game1.player.mailReceived.Add("canReadJunimoText");
				int x = -64;
				int y = -64;
				int index = 0;
				int yIndex = 0;
				while (y < Game1.viewport.Height + 128)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(367 + ((index % 2 == 0) ? 8 : 0), 1969, 8, 8), 9999f, 1, 999999, new Vector2(x, y), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.0015f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 96f,
						rotationChange = (float)Game1.random.Next(-1, 2) * (float)Math.PI / 256f,
						id = 1,
						delayBeforeAnimationStart = 20 * index
					});
					x += 128;
					if (x > Game1.viewport.Width + 64)
					{
						yIndex++;
						x = ((yIndex % 2 == 0) ? (-64) : 64);
						y += 128;
					}
					index++;
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 2 - 100, Game1.viewport.Height / 2 - 240), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 6000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 4 - 100, Game1.viewport.Height / 4 - 120), flicker: false, flipped: false, 0.99f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 9000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 3 / 4, Game1.viewport.Height / 3 - 120), flicker: false, flipped: false, 0.98f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 12000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 3 - 60, Game1.viewport.Height * 3 / 4 - 120), flicker: false, flipped: false, 0.97f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 15000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height * 2 / 3 - 120), flicker: false, flipped: false, 0.96f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 18000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 8, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.95f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 19500,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.94f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1,
					delayBeforeAnimationStart = 21000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				break;
			}
			}
			break;
		case 14:
			switch (key[6])
			{
			case 'n':
				if (key == "raccoonCircle2")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\raccoon_circle_cutout", new Microsoft.Xna.Framework.Rectangle(0, 0, 263, 263), new Vector2(56.5f, 0f) * 64f - new Vector2(131.5f, 44f) * 4f, flipped: false, 0f, Color.White)
					{
						drawAboveAlwaysFront = true,
						interval = 297f,
						animationLength = 3,
						totalNumberOfLoops = 99999,
						id = 997797,
						scale = 4f,
						alpha = 0.01f,
						alphaFade = -0.003f,
						layerDepth = 0.8f
					});
				}
				break;
			case 'L':
				if (key == "georgeLeekGift")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(17f, 19f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999,
						paused = false,
						holdLastFrame = true
					});
				}
				break;
			case 'P':
				if (key == "parrotPerchHut")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24), new Vector2(7f, 4f) * 64f, flipped: false, 0f, Color.White)
					{
						animationLength = 1,
						interval = 999999f,
						scale = 4f,
						layerDepth = 1f,
						id = 999
					});
				}
				break;
			case 'e':
				if (key == "trashBearMagic")
				{
					Utility.addStarsAndSpirals(location, 95, 103, 24, 12, 2000, 10, Color.Lime);
					(location as Forest).removeSewerTrash();
					Game1.flashAlpha = 0.75f;
					Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
				}
				break;
			case 'l':
				if (key == "gridballGameTV")
				{
					Texture2D tempTxture9 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture9,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
						animationLength = 7,
						sourceRectStartingPos = new Vector2(368f, 336f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
						scale = 4f,
						layerDepth = 1f
					});
				}
				break;
			case 'h':
				if (key == "waterShaneDone")
				{
					this.farmer.completelyStopAnimatingOrDoingAction();
					this.farmer.TemporaryItem = null;
					this.drawTool = false;
					location.removeTemporarySpritesWithID(999);
				}
				break;
			case 'a':
				if (key == "shanePassedOut")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'C':
				if (key == "junimoCageGone")
				{
					location.removeTemporarySpritesWithID(1);
				}
				break;
			case 'G':
				if (key == "secretGiftOpen")
				{
					TemporaryAnimatedSprite t = location.getTemporarySpriteByID(666);
					if (t != null)
					{
						t.animationLength = 6;
						t.interval = 100f;
						t.totalNumberOfLoops = 1;
						t.timer = 0f;
						t.holdLastFrame = true;
					}
				}
				break;
			case 'B':
				if (key == "candleBoatMove")
				{
					this.showGroundObjects = false;
					location.getTemporarySpriteByID(1).motion = new Vector2(0f, 2f);
				}
				break;
			case 'i':
				if (key == "pennyFieldTrip")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1813, 86, 54), 999999f, 1, 0, new Vector2(68f, 44f) * 64f, flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			}
			break;
		case 11:
			switch (key[5])
			{
			case 'o':
				if (key == "raccoonSong")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3706f, 340f) - new Vector2(6.5f, 12f) * 4f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						usePreciseTiming = true
					});
					for (int i3 = 0; i3 < 8; i3++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(3706f, 340f) + new Vector2(14f, -12f) * 4f, flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.05057f,
							delayBeforeAnimationStart = 2376 * i3,
							usePreciseTiming = true,
							motion = new Vector2(1f, 0f),
							acceleration = new Vector2(0f, 0.001f),
							color = new Color(255, 200, 200),
							rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(455, 414, 14, 17), 2376f, 1, 999, new Vector2(3706f, 340f) + new Vector2(7f, -12f) * 4f, flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i3,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(54f, 4f) * 64f + new Vector2(0f, -16f), flicker: false, flipped: true)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 297,
						usePreciseTiming = true
					});
					for (int i4 = 0; i4 < 8; i4++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(385, 414, 14, 17), 2376f, 1, 999, new Vector2(54f, 4f) * 64f + new Vector2(16f, -8f) + new Vector2(-15f, -17f) * 4f, flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i4 + 297,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3462f, 433f) - new Vector2(6.5f, 12f) * 4f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 594,
						usePreciseTiming = true
					});
					for (int i5 = 0; i5 < 8; i5++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(3462f, 433f) + new Vector2(-20f, -16f) + new Vector2(-15f, -17f) * 4f, flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.05057f,
							delayBeforeAnimationStart = 2376 * i5 + 594,
							usePreciseTiming = true,
							motion = new Vector2(-1f, -1f),
							acceleration = new Vector2(0f, 0.001f),
							color = new Color(180, 200, 255),
							rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(371, 414, 14, 17), 2376f, 1, 999, new Vector2(3462f, 433f) + new Vector2(-20f, -16f) + new Vector2(-15f, -17f) * 4f, flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i5 + 594,
							alphaFade = 0.013f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(58f, 4f) * 64f + new Vector2(0f, -24f), flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 891,
						usePreciseTiming = true
					});
					for (int i6 = 0; i6 < 8; i6++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(440, 415, 14, 15), 2376f, 1, 999, new Vector2(58f, 4f) * 64f + new Vector2(48f, -56f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i6 + 891,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(3770f, 408f) - new Vector2(6.5f, 12f) * 4f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 1188,
						usePreciseTiming = true
					});
					for (int i7 = 0; i7 < 8; i7++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(469, 415, 14, 14), 2376f, 1, 999, new Vector2(3770f, 408f) + new Vector2(24f, -64f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i7 + 1188,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(55f, 3f) * 64f + new Vector2(12f, 4f) - new Vector2(6.5f, 12f) * 4f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 1485,
						usePreciseTiming = true
					});
					for (int i8 = 0; i8 < 8; i8++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(400, 414, 12, 16), 2376f, 1, 999, new Vector2(55f, 3f) * 64f + new Vector2(-32f, -100f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i8 + 1485,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(279, 55, 12, 15), 297f, 8, 999, new Vector2(56f, 3f) * 64f + new Vector2(40f, -8f) - new Vector2(6.5f, 12f) * 4f, flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 1782,
						usePreciseTiming = true
					});
					for (int i9 = 0; i9 < 8; i9++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(304, 397, 11, 11), 49f, 12, 1, new Vector2(56f, 3f) * 64f + new Vector2(12f, -112f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.05057f,
							delayBeforeAnimationStart = 2376 * i9 + 1782,
							usePreciseTiming = true,
							motion = new Vector2(-0.25f, -1.5f),
							acceleration = new Vector2(0f, 0.001f),
							color = new Color(220, 255, 180),
							rotationChange = (float)Game1.random.Next(-20, 20) / 1000f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(414, 414, 12, 16), 2376f, 1, 999, new Vector2(56f, 3f) * 64f + new Vector2(12f, -112f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i9 + 1782,
							alphaFade = 0.013f,
							usePreciseTiming = true
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(374, 55, 12, 15), 297f, 8, 999, new Vector2(58f, 3f) * 64f + new Vector2(-24f, -52f), flicker: false, flipped: false)
					{
						scale = 4f,
						layerDepth = 0.044809997f,
						delayBeforeAnimationStart = 2079,
						usePreciseTiming = true
					});
					for (int i10 = 0; i10 < 8; i10++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(426, 414, 14, 15), 2376f, 1, 999, new Vector2(58f, 3f) * 64f + new Vector2(28f, -88f), flicker: false, flipped: false)
						{
							scale = 4f,
							layerDepth = 0.051209997f,
							delayBeforeAnimationStart = 2376 * i10 + 2079,
							alphaFade = 0.02f,
							usePreciseTiming = true
						});
					}
				}
				break;
			case 's':
				if (!(key == "krobusBeach"))
				{
					if (!(key == "krobusraven"))
					{
						break;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 100f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 16f,
						startSound = "shadowpeep"
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 32, 32, 32), 30f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2.5f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2800f,
						yPeriodicRange = 16f,
						delayBeforeAnimationStart = 8000
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 64, 32, 39), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-3f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 16f,
						delayBeforeAnimationStart = 15000,
						startSound = "fireball"
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2200f,
						yPeriodicRange = 32f,
						local = true,
						delayBeforeAnimationStart = 20000
					});
					for (int i11 = 0; i11 < 12; i11++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(16, 594, 16, 12), 100f, 2, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = Game1.random.Next(1500, 2000),
							yPeriodicRange = 32f,
							local = true,
							delayBeforeAnimationStart = 24000 + i11 * 200,
							startSound = ((i11 == 0) ? "yoba" : null)
						});
					}
					int whenToStart = 0;
					if (Game1.player.mailReceived.Contains("Capsule_Broken"))
					{
						for (int i12 = 0; i12 < 3; i12++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(639, 785, 16, 16), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(-2f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = Game1.random.Next(1500, 2000),
								yPeriodicRange = 16f,
								local = true,
								delayBeforeAnimationStart = 30000 + i12 * 500,
								startSound = ((i12 == 0) ? "UFO" : null)
							});
						}
						whenToStart += 5000;
					}
					if (Game1.year <= 2)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							delayBeforeAnimationStart = 30000 + whenToStart
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(2, 129, 120, 27), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "discoverMineral",
							delayBeforeAnimationStart = 30000 + whenToStart
						});
						whenToStart += 5000;
					}
					else if (Game1.year <= 3)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							delayBeforeAnimationStart = 30000 + whenToStart
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(1, 104, 100, 24), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "newArtifact",
							delayBeforeAnimationStart = 30000 + whenToStart
						});
						whenToStart += 5000;
					}
					if (Game1.MasterPlayer.totalMoneyEarned >= 100000000)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(125, 108, 34, 50), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "discoverMineral",
							delayBeforeAnimationStart = 30000 + whenToStart
						});
						whenToStart += 5000;
					}
				}
				else
				{
					for (int i13 = 0; i13 < 8; i13++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 4, 0, new Vector2(84f + ((i13 % 2 == 0) ? 0.25f : (-0.05f)), 41f) * 64f, flicker: false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f)
						{
							delayBeforeAnimationStart = 500 + i13 * 1000,
							startSound = "waterSlosh"
						});
					}
					this.underwaterSprites = new TemporaryAnimatedSpriteList
					{
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_1"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 0,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_2"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 3008,
							delayBeforeAnimationStart = 2000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_3"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 150,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_4"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 3008,
							delayBeforeAnimationStart = 2000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(90f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_5"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 300,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(79f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId("krobusBeach_6"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 1000,
							pingPong = true
						}
					};
				}
				break;
			case 'w':
				if (key == "woodswalker")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(448, 419, 16, 21),
						sourceRectStartingPos = new Vector2(448f, 419f),
						animationLength = 4,
						totalNumberOfLoops = 7,
						interval = 150f,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(5f, 22f) * 4f,
						shakeIntensity = 1f,
						motion = new Vector2(1f, 0f),
						xStopCoordinate = 576,
						layerDepth = 1f,
						id = 996
					});
				}
				break;
			case 'g':
				if (key == "springOnion")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(1, 129, 16, 16), 200f, 8, 999999, new Vector2(84f, 39f) * 64f, flicker: false, flipped: false, 0.4736f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
				}
				break;
			case 'i':
				if (key == "curtainOpen")
				{
					location.getTemporarySpriteByID(999).sourceRect.X = 672;
					Game1.playSound("shwip");
				}
				break;
			case 't':
				switch (key)
				{
				case "parrotSlide":
					location.getTemporarySpriteByID(666).yStopCoordinate = 5632;
					location.getTemporarySpriteByID(666).motion.X = 0f;
					location.getTemporarySpriteByID(666).motion.Y = 1f;
					break;
				case "parrotSplat":
					this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.viewport.X + Game1.graphics.GraphicsDevice.Viewport.Width, Game1.viewport.Y + 64), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999,
						motion = new Vector2(-2f, 4f),
						acceleration = new Vector2(-0.1f, 0f),
						delayBeforeAnimationStart = 0,
						yStopCoordinate = 5568,
						xStopCoordinate = 1504,
						reachedStopCoordinate = parrotSplat
					});
					break;
				case "elliottBoat":
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(461, 1843, 32, 51), 1000f, 2, 9999, new Vector2(15f, 26f) * 64f + new Vector2(-28f, 0f), flicker: false, flipped: false, 0.1664f, 0f, Color.White, 4f, 0f, 0f, 0f));
					break;
				}
				break;
			case 'C':
				if (key == "shaneCliffs")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(84f, 99f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(82f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(83f, 99f) * 64f + new Vector2(-8f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'f':
				if (key == "jasGiftOpen")
				{
					location.getTemporarySpriteByID(999).paused = false;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(537, 1850, 11, 10), 1500f, 1, 1, new Vector2(23f, 16f) * 64f + new Vector2(16f, -48f), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -0.25f),
						delayBeforeAnimationStart = 500,
						yStopCoordinate = 928
					});
					location.temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(1440, 992, 128, 64), 5, Color.White, 300));
				}
				break;
			case 'd':
				if (key == "wizardWarp2")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(54f, 34f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-1f, 2f),
						acceleration = new Vector2(-0.1f, 0.2f),
						scaleChange = 0.03f,
						alphaFade = 0.001f
					});
				}
				break;
			case 'L':
				if (key == "linusLights")
				{
					string lightSourceId = this.GenerateLightSourceId("linusLights");
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "1", 2, new Vector2(55f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "2", 2, new Vector2(60f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "3", 2, new Vector2(57f, 60f) * 64f, 3f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "4", 2, new Vector2(57f, 60f) * 64f, 2f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "5", 2, new Vector2(47f, 70f) * 64f, 2f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(lightSourceId + "6", 2, new Vector2(52f, 63f) * 64f, 2f, LightSource.LightContext.None, 0L));
				}
				break;
			case 'l':
				if (key == "dickGlitter")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 300
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(16f, 16f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 400
					});
				}
				break;
			}
			break;
		case 19:
			switch (key[0])
			{
			case 't':
			{
				if (!(key == "terraria_warp_begin"))
				{
					break;
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 18, 36, 68), 90f, 3, 9999, new Vector2(16f, 5f) * 64f + new Vector2(0f, -50f) * 4f, flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					layerDepth = 0.8f,
					id = 888
				});
				TemporaryAnimatedSprite cat_sprite = new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 90f, 8, 9999, new Vector2(16f, 5f) * 64f + new Vector2(34f, -12f) * 4f, flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 777,
					layerDepth = 0.85f,
					motion = new Vector2(-1f, 0f),
					delayBeforeAnimationStart = 1000,
					xStopCoordinate = 960
				};
				cat_sprite.reachedStopCoordinate = delegate
				{
					cat_sprite.paused = true;
					cat_sprite.sourceRect = new Microsoft.Xna.Framework.Rectangle(112, 16, 16, 16);
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.playSound("terraria_meowmere");
						cat_sprite.shakeIntensity = 1f;
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 1000f, 1, 1, new Vector2(15f, 5f) * 64f, flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							layerDepth = 0.86f,
							motion = new Vector2(-1f, -4f),
							acceleration = new Vector2(0f, 0.1f)
						});
					}, 1000);
					DelayedAction.functionAfterDelay(delegate
					{
						cat_sprite.shakeIntensity = 0f;
					}, 1300);
				};
				location.TemporarySprites.Add(cat_sprite);
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(4, 88, 19, 15), 90f, 3, 9999, new Vector2(16f, 5f) * 64f + new Vector2(31f, -10f) * 4f, flicker: false, flipped: false, 0.8f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					layerDepth = 0.9f,
					id = 888
				});
				Microsoft.Xna.Framework.Rectangle warpRect = new Microsoft.Xna.Framework.Rectangle(1024, 120, 144, 272);
				for (int i2 = 0; i2 < 80; i2++)
				{
					Vector2 warpSparklePos = Utility.getRandomPositionInThisRectangle(warpRect, Game1.random);
					Vector2 warpSkarleMotion = warpSparklePos - Utility.PointToVector2(warpRect.Center);
					warpSkarleMotion.Normalize();
					warpSkarleMotion *= (float)(Game1.random.Next(10, 21) / 10);
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(113 + Game1.random.Next(3) * 5, 123, 5, 5), 999f, 1, 9999, warpSparklePos, flicker: false, flipped: false, 0.8f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
					{
						layerDepth = 0.99f,
						rotationChange = (float)Game1.random.Next(-10, 10) / 100f,
						motion = warpSkarleMotion,
						acceleration = -warpSkarleMotion / 150f,
						scaleChange = (float)Game1.random.Next(-10, 0) / 500f,
						delayBeforeAnimationStart = i2 * 5
					});
				}
				break;
			}
			case 'm':
			{
				if (!(key == "movieTheater_screen"))
				{
					break;
				}
				if (!ArgUtility.TryGet(args, 2, out var movieId, out var error, allowBlank: true, "string movieId") || !ArgUtility.TryGetInt(args, 3, out var screenIndex, out error, "int screenIndex") || !ArgUtility.TryGetBool(args, 4, out var shake, out error, "bool shake"))
				{
					this.LogCommandError(args, error);
					break;
				}
				movieId = MovieTheater.GetMovieIdFromLegacyIndex(movieId);
				if (!MovieTheater.TryGetMovieData(movieId, out var data2))
				{
					this.LogCommandError(args, "No movie found with ID '" + movieId + "'.");
					break;
				}
				Microsoft.Xna.Framework.Rectangle sourceRect2 = MovieTheater.GetSourceRectForScreen(data2.SheetIndex, screenIndex);
				location.removeTemporarySpritesWithIDLocal(998);
				if (screenIndex >= 0)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>(data2.Texture ?? "LooseSprites\\Movies"),
						sourceRect = sourceRect2,
						sourceRectStartingPos = new Vector2(sourceRect2.X, sourceRect2.Y),
						animationLength = 1,
						totalNumberOfLoops = 9999,
						interval = 5000f,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
						shakeIntensity = (shake ? 1f : 0f),
						layerDepth = 0.0128f,
						id = 998
					});
				}
				break;
			}
			case 'w':
				if (key == "willyCrabExperiment")
				{
					Texture2D tempTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 250f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(2f, 4f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 200f,
						totalNumberOfLoops = 99999,
						id = 1,
						initialPosition = new Vector2(2f, 6f) * 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 8000f,
						yPeriodicRange = 32f,
						position = new Vector2(2f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(1f, 5.75f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11,
						position = new Vector2(5f, 3f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 140f,
						totalNumberOfLoops = 99999,
						id = 22,
						position = new Vector2(4f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 140f,
						totalNumberOfLoops = 99999,
						id = 22,
						position = new Vector2(8.5f, 5f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 170f,
						totalNumberOfLoops = 99999,
						id = 222,
						position = new Vector2(6f, 3.25f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 190f,
						totalNumberOfLoops = 99999,
						id = 222,
						position = new Vector2(6f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 150f,
						totalNumberOfLoops = 99999,
						id = 222,
						position = new Vector2(7f, 4f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 200f,
						totalNumberOfLoops = 99999,
						id = 2,
						position = new Vector2(4f, 7f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 127f),
						pingPong = true,
						interval = 180f,
						totalNumberOfLoops = 99999,
						id = 3,
						position = new Vector2(8f, 6f) * 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 10000f,
						yPeriodicRange = 32f,
						initialPosition = new Vector2(8f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 220f,
						totalNumberOfLoops = 99999,
						id = 33,
						position = new Vector2(9f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTexture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
						animationLength = 3,
						sourceRectStartingPos = new Vector2(259f, 146f),
						pingPong = true,
						interval = 150f,
						totalNumberOfLoops = 99999,
						id = 33,
						position = new Vector2(10f, 5f) * 64f,
						scale = 4f
					});
				}
				break;
			case 'E':
			{
				if (!(key == "EmilySongBackLights"))
				{
					break;
				}
				this.aboveMapSprites = new TemporaryAnimatedSpriteList();
				for (int lightcolumns = 0; lightcolumns < 5; lightcolumns++)
				{
					for (int yPos = 0; yPos < Game1.graphics.GraphicsDevice.Viewport.Height + 48; yPos += 48)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(681, 1890, 18, 12), 42241f, 1, 1, new Vector2((lightcolumns + 1) * Game1.graphics.GraphicsDevice.Viewport.Width / 5 - Game1.graphics.GraphicsDevice.Viewport.Width / 7, -24 + yPos), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 1760f,
							xPeriodicRange = 128 + yPos / 12 * 4,
							delayBeforeAnimationStart = lightcolumns * 100 + yPos / 4,
							local = true
						});
					}
				}
				for (int numFlyers = 0; numFlyers < 27; numFlyers++)
				{
					int flyerNumber = 0;
					int yPos2 = Game1.random.Next(64, Game1.graphics.GraphicsDevice.Viewport.Height - 64);
					int loopTime = Game1.random.Next(800, 2000);
					int loopRange = Game1.random.Next(32, 64);
					bool pulse = Game1.random.NextDouble() < 0.25;
					int speed = Game1.random.Next(-6, -3);
					for (int tails = 0; tails < 8; tails++)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(616 + flyerNumber * 10, 1891, 10, 10), 42241f, 1, 1, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, yPos2), flicker: false, flipped: false, 0.01f, 0f, Color.White * (1f - (float)tails * 0.11f), 4f, 0f, 0f, 0f)
						{
							yPeriodic = true,
							motion = new Vector2(speed, 0f),
							yPeriodicLoopTime = loopTime,
							pulse = pulse,
							pulseTime = 440f,
							pulseAmount = 1.5f,
							yPeriodicRange = loopRange,
							delayBeforeAnimationStart = 14000 + numFlyers * 900 + tails * 100,
							local = true
						});
					}
				}
				for (int numRainbows = 0; numRainbows < 15; numRainbows++)
				{
					int it = 0;
					int yPos3 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width - 128);
					for (int xPos = Game1.graphics.GraphicsDevice.Viewport.Height; xPos >= -64; xPos -= 48)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(yPos3, xPos), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, -(float)Math.PI / 2f, 0f)
						{
							delayBeforeAnimationStart = 27500 + numRainbows * 880 + it * 25,
							local = true
						});
						it++;
					}
				}
				for (int k = 0; k < 120; k++)
				{
					this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(626 + k / 28 * 10, 1891, 10, 10), 2000f, 1, 1, new Vector2(Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width), Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height)), flicker: false, flipped: false, 0.01f, 0f, Color.White, 0.1f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						alphaFade = 0.002f,
						scaleChange = 0.5f,
						scaleChangeChange = -0.0085f,
						delayBeforeAnimationStart = 27500 + k * 110,
						local = true
					});
				}
				break;
			}
			}
			break;
		case 15:
			switch (key[10])
			{
			case 's':
				if (key == "LeoWillyFishing")
				{
					for (int j = 0; j < 20; j++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(0, new Vector2(42.5f, 38f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), Color.White * 0.7f)
						{
							layerDepth = (float)(1280 + j) / 10000f,
							delayBeforeAnimationStart = j * 150
						});
					}
				}
				break;
			case 'o':
				if (key == "LeoLinusCooking")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(240, 128, 16, 16), 9999f, 1, 1, new Vector2(29f, 8.5f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						layerDepth = 1f
					});
					for (int smokePuffs = 0; smokePuffs < 10; smokePuffs++)
					{
						Utility.addSmokePuff(location, new Vector2(29.5f, 8.6f) * 64f, smokePuffs * 500);
					}
				}
				break;
			case 'L':
				if (key == "BoatParrotLeave")
				{
					TemporaryAnimatedSprite temporaryAnimatedSprite = this.aboveMapSprites[0];
					temporaryAnimatedSprite.motion = new Vector2(4f, -6f);
					temporaryAnimatedSprite.sourceRect.X = 48;
					temporaryAnimatedSprite.sourceRectStartingPos.X = 48f;
					temporaryAnimatedSprite.animationLength = 3;
					temporaryAnimatedSprite.pingPong = true;
				}
				break;
			case 'q':
				if (key == "parrotHutSquawk")
				{
					(location as IslandHut).parrotUpgradePerches[0].timeUntilSqwawk = 1f;
				}
				break;
			case 'r':
				if (key == "coldstarMiracle")
				{
					if (!MovieTheater.TryGetMovieData("winter_movie_0", out var data))
					{
						Game1.log.Error("Can't find data for movie 'winter_movie_0'.");
						break;
					}
					Microsoft.Xna.Framework.Rectangle sourceRect = MovieTheater.GetSourceRectForScreen(data.SheetIndex, 9);
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>(data.Texture ?? "LooseSprites\\Movies"),
						sourceRect = sourceRect,
						sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y),
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 99999f,
						alpha = 0.01f,
						alphaFade = -0.01f,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
						layerDepth = 0.8535f,
						id = 989
					});
				}
				break;
			case 'l':
				if (key == "junimoSpotlight")
				{
					this.actors[0].drawOnTop = true;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(316, 123, 67, 43),
						sourceRectStartingPos = new Vector2(316f, 123f),
						animationLength = 1,
						interval = 5000f,
						totalNumberOfLoops = 9999,
						scale = 4f,
						position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 268, 172, 0, -20),
						layerDepth = 0.0001f,
						local = true,
						id = 999
					});
				}
				break;
			case 'e':
				switch (key)
				{
				case "harveyDinnerSet":
				{
					Vector2 centerPoint = new Vector2(5f, 16f);
					if (location is DecoratableLocation decoratableLocation)
					{
						foreach (Furniture f in decoratableLocation.furniture)
						{
							if (f.furniture_type.Value == 14 && !location.hasTileAt((int)f.tileLocation.X, (int)f.tileLocation.Y + 1, "Buildings") && !location.hasTileAt((int)f.tileLocation.X + 1, (int)f.tileLocation.Y + 1, "Buildings") && !location.hasTileAt((int)f.tileLocation.X + 2, (int)f.tileLocation.Y + 1, "Buildings") && !location.hasTileAt((int)f.tileLocation.X - 1, (int)f.tileLocation.Y + 1, "Buildings"))
							{
								centerPoint = new Vector2((int)f.TileLocation.X, (int)f.TileLocation.Y + 1);
								f.isOn.Value = true;
								f.setFireplace(playSound: false);
								break;
							}
						}
					}
					location.TemporarySprites.Clear();
					this.getActorByName("Harvey").setTilePosition((int)centerPoint.X + 2, (int)centerPoint.Y);
					this.getActorByName("Harvey").Position = new Vector2(this.getActorByName("Harvey").Position.X - 32f, this.getActorByName("Harvey").Position.Y);
					this.farmer.Position = new Vector2(centerPoint.X * 64f - 32f, centerPoint.Y * 64f + 32f);
					Object o = location.getObjectAtTile((int)centerPoint.X, (int)centerPoint.Y);
					if (o != null)
					{
						o.isTemporarilyInvisible = true;
					}
					o = location.getObjectAtTile((int)centerPoint.X + 1, (int)centerPoint.Y);
					if (o != null)
					{
						o.isTemporarilyInvisible = true;
					}
					o = location.getObjectAtTile((int)centerPoint.X - 1, (int)centerPoint.Y);
					if (o != null)
					{
						o.isTemporarilyInvisible = true;
					}
					o = location.getObjectAtTile((int)centerPoint.X + 2, (int)centerPoint.Y);
					if (o != null)
					{
						o.isTemporarilyInvisible = true;
					}
					Texture2D tempTxture8 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture8,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(385, 423, 48, 32),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(385f, 423f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = centerPoint * 64f + new Vector2(-8f, -16f) * 4f,
						scale = 4f,
						layerDepth = (centerPoint.Y + 0.2f) * 64f / 10000f,
						lightId = this.GenerateLightSourceId("harveyDinnerSet"),
						lightRadius = 4f,
						lightcolor = Color.Black
					});
					List<string> tmp = this.eventCommands.ToList();
					tmp.Insert(this.CurrentCommand + 1, "viewport " + (int)centerPoint.X + " " + (int)centerPoint.Y + " true");
					this.eventCommands = tmp.ToArray();
					break;
				}
				case "ClothingTherapy":
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1405, 28, 46), 999999f, 1, 99999, new Vector2(5f, 6f) * 64f + new Vector2(-32f, -144f), flicker: false, flipped: false, 0.0424f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
					break;
				case "getEndSlideshow":
				{
					Summit obj = location as Summit;
					string[] s = Event.ParseCommands(obj.getEndSlideshow());
					List<string> commandsList = this.eventCommands.ToList();
					commandsList.InsertRange(this.CurrentCommand + 1, s);
					this.eventCommands = commandsList.ToArray();
					obj.isShowingEndSlideshow = true;
					break;
				}
				}
				break;
			case 'n':
				switch (key)
				{
				case "shaneSaloonCola":
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(552f, 1862f),
						interval = 999999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(32f, 17f) * 64f + new Vector2(10f, 3f) * 4f,
						scale = 4f,
						layerDepth = 1E-07f
					});
					break;
				case "springOnionPeel":
				{
					TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(777);
					temporarySpriteByID2.sourceRectStartingPos = new Vector2(144f, 327f);
					temporarySpriteByID2.sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 327, 112, 112);
					break;
				}
				case "springOnionDemo":
				{
					Texture2D tempTex = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTex,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 215, 112, 112),
						animationLength = 2,
						sourceRectStartingPos = new Vector2(144f, 215f),
						interval = 200f,
						totalNumberOfLoops = 99999,
						id = 777,
						position = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 264, Game1.graphics.GraphicsDevice.Viewport.Height / 3 - 264),
						local = true,
						scale = 4f,
						destroyable = false,
						overrideLocationDestroy = true
					});
					break;
				}
				case "sebastianOnBike":
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 80f, 8, 9999, new Vector2(19f, 27f) * 64f + new Vector2(32f, -16f), flicker: false, flipped: true, 0.1792f, 0f, Color.White, 1f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1854, 47, 33), 9999f, 1, 999, new Vector2(17f, 27f) * 64f + new Vector2(0f, -8f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f));
					break;
				}
				break;
			case 'P':
				if (key == "shaneCliffProps")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(104f, 96f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999
					});
				}
				break;
			case 'm':
				if (key == "grandpaThumbsUp")
				{
					TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(77777);
					temporarySpriteByID.texture = Game1.mouseCursors2;
					temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(186, 265, 22, 34);
					temporarySpriteByID.sourceRectStartingPos = new Vector2(186f, 265f);
					temporarySpriteByID.yPeriodic = true;
					temporarySpriteByID.yPeriodicLoopTime = 1000f;
					temporarySpriteByID.yPeriodicRange = 16f;
					temporarySpriteByID.xPeriodicLoopTime = 2500f;
					temporarySpriteByID.xPeriodicRange = 16f;
					temporarySpriteByID.initialPosition = temporarySpriteByID.position;
				}
				break;
			case 'G':
				if (key == "junimoCageGone2")
				{
					location.removeTemporarySpritesWithID(1);
					Game1.viewportFreeze = true;
					Game1.viewport.X = -1000;
					Game1.viewport.Y = -1000;
				}
				break;
			case 'C':
				if (key == "iceFishingCatch")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(68f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(74f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 490f, 3, 99999, new Vector2(67f, 36f) * 64f, flicker: false, flipped: false, 0.2368f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(76f, 35f) * 64f, flicker: false, flipped: false, 0.2304f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'a':
				if (key == "sebastianGarage")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1843, 48, 42), 9999f, 1, 999, new Vector2(17f, 23f) * 64f + new Vector2(0f, 8f), flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
					this.getActorByName("Sebastian").HideShadow = true;
				}
				break;
			case 'c':
				if (key == "abbyvideoscreen")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 9999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			}
			break;
		case 16:
			switch (key[0])
			{
			case 'B':
				if (key == "BoatParrotSquawk")
				{
					TemporaryAnimatedSprite temporaryAnimatedSprite3 = this.aboveMapSprites[0];
					temporaryAnimatedSprite3.sourceRect.X = 24;
					temporaryAnimatedSprite3.sourceRectStartingPos.X = 24f;
					Game1.playSound("parrot_squawk");
				}
				break;
			case 'i':
				if (key == "islandFishSplash")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 544, 16, 16), 100000f, 1, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 9999,
						motion = new Vector2(-2f, -8f),
						acceleration = new Vector2(0f, 0.2f),
						flipped = true,
						rotationChange = -0.02f,
						yStopCoordinate = 5952,
						layerDepth = 0.99f,
						reachedStopCoordinate = delegate
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, location.getTemporarySpriteByID(9999).position, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								layerDepth = 1f
							});
							location.removeTemporarySpritesWithID(9999);
							Game1.playSound("waterSlosh");
						}
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						layerDepth = 1f
					});
				}
				break;
			case 't':
				if (key == "trashBearPrelude")
				{
					Utility.addStarsAndSpirals(location, 95, 106, 23, 4, 10000, 275, Color.Lime);
				}
				break;
			case 'l':
				if (key == "leahHoldPainting")
				{
					Texture2D tempTxture105 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.getTemporarySpriteByID(999).sourceRect.X += 15;
					location.getTemporarySpriteByID(999).sourceRectStartingPos.X += 15f;
					int whichPainting = ((!Game1.netWorldState.Value.hasWorldStateID("m_painting0")) ? (Game1.netWorldState.Value.hasWorldStateID("m_painting1") ? 1 : 2) : 0);
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture105,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(400 + whichPainting * 25, 394, 25, 23),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(400 + whichPainting * 25, 394f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(73f, 38f) * 64f + new Vector2(-2f, -16f) * 4f,
						scale = 4f,
						layerDepth = 1f,
						id = 777
					});
				}
				break;
			case 'E':
				if (key == "EmilyBoomBoxStop")
				{
					location.getTemporarySpriteByID(999).pulse = false;
					location.getTemporarySpriteByID(999).scale = 4f;
				}
				break;
			case 'w':
				if (key == "wizardSewerMagic")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(15f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("wizardSewerMagic_1"),
						lightRadius = 1f,
						lightcolor = Color.Black,
						alphaFade = 0.005f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(17f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("wizardSewerMagic_2"),
						lightRadius = 1f,
						lightcolor = Color.Black,
						alphaFade = 0.005f
					});
				}
				break;
			case 'm':
				if (key == "moonlightJellies")
				{
					int lightIndex2 = 1;
					this.showGroundObjects = false;
					this.npcControllers?.Clear();
					this.underwaterSprites = new TemporaryAnimatedSpriteList
					{
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(26f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 10000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(29f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(31f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2624,
							delayBeforeAnimationStart = 12000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1728,
							delayBeforeAnimationStart = 14000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1856,
							delayBeforeAnimationStart = 19500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2048,
							delayBeforeAnimationStart = 20300,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 21500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 22400,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(12f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 23200,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(9f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(18f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(21f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2176,
							delayBeforeAnimationStart = 28000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(22f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2304,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 29000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 30000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 32, 16, 16), 250f, 3, 9999, new Vector2(28f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-0.5f, -0.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 4000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 2f,
							xStopCoordinate = 1216,
							yStopCoordinate = 2432,
							delayBeforeAnimationStart = 32000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(40f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 10000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(42f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(43f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2624,
							delayBeforeAnimationStart = 12000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(45f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 14000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(46f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1856,
							delayBeforeAnimationStart = 19500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(48f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 20300,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(49f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 21500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 22400,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2112,
							delayBeforeAnimationStart = 23200,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2432,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(53f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(54f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(55f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(4f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(5f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(3f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2176,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(6f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2368,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(8f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						},
						new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							lightId = this.GenerateLightSourceId($"moonlightJellies_{lightIndex2++}"),
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 29000,
							pingPong = true
						}
					};
				}
				break;
			case 'a':
				if (key == "abbyOuijaCandles")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(5f, 9f) * 64f, flicker: false, flipped: false)
					{
						lightId = this.GenerateLightSourceId("abbyOuijaCandles_1"),
						lightRadius = 1f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(7f, 8f) * 64f, flicker: false, flipped: false)
					{
						lightId = this.GenerateLightSourceId("abbyOuijaCandles_2"),
						lightRadius = 1f
					});
				}
				break;
			}
			break;
		case 10:
			switch (key[6])
			{
			case 'r':
				if (!(key == "BoatParrot"))
				{
					if (!(key == "movieFrame"))
					{
						break;
					}
					if (!ArgUtility.TryGet(args, 2, out var movieId2, out var error7, allowBlank: true, "string movieId") || !ArgUtility.TryGetInt(args, 3, out var frame, out error7, "int frame") || !ArgUtility.TryGetInt(args, 4, out var duration, out error7, "int duration"))
					{
						this.LogCommandError(args, error7);
						break;
					}
					movieId2 = MovieTheater.GetMovieIdFromLegacyIndex(movieId2);
					if (!MovieTheater.TryGetMovieData(movieId2, out var data3))
					{
						this.LogCommandError(args, "no movie found with ID '" + movieId2 + "'");
						break;
					}
					Microsoft.Xna.Framework.Rectangle sourceRect4 = MovieTheater.GetSourceRectForScreen(data3.SheetIndex, frame);
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>(data3.Texture ?? "LooseSprites\\Movies"),
						sourceRect = sourceRect4,
						sourceRectStartingPos = new Vector2(sourceRect4.X, sourceRect4.Y),
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = duration,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
						shakeIntensity = 0.25f,
						layerDepth = 0.0192f,
						id = 997
					});
					break;
				}
				this.aboveMapSprites = new TemporaryAnimatedSpriteList();
				this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(48, 0, 24, 24), 100f, 3, 99999, new Vector2(Game1.viewport.X - 64, 2112f), flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999,
					motion = new Vector2(6f, 1f),
					delayBeforeAnimationStart = 0,
					pingPong = true,
					xStopCoordinate = 1040,
					reachedStopCoordinate = delegate
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite4 = this.aboveMapSprites[0];
						if (temporaryAnimatedSprite4 != null)
						{
							temporaryAnimatedSprite4.motion = new Vector2(0f, 2f);
							temporaryAnimatedSprite4.yStopCoordinate = 2336;
							temporaryAnimatedSprite4.reachedStopCoordinate = delegate
							{
								TemporaryAnimatedSprite temporaryAnimatedSprite5 = this.aboveMapSprites[0];
								temporaryAnimatedSprite5.animationLength = 1;
								temporaryAnimatedSprite5.pingPong = false;
								temporaryAnimatedSprite5.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24);
								temporaryAnimatedSprite5.sourceRectStartingPos = Vector2.Zero;
							};
						}
					}
				});
				break;
			case 'b':
				if (key == "evilRabbit")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(264, 209, 19, 16),
						sourceRectStartingPos = new Vector2(264f, 209f),
						animationLength = 1,
						totalNumberOfLoops = 999,
						interval = 999f,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(38f, 23f) * 4f,
						layerDepth = 1f,
						motion = new Vector2(-2f, -2f),
						acceleration = new Vector2(0f, 0.1f),
						yStopCoordinate = 204,
						xStopCoordinate = 316,
						flipped = true,
						id = 778
					});
				}
				break;
			case 'S':
				if (key == "junimoShow")
				{
					Texture2D tempTxture104 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture104,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 350, 19, 14),
						animationLength = 6,
						sourceRectStartingPos = new Vector2(393f, 350f),
						interval = 90f,
						totalNumberOfLoops = 86,
						position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
						scale = 4f,
						layerDepth = 0.95f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture104,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 364, 19, 14),
						animationLength = 4,
						sourceRectStartingPos = new Vector2(393f, 364f),
						interval = 90f,
						totalNumberOfLoops = 31,
						position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
						scale = 4f,
						layerDepth = 0.97f,
						delayBeforeAnimationStart = 11034
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture104,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 378, 19, 14),
						animationLength = 6,
						sourceRectStartingPos = new Vector2(393f, 378f),
						interval = 90f,
						totalNumberOfLoops = 21,
						position = new Vector2(52f, 24f) * 64f + new Vector2(7f, -2f) * 4f,
						scale = 4f,
						layerDepth = 1f,
						delayBeforeAnimationStart = 22069
					});
				}
				break;
			case 'o':
				if (!(key == "luauShorts"))
				{
					if (key == "linusMoney")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1002f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 10,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1003f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 100,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-999f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 200,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1004f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 300,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1001f, -998f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 400,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -999f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 500,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 600,
							overrideLocationDestroy = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-997f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							startSound = "money",
							delayBeforeAnimationStart = 700,
							overrideLocationDestroy = true
						});
					}
				}
				else
				{
					Vector2 shortsSpot = ((Game1.year % 2 == 0) ? new Vector2(24f, 10f) : new Vector2(35f, 10f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 512, 16, 16), 9999f, 1, 99999, shortsSpot * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, -8f),
						acceleration = new Vector2(0f, 0.25f),
						yStopCoordinate = ((int)shortsSpot.Y + 1) * 64,
						xStopCoordinate = ((int)shortsSpot.X - 2) * 64
					});
				}
				break;
			case 'B':
			{
				if (!(key == "arcaneBook"))
				{
					if (key == "candleBoat")
					{
						this.showGroundObjects = false;
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(240, 112, 16, 32), 1000f, 2, 99999, new Vector2(22f, 36f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1,
							lightId = this.GenerateLightSourceId("candleBoat"),
							lightRadius = 2f,
							lightcolor = Color.Black
						});
					}
					break;
				}
				for (int i24 = 0; i24 < 16; i24++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(128f, 792f) + new Vector2(Game1.random.Next(32), Game1.random.Next(32) - i24 * 4), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 1f,
						scale = 4f,
						alphaFade = 0.008f,
						motion = new Vector2(0f, -0.5f)
					});
				}
				this.aboveMapSprites = new TemporaryAnimatedSpriteList
				{
					new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 18), new Vector2(160f, 800f), flipped: false, 0f, Color.White)
					{
						interval = 25f,
						totalNumberOfLoops = 99999,
						animationLength = 3,
						layerDepth = 1f,
						scale = 1f,
						scaleChange = 1f,
						scaleChangeChange = -0.05f,
						alpha = 0.65f,
						alphaFade = 0.005f,
						motion = new Vector2(-8f, -8f),
						acceleration = new Vector2(0.4f, 0.4f)
					}
				};
				for (int i25 = 0; i25 < 16; i25++)
				{
					this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 12f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i25 * 25
					});
				}
				location.setMapTile(2, 12, 2143, "Front", "untitled tile sheet");
				break;
			}
			case 'G':
				if (!(key == "parrotGone"))
				{
					if (key == "secretGift")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), new Vector2(30f, 70f) * 64f + new Vector2(0f, -21f), flipped: false, 0f, Color.White)
						{
							animationLength = 1,
							interval = 999999f,
							id = 666,
							scale = 4f
						});
					}
				}
				else
				{
					location.removeTemporarySpritesWithID(666);
				}
				break;
			case 'h':
				if (key == "waterShane")
				{
					this.drawTool = true;
					this.farmer.TemporaryItem = ItemRegistry.Create("(T)WateringCan");
					this.farmer.CurrentTool.Update(1, 0, this.farmer);
					this.farmer.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
					{
						new FarmerSprite.AnimationFrame(58, 0, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(58, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect),
						new FarmerSprite.AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true),
						new FarmerSprite.AnimationFrame(45, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
					});
				}
				break;
			case 'W':
				if (key == "wizardWarp")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(8f, 16f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(2f, -2f),
						acceleration = new Vector2(0.1f, 0f),
						scaleChange = -0.02f,
						alphaFade = 0.001f
					});
				}
				break;
			case 'l':
				if (key == "witchFlyby")
				{
					Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-4f, 0f),
						acceleration = new Vector2(-0.025f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 64f,
						local = true
					});
				}
				break;
			case 'C':
				if (key == "junimoCage")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 19), 60f, 3, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("junimoCage_1"),
						lightRadius = 1f,
						lightcolor = Color.Black,
						id = 1,
						shakeIntensity = 0f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("junimoCage_2"),
						lightRadius = 0.5f,
						lightcolor = Color.Black,
						id = 1,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 24f,
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 24f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("junimoCage_3"),
						lightRadius = 0.5f,
						lightcolor = Color.Black,
						id = 1,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = -24f,
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 24f,
						delayBeforeAnimationStart = 250
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("junimoCage_3"),
						lightRadius = 0.5f,
						lightcolor = Color.Black,
						id = 1,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = -24f,
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 24f,
						delayBeforeAnimationStart = 450
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId("junimoCage_4"),
						lightRadius = 0.5f,
						lightcolor = Color.Black,
						id = 1,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 24f,
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 24f,
						delayBeforeAnimationStart = 650
					});
				}
				break;
			case 'n':
				if (key == "joshDinner")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(649, 9999f, 1, 9999, new Vector2(6f, 4f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false)
					{
						layerDepth = 0.0256f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(664, 9999f, 1, 9999, new Vector2(8f, 4f) * 64f + new Vector2(-8f, 32f), flicker: false, flipped: false)
					{
						layerDepth = 0.0256f
					});
				}
				break;
			case 't':
				if (key == "beachStuff")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1887, 47, 29), 9999f, 1, 999, new Vector2(44f, 21f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'p':
				if (key == "leahLaptop")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(130, 1849, 19, 19), 9999f, 1, 999, new Vector2(12f, 10f) * 64f + new Vector2(0f, 24f), flicker: false, flipped: false, 0.1856f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'c':
				if (key == "leahPicnic")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(96, 1808, 32, 48), 9999f, 1, 999, new Vector2(75f, 37f) * 64f, flicker: false, flipped: false, 0.2496f, 0f, Color.White, 4f, 0f, 0f, 0f));
					NPC n = new NPC(new AnimatedSprite(this.festivalContent, "Characters\\" + (this.farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(-100f, -100f) * 64f, 2, "LeahEx");
					n.AllowDynamicAppearance = false;
					this.actors.Add(n);
				}
				break;
			case 'a':
				if (key == "maruBeaker")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 1380f, 1, 0, new Vector2(9f, 14f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
					{
						rotationChange = (float)Math.PI / 24f,
						motion = new Vector2(0f, -7f),
						acceleration = new Vector2(0f, 0.2f),
						endFunction = beakerSmashEndFunction,
						layerDepth = 1f
					});
				}
				break;
			case 'e':
				if (key == "abbyOneBat")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
					{
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 128f,
						motion = new Vector2(0f, -8f)
					});
				}
				break;
			case 'w':
				if (key == "swordswipe")
				{
					if (!ArgUtility.TryGetVector2(args, 2, out var position4, out var error6, integerOnly: true, "Vector2 position"))
					{
						this.LogCommandError(args, error6);
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, position4 * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
					}
				}
				break;
			case 'L':
				if (key == "abbyAtLake")
				{
					int lightIndex = 1;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(735, 999999f, 1, 0, new Vector2(48f, 30f) * 64f, flicker: false, flipped: false)
					{
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 2f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2000f,
						yPeriodicLoopTime = 1600f,
						xPeriodicRange = 32f,
						yPeriodicRange = 21f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 1000f,
						yPeriodicLoopTime = 1600f,
						xPeriodicRange = 16f,
						yPeriodicRange = 21f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2400f,
						yPeriodicLoopTime = 2800f,
						xPeriodicRange = 21f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2000f,
						yPeriodicLoopTime = 2400f,
						xPeriodicRange = 16f,
						yPeriodicRange = 16f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2000f,
						yPeriodicLoopTime = 2600f,
						xPeriodicRange = 21f,
						yPeriodicRange = 48f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2000f,
						yPeriodicLoopTime = 2600f,
						xPeriodicRange = 32f,
						yPeriodicRange = 21f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 4000f,
						yPeriodicLoopTime = 5000f,
						xPeriodicRange = 42f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 4000f,
						yPeriodicLoopTime = 5500f,
						xPeriodicRange = 32f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2400f,
						yPeriodicLoopTime = 3600f,
						xPeriodicRange = 32f,
						yPeriodicRange = 21f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2500f,
						yPeriodicLoopTime = 3600f,
						xPeriodicRange = 42f,
						yPeriodicRange = 51f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 4500f,
						yPeriodicLoopTime = 3000f,
						xPeriodicRange = 21f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 5000f,
						yPeriodicLoopTime = 4500f,
						xPeriodicRange = 64f,
						yPeriodicRange = 48f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2000f,
						yPeriodicLoopTime = 3000f,
						xPeriodicRange = 32f,
						yPeriodicRange = 21f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 2900f,
						yPeriodicLoopTime = 3200f,
						xPeriodicRange = 21f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 4200f,
						yPeriodicLoopTime = 3300f,
						xPeriodicRange = 16f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						lightcolor = Color.Orange,
						lightId = this.GenerateLightSourceId($"abbyAtLake_{lightIndex++}"),
						lightRadius = 0.2f,
						xPeriodic = true,
						yPeriodic = true,
						xPeriodicLoopTime = 5100f,
						yPeriodicLoopTime = 4000f,
						xPeriodicRange = 32f,
						yPeriodicRange = 16f
					});
				}
				break;
			}
			break;
		case 12:
			switch (key[4])
			{
			case 'i':
			{
				string textureName;
				string error5;
				Microsoft.Xna.Framework.Rectangle sourceRect3;
				Vector2 tile2;
				int id;
				float layerDepth;
				if (!(key == "staticSprite"))
				{
					if (key == "morrisFlying")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(105, 1318, 13, 31), 9999f, 1, 99999, new Vector2(32f, 13f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(4f, -8f),
							rotationChange = (float)Math.PI / 16f,
							shakeIntensity = 1f
						});
					}
				}
				else if (!ArgUtility.TryGet(args, 2, out textureName, out error5, allowBlank: true, "string textureName") || !ArgUtility.TryGetRectangle(args, 3, out sourceRect3, out error5, "Rectangle sourceRect") || !ArgUtility.TryGetVector2(args, 7, out tile2, out error5, integerOnly: false, "Vector2 tile") || !ArgUtility.TryGetOptionalInt(args, 9, out id, out error5, 999, "int id") || !ArgUtility.TryGetOptionalFloat(args, 10, out layerDepth, out error5, 1f, "float layerDepth"))
				{
					this.LogCommandError(args, error5);
				}
				else
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect3, tile2 * 64f, flipped: false, 0f, Color.White)
					{
						animationLength = 1,
						interval = 999999f,
						scale = 4f,
						layerDepth = layerDepth,
						id = id
					});
				}
				break;
			}
			case 'v':
				if (key == "removeSprite")
				{
					if (!ArgUtility.TryGetInt(args, 2, out var spriteId, out var error3, "int spriteId"))
					{
						this.LogCommandError(args, error3);
					}
					else
					{
						location.removeTemporarySpritesWithID(spriteId);
					}
				}
				break;
			case 'y':
				if (!(key == "EmilyCamping"))
				{
					if (key == "EmilyBoomBox")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(586, 1871, 24, 14), 99999f, 1, 99999, new Vector2(15f, 4f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999
						});
					}
					break;
				}
				this.showGroundObjects = false;
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1578, 59, 53), 999999f, 1, 99999, new Vector2(26f, 9f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0788f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(675, 1299, 29, 24), 999999f, 1, 99999, new Vector2(27f, 14f) * 64f, flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 99
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(27f, 14f) * 64f + new Vector2(8f, 4f) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = this.GenerateLightSourceId("EmilyCamping_1"),
					id = 666,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.01f
				});
				Game1.currentLightSources.Add(new LightSource(this.GenerateLightSourceId("EmilyCamping_2"), 4, new Vector2(27f, 14f) * 64f, 2f, LightSource.LightContext.None, 0L));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(585, 1846, 26, 22), 999999f, 1, 99999, new Vector2(25f, 12f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 96
				});
				AmbientLocationSounds.addSound(new Vector2(27f, 14f), 1);
				break;
			case 'a':
				if (key == "curtainClose")
				{
					location.getTemporarySpriteByID(999).sourceRect.X = 644;
					Game1.playSound("shwip");
				}
				break;
			case 'd':
				if (key == "grandpaNight")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 1f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.Cyan, 4f, 0f, 0f, 0f, local: true)
					{
						alpha = 0.01f,
						alphaFade = -0.002f,
						local = true
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 768f), flicker: false, flipped: true, 0.9f, 0f, Color.Blue, 4f, 0f, 0f, 0f, local: true)
					{
						alpha = 0.01f,
						alphaFade = -0.002f,
						local = true
					});
				}
				break;
			case 'C':
				if (key == "jojaCeremony")
				{
					this.aboveMapSprites = new TemporaryAnimatedSpriteList();
					for (int i23 = 0; i23 < 16; i23++)
					{
						Vector2 position3 = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + i23 * 64);
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position3, flicker: false, flipped: false, 0.99f, 0f, Color.DeepSkyBlue, 4f, 0f, 0f, 0f)
						{
							local = true,
							motion = new Vector2(0.25f, -1.5f),
							acceleration = new Vector2(0f, -0.001f),
							id = 79797 + i23
						});
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position3 + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							local = true,
							motion = new Vector2(0.25f, -1.5f),
							acceleration = new Vector2(0f, -0.001f),
							id = 79797 + i23
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1363, 114, 58), 99999f, 1, 99999, new Vector2(50f, 20f) * 64f, flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(48f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						pingPong = true
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(49f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						pingPong = true
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 210f, 3, 99999, new Vector2(62f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						pingPong = true
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 190f, 3, 99999, new Vector2(60f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						pingPong = true
					});
				}
				break;
			case 'F':
				if (key == "joshFootball")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1916, 14, 8), 40f, 6, 9999, new Vector2(25f, 16f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						rotation = -(float)Math.PI / 4f,
						rotationChange = (float)Math.PI / 200f,
						motion = new Vector2(6f, -4f),
						acceleration = new Vector2(0f, 0.2f),
						xStopCoordinate = 1856,
						reachedStopCoordinate = catchFootball,
						layerDepth = 1f,
						id = 56232
					});
				}
				break;
			case 'o':
				if (key == "balloonBirds")
				{
					if (!ArgUtility.TryGetOptionalInt(args, 2, out var positionOffset, out var error4, 0, "int positionOffset"))
					{
						this.LogCommandError(args, error4);
						break;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 12) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1500
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 13) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1250
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 14) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1100
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, positionOffset + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1000
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 16) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1080
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 17) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1300
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 18) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = 1450
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-4f, 0f),
						delayBeforeAnimationStart = 5450
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 10) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 500
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 11) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 250
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 12) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 100
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, positionOffset + 13) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f)
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 14) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 80
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 15) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 300
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 16) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						delayBeforeAnimationStart = 450
					});
				}
				break;
			case 'e':
				if (key == "marcelloLand")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, (new Vector2(25f, 19f) + this.eventPositionTileOffset) * 64f + new Vector2(-23f, 0f) * 4f, flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, 2f),
						yStopCoordinate = (41 + (int)this.eventPositionTileOffset.Y) * 64 - 640,
						reachedStopCoordinate = marcelloBalloonLand,
						attachedCharacter = this.getActorByName("Marcello"),
						id = 1
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, (new Vector2(25f, 19f) + this.eventPositionTileOffset) * 64f + new Vector2(0f, 134f) * 4f, flicker: false, flipped: false, (41f + this.eventPositionTileOffset.Y) * 64f / 10000f + 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, 2f),
						id = 2
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(24, 1343, 36, 19), 7000f, 1, 99999, (new Vector2(25f, 40f) + this.eventPositionTileOffset) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 0f, 0f, 0f, 0f)
					{
						scaleChange = 0.01f,
						id = 3
					});
				}
				break;
			case 'T':
				if (key == "maruTrapdoor")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1632, 16, 32), 150f, 4, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(688, 1632, 16, 32), 99999f, 1, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 500
					});
				}
				break;
			case 'M':
				if (key == "abbyManyBats")
				{
					for (int i21 = 0; i21 < 100; i21++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = Game1.random.Next(1500, 2500),
							xPeriodicRange = Game1.random.Next(64, 192),
							motion = new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-8, -4)),
							delayBeforeAnimationStart = i21 * 30,
							startSound = ((i21 % 10 == 0 || Game1.random.NextDouble() < 0.1) ? "batScreech" : null)
						});
					}
					for (int i22 = 0; i22 < 100; i22++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-8, -4)),
							delayBeforeAnimationStart = 10 + i22 * 30
						});
					}
				}
				break;
			}
			break;
		case 8:
			switch (key[4])
			{
			case 'y':
				if (key == "WillyWad")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors2"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(192, 61, 32, 32),
						sourceRectStartingPos = new Vector2(192f, 61f),
						animationLength = 2,
						totalNumberOfLoops = 99999,
						interval = 400f,
						scale = 4f,
						position = new Vector2(50f, 23f) * 64f,
						layerDepth = 0.1536f,
						id = 996
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3328f, 1728f), Color.White, 10, flipped: false, 80f, 999999));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3264f, 1792f), Color.White, 10, flipped: false, 70f, 999999));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3392f, 1792f), Color.White, 10, flipped: false, 85f, 999999));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(53f, 24f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(54f, 23f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'J':
				if (key == "frogJump")
				{
					TemporaryAnimatedSprite temporarySpriteByID6 = location.getTemporarySpriteByID(777);
					temporarySpriteByID6.motion = new Vector2(-2f, 0f);
					temporarySpriteByID6.animationLength = 4;
					temporarySpriteByID6.interval = 150f;
				}
				break;
			case 'm':
				if (key == "golemDie")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(40f, 11f) * 64f, Color.DarkGray, 10));
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, new Vector2(40f, 11f) * 64f, Color.LimeGreen, 10), location, 2);
					Texture2D tempTxture103 = Game1.temporaryContent.Load<Texture2D>("Characters\\Monsters\\Wilderness Golem");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture103,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 0f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(40f, 11f) * 64f + new Vector2(2f, -8f) * 4f,
						scale = 4f,
						layerDepth = 0.01f,
						rotation = (float)Math.PI / 2f,
						motion = new Vector2(0f, 4f),
						yStopCoordinate = 832
					});
				}
				break;
			case 'o':
				if (key == "parrots1")
				{
					this.aboveMapSprites = new TemporaryAnimatedSpriteList
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 256f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 0,
							local = true
						},
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 600,
							local = true
						},
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 320f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 1200,
							local = true
						}
					};
				}
				break;
			case 'e':
				if (key == "umbrella")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1843, 27, 23), 80f, 3, 9999, new Vector2(12f, 39f) * 64f + new Vector2(-20f, -104f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'S':
				if (key == "leahShow")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(29f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(112, 656, 16, 64), 9999f, 1, 999, new Vector2(29f, 56f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(128, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 58f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(160, 656, 32, 64), 9999f, 1, 999, new Vector2(29f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(34f, 63f) * 64f, flicker: false, flipped: false, 0.4031f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(113, 592, 16, 64), 100f, 4, 99999, new Vector2(34f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
					NPC n = new NPC(new AnimatedSprite(this.festivalContent, "Characters\\" + (this.farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(46f, 57f) * 64f, 2, "LeahEx");
					n.AllowDynamicAppearance = false;
					this.actors.Add(n);
				}
				break;
			case 'T':
				if (key == "leahTree")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(744, 999999f, 1, 0, new Vector2(42f, 8f) * 64f, flicker: false, flipped: false));
				}
				break;
			}
			break;
		case 7:
			switch (key[1])
			{
			case 'u':
				if (key == "sunroom")
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(304, 486, 24, 26),
						sourceRectStartingPos = new Vector2(304f, 486f),
						animationLength = 1,
						totalNumberOfLoops = 997,
						interval = 99999f,
						scale = 4f,
						position = new Vector2(4f, 8f) * 64f + new Vector2(8f, -8f) * 4f,
						layerDepth = 0.0512f,
						id = 996
					});
					location.addCritter(new Butterfly(location, location.getRandomTile()).setStayInbounds(stayInbounds: true));
					while (Game1.random.NextBool())
					{
						location.addCritter(new Butterfly(location, location.getRandomTile()).setStayInbounds(stayInbounds: true));
					}
				}
				break;
			case 'a':
				if (key == "jasGift")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(22f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999,
						paused = true,
						holdLastFrame = true
					});
				}
				break;
			case 'e':
				if (key == "wedding")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1196, 98, 54), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1250, 98, 25), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, 54f) * 4f + new Vector2(0f, -64f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'i':
				if (key == "dickBag")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(528, 1435, 16, 16), 99999f, 1, 99999, new Vector2(48f, 7f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'o':
			{
				if (!(key == "JoshMom"))
				{
					if (key == "joshDog")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1916, 12, 20), 500f, 6, 9999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1
						});
					}
					break;
				}
				TemporaryAnimatedSprite parent3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(416, 1931, 58, 65), 750f, 2, 99999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					alpha = 0.6f,
					local = true,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -1.25f),
					initialPosition = new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height)
				};
				location.temporarySprites.Add(parent3);
				for (int i20 = 0; i20 < 19; i20++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(516, 1916, 7, 10), 99999f, 1, 99999, new Vector2(64f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						alphaFade = 0.01f,
						local = true,
						motion = new Vector2(-1f, -1f),
						parentSprite = parent3,
						delayBeforeAnimationStart = (i20 + 1) * 1000
					});
				}
				break;
			}
			case 'r':
				if (key == "dropEgg")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(176, 800f, 1, 0, new Vector2(6f, 4f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
					{
						rotationChange = (float)Math.PI / 24f,
						motion = new Vector2(0f, -7f),
						acceleration = new Vector2(0f, 0.3f),
						endFunction = eggSmashEndFunction,
						layerDepth = 1f
					});
				}
				break;
			}
			break;
		case 9:
			switch (key[5])
			{
			case 'G':
				if (key == "sauceGood")
				{
					Utility.addSprinklesToLocation(location, this.OffsetTileX(64), this.OffsetTileY(16), 3, 1, 800, 200, Color.White);
				}
				break;
			case 'F':
				if (key == "sauceFire")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
						animationLength = 4,
						sourceRectStartingPos = new Vector2(276f, 1985f),
						interval = 100f,
						totalNumberOfLoops = 5,
						position = this.OffsetPosition(new Vector2(64f, 16f) * 64f + new Vector2(3f, -4f) * 4f),
						scale = 4f,
						layerDepth = 1f
					});
					this.aboveMapSprites = new TemporaryAnimatedSpriteList();
					for (int i19 = 0; i19 < 8; i19++)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), this.OffsetPosition(new Vector2(64f, 16f) * 64f) + new Vector2(Game1.random.Next(-16, 32), 0f), flipped: false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
							interval = 99999f,
							layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i19 * 25
						});
					}
				}
				break;
			case 'B':
				if (!(key == "shakeBush"))
				{
					if (key == "movieBush")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\bushes"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(65, 58, 30, 35),
							sourceRectStartingPos = new Vector2(65f, 58f),
							animationLength = 1,
							totalNumberOfLoops = 999,
							interval = 999f,
							scale = 4f,
							position = new Vector2(4f, 1f) * 64f + new Vector2(33f, 13f) * 4f,
							layerDepth = 0.99f,
							id = 777
						});
					}
				}
				else
				{
					location.getTemporarySpriteByID(777).shakeIntensity = 1f;
				}
				break;
			case 'T':
				if (key == "shakeTent")
				{
					location.getTemporarySpriteByID(999).shakeIntensity = 1f;
				}
				break;
			case 'S':
			{
				if (!(key == "EmilySign"))
				{
					break;
				}
				this.aboveMapSprites = new TemporaryAnimatedSpriteList();
				for (int numRainbows2 = 0; numRainbows2 < 10; numRainbows2++)
				{
					int iter = 0;
					int yPos4 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height - 128);
					for (int xPos2 = Game1.graphics.GraphicsDevice.Viewport.Width; xPos2 >= -64; xPos2 -= 48)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(xPos2, yPos4), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = numRainbows2 * 600 + iter * 25,
							startSound = ((iter == 0) ? "dwoop" : null),
							local = true
						});
						iter++;
					}
				}
				break;
			}
			case 't':
				if (key == "joshSteak")
				{
					location.temporarySprites.Clear();
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 1
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), 999f, 1, 9999, new Vector2(50f, 68f) * 64f + new Vector2(32f, -8f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			case 'a':
				if (key == "samSkate1")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0), 9999f, 1, 999, new Vector2(12f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(4f, 0f),
						acceleration = new Vector2(-0.008f, 0f),
						xStopCoordinate = 1344,
						reachedStopCoordinate = samPreOllie,
						attachedCharacter = this.getActorByName("Sam"),
						id = 92473
					});
				}
				break;
			case 'C':
				if (key == "pennyCook")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
					{
						layerDepth = 1f,
						animationLength = 6,
						interval = 75f,
						motion = new Vector2(0f, -0.5f)
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(16f, 0f), flipped: false, 0f, Color.White)
					{
						layerDepth = 0.1f,
						animationLength = 6,
						interval = 75f,
						motion = new Vector2(0f, -0.5f),
						delayBeforeAnimationStart = 500
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(-16f, 0f), flipped: false, 0f, Color.White)
					{
						layerDepth = 1f,
						animationLength = 6,
						interval = 75f,
						motion = new Vector2(0f, -0.5f),
						delayBeforeAnimationStart = 750
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
					{
						layerDepth = 0.1f,
						animationLength = 6,
						interval = 75f,
						motion = new Vector2(0f, -0.5f),
						delayBeforeAnimationStart = 1000
					});
				}
				break;
			case 'M':
				if (key == "pennyMess")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(739, 999999f, 1, 0, new Vector2(10f, 5f) * 64f, flicker: false, flipped: false));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(740, 999999f, 1, 0, new Vector2(15f, 5f) * 64f, flicker: false, flipped: false));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(741, 999999f, 1, 0, new Vector2(16f, 6f) * 64f, flicker: false, flipped: false));
				}
				break;
			case 'u':
				if (key == "abbyOuija")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(6f, 9f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
				}
				break;
			}
			break;
		case 17:
			switch (key[0])
			{
			case 'l':
				if (key == "leahPaintingSetup")
				{
					Texture2D tempTxture102 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture102,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(368f, 393f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(72f, 38f) * 64f + new Vector2(3f, -13f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 999
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture102,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(368f, 393f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(74f, 40f) * 64f + new Vector2(3f, -17f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 888
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture102,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(369, 424, 11, 15),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(369f, 424f),
						interval = 9999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(75f, 40f) * 64f + new Vector2(-2f, -11f) * 4f,
						scale = 4f,
						layerDepth = 0.01f,
						id = 444
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 1822, 32, 34),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(96f, 1822f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(79f, 36f) * 64f,
						scale = 4f,
						layerDepth = 0.1f
					});
				}
				break;
			case 's':
				if (key == "springOnionRemove")
				{
					location.removeTemporarySpritesWithID(777);
				}
				break;
			case 'E':
				if (key == "EmilyBoomBoxStart")
				{
					location.getTemporarySpriteByID(999).pulse = true;
					location.getTemporarySpriteByID(999).pulseTime = 420f;
				}
				break;
			case 'd':
				if (key == "doneWithSlideShow")
				{
					(location as Summit).isShowingEndSlideshow = false;
				}
				break;
			case 'm':
				if (key == "maruElectrocution")
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1664, 16, 32), 40f, 1, 20, new Vector2(7f, 5f) * 64f - new Vector2(-4f, 8f), flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				}
				break;
			}
			break;
		case 5:
			switch (key[0])
			{
			case 's':
				if (key == "samTV")
				{
					Texture2D tempTxture101 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture101,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 350, 25, 29),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(368f, 350f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(52f, 24f) * 64f + new Vector2(4f, -12f) * 4f,
						scale = 4f,
						layerDepth = 0.9f
					});
				}
				break;
			case 'h':
				if (key == "heart")
				{
					if (!ArgUtility.TryGetVector2(args, 2, out var tile, out var error2, integerOnly: true, "Vector2 tile"))
					{
						this.LogCommandError(args, error2);
						break;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, this.OffsetPosition(tile) * 64f + new Vector2(-16f, -16f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -0.5f),
						alphaFade = 0.01f
					});
				}
				break;
			case 'r':
				if (key == "robot")
				{
					TemporaryAnimatedSprite parent2 = new TemporaryAnimatedSprite(this.getActorByName("robot").Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(35, 42, 35, 42), 50f, 1, 9999, new Vector2(13f, 27f) * 64f - new Vector2(0f, 32f), flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						acceleration = new Vector2(0f, -0.01f),
						accelerationChange = new Vector2(0f, -0.0001f)
					};
					location.temporarySprites.Add(parent2);
					for (int i18 = 0; i18 < 420; i18++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 64, 320, 64, 64), new Vector2(Game1.random.Next(96), 136f), flipped: false, 0.01f, Color.White * 0.75f)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = i18 * 10,
							animationLength = 1,
							currentNumberOfLoops = 0,
							interval = 9999f,
							motion = new Vector2(Game1.random.Next(-100, 100) / (i18 + 20), 0.25f + (float)i18 / 100f),
							parentSprite = parent2
						});
					}
				}
				break;
			}
			break;
		case 20:
			if (key == "BoatParrotSquawkStop")
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite2 = this.aboveMapSprites[0];
				temporaryAnimatedSprite2.sourceRect.X = 0;
				temporaryAnimatedSprite2.sourceRectStartingPos.X = 0f;
			}
			break;
		case 23:
			if (key == "leahStopHoldingPainting")
			{
				location.getTemporarySpriteByID(999).sourceRect.X -= 15;
				location.getTemporarySpriteByID(999).sourceRectStartingPos.X -= 15f;
				location.removeTemporarySpritesWithIDLocal(777);
				Game1.playSound("thudStep");
			}
			break;
		case 6:
			if (key == "qiCave")
			{
				Texture2D tempTxt = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(415, 216, 96, 89),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(415f, 216f),
					interval = 999999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(2f, 2f) * 64f + new Vector2(112f, 25f) * 4f,
					scale = 4f,
					layerDepth = 1E-07f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(370, 272, 107, 64),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(370f, 216f),
					interval = 999999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(2f, 2f) * 64f + new Vector2(67f, 81f) * 4f,
					scale = 4f,
					layerDepth = 1.1E-07f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.objectSpriteSheet,
					sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16),
					sourceRectStartingPos = new Vector2(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).X, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).Y),
					animationLength = 1,
					interval = 999999f,
					id = 803,
					totalNumberOfLoops = 99999,
					position = new Vector2(13f, 7f) * 64f + new Vector2(1f, 9f) * 4f,
					scale = 4f,
					layerDepth = 2.1E-06f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(8f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 90f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(5f, 7f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 120f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(7f, 10f) * 64f,
					scale = 4f,
					layerDepth = 1f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 80f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(15f, 7f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(12f, 11f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 105f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(16f, 10f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 85f,
					totalNumberOfLoops = 99999,
					id = 11,
					position = new Vector2(3f, 9f) * 64f,
					scale = 4f
				});
			}
			break;
		case 3:
			if (key == "wed")
			{
				this.aboveMapSprites = new TemporaryAnimatedSpriteList();
				Game1.flashAlpha = 1f;
				for (int i = 0; i < 150; i++)
				{
					Vector2 position = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.random.Next(Game1.viewport.Height));
					int scale = Game1.random.Next(2, 5);
					this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(424, 1266, 8, 8), 60f + (float)Game1.random.Next(-10, 10), 7, 999999, position, flicker: false, flipped: false, 0.99f, 0f, Color.White, scale, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.1625f, -0.25f) * scale
					});
				}
				Game1.changeMusicTrack("wedding", track_interruptable: false, MusicContext.Event);
				Game1.musicPlayerVolume = 0f;
			}
			break;
		case 4:
		case 21:
		case 22:
			break;
		}
	}

	private Microsoft.Xna.Framework.Rectangle skipBounds()
	{
		int scale = 4;
		int width = 22 * scale;
		Microsoft.Xna.Framework.Rectangle skipBounds = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - width - 8, Game1.viewport.Height - 64, width, 15 * scale);
		Utility.makeSafe(ref skipBounds);
		return skipBounds;
	}

	public void receiveMouseClick(int x, int y)
	{
		if (!Game1.options.SnappyMenus && !this.skipped && this.skippable && this.skipBounds().Contains(x, y))
		{
			this.skipped = true;
			this.skipEvent();
			Game1.freezeControls = false;
		}
		this.popBalloons(x, y);
	}

	public void skipEvent()
	{
		if (this.playerControlSequence)
		{
			this.EndPlayerControlSequence();
		}
		Game1.playSound("drumkit6");
		this.actorPositionsAfterMove.Clear();
		foreach (NPC n in this.actors)
		{
			bool ignore_stop_animation = n.Sprite.ignoreStopAnimation;
			n.Sprite.ignoreStopAnimation = true;
			n.Halt();
			n.Sprite.ignoreStopAnimation = ignore_stop_animation;
			this.resetDialogueIfNecessary(n);
		}
		this.farmer.Halt();
		this.farmer.ignoreCollisions = false;
		Game1.exitActiveMenu();
		Game1.fadeClear();
		Game1.dialogueUp = false;
		Game1.dialogueTyping = false;
		Game1.pauseTime = 0f;
		string[] array = this.actionsOnSkip;
		if (array != null && array.Length != 0)
		{
			string[] array2 = this.actionsOnSkip;
			foreach (string action in array2)
			{
				if (!TriggerActionManager.TryRunAction(action, out var error, out var ex))
				{
					Game1.log.Error($"Event '{this.id}' failed applying post-skip action '{action}': {error}.", ex);
				}
			}
			Game1.log.Verbose($"Event '{this.id}' applied post-skip actions [{string.Join(", ", this.actionsOnSkip)}].");
		}
		switch (this.id)
		{
		case "33":
			Game1.player.craftingRecipes.TryAdd("Drum Block", 0);
			Game1.player.craftingRecipes.TryAdd("Flute Block", 0);
			this.endBehaviors();
			break;
		case "897405":
		case "1590166":
			if (!this.gotPet)
			{
				string defaultName = ((!Game1.player.IsMale) ? Game1.content.LoadString((Game1.player.whichPetType == "Dog") ? "Strings\\StringsFromCSFiles:Event.cs.1797" : "Strings\\StringsFromCSFiles:Event.cs.1796") : Game1.content.LoadString(Game1.player.catPerson ? "Strings\\StringsFromCSFiles:Event.cs.1794" : "Strings\\StringsFromCSFiles:Event.cs.1795"));
				this.namePet(defaultName);
			}
			this.endBehaviors();
			break;
		case "980559":
			if (Game1.player.GetSkillLevel(1) < 1)
			{
				Game1.player.setSkillLevel("Fishing", 1);
			}
			if (!Game1.player.Items.ContainsId("(T)TrainingRod"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)TrainingRod"));
			}
			this.endBehaviors();
			break;
		case "-157039427":
			this.endBehaviors(new string[2] { "End", "islandDepart" }, Game1.currentLocation);
			break;
		case "-888999":
		{
			Object o = ItemRegistry.Create<Object>("(O)864");
			o.specialItem = true;
			o.questItem.Value = true;
			Game1.player.addItemByMenuIfNecessary(o);
			Game1.player.addQuest("130");
			this.endBehaviors();
			break;
		}
		case "-666777":
			if (!Game1.netWorldState.Value.ActivatedGoldenParrot)
			{
				Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
			}
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
			{
				Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
			}
			Game1.player.craftingRecipes.TryAdd("Fairy Dust", 0);
			this.endBehaviors();
			break;
		case "6497428":
			this.endBehaviors(new string[2] { "End", "Leo" }, Game1.currentLocation);
			break;
		case "-78765":
			this.endBehaviors(new string[2] { "End", "tunnelDepart" }, Game1.currentLocation);
			break;
		case "690006":
			if (!Game1.player.Items.ContainsId("(O)680"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)680"));
			}
			this.endBehaviors();
			break;
		case "191393":
			if (!Game1.player.Items.ContainsId("(BC)116"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)116"));
			}
			this.endBehaviors(new string[4] { "End", "position", "52", "20" }, Game1.currentLocation);
			break;
		case "2123343":
			this.endBehaviors(new string[2] { "End", "newDay" }, Game1.currentLocation);
			break;
		case "404798":
			if (!Game1.player.Items.ContainsId("(T)Pan"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)Pan"));
			}
			this.endBehaviors();
			break;
		case "26":
			Game1.player.craftingRecipes.TryAdd("Wild Bait", 0);
			this.endBehaviors();
			break;
		case "611173":
			if (!Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgradeAnonymous"))
			{
				Game1.player.activeDialogueEvents.TryAdd("pamHouseUpgrade", 4);
			}
			this.endBehaviors();
			break;
		case "3091462":
			if (!Game1.player.Items.ContainsId("(F)1802"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1802"));
			}
			this.endBehaviors();
			break;
		case "3918602":
			if (!Game1.player.Items.ContainsId("(F)1309"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(F)1309"));
			}
			this.endBehaviors();
			break;
		case "19":
			Game1.player.cookingRecipes.TryAdd("Cookies", 0);
			this.endBehaviors();
			break;
		case "992553":
			Game1.player.craftingRecipes.TryAdd("Furnace", 0);
			Game1.player.addQuest("11");
			this.endBehaviors();
			break;
		case "900553":
			Game1.player.craftingRecipes.TryAdd("Garden Pot", 0);
			if (!Game1.player.Items.ContainsId("(BC)62"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)62"));
			}
			this.endBehaviors();
			break;
		case "980558":
			Game1.player.craftingRecipes.TryAdd("Mini-Jukebox", 0);
			if (!Game1.player.Items.ContainsId("(BC)209"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)209"));
			}
			this.endBehaviors();
			break;
		case "60367":
			this.endBehaviors(new string[2] { "End", "beginGame" }, Game1.currentLocation);
			break;
		case "739330":
			if (!Game1.player.Items.ContainsId("(T)BambooPole"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(T)BambooPole"));
			}
			this.endBehaviors(new string[4] { "End", "position", "43", "36" }, Game1.currentLocation);
			break;
		case "112":
			this.endBehaviors();
			Game1.player.mailReceived.Add("canReadJunimoText");
			break;
		case "558292":
			Game1.player.eventsSeen.Remove("2146991");
			this.endBehaviors(new string[2] { "End", "bed" }, Game1.currentLocation);
			break;
		case "100162":
			if (!Game1.player.Items.ContainsId("(W)0"))
			{
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(W)0"));
			}
			Game1.player.Position = new Vector2(-9999f, -99999f);
			this.endBehaviors();
			break;
		default:
			this.endBehaviors();
			break;
		}
	}

	public void receiveActionPress(int xTile, int yTile)
	{
		if (xTile != this.playerControlTargetTile.X || yTile != this.playerControlTargetTile.Y)
		{
			return;
		}
		string text = this.playerControlSequenceID;
		if (!(text == "haleyBeach"))
		{
			if (text == "haleyBeach2")
			{
				this.EndPlayerControlSequence();
				this.CurrentCommand++;
			}
		}
		else
		{
			this.props.Clear();
			Game1.playSound("coin");
			this.playerControlTargetTile = new Point(35, 11);
			this.playerControlSequenceID = "haleyBeach2";
		}
	}

	public void startSecretSantaEvent()
	{
		this.playerControlSequence = false;
		this.playerControlSequenceID = null;
		if (!this.TryGetFestivalDataForYear("secretSanta", out var rawCommands))
		{
			Game1.log.Error("Festival " + this.id + " doesn't have the required 'secretSanta' data field.");
		}
		this.eventCommands = Event.ParseCommands(rawCommands);
		this.doingSecretSanta = true;
		this.setUpSecretSantaCommands();
		this.currentCommand = 0;
	}

	public void festivalUpdate(GameTime time)
	{
		Game1.player.team.festivalScoreStatus.UpdateState(Game1.player.festivalScore.ToString() ?? "");
		if (this.festivalTimer > 0)
		{
			int oldTime = this.festivalTimer;
			this.festivalTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.playerControlSequenceID == "iceFishing")
			{
				if (!Game1.player.UsingTool)
				{
					Game1.player.forceCanMove();
				}
				if (oldTime % 500 < this.festivalTimer % 500)
				{
					NPC temp = this.getActorByName("Pam");
					temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
					if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
					{
						temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
					}
					temp = this.getActorByName("Elliott");
					temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
					if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
					{
						temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
					}
					temp = this.getActorByName("Willy");
					temp.Sprite.sourceRect.Offset(temp.Sprite.SourceRect.Width, 0);
					if (temp.Sprite.sourceRect.X >= temp.Sprite.Texture.Width)
					{
						temp.Sprite.sourceRect.Offset(-temp.Sprite.Texture.Width, 0);
					}
				}
				if (oldTime % 29900 < this.festivalTimer % 29900)
				{
					this.getActorByName("Willy").shake(500);
					Game1.playSound("dwop");
					this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Willy").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
					{
						layerDepth = 1f,
						scale = 4f,
						interval = 9999f,
						motion = new Vector2(0f, -1f)
					});
				}
				if (oldTime % 45900 < this.festivalTimer % 45900)
				{
					this.getActorByName("Pam").shake(500);
					Game1.playSound("dwop");
					this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Pam").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
					{
						layerDepth = 1f,
						scale = 4f,
						interval = 9999f,
						motion = new Vector2(0f, -1f)
					});
				}
				if (oldTime % 59900 < this.festivalTimer % 59900)
				{
					this.getActorByName("Elliott").shake(500);
					Game1.playSound("dwop");
					this.temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), this.getActorByName("Elliott").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
					{
						layerDepth = 1f,
						scale = 4f,
						interval = 9999f,
						motion = new Vector2(0f, -1f)
					});
				}
			}
			if (this.festivalTimer <= 0)
			{
				Game1.player.Halt();
				string text = this.playerControlSequenceID;
				if (!(text == "eggHunt"))
				{
					if (text == "iceFishing")
					{
						this.EndPlayerControlSequence();
						if (!this.TryGetFestivalDataForYear("afterIceFishing", out var rawCommands))
						{
							Game1.log.Error("Festival " + this.id + " doesn't have the required 'afterIceFishing' data field.");
						}
						this.eventCommands = Event.ParseCommands(rawCommands);
						this.currentCommand = 0;
						if (Game1.activeClickableMenu != null)
						{
							Game1.activeClickableMenu.emergencyShutDown();
						}
						Game1.activeClickableMenu = null;
						if (Game1.player.UsingTool && Game1.player.CurrentTool is FishingRod rod)
						{
							rod.doneFishing(Game1.player);
						}
						Game1.screenOverlayTempSprites.Clear();
						Game1.player.forceCanMove();
					}
				}
				else
				{
					this.EndPlayerControlSequence();
					if (!this.TryGetFestivalDataForYear("afterEggHunt", out var rawCommands2))
					{
						Game1.log.Error("Festival " + this.id + " doesn't have the required 'afterEggHunt' data field.");
					}
					this.eventCommands = Event.ParseCommands(rawCommands2);
					this.currentCommand = 0;
				}
			}
		}
		if (this.startSecretSantaAfterDialogue && !Game1.dialogueUp)
		{
			Game1.globalFadeToBlack(startSecretSantaEvent, 0.01f);
			this.startSecretSantaAfterDialogue = false;
		}
		Game1.player.festivalScore = Math.Min(Game1.player.festivalScore, 9999);
	}

	private void setUpSecretSantaCommands()
	{
		Point secretSantaTile;
		try
		{
			secretSantaTile = this.getActorByName(this.mySecretSanta.Name).TilePoint;
		}
		catch
		{
			this.mySecretSanta = this.getActorByName("Lewis");
			secretSantaTile = this.getActorByName(this.mySecretSanta.Name).TilePoint;
		}
		string beforeDialogue = this.mySecretSanta.Dialogue?.GetValueOrDefault("WinterStar_GiveGift_Before");
		string afterDialogue = this.mySecretSanta.Dialogue?.GetValueOrDefault("WinterStar_GiveGift_After");
		if (Game1.player.spouse == this.mySecretSanta.Name)
		{
			beforeDialogue = this.mySecretSanta.Dialogue?.GetValueOrDefault("WinterStar_GiveGift_Before_" + (Game1.player.isRoommate(this.mySecretSanta.Name) ? "Roommate" : "Spouse")) ?? beforeDialogue;
			afterDialogue = this.mySecretSanta.Dialogue?.GetValueOrDefault("WinterStar_GiveGift_After_" + (Game1.player.isRoommate(this.mySecretSanta.Name) ? "Roommate" : "Spouse")) ?? afterDialogue;
		}
		if (this.mySecretSanta.Age == 2)
		{
			if (beforeDialogue == null)
			{
				beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1497");
			}
			if (afterDialogue == null)
			{
				afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1498");
			}
		}
		else if (this.mySecretSanta.Manners == 2)
		{
			if (beforeDialogue == null)
			{
				beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1501");
			}
			if (afterDialogue == null)
			{
				afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1504");
			}
		}
		else
		{
			if (beforeDialogue == null)
			{
				beforeDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1499");
			}
			if (afterDialogue == null)
			{
				afterDialogue = Game1.LoadStringByGender(this.mySecretSanta.Gender, "Strings\\StringsFromCSFiles:Event.cs.1500");
			}
		}
		for (int i = 0; i < this.eventCommands.Length; i++)
		{
			this.eventCommands[i] = this.eventCommands[i].Replace("secretSanta", this.mySecretSanta.Name);
			this.eventCommands[i] = this.eventCommands[i].Replace("warpX", secretSantaTile.X.ToString() ?? "");
			this.eventCommands[i] = this.eventCommands[i].Replace("warpY", secretSantaTile.Y.ToString() ?? "");
			this.eventCommands[i] = this.eventCommands[i].Replace("dialogue1", beforeDialogue);
			this.eventCommands[i] = this.eventCommands[i].Replace("dialogue2", afterDialogue);
		}
	}

	public void drawFarmers(SpriteBatch b)
	{
		foreach (Farmer farmerActor in this.farmerActors)
		{
			farmerActor.draw(b);
		}
	}

	public virtual bool ShouldHideCharacter(NPC n)
	{
		if (n is Child && this.doingSecretSanta)
		{
			return true;
		}
		return false;
	}

	public void draw(SpriteBatch b)
	{
		if (this.currentCustomEventScript != null)
		{
			this.currentCustomEventScript.draw(b);
			return;
		}
		foreach (NPC n in this.actors)
		{
			if (!this.ShouldHideCharacter(n))
			{
				n.Name.Equals("Marcello");
				if (n.ySourceRectOffset == 0)
				{
					n.draw(b);
				}
				else
				{
					n.draw(b, n.ySourceRectOffset);
				}
			}
		}
		foreach (Object prop in this.props)
		{
			prop.drawAsProp(b);
		}
		foreach (Prop festivalProp in this.festivalProps)
		{
			festivalProp.draw(b);
		}
		if (this.isSpecificFestival("fall16"))
		{
			Vector2 start = Game1.GlobalToLocal(Game1.viewport, new Vector2(37f, 56f) * 64f);
			start.X += 4f;
			int xCutoff = (int)start.X + 168;
			start.Y += 8f;
			for (int i = 0; i < Game1.player.team.grangeDisplay.Count; i++)
			{
				if (Game1.player.team.grangeDisplay[i] != null)
				{
					start.Y += 42f;
					start.X += 4f;
					b.Draw(Game1.shadowTexture, start, Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					start.Y -= 42f;
					start.X -= 4f;
					Game1.player.team.grangeDisplay[i].drawInMenu(b, start, 1f, 1f, (float)i / 1000f + 0.001f, StackDrawType.Hide);
				}
				start.X += 60f;
				if (start.X >= (float)xCutoff)
				{
					start.X = xCutoff - 168;
					start.Y += 64f;
				}
			}
		}
		if (this.drawTool)
		{
			Game1.drawTool(this.farmer);
		}
	}

	public void drawUnderWater(SpriteBatch b)
	{
		if (this.underwaterSprites == null)
		{
			return;
		}
		foreach (TemporaryAnimatedSprite underwaterSprite in this.underwaterSprites)
		{
			underwaterSprite.draw(b);
		}
	}

	public void drawAfterMap(SpriteBatch b)
	{
		if (this.aboveMapSprites != null)
		{
			foreach (TemporaryAnimatedSprite aboveMapSprite in this.aboveMapSprites)
			{
				aboveMapSprite.draw(b);
			}
		}
		if (!Game1.game1.takingMapScreenshot && this.playerControlSequenceID != null)
		{
			switch (this.playerControlSequenceID)
			{
			case "eggHunt":
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(32, 32, 224, 160), Color.Black * 0.5f);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", this.festivalTimer / 1000), Color.Black, Color.Yellow, new Vector2(64f, 64f), 0f, 1f, 1f, tiny: false);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1515", Game1.player.festivalScore), Color.Black, Color.Pink, new Vector2(64f, 128f), 0f, 1f, 1f, tiny: false);
				if (Game1.IsMultiplayer)
				{
					Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
				}
				break;
			case "fair":
				b.End();
				Game1.PushUIMode();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
				b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
				if (Game1.activeClickableMenu == null)
				{
					Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.dayTimeMoneyBox.xPositionOnScreen, 4);
				}
				b.End();
				Game1.PopUIMode();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (Game1.IsMultiplayer)
				{
					Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
				}
				break;
			case "iceFishing":
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 128), Color.Black * 0.75f);
				b.Draw(this.festivalTexture, new Vector2(32f, 16f), new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(96f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
				Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(this.festivalTimer), Color.Black, Color.White, new Vector2(32f, 93f), 0f, 1f, 1f, tiny: false);
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (Game1.IsMultiplayer)
				{
					Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
				}
				break;
			}
		}
		string text = this.spriteTextToDraw;
		if (text != null && text.Length > 0)
		{
			Color color = SpriteText.getColorFromIndex(this.int_useMeForAnything2);
			SpriteText.drawStringHorizontallyCenteredAt(b, this.spriteTextToDraw, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 192, this.int_useMeForAnything, -1, 999999, 1f, 1f, junimoText: false, color);
		}
		foreach (NPC actor in this.actors)
		{
			actor.drawAboveAlwaysFrontLayer(b);
		}
		if (this.skippable && !Game1.options.SnappyMenus && !Game1.game1.takingMapScreenshot)
		{
			Microsoft.Xna.Framework.Rectangle skipBounds = this.skipBounds();
			Color renderCol = Color.White;
			if (skipBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				renderCol *= 0.5f;
			}
			b.Draw(sourceRectangle: new Microsoft.Xna.Framework.Rectangle(205, 406, 22, 15), texture: Game1.mouseCursors, position: Utility.PointToVector2(skipBounds.Location), color: renderCol, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: 0.92f);
		}
		this.currentCustomEventScript?.drawAboveAlwaysFront(b);
	}

	public void EndPlayerControlSequence()
	{
		this.playerControlSequence = false;
		this.playerControlSequenceID = null;
	}

	public void OnPlayerControlSequenceEnd(string id)
	{
		Game1.player.StopSitting();
		Game1.player.CanMove = false;
		Game1.player.Halt();
	}

	public void setUpPlayerControlSequence(string id)
	{
		this.playerControlSequenceID = id;
		this.playerControlSequence = true;
		Game1.player.CanMove = true;
		Game1.viewportFreeze = false;
		Game1.forceSnapOnNextViewportUpdate = true;
		Game1.globalFade = false;
		this.doingSecretSanta = false;
		if (id == null)
		{
			return;
		}
		switch (id.Length)
		{
		case 10:
			switch (id[0])
			{
			case 'h':
				if (id == "haleyBeach")
				{
					Vector2 tile = new Vector2(53f, 8f);
					Object item = ItemRegistry.Create<Object>("(O)742");
					item.TileLocation = tile;
					item.Flipped = false;
					this.props.Add(item);
					this.playerControlTargetTile = new Point(53, 8);
					Game1.player.canOnlyWalk = false;
				}
				break;
			case 'p':
				if (id == "parrotRide")
				{
					Game1.player.canOnlyWalk = false;
					this.currentCommand++;
				}
				break;
			case 'i':
				if (id == "iceFishing")
				{
					Tool rod = ItemRegistry.Create<Tool>("(T)BambooPole");
					rod.AttachmentSlotsCount = 2;
					rod.attachments[1] = ItemRegistry.Create<Object>("(O)687");
					this.festivalTimer = 120000;
					this.farmer.festivalScore = 0;
					this.farmer.CurrentToolIndex = 0;
					this.farmer.TemporaryItem = rod;
					this.farmer.CurrentToolIndex = 0;
				}
				break;
			}
			break;
		case 11:
			switch (id[0])
			{
			case 'e':
				if (id == "eggFestival")
				{
					this.festivalHost = this.getActorByName("Lewis");
					this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1521";
				}
				break;
			case 'i':
			{
				if (!(id == "iceFestival"))
				{
					break;
				}
				this.festivalHost = this.getActorByName("Lewis");
				this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1548";
				if (Game1.year % 2 == 0)
				{
					this.temporaryLocation.setFireplace(on: true, 46, 16, playSound: false, -28, 28);
					this.temporaryLocation.setFireplace(on: true, 61, 43, playSound: false, -28, 28);
				}
				else
				{
					this.temporaryLocation.setFireplace(on: true, 11, 44, playSound: false, -28, 28);
					this.temporaryLocation.setFireplace(on: true, 65, 45, playSound: false, -28, 28);
				}
				if (!Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
				{
					break;
				}
				for (int x = 52; x < 60; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						this.temporaryLocation.removeTile(x, y, "AlwaysFront");
					}
				}
				if (!NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
				{
					this.temporaryLocation.ApplyMapOverride("Forest_RaccoonStump", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6));
				}
				else
				{
					this.temporaryLocation.ApplyMapOverride("Forest_RaccoonHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(53, 2, 7, 6));
				}
				break;
			}
			}
			break;
		case 4:
			switch (id[0])
			{
			case 'l':
				if (id == "luau")
				{
					this.festivalHost = this.getActorByName("Lewis");
					this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1527";
				}
				break;
			case 'f':
				if (id == "fair")
				{
					this.festivalHost = this.getActorByName("Lewis");
					this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1535";
				}
				break;
			}
			break;
		case 7:
			switch (id[0])
			{
			case 'j':
				if (id == "jellies")
				{
					this.festivalHost = this.getActorByName("Lewis");
					this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1531";
				}
				break;
			case 'e':
			{
				if (!(id == "eggHunt"))
				{
					break;
				}
				Layer pathsLayer = Game1.currentLocation.map.RequireLayer("Paths");
				for (int x2 = 0; x2 < pathsLayer.LayerWidth; x2++)
				{
					for (int y2 = 0; y2 < pathsLayer.LayerHeight; y2++)
					{
						Tile tile2 = pathsLayer.Tiles[x2, y2];
						if (tile2 != null && tile2.TileSheet.Id.StartsWith("fest"))
						{
							this.festivalProps.Add(new Prop(this.festivalTexture, tile2.TileIndex, 1, 1, 1, x2, y2));
						}
					}
				}
				this.festivalTimer = 52000;
				this.currentCommand++;
				break;
			}
			}
			break;
		case 9:
			switch (id[0])
			{
			case 'h':
				if (id == "halloween")
				{
					if (Game1.year % 2 == 0)
					{
						this.temporaryLocation.objects.Add(new Vector2(63f, 16f), new Chest(new List<Item> { ItemRegistry.Create("(O)PrizeTicket") }, new Vector2(63f, 16f)));
					}
					else
					{
						this.temporaryLocation.objects.Add(new Vector2(33f, 13f), new Chest(new List<Item> { ItemRegistry.Create("(O)373") }, new Vector2(33f, 13f)));
					}
				}
				break;
			case 'c':
				if (id == "christmas")
				{
					this.secretSantaRecipient = Utility.GetRandomWinterStarParticipant();
					this.mySecretSanta = Utility.GetRandomWinterStarParticipant((string name) => name == this.secretSantaRecipient.Name || NPC.IsDivorcedFrom(this.farmer, name)) ?? this.secretSantaRecipient;
					Game1.debugOutput = "Secret Santa Recipient: " + this.secretSantaRecipient.Name + "  My Secret Santa: " + this.mySecretSanta.Name;
				}
				break;
			}
			break;
		case 14:
			if (id == "flowerFestival")
			{
				this.festivalHost = this.getActorByName("Lewis");
				this.hostMessageKey = "Strings\\StringsFromCSFiles:Event.cs.1524";
				if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
				{
					Game1.currentLocation.removeMapTile(62, 28, "Buildings");
					Game1.currentLocation.removeMapTile(64, 28, "Buildings");
					Game1.currentLocation.removeMapTile(73, 48, "Buildings");
				}
			}
			break;
		case 8:
			if (id == "boatRide")
			{
				Game1.viewportFreeze = true;
				Game1.currentViewportTarget = Utility.PointToVector2(Game1.viewportCenter);
				this.currentCommand++;
			}
			break;
		case 5:
		case 6:
		case 12:
		case 13:
			break;
		}
	}

	public bool canMoveAfterDialogue()
	{
		if (this.playerControlSequenceID != null && this.playerControlSequenceID.Equals("eggHunt"))
		{
			Game1.player.canMove = true;
			this.CurrentCommand++;
		}
		return this.playerControlSequence;
	}

	public void forceFestivalContinue()
	{
		bool isFallFestival = this.isSpecificFestival("fall16");
		if (isFallFestival)
		{
			this.initiateGrangeJudging();
		}
		else
		{
			Game1.dialogueUp = false;
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
			}
			Game1.exitActiveMenu();
			if (!this.TryGetFestivalDataForYear("mainEvent", out var rawCommands))
			{
				Game1.log.Error("Festival " + this.id + " doesn't have the required 'mainEvent' data field.");
			}
			string[] newCommands = Event.ParseCommands(rawCommands);
			this.eventCommands = newCommands;
			this.CurrentCommand = 0;
			this.eventSwitched = true;
			this.playerControlSequence = false;
			this.setUpFestivalMainEvent();
			Game1.player.Halt();
		}
		if (Game1.IsServer && (isFallFestival || !Game1.HasDedicatedHost))
		{
			Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
		}
	}

	/// <summary>Split an event's key into its ID and preconditions.</summary>
	/// <param name="rawScript">The event key to split.</param>
	public static string[] SplitPreconditions(string rawScript)
	{
		return ArgUtility.SplitQuoteAware(rawScript, '/', StringSplitOptions.RemoveEmptyEntries, keepQuotesAndEscapes: true);
	}

	/// <summary>Split and preprocess a raw event script into its component commands.</summary>
	/// <param name="rawScript">The raw event script to split.</param>
	/// <param name="player">The player for which the event is being parsed.</param>
	public static string[] ParseCommands(string rawScript, Farmer player = null)
	{
		rawScript = Dialogue.applyGenderSwitchBlocks(((player != null) ? new Gender?(player.Gender) : Game1.player?.Gender).GetValueOrDefault(), rawScript);
		rawScript = TokenParser.ParseText(rawScript);
		return ArgUtility.SplitQuoteAware(rawScript, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries, keepQuotesAndEscapes: true);
	}

	public bool isSpecificFestival(string festivalId)
	{
		if (this.isFestival)
		{
			return this.id == "festival_" + festivalId;
		}
		return false;
	}

	public void setUpFestivalMainEvent()
	{
		if (!this.isSpecificFestival("spring24"))
		{
			return;
		}
		List<NetDancePartner> females = new List<NetDancePartner>();
		List<NetDancePartner> males = new List<NetDancePartner>();
		List<string> leftoverFemales = new List<string> { "Abigail", "Penny", "Leah", "Maru", "Haley", "Emily" };
		List<string> leftoverMales = new List<string> { "Sebastian", "Sam", "Elliott", "Harvey", "Alex", "Shane" };
		List<Farmer> farmers = (from farmer in Game1.getOnlineFarmers()
			orderby farmer.UniqueMultiplayerID
			select farmer).ToList();
		while (farmers.Count > 0)
		{
			Farmer f = farmers[0];
			farmers.RemoveAt(0);
			if (Game1.multiplayer.isDisconnecting(f) || f.dancePartner.Value == null)
			{
				continue;
			}
			if (f.dancePartner.GetGender() == Gender.Female)
			{
				females.Add(f.dancePartner);
				if (f.dancePartner.IsVillager())
				{
					leftoverFemales.Remove(f.dancePartner.TryGetVillager().Name);
				}
				males.Add(new NetDancePartner(f));
			}
			else
			{
				males.Add(f.dancePartner);
				if (f.dancePartner.IsVillager())
				{
					leftoverMales.Remove(f.dancePartner.TryGetVillager().Name);
				}
				females.Add(new NetDancePartner(f));
			}
			if (f.dancePartner.IsFarmer())
			{
				farmers.Remove(f.dancePartner.TryGetFarmer());
			}
		}
		while (females.Count < 6)
		{
			string female = leftoverFemales.Last();
			if (leftoverMales.Contains(Utility.getLoveInterest(female)))
			{
				females.Add(new NetDancePartner(female));
				males.Add(new NetDancePartner(Utility.getLoveInterest(female)));
			}
			leftoverFemales.Remove(female);
		}
		if (!this.TryGetFestivalDataForYear("mainEvent", out var rawFestivalData))
		{
			rawFestivalData = string.Empty;
		}
		for (int i = 1; i <= 6; i++)
		{
			string female2 = ((!females[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(females[i - 1].TryGetFarmer())) : females[i - 1].TryGetVillager().Name);
			string male = ((!males[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(males[i - 1].TryGetFarmer())) : males[i - 1].TryGetVillager().Name);
			rawFestivalData = rawFestivalData.Replace("Girl" + i, female2);
			rawFestivalData = rawFestivalData.Replace("Guy" + i, male);
		}
		List<KeyValuePair<NetDancePartner, NetDancePartner>> pairsByInnermost = new List<KeyValuePair<NetDancePartner, NetDancePartner>>();
		List<KeyValuePair<NetDancePartner, NetDancePartner>> playerPairs = new List<KeyValuePair<NetDancePartner, NetDancePartner>>();
		for (int i2 = females.Count - 1; i2 >= 0; i2--)
		{
			NetDancePartner female3 = females[i2];
			NetDancePartner male2 = males[i2];
			if (female3.IsFarmer() || male2.IsFarmer())
			{
				playerPairs.Add(new KeyValuePair<NetDancePartner, NetDancePartner>(female3, male2));
				females.RemoveAt(i2);
				males.RemoveAt(i2);
			}
		}
		pairsByInnermost.AddRange(playerPairs.OrderBy(delegate(KeyValuePair<NetDancePartner, NetDancePartner> keyValuePair)
		{
			int farmerNumberFromFarmer = Utility.getFarmerNumberFromFarmer(keyValuePair.Key.TryGetFarmer());
			int farmerNumberFromFarmer2 = Utility.getFarmerNumberFromFarmer(keyValuePair.Value.TryGetFarmer());
			if (farmerNumberFromFarmer > -1 && farmerNumberFromFarmer2 > -1)
			{
				return Math.Min(farmerNumberFromFarmer, farmerNumberFromFarmer2);
			}
			return (farmerNumberFromFarmer <= -1) ? farmerNumberFromFarmer2 : farmerNumberFromFarmer;
		}));
		for (int i3 = 0; i3 < females.Count; i3++)
		{
			pairsByInnermost.Add(new KeyValuePair<NetDancePartner, NetDancePartner>(females[i3], males[i3]));
		}
		females.Clear();
		males.Clear();
		bool addLeft = true;
		foreach (KeyValuePair<NetDancePartner, NetDancePartner> pair in pairsByInnermost)
		{
			if (addLeft)
			{
				females.Insert(0, pair.Key);
				males.Insert(0, pair.Value);
			}
			else
			{
				females.Add(pair.Key);
				males.Add(pair.Value);
			}
			addLeft = !addLeft;
		}
		List<string> commandsToAdd = new List<string>(Event.ParseCommands(rawFestivalData));
		for (int i4 = 0; i4 < commandsToAdd.Count; i4++)
		{
			string command = commandsToAdd[i4];
			List<NetDancePartner> dancers = null;
			string token = null;
			if (command.Contains("Girls"))
			{
				token = "Girls";
				dancers = females;
			}
			else if (command.Contains("Guys"))
			{
				token = "Guys";
				dancers = males;
			}
			if (dancers == null)
			{
				continue;
			}
			float spacing = 10f / (float)(dancers.Count - 1);
			if (spacing < 1f)
			{
				spacing = 1f;
			}
			for (int j = 0; j < dancers.Count; j++)
			{
				string name = (dancers[j].IsVillager() ? dancers[j].TryGetVillager().Name : ("farmer" + Utility.getFarmerNumberFromFarmer(dancers[j].TryGetFarmer())));
				string newCommand = command.Replace(token, name);
				if (newCommand.StartsWith("warp "))
				{
					string[] warp = ArgUtility.SplitBySpace(newCommand);
					int x = int.Parse(warp[2]);
					warp[2] = (x + (int)Math.Round((float)j * spacing)).ToString();
					newCommand = string.Join(" ", warp);
				}
				commandsToAdd.Insert(i4 + j, newCommand);
			}
			i4 += dancers.Count;
			commandsToAdd.RemoveAt(i4);
			i4--;
		}
		rawFestivalData = string.Join("/", commandsToAdd);
		Regex regex = new Regex("showFrame (?<farmerName>farmer\\d) 44");
		Regex showFrameGirl = new Regex("showFrame (?<farmerName>farmer\\d) 40");
		Regex animation1Guy = new Regex("animate (?<farmerName>farmer\\d) false true 600 44 45");
		Regex animation1Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 43 41 43 42");
		Regex animation2Guy = new Regex("animate (?<farmerName>farmer\\d) false true 300 46 47");
		Regex animation2Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 46 47");
		rawFestivalData = regex.Replace(rawFestivalData, "showFrame $1 12/faceDirection $1 0");
		rawFestivalData = showFrameGirl.Replace(rawFestivalData, "showFrame $1 0/faceDirection $1 2");
		rawFestivalData = animation1Guy.Replace(rawFestivalData, "animate $1 false true 600 12 13 12 14");
		rawFestivalData = animation1Girl.Replace(rawFestivalData, "animate $1 false true 596 4 0");
		rawFestivalData = animation2Guy.Replace(rawFestivalData, "animate $1 false true 150 12 13 12 14");
		rawFestivalData = animation2Girl.Replace(rawFestivalData, "animate $1 false true 600 0 3");
		this.eventCommands = Event.ParseCommands(rawFestivalData);
	}

	private void judgeGrange()
	{
		int pointsEarned = 14;
		Dictionary<int, bool> categoriesRepresented = new Dictionary<int, bool>();
		int nullsCount = 0;
		bool purpleShorts = false;
		foreach (Item i in Game1.player.team.grangeDisplay)
		{
			if (i is Object obj)
			{
				if (Event.IsItemMayorShorts(obj))
				{
					purpleShorts = true;
				}
				pointsEarned += obj.Quality + 1;
				int num = obj.sellToStorePrice(-1L);
				if (num >= 20)
				{
					pointsEarned++;
				}
				if (num >= 90)
				{
					pointsEarned++;
				}
				if (num >= 200)
				{
					pointsEarned++;
				}
				if (num >= 300 && obj.Quality < 2)
				{
					pointsEarned++;
				}
				if (num >= 400 && obj.Quality < 1)
				{
					pointsEarned++;
				}
				switch (obj.Category)
				{
				case -75:
					categoriesRepresented[-75] = true;
					break;
				case -79:
					categoriesRepresented[-79] = true;
					break;
				case -18:
				case -14:
				case -6:
				case -5:
					categoriesRepresented[-5] = true;
					break;
				case -12:
				case -2:
					categoriesRepresented[-12] = true;
					break;
				case -4:
					categoriesRepresented[-4] = true;
					break;
				case -81:
				case -80:
				case -27:
					categoriesRepresented[-81] = true;
					break;
				case -7:
					categoriesRepresented[-7] = true;
					break;
				case -26:
					categoriesRepresented[-26] = true;
					break;
				}
			}
			else if (i == null)
			{
				nullsCount++;
			}
		}
		pointsEarned += Math.Min(30, categoriesRepresented.Count * 5);
		int displayFilledPoints = 9 - 2 * nullsCount;
		pointsEarned += displayFilledPoints;
		this.grangeScore = pointsEarned;
		if (purpleShorts)
		{
			this.grangeScore = -666;
		}
	}

	private void lewisDoneJudgingGrange()
	{
		if (Game1.activeClickableMenu == null)
		{
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1584")));
			Game1.player.Halt();
		}
		this.interpretGrangeResults();
	}

	public void interpretGrangeResults()
	{
		List<Character> winners = new List<Character>
		{
			this.getActorByName("Pierre"),
			this.getActorByName("Marnie"),
			this.getActorByName("Willy")
		};
		if (this.grangeScore >= 90)
		{
			winners.Insert(0, Game1.player);
		}
		else if (this.grangeScore >= 75)
		{
			winners.Insert(1, Game1.player);
		}
		else if (this.grangeScore >= 60)
		{
			winners.Insert(2, Game1.player);
		}
		else
		{
			winners.Add(Game1.player);
		}
		bool pierreWon = (winners[0] as NPC)?.Name == "Pierre";
		bool playerSkipped = Game1.player.team.grangeDisplay.Count == 0;
		bool usedPurpleShorts = this.grangeScore == -666;
		foreach (NPC actor in this.actors)
		{
			Dialogue dialogue = null;
			dialogue = ((!pierreWon) ? (actor.TryGetDialogue("Fair_Judged_PlayerWon") ?? actor.TryGetDialogue("Fair_Judged")) : ((usedPurpleShorts ? actor.TryGetDialogue("Fair_Judged_PlayerLost_PurpleShorts") : null) ?? (playerSkipped ? actor.TryGetDialogue("Fair_Judged_PlayerLost_Skipped") : null) ?? actor.TryGetDialogue("Fair_Judged_PlayerLost") ?? actor.TryGetDialogue("Fair_Judged")));
			if (dialogue != null)
			{
				actor.setNewDialogue(dialogue);
			}
		}
		this.grangeJudged = true;
		if (!(winners[0] is Farmer))
		{
			return;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.autoGenerateActiveDialogueEvent("wonGrange");
		}
	}

	private void initiateGrangeJudging()
	{
		this.judgeGrange();
		this.hostMessageKey = null;
		this.setUpAdvancedMove(ArgUtility.SplitBySpace("advancedMove Lewis False 2 0 0 7 8 0 4 3000 3 0 4 3000 3 0 4 3000 3 0 4 3000 -14 0 2 1000"), lewisDoneJudgingGrange);
		this.getActorByName("Lewis").CurrentDialogue.Clear();
		if (this.getActorByName("Marnie") != null)
		{
			this.npcControllers.RemoveAll((NPCController npcController) => npcController.puppet.Name == "Marnie");
		}
		this.setUpAdvancedMove(ArgUtility.SplitBySpace("advancedMove Marnie False 0 1 4 1000"));
		foreach (NPC actor in this.actors)
		{
			Dialogue dialogue = actor.TryGetDialogue("Fair_Judging");
			if (dialogue != null)
			{
				actor.setNewDialogue(dialogue);
			}
		}
	}

	public void answerDialogueQuestion(NPC who, string answerKey)
	{
		if (!this.isFestival)
		{
			return;
		}
		switch (answerKey)
		{
		case "yes":
			if (Game1.HasDedicatedHost)
			{
				if (this.isSpecificFestival("fall16"))
				{
					if (Game1.IsServer)
					{
						this.forceFestivalContinue();
					}
					else
					{
						Game1.dedicatedServer.DoHostAction("JudgeGrange");
					}
					break;
				}
				string festivalCheck = "MainEvent_" + this.id;
				Game1.netReady.SetLocalReady(festivalCheck, ready: true);
				Game1.activeClickableMenu = new ReadyCheckDialog(festivalCheck, allowCancel: true, delegate
				{
					this.forceFestivalContinue();
				});
			}
			else
			{
				this.forceFestivalContinue();
			}
			break;
		case "danceAsk":
			if (Game1.player.spouse != null && who.Name == Game1.player.spouse)
			{
				Game1.player.dancePartner.Value = who;
				who.setNewDialogue(who.TryGetDialogue("FlowerDance_Accept_" + (Game1.player.isRoommate(who.Name) ? "Roommate" : "Spouse")) ?? who.TryGetDialogue("FlowerDance_Accept") ?? new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1632"));
				foreach (NPC n in this.actors)
				{
					Stack<Dialogue> currentDialogue = n.CurrentDialogue;
					if (currentDialogue != null && currentDialogue.Count > 0 && n.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
					{
						n.CurrentDialogue.Clear();
					}
				}
			}
			else if (!who.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.Name) >= 1000 && !who.isMarried())
			{
				try
				{
					Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name));
				}
				catch
				{
				}
				Game1.player.dancePartner.Value = who;
				who.setNewDialogue(who.TryGetDialogue("FlowerDance_Accept") ?? ((who.Gender == Gender.Female) ? new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1634") : new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1633")));
				foreach (NPC n2 in this.actors)
				{
					Stack<Dialogue> currentDialogue2 = n2.CurrentDialogue;
					if (currentDialogue2 != null && currentDialogue2.Count > 0 && n2.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
					{
						n2.CurrentDialogue.Clear();
					}
				}
			}
			else if (who.HasPartnerForDance)
			{
				who.setNewDialogue("Strings\\StringsFromCSFiles:Event.cs.1635");
			}
			else
			{
				Dialogue dialogue = who.TryGetDialogue("FlowerDance_Decline") ?? who.TryGetDialogue("danceRejection");
				if (dialogue == null)
				{
					break;
				}
				who.setNewDialogue(dialogue);
			}
			Game1.drawDialogue(who);
			who.immediateSpeak = true;
			who.facePlayer(Game1.player);
			who.Halt();
			break;
		case "no":
			break;
		}
	}

	public void addItemToGrangeDisplay(Item i, int position, bool force)
	{
		while (Game1.player.team.grangeDisplay.Count < 9)
		{
			Game1.player.team.grangeDisplay.Add(null);
		}
		if (position >= 0 && position < Game1.player.team.grangeDisplay.Count && (Game1.player.team.grangeDisplay[position] == null || force))
		{
			Game1.player.team.grangeDisplay[position] = i;
		}
	}

	private bool onGrangeChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
	{
		if (!onRemoval)
		{
			if (i.Stack > 1 || (i.Stack == 1 && old != null && old.Stack == 1 && i.canStackWith(old)))
			{
				if (old != null && i != null && old.canStackWith(i))
				{
					container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
					container.heldItem = old;
					return false;
				}
				if (old != null)
				{
					Utility.addItemToInventory(old, position, container.ItemsToGrabMenu.actualInventory);
					container.heldItem = i;
					return false;
				}
				int allButOne = i.Stack - 1;
				Item reject = i.getOne();
				reject.Stack = allButOne;
				container.heldItem = reject;
				i.Stack = 1;
			}
		}
		else if (old != null && old.Stack > 1 && !old.Equals(i))
		{
			return false;
		}
		this.addItemToGrangeDisplay((onRemoval && (old == null || old.Equals(i))) ? null : i, position, force: true);
		return true;
	}

	public bool canPlayerUseTool()
	{
		if (this.isSpecificFestival("winter8") && this.festivalTimer > 0 && !Game1.player.UsingTool)
		{
			this.previousFacingDirection = Game1.player.FacingDirection;
			return true;
		}
		return false;
	}

	public bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (this.isFestival)
		{
			if (this.temporaryLocation != null && this.temporaryLocation.objects.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y), out var tempObj))
			{
				tempObj.checkForAction(who);
			}
			GameLocation location = Game1.currentLocation;
			switch (this.id)
			{
			case "festival_fall16":
				switch (location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "untitled tile sheet"))
				{
				case 175:
				case 176:
					if (who.IsLocalPlayer)
					{
						Game1.player.eatObject(ItemRegistry.Create<Object>("(O)241"), overrideFullness: true);
					}
					return true;
				case 308:
				case 309:
					if (who.IsLocalPlayer)
					{
						Response[] colors = new Response[3]
						{
							new Response("Orange", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1645")),
							new Response("Green", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1647")),
							new Response("I", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1650"))
						};
						location.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1652")), colors, "wheelBet");
					}
					return true;
				case 87:
				case 88:
					if (who.IsLocalPlayer)
					{
						Response[] responses4 = new Response[2]
						{
							new Response("Buy", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654")),
							new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1656"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1659"), responses4, "StarTokenShop");
					}
					return true;
				case 501:
				case 502:
					if (who.IsLocalPlayer)
					{
						Response[] responses3 = new Response[2]
						{
							new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
							new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1666"), responses3, "slingshotGame");
					}
					return true;
				case 510:
				case 511:
					if (who.IsLocalPlayer)
					{
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1672"), location.createYesNoResponses(), "starTokenShop");
					}
					return true;
				case 349:
				case 350:
				case 351:
					Game1.player.team.grangeMutex.RequestLock(delegate
					{
						while (Game1.player.team.grangeDisplay.Count < 9)
						{
							Game1.player.team.grangeDisplay.Add(null);
						}
						Game1.activeClickableMenu = new StorageContainer(Game1.player.team.grangeDisplay.ToList(), 9, 3, onGrangeChange, Utility.highlightSmallObjects);
					});
					return true;
				case 503:
				case 504:
					if (who.IsLocalPlayer)
					{
						Response[] responses2 = new Response[2]
						{
							new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
							new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
						};
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1681"), responses2, "fishingGame");
					}
					return true;
				case 540:
					if (who.IsLocalPlayer)
					{
						if (who.TilePoint.X == 29)
						{
							Game1.activeClickableMenu = new StrengthGame();
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1684")));
						}
					}
					return true;
				case 505:
				case 506:
					if (who.IsLocalPlayer)
					{
						if (who.Money >= 100 && !who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Response[] responses = new Response[2]
							{
								new Response("Read", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1688")),
								new Response("No", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1690"))
							};
							location.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1691")), responses, "fortuneTeller");
						}
						else if (who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1694")));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1695")));
						}
						who.Halt();
					}
					return true;
				}
				break;
			case "festival_fall27":
				if (location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "Landscape") == 958 && ((tileLocation.X == 44 && tileLocation.Y == 9) || (tileLocation.X == 61 && tileLocation.Y == 13)))
				{
					if (who.IsLocalPlayer)
					{
						location.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SpiritsEveCart"), location.createYesNoResponses(), "spirits_eve_shortcut");
					}
					return true;
				}
				break;
			case "festival_winter8":
			{
				int tileIndexAt = location.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "fest");
				if ((uint)(tileIndexAt - 1009) <= 1u || (uint)(tileIndexAt - 1012) <= 1u)
				{
					Game1.playSound("pig");
					return true;
				}
				break;
			}
			}
			string tileAction = location.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
			if (tileAction != null)
			{
				try
				{
					string[] args = ArgUtility.SplitBySpace(tileAction);
					switch (ArgUtility.Get(args, 0))
					{
					case "OpenShop":
					case "Shop":
					{
						if (!ArgUtility.TryGet(args, 1, out var shop_id, out var error, allowBlank: true, "string shop_id"))
						{
							location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error);
							return false;
						}
						if (!who.IsLocalPlayer)
						{
							return false;
						}
						bool opened = false;
						if (shop_id == "shop" && this.isFestival)
						{
							switch (this.id)
							{
							case "festival_fall27":
								shop_id = "Festival_SpiritsEve_Pierre";
								break;
							case "festival_spring13":
								shop_id = "Festival_EggFestival_Pierre";
								break;
							case "festival_spring24":
								shop_id = "Festival_FlowerDance_Pierre";
								break;
							case "festival_summer11":
								shop_id = "Festival_Luau_Pierre";
								break;
							case "festival_summer28":
								shop_id = "Festival_DanceOfTheMoonlightJellies_Pierre";
								break;
							case "festival_winter8":
								shop_id = "Festival_FestivalOfIce_TravelingMerchant";
								break;
							case "festival_winter25":
								shop_id = "Festival_FeastOfTheWinterStar_Pierre";
								break;
							}
						}
						if (this.festivalData.TryGetValue(shop_id, out var legacyShopData))
						{
							if (this.festivalShops == null)
							{
								this.festivalShops = new Dictionary<string, Dictionary<ISalable, ItemStockInformation>>();
							}
							if (!this.festivalShops.TryGetValue(shop_id, out var stockList))
							{
								string[] inventoryList = ArgUtility.SplitBySpace(legacyShopData);
								stockList = new Dictionary<ISalable, ItemStockInformation>();
								for (int i = 0; i < inventoryList.Length; i += 4)
								{
									if (!ArgUtility.TryGet(args, i, out var type, out error, allowBlank: true, "string type") || !ArgUtility.TryGet(args, i + 1, out var itemId, out error, allowBlank: true, "string itemId") || !ArgUtility.TryGetInt(args, i + 2, out var price, out error, "int price") || !ArgUtility.TryGetInt(args, i + 3, out var stock, out error, "int stock"))
									{
										Game1.log.Error($"Festival '{this.id}' has legacy shop inventory '{string.Join(" ", inventoryList)}' which couldn't be parsed: {error}.");
										break;
									}
									Item item = Utility.getItemFromStandardTextDescription(type, itemId, stock, who);
									if (item != null)
									{
										if (item.Category == -74)
										{
											price = (int)Math.Max(1f, (float)price * Game1.MasterPlayer.difficultyModifier);
										}
										if (!item.IsRecipe || !who.knowsRecipe(item.Name))
										{
											stockList.Add(item, new ItemStockInformation(price, (stock <= 0) ? int.MaxValue : stock, null, null, LimitedStockMode.Player));
										}
									}
								}
								this.festivalShops[shop_id] = stockList;
							}
							if (stockList != null && stockList.Count > 0)
							{
								who.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(who.currentLocation.Name + shop_id, stockList);
								Game1.activeClickableMenu = new ShopMenu(this.id + "_" + shop_id, stockList);
								opened = true;
							}
						}
						bool showedClosedMessage = false;
						if (!opened && Utility.TryOpenShopMenu(shop_id, this.temporaryLocation, null, null, forceOpen: false, playOpenSound: true, delegate(string dialogue4)
						{
							showedClosedMessage = true;
							Game1.drawObjectDialogue(dialogue4);
						}))
						{
							opened = true;
						}
						if (!opened && !showedClosedMessage)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1714"));
						}
						break;
					}
					case "Message":
					{
						if (!ArgUtility.TryGet(args, 1, out var translationKey, out var error3, allowBlank: true, "string translationKey"))
						{
							location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error3);
							return false;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey.Replace("\"", "")));
						break;
					}
					case "Dialogue":
					{
						if (!ArgUtility.TryGetRemainder(args, 1, out var dialogue, out var error2, ' ', "string dialogue"))
						{
							location.LogTileActionError(args, tileLocation.X, tileLocation.Y, error2);
							return false;
						}
						Game1.drawObjectDialogue(dialogue.Replace("#", " "));
						break;
					}
					case "LuauSoup":
						if (!this.specialEventVariable2)
						{
							Game1.activeClickableMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightLuauSoupItems, clickToAddItemToLuauSoup, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1719"), null, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
						}
						break;
					}
				}
				catch (Exception)
				{
				}
				return false;
			}
			if (who.IsLocalPlayer && (!this.playerControlSequence || !this.playerControlSequenceID.Equals("iceFishing")))
			{
				foreach (NPC n in this.actors)
				{
					Point tile = n.TilePoint;
					Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
					if (tile.X == tileLocation.X && tile.Y == tileLocation.Y && n is Child child)
					{
						child.checkAction(who, this.temporaryLocation);
						return true;
					}
					if (((tile.X != tileLocation.X || (tile.Y != tileLocation.Y && tile.Y != tileLocation.Y + 1)) && !n.GetBoundingBox().Intersects(tileRect)) || (n.CurrentDialogue.Count < 1 && (n.CurrentDialogue.Count <= 0 || n.CurrentDialogue.Peek().isOnFinalDialogue()) && !n.Equals(this.festivalHost) && (!n.datable.Value || !this.isSpecificFestival("spring24")) && (this.secretSantaRecipient == null || !n.Name.Equals(this.secretSantaRecipient.Name))))
					{
						continue;
					}
					Friendship friendship;
					bool divorced = who.friendshipData.TryGetValue(n.Name, out friendship) && friendship.IsDivorced();
					if ((this.grangeScore > -100 || this.grangeScore == -666) && n.Equals(this.festivalHost) && this.grangeJudged)
					{
						Dialogue message;
						if (this.grangeScore >= 90)
						{
							Game1.playSound("reward");
							message = Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1723", this.grangeScore);
							Game1.player.festivalScore += 1000;
							Game1.getAchievement(37);
						}
						else if (this.grangeScore >= 75)
						{
							Game1.playSound("reward");
							message = Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1726", this.grangeScore);
							Game1.player.festivalScore += 500;
						}
						else if (this.grangeScore >= 60)
						{
							Game1.playSound("newArtifact");
							message = Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1729", this.grangeScore);
							Game1.player.festivalScore += 250;
						}
						else if (this.grangeScore == -666)
						{
							Game1.playSound("secret1");
							message = new Dialogue(n, "Strings\\StringsFromCSFiles:Event.cs.1730");
							Game1.player.festivalScore += 750;
						}
						else
						{
							Game1.playSound("newArtifact");
							message = Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1732", this.grangeScore);
							Game1.player.festivalScore += 50;
						}
						this.grangeScore = -100;
						n.setNewDialogue(message);
					}
					else if ((Game1.HasDedicatedHost || Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && n.Equals(this.festivalHost) && (n.CurrentDialogue.Count == 0 || n.CurrentDialogue.Peek().isOnFinalDialogue()) && this.hostMessageKey != null)
					{
						n.setNewDialogue(this.hostMessageKey);
					}
					if (this.isSpecificFestival("spring24") && !divorced)
					{
						bool? flag = n.GetData()?.FlowerDanceCanDance;
						bool num;
						if (!flag.HasValue)
						{
							if (n.datable.Value)
							{
								goto IL_0f2f;
							}
							num = n.Name == who.spouse;
						}
						else
						{
							num = flag == true;
						}
						if (num)
						{
							goto IL_0f2f;
						}
					}
					goto IL_1149;
					IL_0f2f:
					n.grantConversationFriendship(who);
					if (who.dancePartner.Value == null)
					{
						if (n.CurrentDialogue.Count > 0 && n.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
						{
							n.CurrentDialogue.Clear();
						}
						if (n.CurrentDialogue.Count == 0)
						{
							n.CurrentDialogue.Push(new Dialogue(n, null, "..."));
							if (n.name.Value == who.spouse)
							{
								n.setNewDialogue(Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1736", n.displayName), add: true);
							}
							else
							{
								n.setNewDialogue(Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:Event.cs.1738", n.displayName), add: true);
							}
						}
						else if (n.CurrentDialogue.Peek().isOnFinalDialogue())
						{
							Dialogue d = n.CurrentDialogue.Peek();
							if (who.spouse != null && n.Name == who.spouse)
							{
								Dialogue dialogue2 = null;
								if (n.isRoommate())
								{
									this.TryGetFestivalDialogueForYear(n, n.Name + "_roommate", out dialogue2);
								}
								if (dialogue2 == null)
								{
									this.TryGetFestivalDialogueForYear(n, n.Name + "_spouse", out dialogue2);
								}
								if (dialogue2 != null)
								{
									n.CurrentDialogue.Clear();
									n.CurrentDialogue.Push(dialogue2);
									d = n.CurrentDialogue.Peek();
								}
							}
							Game1.drawDialogue(n);
							n.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
							who.Halt();
							n.CurrentDialogue = new Stack<Dialogue>();
							n.CurrentDialogue.Push(new Dialogue(n, null, "..."));
							n.CurrentDialogue.Push(d);
							return true;
						}
					}
					else if (n.CurrentDialogue.Count > 0 && n.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
					{
						n.CurrentDialogue.Clear();
					}
					goto IL_1149;
					IL_1149:
					if (!divorced && this.secretSantaRecipient != null && n.Name.Equals(this.secretSantaRecipient.Name))
					{
						n.grantConversationFriendship(who);
						location.createQuestionDialogue(Game1.parseText((this.secretSantaRecipient.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1740", this.secretSantaRecipient.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1741", this.secretSantaRecipient.displayName)), location.createYesNoResponses(), "secretSanta");
						who.Halt();
						return true;
					}
					if (n.CurrentDialogue.Count == 0)
					{
						return true;
					}
					if (who.spouse != null && n.Name == who.spouse && !this.isSpecificFestival("spring24"))
					{
						Dialogue dialogue3 = null;
						if (n.isRoommate())
						{
							this.TryGetFestivalDialogueForYear(n, n.Name + "_roommate", out dialogue3);
						}
						if (dialogue3 == null)
						{
							this.TryGetFestivalDialogueForYear(n, n.Name + "_spouse", out dialogue3);
						}
						if (dialogue3 != null && (n.CurrentDialogue.Count == 0 || !n.CurrentDialogue.Peek().TranslationKey.Equals(dialogue3.TranslationKey)))
						{
							n.CurrentDialogue.Clear();
							n.CurrentDialogue.Push(dialogue3);
						}
					}
					if (divorced)
					{
						n.CurrentDialogue.Clear();
						n.CurrentDialogue.Push(new Dialogue(n, "Characters\\Dialogue\\" + n.Name + ":divorced"));
					}
					n.grantConversationFriendship(who);
					if (n.CurrentDialogue == null || n.CurrentDialogue.Count == 0 || !n.CurrentDialogue.Peek().dontFaceFarmer)
					{
						n.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
					}
					Game1.drawDialogue(n);
					who.Halt();
					return true;
				}
			}
			if (this.festivalData != null && this.isSpecificFestival("spring13"))
			{
				Microsoft.Xna.Framework.Rectangle tile2 = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
				for (int i2 = this.festivalProps.Count - 1; i2 >= 0; i2--)
				{
					if (this.festivalProps[i2].isColliding(tile2))
					{
						who.festivalScore++;
						this.festivalProps.RemoveAt(i2);
						who.team.FestivalPropsRemoved(tile2);
						if (who.IsLocalPlayer)
						{
							Game1.playSound("coin");
						}
						return true;
					}
				}
			}
			foreach (MapSeat seat in location.mapSeats)
			{
				if (seat.OccupiesTile(tileLocation.X, tileLocation.Y) && !seat.IsBlocked(location))
				{
					who.BeginSitting(seat);
					return true;
				}
			}
		}
		return false;
	}

	public void removeFestivalProps(Microsoft.Xna.Framework.Rectangle rect)
	{
		this.festivalProps.RemoveAll((Prop prop) => prop.isColliding(rect));
	}

	public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
	{
		if (this.isFestival && this.festivalHost != null && this.festivalHost.Tile == tileLocation)
		{
			Game1.mouseCursor = Game1.cursor_talk;
		}
	}

	public void forceEndFestival(Farmer who)
	{
		Game1.currentMinigame = null;
		Game1.exitActiveMenu();
		Game1.player.Halt();
		this.endBehaviors();
		if (Game1.IsServer)
		{
			Game1.multiplayer.sendServerToClientsMessage("endFest");
		}
		Game1.changeMusicTrack("none");
	}

	public bool checkForCollision(Microsoft.Xna.Framework.Rectangle position, Farmer who)
	{
		Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
		foreach (NPC n in this.actors)
		{
			Microsoft.Xna.Framework.Rectangle actorBounds = n.GetBoundingBox();
			if (actorBounds.Intersects(position) && !this.farmer.temporarilyInvincible && this.farmer.TemporaryPassableTiles.IsEmpty() && !n.IsInvisible && !playerBounds.Intersects(actorBounds) && !n.farmerPassesThrough)
			{
				return true;
			}
		}
		if (Game1.currentLocation.IsOutOfBounds(position))
		{
			this.TryStartEndFestivalDialogue(who);
			return true;
		}
		foreach (Object prop in this.props)
		{
			if (prop.GetBoundingBox().Intersects(position))
			{
				return true;
			}
		}
		if (this.temporaryLocation != null)
		{
			foreach (Object value in this.temporaryLocation.objects.Values)
			{
				if (value.GetBoundingBox().Intersects(position))
				{
					return true;
				}
			}
		}
		foreach (Prop festivalProp in this.festivalProps)
		{
			if (festivalProp.isColliding(position))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Show the dialogue to end the current festival when the player tries to leave the location.</summary>
	/// <param name="who">The local player instance.</param>
	/// <returns>Returns whether the dialogue was displayed.</returns>
	public bool TryStartEndFestivalDialogue(Farmer who)
	{
		if (!who.IsLocalPlayer || !this.isFestival)
		{
			return false;
		}
		who.Halt();
		who.Position = who.lastPosition;
		if (!Game1.IsMultiplayer && Game1.activeClickableMenu == null)
		{
			Game1.activeClickableMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1758", this.FestivalName), forceEndFestival);
		}
		else if (Game1.activeClickableMenu == null)
		{
			Game1.netReady.SetLocalReady("festivalEnd", ready: true);
			Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", allowCancel: true, forceEndFestival);
		}
		return true;
	}

	public void answerDialogue(string questionKey, int answerChoice)
	{
		this.previousAnswerChoice = answerChoice;
		if (questionKey.Contains("fork"))
		{
			int forkAnswer = Convert.ToInt32(questionKey.Replace("fork", ""));
			if (answerChoice == forkAnswer)
			{
				this.specialEventVariable1 = !this.specialEventVariable1;
			}
		}
		else if (questionKey.Contains("quickQuestion"))
		{
			string obj = this.eventCommands[Math.Min(this.eventCommands.Length - 1, this.CurrentCommand)];
			string[] newCommands = obj.Substring(obj.IndexOf(' ') + 1).Split("(break)")[1 + answerChoice].Split('\\');
			List<string> tmp = this.eventCommands.ToList();
			tmp.InsertRange(this.CurrentCommand + 1, newCommands);
			this.eventCommands = tmp.ToArray();
		}
		else
		{
			if (questionKey == null)
			{
				return;
			}
			switch (questionKey.Length)
			{
			case 11:
				switch (questionKey[1])
				{
				case 'h':
					if (questionKey == "shaneCliffs")
					{
						switch (answerChoice)
						{
						case 0:
							this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1760") + "\"";
							break;
						case 1:
							this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1761") + "\"";
							break;
						case 2:
							this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1763") + "\"";
							break;
						case 3:
							this.eventCommands[this.currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1764") + "\"";
							break;
						}
					}
					break;
				case 'i':
					if (questionKey == "fishingGame" && answerChoice == 0)
					{
						if (Game1.player.Money >= 50)
						{
							Game1.globalFadeToBlack(FishingGame.startMe, 0.01f);
							Game1.player.Money -= 50;
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
						}
					}
					break;
				case 'e':
					if (questionKey == "secretSanta" && answerChoice == 0)
					{
						Game1.activeClickableMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightSantaObjects, chooseSecretSantaGift, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1788", this.secretSantaRecipient.displayName), null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
					}
					break;
				case 'f':
				case 'g':
					break;
				}
				break;
			case 13:
				switch (questionKey[0])
				{
				case 'h':
					if (questionKey == "haleyDarkRoom")
					{
						switch (answerChoice)
						{
						case 0:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork decorate";
							break;
						case 1:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork leave";
							break;
						case 2:
							break;
						}
					}
					break;
				case 'S':
					if (questionKey == "StarTokenShop" && answerChoice == 0)
					{
						Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1774"), buyStarTokens, 50, 0, 999);
					}
					break;
				case 'f':
					if (questionKey == "fortuneTeller" && answerChoice == 0)
					{
						Game1.globalFadeToBlack(readFortune);
						Game1.player.Money -= 100;
						Game1.player.mailReceived.Add("fortuneTeller" + Game1.year);
					}
					break;
				case 's':
					if (!(questionKey == "slingshotGame"))
					{
						if (questionKey == "starTokenShop" && answerChoice == 0 && Utility.TryOpenShopMenu("Festival_StardewValleyFair_StarTokens", this.temporaryLocation, null, null, forceOpen: false, playOpenSound: false) && Game1.activeClickableMenu is ShopMenu shop)
						{
							if (shop.IsOutOfStock())
							{
								shop.exitThisMenuNoSound();
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1785")));
							}
							else
							{
								shop.PlayOpenSound();
							}
						}
					}
					else if (answerChoice == 0)
					{
						if (Game1.player.Money >= 50)
						{
							Game1.globalFadeToBlack(TargetGame.startMe, 0.01f);
							Game1.player.Money -= 50;
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
						}
					}
					break;
				}
				break;
			case 8:
				switch (questionKey[0])
				{
				case 'b':
					if (questionKey == "bandFork")
					{
						switch (answerChoice)
						{
						case 76:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork poppy";
							break;
						case 77:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork heavy";
							break;
						case 78:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork techno";
							break;
						case 79:
							this.specialEventVariable1 = true;
							this.eventCommands[this.currentCommand + 1] = "fork honkytonk";
							break;
						}
					}
					break;
				case 'w':
					if (questionKey == "wheelBet")
					{
						this.specialEventVariable2 = answerChoice == 1;
						if (answerChoice != 2)
						{
							Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1776"), betStarTokens, -1, 1, Game1.player.festivalScore, Math.Min(1, Game1.player.festivalScore));
						}
					}
					break;
				}
				break;
			case 20:
				if (questionKey == "spirits_eve_shortcut" && answerChoice == 0)
				{
					Game1.player.freezePause = 2000;
					Game1.globalFadeToBlack(delegate
					{
						Game1.player.Position = new Vector2(32f, 49f) * 64f;
						Game1.player.faceDirection(2);
						Game1.playSound("stairsdown");
						Game1.globalFadeToClear();
					});
				}
				break;
			case 9:
				if (questionKey == "shaneLoan")
				{
					if (answerChoice != 0)
					{
						_ = 1;
						break;
					}
					this.specialEventVariable1 = true;
					this.eventCommands[this.currentCommand + 1] = "fork giveShaneLoan";
					Game1.player.Money -= 3000;
				}
				break;
			case 15:
				if (questionKey == "chooseCharacter")
				{
					switch (answerChoice)
					{
					case 0:
						this.specialEventVariable1 = true;
						this.eventCommands[this.currentCommand + 1] = "fork warrior";
						break;
					case 1:
						this.specialEventVariable1 = true;
						this.eventCommands[this.currentCommand + 1] = "fork healer";
						break;
					case 2:
						break;
					}
				}
				break;
			case 4:
				if (questionKey == "cave")
				{
					Game1.dedicatedServer.DoHostAction("ChooseCave", answerChoice);
				}
				break;
			case 3:
				if (questionKey == "pet")
				{
					if (answerChoice == 0)
					{
						string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
						Game1.activeClickableMenu = new NamingMenu(defaultName: (!Game1.player.IsMale) ? Game1.content.LoadString((Game1.player.whichPetType == "Dog") ? "Strings\\StringsFromCSFiles:Event.cs.1797" : "Strings\\StringsFromCSFiles:Event.cs.1796") : Game1.content.LoadString(Game1.player.catPerson ? "Strings\\StringsFromCSFiles:Event.cs.1794" : "Strings\\StringsFromCSFiles:Event.cs.1795"), b: namePet, title: title);
						break;
					}
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "rejectedPet", MailType.Received, add: true);
					this.eventCommands = new string[2];
					this.eventCommands[1] = "end";
					this.eventCommands[0] = "speak Marnie \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1798") + "\"";
					this.currentCommand = 0;
					this.eventSwitched = true;
					this.specialEventVariable1 = true;
				}
				break;
			}
		}
	}

	internal static void hostActionChooseCave(Farmer who, BinaryReader reader)
	{
		if (reader.ReadInt32() == 0)
		{
			Game1.MasterPlayer.caveChoice.Value = 2;
			Game1.RequireLocation<FarmCave>("FarmCave").setUpMushroomHouse();
		}
		else
		{
			Game1.MasterPlayer.caveChoice.Value = 1;
		}
	}

	internal static void hostActionNamePet(Farmer who, BinaryReader reader)
	{
		string name = reader.ReadString();
		Pet p = new Pet(68, 13, Game1.player.whichPetBreed, Game1.player.whichPetType);
		p.warpToFarmHouse(Game1.player);
		p.Name = name;
		p.displayName = p.name.Value;
		foreach (Building building in Game1.getFarm().buildings)
		{
			if (building is PetBowl bowl && !bowl.HasPet())
			{
				bowl.AssignPet(p);
				break;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.autoGenerateActiveDialogueEvent("gotPet");
		}
	}

	private void namePet(string name)
	{
		this.gotPet = true;
		Game1.dedicatedServer.DoHostAction("NamePet", name);
		Game1.exitActiveMenu();
		this.CurrentCommand++;
	}

	public void chooseSecretSantaGift(Item i, Farmer who)
	{
		if (i == null)
		{
			return;
		}
		Object obj = i as Object;
		if (obj != null)
		{
			if (obj.Stack > 1)
			{
				obj.Stack--;
				who.addItemToInventory(obj);
			}
			Game1.exitActiveMenu();
			NPC recipient = this.getActorByName(this.secretSantaRecipient.Name);
			recipient.faceTowardFarmerForPeriod(15000, 5, faceAway: false, who);
			recipient.receiveGift(obj, who, updateGiftLimitInfo: false, 5f, showResponse: false);
			recipient.CurrentDialogue.Clear();
			string article = Lexicon.getProperArticleForWord(obj.DisplayName);
			recipient.CurrentDialogue.Push(recipient.TryGetDialogue("WinterStar_ReceiveGift_" + obj.QualifiedItemId, obj.DisplayName, article) ?? (from tag in obj.GetContextTags()
				select recipient.TryGetDialogue("WinterStar_ReceiveGift_" + tag, obj.DisplayName, article)).FirstOrDefault((Dialogue p) => p != null) ?? recipient.TryGetDialogue("WinterStar_ReceiveGift", obj.DisplayName, article) ?? Dialogue.FromTranslation(recipient, "Strings\\StringsFromCSFiles:Event.cs.1801", obj.DisplayName, article));
			Game1.drawDialogue(recipient);
			this.secretSantaRecipient = null;
			this.startSecretSantaAfterDialogue = true;
			who.Halt();
			who.completelyStopAnimatingOrDoingAction();
			who.faceGeneralDirection(recipient.Position, 0, opposite: false, useTileCalculations: false);
		}
		else
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1803"));
		}
	}

	public void perfectFishing()
	{
		if (this.isFestival && Game1.currentMinigame is FishingGame fishingGame && this.isSpecificFestival("fall16"))
		{
			fishingGame.perfections++;
		}
	}

	public void caughtFish(string itemId, int size, Farmer who)
	{
		if (itemId == null || !this.isFestival)
		{
			return;
		}
		if (Game1.currentMinigame is FishingGame fishingGame && this.isSpecificFestival("fall16"))
		{
			fishingGame.score += ((size <= 0) ? 1 : (size + 5));
			if (size > 0)
			{
				fishingGame.fishCaught++;
			}
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			Game1.player.FarmerSprite.StopAnimation();
		}
		else if (this.isSpecificFestival("winter8"))
		{
			if (size > 0 && who.TilePoint.X < 79 && who.TilePoint.Y < 43)
			{
				who.festivalScore++;
				Game1.playSound("newArtifact");
			}
			who.forceCanMove();
			if (this.previousFacingDirection != -1)
			{
				who.faceDirection(this.previousFacingDirection);
			}
		}
	}

	public void readFortune()
	{
		Game1.globalFade = true;
		Game1.fadeToBlackAlpha = 1f;
		NPC topRomance = Utility.getTopRomanticInterest(Game1.player);
		NPC topFriend = Utility.getTopNonRomanticInterest(Game1.player);
		int topSkill = Utility.getHighestSkill(Game1.player);
		string[] fortune = new string[5];
		if (topFriend != null && Game1.player.getFriendshipLevelForNPC(topFriend.Name) > 100)
		{
			if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topFriend.Name) - 100, Game1.player.getFriendshipLevelForNPC(topFriend.Name)) > 3 && Game1.random.NextBool())
			{
				fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1810");
			}
			else
			{
				switch (Game1.random.Next(4))
				{
				case 0:
					fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1811", topFriend.displayName);
					break;
				case 1:
					fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1813", topFriend.displayName) + ((topFriend.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1815") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1816"));
					break;
				case 2:
					fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1818", topFriend.displayName);
					break;
				case 3:
					fortune[0] = ((topFriend.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1820") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1821")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1823", topFriend.displayName);
					break;
				}
			}
		}
		else
		{
			fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1825");
		}
		if (topRomance != null && Game1.player.getFriendshipLevelForNPC(topRomance.Name) > 250)
		{
			if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topRomance.Name) - 100, Game1.player.getFriendshipLevelForNPC(topRomance.Name), romanceOnly: true) > 2)
			{
				fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826");
			}
			else
			{
				switch (Game1.random.Next(4))
				{
				case 0:
					fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomance.displayName);
					break;
				case 1:
					fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1829", topRomance.displayName);
					break;
				case 2:
					fortune[1] = ((topRomance.Gender != Gender.Male) ? ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) : ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832"))) + " " + ((topRomance.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", topRomance.displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", topRomance.displayName[0]));
					break;
				case 3:
					fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", topRomance.displayName);
					break;
				}
			}
		}
		else
		{
			fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1845");
		}
		switch (topSkill)
		{
		case 0:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1846");
			break;
		case 3:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1847");
			break;
		case 4:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1848");
			break;
		case 1:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1849");
			break;
		case 2:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1850");
			break;
		case 5:
			fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1851");
			break;
		}
		fortune[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1852");
		fortune[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1853");
		Game1.multipleDialogues(fortune);
		Game1.afterDialogues = fadeClearAndviewportUnfreeze;
		Game1.viewportFreeze = true;
		Game1.viewport.X = -9999;
	}

	public void fadeClearAndviewportUnfreeze()
	{
		Game1.fadeClear();
		Game1.viewportFreeze = false;
	}

	public void betStarTokens(int value, int price, Farmer who)
	{
		if (value <= who.festivalScore)
		{
			Game1.playSound("smallSelect");
			Game1.activeClickableMenu = new WheelSpinGame(value);
		}
	}

	public void buyStarTokens(int value, int price, Farmer who)
	{
		if (value > 0 && value * price <= who.Money)
		{
			who.Money -= price * value;
			who.festivalScore += value;
			Game1.playSound("purchase");
			Game1.exitActiveMenu();
		}
	}

	public void clickToAddItemToLuauSoup(Item i, Farmer who)
	{
		this.addItemToLuauSoup(i, who);
	}

	public void setUpAdvancedMove(string[] args, NPCController.endBehavior endBehavior = null)
	{
		if (!ArgUtility.TryGet(args, 1, out var actorName, out var error, allowBlank: false, "string actorName") || !ArgUtility.TryGetBool(args, 2, out var loop, out error, "bool loop"))
		{
			this.LogCommandError(args, error);
			return;
		}
		List<Vector2> path = new List<Vector2>();
		for (int i = 3; i < args.Length; i += 2)
		{
			if (ArgUtility.TryGetVector2(args, i, out var tile, out error, integerOnly: true, "Vector2 tile"))
			{
				path.Add(tile);
			}
			else
			{
				this.LogCommandError(args, error);
			}
		}
		if (this.npcControllers == null)
		{
			this.npcControllers = new List<NPCController>();
		}
		if (this.IsFarmerActorId(actorName, out var farmerNumber))
		{
			Farmer f = this.GetFarmerActor(farmerNumber);
			if (f != null)
			{
				this.npcControllers.Add(new NPCController(f, path, loop, endBehavior));
			}
		}
		else
		{
			NPC n = this.getActorByName(actorName, legacyReplaceUnderscores: true);
			if (n != null)
			{
				this.npcControllers.Add(new NPCController(n, path, loop, endBehavior));
			}
		}
	}

	public static bool IsItemMayorShorts(Item i)
	{
		if (!(i?.QualifiedItemId == "(O)789"))
		{
			return i?.QualifiedItemId == "(O)71";
		}
		return true;
	}

	public void addItemToLuauSoup(Item i, Farmer who)
	{
		if (i == null)
		{
			return;
		}
		who.team.luauIngredients.Add(i.getOne());
		if (who.IsLocalPlayer)
		{
			this.specialEventVariable2 = true;
			bool is_shorts = Event.IsItemMayorShorts(i);
			if (i != null && i.Stack > 1 && !is_shorts)
			{
				i.Stack--;
				who.addItemToInventory(i);
			}
			else if (is_shorts)
			{
				who.addItemToInventory(i);
			}
			Game1.exitActiveMenu();
			Game1.playSound("dropItemInWater");
			if (i != null)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1857", i.DisplayName));
			}
			string qualityString = "";
			switch (i.Quality)
			{
			case 1:
				qualityString = " ([51])";
				break;
			case 2:
				qualityString = " ([52])";
				break;
			case 4:
				qualityString = " ([53])";
				break;
			}
			if (!is_shorts)
			{
				Game1.multiplayer.globalChatInfoMessage("LuauSoup", Game1.player.Name, TokenStringBuilder.ItemNameFor(i) + qualityString);
			}
		}
	}

	private void governorTaste()
	{
		int likeLevel = 5;
		foreach (Item luauIngredient in Game1.player.team.luauIngredients)
		{
			Object o = luauIngredient as Object;
			int itemLevel = 5;
			if (Event.IsItemMayorShorts(o))
			{
				likeLevel = 6;
				break;
			}
			if ((o.Quality >= 2 && o.price.Value >= 160) || (o.Quality == 1 && o.price.Value >= 300 && o.edibility.Value > 10))
			{
				itemLevel = 4;
				Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 120, "Town");
			}
			else if (o.edibility.Value >= 20 || o.price.Value >= 100 || (o.price.Value >= 70 && o.Quality >= 1))
			{
				itemLevel = 3;
				Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 60, "Town");
			}
			else if ((o.price.Value > 20 && o.edibility.Value >= 10) || (o.price.Value >= 40 && o.edibility.Value >= 5))
			{
				itemLevel = 2;
			}
			else if (o.edibility.Value >= 0)
			{
				itemLevel = 1;
				Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -50, "Town");
			}
			if (o.edibility.Value > -300 && o.edibility.Value < 0)
			{
				itemLevel = 0;
				Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -100, "Town");
			}
			if (itemLevel < likeLevel)
			{
				likeLevel = itemLevel;
			}
		}
		int numPlayers = Game1.numberOfPlayers() - (Game1.HasDedicatedHost ? 1 : 0);
		if (likeLevel != 6 && Game1.player.team.luauIngredients.Count < numPlayers)
		{
			likeLevel = 5;
		}
		this.eventCommands[this.CurrentCommand + 1] = "switchEvent governorReaction" + likeLevel;
		if (likeLevel == 4)
		{
			Game1.getAchievement(38);
		}
	}

	private void eggHuntWinner()
	{
		int numberOfEggsToWin = (Game1.numberOfPlayers() - (Game1.HasDedicatedHost ? 1 : 0)) switch
		{
			1 => 9, 
			2 => 6, 
			3 => 5, 
			_ => 4, 
		};
		List<Farmer> winners = new List<Farmer>();
		int mostEggsScore = Game1.player.festivalScore;
		foreach (Farmer temp in Game1.getOnlineFarmers())
		{
			if (temp.festivalScore > mostEggsScore)
			{
				mostEggsScore = temp.festivalScore;
			}
		}
		foreach (Farmer temp2 in Game1.getOnlineFarmers())
		{
			if (temp2.festivalScore == mostEggsScore)
			{
				winners.Add(temp2);
				this.festivalWinners.Add(temp2.UniqueMultiplayerID);
			}
		}
		string winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1862");
		if (mostEggsScore >= numberOfEggsToWin)
		{
			foreach (Farmer item in winners)
			{
				item.autoGenerateActiveDialogueEvent("wonEggHunt");
			}
			if (winners.Count == 1)
			{
				winnerDialogue = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? ("¡" + winners[0].displayName + "!") : (winners[0].displayName + "!"));
			}
			else
			{
				winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
				for (int i = 0; i < winners.Count; i++)
				{
					if (i == winners.Count - 1)
					{
						winnerDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
					}
					winnerDialogue = winnerDialogue + " " + winners[i].displayName;
					if (i < winners.Count - 1)
					{
						winnerDialogue += ",";
					}
				}
				winnerDialogue += "!";
			}
			this.specialEventVariable1 = false;
		}
		else
		{
			this.specialEventVariable1 = true;
		}
		NPC lewis = this.getActorByName("Lewis");
		lewis.CurrentDialogue.Push(new Dialogue(lewis, null, winnerDialogue));
		Game1.drawDialogue(lewis);
	}

	private void iceFishingWinner()
	{
		int numberOfFishToWin = 5;
		this.iceFishWinners = new List<Farmer>();
		int mostFishScore = Game1.player.festivalScore;
		for (int i = 1; i <= Game1.numberOfPlayers(); i++)
		{
			Farmer temp = this.GetFarmerActor(i);
			if (temp != null && temp.festivalScore > mostFishScore)
			{
				mostFishScore = temp.festivalScore;
			}
		}
		for (int j = 1; j <= Game1.numberOfPlayers(); j++)
		{
			Farmer temp2 = this.GetFarmerActor(j);
			if (temp2 != null && temp2.festivalScore == mostFishScore)
			{
				this.iceFishWinners.Add(temp2);
				this.festivalWinners.Add(temp2.UniqueMultiplayerID);
			}
		}
		string winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1871");
		if (mostFishScore >= numberOfFishToWin)
		{
			foreach (Farmer iceFishWinner in this.iceFishWinners)
			{
				iceFishWinner.autoGenerateActiveDialogueEvent("wonIceFishing");
			}
			if (this.iceFishWinners.Count == 1)
			{
				winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1872", this.iceFishWinners[0].displayName, this.iceFishWinners[0].festivalScore);
			}
			else
			{
				winnerDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
				for (int k = 0; k < this.iceFishWinners.Count; k++)
				{
					if (k == this.iceFishWinners.Count - 1)
					{
						winnerDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
					}
					winnerDialogue = winnerDialogue + " " + this.iceFishWinners[k].displayName;
					if (k < this.iceFishWinners.Count - 1)
					{
						winnerDialogue += ",";
					}
				}
				winnerDialogue += "!";
			}
			this.specialEventVariable1 = false;
		}
		else
		{
			this.specialEventVariable1 = true;
		}
		NPC lewis = this.getActorByName("Lewis");
		lewis.CurrentDialogue.Push(new Dialogue(lewis, null, winnerDialogue));
		Game1.drawDialogue(lewis);
	}

	private void iceFishingWinnerMP()
	{
		this.specialEventVariable1 = !this.iceFishWinners.Contains(Game1.player);
	}

	public void popBalloons(int x, int y)
	{
		if ((!this.id.Equals("191393") && !this.id.Equals("502261")) || this.aboveMapSprites == null)
		{
			return;
		}
		List<int> idsToRemove = new List<int>();
		for (int i = this.aboveMapSprites.Count - 1; i >= 0; i--)
		{
			TemporaryAnimatedSprite t = this.aboveMapSprites[i];
			int width = t.sourceRect.Width * 4;
			int height = t.sourceRect.Height * 4;
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)t.Position.X, (int)t.Position.Y, width, height);
			if (r.Contains(x, y))
			{
				idsToRemove.Add(t.id);
				if (t.sourceRect.Height <= 16)
				{
					for (int z = 0; z < 3; z++)
					{
						this.aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(280 + Game1.random.Choose(8, 0), 1954, 8, 8), 1000f, 1, 99, Utility.getRandomPositionInThisRectangle(r, Game1.random), flicker: false, flipped: false, 1f, 0f, t.color, 4f, 0f, 0f, (float)Game1.random.Next(-10, 11) / 100f)
						{
							motion = new Vector2(Game1.random.Next(-4, 5), -8f + (float)Game1.random.Next(-10, 1) / 100f),
							acceleration = new Vector2(0f, 0.3f),
							local = true
						});
					}
				}
			}
		}
		this.aboveMapSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == 9988 || idsToRemove.Contains(sprite.id));
		if (idsToRemove.Count > 0)
		{
			this.int_useMeForAnything++;
			this.aboveMapSprites.Add(new TemporaryAnimatedSprite(null, Microsoft.Xna.Framework.Rectangle.Empty, new Vector2(16f, 16f), flipped: false, 0f, Color.White)
			{
				text = (this.int_useMeForAnything.ToString() ?? ""),
				layerDepth = 1f,
				animationLength = 1,
				totalNumberOfLoops = 10,
				interval = 300f,
				scale = 2f,
				local = true,
				id = 9988
			});
			Game1.playSound("coin");
		}
	}

	/// <summary>Auto-generate a default light source ID for an event.</summary>
	/// <param name="suffix">A suffix which distinguishes the specific light source for this event.</param>
	public virtual string GenerateLightSourceId(string suffix)
	{
		return $"{"Event"}_{this.id}_{suffix}";
	}
}
