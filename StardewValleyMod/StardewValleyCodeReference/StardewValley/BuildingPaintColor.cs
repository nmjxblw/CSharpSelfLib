using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley;

public class BuildingPaintColor : INetObject<NetFields>
{
	public NetString ColorName = new NetString();

	public NetBool Color1Default = new NetBool(value: true);

	public NetInt Color1Hue = new NetInt();

	public NetInt Color1Saturation = new NetInt();

	public NetInt Color1Lightness = new NetInt();

	public NetBool Color2Default = new NetBool(value: true);

	public NetInt Color2Hue = new NetInt();

	public NetInt Color2Saturation = new NetInt();

	public NetInt Color2Lightness = new NetInt();

	public NetBool Color3Default = new NetBool(value: true);

	public NetInt Color3Hue = new NetInt();

	public NetInt Color3Saturation = new NetInt();

	public NetInt Color3Lightness = new NetInt();

	protected bool _dirty;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("BuildingPaintColor");

	public BuildingPaintColor()
	{
		this.NetFields.SetOwner(this).AddField(this.ColorName, "ColorName").AddField(this.Color1Default, "Color1Default")
			.AddField(this.Color2Default, "Color2Default")
			.AddField(this.Color3Default, "Color3Default")
			.AddField(this.Color1Hue, "Color1Hue")
			.AddField(this.Color1Saturation, "Color1Saturation")
			.AddField(this.Color1Lightness, "Color1Lightness")
			.AddField(this.Color2Hue, "Color2Hue")
			.AddField(this.Color2Saturation, "Color2Saturation")
			.AddField(this.Color2Lightness, "Color2Lightness")
			.AddField(this.Color3Hue, "Color3Hue")
			.AddField(this.Color3Saturation, "Color3Saturation")
			.AddField(this.Color3Lightness, "Color3Lightness");
		this.Color1Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		this.Color2Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		this.Color3Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
		this.Color1Hue.fieldChangeVisibleEvent += OnColorChanged;
		this.Color1Saturation.fieldChangeVisibleEvent += OnColorChanged;
		this.Color1Lightness.fieldChangeVisibleEvent += OnColorChanged;
		this.Color2Hue.fieldChangeVisibleEvent += OnColorChanged;
		this.Color2Saturation.fieldChangeVisibleEvent += OnColorChanged;
		this.Color2Lightness.fieldChangeVisibleEvent += OnColorChanged;
		this.Color3Hue.fieldChangeVisibleEvent += OnColorChanged;
		this.Color3Saturation.fieldChangeVisibleEvent += OnColorChanged;
		this.Color3Lightness.fieldChangeVisibleEvent += OnColorChanged;
	}

	public virtual void CopyFrom(BuildingPaintColor other)
	{
		this.ColorName.Value = other.ColorName.Value;
		this.Color1Default.Value = other.Color1Default.Value;
		this.Color1Hue.Value = other.Color1Hue.Value;
		this.Color1Saturation.Value = other.Color1Saturation.Value;
		this.Color1Lightness.Value = other.Color1Lightness.Value;
		this.Color2Default.Value = other.Color2Default.Value;
		this.Color2Hue.Value = other.Color2Hue.Value;
		this.Color2Saturation.Value = other.Color2Saturation.Value;
		this.Color2Lightness.Value = other.Color2Lightness.Value;
		this.Color3Default.Value = other.Color3Default.Value;
		this.Color3Hue.Value = other.Color3Hue.Value;
		this.Color3Saturation.Value = other.Color3Saturation.Value;
		this.Color3Lightness.Value = other.Color3Lightness.Value;
	}

	public virtual void OnDefaultFlagChanged(NetBool field, bool old_value, bool new_value)
	{
		this._dirty = true;
	}

	public virtual void OnColorChanged(NetInt field, int old_value, int new_value)
	{
		this._dirty = true;
	}

	public virtual void Poll(Action apply)
	{
		if (this._dirty)
		{
			apply?.Invoke();
			this._dirty = false;
		}
	}

	public bool IsDirty()
	{
		return this._dirty;
	}

	public bool RequiresRecolor()
	{
		if (this.Color1Default.Value && this.Color2Default.Value)
		{
			return !this.Color3Default.Value;
		}
		return true;
	}
}
