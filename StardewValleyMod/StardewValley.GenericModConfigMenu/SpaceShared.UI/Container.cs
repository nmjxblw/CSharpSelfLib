using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceShared.UI;

internal abstract class Container : Element
{
	private readonly IList<Element> ChildrenImpl = new List<Element>();

	private Element renderLast;

	protected bool UpdateChildren { get; set; } = true;

	public Element RenderLast
	{
		get
		{
			return this.renderLast;
		}
		set
		{
			this.renderLast = value;
			if (base.Parent == null)
			{
				return;
			}
			if (value == null)
			{
				if (base.Parent.RenderLast == this)
				{
					base.Parent.RenderLast = null;
				}
			}
			else
			{
				base.Parent.RenderLast = this;
			}
		}
	}

	public Element[] Children => this.ChildrenImpl.ToArray();

	public void AddChild(Element element)
	{
		element.Parent?.RemoveChild(element);
		this.ChildrenImpl.Add(element);
		element.Parent = this;
		this.OnChildrenChanged();
	}

	public void RemoveChild(Element element)
	{
		if (element.Parent != this)
		{
			throw new ArgumentException("Element must be a child of this container.");
		}
		this.ChildrenImpl.Remove(element);
		element.Parent = null;
		this.OnChildrenChanged();
	}

	public virtual void OnChildrenChanged()
	{
	}

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		if (!this.UpdateChildren)
		{
			return;
		}
		foreach (Element element in this.ChildrenImpl)
		{
			element.Update(isOffScreen);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.IsHidden())
		{
			return;
		}
		foreach (Element child in this.ChildrenImpl)
		{
			if (child != this.RenderLast)
			{
				child.Draw(b);
			}
		}
		this.RenderLast?.Draw(b);
	}
}
