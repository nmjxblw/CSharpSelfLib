using System;
using System.Reflection;
using StardewModdingAPI;

namespace BirbCore.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class MethodHandler : Attribute
{
	public abstract void Handle(MethodInfo method, object? instance, IMod mod, object[]? args = null);
}
