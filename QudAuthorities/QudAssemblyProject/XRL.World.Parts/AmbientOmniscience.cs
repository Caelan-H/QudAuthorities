using System;

namespace XRL.World.Parts;

[Serializable]
public class AmbientOmniscience : IPart
{
	public bool IsRealityDistortionBased;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (ParentObject.InSameZone(The.Player))
		{
			if (IsRealityDistortionBased)
			{
				Zone currentZone = ParentObject.CurrentZone;
				for (int i = 0; i < currentZone.Height; i++)
				{
					for (int j = 0; j < currentZone.Width; j++)
					{
						if (currentZone.GetCell(j, i).FireEvent("CheckRealityDistortionUsability"))
						{
							currentZone.AddLight(j, i, 0, LightLevel.Omniscient);
						}
					}
				}
			}
			else
			{
				ParentObject.CurrentZone.AddLight(LightLevel.Omniscient);
			}
		}
		return base.HandleEvent(E);
	}
}
