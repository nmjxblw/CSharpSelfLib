using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;

namespace StardewValley;

public class DebugMetricsComponent : DrawableGameComponent
{
	private readonly Game _game;

	private SpriteFont _font;

	private SpriteBatch _spriteBatch;

	private int _drawX;

	private int _drawY;

	private double _fps;

	private double _mspf;

	private int _lastCollection;

	private float _lastBaseMB;

	private bool _runningSlowly;

	private StringBuilder _stringBuilder = new StringBuilder(512);

	private Texture2D _opaqueWhite;

	public int XOffset = 10;

	public int YOffset = 10;

	private IBandwidthMonitor bandwidthMonitor;

	private BarGraph bandwidthUpGraph;

	private BarGraph bandwidthDownGraph;

	public SpriteFont Font
	{
		get
		{
			return this._font;
		}
		set
		{
			this._font = value;
		}
	}

	public DebugMetricsComponent(Game game)
		: base(game)
	{
		this._game = game;
		base.DrawOrder = int.MaxValue;
	}

	protected override void LoadContent()
	{
		this._spriteBatch = new SpriteBatch(base.GraphicsDevice);
		int w = 2;
		int h = 2;
		this._opaqueWhite = new Texture2D(base.GraphicsDevice, w, h, mipmap: false, SurfaceFormat.Color)
		{
			Name = "@DebugMetricsComponent._opaqueWhite"
		};
		Color[] data = new Color[w * h];
		this._opaqueWhite.GetData(data);
		for (int i = 0; i < w * h; i++)
		{
			data[i] = Color.White;
		}
		this._opaqueWhite.SetData(data);
		base.LoadContent();
	}

	public override void Update(GameTime gameTime)
	{
		if (Game1.IsServer)
		{
			this.bandwidthMonitor = Game1.server;
		}
		else if (Game1.IsClient)
		{
			this.bandwidthMonitor = Game1.client;
		}
		else
		{
			this.bandwidthMonitor = null;
		}
		if (this.bandwidthMonitor == null || !this.bandwidthMonitor.LogBandwidth)
		{
			this.bandwidthDownGraph = null;
			this.bandwidthUpGraph = null;
		}
		if (this.bandwidthMonitor != null && this.bandwidthMonitor.LogBandwidth && (this.bandwidthDownGraph == null || this.bandwidthUpGraph == null))
		{
			int barGraphWidth = 200;
			int barGraphHeight = 150;
			int buffer = 50;
			this.bandwidthUpGraph = new BarGraph(this.bandwidthMonitor.BandwidthLogger.LoggedAvgBitsUp, Game1.uiViewport.Width - barGraphWidth - buffer, buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Yellow * 0.8f, this._opaqueWhite);
			this.bandwidthDownGraph = new BarGraph(this.bandwidthMonitor.BandwidthLogger.LoggedAvgBitsDown, Game1.uiViewport.Width - barGraphWidth - buffer, buffer + barGraphHeight + buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Cyan * 0.8f, this._opaqueWhite);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		if (!Game1.displayHUD || !Game1.debugMode)
		{
			return;
		}
		if (gameTime.ElapsedGameTime.TotalSeconds > 0.0)
		{
			this._fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
			this._mspf = gameTime.ElapsedGameTime.TotalSeconds * 1000.0;
		}
		if (gameTime.IsRunningSlowly)
		{
			this._runningSlowly = true;
		}
		if (this._font == null)
		{
			return;
		}
		this._spriteBatch.Begin();
		this._drawX = this.XOffset;
		this._drawY = this.YOffset;
		StringBuilder sb = this._stringBuilder;
		Utility.makeSafe(ref this._drawX, ref this._drawY, 0, 0);
		int collection = GC.CollectionCount(0);
		float memory = (float)GC.GetTotalMemory(forceFullCollection: false) / 1048576f;
		if (this._lastCollection != collection)
		{
			this._lastCollection = collection;
			this._lastBaseMB = memory;
		}
		float diff = memory - this._lastBaseMB;
		sb.AppendFormatEx("FPS: {0,3}   GC: {1,3}   {2:0.00}MB   +{3:0.00}MB", (int)Math.Round(this._fps), this._lastCollection % 1000, this._lastBaseMB, diff);
		Color col = Color.Yellow;
		if (this._runningSlowly)
		{
			sb.Append("   [IsRunningSlowly]");
			this._runningSlowly = false;
			col = Color.Red;
		}
		this.DrawLine(col, sb, this._drawX);
		if (Game1.IsMultiplayer)
		{
			col = Color.Yellow;
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> farmer in Game1.otherFarmers)
				{
					sb.AppendFormat("Ping({0}): {1:0.0}ms", farmer.Value.Name, Game1.server.getPingToClient(farmer.Key));
					this.DrawLine(col, sb, this._drawX);
				}
			}
			else
			{
				sb.AppendFormat("Ping: {0:0.0}ms", Game1.client.GetPingToHost());
				this.DrawLine(col, sb, this._drawX);
			}
		}
		if (this.bandwidthMonitor != null && this.bandwidthMonitor.LogBandwidth)
		{
			sb.AppendFormat("Up - b/s: {0}  Avg b/s: {1}", (int)this.bandwidthMonitor.BandwidthLogger.BitsUpPerSecond, (int)this.bandwidthMonitor.BandwidthLogger.AvgBitsUpPerSecond);
			this.DrawLine(col, sb, this._drawX);
			sb.AppendFormat("Down - b/s: {0}  Avg b/s: {1}", (int)this.bandwidthMonitor.BandwidthLogger.BitsDownPerSecond, (int)this.bandwidthMonitor.BandwidthLogger.AvgBitsDownPerSecond);
			this.DrawLine(col, sb, this._drawX);
			sb.AppendFormat("Total MB Up: {0:0.00}  Total MB Down: {1:0.00}  Total Seconds: {2:0.00}", (float)this.bandwidthMonitor.BandwidthLogger.TotalBitsUp / 8f / 1000f / 1000f, (float)this.bandwidthMonitor.BandwidthLogger.TotalBitsDown / 8f / 1000f / 1000f, (float)this.bandwidthMonitor.BandwidthLogger.TotalMs / 1000f);
			this.DrawLine(col, sb, this._drawX);
			if (this.bandwidthUpGraph != null && this.bandwidthDownGraph != null)
			{
				this.bandwidthUpGraph.Draw(this._spriteBatch);
				this.bandwidthDownGraph.Draw(this._spriteBatch);
			}
		}
		this._spriteBatch.End();
	}

	private void DrawLine(Color color, StringBuilder sb, int x)
	{
		if (sb != null)
		{
			Vector2 size = this._font.MeasureString(sb);
			int y = this._drawY;
			int yoffset = this._font.LineSpacing;
			yoffset -= yoffset / 10;
			this._spriteBatch.Draw(this._opaqueWhite, new Rectangle(x - 1, y, (int)size.X + 2, yoffset), null, Color.Black * 0.5f);
			this._spriteBatch.DrawString(this._font, sb, new Vector2(x, y), color);
			this._drawY += yoffset;
			sb.Clear();
		}
	}
}
