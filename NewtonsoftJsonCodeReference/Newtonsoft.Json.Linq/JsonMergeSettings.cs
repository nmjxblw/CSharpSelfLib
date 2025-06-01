using System;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Specifies the settings used when merging JSON.
/// </summary>
public class JsonMergeSettings
{
	private MergeArrayHandling _mergeArrayHandling;

	private MergeNullValueHandling _mergeNullValueHandling;

	private StringComparison _propertyNameComparison;

	/// <summary>
	/// Gets or sets the method used when merging JSON arrays.
	/// </summary>
	/// <value>The method used when merging JSON arrays.</value>
	public MergeArrayHandling MergeArrayHandling
	{
		get
		{
			return this._mergeArrayHandling;
		}
		set
		{
			if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._mergeArrayHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how null value properties are merged.
	/// </summary>
	/// <value>How null value properties are merged.</value>
	public MergeNullValueHandling MergeNullValueHandling
	{
		get
		{
			return this._mergeNullValueHandling;
		}
		set
		{
			if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._mergeNullValueHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets the comparison used to match property names while merging.
	/// The exact property name will be searched for first and if no matching property is found then
	/// the <see cref="T:System.StringComparison" /> will be used to match a property.
	/// </summary>
	/// <value>The comparison used to match property names while merging.</value>
	public StringComparison PropertyNameComparison
	{
		get
		{
			return this._propertyNameComparison;
		}
		set
		{
			if (value < StringComparison.CurrentCulture || value > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._propertyNameComparison = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JsonMergeSettings" /> class.
	/// </summary>
	public JsonMergeSettings()
	{
		this._propertyNameComparison = StringComparison.Ordinal;
	}
}
