using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sickhead.Engine.Util;

public static class ObjToStr
{
	private struct ToStringDescription
	{
		public Type Type;

		public List<ToStringMember> Members;
	}

	private struct ToStringMember
	{
		public MemberInfo Member;

		private string _name;

		private string _format;

		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(this._name))
				{
					return this._name;
				}
				return this.Member.Name;
			}
			set
			{
				this._name = value;
			}
		}

		public string Format
		{
			get
			{
				if (!string.IsNullOrEmpty(this._format))
				{
					return this._format;
				}
				return "{0}";
			}
			set
			{
				this._format = value;
			}
		}
	}

	public class Style
	{
		public bool ShowRootObjectType;

		public string ObjectDelimiter;

		public string MemberDelimiter;

		public string MemberNameValueDelimiter;

		public bool TrailingNewline;

		public static Style TypeAndMembersSingleLine = new Style
		{
			ShowRootObjectType = true,
			ObjectDelimiter = ":",
			MemberDelimiter = ",",
			MemberNameValueDelimiter = "="
		};

		public static Style MembersOnlyMultiline = new Style
		{
			ShowRootObjectType = false,
			ObjectDelimiter = "",
			MemberDelimiter = "\n",
			MemberNameValueDelimiter = "="
		};

		public Style()
		{
			this.ShowRootObjectType = true;
			this.ObjectDelimiter = ":";
			this.MemberDelimiter = ",";
			this.MemberNameValueDelimiter = "=";
		}
	}

	private static readonly StringBuilder _stringBuilder = new StringBuilder();

	private static readonly Dictionary<Type, ToStringDescription> _cache = new Dictionary<Type, ToStringDescription>();

	public static string Format(object obj, Style style)
	{
		Type type = obj.GetType();
		ObjToStr._cache.Clear();
		if (!ObjToStr._cache.TryGetValue(obj.GetType(), out var desc))
		{
			desc = new ToStringDescription
			{
				Type = type,
				Members = new List<ToStringMember>()
			};
			ObjToStr._cache.Add(type, desc);
			BindingFlags attrs = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fields = type.GetFields(attrs);
			foreach (FieldInfo m in fields)
			{
				ToStringMember item = new ToStringMember
				{
					Member = m,
					Name = m.Name
				};
				Type dataType = m.GetDataType();
				if (dataType == typeof(string))
				{
					item.Format = "\"{0}\"";
				}
				if (dataType.HasElementType)
				{
					item.Format = "{1}[{2}] {0}";
				}
				desc.Members.Add(item);
			}
			desc.Members.Sort(CompareToStringMembers);
		}
		lock (ObjToStr._stringBuilder)
		{
			ObjToStr._stringBuilder.Clear();
			if (style.ShowRootObjectType)
			{
				ObjToStr._stringBuilder.Append(desc.Type.Name);
				ObjToStr._stringBuilder.Append(style.ObjectDelimiter);
			}
			for (int j = 0; j < desc.Members.Count; j++)
			{
				ToStringMember m2 = desc.Members[j];
				Type dataType2 = m2.Member.GetDataType();
				object val = m2.Member.GetValue(obj);
				ObjToStr._stringBuilder.Append(dataType2.Name);
				ObjToStr._stringBuilder.Append(" ");
				ObjToStr._stringBuilder.Append(m2.Name);
				ObjToStr._stringBuilder.Append(style.MemberNameValueDelimiter);
				if (val == null)
				{
					ObjToStr._stringBuilder.Append("null");
				}
				else
				{
					Type vtype = val.GetType();
					if (vtype.HasElementType)
					{
						Type etype = vtype.GetElementType();
						string ecount = "?";
						ObjToStr._stringBuilder.AppendFormat(m2.Format, val, etype, ecount);
					}
					else
					{
						ObjToStr._stringBuilder.AppendFormat(m2.Format, val);
					}
				}
				if (j != desc.Members.Count - 1)
				{
					ObjToStr._stringBuilder.Append(style.MemberDelimiter);
				}
			}
			return ObjToStr._stringBuilder.ToString();
		}
	}

	private static int CompareToStringMembers(ToStringMember a, ToStringMember b)
	{
		return a.Name.CompareTo(b.Name);
	}
}
