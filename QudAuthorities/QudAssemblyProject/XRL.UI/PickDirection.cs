using ConsoleLib.Console;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("PickDirection", true, false, false, "Adventure,Cancel", null, false, 0, false)]
public class PickDirection : IWantsTextConsoleInit
{
	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	public static ScreenBuffer Buffer = ScreenBuffer.create(80, 25);

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static string ShowPicker(string Label = null)
	{
		GameManager.Instance.PushGameView("PickDirection");
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Buffer.Copy(TextConsole.CurrentBuffer);
		Physics obj = XRLCore.Core.Game.Player.Body.GetPart("Physics") as Physics;
		bool flag = false;
		if (obj != null)
		{
			while (!flag)
			{
				Event.ResetPool(resetMinEventPools: false);
				XRLCore.Core.RenderMapToBuffer(Buffer);
				Buffer.Goto(2, 0);
				Buffer.Write(Label ?? "[Select a direction]");
				_TextConsole.DrawBuffer(Buffer);
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				_ = 252;
				if (keys == Keys.NumPad5)
				{
					GameManager.Instance.PopGameView(bHard: true);
					_TextConsole.DrawBuffer(OldBuffer);
					return ".";
				}
				switch (keys)
				{
				case Keys.Escape:
				case Keys.NumPad5:
					GameManager.Instance.PopGameView(bHard: true);
					_TextConsole.DrawBuffer(OldBuffer);
					return null;
				case Keys.NumPad1:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "SW";
				case Keys.NumPad2:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "S";
				case Keys.NumPad3:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "SE";
				case Keys.NumPad4:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "W";
				case Keys.NumPad6:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "E";
				case Keys.NumPad7:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "NW";
				case Keys.NumPad8:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "N";
				case Keys.NumPad9:
					_TextConsole.DrawBuffer(OldBuffer);
					GameManager.Instance.PopGameView(bHard: true);
					return "NE";
				case Keys.MouseEvent:
				{
					if (Keyboard.CurrentMouseEvent.Event == "RightClick")
					{
						GameManager.Instance.PopGameView(bHard: true);
						return null;
					}
					if (!(Keyboard.CurrentMouseEvent.Event == "LeftClick") || XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell == null)
					{
						break;
					}
					Point2D pos2D = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.Pos2D;
					Point2D point2D = new Point2D(Keyboard.CurrentMouseEvent.x, Keyboard.CurrentMouseEvent.y);
					if (pos2D == point2D)
					{
						GameManager.Instance.PopGameView(bHard: true);
						return ".";
					}
					for (int i = 0; i < Directions.DirectionList.Length; i++)
					{
						if (point2D == pos2D.FromDirection(Directions.DirectionList[i]))
						{
							GameManager.Instance.PopGameView(bHard: true);
							return Directions.DirectionList[i];
						}
					}
					break;
				}
				}
			}
		}
		GameManager.Instance.PopGameView(bHard: true);
		return null;
	}
}
