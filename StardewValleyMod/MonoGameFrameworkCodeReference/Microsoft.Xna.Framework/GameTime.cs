using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Holds the time state of a <see cref="T:Microsoft.Xna.Framework.Game" />.
/// </summary>
public class GameTime
{
	/// <summary>
	/// Time since the start of the <see cref="T:Microsoft.Xna.Framework.Game" />.
	/// </summary>
	public TimeSpan TotalGameTime { get; set; }

	/// <summary>
	/// Time since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.
	/// </summary>
	public TimeSpan ElapsedGameTime { get; set; }

	/// <summary>
	/// Indicates whether the <see cref="T:Microsoft.Xna.Framework.Game" /> is running slowly.
	///
	/// This flag is set to <c>true</c> when <see cref="P:Microsoft.Xna.Framework.Game.IsFixedTimeStep" /> is set to <c>true</c>
	/// and a tick of the game loop takes longer than <see cref="P:Microsoft.Xna.Framework.Game.TargetElapsedTime" /> for
	/// a few frames in a row.
	/// </summary>
	public bool IsRunningSlowly { get; set; }

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.GameTime" /> instance with a <see cref="P:Microsoft.Xna.Framework.GameTime.TotalGameTime" /> and
	/// <see cref="P:Microsoft.Xna.Framework.GameTime.ElapsedGameTime" /> of <code>0</code>.
	/// </summary>
	public GameTime()
	{
		this.TotalGameTime = TimeSpan.Zero;
		this.ElapsedGameTime = TimeSpan.Zero;
		this.IsRunningSlowly = false;
	}

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.GameTime" /> with the specified <see cref="P:Microsoft.Xna.Framework.GameTime.TotalGameTime" />
	/// and <see cref="P:Microsoft.Xna.Framework.GameTime.ElapsedGameTime" />.
	/// </summary>
	/// <param name="totalGameTime">The total game time elapsed since the start of the <see cref="T:Microsoft.Xna.Framework.Game" />.</param>
	/// <param name="elapsedGameTime">The time elapsed since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.</param>
	public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
	{
		this.TotalGameTime = totalGameTime;
		this.ElapsedGameTime = elapsedGameTime;
		this.IsRunningSlowly = false;
	}

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.GameTime" /> with the specified <see cref="P:Microsoft.Xna.Framework.GameTime.TotalGameTime" />
	/// and <see cref="P:Microsoft.Xna.Framework.GameTime.ElapsedGameTime" />.
	/// </summary>
	/// <param name="totalRealTime">The total game time elapsed since the start of the <see cref="T:Microsoft.Xna.Framework.Game" />.</param>
	/// <param name="elapsedRealTime">The time elapsed since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.</param>
	/// <param name="isRunningSlowly">A value indicating if the <see cref="T:Microsoft.Xna.Framework.Game" /> is running slowly.</param>
	public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
	{
		this.TotalGameTime = totalRealTime;
		this.ElapsedGameTime = elapsedRealTime;
		this.IsRunningSlowly = isRunningSlowly;
	}
}
