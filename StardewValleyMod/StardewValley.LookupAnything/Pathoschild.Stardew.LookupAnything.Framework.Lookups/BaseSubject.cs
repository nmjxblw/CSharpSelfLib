using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal abstract class BaseSubject : ISubject
{
	protected GameHelper GameHelper { get; }

	protected Metadata Metadata => this.GameHelper.Metadata;

	protected ConstantData Constants => this.Metadata.Constants;

	public string Name { get; protected set; }

	public string? Description { get; protected set; }

	public string? Type { get; protected set; }

	public abstract IEnumerable<ICustomField> GetData();

	public abstract IEnumerable<IDebugField> GetDebugFields();

	public abstract bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size);

	protected BaseSubject(GameHelper gameHelper)
	{
		this.GameHelper = gameHelper;
		this.Name = string.Empty;
	}

	protected BaseSubject(GameHelper gameHelper, string name, string? description, string? type)
		: this(gameHelper)
	{
		this.Initialize(name, description, type);
	}

	[MemberNotNull("Name")]
	protected void Initialize(string name, string? description, string? type)
	{
		this.Name = name;
		this.Description = description;
		this.Type = type;
	}

	protected IEnumerable<IDebugField> GetDebugFieldsFrom(object? obj)
	{
		if (obj == null)
		{
			yield break;
		}
		Dictionary<string, string?> seenValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Type type = obj.GetType();
		while (type != null)
		{
			var fields = (from fieldInfo in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where !fieldInfo.IsLiteral && !fieldInfo.Name.EndsWith(">k__BackingField")
				select new
				{
					Name = fieldInfo.Name,
					Type = fieldInfo.FieldType,
					Value = this.GetDebugValue(obj, fieldInfo),
					IsProperty = false
				}).Concat(from property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where property.CanRead
				select new
				{
					Name = property.Name,
					Type = property.PropertyType,
					Value = this.GetDebugValue(obj, property),
					IsProperty = true
				}).OrderBy(anon => anon.Name, StringComparer.OrdinalIgnoreCase).ThenByDescending(anon => anon.IsProperty);
			foreach (var field in fields)
			{
				if ((!seenValues.TryGetValue(field.Name, out string value) || !(value == field.Value)) && (!(field.Name == "modDataForSerialization") || !seenValues.ContainsKey("modData")) && !(field.Value == field.Type.ToString()))
				{
					seenValues[field.Name] = field.Value;
					yield return new GenericDebugField(type.Name + "::" + field.Name, field.Value);
				}
			}
			type = type.BaseType;
		}
	}

	protected string Stringify(object? value)
	{
		return I18n.Stringify(value);
	}

	protected string GetRelativeDateStr(SDate date)
	{
		return this.GetRelativeDateStr(date.DaysSinceStart - SDate.Now().DaysSinceStart);
	}

	protected string GetRelativeDateStr(int days)
	{
		switch (days)
		{
		case -1:
			return I18n.Generic_Yesterday();
		case 0:
			return I18n.Generic_Now();
		case 1:
			return I18n.Generic_Tomorrow();
		default:
			if (days <= 0)
			{
				return I18n.Generic_XDaysAgo(-days);
			}
			return I18n.Generic_InXDays(days);
		}
	}

	private string GetDebugValue(object obj, FieldInfo field)
	{
		try
		{
			return this.Stringify(field.GetValue(obj));
		}
		catch (Exception ex)
		{
			return "error reading field: " + ex.Message;
		}
	}

	private string GetDebugValue(object obj, PropertyInfo property)
	{
		try
		{
			return this.Stringify(property.GetValue(obj));
		}
		catch (Exception ex)
		{
			return "error reading property: " + ex.Message;
		}
	}
}
