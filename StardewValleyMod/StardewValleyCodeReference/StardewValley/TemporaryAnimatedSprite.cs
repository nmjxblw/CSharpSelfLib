using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley;

public class TemporaryAnimatedSprite
{
	public delegate void endBehavior(int extraInfo);

	public const int FireworkType_Heart = 0;

	public const int FireworkType_Star = 1;

	public const int FireworkType_Junimo = 2;

	public static float[] FireworksLifetimeMultiplier = new float[3] { 1f, 1f, 1.3f };

	public static Color[] FireworksColors = new Color[3]
	{
		new Color(252, 56, 37),
		new Color(144, 51, 237),
		new Color(92, 237, 213)
	};

	public static Vector2[][] FireworksLights = new Vector2[3][]
	{
		new Vector2[1]
		{
			new Vector2(0f, 0f)
		},
		new Vector2[1]
		{
			new Vector2(0f, 0f)
		},
		new Vector2[2]
		{
			new Vector2(-2.5f, 0f),
			new Vector2(2.5f, 0f)
		}
	};

	public static Vector2[][] FireworksPoints = new Vector2[3][]
	{
		new Vector2[14]
		{
			new Vector2(0f, -3f),
			new Vector2(2f, -5f),
			new Vector2(4f, -5f),
			new Vector2(6f, -3f),
			new Vector2(6f, -1f),
			new Vector2(4f, 1f),
			new Vector2(2f, 3f),
			new Vector2(0f, 5f),
			new Vector2(-2f, 3f),
			new Vector2(-4f, 1f),
			new Vector2(-6f, -1f),
			new Vector2(-6f, -3f),
			new Vector2(-4f, -5f),
			new Vector2(-2f, -5f)
		},
		new Vector2[20]
		{
			new Vector2(0f, -6f),
			new Vector2(1f, -4f),
			new Vector2(2f, -2f),
			new Vector2(4f, -2f),
			new Vector2(6f, -2f),
			new Vector2(4f, 0f),
			new Vector2(2f, 1f),
			new Vector2(3f, 3f),
			new Vector2(4f, 5f),
			new Vector2(2f, 4f),
			new Vector2(0f, 3f),
			new Vector2(-2f, 4f),
			new Vector2(-4f, 5f),
			new Vector2(-3f, 3f),
			new Vector2(-2f, 1f),
			new Vector2(-4f, 0f),
			new Vector2(-6f, -2f),
			new Vector2(-4f, -2f),
			new Vector2(-2f, -2f),
			new Vector2(-1f, -4f)
		},
		new Vector2[31]
		{
			new Vector2(-1f, -8f),
			new Vector2(0f, -6f),
			new Vector2(0f, -4f),
			new Vector2(2f, -4f),
			new Vector2(4f, -4f),
			new Vector2(6f, -2f),
			new Vector2(8f, -1f),
			new Vector2(9f, -3f),
			new Vector2(8f, -5f),
			new Vector2(6f, 0f),
			new Vector2(6f, 2f),
			new Vector2(3f, 2f),
			new Vector2(3f, 1f),
			new Vector2(5f, 4f),
			new Vector2(3f, 5f),
			new Vector2(3f, 7f),
			new Vector2(1f, 5f),
			new Vector2(-1f, 5f),
			new Vector2(-3f, 7f),
			new Vector2(-3f, 5f),
			new Vector2(-5f, 4f),
			new Vector2(-3f, 2f),
			new Vector2(-3f, 1f),
			new Vector2(-6f, 2f),
			new Vector2(-6f, 0f),
			new Vector2(-8f, -5f),
			new Vector2(-9f, -3f),
			new Vector2(-8f, -1f),
			new Vector2(-6f, -2f),
			new Vector2(-4f, -4f),
			new Vector2(-2f, -4f)
		}
	};

	public float timer;

	public float interval = 200f;

	public int currentParentTileIndex;

	public int oldCurrentParentTileIndex;

	public int initialParentTileIndex;

	public int totalNumberOfLoops;

	public int currentNumberOfLoops;

	public int xStopCoordinate = -1;

	public int yStopCoordinate = -1;

	public int animationLength;

	public int bombRadius;

	public int pingPongMotion = 1;

	public int bombDamage = -1;

	public int fireworkType = -1;

	public bool flicker;

	public bool timeBasedMotion;

	public bool overrideLocationDestroy;

	public bool pingPong;

	public bool holdLastFrame;

	public bool pulse;

	public int extraInfoForEndBehavior;

	/// <summary>The unique ID of the light source to add when rendering this sprite, or <c>null</c> for no light source.</summary>
	public string lightId;

	public int id;

	public bool bigCraftable;

	public bool swordswipe;

	public bool flash;

	public bool flipped;

	public bool verticalFlipped;

	public bool local;

	public bool hasLit;

	public bool xPeriodic;

	public bool yPeriodic;

	public bool destroyable = true;

	public bool paused;

	public bool stopAcceleratingWhenVelocityIsZero;

	public bool positionFollowsAttachedCharacter;

	public bool usePreciseTiming;

	public float rotation;

	public float alpha = 1f;

	public float alphaFade;

	public float layerDepth = -1f;

	public float scale = 1f;

	public float scaleChange;

	public float scaleChangeChange;

	public float rotationChange;

	public float lightRadius;

	public float xPeriodicRange;

	public float yPeriodicRange;

	public float xPeriodicLoopTime;

	public float yPeriodicLoopTime;

	public float shakeIntensityChange;

	public float shakeIntensity;

	public float pulseTime;

	public float pulseAmount = 1.1f;

	public float alphaFadeFade;

	public int lightFade = -1;

	public float afterAccelStopMotionX;

	public float afterAccelStopMotionY;

	public float layerDepthOffset;

	public Vector2 position;

	public Vector2 sourceRectStartingPos;

	protected GameLocation parent;

	public string textureName;

	public Texture2D texture;

	public Rectangle sourceRect;

	public Color color = Color.White;

	public Color lightcolor = Color.White;

	public Farmer owner;

	public Vector2 motion = Vector2.Zero;

	public Vector2 acceleration = Vector2.Zero;

	public Vector2 accelerationChange = Vector2.Zero;

	public Vector2 initialPosition;

	public Vector2 vectorScale;

	public int delayBeforeAnimationStart;

	public int ticksBeforeAnimationStart;

	public string startSound;

	public string endSound;

	public string text;

	public endBehavior endFunction;

	public endBehavior reachedStopCoordinate;

	public Action<TemporaryAnimatedSprite> reachedStopCoordinateSprite;

	public TemporaryAnimatedSprite parentSprite;

	public Character attachedCharacter;

	private float pulseTimer;

	private float originalScale;

	public bool drawAboveAlwaysFront;

	public bool dontClearOnAreaEntry;

	private Stopwatch stopWatch;

	private long previousStopwatchTime;

	protected bool _pooled;

	public static List<TemporaryAnimatedSprite> _pool;

	private float totalTimer;

	public bool Pooled => this._pooled;

	public Vector2 Position
	{
		get
		{
			return this.position;
		}
		set
		{
			this.position = value;
		}
	}

	public Texture2D Texture => this.texture;

	public GameLocation Parent
	{
		get
		{
			return this.parent;
		}
		set
		{
			this.parent = value;
		}
	}

	public static float GetFireworkLifetimeMultiplier(int id)
	{
		return TemporaryAnimatedSprite.FireworksLifetimeMultiplier[id];
	}

	public static Color GetFireworkColor(int id)
	{
		return TemporaryAnimatedSprite.FireworksColors[id];
	}

	public static Vector2[] GetFireworkLights(int id)
	{
		return TemporaryAnimatedSprite.FireworksLights[id];
	}

