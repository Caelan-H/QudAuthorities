using System;
using System.IO;
using System.Net;
using Ionic.Zip;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.UI;

public static class CodeRedemptionManager
{
	public static void redeem(string code)
	{
		Popup.ShowProgress(delegate(Progress p)
		{
			if (string.IsNullOrEmpty(code) || Uri.EscapeUriString(code) != code)
			{
				Popup.Show("That code is invalid.");
			}
			else
			{
				code = code.ToUpper();
				if (code[0] != 'P')
				{
					Popup.Show("That code is invalid.");
				}
				else
				{
					p.setCurrentProgressText("Redeeming code...");
					p.setCurrentProgress(20);
					if (code[0] == 'P')
					{
						code = code.Substring(1);
						WebClient webClient = new WebClient();
						string text = DataManager.SavePath("Temp");
						Directory.CreateDirectory(text);
						string text2 = code + ".zip";
						string text3 = Path.Combine(text, text2).Replace('\\', '/');
						if (File.Exists(text3))
						{
							Debug.Log("Deleting existing file " + text3 + "...");
							File.Delete(text3);
						}
						p.setCurrentProgressText("Downloading pet...");
						p.setCurrentProgress(40);
						try
						{
							string address = "http://s3.us-east-2.amazonaws.com/cavesofqud/pets/" + text2;
							webClient.DownloadFile(address, text3);
						}
						catch (Exception ex)
						{
							Popup.Show("Error downloading pet: " + ex.ToString());
							return;
						}
						p.setCurrentProgressText("The pet finished downloading!");
						p.setCurrentProgress(60);
						p.setCurrentProgressText("Installing pet...");
						string text4 = DataManager.SavePath("Mods");
						Directory.CreateDirectory(text4);
						using (ZipFile zipFile = ZipFile.Read(text3))
						{
							foreach (ZipEntry item in zipFile)
							{
								item.Extract(text4, ExtractExistingFileAction.OverwriteSilently);
							}
						}
						p.setCurrentProgress(80);
						p.setCurrentProgressText("Reloading configuration...");
						ModManager.Init(Reload: true);
						ModManager.BuildScriptMods();
						XRLCore.Core.HotloadConfiguration();
						p.setCurrentProgress(100);
						Popup.Show("Your new pet is ready to love.");
					}
				}
			}
		});
	}
}
