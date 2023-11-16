using Steamworks;
using UnityEngine;

namespace XRL;

public class SteamWorkshopInfo
{
	public ulong WorkshopId;

	public string Title;

	public string Description;

	public string Tags;

	public string Visibility;

	public string ImagePath;

	/// <summary>
	///             Open the mod's workshop page in the steam overlay, client, or browser.
	///             </summary>
	public void OpenWorkshopPage()
	{
		if (WorkshopId == 0L)
		{
			return;
		}
		if (SteamManager.Initialized)
		{
			string text = "steam://url/CommunityFilePage/" + WorkshopId;
			if (SteamUtils.IsOverlayEnabled())
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Application.OpenURL(text);
			}
		}
		else
		{
			Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + WorkshopId);
		}
	}
}
