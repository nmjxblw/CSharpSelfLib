using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Tools;

public class Shears : Tool
{
	[XmlIgnore]
	private readonly NetEvent0 finishEvent = new NetEvent0();

	/// <summary>The farm animal the shears are being used on, if any.</summary>
	[XmlIgnore]
	public FarmAnimal animal;

	public Shears()
		: base("Shears", -1, 7, 7, stackable: false)
	{
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Shears();
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.finishEvent, "finishEvent");
		this.finishEvent.onEvent += doFinish;
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		x = (int)who.GetToolLocation().X;
		y = (int)who.GetToolLocation().Y;
		this.animal = Utility.GetBestHarvestableFarmAnimal(toolRect: new Rectangle(x - 32, y - 32, 64, 64), animals: location.animals.Values, tool: this);
		who.Halt();
		int g = who.FarmerSprite.CurrentFrame;
		who.FarmerSprite.animateOnce(283 + who.FacingDirection, 50f, 4);
		who.FarmerSprite.oldFrame = g;
		who.UsingTool = true;
		who.CanMove = false;
		return true;
	}

	public static void playSnip(Farmer who)
	{
		who.playNearbySoundAll("scissors");
	}

	public override void tickUpdate(GameTime time, Farmer who)
	{
		base.lastUser = who;
		base.tickUpdate(time, who);
		this.finishEvent.Poll();
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		who.Stamina -= 4f;
		if (base.PlayUseSounds)
		{
			Shears.playSnip(who);
		}
		base.CurrentParentTileIndex = 7;
		base.IndexOfMenuItemView = 7;
		if (this.animal?.currentProduce.Value != null && this.animal.isAdult() && this.animal.CanGetProduceWithTool(this))
		{
			Object produce = ItemRegistry.Create<Object>("(O)" + this.animal.currentProduce.Value);
			produce.CanBeSetDown = false;
			produce.Quality = this.animal.produceQuality.Value;
			if (this.animal.hasEatenAnimalCracker.Value)
			{
				produce.Stack = 2;
			}
			if (who.addItemToInventoryBool(produce))
			{
				this.animal.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
				this.animal.currentProduce.Value = null;
				if (base.PlayUseSounds)
				{
					Game1.playSound("coin");
				}
				this.animal.friendshipTowardFarmer.Value = Math.Min(1000, this.animal.friendshipTowardFarmer.Value + 5);
				this.animal.ReloadTextureIfNeeded();
				who.gainExperience(0, 5);
			}
		}
		else
		{
			string toSay = null;
			if (this.animal != null)
			{
				toSay = (this.animal.CanGetProduceWithTool(this) ? (this.animal.isBaby() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14246", this.animal.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14247", this.animal.displayName)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14245", this.animal.displayName));
			}
			if (toSay != null)
			{
				Game1.drawObjectDialogue(toSay);
			}
		}
		this.finish();
	}

	private void finish()
	{
		this.finishEvent.Fire();
	}

	private void doFinish()
	{
		this.animal = null;
		base.lastUser.CanMove = true;
		base.lastUser.completelyStopAnimatingOrDoingAction();
		base.lastUser.UsingTool = false;
		base.lastUser.canReleaseTool = true;
	}
}
