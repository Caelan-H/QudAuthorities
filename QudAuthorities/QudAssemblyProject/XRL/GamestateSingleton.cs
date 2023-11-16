using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class)]
public class GamestateSingleton : Attribute
{
	public string id;

	public GamestateSingleton(string id)
	{
		this.id = id;
	}
}
