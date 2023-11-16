using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Field)]
public class GameBasedStaticCacheAttribute : Attribute
{
	public bool CreateInstance = true;
}