	public static Vector2[] GetFireworkPoints(int id)
	{
		return TemporaryAnimatedSprite.FireworksPoints[id];
	}

	public TemporaryAnimatedSprite getClone()
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite();
		temporaryAnimatedSprite.texture = this.texture;
		temporaryAnimatedSprite.interval = this.interval;
		temporaryAnimatedSprite.currentParentTileIndex = this.currentParentTileIndex;
		temporaryAnimatedSprite.oldCurrentParentTileIndex = this.oldCurrentParentTileIndex;
		temporaryAnimatedSprite.initialParentTileIndex = this.initialParentTileIndex;
		temporaryAnimatedSprite.totalNumberOfLoops = this.totalNumberOfLoops;
		temporaryAnimatedSprite.currentNumberOfLoops = this.currentNumberOfLoops;
		temporaryAnimatedSprite.xStopCoordinate = this.xStopCoordinate;
		temporaryAnimatedSprite.yStopCoordinate = this.yStopCoordinate;
		temporaryAnimatedSprite.animationLength = this.animationLength;
		temporaryAnimatedSprite.bombRadius = this.bombRadius;
		temporaryAnimatedSprite.bombDamage = this.bombDamage;
		temporaryAnimatedSprite.pingPongMotion = this.pingPongMotion;
		temporaryAnimatedSprite.fireworkType = this.fireworkType;
		temporaryAnimatedSprite.flicker = this.flicker;
		temporaryAnimatedSprite.timeBasedMotion = this.timeBasedMotion;
		temporaryAnimatedSprite.overrideLocationDestroy = this.overrideLocationDestroy;
		temporaryAnimatedSprite.pingPong = this.pingPong;
		temporaryAnimatedSprite.holdLastFrame = this.holdLastFrame;
		temporaryAnimatedSprite.extraInfoForEndBehavior = this.extraInfoForEndBehavior;
		temporaryAnimatedSprite.lightId = this.lightId;
		temporaryAnimatedSprite.acceleration = this.acceleration;
		temporaryAnimatedSprite.accelerationChange = this.accelerationChange;
		temporaryAnimatedSprite.alpha = this.alpha;
		temporaryAnimatedSprite.alphaFade = this.alphaFade;
		temporaryAnimatedSprite.attachedCharacter = this.attachedCharacter;
		temporaryAnimatedSprite.bigCraftable = this.bigCraftable;
		temporaryAnimatedSprite.color = this.color;
		temporaryAnimatedSprite.delayBeforeAnimationStart = this.delayBeforeAnimationStart;
		temporaryAnimatedSprite.ticksBeforeAnimationStart = this.ticksBeforeAnimationStart;
		temporaryAnimatedSprite.destroyable = this.destroyable;
		temporaryAnimatedSprite.endFunction = this.endFunction;
		temporaryAnimatedSprite.endSound = this.endSound;
		temporaryAnimatedSprite.flash = this.flash;
		temporaryAnimatedSprite.flipped = this.flipped;
		temporaryAnimatedSprite.hasLit = this.hasLit;
		temporaryAnimatedSprite.id = this.id;
		temporaryAnimatedSprite.initialPosition = this.initialPosition;
		temporaryAnimatedSprite.lightFade = this.lightFade;
		temporaryAnimatedSprite.local = this.local;
		temporaryAnimatedSprite.motion = this.motion;
		temporaryAnimatedSprite.owner = this.owner;
		temporaryAnimatedSprite.parent = this.parent;
		temporaryAnimatedSprite.parentSprite = this.parentSprite;
		temporaryAnimatedSprite.position = this.position;
		temporaryAnimatedSprite.rotation = this.rotation;
		temporaryAnimatedSprite.rotationChange = this.rotationChange;
		temporaryAnimatedSprite.scale = this.scale;
		temporaryAnimatedSprite.scaleChange = this.scaleChange;
		temporaryAnimatedSprite.scaleChangeChange = this.scaleChangeChange;
		temporaryAnimatedSprite.shakeIntensity = this.shakeIntensity;
		temporaryAnimatedSprite.shakeIntensityChange = this.shakeIntensityChange;
		temporaryAnimatedSprite.sourceRect = this.sourceRect;
		temporaryAnimatedSprite.sourceRectStartingPos = this.sourceRectStartingPos;
		temporaryAnimatedSprite.startSound = this.startSound;
		temporaryAnimatedSprite.timeBasedMotion = this.timeBasedMotion;
		temporaryAnimatedSprite.verticalFlipped = this.verticalFlipped;
		temporaryAnimatedSprite.xPeriodic = this.xPeriodic;
		temporaryAnimatedSprite.xPeriodicLoopTime = this.xPeriodicLoopTime;
		temporaryAnimatedSprite.xPeriodicRange = this.xPeriodicRange;
		temporaryAnimatedSprite.yPeriodic = this.yPeriodic;
		temporaryAnimatedSprite.yPeriodicLoopTime = this.yPeriodicLoopTime;
		temporaryAnimatedSprite.yPeriodicRange = this.yPeriodicRange;
		temporaryAnimatedSprite.yStopCoordinate = this.yStopCoordinate;
		temporaryAnimatedSprite.totalNumberOfLoops = this.totalNumberOfLoops;
		temporaryAnimatedSprite.stopAcceleratingWhenVelocityIsZero = this.stopAcceleratingWhenVelocityIsZero;
		temporaryAnimatedSprite.afterAccelStopMotionX = this.afterAccelStopMotionX;
		temporaryAnimatedSprite.afterAccelStopMotionY = this.afterAccelStopMotionY;
		temporaryAnimatedSprite.layerDepthOffset = this.layerDepthOffset;
		temporaryAnimatedSprite.positionFollowsAttachedCharacter = this.positionFollowsAttachedCharacter;
		temporaryAnimatedSprite.dontClearOnAreaEntry = this.dontClearOnAreaEntry;
		return temporaryAnimatedSprite;
	}

	public virtual void Pool()
	{
		this.timer = 0f;
		this.interval = 200f;
		this.currentParentTileIndex = 0;
		this.oldCurrentParentTileIndex = 0;
		this.initialParentTileIndex = 0;
		this.totalNumberOfLoops = 0;
		this.currentNumberOfLoops = 0;
		this.xStopCoordinate = -1;
		this.yStopCoordinate = -1;
		this.animationLength = 0;
		this.bombRadius = 0;
		this.pingPongMotion = 1;
		this.bombDamage = -1;
		this.fireworkType = -1;
		this.flicker = false;
		this.timeBasedMotion = false;
		this.overrideLocationDestroy = false;
		this.pingPong = false;
		this.holdLastFrame = false;
		this.pulse = false;
		this.extraInfoForEndBehavior = 0;
		this.lightId = null;
		this.bigCraftable = false;
		this.swordswipe = false;
		this.flash = false;
		this.flipped = false;
		this.verticalFlipped = false;
		this.local = false;
		this.hasLit = false;
		this.xPeriodic = false;
		this.yPeriodic = false;
		this.destroyable = true;
		this.paused = false;
		this.stopAcceleratingWhenVelocityIsZero = false;
		this.positionFollowsAttachedCharacter = false;
		this.rotation = 0f;
		this.alpha = 1f;
		this.alphaFade = 0f;
		this.layerDepth = -1f;
		this.scale = 1f;
		this.scaleChange = 0f;
		this.scaleChangeChange = 0f;
		this.rotationChange = 0f;
		this.id = 0;
		this.lightRadius = 0f;
		this.xPeriodicRange = 0f;
		this.yPeriodicRange = 0f;
		this.xPeriodicLoopTime = 0f;
		this.yPeriodicLoopTime = 0f;
		this.shakeIntensityChange = 0f;
		this.shakeIntensity = 0f;
		this.pulseTime = 0f;
		this.pulseAmount = 1.1f;
		this.alphaFadeFade = 0f;
		this.lightFade = -1;
		this.layerDepthOffset = 0f;
		this.afterAccelStopMotionX = 0f;
		this.afterAccelStopMotionY = 0f;
		this.position = Vector2.Zero;
		this.sourceRectStartingPos = Vector2.Zero;
		this.parent = null;
		this.textureName = null;
		this.texture = null;
		this.sourceRect = Rectangle.Empty;
		this.color = Color.White;
		this.lightcolor = Color.White;
		this.owner = null;
		this.motion = Vector2.Zero;
		this.acceleration = Vector2.Zero;
		this.accelerationChange = Vector2.Zero;
		this.initialPosition = Vector2.Zero;
		this.delayBeforeAnimationStart = 0;
		this.ticksBeforeAnimationStart = 0;
		this.startSound = null;
		this.endSound = null;
		this.text = null;
		this.endFunction = null;
		this.reachedStopCoordinate = null;
		this.reachedStopCoordinateSprite = null;
		this.parentSprite = null;
		this.attachedCharacter = null;
		this.pulseTimer = 0f;
		this.originalScale = 0f;
		this.drawAboveAlwaysFront = false;
		this.dontClearOnAreaEntry = false;
		TemporaryAnimatedSprite._pool.Add(this);
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite()
	{
		TemporaryAnimatedSprite s = null;
		if (TemporaryAnimatedSprite._pool == null)
		{
			TemporaryAnimatedSprite._pool = new List<TemporaryAnimatedSprite>();
			for (int i = 0; i < 256; i++)
			{
				TemporaryAnimatedSprite newInstance = new TemporaryAnimatedSprite
				{
					_pooled = true
				};
				TemporaryAnimatedSprite._pool.Add(newInstance);
			}
		}
		if (TemporaryAnimatedSprite._pool.Count > 0)
		{
			s = TemporaryAnimatedSprite._pool[TemporaryAnimatedSprite._pool.Count - 1];
			TemporaryAnimatedSprite._pool.RemoveAt(TemporaryAnimatedSprite._pool.Count - 1);
		}
		if (s == null)
		{
			s = new TemporaryAnimatedSprite();
		}
		return s;
	}

	public TemporaryAnimatedSprite()
	{
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
	{
		TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite();
		if (s.initialParentTileIndex == -1)
		{
			s.swordswipe = true;
			s.currentParentTileIndex = 0;
		}
		else
		{
			s.currentParentTileIndex = initialParentTileIndex;
		}
		s.initialParentTileIndex = initialParentTileIndex;
		s.interval = animationInterval;
		s.totalNumberOfLoops = numberOfLoops;
		s.position = position;
		s.animationLength = animationLength;
		s.flicker = flicker;
		s.flipped = flipped;
		return s;
	}

	public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
	{
		if (initialParentTileIndex == -1)
		{
			this.swordswipe = true;
			this.currentParentTileIndex = 0;
		}
		else
		{
			this.currentParentTileIndex = initialParentTileIndex;
		}
		this.initialParentTileIndex = initialParentTileIndex;
		this.interval = animationInterval;
		this.totalNumberOfLoops = numberOfLoops;
		this.position = position;
		this.animationLength = animationLength;
		this.flicker = flicker;
		this.flipped = flipped;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int rowInAnimationTexture, Vector2 position, Color color, int animationLength = 8, bool flipped = false, float animationInterval = 100f, int numberOfLoops = 0, int sourceRectWidth = -1, float layerDepth = -1f, int sourceRectHeight = -1, int delay = 0)
	{
		TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, rowInAnimationTexture * 64, sourceRectWidth, sourceRectHeight), animationInterval, animationLength, numberOfLoops, position, flicker: false, flipped, layerDepth, 0f, color, 1f, 0f, 0f, 0f);
		if (sourceRectWidth == -1)
		{
			sourceRectWidth = 64;
			s.sourceRect.Width = 64;
		}
		if (sourceRectHeight == -1)
		{
			sourceRectHeight = 64;
			s.sourceRect.Height = 64;
		}
		if (s.layerDepth == -1f)
		{
			s.layerDepth = (s.position.Y + 32f) / 10000f;
		}
		s.delayBeforeAnimationStart = delay;
		return s;
	}

	public TemporaryAnimatedSprite(int rowInAnimationTexture, Vector2 position, Color color, int animationLength = 8, bool flipped = false, float animationInterval = 100f, int numberOfLoops = 0, int sourceRectWidth = -1, float layerDepth = -1f, int sourceRectHeight = -1, int delay = 0)
		: this("TileSheets\\animations", new Rectangle(0, rowInAnimationTexture * 64, sourceRectWidth, sourceRectHeight), animationInterval, animationLength, numberOfLoops, position, flicker: false, flipped, layerDepth, 0f, color, 1f, 0f, 0f, 0f)
	{
		if (sourceRectWidth == -1)
		{
			sourceRectWidth = 64;
			this.sourceRect.Width = 64;
		}
		if (sourceRectHeight == -1)
		{
			sourceRectHeight = 64;
			this.sourceRect.Height = 64;
		}
		if (layerDepth == -1f)
		{
			layerDepth = (position.Y + 32f) / 10000f;
		}
		this.delayBeforeAnimationStart = delay;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, bool verticalFlipped, float rotation)
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
		temporaryAnimatedSprite.rotation = rotation;
		temporaryAnimatedSprite.verticalFlipped = verticalFlipped;
		return temporaryAnimatedSprite;
	}

	public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, bool verticalFlipped, float rotation)
		: this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
	{
		this.rotation = rotation;
		this.verticalFlipped = verticalFlipped;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool bigCraftable, bool flipped)
	{
		TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
		s.bigCraftable = bigCraftable;
		if (s.bigCraftable)
		{
			s.position.Y -= 64f;
		}
		return s;
	}

	public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool bigCraftable, bool flipped)
		: this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
	{
		this.bigCraftable = bigCraftable;
		if (bigCraftable)
		{
			this.position.Y -= 64f;
		}
	}

	public TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
		temporaryAnimatedSprite.textureName = textureName;
		temporaryAnimatedSprite.loadTexture();
		temporaryAnimatedSprite.sourceRect = sourceRect;
		temporaryAnimatedSprite.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		temporaryAnimatedSprite.initialPosition = position;
		return temporaryAnimatedSprite;
	}

	public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped)
		: this(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
	{
		this.textureName = textureName;
		this.loadTexture();
		this.sourceRect = sourceRect;
		this.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		this.initialPosition = position;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, float layerDepth, float alphaFade, Color color, float scale, float scaleChange, float rotation, float rotationChange, bool local = false)
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
		temporaryAnimatedSprite.textureName = textureName;
		temporaryAnimatedSprite.loadTexture();
		temporaryAnimatedSprite.sourceRect = sourceRect;
		temporaryAnimatedSprite.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		temporaryAnimatedSprite.layerDepth = layerDepth;
		temporaryAnimatedSprite.alphaFade = Math.Max(0f, alphaFade);
		temporaryAnimatedSprite.color = color;
		temporaryAnimatedSprite.scale = scale;
		temporaryAnimatedSprite.scaleChange = scaleChange;
		temporaryAnimatedSprite.rotation = rotation;
		temporaryAnimatedSprite.rotationChange = rotationChange;
		temporaryAnimatedSprite.local = local;
		temporaryAnimatedSprite.initialPosition = position;
		return temporaryAnimatedSprite;
	}

	public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, float layerDepth, float alphaFade, Color color, float scale, float scaleChange, float rotation, float rotationChange, bool local = false)
		: this(0, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
	{
		this.textureName = textureName;
		this.loadTexture();
		this.sourceRect = sourceRect;
		this.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		this.layerDepth = layerDepth;
		this.alphaFade = Math.Max(0f, alphaFade);
		this.color = color;
		this.scale = scale;
		this.scaleChange = scaleChange;
		this.rotation = rotation;
		this.rotationChange = rotationChange;
		this.local = local;
		this.initialPosition = position;
	}

	public virtual void CopyAppearanceFromItemId(string itemId, int offset = 0)
	{
		this.scale = 4f * this.scale;
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(itemId);
		this.textureName = itemData.TextureName;
		this.loadTexture();
		this.sourceRect = itemData.GetSourceRect(offset);
		this.sourceRectStartingPos = Utility.PointToVector2(this.sourceRect.Location);
		this.currentParentTileIndex = 0;
		this.initialParentTileIndex = 0;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(string textureName, Rectangle sourceRect, Vector2 position, bool flipped, float alphaFade, Color color)
	{
		TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(0, 999999f, 1, 0, position, flicker: false, flipped);
		temporaryAnimatedSprite.textureName = textureName;
		temporaryAnimatedSprite.loadTexture();
		temporaryAnimatedSprite.sourceRect = sourceRect;
		temporaryAnimatedSprite.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		temporaryAnimatedSprite.initialPosition = position;
		temporaryAnimatedSprite.alphaFade = Math.Max(0f, alphaFade);
		temporaryAnimatedSprite.color = color;
		return temporaryAnimatedSprite;
	}

	public TemporaryAnimatedSprite(string textureName, Rectangle sourceRect, Vector2 position, bool flipped, float alphaFade, Color color)
		: this(0, 999999f, 1, 0, position, flicker: false, flipped)
	{
		this.textureName = textureName;
		this.loadTexture();
		this.sourceRect = sourceRect;
		this.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
		this.initialPosition = position;
		this.alphaFade = Math.Max(0f, alphaFade);
		this.color = color;
	}

	public static TemporaryAnimatedSprite GetTemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, GameLocation parent, Farmer owner)
	{
		TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped);
		s.position.X = (int)s.position.X;
		s.position.Y = (int)s.position.Y;
		s.parent = parent;
		switch (s.initialParentTileIndex)
		{
		case 286:
			s.bombRadius = 3;
			break;
		case 287:
			s.bombRadius = 5;
			break;
		case 288:
			s.bombRadius = 7;
			break;
		}
		s.owner = owner;
		return s;
	}

	/// <summary>Construct an instance for a bomb.</summary>
	public TemporaryAnimatedSprite(int initialParentTileIndex, float animationInterval, int animationLength, int numberOfLoops, Vector2 position, bool flicker, bool flipped, GameLocation parent, Farmer owner)
		: this(initialParentTileIndex, animationInterval, animationLength, numberOfLoops, position, flicker, flipped)
	{
		this.position.X = (int)this.position.X;
		this.position.Y = (int)this.position.Y;
		this.parent = parent;
		switch (initialParentTileIndex)
		{
		case 286:
			this.bombRadius = 3;
			break;
		case 287:
			this.bombRadius = 5;
			break;
		case 288:
			this.bombRadius = 7;
			break;
		}
		this.owner = owner;
	}

	private void loadTexture()
	{
		string text = this.textureName;
		if (text != null)
		{
			if (text == "")
			{
				this.texture = Game1.staminaRect;
			}
			else
			{
				this.texture = Game1.content.Load<Texture2D>(this.textureName);
			}
		}
		else
		{
			this.texture = null;
		}
	}

	public void Read(BinaryReader reader, GameLocation location)
	{
		this.timer = 0f;
		BitArray bitArray = reader.ReadBitArray();
		int i = 0;
		if (bitArray[i++])
		{
			this.interval = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.currentParentTileIndex = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.oldCurrentParentTileIndex = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.initialParentTileIndex = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.totalNumberOfLoops = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.currentNumberOfLoops = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.xStopCoordinate = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.yStopCoordinate = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.animationLength = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.bombRadius = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.bombDamage = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.pingPongMotion = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.fireworkType = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.flicker = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.timeBasedMotion = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.overrideLocationDestroy = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.pingPong = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.holdLastFrame = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.pulse = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.extraInfoForEndBehavior = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.lightId = reader.ReadString();
		}
		if (bitArray[i++])
		{
			this.bigCraftable = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.swordswipe = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.flash = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.flipped = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.verticalFlipped = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.local = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.lightFade = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.hasLit = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.xPeriodic = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.yPeriodic = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.destroyable = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.paused = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.rotation = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.alpha = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.alphaFade = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.layerDepth = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.scale = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.scaleChange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.scaleChangeChange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.rotationChange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.id = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.lightRadius = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.xPeriodicRange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.yPeriodicRange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.xPeriodicLoopTime = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.yPeriodicLoopTime = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.shakeIntensityChange = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.shakeIntensity = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.pulseTime = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.pulseAmount = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.position = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.sourceRectStartingPos = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.sourceRect = reader.ReadRectangle();
		}
		if (bitArray[i++])
		{
			this.color = reader.ReadColor();
		}
		if (bitArray[i++])
		{
			this.lightcolor = reader.ReadColor();
		}
		if (bitArray[i++])
		{
			this.motion = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.acceleration = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.accelerationChange = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.initialPosition = reader.ReadVector2();
		}
		if (bitArray[i++])
		{
			this.delayBeforeAnimationStart = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.ticksBeforeAnimationStart = reader.ReadInt32();
		}
		if (bitArray[i++])
		{
			this.startSound = reader.ReadString();
		}
		if (bitArray[i++])
		{
			this.endSound = reader.ReadString();
		}
		if (bitArray[i++])
		{
			this.text = reader.ReadString();
		}
		if (bitArray[i++])
		{
			this.textureName = reader.ReadString();
		}
		if (bitArray[i++])
		{
			this.owner = Game1.GetPlayer(reader.ReadInt64()) ?? Game1.MasterPlayer;
		}
		if (bitArray[i++])
		{
			this.stopAcceleratingWhenVelocityIsZero = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.layerDepthOffset = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.afterAccelStopMotionX = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.afterAccelStopMotionY = reader.ReadSingle();
		}
		if (bitArray[i++])
		{
			this.positionFollowsAttachedCharacter = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.dontClearOnAreaEntry = reader.ReadBoolean();
		}
		if (bitArray[i++])
		{
			this.drawAboveAlwaysFront = reader.ReadBoolean();
		}
		this.parent = location;
		this.loadTexture();
		switch (reader.ReadByte())
		{
		case 1:
			this.attachedCharacter = Game1.GetPlayer(reader.ReadInt64()) ?? Game1.MasterPlayer;
			break;
		case 2:
		{
			Guid guid = reader.ReadGuid();
			if (!location.characters.ContainsGuid(guid))
			{
				Game1.log.Warn($"Failed to find character with GUID {guid} for TemporaryAniamtedSprite.attachedCharacter");
			}
			else
			{
				this.attachedCharacter = location.characters[guid];
			}
			break;
		}
		}
	}

	private void checkDirty<T>(BitArray dirtyBits, ref int i, T value, T defaultValue = default(T))
	{
		dirtyBits[i++] = !object.Equals(value, defaultValue);
	}

	public void Write(BinaryWriter writer, GameLocation location)
	{
		if (base.GetType() != typeof(TemporaryAnimatedSprite))
		{
			throw new InvalidOperationException("TemporaryAnimatedSprite.Write is not implemented for other types");
		}
		BitArray dirtyBits = new BitArray(80);
		int i = 0;
		this.checkDirty(dirtyBits, ref i, this.interval, 200f);
		this.checkDirty(dirtyBits, ref i, this.currentParentTileIndex, 0);
		this.checkDirty(dirtyBits, ref i, this.oldCurrentParentTileIndex, 0);
		this.checkDirty(dirtyBits, ref i, this.initialParentTileIndex, 0);
		this.checkDirty(dirtyBits, ref i, this.totalNumberOfLoops, 0);
		this.checkDirty(dirtyBits, ref i, this.currentNumberOfLoops, 0);
		this.checkDirty(dirtyBits, ref i, this.xStopCoordinate, -1);
		this.checkDirty(dirtyBits, ref i, this.yStopCoordinate, -1);
		this.checkDirty(dirtyBits, ref i, this.animationLength, 0);
		this.checkDirty(dirtyBits, ref i, this.bombRadius, 0);
		this.checkDirty(dirtyBits, ref i, this.bombDamage, 0);
		this.checkDirty(dirtyBits, ref i, this.pingPongMotion, -1);
		this.checkDirty(dirtyBits, ref i, this.fireworkType, -1);
		this.checkDirty(dirtyBits, ref i, this.flicker, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.timeBasedMotion, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.overrideLocationDestroy, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.pingPong, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.holdLastFrame, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.pulse, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.extraInfoForEndBehavior, 0);
		this.checkDirty(dirtyBits, ref i, this.lightId);
		this.checkDirty(dirtyBits, ref i, this.bigCraftable, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.swordswipe, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.flash, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.flipped, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.verticalFlipped, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.local, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.lightFade, 0);
		this.checkDirty(dirtyBits, ref i, this.hasLit, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.xPeriodic, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.yPeriodic, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.destroyable, defaultValue: true);
		this.checkDirty(dirtyBits, ref i, this.paused, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.rotation, 0f);
		this.checkDirty(dirtyBits, ref i, this.alpha, 1f);
		this.checkDirty(dirtyBits, ref i, this.alphaFade, 0f);
		this.checkDirty(dirtyBits, ref i, this.layerDepth, -1f);
		this.checkDirty(dirtyBits, ref i, this.scale, 1f);
		this.checkDirty(dirtyBits, ref i, this.scaleChange, 0f);
		this.checkDirty(dirtyBits, ref i, this.scaleChangeChange, 0f);
		this.checkDirty(dirtyBits, ref i, this.rotationChange, 0f);
		this.checkDirty(dirtyBits, ref i, this.id, 0);
		this.checkDirty(dirtyBits, ref i, this.lightRadius, 0f);
		this.checkDirty(dirtyBits, ref i, this.xPeriodicRange, 0f);
		this.checkDirty(dirtyBits, ref i, this.yPeriodicRange, 0f);
		this.checkDirty(dirtyBits, ref i, this.xPeriodicLoopTime, 0f);
		this.checkDirty(dirtyBits, ref i, this.yPeriodicLoopTime, 0f);
		this.checkDirty(dirtyBits, ref i, this.shakeIntensityChange, 0f);
		this.checkDirty(dirtyBits, ref i, this.shakeIntensity, 0f);
		this.checkDirty(dirtyBits, ref i, this.pulseTime, 0f);
		this.checkDirty(dirtyBits, ref i, this.pulseAmount, 1.1f);
		this.checkDirty(dirtyBits, ref i, this.position);
		this.checkDirty(dirtyBits, ref i, this.sourceRectStartingPos);
		this.checkDirty(dirtyBits, ref i, this.sourceRect);
		this.checkDirty(dirtyBits, ref i, this.color, Color.White);
		this.checkDirty(dirtyBits, ref i, this.lightcolor, Color.White);
		this.checkDirty(dirtyBits, ref i, this.motion, Vector2.Zero);
		this.checkDirty(dirtyBits, ref i, this.acceleration, Vector2.Zero);
		this.checkDirty(dirtyBits, ref i, this.accelerationChange, Vector2.Zero);
		this.checkDirty(dirtyBits, ref i, this.initialPosition);
		this.checkDirty(dirtyBits, ref i, this.delayBeforeAnimationStart, 0);
		this.checkDirty(dirtyBits, ref i, this.ticksBeforeAnimationStart, 0);
		this.checkDirty(dirtyBits, ref i, this.startSound);
		this.checkDirty(dirtyBits, ref i, this.endSound);
		this.checkDirty(dirtyBits, ref i, this.text);
		this.checkDirty(dirtyBits, ref i, this.texture);
		this.checkDirty(dirtyBits, ref i, this.owner);
		this.checkDirty(dirtyBits, ref i, this.stopAcceleratingWhenVelocityIsZero, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.layerDepthOffset, 0f);
		this.checkDirty(dirtyBits, ref i, this.afterAccelStopMotionX, 0f);
		this.checkDirty(dirtyBits, ref i, this.afterAccelStopMotionY, 0f);
		this.checkDirty(dirtyBits, ref i, this.positionFollowsAttachedCharacter, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.dontClearOnAreaEntry, defaultValue: false);
		this.checkDirty(dirtyBits, ref i, this.drawAboveAlwaysFront, defaultValue: false);
		writer.WriteBitArray(dirtyBits);
		i = 0;
		if (dirtyBits[i++])
		{
			writer.Write(this.interval);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.currentParentTileIndex);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.oldCurrentParentTileIndex);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.initialParentTileIndex);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.totalNumberOfLoops);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.currentNumberOfLoops);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.xStopCoordinate);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.yStopCoordinate);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.animationLength);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.bombRadius);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.bombDamage);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.pingPongMotion);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.fireworkType);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.flicker);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.timeBasedMotion);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.overrideLocationDestroy);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.pingPong);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.holdLastFrame);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.pulse);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.extraInfoForEndBehavior);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.lightId);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.bigCraftable);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.swordswipe);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.flash);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.flipped);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.verticalFlipped);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.local);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.lightFade);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.hasLit);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.xPeriodic);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.yPeriodic);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.destroyable);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.paused);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.rotation);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.alpha);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.alphaFade);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.layerDepth);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.scale);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.scaleChange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.scaleChangeChange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.rotationChange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.id);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.lightRadius);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.xPeriodicRange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.yPeriodicRange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.xPeriodicLoopTime);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.yPeriodicLoopTime);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.shakeIntensityChange);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.shakeIntensity);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.pulseTime);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.pulseAmount);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.position);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.sourceRectStartingPos);
		}
		if (dirtyBits[i++])
		{
			writer.WriteRectangle(this.sourceRect);
		}
		if (dirtyBits[i++])
		{
			writer.WriteColor(this.color);
		}
		if (dirtyBits[i++])
		{
			writer.WriteColor(this.lightcolor);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.motion);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.acceleration);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.accelerationChange);
		}
		if (dirtyBits[i++])
		{
			writer.WriteVector2(this.initialPosition);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.delayBeforeAnimationStart);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.ticksBeforeAnimationStart);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.startSound);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.endSound);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.text);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.textureName);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.owner.uniqueMultiplayerID.Value);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.stopAcceleratingWhenVelocityIsZero);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.layerDepthOffset);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.afterAccelStopMotionX);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.afterAccelStopMotionY);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.positionFollowsAttachedCharacter);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.dontClearOnAreaEntry);
		}
		if (dirtyBits[i++])
		{
			writer.Write(this.drawAboveAlwaysFront);
		}
		Character character = this.attachedCharacter;
		if (character != null)
		{
			if (!(character is Farmer farmer))
			{
				if (!(character is NPC npc))
				{
					throw new ArgumentException();
				}
				writer.Write((byte)2);
				writer.WriteGuid(location.characters.GuidOf(npc));
			}
			else
			{
				writer.Write((byte)1);
				writer.Write(farmer.UniqueMultiplayerID);
			}
		}
		else
		{
			writer.Write((byte)0);
		}
	}

	public virtual void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
	{
		if (this.local)
		{
			localPosition = true;
		}
		if (this.currentParentTileIndex < 0 || this.delayBeforeAnimationStart > 0 || this.ticksBeforeAnimationStart > 0)
		{
			return;
		}
		if (this.text != null)
		{
			if (this.extraInfoForEndBehavior == -777)
			{
				Vector2 v = Game1.GlobalToLocal(this.position);
				SpriteText.drawString(spriteBatch, this.text, (int)v.X, (int)v.Y, 999999, -1, 999999, this.alpha, this.layerDepth, junimoText: false, -1, "", this.color.Equals(Color.White) ? SpriteText.color_White : SpriteText.color_Black);
			}
			else
			{
				spriteBatch.DrawString(Game1.dialogueFont, this.text, localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, this.Position), this.color * this.alpha * extraAlpha, this.rotation, Vector2.Zero, this.scale, SpriteEffects.None, this.layerDepth + this.layerDepthOffset);
			}
		}
		else if (this.Texture != null)
		{
			if (this.positionFollowsAttachedCharacter && this.attachedCharacter != null)
			{
				spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, this.attachedCharacter.Position + new Vector2((int)this.Position.X + xOffset, (int)this.Position.Y + yOffset))) + new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2) * this.scale + new Vector2((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0, (this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), this.sourceRect, this.color * this.alpha * extraAlpha, this.rotation, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), this.scale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
			}
			else if (!this.vectorScale.Equals(Vector2.Zero))
			{
				spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.Position.X + xOffset, (int)this.Position.Y + yOffset))) + new Vector2((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0, (this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), this.sourceRect, this.color * this.alpha * extraAlpha, this.rotation, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), this.vectorScale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
			}
			else
			{
				spriteBatch.Draw(this.Texture, (localPosition ? this.Position : Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.Position.X + xOffset, (int)this.Position.Y + yOffset))) + new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2) * this.scale + new Vector2((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0, (this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0), this.sourceRect, this.color * this.alpha * extraAlpha, this.rotation, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), this.scale, this.flipped ? SpriteEffects.FlipHorizontally : (this.verticalFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None), ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + (float)this.sourceRect.Height) / 10000f)) + this.layerDepthOffset);
			}
		}
		else if (this.bigCraftable)
		{
			spriteBatch.Draw(Game1.bigCraftableSpriteSheet, localPosition ? this.Position : (Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.Position.X + xOffset, (int)this.Position.Y + yOffset)) + new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2)), Object.getSourceRectForBigCraftable(this.currentParentTileIndex), Color.White * extraAlpha, 0f, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), this.scale, SpriteEffects.None, (this.Position.Y + 32f) / 10000f + this.layerDepthOffset);
		}
		else
		{
			if (this.swordswipe)
			{
				return;
			}
			if (this.attachedCharacter != null)
			{
				if (this.local)
				{
					this.attachedCharacter.Position = new Vector2((float)Game1.viewport.X + this.Position.X, (float)Game1.viewport.Y + this.Position.Y);
				}
				this.attachedCharacter.draw(spriteBatch);
			}
			else
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, localPosition ? this.Position : (Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.Position.X + xOffset, (int)this.Position.Y + yOffset)) + new Vector2(8f, 8f) * 4f + new Vector2((this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0, (this.shakeIntensity > 0f) ? Game1.random.Next(-(int)this.shakeIntensity, (int)this.shakeIntensity + 1) : 0)), GameLocation.getSourceRectForObject(this.currentParentTileIndex), (this.flash ? (Color.LightBlue * 0.85f) : this.color) * this.alpha * extraAlpha, this.rotation, new Vector2(8f, 8f), 4f * this.scale, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((this.layerDepth >= 0f) ? this.layerDepth : ((this.Position.Y + 32f) / 10000f)) + this.layerDepthOffset);
			}
		}
	}

	public void bounce(int extraInfo)
	{
		if ((float)extraInfo > 1f)
		{
			this.motion.Y = (float)(-extraInfo) / 2f;
			this.motion.X /= 2f;
			this.rotationChange = this.motion.Y / 50f;
			this.acceleration.Y = 0.7f;
			this.yStopCoordinate = (int)this.initialPosition.Y;
			this.parent?.playSound("thudStep");
		}
		else
		{
			if (this.extraInfoForEndBehavior != -777)
			{
				this.alphaFade = 0.01f;
			}
			this.motion.X = 0f;
		}
	}

	public void unload()
	{
		this.PlaySound(this.endSound);
		this.endFunction?.Invoke(this.extraInfoForEndBehavior);
		if (this.hasLit)
		{
			Utility.removeLightSource(this.lightId);
		}
	}

	public void reset()
	{
		this.sourceRect.X = (int)this.sourceRectStartingPos.X;
		this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
		this.currentParentTileIndex = 0;
		this.oldCurrentParentTileIndex = 0;
		this.timer = 0f;
		this.totalTimer = 0f;
		this.currentNumberOfLoops = 0;
		this.pingPongMotion = 1;
	}

	public void resetEnd()
	{
		this.reset();
		this.currentParentTileIndex = this.initialParentTileIndex + this.animationLength - 1;
	}

	public virtual bool update(GameTime time)
	{
		if (this.paused)
		{
			return false;
		}
		int elapsedMs = (int)time.ElapsedGameTime.TotalMilliseconds;
		if (this.usePreciseTiming)
		{
			if (this.stopWatch == null)
			{
				this.stopWatch = new Stopwatch();
				this.stopWatch.Start();
			}
			elapsedMs = (int)(this.stopWatch.ElapsedMilliseconds - this.previousStopwatchTime);
			this.previousStopwatchTime = this.stopWatch.ElapsedMilliseconds;
		}
		if (this.bombRadius > 0 && !Game1.shouldTimePass())
		{
			return false;
		}
		if (this.ticksBeforeAnimationStart > 0)
		{
			this.ticksBeforeAnimationStart--;
			return false;
		}
		if (this.delayBeforeAnimationStart > 0)
		{
			this.delayBeforeAnimationStart -= elapsedMs;
			if (this.delayBeforeAnimationStart <= 0)
			{
				this.PlaySound(this.startSound);
				this.timer = -this.delayBeforeAnimationStart;
			}
			if (this.delayBeforeAnimationStart <= 0 && this.parentSprite != null)
			{
				this.position = this.parentSprite.position + this.position;
			}
			return false;
		}
		if (float.IsNaN(this.motion.X))
		{
			this.motion.X = 0f;
		}
		if (float.IsNaN(this.motion.Y))
		{
			this.motion.Y = 0f;
		}
		this.timer += elapsedMs;
		this.totalTimer += elapsedMs;
		this.alpha -= this.alphaFade * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		this.alphaFade -= this.alphaFadeFade * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		if (this.alphaFade > 0f && this.lightId != null && this.alpha < 1f && this.alpha >= 0f)
		{
			LightSource ls = Utility.getLightSource(this.lightId);
			if (ls != null)
			{
				ls.color.A = (byte)(255f * this.alpha);
			}
		}
		this.shakeIntensity += this.shakeIntensityChange * (float)elapsedMs;
		this.scale += this.scaleChange * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		this.scaleChange += this.scaleChangeChange * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		this.rotation += this.rotationChange;
		if (this.xPeriodic)
		{
			this.position.X = this.initialPosition.X + this.xPeriodicRange * (float)Math.Sin(Math.PI * 2.0 / (double)this.xPeriodicLoopTime * (double)this.totalTimer);
		}
		else
		{
			this.position.X += this.motion.X * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		}
		if (this.yPeriodic)
		{
			this.position.Y = this.initialPosition.Y + this.yPeriodicRange * (float)Math.Sin(Math.PI * 2.0 / (double)this.yPeriodicLoopTime * (double)(this.totalTimer + this.yPeriodicLoopTime / 2f));
		}
		else
		{
			this.position.Y += this.motion.Y * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		}
		if (this.attachedCharacter != null && !this.positionFollowsAttachedCharacter)
		{
			if (this.xPeriodic)
			{
				this.attachedCharacter.position.X = this.initialPosition.X + this.xPeriodicRange * (float)Math.Sin(Math.PI * 2.0 / (double)this.xPeriodicLoopTime * (double)this.totalTimer);
			}
			else
			{
				this.attachedCharacter.position.X += this.motion.X * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
			}
			if (this.yPeriodic)
			{
				this.attachedCharacter.position.Y = this.initialPosition.Y + this.yPeriodicRange * (float)Math.Sin(Math.PI * 2.0 / (double)this.yPeriodicLoopTime * (double)this.totalTimer);
			}
			else
			{
				this.attachedCharacter.position.Y += this.motion.Y * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
			}
		}
		int sign = Math.Sign(this.motion.X);
		this.motion.X += this.acceleration.X * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		if (this.stopAcceleratingWhenVelocityIsZero && Math.Sign(this.motion.X) != sign)
		{
			this.motion.X = this.afterAccelStopMotionX;
			this.acceleration.X = 0f;
			this.accelerationChange.X = 0f;
		}
		sign = Math.Sign(this.motion.Y);
		this.motion.Y += this.acceleration.Y * (float)((!this.timeBasedMotion) ? 1 : elapsedMs);
		if (this.stopAcceleratingWhenVelocityIsZero && Math.Sign(this.motion.Y) != sign)
		{
			this.motion.Y = this.afterAccelStopMotionY;
			this.acceleration.Y = 0f;
			this.accelerationChange.Y = 0f;
		}
		this.acceleration.X += this.accelerationChange.X;
		this.acceleration.Y += this.accelerationChange.Y;
		if (this.xStopCoordinate != -1 || this.yStopCoordinate != -1)
		{
			int oldY = (int)this.motion.Y;
			if (this.xStopCoordinate != -1 && Math.Abs(this.position.X - (float)this.xStopCoordinate) <= Math.Abs(this.motion.X))
			{
				this.motion.X = 0f;
				this.acceleration.X = 0f;
				this.xStopCoordinate = -1;
			}
			if (this.yStopCoordinate != -1 && Math.Abs(this.position.Y - (float)this.yStopCoordinate) <= Math.Abs(this.motion.Y))
			{
				this.motion.Y = 0f;
				this.acceleration.Y = 0f;
				this.yStopCoordinate = -1;
			}
			if (this.xStopCoordinate == -1 && this.yStopCoordinate == -1)
			{
				this.rotationChange = 0f;
				this.reachedStopCoordinate?.Invoke(oldY);
				this.reachedStopCoordinateSprite?.Invoke(this);
			}
		}
		if (!this.pingPong)
		{
			this.pingPongMotion = 1;
		}
		if (this.pulse)
		{
			this.pulseTimer -= elapsedMs;
			if (this.originalScale == 0f)
			{
				this.originalScale = this.scale;
			}
			if (this.pulseTimer <= 0f)
			{
				this.pulseTimer = this.pulseTime;
				this.scale = this.originalScale * this.pulseAmount;
			}
			if (this.scale > this.originalScale)
			{
				this.scale -= this.pulseAmount / 100f * (float)elapsedMs;
			}
		}
		if (this.lightId != null)
		{
			if (!this.hasLit)
			{
				this.hasLit = true;
				if (this.parent == null || Game1.currentLocation == this.parent)
				{
					Game1.currentLightSources.Add(new LightSource(this.lightId, 4, this.position + new Vector2(32f, 32f), this.lightRadius, this.lightcolor.Equals(Color.White) ? new Color(0, 65, 128) : this.lightcolor, LightSource.LightContext.None, 0L)
					{
						fadeOut = { this.lightFade }
					});
				}
			}
			else
			{
				Utility.repositionLightSource(this.lightId, this.position + new Vector2(32f, 32f));
			}
		}
		if (this.alpha <= 0f || (this.position.X < -2000f && !this.overrideLocationDestroy) || this.scale <= 0f)
		{
			this.unload();
			return this.destroyable;
		}
		if (this.timer > this.interval)
		{
			this.currentParentTileIndex += this.pingPongMotion;
			this.sourceRect.X += this.sourceRect.Width * this.pingPongMotion;
			if (this.Texture != null)
			{
				if (!this.pingPong && this.sourceRect.X >= this.Texture.Width)
				{
					this.sourceRect.Y += this.sourceRect.Height;
				}
				if (!this.pingPong)
				{
					this.sourceRect.X %= this.Texture.Width;
				}
				if (this.pingPong)
				{
					if ((float)this.sourceRect.X + ((float)this.sourceRect.Y - this.sourceRectStartingPos.Y) / (float)this.sourceRect.Height * (float)this.Texture.Width >= this.sourceRectStartingPos.X + (float)(this.sourceRect.Width * this.animationLength))
					{
						this.pingPongMotion = -1;
						this.sourceRect.X -= this.sourceRect.Width * 2;
						this.currentParentTileIndex--;
						if (this.sourceRect.X < 0)
						{
							this.sourceRect.X = this.Texture.Width + this.sourceRect.X;
						}
					}
					else if ((float)this.sourceRect.X < this.sourceRectStartingPos.X && (float)this.sourceRect.Y == this.sourceRectStartingPos.Y)
					{
						this.pingPongMotion = 1;
						this.sourceRect.X = (int)this.sourceRectStartingPos.X + this.sourceRect.Width;
						this.currentParentTileIndex++;
						this.currentNumberOfLoops++;
						if (this.endFunction != null)
						{
							this.endFunction(this.extraInfoForEndBehavior);
							this.endFunction = null;
						}
						if (this.currentNumberOfLoops >= this.totalNumberOfLoops)
						{
							this.unload();
							return this.destroyable;
						}
					}
				}
				else if (this.totalNumberOfLoops >= 1 && (float)this.sourceRect.X + ((float)this.sourceRect.Y - this.sourceRectStartingPos.Y) / (float)this.sourceRect.Height * (float)this.Texture.Width >= this.sourceRectStartingPos.X + (float)(this.sourceRect.Width * this.animationLength))
				{
					this.sourceRect.X = (int)this.sourceRectStartingPos.X;
					this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
				}
			}
			this.timer -= this.interval;
			if (this.flicker)
			{
				if (this.currentParentTileIndex < 0 || this.flash)
				{
					this.currentParentTileIndex = this.oldCurrentParentTileIndex;
					this.flash = false;
				}
				else
				{
					this.oldCurrentParentTileIndex = this.currentParentTileIndex;
					if (this.bombRadius > 0)
					{
						this.flash = true;
					}
					else
					{
						this.currentParentTileIndex = -100;
					}
				}
			}
			if (this.currentParentTileIndex - this.initialParentTileIndex >= this.animationLength)
			{
				this.currentNumberOfLoops++;
				if (this.holdLastFrame)
				{
					this.currentParentTileIndex = this.initialParentTileIndex + this.animationLength - 1;
					if (this.texture != null)
					{
						this.setSourceRectToCurrentTileIndex();
					}
					if (this.endFunction != null)
					{
						this.endFunction(this.extraInfoForEndBehavior);
						this.endFunction = null;
					}
					return false;
				}
				this.currentParentTileIndex = this.initialParentTileIndex;
				if (this.currentNumberOfLoops >= this.totalNumberOfLoops)
				{
					if (this.bombRadius > 0)
					{
						if (Game1.currentLocation == this.parent)
						{
							Game1.flashAlpha = 1f;
						}
						if (Game1.IsMasterGame)
						{
							this.parent.netAudio.StopPlaying("fuse");
							this.parent.playSound("explosion");
							this.parent.explode(new Vector2((int)(this.position.X / 64f), (int)(this.position.Y / 64f)), this.bombRadius, this.owner, damageFarmers: true, this.bombDamage);
						}
					}
					if (this.fireworkType >= 0)
					{
						float mult = TemporaryAnimatedSprite.GetFireworkLifetimeMultiplier(this.fireworkType);
						Color col = TemporaryAnimatedSprite.GetFireworkColor(this.fireworkType);
						if (Game1.currentLocation == this.parent)
						{
							Game1.screenGlowOnce(col * 0.8f, hold: false);
						}
						if (Game1.IsMasterGame)
						{
							float outMult = 0.3f;
							float inDiv = this.id;
							Vector2[] fireworkLights = TemporaryAnimatedSprite.GetFireworkLights(this.fireworkType);
							Vector2[] points = TemporaryAnimatedSprite.GetFireworkPoints(this.fireworkType);
							List<TemporaryAnimatedSprite> fireworkSprites = new List<TemporaryAnimatedSprite>();
							Vector2[] array = fireworkLights;
							for (int i = 0; i < array.Length; i++)
							{
								Vector2 point = array[i];
								fireworkSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(0, 0, 1, 1), 1800f * mult, 1, 0, this.position, flicker: false, flipped: false, -1f, 0f, Color.Transparent, 1f, 0f, 0f, 0f)
								{
									motion = point,
									acceleration = point * outMult,
									accelerationChange = -point / inDiv,
									stopAcceleratingWhenVelocityIsZero = true,
									afterAccelStopMotionX = (float)Math.Sign(point.X) * 0.1f,
									afterAccelStopMotionY = 0.33f,
									layerDepthOffset = 320f,
									lightId = $"Firework_{this.id}_{point.X}_{point.Y}",
									lightRadius = 1.3f,
									drawAboveAlwaysFront = true,
									lightFade = 2
								});
							}
							array = points;
							for (int i = 0; i < array.Length; i++)
							{
								Vector2 point2 = array[i];
								fireworkSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(304, 364 + this.fireworkType * 11, 11, 11), 75f * mult + (float)Game1.random.Next(-20, 21), 12, 1, this.position, flicker: false, flipped: false, -1f, 0f, Color.White, 4f, 0f, (float)(Game1.random.NextDouble() * Math.PI) * 0.5f, 0f)
								{
									motion = point2,
									acceleration = point2 * outMult,
									accelerationChange = -point2 / inDiv,
									stopAcceleratingWhenVelocityIsZero = true,
									afterAccelStopMotionX = (float)Math.Sign(point2.X) * 0.1f,
									afterAccelStopMotionY = 0.33f,
									alpha = 1f,
									alphaFade = 0.01f,
									alphaFadeFade = 0.00025f,
									drawAboveAlwaysFront = true
								});
								int which = ((Game1.random.Next(3) != 0) ? 1 : 0);
								fireworkSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 64 * (10 + which), 64, 64), 100f * mult, (which == 0) ? 9 : 6, 2, this.position, flicker: false, flipped: false, -1f, 0f, Utility.getBlendedColor(col, Color.White), 1f, 0f, (float)(Game1.random.NextDouble() * Math.PI) * 0.5f, 0f)
								{
									motion = point2 * 0.75f,
									acceleration = point2 * outMult,
									accelerationChange = -point2 / inDiv,
									stopAcceleratingWhenVelocityIsZero = true,
									afterAccelStopMotionX = (float)Math.Sign(point2.X) * 0.1f,
									afterAccelStopMotionY = 0.33f,
									drawAboveAlwaysFront = true,
									alpha = 0.5f,
									delayBeforeAnimationStart = Game1.random.Next(50, 100)
								});
							}
							if (this.id == 30)
							{
								for (int j = 0; j < 8; j++)
								{
									Vector2 mot = points[Game1.random.Next(points.Length)];
									fireworkSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(304, 397, 11, 11), 75f * mult, 12, 5, this.position, flicker: false, flipped: false, -1f, 0f, Utility.getBlendedColor(Color.White, Utility.getRandomRainbowColor()), 4f, 0f, 0f, 0f)
									{
										motion = mot * 1.1f,
										alpha = 1f,
										alphaFade = 0.01f,
										acceleration = mot * outMult,
										accelerationChange = -mot / ((float)this.id * 1.25f),
										stopAcceleratingWhenVelocityIsZero = true,
										afterAccelStopMotionX = (float)Math.Sign(mot.X) * 0.1f,
										afterAccelStopMotionY = 0.33f,
										drawAboveAlwaysFront = true,
										lightId = $"Firework_{this.id}_{j}",
										lightRadius = 0.33f,
										lightFade = 3
									});
								}
							}
							Game1.multiplayer.broadcastSprites(this.parent, fireworkSprites.ToArray());
							this.parent.netAudio.StopPlaying("fuse");
						}
					}
					this.unload();
					return this.destroyable;
				}
				if (this.bombRadius > 0 && this.currentNumberOfLoops == this.totalNumberOfLoops - 5)
				{
					this.interval -= this.interval / 3f;
				}
			}
		}
		return false;
	}

	public bool clearOnAreaEntry()
	{
		if (this.dontClearOnAreaEntry)
		{
			return false;
		}
		if (this.bombRadius > 0)
		{
			return false;
		}
		return true;
	}

	private void setSourceRectToCurrentTileIndex()
	{
		this.sourceRect.X = (int)(this.sourceRectStartingPos.X + (float)(this.currentParentTileIndex * this.sourceRect.Width)) % this.texture.Width;
		if (this.sourceRect.X < 0)
		{
			this.sourceRect.X = 0;
		}
		this.sourceRect.Y = (int)this.sourceRectStartingPos.Y;
	}

	/// <summary>Play a sound locally, preferring the parent location if possible.</summary>
	/// <param name="sound">The sound to play.</param>
	private void PlaySound(string sound)
	{
		if (sound != null)
		{
			if (this.parent == null)
			{
				Game1.playSound(sound);
			}
			else
			{
				this.parent.localSound(sound);
			}
		}
	}

	public static TemporaryAnimatedSprite CreateFromData(TemporaryAnimatedSpriteDefinition temporarySprite, float x, float y, float sortLayer)
	{
		return new TemporaryAnimatedSprite(temporarySprite.Texture, temporarySprite.SourceRect, temporarySprite.Interval, temporarySprite.Frames, temporarySprite.Loops, new Vector2(x, y) * 64f + temporarySprite.PositionOffset * 4f, temporarySprite.Flicker, temporarySprite.Flip, sortLayer + temporarySprite.SortOffset, temporarySprite.AlphaFade, Utility.StringToColor(temporarySprite.Color) ?? Color.White, temporarySprite.Scale * 4f, temporarySprite.ScaleChange, temporarySprite.Rotation, temporarySprite.RotationChange);
	}
}
