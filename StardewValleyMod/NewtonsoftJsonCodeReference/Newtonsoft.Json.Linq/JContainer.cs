using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a token that can contain other tokens.
/// </summary>
public abstract class JContainer : JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable, ITypedList, IBindingList, ICollection, IList, INotifyCollectionChanged
{
	internal ListChangedEventHandler? _listChanged;

	internal AddingNewEventHandler? _addingNew;

	internal NotifyCollectionChangedEventHandler? _collectionChanged;

	private object? _syncRoot;

	private bool _busy;

	/// <summary>
	/// Gets the container's children tokens.
	/// </summary>
	/// <value>The container's children tokens.</value>
	protected abstract IList<JToken> ChildrenTokens { get; }

	/// <summary>
	/// Gets a value indicating whether this token has child tokens.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
	/// </value>
	public override bool HasValues => this.ChildrenTokens.Count > 0;

	/// <summary>
	/// Get the first child token of this token.
	/// </summary>
	/// <value>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the first child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </value>
	public override JToken? First
	{
		get
		{
			IList<JToken> childrenTokens = this.ChildrenTokens;
			if (childrenTokens.Count <= 0)
			{
				return null;
			}
			return childrenTokens[0];
		}
	}

	/// <summary>
	/// Get the last child token of this token.
	/// </summary>
	/// <value>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the last child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </value>
	public override JToken? Last
	{
		get
		{
			IList<JToken> childrenTokens = this.ChildrenTokens;
			int count = childrenTokens.Count;
			if (count <= 0)
			{
				return null;
			}
			return childrenTokens[count - 1];
		}
	}

	JToken IList<JToken>.this[int index]
	{
		get
		{
			return this.GetItem(index);
		}
		set
		{
			this.SetItem(index, value);
		}
	}

	bool ICollection<JToken>.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => false;

	object? IList.this[int index]
	{
		get
		{
			return this.GetItem(index);
		}
		set
		{
			this.SetItem(index, this.EnsureValue(value));
		}
	}

