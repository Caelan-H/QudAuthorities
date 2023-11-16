using XRL.UI.Framework;

namespace XRL.CharacterBuilds.UI;

public class MenuOption : FrameworkDataElement
{
	public string KeyDescription;

	public string InputCommand;

	public string getKeyDescription()
	{
		if (!string.IsNullOrEmpty(InputCommand))
		{
			KeyDescription = ControlManager.getCommandInputDescription(InputCommand);
		}
		return KeyDescription;
	}

	public string getMenuText()
	{
		if (!string.IsNullOrEmpty(InputCommand))
		{
			KeyDescription = ControlManager.getCommandInputDescription(InputCommand);
		}
		if (!string.IsNullOrEmpty(KeyDescription))
		{
			return "[{{W|" + KeyDescription + "}}] " + Description;
		}
		return Description;
	}
}
