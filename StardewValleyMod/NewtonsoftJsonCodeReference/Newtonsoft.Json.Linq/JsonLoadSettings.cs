using System;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Specifies the settings used when loading JSON.
/// </summary>
public class JsonLoadSettings
{
	private CommentHandling _commentHandling;

	private LineInfoHandling _lineInfoHandling;

	private DuplicatePropertyNameHandling _duplicatePropertyNameHandling;

	/// <summary>
	/// Gets or sets how JSON comments are handled when loading JSON.
	/// The default value is <see cref="F:Newtonsoft.Json.Linq.CommentHandling.Ignore" />.
	/// </summary>
	/// <value>The JSON comment handling.</value>
	public CommentHandling CommentHandling
	{
		get
		{
			return this._commentHandling;
		}
		set
		{
			if (value < CommentHandling.Ignore || value > CommentHandling.Load)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._commentHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how JSON line info is handled when loading JSON.
	/// The default value is <see cref="F:Newtonsoft.Json.Linq.LineInfoHandling.Load" />.
	/// </summary>
	/// <value>The JSON line info handling.</value>
	public LineInfoHandling LineInfoHandling
	{
		get
		{
			return this._lineInfoHandling;
		}
		set
		{
			if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._lineInfoHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how duplicate property names in JSON objects are handled when loading JSON.
	/// The default value is <see cref="F:Newtonsoft.Json.Linq.DuplicatePropertyNameHandling.Replace" />.
	/// </summary>
	/// <value>The JSON duplicate property name handling.</value>
	public DuplicatePropertyNameHandling DuplicatePropertyNameHandling
	{
		get
		{
			return this._duplicatePropertyNameHandling;
		}
		set
		{
			if (value < DuplicatePropertyNameHandling.Replace || value > DuplicatePropertyNameHandling.Error)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._duplicatePropertyNameHandling = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> class.
	/// </summary>
	public JsonLoadSettings()
	{
		this._lineInfoHandling = LineInfoHandling.Load;
		this._commentHandling = CommentHandling.Ignore;
		this._duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
	}
}
