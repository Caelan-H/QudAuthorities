using System;

namespace XRL.EditorFormats.Map;

public class MapFileObjectBlueprint
{
	public string Name;

	public string Owner;

	public string Part;

	public MapFileObjectBlueprint(string Name, string Owner = null, string Part = null)
	{
		this.Name = Name;
		this.Owner = Owner;
		this.Part = Part;
	}

	public MapFileObjectBlueprint(MapFileObjectBlueprint bp)
	{
		Name = bp.Name;
		Owner = bp.Owner;
		Part = bp.Part;
	}

	public static bool operator ==(MapFileObjectBlueprint a, MapFileObjectBlueprint b)
	{
		return object.Equals(a, b);
	}

	public static bool operator !=(MapFileObjectBlueprint a, MapFileObjectBlueprint b)
	{
		return !object.Equals(a, b);
	}

	public override int GetHashCode()
	{
		return new Tuple<string, string, string>(Name, Owner ?? "", Part ?? "").GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is MapFileObjectBlueprint mapFileObjectBlueprint)
		{
			if (Name == mapFileObjectBlueprint.Name && (Owner == mapFileObjectBlueprint.Owner || (Owner.IsNullOrEmpty() && mapFileObjectBlueprint.Owner.IsNullOrEmpty())))
			{
				if (!(Part == mapFileObjectBlueprint.Part))
				{
					if (Part.IsNullOrEmpty())
					{
						return mapFileObjectBlueprint.Part.IsNullOrEmpty();
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
