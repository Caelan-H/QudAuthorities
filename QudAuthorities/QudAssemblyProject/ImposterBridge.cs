using UnityEngine;

public static class ImposterBridge
{
	public static void DestroyImposter(long imposterID)
	{
		ImposterState qudImposterState = ImposterManager.getQudImposterState(imposterID);
		if (qudImposterState == null)
		{
			Debug.LogWarning(">>>destroying a nonexisting thing?<<<");
			return;
		}
		qudImposterState.destroyed = true;
		qudImposterState = qudImposterState.clone();
		QudScreenBufferExtra.offbandUpdates.Add(qudImposterState);
	}
}
