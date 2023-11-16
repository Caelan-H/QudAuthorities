using System.Collections.Generic;

namespace XRL.World;

public class TypeFieldComparer : IEqualityComparer<TypeField>
{
	public bool Equals(TypeField o1, TypeField o2)
	{
		return o1 == o2;
	}

	public int GetHashCode(TypeField o)
	{
		return o.GetHashCode();
	}
}
