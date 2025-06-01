using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace StardewValley;

public class DebugTimings
{
	private static readonly Vector2 DrawPos = Vector2.One * 12f;

	private readonly Stopwatch StopwatchDraw = new Stopwatch();

	private readonly Stopwatch StopwatchUpdate = new Stopwatch();

	private double LastTimingDraw;

	private double LastTimingUpdate;

	private float DrawTextWidth = -1f;

	private bool Active;

	public bool Toggle()
	{
		if ((!(Game1.game1?.IsMainInstance)) ?? true)
		{
			return false;
		}
		this.Active = !this.Active;
		return this.Active;
	}

	public void StartDrawTimer()
	{
		if (this.Active && (Game1.game1?.IsMainInstance ?? false))
		{
			this.StopwatchDraw.Restart();
		}
	}

	public void StopDrawTimer()
	{
		if (this.Active && (Game1.game1?.IsMainInstance ?? false))
		{
			this.StopwatchDraw.Stop();
			this.LastTimingDraw = this.StopwatchDraw.Elapsed.TotalMilliseconds;
		}
	}

	public void StartUpdateTimer()
	{
		if (this.Active && (Game1.game1?.IsMainInstance ?? false))
		{
			this.StopwatchUpdate.Restart();
		}
	}

	public void StopUpdateTimer()
	{
		if (this.Active && (Game1.game1?.IsMainInstance ?? false))
		{
			this.StopwatchUpdate.Stop();
			this.LastTimingUpdate = this.StopwatchUpdate.Elapsed.TotalMilliseconds;
		}
	}

	public void Draw()
	{
		if (!this.Active)
		{
			return;
		}
		bool? flag = Game1.game1?.IsMainInstance;
		if (flag.HasValue && flag == true && Game1.spriteBatch != null && Game1.dialogueFont != null)
		{
			if (this.DrawTextWidth <= 0f)
			{
				this.DrawTextWidth = Game1.dialogueFont.MeasureString($"Draw time: {0:00.00} ms  ").X;
			}
			Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, 64), Color.Black * 0.5f);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, $"Draw time: {this.LastTimingDraw:00.00} ms  ", DebugTimings.DrawPos, Color.White);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, $"Update time: {this.LastTimingUpdate:00.00} ms", new Vector2(DebugTimings.DrawPos.X + this.DrawTextWidth, DebugTimings.DrawPos.Y), Color.White);
		}
	}
}
