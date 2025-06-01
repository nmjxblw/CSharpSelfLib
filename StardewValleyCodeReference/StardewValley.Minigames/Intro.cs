using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley.Minigames;

[InstanceStatics]
public class Intro : IMinigame
{
	public class Balloon
	{
		public Vector2 position;

		public Color color;

		public Balloon(int screenWidth, int screenHeight)
		{
			int g = Game1.random.Next(255);
			int b = 255 - g;
			int r = Game1.random.Choose(255, 0);
			this.position = new Vector2(Game1.random.Next(screenWidth / 5, screenWidth), screenHeight);
			this.color = new Color(r, g, b);
		}

		public void update(float speed, GameTime time)
		{
			this.position.Y -= speed * (float)time.ElapsedGameTime.TotalMilliseconds / 16f;
			this.position.X -= speed * (float)time.ElapsedGameTime.TotalMilliseconds / 32f;
		}
	}

	public int pixelScale = 4;

	public const int skyLoopWidth = 112;

	public const int cloudLoopWidth = 170;

	public const int tilesBeyondViewportToSimulate = 6;

	public const int leftFence = 0;

	public const int centerFence = 1;

	public const int rightFence = 2;

	public const int busYRest = 240;

	public const int choosingCharacterState = 0;

	public const int panningDownFromCloudsState = 1;

	public const int panningDownToRoadState = 2;

	public const int drivingState = 3;

	public const int stardewInViewState = 4;

	public float speed = 0.1f;

	private float valleyPosition;

	private float skyPosition;

	private float roadPosition;

	private float bigCloudPosition;

	private float backCloudPosition;

	private float globalYPan;

	private float globalYPanDY;

	private float drivingTimer;

	private float fadeAlpha;

	private float treePosition;

	private int screenWidth;

	private int screenHeight;

	private int tileSize = 16;

	private Matrix transformMatrix;

	private Texture2D texture;

	private Texture2D roadsideTexture;

	private Texture2D cloudTexture;

	private Texture2D treeStripTexture;

	private List<Point> backClouds = new List<Point>();

	private List<int> road = new List<int>();

	private List<int> sky = new List<int>();

	private List<int> roadsideObjects = new List<int>();

	private List<int> roadsideFences = new List<int>();

	private Color skyColor;

	private Color roadColor;

	private Color carColor;

	private bool cameraCenteredOnBus = true;

	private bool addedSign;

	private Vector2 busPosition;

	private Vector2 carPosition;

	private Vector2 birdPosition = Vector2.Zero;

	private CharacterCustomization characterCreateMenu;

	private List<Balloon> balloons = new List<Balloon>();

	private int birdFrame;

	private float birdTimer;

	private float birdXTimer;

	public static ICue roadNoise;

	private int fenceBuildStatus = -1;

	private int currentState;

	private bool quit;

	private bool hasQuit;

	public Intro()
	{
		this.texture = Game1.content.Load<Texture2D>("Minigames\\Intro");
		this.roadsideTexture = Game1.content.Load<Texture2D>("Maps\\spring_outdoorsTileSheet");
		this.cloudTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
		this.treeStripTexture = Game1.content.Load<Texture2D>("Minigames\\treestrip");
		this.transformMatrix = Matrix.CreateScale(this.pixelScale);
		this.skyColor = new Color(64, 136, 248);
		this.roadColor = new Color(130, 130, 130);
		this.createBeginningOfLevel();
		Game1.player.FarmerSprite.SourceRect = new Rectangle(0, 0, 16, 32);
		this.bigCloudPosition = this.cloudTexture.Width;
		Intro.roadNoise = Game1.soundBank.GetCue("roadnoise");
		this.currentState = 1;
		Game1.changeMusicTrack("spring_day_ambient");
		this.changeScreenSize();
	}

