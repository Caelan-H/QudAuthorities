using System;

namespace XRL.World;

[Serializable]
public enum LightLevel : byte
{
	None = 0,
	Darkvision = 1,
	Dimvision = 15,
	Safelight = 30,
	Light = 200,
	Interpolight = 210,
	Radar = 228,
	LitRadar = 232,
	Omniscient = byte.MaxValue
}
