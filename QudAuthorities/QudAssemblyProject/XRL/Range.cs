using System;

namespace XRL;

[Serializable]
public class Range
{
	public int Min;

	public int Max;

	public Range(int Amount)
	{
		Min = Amount;
		Max = Amount;
	}

	public Range(int _Min, int _Max)
	{
		Min = _Min;
		Max = _Max;
	}
}
