using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework;

/// <summary>
/// A container for services for a <see cref="T:Microsoft.Xna.Framework.Game" />.
/// </summary>
public class GameServiceContainer : IServiceProvider
{
	private Dictionary<Type, object> services;

	/// <summary>
	/// Create an empty <see cref="T:Microsoft.Xna.Framework.GameServiceContainer" />.
	/// </summary>
	public GameServiceContainer()
	{
		this.services = new Dictionary<Type, object>();
	}

	/// <summary>
	/// Add a service provider to this container.
	/// </summary>
	/// <param name="type">The type of the service.</param>
	/// <param name="provider">The provider of the service.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// If <paramref name="type" /> or <paramref name="provider" /> is <code>null</code>.
	/// </exception>
	/// <exception cref="T:System.ArgumentException">
	/// If <paramref name="provider" /> cannot be assigned to <paramref name="type" />.
	/// </exception>
	public void AddService(Type type, object provider)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (!ReflectionHelpers.IsAssignableFrom(type, provider))
		{
			throw new ArgumentException("The provider does not match the specified service type!");
		}
		this.services.Add(type, provider);
	}

	/// <summary>
	/// Get a service provider for the service of the specified type.
	/// </summary>
	/// <param name="type">The type of the service.</param>
	/// <returns>
	/// A service provider for the service of the specified type or <code>null</code> if
	/// no suitable service provider is registered in this container.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">If the specified type is <code>null</code>.</exception>
	public object GetService(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (this.services.TryGetValue(type, out var service))
		{
			return service;
		}
		return null;
	}

	/// <summary>
	/// Remove the service with the specified type. Does nothing no service of the specified type is registered.
	/// </summary>
	/// <param name="type">The type of the service to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">If the specified type is <code>null</code>.</exception>
	public void RemoveService(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		this.services.Remove(type);
	}

	/// <summary>
	/// Add a service provider to this container.
	/// </summary>
	/// <typeparam name="T">The type of the service.</typeparam>
	/// <param name="provider">The provider of the service.</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// If <paramref name="provider" /> is <code>null</code>.
	/// </exception>
	public void AddService<T>(T provider)
	{
		this.AddService(typeof(T), provider);
	}

	/// <summary>
	/// Get a service provider of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of the service provider.</typeparam>
	/// <returns>
	/// A service provider of the specified type or <code>null</code> if
	/// no suitable service provider is registered in this container.
	/// </returns>
	public T GetService<T>() where T : class
	{
		object service = this.GetService(typeof(T));
		if (service == null)
		{
			return null;
		}
		return (T)service;
	}
}
