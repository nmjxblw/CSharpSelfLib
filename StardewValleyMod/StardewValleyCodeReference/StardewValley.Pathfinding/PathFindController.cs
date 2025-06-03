using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;
using xTile.Tiles;

namespace StardewValley.Pathfinding;

/// This class finds a path from one point to another using the A* pathfinding algorithm. Then it will guide the given character along that path.
/// Can only be used on maps where the tile width and height are each 127 or less. 
[InstanceStatics]
public class PathFindController
{
	public delegate bool isAtEnd(PathNode currentNode, Point endPoint, GameLocation location, Character c);

	public delegate void endBehavior(Character c, GameLocation location);

	public const byte impassable = byte.MaxValue;

	public const int timeToWaitBeforeCancelling = 5000;

	private Character character;

	public GameLocation location;

	public Stack<Point> pathToEndPoint;

	public Point endPoint;

	public int finalFacingDirection;

	public int pausedTimer;

	public endBehavior endBehaviorFunction;

	public bool nonDestructivePathing;

	public bool allowPlayerPathingInEvent;

	public bool NPCSchedule;

	protected static readonly sbyte[,] Directions = new sbyte[4, 2]
	{
		{ -1, 0 },
		{ 1, 0 },
		{ 0, 1 },
		{ 0, -1 }
	};

	protected static PriorityQueue _openList = new PriorityQueue();

	protected static HashSet<int> _closedList = new HashSet<int>();

	protected static int _counter = 0;

	public int timerSinceLastCheckPoint;

