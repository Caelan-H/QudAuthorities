using System;
using System.Text;
using XRL.Language;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Capabilities;

public static class Messaging
{
	private static void HandleMessage(GameObject what, string Msg, bool FromDialog = false, bool UsePopup = false)
	{
		if (!UsePopup && (!FromDialog || what == null || !what.IsPlayer()))
		{
			MessageQueue.AddPlayerMessage(Msg);
		}
		else
		{
			Popup.Show(Msg);
		}
	}

	private static void HandleMessage(GameObject what, StringBuilder Msg, bool FromDialog = false, bool UsePopup = false)
	{
		HandleMessage(what, Msg.ToString(), FromDialog, UsePopup);
	}

	public static void EmitMessage(GameObject what, string Msg, bool FromDialog = false, bool UsePopup = false)
	{
		if (what != null && (what.IsPlayer() || what.IsVisible()))
		{
			HandleMessage(what, Msg, FromDialog, UsePopup);
		}
	}

	public static void EmitMessage(GameObject what, StringBuilder Msg, bool FromDialog = false, bool UsePopup = false)
	{
		EmitMessage(what, Msg.ToString(), FromDialog, UsePopup);
	}

	public static void XDidY(GameObject what, string verb, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		if (what == null)
		{
			return;
		}
		string text = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
		if (what.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (text != null)
			{
				stringBuilder.Append("{{").Append(text).Append('|');
			}
			stringBuilder.Append("You ").Append(verb);
			AppendWithSpaceIfNeeded(stringBuilder, extra);
			stringBuilder.Append(terminalPunctuation ?? ".");
			if (text != null)
			{
				stringBuilder.Append("}}");
			}
			HandleMessage(MessageActor ?? what, stringBuilder, FromDialog, UsePopup);
		}
		else
		{
			if (!AlwaysVisible && !(UseVisibilityOf ?? what).IsVisible())
			{
				return;
			}
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			if (text != null)
			{
				stringBuilder2.Append("{{").Append(text).Append('|');
			}
			string value = null;
			if (DescribeSubjectDirection || DescribeSubjectDirectionLate)
			{
				value = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? what);
			}
			string defaultDefiniteArticle = null;
			bool withIndefiniteArticle = IndefiniteSubject;
			if (SubjectPossessedBy != null && (string.IsNullOrEmpty(value) || DescribeSubjectDirectionLate))
			{
				if (SubjectPossessedBy.IsPlayer())
				{
					defaultDefiniteArticle = "your";
					value = null;
				}
				else
				{
					defaultDefiniteArticle = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
					withIndefiniteArticle = false;
				}
			}
			else
			{
				GameObject gameObject = what.Equipped ?? what.InInventory;
				if (gameObject != null)
				{
					if (gameObject.IsPlayer())
					{
						defaultDefiniteArticle = "your";
						value = null;
					}
					else
					{
						defaultDefiniteArticle = Grammar.MakePossessive(gameObject.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
						withIndefiniteArticle = false;
					}
				}
			}
			stringBuilder2.Append(what.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle, defaultDefiniteArticle));
			if (!string.IsNullOrEmpty(value) && !DescribeSubjectDirectionLate)
			{
				if (SubjectPossessedBy != null && !what.HasProperName)
				{
					stringBuilder2.Append(" of ").Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject)).Append(' ')
						.Append(value);
				}
				else
				{
					stringBuilder2.Append(' ').Append(value);
				}
			}
			stringBuilder2.Append(what.GetVerb(verb));
			AppendWithSpaceIfNeeded(stringBuilder2, extra);
			if (!string.IsNullOrEmpty(value) && DescribeSubjectDirectionLate)
			{
				stringBuilder2.Append(' ').Append(value);
			}
			stringBuilder2.Append(terminalPunctuation ?? ".");
			if (text != null)
			{
				stringBuilder2.Append("}}");
			}
			HandleMessage(MessageActor ?? what, stringBuilder2, FromDialog, UsePopup);
		}
	}

	public static void XDidYToZ(GameObject what, string verb, string preposition = null, GameObject obj = null, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		try
		{
			if (what == null)
			{
				return;
			}
			if (what.IsPlayer())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				string text = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
				if (text != null)
				{
					stringBuilder.Append("{{").Append(text).Append('|');
				}
				stringBuilder.Append("You ").Append(verb).Append(' ');
				if (!string.IsNullOrEmpty(preposition))
				{
					stringBuilder.Append(preposition).Append(' ');
				}
				if (obj.IsPlayer())
				{
					stringBuilder.Append(PossessiveObject ? obj.its : obj.itself);
				}
				else
				{
					string defaultDefiniteArticle = null;
					bool withIndefiniteArticle = IndefiniteObject;
					if (ObjectPossessedBy != null)
					{
						defaultDefiniteArticle = ObjectPossessedBy.its;
						withIndefiniteArticle = false;
					}
					string text2 = obj.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle, defaultDefiniteArticle);
					if (PossessiveObject)
					{
						text2 = Grammar.MakePossessive(text2);
					}
					stringBuilder.Append(text2);
				}
				AppendWithSpaceIfNeeded(stringBuilder, extra);
				stringBuilder.Append(terminalPunctuation ?? ".");
				if (text != null)
				{
					stringBuilder.Append("}}");
				}
				HandleMessage(MessageActor ?? what, stringBuilder, FromDialog, UsePopup);
			}
			else
			{
				if (!AlwaysVisible && !(UseVisibilityOf ?? what).IsVisible())
				{
					return;
				}
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				string text3 = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
				if (text3 != null)
				{
					stringBuilder2.Append("{{").Append(text3).Append('|');
				}
				string value = null;
				if (DescribeSubjectDirection || DescribeSubjectDirectionLate)
				{
					value = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? what);
				}
				string defaultDefiniteArticle2 = null;
				bool withIndefiniteArticle2 = IndefiniteSubject;
				if (SubjectPossessedBy != null && (string.IsNullOrEmpty(value) || DescribeSubjectDirectionLate))
				{
					if (SubjectPossessedBy.IsPlayer())
					{
						defaultDefiniteArticle2 = "your";
						value = null;
					}
					else
					{
						defaultDefiniteArticle2 = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
					}
				}
				else
				{
					GameObject gameObject = what.Equipped ?? what.InInventory;
					if (gameObject != null)
					{
						if (gameObject.IsPlayer())
						{
							defaultDefiniteArticle2 = "your";
							value = null;
						}
						else
						{
							defaultDefiniteArticle2 = Grammar.MakePossessive(gameObject.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
							withIndefiniteArticle2 = false;
						}
					}
				}
				stringBuilder2.Append(what.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle2, defaultDefiniteArticle2));
				if (!string.IsNullOrEmpty(value) && !DescribeSubjectDirectionLate)
				{
					if (SubjectPossessedBy != null && !what.HasProperName)
					{
						stringBuilder2.Append(" of ").Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject)).Append(' ')
							.Append(value);
					}
					else
					{
						stringBuilder2.Append(' ').Append(value);
					}
				}
				stringBuilder2.Append(what.GetVerb(verb));
				AppendWithSpaceIfNeeded(stringBuilder2, preposition);
				stringBuilder2.Append(' ');
				if (obj == what)
				{
					stringBuilder2.Append(PossessiveObject ? what.its : what.itself);
				}
				else if (obj.IsPlayer())
				{
					stringBuilder2.Append(PossessiveObject ? "your" : "you");
				}
				else
				{
					string defaultDefiniteArticle3 = null;
					bool withIndefiniteArticle3 = IndefiniteObject || IndefiniteObjectForOthers;
					if (ObjectPossessedBy != null && !obj.HasProperName)
					{
						defaultDefiniteArticle3 = ObjectPossessedBy.its;
						withIndefiniteArticle3 = false;
					}
					string text4 = obj.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle3, defaultDefiniteArticle3);
					if (PossessiveObject)
					{
						text4 = Grammar.MakePossessive(text4);
					}
					stringBuilder2.Append(text4);
				}
				AppendWithSpaceIfNeeded(stringBuilder2, extra);
				if (!string.IsNullOrEmpty(value) && DescribeSubjectDirectionLate)
				{
					stringBuilder2.Append(' ').Append(value);
				}
				stringBuilder2.Append(terminalPunctuation ?? ".");
				if (text3 != null)
				{
					stringBuilder2.Append("}}");
				}
				HandleMessage(MessageActor ?? what, stringBuilder2, FromDialog, UsePopup);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("XDidYToZ", x);
		}
	}

	public static void WDidXToYWithZ(GameObject what, string verb, string directPreposition, GameObject directObject, string indirectPreposition, GameObject indirectObject, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool indefiniteDirectObject = false, bool indefiniteIndirectObject = false, bool indefiniteDirectObjectForOthers = false, bool indefiniteIndirectObjectForOthers = false, bool possessiveDirectObject = false, bool possessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject directObjectPossessedBy = null, GameObject indirectObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		if (what == null)
		{
			return;
		}
		if (what.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			string text = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
			if (text != null)
			{
				stringBuilder.Append("{{").Append(text).Append('|');
			}
			stringBuilder.Append("You ").Append(verb);
			AppendWithSpaceIfNeeded(stringBuilder, directPreposition);
			stringBuilder.Append(' ');
			if (directObject.IsPlayer())
			{
				stringBuilder.Append(possessiveDirectObject ? directObject.its : directObject.itself);
			}
			else
			{
				string defaultDefiniteArticle = null;
				bool withIndefiniteArticle = indefiniteDirectObject;
				if (directObjectPossessedBy != null)
				{
					defaultDefiniteArticle = directObjectPossessedBy.its;
					withIndefiniteArticle = false;
				}
				string text2 = directObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle, defaultDefiniteArticle);
				if (possessiveDirectObject)
				{
					text2 = Grammar.MakePossessive(text2);
				}
				stringBuilder.Append(text2);
			}
			AppendWithSpaceIfNeeded(stringBuilder, indirectPreposition);
			stringBuilder.Append(' ');
			if (indirectObject == directObject)
			{
				stringBuilder.Append(possessiveIndirectObject ? indirectObject.itself : indirectObject.them);
			}
			else if (indirectObject.IsPlayer())
			{
				stringBuilder.Append(possessiveIndirectObject ? "yours" : "you");
			}
			else
			{
				string defaultDefiniteArticle2 = null;
				bool withIndefiniteArticle2 = indefiniteIndirectObject;
				if (indirectObjectPossessedBy != null)
				{
					defaultDefiniteArticle2 = indirectObjectPossessedBy.its;
				}
				string text3 = indirectObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle2, defaultDefiniteArticle2);
				if (possessiveIndirectObject)
				{
					text3 = Grammar.MakePossessive(text3);
				}
				stringBuilder.Append(text3);
			}
			AppendWithSpaceIfNeeded(stringBuilder, extra);
			stringBuilder.Append(terminalPunctuation ?? ".");
			if (text != null)
			{
				stringBuilder.Append("}}");
			}
			HandleMessage(MessageActor ?? what, stringBuilder, FromDialog, UsePopup);
		}
		else
		{
			if (!AlwaysVisible && !(UseVisibilityOf ?? what).IsVisible())
			{
				return;
			}
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			string text4 = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
			if (text4 != null)
			{
				stringBuilder2.Append("{{").Append(text4).Append('|');
			}
			string value = null;
			if (DescribeSubjectDirection || DescribeSubjectDirectionLate)
			{
				value = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? what);
			}
			string defaultDefiniteArticle3 = null;
			bool withIndefiniteArticle3 = IndefiniteSubject;
			if (SubjectPossessedBy != null && (string.IsNullOrEmpty(value) || DescribeSubjectDirectionLate))
			{
				if (SubjectPossessedBy.IsPlayer())
				{
					defaultDefiniteArticle3 = "your";
					value = null;
				}
				else
				{
					defaultDefiniteArticle3 = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
					withIndefiniteArticle3 = false;
				}
			}
			else
			{
				GameObject gameObject = what.Equipped ?? what.InInventory;
				if (gameObject != null)
				{
					if (gameObject.IsPlayer())
					{
						defaultDefiniteArticle3 = "your";
						value = null;
					}
					else
					{
						defaultDefiniteArticle3 = Grammar.MakePossessive(gameObject.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject));
						withIndefiniteArticle3 = false;
					}
				}
			}
			stringBuilder2.Append(what.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle3, defaultDefiniteArticle3));
			if (!string.IsNullOrEmpty(value) && !DescribeSubjectDirectionLate)
			{
				if (SubjectPossessedBy != null && !what.HasProperName)
				{
					stringBuilder2.Append(" of ").Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, IndefiniteSubject)).Append(' ')
						.Append(value);
				}
				else
				{
					stringBuilder2.Append(' ').Append(value);
				}
			}
			stringBuilder2.Append(what.GetVerb(verb));
			AppendWithSpaceIfNeeded(stringBuilder2, directPreposition);
			stringBuilder2.Append(' ');
			if (directObject == what)
			{
				stringBuilder2.Append(possessiveDirectObject ? what.its : what.itself);
			}
			else if (directObject.IsPlayer())
			{
				stringBuilder2.Append(possessiveDirectObject ? "yours" : "you");
			}
			else
			{
				string defaultDefiniteArticle4 = null;
				bool withIndefiniteArticle4 = indefiniteDirectObject || indefiniteDirectObjectForOthers;
				if (directObjectPossessedBy != null && !directObject.HasProperName)
				{
					defaultDefiniteArticle4 = directObjectPossessedBy.its;
					withIndefiniteArticle4 = false;
				}
				string text5 = directObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle4, defaultDefiniteArticle4);
				if (possessiveDirectObject)
				{
					text5 = Grammar.MakePossessive(text5);
				}
				stringBuilder2.Append(text5);
			}
			AppendWithSpaceIfNeeded(stringBuilder2, indirectPreposition);
			stringBuilder2.Append(' ');
			if (indirectObject == directObject)
			{
				stringBuilder2.Append(what.them);
			}
			else if (indirectObject == what)
			{
				stringBuilder2.Append(possessiveIndirectObject ? what.its : what.itself);
			}
			else if (indirectObject.IsPlayer())
			{
				stringBuilder2.Append(possessiveIndirectObject ? "yours" : "you");
			}
			else
			{
				string defaultDefiniteArticle5 = null;
				bool withIndefiniteArticle5 = indefiniteIndirectObject || indefiniteIndirectObjectForOthers;
				if (indirectObjectPossessedBy != null)
				{
					defaultDefiniteArticle5 = indirectObjectPossessedBy.its;
					withIndefiniteArticle5 = false;
				}
				string text6 = indirectObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, !UseFullNames, BaseOnly: false, withIndefiniteArticle5, defaultDefiniteArticle5);
				if (possessiveIndirectObject)
				{
					text6 = Grammar.MakePossessive(text6);
				}
				stringBuilder2.Append(text6);
			}
			AppendWithSpaceIfNeeded(stringBuilder2, extra);
			if (!string.IsNullOrEmpty(value) && DescribeSubjectDirectionLate)
			{
				stringBuilder2.Append(' ').Append(value);
			}
			stringBuilder2.Append(terminalPunctuation ?? ".");
			if (text4 != null)
			{
				stringBuilder2.Append("}}");
			}
			HandleMessage(MessageActor ?? what, stringBuilder2, FromDialog, UsePopup);
		}
	}

	private static void AppendWithSpaceIfNeeded(StringBuilder SB, string what)
	{
		if (!string.IsNullOrEmpty(what))
		{
			if (!what.StartsWith(",") && !what.StartsWith(";"))
			{
				SB.Append(' ');
			}
			SB.Append(what);
		}
	}
}
