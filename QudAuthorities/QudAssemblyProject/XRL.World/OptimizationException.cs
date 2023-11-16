using System;

namespace XRL.World;

public class OptimizationException : Exception
{
	public OptimizationException(string message)
		: base(message)
	{
	}
}
