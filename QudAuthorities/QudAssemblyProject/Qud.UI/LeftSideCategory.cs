using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class LeftSideCategory : MonoBehaviour, IFrameworkControl
{
	public UITextSkin text;

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>().context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is KeybindCategoryRow keybindCategoryRow)
		{
			text.SetText("{{C|" + keybindCategoryRow.CategoryDescription + "}}");
		}
	}
}
