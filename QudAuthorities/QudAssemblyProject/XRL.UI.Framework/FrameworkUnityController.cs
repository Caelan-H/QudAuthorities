using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRL.UI.Framework;

public class FrameworkUnityController : MonoBehaviour
{
	public NavigationController controller;

	private static HashSet<string> usedCommands = new HashSet<string>();

	public void Awake()
	{
		controller = NavigationController.instance;
	}

	public void Update()
	{
		NavigationContext navigationContext = controller.activeContext;
		usedCommands.Clear();
		while (navigationContext != null)
		{
			if (navigationContext.commandHandlers != null)
			{
				foreach (KeyValuePair<string, Action> commandHandler in navigationContext.commandHandlers)
				{
					if (usedCommands.Contains(commandHandler.Key))
					{
						continue;
					}
					usedCommands.Add(commandHandler.Key);
					if (ControlManager.isCommandDown(commandHandler.Key))
					{
						Event @event = controller.FireInputCommandEvent(commandHandler.Key);
						if (@event.cancelled || @event.handled)
						{
							return;
						}
					}
				}
			}
			navigationContext = navigationContext.parentContext;
		}
		if (controller.activeContext != null)
		{
			if (ControlManager.isCommandDown("Move West") || ControlManager.isCommandDown("Navigate Left"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationXAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Move East") || ControlManager.isCommandDown("Navigate Right"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationXAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Move North") || ControlManager.isCommandDown("Navigate Up"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationYAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Move South") || ControlManager.isCommandDown("Navigate Down"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationYAxis, null, 1);
			}
			if (ControlManager.isCommandDown("U Negative"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationUAxis, null, -1);
			}
			if (ControlManager.isCommandDown("U Positive"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationUAxis, null, 1);
			}
			if (ControlManager.isCommandDown("V Negative"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationVAxis, null, -1);
			}
			if (ControlManager.isCommandDown("V Positive"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationVAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Page Up"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageYAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Page Down"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageYAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Page Left"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageXAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Page Right"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageXAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Accept"))
			{
				controller.FireInputButtonEvent(InputButtonTypes.AcceptButton);
			}
			if (ControlManager.isCommandDown("Cancel"))
			{
				controller.FireInputButtonEvent(InputButtonTypes.CancelButton);
			}
		}
	}
}