	public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection)
		: this(c, location, isAtEndPoint, finalFacingDirection, null, 10000, endPoint)
	{
	}

	public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, endBehavior endBehaviorFunction)
		: this(c, location, isAtEndPoint, finalFacingDirection, null, 10000, endPoint)
	{
		this.endPoint = endPoint;
		this.endBehaviorFunction = endBehaviorFunction;
	}

	public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, endBehavior endBehaviorFunction, int limit)
		: this(c, location, isAtEndPoint, finalFacingDirection, null, limit, endPoint)
	{
		this.endPoint = endPoint;
		this.endBehaviorFunction = endBehaviorFunction;
	}

	public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, bool clearMarriageDialogues = true)
		: this(c, location, isAtEndPoint, finalFacingDirection, null, 10000, endPoint, clearMarriageDialogues)
	{
	}

	public static bool isAtEndPoint(PathNode currentNode, Point endPoint, GameLocation location, Character c)
	{
		if (currentNode.x == endPoint.X)
		{
			return currentNode.y == endPoint.Y;
		}
		return false;
	}

	public PathFindController(Stack<Point> pathToEndPoint, GameLocation location, Character c, Point endPoint)
	{
		this.pathToEndPoint = pathToEndPoint;
		this.location = location;
		this.character = c;
		this.endPoint = endPoint;
	}

	public PathFindController(Stack<Point> pathToEndPoint, Character c, GameLocation l)
	{
		this.pathToEndPoint = pathToEndPoint;
		this.character = c;
		this.location = l;
		this.NPCSchedule = true;
	}

	public PathFindController(Character c, GameLocation location, isAtEnd endFunction, int finalFacingDirection, endBehavior endBehaviorFunction, int limit, Point endPoint, bool clearMarriageDialogues = true)
	{
		this.character = c;
		NPC npc = c as NPC;
		if (npc != null && npc.CurrentDialogue.Count > 0 && npc.CurrentDialogue.Peek().removeOnNextMove)
		{
			npc.CurrentDialogue.Pop();
		}
		if (npc != null && clearMarriageDialogues)
		{
			if (npc.currentMarriageDialogue.Count > 0)
			{
				npc.currentMarriageDialogue.Clear();
			}
			npc.shouldSayMarriageDialogue.Value = false;
		}
		this.location = location;
		this.endBehaviorFunction = endBehaviorFunction;
		if (endPoint == Point.Zero)
		{
			endPoint = c.TilePoint;
		}
		this.finalFacingDirection = finalFacingDirection;
		if (!(this.character is NPC) && !this.isPlayerPresent() && endFunction == new isAtEnd(isAtEndPoint) && endPoint.X > 0 && endPoint.Y > 0)
		{
			this.character.Position = new Vector2(endPoint.X * 64, endPoint.Y * 64 - 32);
		}
		else
		{
			this.pathToEndPoint = PathFindController.findPath(c.TilePoint, endPoint, endFunction, location, this.character, limit);
		}
	}

	public bool isPlayerPresent()
	{
		return this.location.farmers.Any();
	}

	public virtual bool update(GameTime time)
	{
		if (this.pathToEndPoint == null || this.pathToEndPoint.Count == 0)
		{
			return true;
		}
		if (!this.NPCSchedule && !this.isPlayerPresent() && this.endPoint.X > 0 && this.endPoint.Y > 0)
		{
			this.character.Position = new Vector2(this.endPoint.X * 64, this.endPoint.Y * 64 - 32);
			return true;
		}
		if (Game1.activeClickableMenu == null || Game1.IsMultiplayer)
		{
			this.timerSinceLastCheckPoint += time.ElapsedGameTime.Milliseconds;
			Vector2 position = this.character.Position;
			this.moveCharacter(time);
			if (this.character.Position.Equals(position))
			{
				this.pausedTimer += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.pausedTimer = 0;
			}
			if (!this.NPCSchedule && this.pausedTimer > 5000)
			{
				return true;
			}
		}
		return false;
	}

	public static Stack<Point> findPath(Point startPoint, Point endPoint, isAtEnd endPointFunction, GameLocation location, Character character, int limit)
	{
		if (Interlocked.Increment(ref PathFindController._counter) != 1)
		{
			throw new Exception();
		}
		try
		{
			bool ignore_obstructions = character is FarmAnimal animal && animal.CanSwim() && animal.isSwimming.Value;
			PathFindController._openList.Clear();
			PathFindController._closedList.Clear();
			PriorityQueue openList = PathFindController._openList;
			HashSet<int> closedList = PathFindController._closedList;
			int iterations = 0;
			openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
			int layerWidth = location.map.Layers[0].LayerWidth;
			int layerHeight = location.map.Layers[0].LayerHeight;
			while (!openList.IsEmpty())
			{
				PathNode currentNode = openList.Dequeue();
				if (endPointFunction(currentNode, endPoint, location, character))
				{
					return PathFindController.reconstructPath(currentNode);
				}
				closedList.Add(currentNode.id);
				int ng = (byte)(currentNode.g + 1);
				for (int i = 0; i < 4; i++)
				{
					int nx = currentNode.x + PathFindController.Directions[i, 0];
					int ny = currentNode.y + PathFindController.Directions[i, 1];
					int nid = PathNode.ComputeHash(nx, ny);
					if (closedList.Contains(nid))
					{
						continue;
					}
					if ((nx != endPoint.X || ny != endPoint.Y) && (nx < 0 || ny < 0 || nx >= layerWidth || ny >= layerHeight))
					{
						closedList.Add(nid);
						continue;
					}
					PathNode neighbor = new PathNode(nx, ny, currentNode);
					neighbor.g = (byte)(currentNode.g + 1);
					if (!ignore_obstructions && location.isCollidingPosition(new Rectangle(neighbor.x * 64 + 1, neighbor.y * 64 + 1, 62, 62), Game1.viewport, character is Farmer, 0, glider: false, character, pathfinding: true))
					{
						closedList.Add(nid);
						continue;
					}
					int f = ng + (Math.Abs(endPoint.X - nx) + Math.Abs(endPoint.Y - ny));
					closedList.Add(nid);
					openList.Enqueue(neighbor, f);
				}
				iterations++;
				if (iterations >= limit)
				{
					return null;
				}
			}
			return null;
		}
		finally
		{
			if (Interlocked.Decrement(ref PathFindController._counter) != 0)
			{
				throw new Exception();
			}
		}
	}

	public static Stack<Point> reconstructPath(PathNode finalNode)
	{
		Stack<Point> path = new Stack<Point>();
		path.Push(new Point(finalNode.x, finalNode.y));
		for (PathNode walk = finalNode.parent; walk != null; walk = walk.parent)
		{
			path.Push(new Point(walk.x, walk.y));
		}
		return path;
	}

	protected virtual void moveCharacter(GameTime time)
	{
		Point peek = this.pathToEndPoint.Peek();
		Rectangle targetTile = new Rectangle(peek.X * 64, peek.Y * 64, 64, 64);
		targetTile.Inflate(-2, 0);
		Rectangle bbox = this.character.GetBoundingBox();
		if ((targetTile.Contains(bbox) || (bbox.Width > targetTile.Width && targetTile.Contains(bbox.Center))) && targetTile.Bottom - bbox.Bottom >= 2)
		{
			this.timerSinceLastCheckPoint = 0;
			this.pathToEndPoint.Pop();
			this.character.stopWithoutChangingFrame();
			if (this.pathToEndPoint.Count == 0)
			{
				this.character.Halt();
				if (this.finalFacingDirection != -1)
				{
					this.character.faceDirection(this.finalFacingDirection);
				}
				if (this.NPCSchedule)
				{
					NPC npc = this.character as NPC;
					npc.DirectionsToNewLocation = null;
					npc.endOfRouteMessage.Value = npc.nextEndOfRouteMessage;
				}
				this.endBehaviorFunction?.Invoke(this.character, this.location);
			}
			return;
		}
		if (this.character is Farmer farmer)
		{
			farmer.movementDirections.Clear();
		}
		else if (!(this.location is MovieTheater))
		{
			string name = this.character.Name;
			for (int i = 0; i < this.location.characters.Count; i++)
			{
				NPC c = this.location.characters[i];
				if (!c.Equals(this.character) && c.GetBoundingBox().Intersects(bbox) && c.isMoving() && string.Compare(c.Name, name, StringComparison.Ordinal) < 0)
				{
					this.character.Halt();
					return;
				}
			}
		}
		if (bbox.Left < targetTile.Left && bbox.Right < targetTile.Right)
		{
			this.character.SetMovingRight(b: true);
		}
		else if (bbox.Right > targetTile.Right && bbox.Left > targetTile.Left)
		{
			this.character.SetMovingLeft(b: true);
		}
		else if (bbox.Top <= targetTile.Top)
		{
			this.character.SetMovingDown(b: true);
		}
		else if (bbox.Bottom >= targetTile.Bottom - 2)
		{
			this.character.SetMovingUp(b: true);
		}
		this.character.MovePosition(time, Game1.viewport, this.location);
		if (this.nonDestructivePathing)
		{
			if (targetTile.Intersects(this.character.nextPosition(this.character.FacingDirection)))
			{
				Vector2 next_position = this.character.nextPositionVector2();
				Object next_tile_object = this.location.getObjectAt((int)next_position.X, (int)next_position.Y);
				if (next_tile_object != null)
				{
					if (next_tile_object is Fence fence && fence.isGate.Value)
					{
						fence.toggleGate(open: true);
					}
					else if (!next_tile_object.isPassable())
					{
						this.character.Halt();
						this.character.controller = null;
						return;
					}
				}
			}
			this.handleWarps(this.character.nextPosition(this.character.getDirection()));
		}
		else if (this.NPCSchedule)
		{
			this.handleWarps(this.character.nextPosition(this.character.getDirection()));
		}
	}

	public void handleWarps(Rectangle position)
	{
		Warp w = this.location.isCollidingWithWarpOrDoor(position, this.character);
		if (w == null)
		{
			return;
		}
		if (w.TargetName == "Trailer" && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			w = new Warp(w.X, w.Y, "Trailer_Big", 13, 24, flipFarmer: false);
		}
		if (this.character is NPC spouse && spouse.isMarried() && spouse.followSchedule)
		{
			GameLocation gameLocation = this.location;
			if (!(gameLocation is FarmHouse))
			{
				if (gameLocation is BusStop && w.X <= 9)
				{
					GameLocation home = spouse.getHome();
					Point homeEntry = ((FarmHouse)home).getEntryLocation();
					w = new Warp(w.X, w.Y, home.name.Value, homeEntry.X, homeEntry.Y, flipFarmer: false);
				}
			}
			else
			{
				w = new Warp(w.X, w.Y, "BusStop", 10, 23, flipFarmer: false);
			}
			if (spouse.temporaryController != null && spouse.controller != null)
			{
				spouse.controller.location = Game1.RequireLocation(w.TargetName);
			}
		}
		string targetLocationName = w.TargetName;
		foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out var newName))
			{
				targetLocationName = newName;
				break;
			}
		}
		if (this.character is NPC npc && (w.TargetName == "FarmHouse" || w.TargetName == "Cabin") && npc.isMarried() && npc.getSpouse() != null)
		{
			this.location = Utility.getHomeOfFarmer(npc.getSpouse());
			Point entryPoint = ((FarmHouse)this.location).getEntryLocation();
			w = new Warp(w.X, w.Y, this.location.name.Value, entryPoint.X, entryPoint.Y, flipFarmer: false);
			if (npc.temporaryController != null && npc.controller != null)
			{
				npc.controller.location = this.location;
			}
			Game1.warpCharacter(npc, this.location, new Vector2(w.TargetX, w.TargetY));
		}
		else
		{
			this.location = Game1.RequireLocation(targetLocationName);
			Game1.warpCharacter(this.character as NPC, w.TargetName, new Vector2(w.TargetX, w.TargetY));
		}
		if (this.isPlayerPresent() && this.location.doors.ContainsKey(new Point(w.X, w.Y)))
		{
			this.location.playSound("doorClose", new Vector2(w.X, w.Y), null, SoundContext.NPC);
		}
		if (this.isPlayerPresent() && this.location.doors.ContainsKey(new Point(w.TargetX, w.TargetY - 1)))
		{
			this.location.playSound("doorClose", new Vector2(w.TargetX, w.TargetY), null, SoundContext.NPC);
		}
		if (this.pathToEndPoint.Count > 0)
		{
			this.pathToEndPoint.Pop();
		}
		Point tile = this.character.TilePoint;
		while (this.pathToEndPoint.Count > 0 && (Math.Abs(this.pathToEndPoint.Peek().X - tile.X) > 1 || Math.Abs(this.pathToEndPoint.Peek().Y - tile.Y) > 1))
		{
			this.pathToEndPoint.Pop();
		}
	}

	[Obsolete("Use findPathForNPCSchedules overload with 'npc' parameter.")]
	public static Stack<Point> findPathForNPCSchedules(Point startPoint, Point endPoint, GameLocation location, int limit)
	{
		return PathFindController.findPathForNPCSchedules(startPoint, endPoint, location, limit, null);
	}

	public static Stack<Point> findPathForNPCSchedules(Point startPoint, Point endPoint, GameLocation location, int limit, Character npc)
	{
		PriorityQueue openList = new PriorityQueue();
		HashSet<int> closedList = new HashSet<int>();
		int iterations = 0;
		openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
		PathNode previousNode = (PathNode)openList.Peek();
		int layerWidth = location.map.Layers[0].LayerWidth;
		int layerHeight = location.map.Layers[0].LayerHeight;
		while (!openList.IsEmpty())
		{
			PathNode currentNode = openList.Dequeue();
			if (currentNode.x == endPoint.X && currentNode.y == endPoint.Y)
			{
				return PathFindController.reconstructPath(currentNode);
			}
			closedList.Add(currentNode.id);
			for (int i = 0; i < 4; i++)
			{
				int neighbor_tile_x = currentNode.x + PathFindController.Directions[i, 0];
				int neighbor_tile_y = currentNode.y + PathFindController.Directions[i, 1];
				int nid = PathNode.ComputeHash(neighbor_tile_x, neighbor_tile_y);
				if (closedList.Contains(nid))
				{
					continue;
				}
				PathNode neighbor = new PathNode(neighbor_tile_x, neighbor_tile_y, currentNode);
				neighbor.g = (byte)(currentNode.g + 1);
				if ((neighbor.x == endPoint.X && neighbor.y == endPoint.Y) || (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < layerWidth && neighbor.y < layerHeight && !PathFindController.isPositionImpassableForNPCSchedule(location, neighbor.x, neighbor.y, npc)))
				{
					int f = neighbor.g + PathFindController.getPreferenceValueForTerrainType(location, neighbor.x, neighbor.y) + (Math.Abs(endPoint.X - neighbor.x) + Math.Abs(endPoint.Y - neighbor.y) + (((neighbor.x == currentNode.x && neighbor.x == previousNode.x) || (neighbor.y == currentNode.y && neighbor.y == previousNode.y)) ? (-2) : 0));
					if (!openList.Contains(neighbor, f))
					{
						openList.Enqueue(neighbor, f);
					}
				}
			}
			previousNode = currentNode;
			iterations++;
			if (iterations >= limit)
			{
				return null;
			}
		}
		return null;
	}

	protected static bool isPositionImpassableForNPCSchedule(GameLocation loc, int x, int y, Character npc)
	{
		Tile tile = loc.Map.RequireLayer("Buildings").Tiles[x, y];
		if (tile != null && tile.TileIndex != -1)
		{
			if (tile.TileIndexProperties.TryGetValue("Action", out var value) || tile.Properties.TryGetValue("Action", out value))
			{
				if (value.StartsWith("LockedDoorWarp"))
				{
					return true;
				}
				if (!value.Contains("Door") && !value.Contains("Passable"))
				{
					return true;
				}
			}
			else if (loc.doesTileHaveProperty(x, y, "Passable", "Buildings") == null && loc.doesTileHaveProperty(x, y, "NPCPassable", "Buildings") == null)
			{
				return true;
			}
		}
		if (loc.doesTileHaveProperty(x, y, "NoPath", "Back") != null)
		{
			return true;
		}
		foreach (Warp warp in loc.warps)
		{
			if (warp.X == x && warp.Y == y)
			{
				return true;
			}
		}
		if (((!(loc.terrainFeatures.GetValueOrDefault(new Vector2(x, y))?.isPassable(npc))) ?? false) || !(loc.getLargeTerrainFeatureAt(x, y)?.isPassable(npc) ?? true))
		{
			return true;
		}
		return false;
	}

	/// <summary>Get the precedence value for a tile position when choosing a path, where lower values are preferred.</summary>
	/// <param name="l">The location to check.</param>
	/// <param name="x">The X tile position.</param>
	/// <param name="y">The Y tile position.</param>
	protected static int getPreferenceValueForTerrainType(GameLocation l, int x, int y)
	{
		return l.doesTileHaveProperty(x, y, "Type", "Back")?.ToLower() switch
		{
			"stone" => -7, 
			"wood" => -4, 
			"dirt" => -2, 
			"grass" => -1, 
			_ => 0, 
		};
	}
}
