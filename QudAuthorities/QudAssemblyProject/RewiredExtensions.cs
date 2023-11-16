using System.Collections.Generic;
using Rewired;
using UnityEngine;

public static class RewiredExtensions
{
	public static Dictionary<string, float> delayTimers = new Dictionary<string, float>();

	public static Dictionary<string, float> repeatTimers = new Dictionary<string, float>();

	public static Dictionary<string, float> delayTimersNegative = new Dictionary<string, float>();

	public static Dictionary<string, float> repeatTimersNegative = new Dictionary<string, float>();

	public static float delaytime = 0.5f;

	public static float repeattime = 0.1f;

	public static bool GetButtonDownRepeating(this Player player, string button)
	{
		if (player.GetButtonDown(button))
		{
			delayTimers.Set(button, delaytime);
			repeatTimers.Remove(button);
			return true;
		}
		if (player.GetButton(button))
		{
			if (!delayTimers.ContainsKey(button))
			{
				delayTimers.Add(button, delaytime);
			}
			else if (delayTimers[button] <= 0f)
			{
				if (!repeatTimers.ContainsKey(button))
				{
					repeatTimers.Add(button, repeattime);
				}
				else
				{
					repeatTimers[button] -= Time.deltaTime;
					if (repeatTimers[button] <= 0f)
					{
						repeatTimers[button] = repeattime;
						return true;
					}
				}
			}
			else
			{
				delayTimers[button] -= Time.deltaTime;
			}
		}
		else
		{
			delayTimers.Remove(button);
			repeatTimers.Remove(button);
		}
		return false;
	}

	public static bool GetNegativeButtonDownRepeating(this Player player, string button)
	{
		if (player.GetNegativeButtonDown(button))
		{
			delayTimersNegative.Set(button, delaytime);
			repeatTimersNegative.Remove(button);
			return true;
		}
		if (player.GetNegativeButton(button))
		{
			if (!delayTimersNegative.ContainsKey(button))
			{
				delayTimersNegative.Add(button, delaytime);
			}
			else if (delayTimersNegative[button] <= 0f)
			{
				if (!repeatTimersNegative.ContainsKey(button))
				{
					repeatTimersNegative.Add(button, repeattime);
				}
				else
				{
					repeatTimersNegative[button] -= Time.deltaTime;
					if (repeatTimersNegative[button] <= 0f)
					{
						repeatTimersNegative[button] = repeattime;
						return true;
					}
				}
			}
			else
			{
				delayTimersNegative[button] -= Time.deltaTime;
			}
		}
		else
		{
			delayTimersNegative.Remove(button);
			repeatTimersNegative.Remove(button);
		}
		return false;
	}
}
