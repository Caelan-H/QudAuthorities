using System;
using Rewired;
using XRL.UI.Framework;

namespace Qud.UI;

[Serializable]
public class KeybindDataRow : FrameworkDataElement
{
	public string CategoryId;

	public string KeyId;

	public string KeyDescription;

	public string Bind1;

	public ActionElementMap rewiredBind1;

	public string Bind2;

	public ActionElementMap rewiredBind2;

	public Pole rewairedAxisContribution;

	public InputAction rewiredInputAction;

	public InputCategory rewiredInputCategory;

	public ControllerMap rewiredControllerMap;

	public AxisRange rewiredAxisRange;
}
