using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class ModSensitiveStaticCacheAttribute : Attribute
{
	public bool CreateEmptyInstance;

	public ModSensitiveStaticCacheAttribute(bool createEmptyInstance = false)
	{
		CreateEmptyInstance = createEmptyInstance;
	}
}
