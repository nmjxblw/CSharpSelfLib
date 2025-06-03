using System.Collections;
using System.Collections.Generic;

namespace StardewValley;

public class TemporaryAnimatedSpriteList : IList<TemporaryAnimatedSprite>, ICollection<TemporaryAnimatedSprite>, IEnumerable<TemporaryAnimatedSprite>, IEnumerable
{
	public List<TemporaryAnimatedSprite> AnimatedSprites = new List<TemporaryAnimatedSprite>();

	public TemporaryAnimatedSprite this[int index]
	{
		get
		{
			return this.AnimatedSprites[index];
		}
		set
		{
			this.AnimatedSprites[index] = value;
		}
	}

	public int Count => this.AnimatedSprites.Count;

	public bool IsReadOnly => false;

	public void AddRange(IEnumerable<TemporaryAnimatedSprite> values)
	{
		this.AnimatedSprites.AddRange(values);
	}

	public void Add(TemporaryAnimatedSprite item)
	{
		this.AnimatedSprites.Add(item);
	}

	public void Clear()
	{
		foreach (TemporaryAnimatedSprite sprite in this.AnimatedSprites)
		{
			if (sprite.Pooled)
			{
				sprite.Pool();
			}
		}
		this.AnimatedSprites.Clear();
	}

	public bool Contains(TemporaryAnimatedSprite item)
	{
		return this.AnimatedSprites.Contains(item);
	}

	public void CopyTo(TemporaryAnimatedSprite[] array, int index)
	{
		this.AnimatedSprites.CopyTo(array, index);
	}

	public IEnumerator<TemporaryAnimatedSprite> GetEnumerator()
	{
		return this.AnimatedSprites.GetEnumerator();
	}

	public int IndexOf(TemporaryAnimatedSprite item)
	{
		return this.AnimatedSprites.IndexOf(item);
	}

	public void Insert(int index, TemporaryAnimatedSprite item)
	{
		this.AnimatedSprites.Insert(index, item);
	}

	public bool Remove(TemporaryAnimatedSprite item)
	{
		if (this.AnimatedSprites.Remove(item))
		{
			if (item.Pooled)
			{
				item.Pool();
			}
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		TemporaryAnimatedSprite item = this.AnimatedSprites[index];
		this.AnimatedSprites.RemoveAt(index);
		if (item.Pooled)
		{
			item.Pool();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
}
