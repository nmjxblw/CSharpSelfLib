using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

internal class FarmersBox : IClickableMenu
{
	private readonly List<Farmer> _farmers = new List<Farmer>();

	public float _updateTimer;

	public FarmersBox()
		: base(0, 200, 528, 400)
	{
	}

	private void UpdateFarmers(List<ClickableComponent> parentComponents)
	{
		if (this._updateTimer > 0f)
		{
			return;
		}
		this._farmers.Clear();
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			this._farmers.Add(farmer);
		}
		this._updateTimer = 1f;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		this._updateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
	}

	public void draw(SpriteBatch b, int left, int bottom, ClickableComponent current, List<ClickableComponent> parentComponents)
	{
		this.UpdateFarmers(parentComponents);
		if (this._farmers.Count == 0)
		{
			return;
		}
		int sizeY = 100;
		base.height = sizeY * this._farmers.Count;
		base.xPositionOnScreen = left;
		base.yPositionOnScreen = bottom - base.height;
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f, drawShadow: false);
		b.End();
		b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
		int x = base.xPositionOnScreen + 16;
		int y = base.yPositionOnScreen;
		for (int i = 0; i < this._farmers.Count; i++)
		{
			Farmer farmer = this._farmers[i];
			Rectangle newClip = origClip;
			newClip.X = x;
			newClip.Y = y;
			newClip.Height = sizeY - 8;
			newClip.Width = 200;
			b.GraphicsDevice.ScissorRectangle = newClip;
			FarmerRenderer.isDrawingForUI = true;
			farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes.Value ? 108 : 0, 0, secondaryArm: false, flip: false), farmer.bathingClothes.Value ? 108 : 0, new Rectangle(0, farmer.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
			FarmerRenderer.isDrawingForUI = false;
			b.GraphicsDevice.ScissorRectangle = origClip;
			int textX = x + 80;
			int textY = y + 12;
			string farmerName = ChatBox.formattedUserName(farmer);
			b.DrawString(Game1.dialogueFont, farmerName, new Vector2(textX, textY), Color.White);
			string platformUserName = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID);
			if (!string.IsNullOrEmpty(platformUserName))
			{
				textY += Game1.dialogueFont.LineSpacing + 4;
				string userName = "(" + platformUserName + ")";
				b.DrawString(Game1.smallFont, userName, new Vector2(textX, textY), Color.White);
			}
			y += sizeY;
		}
		b.GraphicsDevice.ScissorRectangle = origClip;
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
	}
}
