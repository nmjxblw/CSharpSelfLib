using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class PerchingBirds
{
	public const int BIRD_STARTLE_DISTANCE = 200;

	[XmlIgnore]
	public List<Bird> _birds = new List<Bird>();

	[XmlIgnore]
	protected Point[] _birdLocations;

	protected Point[] _birdRoostLocations;

	[XmlIgnore]
	public Dictionary<Point, Bird> _birdPointOccupancy;

	public bool roosting;

	protected Texture2D _birdSheet;

	protected int _birdWidth;

	protected int _birdHeight;

	protected int _flapFrames = 2;

	protected Vector2 _birdOrigin;

	public int peckDuration = 5;

	public float birdSpeed = 5f;

	public PerchingBirds(Texture2D bird_texture, int flap_frames, int width, int height, Vector2 origin, Point[] perch_locations, Point[] roost_locations)
	{
		this._birdSheet = bird_texture;
		this._birdWidth = width;
		this._birdHeight = height;
		this._birdOrigin = origin;
		this._flapFrames = flap_frames;
		this._birdPointOccupancy = new Dictionary<Point, Bird>();
		this._birdLocations = perch_locations;
		this._birdRoostLocations = roost_locations;
		this.ResetLocalState();
	}

	public int GetBirdWidth()
	{
		return this._birdWidth;
	}

	public int GetBirdHeight()
	{
		return this._birdHeight;
	}

	public Vector2 GetBirdOrigin()
	{
		return this._birdOrigin;
	}

	public Texture2D GetTexture()
	{
		return this._birdSheet;
	}

	public Point GetFreeBirdPoint(Bird bird = null, int clearance = 200)
	{
		List<Point> points = new List<Point>();
		Point[] currentBirdLocationList = this.GetCurrentBirdLocationList();
		for (int i = 0; i < currentBirdLocationList.Length; i++)
		{
			Point point = currentBirdLocationList[i];
			if (this._birdPointOccupancy[point] != null)
			{
				continue;
			}
			bool fail = false;
			if (bird != null)
			{
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (Utility.distance(farmer.position.X, (float)(point.X * 64) + 32f, farmer.position.Y, (float)(point.Y * 64) + 32f) < 200f)
					{
						fail = true;
					}
				}
			}
			if (!fail)
			{
				points.Add(point);
			}
		}
		return Game1.random.ChooseFrom(points);
	}

	public void ReserveBirdPoint(Bird bird, Point point)
	{
		if (this._birdPointOccupancy.ContainsKey(bird.endPosition))
		{
			this._birdPointOccupancy[bird.endPosition] = null;
		}
		if (this._birdPointOccupancy.ContainsKey(point))
		{
			this._birdPointOccupancy[point] = bird;
		}
	}

	public bool ShouldBirdsRoost()
	{
		return this.roosting;
	}

	public Point[] GetCurrentBirdLocationList()
	{
		if (this.ShouldBirdsRoost())
		{
			return this._birdRoostLocations;
		}
		return this._birdLocations;
	}

	public virtual void Update(GameTime time)
	{
		for (int i = 0; i < this._birds.Count; i++)
		{
			this._birds[i].Update(time);
		}
	}

	public virtual void Draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		for (int i = 0; i < this._birds.Count; i++)
		{
			this._birds[i].Draw(b);
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
	}

	public virtual void ResetLocalState()
	{
		this._birds.Clear();
		this._birdPointOccupancy = new Dictionary<Point, Bird>();
		Point[] birdLocations = this._birdLocations;
		foreach (Point point in birdLocations)
		{
			this._birdPointOccupancy[point] = null;
		}
		birdLocations = this._birdRoostLocations;
		foreach (Point point2 in birdLocations)
		{
			this._birdPointOccupancy[point2] = null;
		}
	}

	public virtual void AddBird(int bird_type)
	{
		Bird bird = new Bird(this.GetFreeBirdPoint(), this, bird_type, this._flapFrames);
		this._birds.Add(bird);
		this.ReserveBirdPoint(bird, bird.endPosition);
	}
}
