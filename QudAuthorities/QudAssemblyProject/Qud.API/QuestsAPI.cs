using System;
using XRL.World;

namespace Qud.API;

public static class QuestsAPI
{
	public static Quest fabricateEmptyQuest()
	{
		return new Quest
		{
			ID = Guid.NewGuid().ToString()
		};
	}
}
