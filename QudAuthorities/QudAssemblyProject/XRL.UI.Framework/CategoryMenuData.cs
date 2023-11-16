using System;
using System.Collections.Generic;
using XRL.CharacterBuilds.UI;

namespace XRL.UI.Framework;

[Serializable]
public class CategoryMenuData : FrameworkDataElement
{
	public string Title;

	public List<PrefixMenuOption> menuOptions;
}
