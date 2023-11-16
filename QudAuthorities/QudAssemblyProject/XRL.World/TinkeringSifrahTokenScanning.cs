using System;
using XRL.World.Capabilities;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenScanning : SifrahToken
{
	public TinkeringSifrahTokenScanning()
	{
		Description = "scanning";
		Tile = "Items/sw_aircurrent.bmp";
		RenderString = "å";
		ColorString = "&C";
		DetailColor = 'W';
	}

	public TinkeringSifrahTokenScanning(Scanning.Scan scan)
		: this()
	{
		Description = "read " + Scanning.GetScanSubjectName(scan);
	}
}