	public Intro(int startingGameMode)
	{
		this.texture = Game1.content.Load<Texture2D>("Minigames\\Intro");
		this.roadsideTexture = Game1.content.Load<Texture2D>("Maps\\spring_outdoorsTileSheet");
		this.cloudTexture = Game1.content.Load<Texture2D>("Minigames\\Clouds");
		this.transformMatrix = Matrix.CreateScale(this.pixelScale);
		this.skyColor = new Color(102, 181, 255);
		this.roadColor = new Color(130, 130, 130);
		this.createBeginningOfLevel();
		this.currentState = startingGameMode;
		if (this.currentState == 4)
		{
			this.fadeAlpha = 1f;
		}
		this.changeScreenSize();
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public void createBeginningOfLevel()
	{
		this.backClouds.Clear();
		this.road.Clear();
		this.sky.Clear();
		this.roadsideObjects.Clear();
		this.roadsideFences.Clear();
		for (int i = 0; i < this.screenWidth / this.tileSize + 6; i++)
		{
			this.road.Add((!(Game1.random.NextDouble() < 0.7)) ? Game1.random.Next(0, 3) : 0);
			this.roadsideObjects.Add(-1);
			this.roadsideFences.Add(-1);
		}
		for (int j = 0; j < this.screenWidth / 112 + 2; j++)
		{
			this.sky.Add(Game1.random.Choose(0, 1, 1));
		}
		for (int k = 0; k < this.screenWidth / 170 + 2; k++)
		{
			this.backClouds.Add(new Point(Game1.random.Next(3), Game1.random.Next(this.screenHeight / 2)));
		}
		this.roadsideObjects.Add(-1);
		this.roadsideObjects.Add(-1);
		this.roadsideObjects.Add(-1);
		this.busPosition = new Vector2(this.tileSize * 8, 240f);
	}

	public void updateRoad(GameTime time)
	{
		this.roadPosition += (float)time.ElapsedGameTime.TotalMilliseconds * this.speed;
		if (this.roadPosition >= (float)(this.tileSize * 3))
		{
			this.roadPosition -= this.tileSize * 3;
			for (int i = 0; i < 3; i++)
			{
				this.road.Add((!(Game1.random.NextDouble() < 0.7)) ? Game1.random.Next(0, 3) : 0);
			}
			this.road.RemoveRange(0, 3);
			if (this.fenceBuildStatus != -1 || (this.cameraCenteredOnBus && Game1.random.NextDouble() < 0.1))
			{
				for (int j = 0; j < 3; j++)
				{
					switch (this.fenceBuildStatus)
					{
					case -1:
						this.fenceBuildStatus = 0;
						this.roadsideFences.Add(0);
						break;
					case 0:
						this.fenceBuildStatus = 1;
						this.roadsideFences.Add(Game1.random.Next(3));
						break;
					case 1:
						if (Game1.random.NextDouble() < 0.1)
						{
							this.roadsideFences.Add(2);
							this.fenceBuildStatus = 2;
						}
						else
						{
							this.fenceBuildStatus = 1;
							this.roadsideFences.Add((Game1.random.NextDouble() < 0.1) ? 3 : Game1.random.Next(3));
						}
						break;
					case 2:
					{
						this.fenceBuildStatus = -1;
						for (int k = j; k < 3; k++)
						{
							this.roadsideFences.Add(-1);
						}
						break;
					}
					}
					if (this.fenceBuildStatus == -1)
					{
						break;
					}
				}
			}
			else
			{
				this.roadsideFences.Add(-1);
				this.roadsideFences.Add(-1);
				this.roadsideFences.Add(-1);
			}
			this.roadsideFences.RemoveRange(0, 3);
			if (this.cameraCenteredOnBus && !this.addedSign && Game1.random.NextDouble() < 0.25)
			{
				for (int l = 0; l < 3; l++)
				{
					if (l == 0 && Game1.random.NextDouble() < 0.3)
					{
						this.roadsideObjects.Add(Game1.random.Next(2));
						for (int m = l; m < 3; m++)
						{
							this.roadsideObjects.Add(-1);
						}
						break;
					}
					if (Game1.random.NextBool())
					{
						this.roadsideObjects.Add(Game1.random.Next(2, 5));
					}
					else
					{
						this.roadsideObjects.Add(-1);
					}
				}
			}
			else
			{
				this.roadsideObjects.Add(-1);
				this.roadsideObjects.Add(-1);
				this.roadsideObjects.Add(-1);
			}
			this.roadsideObjects.RemoveRange(0, 3);
		}
		this.skyPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 12f);
		if (this.skyPosition >= 112f)
		{
			this.skyPosition -= 112f;
			this.sky.Add(Game1.random.Next(2));
			this.sky.RemoveAt(0);
		}
		this.treePosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 2f);
		if (this.treePosition >= 256f)
		{
			this.treePosition -= 256f;
		}
		this.valleyPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 6f);
		if (this.carPosition.Equals(Vector2.Zero) && Game1.random.NextDouble() < 0.002 && !this.addedSign)
		{
			this.carPosition = new Vector2(this.screenWidth, 200f);
			this.carColor = new Color(Game1.random.Next(100, 255), Game1.random.Next(100, 255), Game1.random.Next(100, 255));
		}
		else if (!this.carPosition.Equals(Vector2.Zero))
		{
			this.carPosition.X -= 0.1f * (float)time.ElapsedGameTime.TotalMilliseconds * ((float)(int)this.carColor.G / 60f);
			if (this.carPosition.X < -200f)
			{
				this.carPosition = Vector2.Zero;
			}
		}
	}

	public void updateUpperClouds(GameTime time)
	{
		this.bigCloudPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 24f);
		if (this.bigCloudPosition >= (float)(this.cloudTexture.Width * 3))
		{
			this.bigCloudPosition -= this.cloudTexture.Width * 3;
		}
		this.backCloudPosition += (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 36f);
		if (this.backCloudPosition > 170f)
		{
			this.backCloudPosition %= 170f;
			this.backClouds.Add(new Point(Game1.random.Next(3), Game1.random.Next(this.screenHeight / 2)));
			this.backClouds.RemoveAt(0);
		}
		if (Game1.random.NextDouble() < 0.0002)
		{
			this.balloons.Add(new Balloon(this.screenWidth, this.screenHeight));
			if (Game1.random.NextDouble() < 0.1)
			{
				Vector2 position = new Vector2(Game1.random.Next(this.screenWidth / 3, this.screenWidth), this.screenHeight);
				this.balloons.Add(new Balloon(this.screenWidth, this.screenHeight)
				{
					position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
				});
				this.balloons.Add(new Balloon(this.screenWidth, this.screenHeight)
				{
					position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
				});
				this.balloons.Add(new Balloon(this.screenWidth, this.screenHeight)
				{
					position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
				});
				this.balloons.Add(new Balloon(this.screenWidth, this.screenHeight)
				{
					position = new Vector2(position.X + (float)Game1.random.Next(-16, 16), position.Y + (float)Game1.random.Next(8))
				});
			}
		}
		for (int i = this.balloons.Count - 1; i >= 0; i--)
		{
			this.balloons[i].update(this.speed, time);
			if (this.balloons[i].position.X < (float)(-this.tileSize) || this.balloons[i].position.Y < (float)(-this.tileSize))
			{
				this.balloons.RemoveAt(i);
			}
		}
	}

	public bool tick(GameTime time)
	{
		if (this.hasQuit)
		{
			return true;
		}
		if (this.quit && !this.hasQuit)
		{
			Game1.warpFarmer("BusStop", 22, 11, flip: false);
			Intro.roadNoise?.Stop(AudioStopOptions.Immediate);
			Game1.exitActiveMenu();
			this.hasQuit = true;
			return true;
		}
		switch (this.currentState)
		{
		case 0:
			this.updateUpperClouds(time);
			break;
		case 1:
			this.globalYPanDY = Math.Min(4f, this.globalYPanDY + (float)time.ElapsedGameTime.TotalMilliseconds * (this.speed / 140f));
			this.globalYPan -= this.globalYPanDY;
			this.updateUpperClouds(time);
			if (this.globalYPan < -1f)
			{
				this.globalYPan = this.screenHeight * this.pixelScale;
				this.currentState = 2;
				this.transformMatrix = Matrix.CreateScale(this.pixelScale);
				this.transformMatrix.Translation = new Vector3(0f, this.globalYPan, 0f);
				if (Intro.roadNoise != null)
				{
					Intro.roadNoise.SetVariable("Volume", 0);
					Intro.roadNoise.Play();
				}
				Game1.game1.loadForNewGame();
			}
			break;
		case 2:
		{
			int startPanY = this.screenHeight * this.pixelScale;
			int endPanY = -Math.Max(0, 900 - Game1.graphics.GraphicsDevice.Viewport.Height);
			endPanY = -(int)(240f * (540f / (float)Game1.graphics.GraphicsDevice.Viewport.Height));
			this.globalYPanDY = Math.Max(1f, this.globalYPan / 100f);
			this.globalYPan -= this.globalYPanDY;
			if (this.globalYPan <= (float)endPanY)
			{
				this.globalYPan = endPanY;
			}
			this.transformMatrix = Matrix.CreateScale(this.pixelScale);
			this.transformMatrix.Translation = new Vector3(0f, this.globalYPan, 0f);
			this.updateRoad(time);
			if (Intro.roadNoise != null)
			{
				float vol = (this.globalYPan - (float)startPanY) / (float)(endPanY - startPanY) * 10f + 90f;
				Intro.roadNoise.SetVariable("Volume", vol);
			}
			if (this.globalYPan <= (float)endPanY)
			{
				this.currentState = 3;
			}
			break;
		}
		case 3:
			this.updateRoad(time);
			this.drivingTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.drivingTimer > 4700f)
			{
				this.drivingTimer = 0f;
				this.currentState = 4;
			}
			break;
		case 4:
			this.updateRoad(time);
			this.drivingTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (!(this.drivingTimer > 2000f))
			{
				break;
			}
			this.busPosition.X += (float)time.ElapsedGameTime.TotalMilliseconds / 8f;
			Intro.roadNoise?.SetVariable("Volume", Math.Max(0f, Intro.roadNoise.GetVariable("Volume") - 1f));
			this.speed = Math.Max(0f, this.speed - (float)time.ElapsedGameTime.TotalMilliseconds / 70000f);
			if (!this.addedSign)
			{
				this.addedSign = true;
				this.roadsideObjects.RemoveAt(this.roadsideObjects.Count - 1);
				this.roadsideObjects.Add(5);
				Game1.playSound("busDriveOff");
			}
			if (this.speed <= 0f && this.birdPosition.Equals(Vector2.Zero))
			{
				int position = 0;
				for (int i = 0; i < this.roadsideObjects.Count; i++)
				{
					if (this.roadsideObjects[i] == 5)
					{
						position = i;
						break;
					}
				}
				this.birdPosition = new Vector2((float)(position * 16) - this.roadPosition - 32f + 16f, -16f);
				Game1.playSound("SpringBirds");
				this.fadeAlpha = 0f;
			}
			if (!this.birdPosition.Equals(Vector2.Zero) && this.birdPosition.Y < 116f)
			{
				float dy = Math.Max(0.5f, (116f - this.birdPosition.Y) / 116f * 2f);
				this.birdPosition.Y += dy;
				this.birdPosition.X += (float)Math.Sin((double)this.birdXTimer / (Math.PI * 16.0)) * dy / 2f;
				this.birdTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				this.birdXTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.birdTimer >= 100f)
				{
					this.birdFrame = (this.birdFrame + 1) % 4;
					this.birdTimer = 0f;
				}
			}
			else if (!this.birdPosition.Equals(Vector2.Zero))
			{
				this.birdFrame = ((this.birdTimer > 1500f) ? 5 : 4);
				this.birdTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.birdTimer > 2400f || (this.birdTimer > 1800f && Game1.random.NextDouble() < 0.006))
				{
					this.birdTimer = 0f;
					if (Game1.random.NextBool())
					{
						Game1.playSound("SpringBirds");
						this.birdPosition.Y -= 4f;
					}
				}
			}
			if (this.drivingTimer > 14000f)
			{
				this.fadeAlpha += (float)time.ElapsedGameTime.TotalMilliseconds * 0.1f / 128f;
				if (this.fadeAlpha >= 1f)
				{
					Game1.warpFarmer("BusStop", 22, 11, flip: false);
					Intro.roadNoise?.Stop(AudioStopOptions.Immediate);
					Game1.exitActiveMenu();
					return true;
				}
			}
			break;
		}
		return false;
	}

	public void doneCreatingCharacter()
	{
		this.characterCreateMenu = null;
		this.currentState = 1;
		Game1.changeMusicTrack("spring_day_ambient");
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
		this.characterCreateMenu?.receiveLeftClick(x, y);
		for (int i = this.balloons.Count - 1; i >= 0; i--)
		{
			if (new Rectangle((int)this.balloons[i].position.X * 4 + 16, (int)this.balloons[i].position.Y * 4 + 16, 32, 32).Contains(x, y))
			{
				this.balloons.RemoveAt(i);
				Game1.playSound("coin");
			}
		}
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.characterCreateMenu?.receiveRightClick(x, y);
	}

	public void releaseLeftClick(int x, int y)
	{
		this.characterCreateMenu?.releaseLeftClick(x, y);
	}

	public void leftClickHeld(int x, int y)
	{
		this.characterCreateMenu?.leftClickHeld(x, y);
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (k == Keys.Escape && this.currentState != 1)
		{
			if (!this.quit)
			{
				Game1.playSound("bigDeSelect");
			}
			this.quit = true;
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void draw(SpriteBatch b)
	{
		switch (this.currentState)
		{
		case 1:
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			b.GraphicsDevice.Clear(this.skyColor);
			int x = 64;
			int y = Game1.graphics.GraphicsDevice.Viewport.Height - 64;
			int w = 0;
			int h = 64;
			Utility.makeSafe(ref x, ref y, w, h);
			SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3689"), x, y, 999, -1, 999, 1f, 1f, junimoText: false, 0);
			b.End();
			break;
		}
		case 2:
		case 3:
		case 4:
			this.drawRoadArea(b);
			break;
		case 0:
			break;
		}
	}

	public void drawRoadArea(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, this.transformMatrix);
		b.GraphicsDevice.Clear(this.roadColor);
		b.Draw(Game1.staminaRect, new Rectangle(0, -this.screenHeight * 2, this.screenWidth, this.screenHeight * 8), this.skyColor);
		b.Draw(Game1.staminaRect, new Rectangle(0, this.screenHeight / 2 + 80 - 100, this.screenWidth, this.screenHeight * 4), this.roadColor);
		for (int i = 0; i < this.screenWidth / 112 + 2; i++)
		{
			if (this.sky[i] == 0)
			{
				b.Draw(this.texture, new Vector2(0f - this.skyPosition + (float)(i * 112) - (float)(i * 2), -16f), new Rectangle(129, 0, 110, 96), Color.White);
			}
			else
			{
				b.Draw(sourceRectangle: new Rectangle(128, 0, 1, 96), texture: this.texture, destinationRectangle: new Rectangle((int)(0f - this.skyPosition) - 1 + i * 112 - i * 2, -16, 114, 96), color: Color.White);
			}
		}
		for (int j = 0; j < 12; j++)
		{
			b.Draw(Game1.mouseCursors, new Vector2(-10f + (0f - this.valleyPosition) / 2f + (float)(j * 639) - (float)(j * 2), 70f), new Rectangle(0, 886, 639, 148), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2(0f - this.valleyPosition + (float)(j * 639) - (float)(j * 2), 80f), new Rectangle(0, 737, 639, 120), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.08f);
		}
		for (int k = 0; k < 8; k++)
		{
			b.Draw(this.treeStripTexture, new Vector2((float)(k * 256) - this.treePosition, 110f), new Rectangle(0, 0, 256, 64), Color.White);
		}
		for (int l = 0; l < this.road.Count; l++)
		{
			if (l % 3 == 0)
			{
				b.Draw(this.texture, new Vector2((float)(l * 16) - this.roadPosition, 160f), new Rectangle(0, 176, 48, 48), Color.White);
				b.Draw(this.texture, new Vector2((float)(l * 16 + this.tileSize) - this.roadPosition, 272f), new Rectangle(0, 64, 16, 16), Color.White);
			}
			b.Draw(this.texture, new Vector2((float)(l * 16) - this.roadPosition, 208f), new Rectangle(this.road[l] * 16, 240, 16, 16), Color.White);
		}
		for (int m = 0; m < this.roadsideObjects.Count; m++)
		{
			switch (this.roadsideObjects[m])
			{
			case 0:
				b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 96f), new Rectangle(48, 0, 48, 96), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				break;
			case 1:
				b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 96f), new Rectangle(0, 0, 48, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 16f, 160f), new Rectangle(16, 64, 16, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				break;
			case 2:
				b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 176f), new Rectangle(112, 144, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				break;
			case 3:
				b.Draw(this.roadsideTexture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 176f), new Rectangle(112, 160, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				break;
			case 5:
				b.Draw(this.texture, new Vector2((float)(m * 16) - this.roadPosition - 32f, 128f), new Rectangle(48, 176, 64, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				break;
			}
		}
		for (int n = 0; n < this.roadsideFences.Count; n++)
		{
			if (this.roadsideFences[n] != -1)
			{
				if (this.roadsideFences[n] == 3)
				{
					b.Draw(this.roadsideTexture, new Vector2((float)(n * 16) - this.roadPosition, 176f), new Rectangle(144, 256, 16, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
				else
				{
					b.Draw(this.roadsideTexture, new Vector2((float)(n * 16) - this.roadPosition, 176f), new Rectangle(128 + this.roadsideFences[n] * 16, 224, 16, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
		}
		if (!this.carPosition.Equals(Vector2.Zero))
		{
			b.Draw(this.texture, this.carPosition, new Rectangle(160, 112, 80, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			b.Draw(this.texture, this.carPosition, new Rectangle(160, 176, 80, 64), this.carColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
		b.Draw(this.texture, this.busPosition, new Rectangle(0, 0, 128, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		b.Draw(this.texture, this.busPosition + new Vector2(23.5f, 56.5f) * 1f, new Rectangle(21, 54, 5, 5), Color.White, (float)((double)(this.roadPosition / 3f / 16f) * Math.PI * 2.0), new Vector2(2.5f, 2.5f), 1f, SpriteEffects.None, 0f);
		b.Draw(this.texture, this.busPosition + new Vector2(87.5f, 56.5f) * 1f, new Rectangle(21, 54, 5, 5), Color.White, (float)((double)((this.roadPosition + 4f) / 3f / 16f) * Math.PI * 2.0), new Vector2(2.5f, 2.5f), 1f, SpriteEffects.None, 0f);
		if (!this.birdPosition.Equals(Vector2.Zero))
		{
			b.Draw(this.texture, this.birdPosition, new Rectangle(16 + this.birdFrame * 16, 64, 16, 16), Color.White);
		}
		if (this.fadeAlpha > 0f)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, this.screenWidth + 2, this.screenHeight * 2), Color.Black * this.fadeAlpha);
		}
		b.End();
	}

	public void changeScreenSize()
	{
		if (Game1.graphics.GraphicsDevice.Viewport.Height < 1000)
		{
			this.pixelScale = 3;
		}
		else if (Game1.graphics.GraphicsDevice.Viewport.Width > 2600)
		{
			this.pixelScale = 5;
		}
		else
		{
			this.pixelScale = 4;
		}
		this.transformMatrix = Matrix.CreateScale(this.pixelScale);
		this.screenWidth = Game1.graphics.GraphicsDevice.Viewport.Width / this.pixelScale;
		this.screenHeight = Game1.graphics.GraphicsDevice.Viewport.Height / this.pixelScale;
		this.createBeginningOfLevel();
	}

	public void unload()
	{
	}

	public void receiveEventPoke(int data)
	{
		throw new NotImplementedException();
	}

	public string minigameId()
	{
		return null;
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		return false;
	}
}
