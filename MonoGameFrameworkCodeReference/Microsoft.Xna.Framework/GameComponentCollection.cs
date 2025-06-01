using System;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework;

/// <summary>
/// A collection of <see cref="T:Microsoft.Xna.Framework.IGameComponent" /> instances.
/// </summary>
public sealed class GameComponentCollection : Collection<IGameComponent>
{
	/// <summary>
	/// Event that is triggered when a <see cref="T:Microsoft.Xna.Framework.GameComponent" /> is added
	/// to this <see cref="T:Microsoft.Xna.Framework.GameComponentCollection" />.
	/// </summary>
	public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

	/// <summary>
	/// Event that is triggered when a <see cref="T:Microsoft.Xna.Framework.GameComponent" /> is removed
	/// from this <see cref="T:Microsoft.Xna.Framework.GameComponentCollection" />.
	/// </summary>
	public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

	/// <summary>
	/// Removes every <see cref="T:Microsoft.Xna.Framework.GameComponent" /> from this <see cref="T:Microsoft.Xna.Framework.GameComponentCollection" />.
	/// Triggers <see cref="M:Microsoft.Xna.Framework.GameComponentCollection.OnComponentRemoved(Microsoft.Xna.Framework.GameComponentCollectionEventArgs)" /> once for each <see cref="T:Microsoft.Xna.Framework.GameComponent" /> removed.
	/// </summary>
	protected override void ClearItems()
	{
		for (int i = 0; i < base.Count; i++)
		{
			this.OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, IGameComponent item)
	{
		if (base.IndexOf(item) != -1)
		{
			throw new ArgumentException("Cannot Add Same Component Multiple Times");
		}
		base.InsertItem(index, item);
		if (item != null)
		{
			this.OnComponentAdded(new GameComponentCollectionEventArgs(item));
		}
	}

	private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
	{
		EventHelpers.Raise(this, this.ComponentAdded, eventArgs);
	}

	private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
	{
		EventHelpers.Raise(this, this.ComponentRemoved, eventArgs);
	}

	protected override void RemoveItem(int index)
	{
		IGameComponent gameComponent = base[index];
		base.RemoveItem(index);
		if (gameComponent != null)
		{
			this.OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
		}
	}

	protected override void SetItem(int index, IGameComponent item)
	{
		throw new NotSupportedException();
	}
}
