using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Parts;

namespace Qud.API;

public static class AbilityAPI
{
	public static int abilityCount
	{
		get
		{
			if (XRLCore.Core == null)
			{
				return 0;
			}
			if (XRLCore.Core.Game == null)
			{
				return 0;
			}
			if (XRLCore.Core.Game.Player == null)
			{
				return 0;
			}
			if (XRLCore.Core.Game.Player.Body == null)
			{
				return 0;
			}
			return XRLCore.Core.Game.Player.Body.GetPart<ActivatedAbilities>()?.AbilityByGuid.Keys.Count ?? 0;
		}
	}

	public static ActivatedAbilityEntry GetAbility(int nAbility)
	{
		if (XRLCore.Core == null)
		{
			return null;
		}
		if (XRLCore.Core.Game == null)
		{
			return null;
		}
		if (XRLCore.Core.Game.Player == null)
		{
			return null;
		}
		if (XRLCore.Core.Game.Player.Body == null)
		{
			return null;
		}
		ActivatedAbilities part = XRLCore.Core.Game.Player.Body.GetPart<ActivatedAbilities>();
		if (part == null)
		{
			return null;
		}
		int num = 0;
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in part.AbilityByGuid)
		{
			if (num == nAbility)
			{
				return item.Value;
			}
			num++;
		}
		return null;
	}
}