	/// <summary>
	/// Gets the count of child JSON tokens.
	/// </summary>
	/// <value>The count of child JSON tokens.</value>
	public int Count => this.ChildrenTokens.Count;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (this._syncRoot == null)
			{
				Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
			}
			return this._syncRoot;
		}
	}

	bool IBindingList.AllowEdit => true;

	bool IBindingList.AllowNew => true;

	bool IBindingList.AllowRemove => true;

	bool IBindingList.IsSorted => false;

	ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;

	PropertyDescriptor? IBindingList.SortProperty => null;

	bool IBindingList.SupportsChangeNotification => true;

	bool IBindingList.SupportsSearching => false;

	bool IBindingList.SupportsSorting => false;

	/// <summary>
	/// Occurs when the list changes or an item in the list changes.
	/// </summary>
	public event ListChangedEventHandler ListChanged
	{
		add
		{
			this._listChanged = (ListChangedEventHandler)Delegate.Combine(this._listChanged, value);
		}
		remove
		{
			this._listChanged = (ListChangedEventHandler)Delegate.Remove(this._listChanged, value);
		}
	}

	/// <summary>
	/// Occurs before an item is added to the collection.
	/// </summary>
	public event AddingNewEventHandler AddingNew
	{
		add
		{
			this._addingNew = (AddingNewEventHandler)Delegate.Combine(this._addingNew, value);
		}
		remove
		{
			this._addingNew = (AddingNewEventHandler)Delegate.Remove(this._addingNew, value);
		}
	}

	/// <summary>
	/// Occurs when the items list of the collection has changed, or the collection is reset.
	/// </summary>
	public event NotifyCollectionChangedEventHandler? CollectionChanged
	{
		add
		{
			this._collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Combine(this._collectionChanged, value);
		}
		remove
		{
			this._collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Remove(this._collectionChanged, value);
		}
	}

	internal async Task ReadTokenFromAsync(JsonReader reader, JsonLoadSettings? options, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		int startDepth = reader.Depth;
		if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, base.GetType().Name));
		}
		await this.ReadContentFromAsync(reader, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.Depth > startDepth)
		{
			throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType().Name));
		}
	}

	private async Task ReadContentFromAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		IJsonLineInfo lineInfo = reader as IJsonLineInfo;
		JContainer parent = this;
		do
		{
			if (parent is JProperty { Value: not null })
			{
				if (parent == this)
				{
					break;
				}
				parent = parent.Parent;
			}
			switch (reader.TokenType)
			{
			case JsonToken.StartArray:
			{
				JArray jArray = new JArray();
				jArray.SetLineInfo(lineInfo, settings);
				parent.Add(jArray);
				parent = jArray;
				break;
			}
			case JsonToken.EndArray:
				if (parent == this)
				{
					return;
				}
				parent = parent.Parent;
				break;
			case JsonToken.StartObject:
			{
				JObject jObject = new JObject();
				jObject.SetLineInfo(lineInfo, settings);
				parent.Add(jObject);
				parent = jObject;
				break;
			}
			case JsonToken.EndObject:
				if (parent == this)
				{
					return;
				}
				parent = parent.Parent;
				break;
			case JsonToken.StartConstructor:
			{
				JConstructor jConstructor = new JConstructor(reader.Value.ToString());
				jConstructor.SetLineInfo(lineInfo, settings);
				parent.Add(jConstructor);
				parent = jConstructor;
				break;
			}
			case JsonToken.EndConstructor:
				if (parent == this)
				{
					return;
				}
				parent = parent.Parent;
				break;
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Date:
			case JsonToken.Bytes:
			{
				JValue jValue = new JValue(reader.Value);
				jValue.SetLineInfo(lineInfo, settings);
				parent.Add(jValue);
				break;
			}
			case JsonToken.Comment:
				if (settings != null && settings.CommentHandling == CommentHandling.Load)
				{
					JValue jValue = JValue.CreateComment(reader.Value.ToString());
					jValue.SetLineInfo(lineInfo, settings);
					parent.Add(jValue);
				}
				break;
			case JsonToken.Null:
			{
				JValue jValue = JValue.CreateNull();
				jValue.SetLineInfo(lineInfo, settings);
				parent.Add(jValue);
				break;
			}
			case JsonToken.Undefined:
			{
				JValue jValue = JValue.CreateUndefined();
				jValue.SetLineInfo(lineInfo, settings);
				parent.Add(jValue);
				break;
			}
			case JsonToken.PropertyName:
			{
				JProperty jProperty2 = JContainer.ReadProperty(reader, settings, lineInfo, parent);
				if (jProperty2 != null)
				{
					parent = jProperty2;
				}
				else
				{
					await reader.SkipAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			}
			default:
				throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			case JsonToken.None:
				break;
			}
		}
		while (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal JContainer()
	{
	}

	internal JContainer(JContainer other, JsonCloneSettings? settings)
		: this()
	{
		ValidationUtils.ArgumentNotNull(other, "other");
		bool flag = settings?.CopyAnnotations ?? true;
		if (flag)
		{
			base.CopyAnnotations(this, other);
		}
		int num = 0;
		foreach (JToken item in (IEnumerable<JToken>)other)
		{
			this.TryAddInternal(num, item, skipParentCheck: false, flag);
			num++;
		}
	}

	internal void CheckReentrancy()
	{
		if (this._busy)
		{
			throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
	}

	internal virtual IList<JToken> CreateChildrenCollection()
	{
		return new List<JToken>();
	}

	/// <summary>
	/// Raises the <see cref="E:Newtonsoft.Json.Linq.JContainer.AddingNew" /> event.
	/// </summary>
	/// <param name="e">The <see cref="T:System.ComponentModel.AddingNewEventArgs" /> instance containing the event data.</param>
	protected virtual void OnAddingNew(AddingNewEventArgs e)
	{
		this._addingNew?.Invoke(this, e);
	}

	/// <summary>
	/// Raises the <see cref="E:Newtonsoft.Json.Linq.JContainer.ListChanged" /> event.
	/// </summary>
	/// <param name="e">The <see cref="T:System.ComponentModel.ListChangedEventArgs" /> instance containing the event data.</param>
	protected virtual void OnListChanged(ListChangedEventArgs e)
	{
		ListChangedEventHandler listChanged = this._listChanged;
		if (listChanged != null)
		{
			this._busy = true;
			try
			{
				listChanged(this, e);
			}
			finally
			{
				this._busy = false;
			}
		}
	}

	/// <summary>
	/// Raises the <see cref="E:Newtonsoft.Json.Linq.JContainer.CollectionChanged" /> event.
	/// </summary>
	/// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		NotifyCollectionChangedEventHandler collectionChanged = this._collectionChanged;
		if (collectionChanged != null)
		{
			this._busy = true;
			try
			{
				collectionChanged(this, e);
			}
			finally
			{
				this._busy = false;
			}
		}
	}

	internal bool ContentsEqual(JContainer container)
	{
		if (container == this)
		{
			return true;
		}
		IList<JToken> childrenTokens = this.ChildrenTokens;
		IList<JToken> childrenTokens2 = container.ChildrenTokens;
		if (childrenTokens.Count != childrenTokens2.Count)
		{
			return false;
		}
		for (int i = 0; i < childrenTokens.Count; i++)
		{
			if (!childrenTokens[i].DeepEquals(childrenTokens2[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Returns a collection of the child tokens of this token, in document order.
	/// </summary>
	/// <returns>
	/// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.
	/// </returns>
	public override JEnumerable<JToken> Children()
	{
		return new JEnumerable<JToken>(this.ChildrenTokens);
	}

	/// <summary>
	/// Returns a collection of the child values of this token, in document order.
	/// </summary>
	/// <typeparam name="T">The type to convert the values to.</typeparam>
	/// <returns>
	/// A <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing the child values of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.
	/// </returns>
	public override IEnumerable<T?> Values<T>()
	{
		return this.ChildrenTokens.Convert<JToken, T>();
	}

	/// <summary>
	/// Returns a collection of the descendant tokens for this token in document order.
	/// </summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the descendant tokens of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public IEnumerable<JToken> Descendants()
	{
		return this.GetDescendants(self: false);
	}

	/// <summary>
	/// Returns a collection of the tokens that contain this token, and all descendant tokens of this token, in document order.
	/// </summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing this token, and all the descendant tokens of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public IEnumerable<JToken> DescendantsAndSelf()
	{
		return this.GetDescendants(self: true);
	}

	internal IEnumerable<JToken> GetDescendants(bool self)
	{
		if (self)
		{
			yield return this;
		}
		foreach (JToken o in this.ChildrenTokens)
		{
			yield return o;
			if (!(o is JContainer jContainer))
			{
				continue;
			}
			foreach (JToken item in jContainer.Descendants())
			{
				yield return item;
			}
		}
	}

	internal bool IsMultiContent([NotNullWhen(true)] object? content)
	{
		if (content is IEnumerable && !(content is string) && !(content is JToken))
		{
			return !(content is byte[]);
		}
		return false;
	}

	internal JToken EnsureParentToken(JToken? item, bool skipParentCheck, bool copyAnnotations)
	{
		if (item == null)
		{
			return JValue.CreateNull();
		}
		if (skipParentCheck)
		{
			return item;
		}
		if (item.Parent != null || item == this || (item.HasValues && base.Root == item))
		{
			JsonCloneSettings settings = (copyAnnotations ? null : JsonCloneSettings.SkipCopyAnnotations);
			item = item.CloneToken(settings);
		}
		return item;
	}

	internal abstract int IndexOfItem(JToken? item);

	internal virtual bool InsertItem(int index, JToken? item, bool skipParentCheck, bool copyAnnotations)
	{
		IList<JToken> childrenTokens = this.ChildrenTokens;
		if (index > childrenTokens.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index must be within the bounds of the List.");
		}
		this.CheckReentrancy();
		item = this.EnsureParentToken(item, skipParentCheck, copyAnnotations);
		JToken jToken = ((index == 0) ? null : childrenTokens[index - 1]);
		JToken jToken2 = ((index == childrenTokens.Count) ? null : childrenTokens[index]);
		this.ValidateToken(item, null);
		item.Parent = this;
		item.Previous = jToken;
		if (jToken != null)
		{
			jToken.Next = item;
		}
		item.Next = jToken2;
		if (jToken2 != null)
		{
			jToken2.Previous = item;
		}
		childrenTokens.Insert(index, item);
		if (this._listChanged != null)
		{
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}
		if (this._collectionChanged != null)
		{
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}
		return true;
	}

	internal virtual void RemoveItemAt(int index)
	{
		IList<JToken> childrenTokens = this.ChildrenTokens;
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Index is less than 0.");
		}
		if (index >= childrenTokens.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index is equal to or greater than Count.");
		}
		this.CheckReentrancy();
		JToken jToken = childrenTokens[index];
		JToken jToken2 = ((index == 0) ? null : childrenTokens[index - 1]);
		JToken jToken3 = ((index == childrenTokens.Count - 1) ? null : childrenTokens[index + 1]);
		if (jToken2 != null)
		{
			jToken2.Next = jToken3;
		}
		if (jToken3 != null)
		{
			jToken3.Previous = jToken2;
		}
		jToken.Parent = null;
		jToken.Previous = null;
		jToken.Next = null;
		childrenTokens.RemoveAt(index);
		if (this._listChanged != null)
		{
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		}
		if (this._collectionChanged != null)
		{
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, jToken, index));
		}
	}

	internal virtual bool RemoveItem(JToken? item)
	{
		if (item != null)
		{
			int num = this.IndexOfItem(item);
			if (num >= 0)
			{
				this.RemoveItemAt(num);
				return true;
			}
		}
		return false;
	}

	internal virtual JToken GetItem(int index)
	{
		return this.ChildrenTokens[index];
	}

	internal virtual void SetItem(int index, JToken? item)
	{
		IList<JToken> childrenTokens = this.ChildrenTokens;
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Index is less than 0.");
		}
		if (index >= childrenTokens.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Index is equal to or greater than Count.");
		}
		JToken jToken = childrenTokens[index];
		if (!JContainer.IsTokenUnchanged(jToken, item))
		{
			this.CheckReentrancy();
			item = this.EnsureParentToken(item, skipParentCheck: false, copyAnnotations: true);
			this.ValidateToken(item, jToken);
			JToken jToken2 = ((index == 0) ? null : childrenTokens[index - 1]);
			JToken jToken3 = ((index == childrenTokens.Count - 1) ? null : childrenTokens[index + 1]);
			item.Parent = this;
			item.Previous = jToken2;
			if (jToken2 != null)
			{
				jToken2.Next = item;
			}
			item.Next = jToken3;
			if (jToken3 != null)
			{
				jToken3.Previous = item;
			}
			childrenTokens[index] = item;
			jToken.Parent = null;
			jToken.Previous = null;
			jToken.Next = null;
			if (this._listChanged != null)
			{
				this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
			}
			if (this._collectionChanged != null)
			{
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, jToken, index));
			}
		}
	}

	internal virtual void ClearItems()
	{
		this.CheckReentrancy();
		IList<JToken> childrenTokens = this.ChildrenTokens;
		foreach (JToken item in childrenTokens)
		{
			item.Parent = null;
			item.Previous = null;
			item.Next = null;
		}
		childrenTokens.Clear();
		if (this._listChanged != null)
		{
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
		if (this._collectionChanged != null)
		{
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	internal virtual void ReplaceItem(JToken existing, JToken replacement)
	{
		if (existing != null && existing.Parent == this)
		{
			int index = this.IndexOfItem(existing);
			this.SetItem(index, replacement);
		}
	}

	internal virtual bool ContainsItem(JToken? item)
	{
		return this.IndexOfItem(item) != -1;
	}

	internal virtual void CopyItemsTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
		}
		if (arrayIndex >= array.Length && arrayIndex != 0)
		{
			throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
		}
		if (this.Count > array.Length - arrayIndex)
		{
			throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
		}
		int num = 0;
		foreach (JToken childrenToken in this.ChildrenTokens)
		{
			array.SetValue(childrenToken, arrayIndex + num);
			num++;
		}
	}

	internal static bool IsTokenUnchanged(JToken currentValue, JToken? newValue)
	{
		if (currentValue is JValue jValue)
		{
			if (newValue == null)
			{
				return jValue.Type == JTokenType.Null;
			}
			return jValue.Equals(newValue);
		}
		return false;
	}

	internal virtual void ValidateToken(JToken o, JToken? existing)
	{
		ValidationUtils.ArgumentNotNull(o, "o");
		if (o.Type == JTokenType.Property)
		{
			throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), base.GetType()));
		}
	}

	/// <summary>
	/// Adds the specified content as children of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="content">The content to be added.</param>
	public virtual void Add(object? content)
	{
		this.TryAddInternal(this.ChildrenTokens.Count, content, skipParentCheck: false, copyAnnotations: true);
	}

	internal bool TryAdd(object? content)
	{
		return this.TryAddInternal(this.ChildrenTokens.Count, content, skipParentCheck: false, copyAnnotations: true);
	}

	internal void AddAndSkipParentCheck(JToken token)
	{
		this.TryAddInternal(this.ChildrenTokens.Count, token, skipParentCheck: true, copyAnnotations: true);
	}

	/// <summary>
	/// Adds the specified content as the first children of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="content">The content to be added.</param>
	public void AddFirst(object? content)
	{
		this.TryAddInternal(0, content, skipParentCheck: false, copyAnnotations: true);
	}

	internal bool TryAddInternal(int index, object? content, bool skipParentCheck, bool copyAnnotations)
	{
		if (this.IsMultiContent(content))
		{
			IEnumerable obj = (IEnumerable)content;
			int num = index;
			foreach (object item2 in obj)
			{
				this.TryAddInternal(num, item2, skipParentCheck, copyAnnotations);
				num++;
			}
			return true;
		}
		JToken item = JContainer.CreateFromContent(content);
		return this.InsertItem(index, item, skipParentCheck, copyAnnotations);
	}

	internal static JToken CreateFromContent(object? content)
	{
		if (content is JToken result)
		{
			return result;
		}
		return new JValue(content);
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.JsonWriter" /> that can be used to add tokens to the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <returns>A <see cref="T:Newtonsoft.Json.JsonWriter" /> that is ready to have content written to it.</returns>
	public JsonWriter CreateWriter()
	{
		return new JTokenWriter(this);
	}

	/// <summary>
	/// Replaces the child nodes of this token with the specified content.
	/// </summary>
	/// <param name="content">The content.</param>
	public void ReplaceAll(object content)
	{
		this.ClearItems();
		this.Add(content);
	}

	/// <summary>
	/// Removes the child nodes from this token.
	/// </summary>
	public void RemoveAll()
	{
		this.ClearItems();
	}

	internal abstract void MergeItem(object content, JsonMergeSettings? settings);

	/// <summary>
	/// Merge the specified content into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="content">The content to be merged.</param>
	public void Merge(object? content)
	{
		if (content != null)
		{
			this.ValidateContent(content);
			this.MergeItem(content, null);
		}
	}

	/// <summary>
	/// Merge the specified content into this <see cref="T:Newtonsoft.Json.Linq.JToken" /> using <see cref="T:Newtonsoft.Json.Linq.JsonMergeSettings" />.
	/// </summary>
	/// <param name="content">The content to be merged.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonMergeSettings" /> used to merge the content.</param>
	public void Merge(object? content, JsonMergeSettings? settings)
	{
		if (content != null)
		{
			this.ValidateContent(content);
			this.MergeItem(content, settings);
		}
	}

	private void ValidateContent(object content)
	{
		if (content.GetType().IsSubclassOf(typeof(JToken)) || this.IsMultiContent(content))
		{
			return;
		}
		throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, content.GetType()), "content");
	}

	internal void ReadTokenFrom(JsonReader reader, JsonLoadSettings? options)
	{
		int depth = reader.Depth;
		if (!reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, base.GetType().Name));
		}
		this.ReadContentFrom(reader, options);
		if (reader.Depth > depth)
		{
			throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType().Name));
		}
	}

	internal void ReadContentFrom(JsonReader r, JsonLoadSettings? settings)
	{
		ValidationUtils.ArgumentNotNull(r, "r");
		IJsonLineInfo lineInfo = r as IJsonLineInfo;
		JContainer jContainer = this;
		do
		{
			if (jContainer is JProperty { Value: not null })
			{
				if (jContainer == this)
				{
					break;
				}
				jContainer = jContainer.Parent;
			}
			switch (r.TokenType)
			{
			case JsonToken.StartArray:
			{
				JArray jArray = new JArray();
				jArray.SetLineInfo(lineInfo, settings);
				jContainer.Add(jArray);
				jContainer = jArray;
				break;
			}
			case JsonToken.EndArray:
				if (jContainer == this)
				{
					return;
				}
				jContainer = jContainer.Parent;
				break;
			case JsonToken.StartObject:
			{
				JObject jObject = new JObject();
				jObject.SetLineInfo(lineInfo, settings);
				jContainer.Add(jObject);
				jContainer = jObject;
				break;
			}
			case JsonToken.EndObject:
				if (jContainer == this)
				{
					return;
				}
				jContainer = jContainer.Parent;
				break;
			case JsonToken.StartConstructor:
			{
				JConstructor jConstructor = new JConstructor(r.Value.ToString());
				jConstructor.SetLineInfo(lineInfo, settings);
				jContainer.Add(jConstructor);
				jContainer = jConstructor;
				break;
			}
			case JsonToken.EndConstructor:
				if (jContainer == this)
				{
					return;
				}
				jContainer = jContainer.Parent;
				break;
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Date:
			case JsonToken.Bytes:
			{
				JValue jValue = new JValue(r.Value);
				jValue.SetLineInfo(lineInfo, settings);
				jContainer.Add(jValue);
				break;
			}
			case JsonToken.Comment:
				if (settings != null && settings.CommentHandling == CommentHandling.Load)
				{
					JValue jValue = JValue.CreateComment(r.Value.ToString());
					jValue.SetLineInfo(lineInfo, settings);
					jContainer.Add(jValue);
				}
				break;
			case JsonToken.Null:
			{
				JValue jValue = JValue.CreateNull();
				jValue.SetLineInfo(lineInfo, settings);
				jContainer.Add(jValue);
				break;
			}
			case JsonToken.Undefined:
			{
				JValue jValue = JValue.CreateUndefined();
				jValue.SetLineInfo(lineInfo, settings);
				jContainer.Add(jValue);
				break;
			}
			case JsonToken.PropertyName:
			{
				JProperty jProperty2 = JContainer.ReadProperty(r, settings, lineInfo, jContainer);
				if (jProperty2 != null)
				{
					jContainer = jProperty2;
				}
				else
				{
					r.Skip();
				}
				break;
			}
			default:
				throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
			case JsonToken.None:
				break;
			}
		}
		while (r.Read());
	}

	private static JProperty? ReadProperty(JsonReader r, JsonLoadSettings? settings, IJsonLineInfo? lineInfo, JContainer parent)
	{
		DuplicatePropertyNameHandling duplicatePropertyNameHandling = settings?.DuplicatePropertyNameHandling ?? DuplicatePropertyNameHandling.Replace;
		JObject obj = (JObject)parent;
		string text = r.Value.ToString();
		JProperty jProperty = obj.Property(text, StringComparison.Ordinal);
		if (jProperty != null)
		{
			switch (duplicatePropertyNameHandling)
			{
			case DuplicatePropertyNameHandling.Ignore:
				return null;
			case DuplicatePropertyNameHandling.Error:
				throw JsonReaderException.Create(r, "Property with the name '{0}' already exists in the current JSON object.".FormatWith(CultureInfo.InvariantCulture, text));
			}
		}
		JProperty jProperty2 = new JProperty(text);
		jProperty2.SetLineInfo(lineInfo, settings);
		if (jProperty == null)
		{
			parent.Add(jProperty2);
		}
		else
		{
			jProperty.Replace(jProperty2);
		}
		return jProperty2;
	}

	internal int ContentsHashCode()
	{
		int num = 0;
		foreach (JToken childrenToken in this.ChildrenTokens)
		{
			num ^= childrenToken.GetDeepHashCode();
		}
		return num;
	}

	string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
	{
		return string.Empty;
	}

	PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
	{
		return (this.First as ICustomTypeDescriptor)?.GetProperties() ?? new PropertyDescriptorCollection(CollectionUtils.ArrayEmpty<PropertyDescriptor>());
	}

	int IList<JToken>.IndexOf(JToken item)
	{
		return this.IndexOfItem(item);
	}

	void IList<JToken>.Insert(int index, JToken item)
	{
		this.InsertItem(index, item, skipParentCheck: false, copyAnnotations: true);
	}

	void IList<JToken>.RemoveAt(int index)
	{
		this.RemoveItemAt(index);
	}

	void ICollection<JToken>.Add(JToken item)
	{
		this.Add(item);
	}

	void ICollection<JToken>.Clear()
	{
		this.ClearItems();
	}

	bool ICollection<JToken>.Contains(JToken item)
	{
		return this.ContainsItem(item);
	}

	void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
	{
		this.CopyItemsTo(array, arrayIndex);
	}

	bool ICollection<JToken>.Remove(JToken item)
	{
		return this.RemoveItem(item);
	}

	private JToken? EnsureValue(object? value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is JToken result)
		{
			return result;
		}
		throw new ArgumentException("Argument is not a JToken.");
	}

	int IList.Add(object? value)
	{
		this.Add(this.EnsureValue(value));
		return this.Count - 1;
	}

	void IList.Clear()
	{
		this.ClearItems();
	}

	bool IList.Contains(object? value)
	{
		return this.ContainsItem(this.EnsureValue(value));
	}

	int IList.IndexOf(object? value)
	{
		return this.IndexOfItem(this.EnsureValue(value));
	}

	void IList.Insert(int index, object? value)
	{
		this.InsertItem(index, this.EnsureValue(value), skipParentCheck: false, copyAnnotations: false);
	}

	void IList.Remove(object? value)
	{
		this.RemoveItem(this.EnsureValue(value));
	}

	void IList.RemoveAt(int index)
	{
		this.RemoveItemAt(index);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		this.CopyItemsTo(array, index);
	}

	void IBindingList.AddIndex(PropertyDescriptor property)
	{
	}

	object IBindingList.AddNew()
	{
		AddingNewEventArgs e = new AddingNewEventArgs();
		this.OnAddingNew(e);
		if (e.NewObject == null)
		{
			throw new JsonException("Could not determine new value to add to '{0}'.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
		if (!(e.NewObject is JToken jToken))
		{
			throw new JsonException("New item to be added to collection must be compatible with {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JToken)));
		}
		this.Add(jToken);
		return jToken;
	}

	void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
	{
		throw new NotSupportedException();
	}

	int IBindingList.Find(PropertyDescriptor property, object key)
	{
		throw new NotSupportedException();
	}

	void IBindingList.RemoveIndex(PropertyDescriptor property)
	{
	}

	void IBindingList.RemoveSort()
	{
		throw new NotSupportedException();
	}

	internal static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings? settings)
	{
		switch (settings?.MergeArrayHandling ?? MergeArrayHandling.Concat)
		{
		case MergeArrayHandling.Concat:
		{
			foreach (object item in content)
			{
				target.Add(JContainer.CreateFromContent(item));
			}
			break;
		}
		case MergeArrayHandling.Union:
		{
			HashSet<JToken> hashSet = new HashSet<JToken>(target, JToken.EqualityComparer);
			{
				foreach (object item2 in content)
				{
					JToken jToken2 = JContainer.CreateFromContent(item2);
					if (hashSet.Add(jToken2))
					{
						target.Add(jToken2);
					}
				}
				break;
			}
		}
		case MergeArrayHandling.Replace:
			if (target == content)
			{
				break;
			}
			target.ClearItems();
			{
				foreach (object item3 in content)
				{
					target.Add(JContainer.CreateFromContent(item3));
				}
				break;
			}
		case MergeArrayHandling.Merge:
		{
			int num = 0;
			{
				foreach (object item4 in content)
				{
					if (num < target.Count)
					{
						if (target[num] is JContainer jContainer)
						{
							jContainer.Merge(item4, settings);
						}
						else if (item4 != null)
						{
							JToken jToken = JContainer.CreateFromContent(item4);
							if (jToken.Type != JTokenType.Null)
							{
								target[num] = jToken;
							}
						}
					}
					else
					{
						target.Add(JContainer.CreateFromContent(item4));
					}
					num++;
				}
				break;
			}
		}
		default:
			throw new ArgumentOutOfRangeException("settings", "Unexpected merge array handling when merging JSON.");
		}
	}
}
