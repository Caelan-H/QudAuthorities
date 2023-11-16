using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using XRL.Core;

public static class LeaderboardManager
{
	private class LeaderboardRecordRequest
	{
		public string type;

		public string id;

		public int requestTop;

		public int numRecords;

		public int score;

		public bool friendsonly;

		public Action<LeaderboardScoresDownloaded_t> scoreCallback;
	}

	private const ELeaderboardUploadScoreMethod s_leaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

	private static CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();

	private static CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

	private static CallResult<LeaderboardScoresDownloaded_t> m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

	private static SteamLeaderboard_t currentLeaderboard;

	private static Queue<LeaderboardRecordRequest> requests = new Queue<LeaderboardRecordRequest>();

	private static LeaderboardRecordRequest currentRequest = null;

	public static Dictionary<string, SteamLeaderboard_t> leaderboardID = new Dictionary<string, SteamLeaderboard_t>();

	private static Action<LeaderboardScoresDownloaded_t> leaderboardEntriesCallback = null;

	public static Dictionary<string, string> leaderboardresults = new Dictionary<string, string>();

	public static void Update()
	{
		if (requests.Count > 0 && currentRequest == null)
		{
			lock (requests)
			{
				currentRequest = requests.Dequeue();
				leaderboardEntriesCallback = currentRequest.scoreCallback;
				FindOrCreateLeaderboard(currentRequest.id);
			}
		}
	}

	private static void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
	{
		try
		{
			Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);
			currentLeaderboard = pCallback.m_hSteamLeaderboard;
			if (pCallback.m_bLeaderboardFound != 0)
			{
				if (leaderboardID.ContainsKey(currentRequest.id))
				{
					leaderboardID[currentRequest.id] = pCallback.m_hSteamLeaderboard;
				}
				else
				{
					leaderboardID.Add(currentRequest.id, pCallback.m_hSteamLeaderboard);
				}
				if (currentRequest.type == "getleaderboard")
				{
					ELeaderboardDataRequest eLeaderboardDataRequest = (currentRequest.friendsonly ? ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends : ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal);
					SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, eLeaderboardDataRequest, currentRequest.requestTop, currentRequest.requestTop + currentRequest.numRecords);
					m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
					m_downloadResult.Set(hAPICall, OnDownloadLeaderboardEntries);
				}
				else if (currentRequest.type == "submitleaderboardscore")
				{
					SteamAPICall_t hAPICall2 = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, currentRequest.score, null, 0);
					m_uploadResult.Set(hAPICall2, OnLeaderboardUploadResult);
				}
				else if (currentRequest.type == "getleaderboardrank")
				{
					SteamAPICall_t hAPICall3 = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -4, 5);
					m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
					m_downloadResult.Set(hAPICall3, OnDownloadLeaderboardEntries);
				}
			}
			else
			{
				Debug.LogError("Couldn't find leaderboard");
				currentRequest = null;
			}
		}
		catch (Exception ex)
		{
			currentRequest = null;
			XRLCore.LogError("OnLeaderboardFindResult", ex);
		}
	}

	private static void OnDownloadLeaderboardEntries(LeaderboardScoresDownloaded_t pCallback, bool failure)
	{
		if (!failure && leaderboardEntriesCallback != null)
		{
			try
			{
				leaderboardEntriesCallback(pCallback);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		leaderboardEntriesCallback = null;
		currentRequest = null;
	}

	private static void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
	{
		try
		{
			Debug.Log("STEAM LEADERBOARDS: failure - " + failure + " Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -4, 5);
			m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
			m_downloadResult.Set(hAPICall, OnDownloadLeaderboardEntries);
		}
		catch (Exception ex)
		{
			currentRequest = null;
			XRLCore.LogError("OnLeaderboardUploadResult", ex);
		}
	}

	public static void FindOrCreateLeaderboard(string id)
	{
		try
		{
			SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(id, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
			m_findResult = new CallResult<LeaderboardFindResult_t>();
			m_findResult.Set(hAPICall, OnLeaderboardFindResult);
		}
		catch (Exception ex)
		{
			currentRequest = null;
			XRLCore.LogError("FindOrCreateLeaderboard", ex);
		}
	}

	public static string SubmitResult(string id, int score, Action<LeaderboardScoresDownloaded_t> callback)
	{
		if (!SteamManager.Initialized)
		{
			Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
			return null;
		}
		lock (requests)
		{
			string[] array = id.Split(':');
			string text = "unknown " + id;
			if (array[0] == "weekly")
			{
				text = array[0] + " for week " + array[2] + " of " + array[1];
			}
			if (array[0] == "daily")
			{
				text = array[0] + " for day " + array[2] + " of " + array[1];
			}
			Debug.Log("uploading score(" + score + ") to steam leaderboard(" + text + ")");
			LeaderboardRecordRequest leaderboardRecordRequest = new LeaderboardRecordRequest();
			leaderboardRecordRequest.type = "submitleaderboardscore";
			leaderboardRecordRequest.id = text;
			leaderboardRecordRequest.score = score;
			leaderboardRecordRequest.scoreCallback = callback;
			requests.Enqueue(leaderboardRecordRequest);
			return text;
		}
	}

	public static string GetLeaderboard(string id, int requestTop, int numRecords, bool bFriendsOnly, Action<LeaderboardScoresDownloaded_t> callback)
	{
		if (!SteamManager.Initialized)
		{
			Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
			return null;
		}
		lock (requests)
		{
			string[] array = id.Split(':');
			string text = "unknown " + id;
			if (array[0] == "weekly")
			{
				text = array[0] + " for week " + array[2] + " of " + array[1];
			}
			if (array[0] == "daily")
			{
				text = array[0] + " for day " + array[2] + " of " + array[1];
			}
			Debug.Log("getting steam leaderboard(" + text + ")");
			LeaderboardRecordRequest leaderboardRecordRequest = new LeaderboardRecordRequest();
			leaderboardRecordRequest.type = "getleaderboard";
			leaderboardRecordRequest.id = text;
			leaderboardRecordRequest.scoreCallback = callback;
			leaderboardRecordRequest.requestTop = requestTop;
			leaderboardRecordRequest.numRecords = numRecords;
			leaderboardRecordRequest.friendsonly = bFriendsOnly;
			requests.Enqueue(leaderboardRecordRequest);
			return text;
		}
	}

	public static string GetLeaderboardRank(string leaderboardID, Action<LeaderboardScoresDownloaded_t> callback)
	{
		if (!SteamManager.Initialized)
		{
			Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
			return null;
		}
		lock (requests)
		{
			LeaderboardRecordRequest leaderboardRecordRequest = new LeaderboardRecordRequest();
			leaderboardRecordRequest.type = "getleaderboardrank";
			leaderboardRecordRequest.id = leaderboardID;
			leaderboardRecordRequest.scoreCallback = callback;
			requests.Enqueue(leaderboardRecordRequest);
			return leaderboardID;
		}
	}
}
