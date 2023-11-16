using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ItemElements : IActivePart
{
	public string _Elements;

	[NonSerialized]
	private Dictionary<string, int> _ElementMap;

	public string Elements
	{
		get
		{
			return _Elements;
		}
		set
		{
			_ElementMap = null;
			_Elements = value;
		}
	}

	public Dictionary<string, int> ElementMap
	{
		get
		{
			if (_ElementMap == null && !string.IsNullOrEmpty(_Elements))
			{
				_ElementMap = _Elements.CachedNumericDictionaryExpansion();
			}
			return _ElementMap;
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (ElementMap != null)
		{
			foreach (KeyValuePair<string, int> item in ElementMap)
			{
				E.Add(item.Key, item.Value);
			}
		}
		return base.HandleEvent(E);
	}
}
