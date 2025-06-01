using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Tools;

public class MilkPail : Tool
{
	[XmlIgnore]
	private readonly NetEvent0 finishEvent = new NetEvent0();

	/// <summary>The farm animal the milk pail is being used on, if any.</summary>
	[XmlIgnore]
	public FarmAnimal animal;

	public MilkPail()
		: base("Milk Pail", -1, 6, 6, stackable: false)
	{
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new MilkPail();
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
		if (this.animal?.currentProduce.Value != null && this.animal.isAdult() && this.animal.CanGetProduceWithTool(this) && who.couldInventoryAcceptThisItem(this.animal.currentProduce.Value, 1))
		{
			this.animal.pauseTimer = 1500;
			this.animal.doEmote(20);
			if (base.PlayUseSounds)
			{
				who.playNearbySoundLocal("Milking");
			}
		}
		else if (this.animal?.currentProduce.Value != null && this.animal.isAdult())
		{
			if (who == Game1.player)
			{
				if (!this.animal.CanGetProduceWithTool(this))
				{
					string harvestTool = this.animal.GetAnimalData()?.HarvestTool;
					if (harvestTool != null)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Tools:MilkPail_Name", harvestTool));
					}
				}
				else if (!who.couldInventoryAcceptThisItem(this.animal.currentProduce.Value, (!this.animal.hasEatenAnimalCracker.Value) ? 1 : 2))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				}
			}
		}
		else if (who == Game1.player)
		{
			if (base.PlayUseSounds)
			{
				DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
				DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
			}
			string toSay = null;
			if (this.animal != null)
			{
				toSay = (this.animal.CanGetProduceWithTool(this) ? (this.animal.isBaby() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", this.animal.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", this.animal.displayName)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", this.animal.displayName));
			}
			if (toSay != null)
			{
				DelayedAction.showDialogueAfterDelay(toSay, 1000);
			}
		}
		who.Halt();
		int g = who.FarmerSprite.CurrentFrame;
		who.FarmerSprite.animateOnce(287 + who.FacingDirection, 50f, 4);
		who.FarmerSprite.oldFrame = g;
		who.UsingTool = true;
		who.CanMove = false;
		return true;
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
		base.CurrentParentTileIndex = 6;
		base.IndexOfMenuItemView = 6;
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
				if (base.PlayUseSounds)
				{
					Game1.playSound("coin");
				}
				this.animal.currentProduce.Value = null;
				this.animal.friendshipTowardFarmer.Value = Math.Min(1000, this.animal.friendshipTowardFarmer.Value + 5);
				this.animal.ReloadTextureIfNeeded();
				who.gainExperience(0, 5);
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
