using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericModConfigMenu.Framework.ModOption;
using GenericModConfigMenu.Framework.Overlays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace GenericModConfigMenu.Framework;

internal class SpecificModConfigMenu : IClickableMenu
{
	private const int MinimumButtonGap = 32;

	private readonly Action<string> OpenPage;

	private readonly Action ReturnToList;

	private readonly ModConfig ModConfig;

	private readonly int ScrollSpeed;

	private RootElement Ui = new RootElement();

	private readonly Table Table;

	private readonly List<Label> OptHovers = new List<Label>();

	private bool ExitOnNextUpdate;

	private KeybindOverlay ActiveKeybindOverlay;

	private int TitleLabelWidth;

	private ModConfigManager ConfigsForKeybinds;

	private List<Label> keybindOpts = new List<Label>();

	public readonly string CurrPage;

	private int scrollCounter;

	private bool IsSubPage => !string.IsNullOrEmpty(this.CurrPage);

	private bool InGame => Context.IsWorldReady;

	private bool IsBindingKey => this.ActiveKeybindOverlay != null;

	public IManifest Manifest => this.ModConfig?.ModManifest;

	public bool IsKeybindsPage => this.Manifest == null;

	public SpecificModConfigMenu(ModConfigManager mods, int scrollSpeed, Action returnToList)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		this.ConfigsForKeybinds = mods;
		this.ScrollSpeed = scrollSpeed;
		this.ReturnToList = returnToList;
		this.Table = new Table(fixedRowHeight: false)
		{
			RowHeight = 50,
			Size = new Vector2((float)Math.Min(1200, ((Rectangle)(ref Game1.uiViewport)).Width - 200), (float)(((Rectangle)(ref Game1.uiViewport)).Height - 128 - 116))
		};
		this.Table.LocalPosition = new Vector2(((float)((Rectangle)(ref Game1.uiViewport)).Width - this.Table.Size.X) / 2f, ((float)((Rectangle)(ref Game1.uiViewport)).Height - this.Table.Size.Y) / 2f);
		foreach (ModConfig config in mods.GetAll())
		{
			List<Element[]> rows = new List<Element[]>();
			foreach (BaseModOption opt in config.GetAllOptions())
			{
				if (!(opt is SimpleModOption<SButton>) && !(opt is SimpleModOption<KeybindList>))
				{
					continue;
				}
				string name = opt.Name();
				string tooltip = opt.Tooltip();
				if (this.InGame && opt.IsTitleScreenOnly)
				{
					continue;
				}
				opt.BeforeMenuOpened();
				Label label = new Label
				{
					String = name,
					UserData = tooltip
				};
				if (!string.IsNullOrEmpty(tooltip))
				{
					this.OptHovers.Add(label);
				}
				Element optionElement = new Label
				{
					String = "TODO",
					LocalPosition = new Vector2(500f, 0f)
				};
				Label rightLabel = null;
				SimpleModOption<SButton> simpleModOption = opt as SimpleModOption<SButton>;
				if (simpleModOption == null)
				{
					SimpleModOption<KeybindList> simpleModOption2 = opt as SimpleModOption<KeybindList>;
					if (simpleModOption2 != null)
					{
						if ((int)Constants.TargetPlatform == 0)
						{
							continue;
						}
						optionElement = new Label
						{
							String = simpleModOption2.FormatValue(),
							LocalPosition = new Vector2(this.Table.Size.X / 5f * 4f, 0f),
							Callback = delegate(Element e)
							{
								this.ShowKeybindOverlay(simpleModOption2, e as Label);
							},
							UserData = opt
						};
					}
				}
				else
				{
					if ((int)Constants.TargetPlatform == 0)
					{
						continue;
					}
					optionElement = new Label
					{
						String = simpleModOption.FormatValue(),
						LocalPosition = new Vector2(this.Table.Size.X / 5f * 4f, 0f),
						Callback = delegate(Element e)
						{
							this.ShowKeybindOverlay(simpleModOption, e as Label);
						},
						UserData = opt
					};
				}
				this.keybindOpts.Add(optionElement as Label);
				rows.Add(new Element[3] { label, optionElement, rightLabel }.Where((Element p) => p != null).ToArray());
			}
			if (rows.Count <= 0)
			{
				continue;
			}
			Label header = new Label
			{
				String = config.ModName,
				UserData = config.ModManifest.Description,
				Bold = true
			};
			if (!string.IsNullOrEmpty(config.ModManifest.Description))
			{
				this.OptHovers.Add(header);
			}
			this.Table.AddRow(new Element[1] { header });
			foreach (Element[] row in rows)
			{
				this.Table.AddRow(row);
			}
			this.Table.AddRow(Array.Empty<Element>());
		}
		this.Ui.AddChild(this.Table);
		this.AddDefaultLabels(null);
		this.Table.ForceUpdateEvenHidden();
		this.RefreshKeybindColor();
	}

	public SpecificModConfigMenu(ModConfig config, int scrollSpeed, string page, Action<string> openPage, Action returnToList)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_07af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_082a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0845: Unknown result type (might be due to invalid IL or missing references)
		//IL_0879: Unknown result type (might be due to invalid IL or missing references)
		//IL_088f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b62: Unknown result type (might be due to invalid IL or missing references)
		this.ModConfig = config;
		this.ScrollSpeed = scrollSpeed;
		this.OpenPage = openPage;
		this.ReturnToList = returnToList;
		this.CurrPage = page ?? "";
		this.ModConfig.ActiveDisplayPage = this.ModConfig.Pages[this.CurrPage];
		this.Table = new Table(fixedRowHeight: false)
		{
			RowHeight = 50,
			Size = new Vector2((float)Math.Min(1200, ((Rectangle)(ref Game1.uiViewport)).Width - 200), (float)(((Rectangle)(ref Game1.uiViewport)).Height - 128 - 116))
		};
		this.Table.LocalPosition = new Vector2(((float)((Rectangle)(ref Game1.uiViewport)).Width - this.Table.Size.X) / 2f, ((float)((Rectangle)(ref Game1.uiViewport)).Height - this.Table.Size.Y) / 2f);
		Vector2 size = default(Vector2);
		Vector2 localPos = default(Vector2);
		foreach (BaseModOption opt in this.ModConfig.Pages[this.CurrPage].Options)
		{
			string name = opt.Name();
			string tooltip = opt.Tooltip();
			if (this.InGame && opt.IsTitleScreenOnly)
			{
				continue;
			}
			opt.BeforeMenuOpened();
			Label label = new Label
			{
				String = name,
				UserData = tooltip
			};
			if (!string.IsNullOrEmpty(tooltip))
			{
				this.OptHovers.Add(label);
			}
			Element optionElement = new Label
			{
				String = "TODO",
				LocalPosition = new Vector2(500f, 0f)
			};
			Label rightLabel = null;
			BaseModOption baseModOption = opt;
			SimpleModOption<int> simpleModOption4;
			SimpleModOption<float> simpleModOption5;
			if (!(baseModOption is ComplexModOption option))
			{
				SimpleModOption<bool> simpleModOption = baseModOption as SimpleModOption<bool>;
				if (simpleModOption == null)
				{
					SimpleModOption<SButton> simpleModOption2 = baseModOption as SimpleModOption<SButton>;
					if (simpleModOption2 == null)
					{
						SimpleModOption<KeybindList> simpleModOption3 = baseModOption as SimpleModOption<KeybindList>;
						if (simpleModOption3 == null)
						{
							NumericModOption<int> numericModOption = baseModOption as NumericModOption<int>;
							if (numericModOption == null)
							{
								NumericModOption<float> numericModOption2 = baseModOption as NumericModOption<float>;
								if (numericModOption2 == null)
								{
									ChoiceModOption<string> choiceModOption = baseModOption as ChoiceModOption<string>;
									if (choiceModOption == null)
									{
										simpleModOption4 = baseModOption as SimpleModOption<int>;
										if (simpleModOption4 != null)
										{
											goto IL_07af;
										}
										simpleModOption5 = baseModOption as SimpleModOption<float>;
										if (simpleModOption5 != null)
										{
											goto IL_0814;
										}
										SimpleModOption<string> simpleModOption6 = baseModOption as SimpleModOption<string>;
										if (simpleModOption6 == null)
										{
											if (!(baseModOption is SectionTitleModOption))
											{
												PageLinkModOption pageLinkModOption = baseModOption as PageLinkModOption;
												if (pageLinkModOption == null)
												{
													if (!(baseModOption is ParagraphModOption))
													{
														if (baseModOption is ImageModOption option2)
														{
															Texture2D texture = option2.Texture();
															((Vector2)(ref size))._002Ector((float)texture.Width, (float)texture.Height);
															if (option2.TexturePixelArea.HasValue)
															{
																((Vector2)(ref size))._002Ector((float)option2.TexturePixelArea.Value.Width, (float)option2.TexturePixelArea.Value.Height);
															}
															size *= (float)option2.Scale;
															((Vector2)(ref localPos))._002Ector(this.Table.Size.X / 2f - size.X / 2f, 0f);
															optionElement = new Image
															{
																Texture = texture,
																TexturePixelArea = (Rectangle)(((_003F?)option2.TexturePixelArea) ?? new Rectangle(0, 0, (int)size.X, (int)size.Y)),
																Scale = option2.Scale,
																LocalPosition = localPos
															};
														}
													}
													else
													{
														label = null;
														optionElement = null;
														StringBuilder text = new StringBuilder(name.Length + 50);
														string nextLine = "";
														string[] array = name.Split(' ');
														foreach (string word in array)
														{
															if (word == "\n")
															{
																text.AppendLine(nextLine);
																nextLine = "";
																continue;
															}
															if (nextLine == "")
															{
																nextLine = word;
																continue;
															}
															string possibleLine = (nextLine + " " + word).Trim();
															if (Label.MeasureString(possibleLine, bold: false, 1f, Game1.smallFont).X <= this.Table.Size.X)
															{
																nextLine = possibleLine;
																continue;
															}
															text.AppendLine(nextLine);
															nextLine = word;
														}
														if (nextLine != "")
														{
															text.AppendLine(nextLine);
														}
														label = null;
														optionElement = new Label
														{
															UserData = tooltip,
															NonBoldScale = 1f,
															NonBoldShadow = false,
															Font = Game1.smallFont,
															String = text.ToString()
														};
													}
												}
												else
												{
													label.Bold = true;
													label.Callback = delegate
													{
														this.OpenPage(pageLinkModOption.PageId);
													};
													optionElement = null;
												}
											}
											else
											{
												label.LocalPosition = new Vector2(-8f, 0f);
												label.Bold = true;
												if (name == "")
												{
													label = null;
												}
												optionElement = null;
											}
										}
										else
										{
											if ((int)Constants.TargetPlatform == 0)
											{
												continue;
											}
											optionElement = new Textbox
											{
												LocalPosition = new Vector2(this.Table.Size.X / 2f - 8f, 0f),
												String = simpleModOption6.Value,
												Callback = delegate(Element e)
												{
													simpleModOption6.Value = (e as Textbox).String;
												}
											};
										}
									}
									else
									{
										optionElement = new Dropdown
										{
											Choices = choiceModOption.Choices,
											Labels = choiceModOption.Choices.Select((string value) => choiceModOption.FormatChoice?.Invoke(value) ?? value).ToArray(),
											LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
											RequestWidth = (int)this.Table.Size.X / 2,
											Value = choiceModOption.Value,
											MaxValuesAtOnce = Math.Min(choiceModOption.Choices.Length, 5),
											Callback = delegate(Element e)
											{
												choiceModOption.Value = (e as Dropdown).Value;
											}
										};
									}
								}
								else
								{
									if (!numericModOption2.Minimum.HasValue || !numericModOption2.Maximum.HasValue)
									{
										simpleModOption5 = (SimpleModOption<float>)baseModOption;
										goto IL_0814;
									}
									rightLabel = new Label
									{
										String = numericModOption2.FormatValue()
									};
									optionElement = new Slider<float>
									{
										LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
										RequestWidth = (int)this.Table.Size.X / 3,
										Value = numericModOption2.Value,
										Minimum = numericModOption2.Minimum.Value,
										Maximum = numericModOption2.Maximum.Value,
										Interval = (numericModOption2.Interval ?? 0.01f),
										Callback = delegate(Element e)
										{
											numericModOption2.Value = (e as Slider<float>).Value;
											rightLabel.String = numericModOption2.FormatValue();
										}
									};
									rightLabel.LocalPosition = optionElement.LocalPosition + new Vector2((float)(optionElement.Width + 15), 0f);
								}
							}
							else
							{
								if (!numericModOption.Minimum.HasValue || !numericModOption.Maximum.HasValue)
								{
									simpleModOption4 = (SimpleModOption<int>)baseModOption;
									goto IL_07af;
								}
								rightLabel = new Label
								{
									String = numericModOption.FormatValue()
								};
								optionElement = new Slider<int>
								{
									LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
									RequestWidth = (int)this.Table.Size.X / 3,
									Value = numericModOption.Value,
									Minimum = numericModOption.Minimum.Value,
									Maximum = numericModOption.Maximum.Value,
									Interval = (numericModOption.Interval ?? 1),
									Callback = delegate(Element e)
									{
										numericModOption.Value = (e as Slider<int>).Value;
										rightLabel.String = numericModOption.FormatValue();
									}
								};
								rightLabel.LocalPosition = optionElement.LocalPosition + new Vector2((float)(optionElement.Width + 15), 0f);
							}
						}
						else
						{
							if ((int)Constants.TargetPlatform == 0)
							{
								continue;
							}
							optionElement = new Label
							{
								String = simpleModOption3.FormatValue(),
								LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
								Callback = delegate(Element e)
								{
									this.ShowKeybindOverlay(simpleModOption3, e as Label);
								}
							};
						}
					}
					else
					{
						if ((int)Constants.TargetPlatform == 0)
						{
							continue;
						}
						optionElement = new Label
						{
							String = simpleModOption2.FormatValue(),
							LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
							Callback = delegate(Element e)
							{
								this.ShowKeybindOverlay(simpleModOption2, e as Label);
							}
						};
					}
				}
				else
				{
					optionElement = new Checkbox
					{
						LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f),
						Checked = simpleModOption.Value,
						Callback = delegate(Element e)
						{
							simpleModOption.Value = (e as Checkbox).Checked;
						}
					};
				}
			}
			else
			{
				optionElement = new ComplexModOptionWidget(option)
				{
					LocalPosition = new Vector2(this.Table.Size.X / 2f, 0f)
				};
			}
			goto IL_0b6b;
			IL_07af:
			if ((int)Constants.TargetPlatform == 0)
			{
				continue;
			}
			optionElement = new Intbox
			{
				LocalPosition = new Vector2(this.Table.Size.X / 2f - 8f, 0f),
				Value = simpleModOption4.Value,
				Callback = delegate(Element e)
				{
					simpleModOption4.Value = (e as Intbox).Value;
				}
			};
			goto IL_0b6b;
			IL_0b6b:
			this.Table.AddRow(new Element[3] { label, optionElement, rightLabel }.Where((Element p) => p != null).ToArray());
			continue;
			IL_0814:
			if ((int)Constants.TargetPlatform == 0)
			{
				continue;
			}
			optionElement = new Floatbox
			{
				LocalPosition = new Vector2(this.Table.Size.X / 2f - 8f, 0f),
				Value = simpleModOption5.Value,
				Callback = delegate(Element e)
				{
					simpleModOption5.Value = (e as Floatbox).Value;
				}
			};
			goto IL_0b6b;
		}
		this.Ui.AddChild(this.Table);
		this.AddDefaultLabels(this.Manifest);
		this.Table.ForceUpdateEvenHidden();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.IsBindingKey)
		{
			this.ActiveKeybindOverlay.OnLeftClick(x, y);
			if (this.ActiveKeybindOverlay.IsFinished)
			{
				this.CloseKeybindOverlay();
			}
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)key == 27 && !this.IsBindingKey)
		{
			this.ExitOnNextUpdate = true;
		}
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (Dropdown.ActiveDropdown == null)
		{
			this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	public override void update(GameTime time)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((IClickableMenu)this).update(time);
		this.Ui.Update();
		GamePadState gamePadState = Game1.input.GetGamePadState();
		GamePadThumbSticks thumbSticks = ((GamePadState)(ref gamePadState)).ThumbSticks;
		if (((GamePadThumbSticks)(ref thumbSticks)).Right.Y != 0f)
		{
			if (++this.scrollCounter == 5)
			{
				this.scrollCounter = 0;
				Scrollbar scrollbar = this.Table.Scrollbar;
				gamePadState = Game1.input.GetGamePadState();
				thumbSticks = ((GamePadState)(ref gamePadState)).ThumbSticks;
				scrollbar.ScrollBy(Math.Sign(((GamePadThumbSticks)(ref thumbSticks)).Right.Y) * 120 / -this.ScrollSpeed);
			}
		}
		else
		{
			this.scrollCounter = 0;
		}
		if (this.ExitOnNextUpdate)
		{
			this.Cancel();
		}
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		((IClickableMenu)this).draw(b);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, ((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height), new Color(0, 0, 0, 192));
		int titleBoxWidth = Math.Clamp(this.TitleLabelWidth + 64, 864, ((Rectangle)(ref Game1.uiViewport)).Width);
		IClickableMenu.drawTextureBox(b, (((Rectangle)(ref Game1.uiViewport)).Width - titleBoxWidth) / 2, 32, titleBoxWidth, 70, Color.White);
		IClickableMenu.drawTextureBox(b, (((Rectangle)(ref Game1.uiViewport)).Width - 800) / 2 - 32 - 64, ((Rectangle)(ref Game1.uiViewport)).Height - 50 - 20 - 32, 992, 70, Color.White);
		this.Ui.Draw(b);
		this.ActiveKeybindOverlay?.Draw(b);
		((IClickableMenu)this).drawMouse(b, false, -1);
		if ((int)Constants.TargetPlatform == 0 || ((IClickableMenu)this).GetChildMenu() != null)
		{
			return;
		}
		foreach (Label label in this.OptHovers)
		{
			if (label.Hover)
			{
				string text = (string)label.UserData;
				if (text != null && !text.Contains("\n"))
				{
					text = Game1.parseText(text, Game1.smallFont, 800);
				}
				string title = label.String;
				if (title != null && !title.Contains("\n"))
				{
					title = Game1.parseText(title, Game1.dialogueFont, 800);
				}
				IClickableMenu.drawToolTip(b, text, title, (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
			}
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		this.Ui = new RootElement();
		Vector2 newSize = default(Vector2);
		((Vector2)(ref newSize))._002Ector((float)Math.Min(1200, ((Rectangle)(ref Game1.uiViewport)).Width - 200), (float)(((Rectangle)(ref Game1.uiViewport)).Height - 128 - 116));
		Element[] children = this.Table.Children;
		foreach (Element opt in children)
		{
			opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);
			if (opt is Slider slider)
			{
				slider.RequestWidth = (int)(newSize.X / (this.Table.Size.X / (float)slider.Width));
			}
		}
		this.Table.Size = newSize;
		this.Table.LocalPosition = new Vector2(((float)((Rectangle)(ref Game1.uiViewport)).Width - this.Table.Size.X) / 2f, ((float)((Rectangle)(ref Game1.uiViewport)).Height - this.Table.Size.Y) / 2f);
		this.Table.Scrollbar.Update();
		this.Ui.AddChild(this.Table);
		this.AddDefaultLabels(this.Manifest);
		this.ActiveKeybindOverlay?.OnWindowResized();
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return true;
	}

	public void OnButtonsChanged(ButtonsChangedEventArgs e)
	{
		if (this.IsBindingKey)
		{
			this.ActiveKeybindOverlay.OnButtonsChanged(e);
			if (this.ActiveKeybindOverlay.IsFinished)
			{
				this.CloseKeybindOverlay();
			}
		}
	}

	private void AddDefaultLabels(IManifest modManifest)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		string pageTitle = ((modManifest == null) ? "" : this.ModConfig.Pages[this.CurrPage].PageTitle());
		Label titleLabel = new Label
		{
			String = ((modManifest == null) ? I18n.List_Keybinds() : (modManifest.Name + ((pageTitle == "") ? "" : (" > " + pageTitle)))),
			Bold = true
		};
		titleLabel.LocalPosition = new Vector2(((float)((Rectangle)(ref Game1.uiViewport)).Width - titleLabel.Measure().X) / 2f, 44f);
		titleLabel.HoverTextColor = titleLabel.IdleTextColor;
		this.Ui.AddChild(titleLabel);
		this.TitleLabelWidth = (int)titleLabel.Measure().X;
		Vector2 leftPosition = default(Vector2);
		((Vector2)(ref leftPosition))._002Ector((float)(((Rectangle)(ref Game1.uiViewport)).Width / 2 - 450), (float)(((Rectangle)(ref Game1.uiViewport)).Height - 50 - 36));
		Label cancelButton = new Label
		{
			String = I18n.Config_Buttons_Cancel(),
			Bold = true,
			LocalPosition = leftPosition,
			Callback = delegate
			{
				this.Cancel();
			}
		};
		Label resetButton = new Label
		{
			String = I18n.Config_Buttons_ResetToDefault(),
			Bold = true,
			LocalPosition = leftPosition,
			Callback = delegate
			{
				this.ResetConfig();
			},
			ForceHide = () => this.IsSubPage || modManifest == null
		};
		Label saveButton = new Label
		{
			String = I18n.Config_Buttons_Save(),
			Bold = true,
			LocalPosition = leftPosition,
			Callback = delegate
			{
				this.SaveConfig();
			}
		};
		Label saveAndCloseButton = new Label
		{
			String = I18n.Config_Buttons_SaveAndClose(),
			Bold = true,
			LocalPosition = leftPosition,
			Callback = delegate
			{
				this.SaveConfig();
				this.Close();
			}
		};
		Label[] buttons = new Label[4] { cancelButton, resetButton, saveButton, saveAndCloseButton };
		int[] widths = buttons.Select((Label p) => p.Width).ToArray();
		int totalButtonWidths = widths.Sum();
		int leftOffset = 0;
		int gap = (914 - totalButtonWidths) / (buttons.Length - 1);
		if (gap < 32)
		{
			leftOffset = -((32 - gap) / 2) * (buttons.Length - 1);
			gap = 32;
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			Label obj = buttons[i];
			obj.LocalPosition += new Vector2((float)(leftOffset + widths.Take(i).Sum() + gap * i), 0f);
		}
		Label[] array = buttons;
		foreach (Label button in array)
		{
			this.Ui.AddChild(button);
		}
	}

	private void ResetConfig()
	{
		Game1.playSound("backpackIN", (int?)null);
		foreach (BaseModOption option in this.ModConfig.GetAllOptions())
		{
			option.BeforeReset();
		}
		this.ModConfig.Reset();
		foreach (BaseModOption option2 in this.ModConfig.GetAllOptions())
		{
			option2.AfterReset();
		}
		this.SaveConfig(playSound: false);
		this.OpenPage(this.CurrPage);
	}

	private void SaveConfig(bool playSound = true)
	{
		if (playSound)
		{
			Game1.playSound("money", (int?)null);
		}
		if (this.ModConfig != null)
		{
			foreach (BaseModOption option in this.ModConfig.GetAllOptions())
			{
				option.BeforeSave();
			}
			this.ModConfig.Save();
			{
				foreach (BaseModOption option2 in this.ModConfig.GetAllOptions())
				{
					option2.AfterSave();
				}
				return;
			}
		}
		ModConfig[] array = this.ConfigsForKeybinds.GetAll().ToArray();
		foreach (ModConfig config in array)
		{
			bool foundKey = false;
			foreach (BaseModOption option3 in config.GetAllOptions())
			{
				if (option3 is SimpleModOption<SButton> || option3 is SimpleModOption<KeybindList>)
				{
					foundKey = true;
					break;
				}
			}
			if (!foundKey)
			{
				continue;
			}
			foreach (BaseModOption option4 in config.GetAllOptions())
			{
				option4.BeforeSave();
			}
			config.Save();
			foreach (BaseModOption option5 in config.GetAllOptions())
			{
				option5.AfterSave();
			}
		}
	}

	private void Close()
	{
		if (this.ModConfig != null)
		{
			foreach (BaseModOption option in this.ModConfig.ActiveDisplayPage.Options)
			{
				option.BeforeMenuClosed();
			}
		}
		else
		{
			foreach (ModConfig config in this.ConfigsForKeybinds.GetAll())
			{
				foreach (BaseModOption option2 in config.GetAllOptions())
				{
					if (option2 is SimpleModOption<SButton> || option2 is SimpleModOption<KeybindList>)
					{
						option2.BeforeMenuClosed();
					}
				}
			}
		}
		if (this.IsSubPage)
		{
			this.OpenPage(null);
		}
		else
		{
			this.ReturnToList();
		}
	}

	private void Cancel()
	{
		Game1.playSound("bigDeSelect", (int?)null);
		this.Close();
	}

	private void ShowKeybindOverlay<TKeybind>(SimpleModOption<TKeybind> option, Label label)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected I4, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		Game1.playSound("breathin", (int?)null);
		SimpleModOption<SButton> simpleModOption = option as SimpleModOption<SButton>;
		KeybindOverlay activeKeybindOverlay;
		if (simpleModOption == null)
		{
			SimpleModOption<KeybindList> simpleModOption2 = option as SimpleModOption<KeybindList>;
			if (simpleModOption2 == null)
			{
				throw new InvalidOperationException("Unsupported keybind type " + typeof(TKeybind).FullName + ".");
			}
			activeKeybindOverlay = new KeybindOverlay(simpleModOption2.Value.Keybinds, onlyAllowSingleButton: false, option.Name(), delegate(Keybind[] keybinds)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				simpleModOption2.Value = new KeybindList(keybinds);
				label.String = option.FormatValue();
			});
		}
		else
		{
			activeKeybindOverlay = new KeybindOverlay((Keybind[])(object)new Keybind[1]
			{
				new Keybind((SButton[])(object)new SButton[1] { (SButton)(int)simpleModOption.Value })
			}, onlyAllowSingleButton: true, option.Name(), delegate(Keybind[] keybinds)
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				SimpleModOption<SButton> simpleModOption3 = simpleModOption;
				Keybind? obj = keybinds.FirstOrDefault();
				simpleModOption3.Value = (SButton)((obj != null) ? ((int)obj.Buttons.FirstOrDefault((SButton)0)) : 0);
				label.String = option.FormatValue();
			});
		}
		this.ActiveKeybindOverlay = activeKeybindOverlay;
		this.Ui.Obscured = true;
	}

	private void CloseKeybindOverlay()
	{
		this.ActiveKeybindOverlay = null;
		this.Ui.Obscured = false;
		this.RefreshKeybindColor();
	}

	private void RefreshKeybindColor()
	{
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (!this.IsKeybindsPage)
		{
			return;
		}
		Dictionary<string, int> keybinds = new Dictionary<string, int>();
		foreach (Label opt in this.keybindOpts)
		{
			if (opt.UserData is SimpleModOption<Keybind> kopt)
			{
				string entry = kopt.FormatValue();
				if (!keybinds.ContainsKey(entry))
				{
					keybinds.Add(entry, 0);
				}
				keybinds[entry]++;
			}
			else if (opt.UserData is SimpleModOption<KeybindList> klopt)
			{
				string entry2 = klopt.FormatValue();
				if (!keybinds.ContainsKey(entry2))
				{
					keybinds.Add(entry2, 0);
				}
				keybinds[entry2]++;
			}
		}
		foreach (Label opt2 in this.keybindOpts)
		{
			string entry3 = "";
			if (opt2.UserData is SimpleModOption<Keybind> kopt2)
			{
				entry3 = kopt2.FormatValue();
			}
			else if (opt2.UserData is SimpleModOption<KeybindList> klopt2)
			{
				entry3 = klopt2.FormatValue();
			}
			if (keybinds.ContainsKey(entry3))
			{
				if (keybinds[entry3] > 1 && entry3 != "(None)")
				{
					opt2.IdleTextColor = Color.Red;
					opt2.HoverTextColor = Color.PaleVioletRed;
				}
				else
				{
					opt2.IdleTextColor = Game1.textColor;
					opt2.HoverTextColor = Game1.unselectedOptionColor;
				}
			}
		}
	}
}
