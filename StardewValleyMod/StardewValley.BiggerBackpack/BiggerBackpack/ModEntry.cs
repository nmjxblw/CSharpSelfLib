using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewHack;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;

namespace BiggerBackpack;

public class ModEntry : HackWithConfig<ModEntry, ModConfig>
{
	public static ModEntry instance;

	private static Texture2D bigBackpack;

	public override void HackEntry(IModHelper helper)
	{
		I18n.Init(helper.Translation);
		ModEntry.bigBackpack = ((Mod)this).Helper.ModContent.Load<Texture2D>("backpack.png");
		helper.Events.Content.AssetRequested += OnAssetRequested;
		((Mod)this).Helper.ConsoleCommands.Add("player_setbackpacksize", I18n.SetBackpackSizeCommand(), (Action<string, string[]>)command);
		((HackBase)this).Patch<SeedShop>((Expression<Action<SeedShop>>)((SeedShop s) => ((GameLocation)s).draw((SpriteBatch)null)), (Action)SeedShop_draw);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(SpecialItem), "si");
		((HackBase)this).Patch<SpecialItem>(Expression.Lambda<Action<SpecialItem>>(Expression.Call(parameterExpression, (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/), Expression.New(typeof(Vector2))), new ParameterExpression[1] { parameterExpression }), (Action)SpecialItem_getTemporarySpriteForHoldingUp);
		parameterExpression = Expression.Parameter(typeof(GameLocation), "gl");
		((HackBase)this).Patch<GameLocation>(Expression.Lambda<Action<GameLocation>>(Expression.Call(parameterExpression, (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/), new Expression[3]
		{
			Expression.NewArrayBounds(typeof(string), Expression.Constant(0, typeof(int))),
			Expression.Constant(null, typeof(Farmer)),
			Expression.New(typeof(Location))
		}), new ParameterExpression[1] { parameterExpression }), (Action)GameLocation_performAction);
		((HackBase)this).Patch<GameLocation>((Expression<Action<GameLocation>>)((GameLocation gl) => gl.answerDialogueAction("", (string[])null)), (Action)GameLocation_answerDialogueAction);
		((HackBase)this).Patch((Expression<Action>)(() => new InventoryPage(0, 0, 0, 0)), (Action)InventoryPage_ctor);
		((HackBase)this).Patch<InventoryPage>((Expression<Action<InventoryPage>>)((InventoryPage ip) => ((IClickableMenu)ip).draw((SpriteBatch)null)), (Action)InventoryPage_draw);
		((HackBase)this).Patch((Expression<Action>)(() => new CraftingPage(0, 0, 0, 0, false, false, (List<IInventory>)null)), (Action)CraftingPage_ctor);
		((HackBase)this).Patch(typeof(ShopMenu), "Initialize", (Action)ShopMenu_Initialize);
		((HackBase)this).Patch<ShopMenu>((Expression<Action<ShopMenu>>)((ShopMenu m) => ((IClickableMenu)m).draw((SpriteBatch)null)), (Action)ShopMenu_draw);
		((HackBase)this).Patch<ShopMenu>((Expression<Action<ShopMenu>>)((ShopMenu m) => m.drawCurrency((SpriteBatch)null)), (Action)ShopMenu_drawCurrency);
		((HackBase)this).Patch((Expression<Action>)(() => new MenuWithInventory((highlightThisItem)null, false, false, 0, 0, 0, (ItemExitBehavior)2, false)), (Action)ShippingMenu_ctor);
		((HackBase)this).Patch<JunimoNoteMenu>((Expression<Action<JunimoNoteMenu>>)((JunimoNoteMenu m) => m.setUpMenu(0, (Dictionary<int, bool[]>)null)), (Action)JunimoNoteMenu_setUpMenu);
	}

	private void command(string cmd, string[] args)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (args.Length != 1)
		{
			((Mod)this).Monitor.Log("Must have one command argument", (LogLevel)4);
			return;
		}
		int newMax;
		switch (args[0])
		{
		case "12":
			newMax = 12;
			break;
		case "24":
			newMax = 24;
			break;
		case "36":
			newMax = 36;
			break;
		case "48":
			newMax = 48;
			break;
		default:
			((Mod)this).Monitor.Log("The new size must be 12, 24, 36 or 48.", (LogLevel)4);
			return;
		}
		if (newMax < Game1.player.MaxItems)
		{
			for (int i = Game1.player.MaxItems - 1; i >= newMax; i--)
			{
				if (Game1.player.Items[i] != null)
				{
					Game1.createItemDebris(Game1.player.Items[i], ((Character)Game1.player).getStandingPosition(), ((Character)Game1.player).getDirection(), (GameLocation)null, -1, false);
				}
				Game1.player.Items.RemoveAt(i);
			}
		}
		else
		{
			for (int j = Game1.player.Items.Count; j < Game1.player.MaxItems; j++)
			{
				Game1.player.Items.Add((Item)null);
			}
		}
		Game1.player.MaxItems = newMax;
	}

	public static void drawBiggerBackpack(SpriteBatch b)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		b.Draw(ModEntry.bigBackpack, Game1.GlobalToLocal(new Vector2(456f, 1088f)), (Rectangle?)new Rectangle(0, 0, 12, 14), Color.White, 0f, Vector2.Zero, 4f, (SpriteEffects)0, 0.1232f);
	}

	private void SeedShop_draw()
	{
		InstructionRange check = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(Instructions.Call_get(typeof(Game1), "player")),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(Farmer), "maxItems")),
			InstructionMatcher.op_Implicit(OpCodes.Call),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36)),
			InstructionMatcher.op_Implicit(OpCodes.Bge)
		});
		InstructionRange pos = check.Follow(4);
		if ((!(pos[-1].opcode == OpCodes.Ret) && !(pos[-1].opcode == OpCodes.Br)) || !(pos[0].opcode == OpCodes.Ldarg_1))
		{
			throw new Exception("Jump does not go to expected location.");
		}
		pos.Insert(0, (CodeInstruction[])(object)new CodeInstruction[7]
		{
			Instructions.Call_get(typeof(Game1), "player"),
			Instructions.Ldfld(typeof(Farmer), "maxItems"),
			check[2],
			Instructions.Ldc_I4_S((sbyte)48),
			check[4],
			Instructions.Ldarg_1(),
			Instructions.Call(typeof(ModEntry), "drawBiggerBackpack", new Type[1] { typeof(SpriteBatch) })
		});
		check[4] = Instructions.Bge(((HackBase)this).AttachLabel(pos[0]));
	}

	public static TemporaryAnimatedSprite getBackpackSprite(Vector2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		return new TemporaryAnimatedSprite((string)null, new Rectangle(1, 0, 11, 13), position + new Vector2(16f, 0f), false, 0f, Color.White)
		{
			scale = 4f,
			layerDepth = 1f,
			texture = ModEntry.bigBackpack
		};
	}

	private void SpecialItem_getTemporarySpriteForHoldingUp()
	{
		InstructionRange code = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(Instructions.Ldstr("LooseSprites\\Cursors")),
			InstructionMatcher.op_Implicit(Instructions.Call_get(typeof(Game1), "player")),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(Farmer), "maxItems")),
			InstructionMatcher.op_Implicit(OpCodes.Call),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36))
		});
		code.Prepend((CodeInstruction[])(object)new CodeInstruction[8]
		{
			Instructions.Call_get(typeof(Game1), "player"),
			Instructions.Ldfld(typeof(Farmer), "maxItems"),
			code[3],
			Instructions.Ldc_I4_S((sbyte)48),
			Instructions.Bne_Un(((HackBase)this).AttachLabel(code[0])),
			Instructions.Ldarg_1(),
			Instructions.Call(typeof(ModEntry), "getBackpackSprite", new Type[1] { typeof(Vector2) }),
			Instructions.Ret()
		});
	}

	public static void clickBackpack()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		Response yes = new Response("Purchase", I18n.Purchase(ModEntry.getBackpackCost()));
		Response no = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
		Response[] resps = (Response[])(object)new Response[2] { yes, no };
		Game1.currentLocation.createQuestionDialogue(I18n.BackpackUpgrade(), resps, "Backpack");
	}

	private void GameLocation_performAction()
	{
		InstructionRange code = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(Instructions.Call_get(typeof(Game1), "player")),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(Farmer), "maxItems")),
			InstructionMatcher.op_Implicit(OpCodes.Call),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36)),
			InstructionMatcher.op_Implicit(OpCodes.Bge)
		});
		code.Extend((InstructionMatcher[])(object)new InstructionMatcher[3]
		{
			InstructionMatcher.op_Implicit(Instructions.Ldstr("Backpack")),
			InstructionMatcher.op_Implicit(OpCodes.Call),
			InstructionMatcher.op_Implicit(OpCodes.Br)
		});
		int len = code.length;
		code.Append((CodeInstruction[])(object)new CodeInstruction[7]
		{
			Instructions.Call_get(typeof(Game1), "player"),
			Instructions.Ldfld(typeof(Farmer), "maxItems"),
			code[2],
			Instructions.Ldc_I4_S((sbyte)48),
			code[4],
			Instructions.Call(typeof(ModEntry), "clickBackpack", Array.Empty<Type>()),
			Instructions.Br((Label)code[len - 1].operand)
		});
		code[4] = Instructions.Bge(((HackBase)this).AttachLabel(code[len]));
	}

	private static void buyBackpack()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		Farmer player = Game1.player;
		player.Money -= ModEntry.getBackpackCost();
		Game1.player.holdUpItemThenMessage((Item)new SpecialItem(99, "Premium Pack"), true);
		Game1.player.increaseBackpackSize(12);
	}

	public static int getBackpackCost()
	{
		return ((HackWithConfig<ModEntry, ModConfig>)HackImpl<ModEntry>.getInstance()).config.BackpackCost;
	}

	private void GameLocation_answerDialogueAction()
	{
		InstructionRange code = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(Instructions.Call_get(typeof(Game1), "player")),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(Farmer), "maxItems")),
			InstructionMatcher.op_Implicit(OpCodes.Call),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36)),
			InstructionMatcher.op_Implicit(OpCodes.Beq)
		});
		CodeInstruction get_player = Instructions.Call_get(typeof(Game1), "player");
		code.Replace((CodeInstruction[])(object)new CodeInstruction[15]
		{
			Instructions.Call_get(typeof(Game1), "player"),
			Instructions.Ldfld(typeof(Farmer), "maxItems"),
			code[2],
			Instructions.Ldc_I4_S((sbyte)48),
			Instructions.Bge(((HackBase)this).AttachLabel(get_player)),
			Instructions.Call_get(typeof(Game1), "player"),
			Instructions.Callvirt_get(typeof(Farmer), "Money"),
			Instructions.Call(((object)this).GetType(), "getBackpackCost", Array.Empty<Type>()),
			Instructions.Blt(((HackBase)this).AttachLabel(get_player)),
			Instructions.Call(typeof(ModEntry), "buyBackpack", Array.Empty<Type>()),
			get_player,
			Instructions.Ldfld(typeof(Farmer), "maxItems"),
			code[2],
			Instructions.Ldc_I4_S((sbyte)48),
			code[4]
		});
	}

	public static void shiftIconsDown(List<ClickableComponent> equipmentIcons)
	{
		foreach (ClickableComponent icon in equipmentIcons)
		{
			icon.bounds.Y += 64;
		}
	}

	private void resize_inventory()
	{
		InstructionRange inv = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_M1),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_3),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_0),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_0),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_1)
		});
		inv[0] = Instructions.Ldc_I4(48);
		inv[1] = Instructions.Ldc_I4_4();
	}

	private void InventoryPage_ctor()
	{
		((HackBase)this).BeginCode().Prepend((CodeInstruction[])(object)new CodeInstruction[4]
		{
			Instructions.Ldarg_S((byte)4),
			Instructions.Ldc_I4_S((sbyte)64),
			Instructions.Add(),
			Instructions.Starg_S((byte)4)
		});
		this.resize_inventory();
		((HackBase)this).EndCode().Insert(-1, (CodeInstruction[])(object)new CodeInstruction[3]
		{
			Instructions.Ldarg_0(),
			Instructions.Ldfld(typeof(InventoryPage), "equipmentIcons"),
			Instructions.Call(typeof(ModEntry), "shiftIconsDown", new Type[1] { typeof(List<ClickableComponent>) })
		});
		((HackBase)this).EndCode().ReplaceJump(-1, ((HackBase)this).EndCode()[-4]);
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[12]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "yPositionOnScreen")),
			InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "borderWidth")),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "spaceToClearTopBorder")),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(256)),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_8()),
			InstructionMatcher.op_Implicit(OpCodes.Sub),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)64)),
			InstructionMatcher.op_Implicit(OpCodes.Add)
		})[6].operand = 320;
	}

	private void InventoryPage_draw()
	{
		InstructionRange code = ((HackBase)this).BeginCode();
		LocalBuilder yoffset = ((HackBase)this).generator.DeclareLocal(typeof(int));
		code.Prepend((CodeInstruction[])(object)new CodeInstruction[9]
		{
			Instructions.Ldarg_0(),
			Instructions.Ldfld(typeof(IClickableMenu), "yPositionOnScreen"),
			Instructions.Ldsfld(typeof(IClickableMenu), "borderWidth"),
			Instructions.Add(),
			Instructions.Ldsfld(typeof(IClickableMenu), "spaceToClearTopBorder"),
			Instructions.Add(),
			Instructions.Ldc_I4_S((sbyte)64),
			Instructions.Add(),
			Instructions.Stloc_S(yoffset)
		});
		int yoffset_counter = 0;
		try
		{
			while (true)
			{
				code = code.FindNext((InstructionMatcher[])(object)new InstructionMatcher[6]
				{
					InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
					InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "yPositionOnScreen")),
					InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "borderWidth")),
					InstructionMatcher.op_Implicit(OpCodes.Add),
					InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "spaceToClearTopBorder")),
					InstructionMatcher.op_Implicit(OpCodes.Add)
				});
				code.Replace((CodeInstruction[])(object)new CodeInstruction[1] { Instructions.Ldloc_S(yoffset) });
				yoffset_counter++;
			}
		}
		catch (InstructionNotFoundException)
		{
			switch (yoffset_counter)
			{
			case 0:
				throw;
			case 9:
				return;
			}
			((Mod)this).Monitor.Log($"Replaced yoffset {yoffset_counter} times instead of expected 9 times.", (LogLevel)3);
		}
	}

	private void CraftingPage_ctor()
	{
		((HackBase)this).BeginCode().Prepend((CodeInstruction[])(object)new CodeInstruction[4]
		{
			Instructions.Ldarg_S((byte)4),
			Instructions.Ldc_I4_S((sbyte)64),
			Instructions.Add(),
			Instructions.Starg_S((byte)4)
		});
		this.resize_inventory();
	}

	private void ShopMenu_Initialize()
	{
		this.resize_inventory();
		InstructionRange code = ((HackBase)this).BeginCode();
		for (int i = 0; i < 2; i++)
		{
			code = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[6]
			{
				InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
				InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "height")),
				InstructionMatcher.op_Implicit(Instructions.Ldc_I4(256)),
				InstructionMatcher.op_Implicit(OpCodes.Sub),
				InstructionMatcher.op_Implicit(Instructions.Ldc_I4_4()),
				InstructionMatcher.op_Implicit(OpCodes.Div)
			});
			code.Replace((CodeInstruction[])(object)new CodeInstruction[1] { Instructions.Ldc_I4(106) });
		}
	}

	private void ShopMenu_draw()
	{
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[9]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "yPositionOnScreen")),
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "height")),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(256)),
			InstructionMatcher.op_Implicit(OpCodes.Sub),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)40)),
			InstructionMatcher.op_Implicit(OpCodes.Add)
		}).SubRange(2, 6).Replace((CodeInstruction[])(object)new CodeInstruction[1] { Instructions.Ldc_I4(464) });
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[6]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "height")),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(448)),
			InstructionMatcher.op_Implicit(OpCodes.Sub),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)20)),
			InstructionMatcher.op_Implicit(OpCodes.Add)
		}).Replace((CodeInstruction[])(object)new CodeInstruction[5]
		{
			Instructions.Ldarg_0(),
			Instructions.Ldfld(typeof(ShopMenu), "inventory"),
			Instructions.Ldfld(typeof(IClickableMenu), "height"),
			Instructions.Ldc_I4_S((sbyte)44),
			Instructions.Add()
		});
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[8]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "height")),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(256)),
			InstructionMatcher.op_Implicit(OpCodes.Sub),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)32)),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_4()),
			InstructionMatcher.op_Implicit(OpCodes.Add)
		}).Replace((CodeInstruction[])(object)new CodeInstruction[1] { Instructions.Ldc_I4(460) });
	}

	private void ShopMenu_drawCurrency()
	{
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[2]
		{
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)12)),
			InstructionMatcher.op_Implicit(OpCodes.Sub)
		}).Replace((CodeInstruction[])(object)new CodeInstruction[2]
		{
			Instructions.Ldc_I4_S((sbyte)52),
			Instructions.Add()
		});
	}

	private void ShippingMenu_ctor()
	{
		this.resize_inventory();
		InstructionRange code = ((HackBase)this).BeginCode();
		for (int i = 0; i < 2; i++)
		{
			code = code.FindNext((InstructionMatcher[])(object)new InstructionMatcher[5]
			{
				InstructionMatcher.op_Implicit(Instructions.Ldc_I4(600)),
				InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "borderWidth")),
				InstructionMatcher.op_Implicit(Instructions.Ldc_I4_2()),
				InstructionMatcher.op_Implicit(OpCodes.Mul),
				InstructionMatcher.op_Implicit(OpCodes.Add)
			});
			code[0].operand = 664;
		}
	}

	private void JunimoNoteMenu_setUpMenu()
	{
		InstructionRange code = ((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[8]
		{
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "xPositionOnScreen")),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(128)),
			InstructionMatcher.op_Implicit(OpCodes.Add),
			InstructionMatcher.op_Implicit(OpCodes.Ldarg_0),
			InstructionMatcher.op_Implicit(Instructions.Ldfld(typeof(IClickableMenu), "yPositionOnScreen")),
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4(140)),
			InstructionMatcher.op_Implicit(OpCodes.Add)
		});
		code[2].operand = 104;
		code[6].operand = 120;
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[5]
		{
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36)),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_6),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_8),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_8),
			InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_0)
		}).Replace((CodeInstruction[])(object)new CodeInstruction[5]
		{
			Instructions.Ldc_I4_S((sbyte)49),
			Instructions.Ldc_I4_7(),
			Instructions.Ldc_I4_4(),
			Instructions.Ldc_I4_4(),
			Instructions.Ldc_I4_0()
		});
		((HackBase)this).FindCode((InstructionMatcher[])(object)new InstructionMatcher[2]
		{
			InstructionMatcher.op_Implicit(Instructions.Ldc_I4_S((sbyte)36)),
			InstructionMatcher.op_Implicit(Instructions.Stfld(typeof(InventoryMenu), "capacity"))
		})[0].operand = 49;
		code = ((HackBase)this).BeginCode();
		for (int i = 0; i < 2; i++)
		{
			code = code.FindNext((InstructionMatcher[])(object)new InstructionMatcher[4]
			{
				InstructionMatcher.op_Implicit(Instructions.Ldsfld(typeof(IClickableMenu), "borderWidth")),
				InstructionMatcher.op_Implicit(OpCodes.Ldc_I4_2),
				InstructionMatcher.op_Implicit(OpCodes.Mul),
				InstructionMatcher.op_Implicit(OpCodes.Add)
			});
			code.Remove();
		}
	}

	private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/JunimoNote", false))
		{
			e.Edit((Action<IAssetData>)delegate(IAssetData asset)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				IAssetDataForImage val = asset.AsImage();
				IRawTextureData val2 = ((Mod)this).Helper.ModContent.Load<IRawTextureData>("JunimoNote.png");
				Rectangle? val3 = new Rectangle(344, 28, 121, 127);
				val.PatchImage(val2, (Rectangle?)null, val3, (PatchMode)0);
			}, (AssetEditPriority)0, (string)null);
		}
	}

	protected override void InitializeApi(IGenericModConfigMenuApi api)
	{
		api.AddNumberOption(((Mod)this).ModManifest, (Func<int>)(() => base.config.BackpackCost), (Action<int>)delegate(int val)
		{
			base.config.BackpackCost = val;
		}, (Func<string>)(() => I18n.BackpackCostName()), (Func<string>)(() => I18n.BackpackCostTooltip()), (int?)0, (int?)null, (int?)null, (Func<int, string>)null, (string)null);
	}
}
