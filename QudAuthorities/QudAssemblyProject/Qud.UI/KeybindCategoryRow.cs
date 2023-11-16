using System;
using Rewired;
using XRL.UI.Framework;

namespace Qud.UI;

[Serializable]
public class KeybindCategoryRow : FrameworkDataElement
{
	public string CategoryId;

	public string CategoryDescription;

	public bool Collapsed;

	public InputCategory rewiredInputCategory;
}
