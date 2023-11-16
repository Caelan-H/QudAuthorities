using System;
using System.Reflection;

namespace XRL.World;

/// <summary>
///             Base game object
///             </summary>
public class TypeField : IEquatable<TypeField>
{
	public Type T;

	public FieldInfo F;

	public TypeField(Type _T)
	{
		T = _T;
	}

	public TypeField(Type _T, FieldInfo _F)
		: this(_T)
	{
		F = _F;
	}

	public TypeField(Type _T, string FieldName)
		: this(_T)
	{
		F = T.GetField(FieldName);
	}

	public TypeField(TypeField o)
		: this(o.T, o.F)
	{
	}

	public static bool operator ==(TypeField o1, TypeField o2)
	{
		if ((object)o1 == o2)
		{
			return true;
		}
		if ((object)o1 == null)
		{
			return false;
		}
		if ((object)o2 == null)
		{
			return false;
		}
		if (o1.T != o2.T)
		{
			return false;
		}
		if (o1.F.Name != o2.F.Name)
		{
			return false;
		}
		return true;
	}

	public static bool operator !=(TypeField o1, TypeField o2)
	{
		return !(o1 == o2);
	}

	public override bool Equals(object o)
	{
		return (object)(o as TypeField) == this;
	}

	public bool Equals(TypeField o)
	{
		return o == this;
	}

	public override int GetHashCode()
	{
		return ((!(T == null)) ? T.GetHashCode() : 0) ^ ((!(F == null)) ? F.Name.GetHashCode() : 0);
	}

	public override string ToString()
	{
		if (T == null || F == null)
		{
			return null;
		}
		return T.FullName + "." + F.Name;
	}
}
