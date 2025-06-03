using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley;

public class NPCController
{
	public delegate void endBehavior();

	public Character puppet;

	private bool loop;

	private bool destroyAtNextTurn;

	private List<Vector2> path;

	private Vector2 target;

	private int pathIndex;

	private int pauseTime = -1;

	private int speed;

	private endBehavior behaviorAtEnd;

	private int CurrentPathX
	{
		get
		{
			if (this.pathIndex >= this.path.Count)
			{
				return 0;
			}
			return (int)this.path[this.pathIndex].X;
		}
	}

	private int CurrentPathY
	{
		get
		{
			if (this.pathIndex >= this.path.Count)
			{
				return 0;
			}
			return (int)this.path[this.pathIndex].Y;
		}
	}

	private bool MovingHorizontally => this.CurrentPathX != 0;

	public NPCController(Character n, List<Vector2> path, bool loop, endBehavior endBehavior = null)
	{
		if (n != null)
		{
			this.speed = n.speed;
			this.loop = loop;
			this.puppet = n;
			this.path = path;
			this.setMoving(newTarget: true);
			this.behaviorAtEnd = endBehavior;
		}
	}

	public void destroyAtNextCrossroad()
	{
		this.destroyAtNextTurn = true;
	}

	private bool setMoving(bool newTarget)
	{
		if (this.puppet != null && this.pathIndex < this.path.Count)
		{
			int facingDirection = 2;
			if (this.CurrentPathX > 0)
			{
				facingDirection = 1;
			}
			else if (this.CurrentPathX < 0)
			{
				facingDirection = 3;
			}
			else if (this.CurrentPathY < 0)
			{
				facingDirection = 0;
			}
			else if (this.CurrentPathY > 0)
			{
				facingDirection = 2;
			}
			this.puppet.Halt();
			this.puppet.faceDirection(facingDirection);
			if (this.CurrentPathX != 0 && this.CurrentPathY != 0)
			{
				this.pauseTime = this.CurrentPathY;
				facingDirection = this.CurrentPathX % 4;
				this.puppet.faceDirection(facingDirection);
				return true;
			}
			this.puppet.setMovingInFacingDirection();
			if (newTarget)
			{
				this.target = new Vector2(this.puppet.Position.X + (float)(this.CurrentPathX * 64), this.puppet.Position.Y + (float)(this.CurrentPathY * 64));
			}
			return true;
		}
		return false;
	}

	public bool update(GameTime time, GameLocation location, List<NPCController> allControllers)
	{
		this.puppet.speed = this.speed;
		bool reachedMeYet = false;
		foreach (NPCController n in allControllers)
		{
			if (n.puppet == null)
			{
				continue;
			}
			if (n.puppet.Equals(this.puppet))
			{
				reachedMeYet = true;
			}
			if (n.puppet.FacingDirection == this.puppet.FacingDirection && !n.puppet.Equals(this.puppet) && n.puppet.GetBoundingBox().Intersects(this.puppet.nextPosition(this.puppet.FacingDirection)))
			{
				if (reachedMeYet)
				{
					break;
				}
				return false;
			}
		}
		if (this.puppet is Farmer player)
		{
			player.setRunning(isRunning: false, force: true);
			player.speed = 2;
			player.ignoreCollisions = true;
			if (Game1.CurrentEvent != null && Game1.CurrentEvent.farmer != this.puppet)
			{
				player.updateMovementAnimation(time);
			}
		}
		this.puppet.MovePosition(time, Game1.viewport, location);
		if (this.pauseTime < 0 && !this.puppet.isMoving())
		{
			this.setMoving(newTarget: false);
		}
		if (this.pauseTime < 0 && Math.Abs(Vector2.Distance(this.puppet.Position, this.target)) <= (float)this.puppet.Speed)
		{
			this.pathIndex++;
			if (this.destroyAtNextTurn)
			{
				return true;
			}
			if (!this.setMoving(newTarget: true))
			{
				if (this.loop)
				{
					this.pathIndex = 0;
					this.setMoving(newTarget: true);
				}
				else if (Game1.currentMinigame == null)
				{
					this.behaviorAtEnd?.Invoke();
					return true;
				}
			}
		}
		else if (this.pauseTime >= 0)
		{
			this.pauseTime -= time.ElapsedGameTime.Milliseconds;
			if (this.pauseTime < 0)
			{
				this.pathIndex++;
				if (this.destroyAtNextTurn)
				{
					return true;
				}
				if (!this.setMoving(newTarget: true))
				{
					if (this.loop)
					{
						this.pathIndex = 0;
						this.setMoving(newTarget: true);
					}
					else if (Game1.currentMinigame == null)
					{
						this.behaviorAtEnd?.Invoke();
						return true;
					}
				}
			}
		}
		return false;
	}
}
