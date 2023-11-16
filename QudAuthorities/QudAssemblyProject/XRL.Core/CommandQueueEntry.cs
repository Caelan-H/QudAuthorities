using System;

namespace XRL.Core;

[Serializable]
public class CommandQueueEntry
{
	public string Action;

	public object Target;

	public int SegmentDelay;
}
