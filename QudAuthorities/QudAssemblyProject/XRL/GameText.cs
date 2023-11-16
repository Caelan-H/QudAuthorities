using System;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace XRL;

public static class GameText
{
	public static string GetRandomAlchemistSaying()
	{
		if (BookUI.Books.TryGetValue("AlchemistMutterings", out var value) && value.Count > 0)
		{
			return value[0].Lines.GetRandomElement();
		}
		return "";
	}

	public static string VariableReplace(string Message, GameObject Subject = null, string ExplicitSubject = null, bool ExplicitSubjectPlural = false, GameObject Object = null, string ExplicitObject = null, bool ExplicitObjectPlural = false)
	{
		if (string.IsNullOrEmpty(Message))
		{
			return Message;
		}
		return VariableReplace(Event.NewStringBuilder(Message), Subject, ExplicitSubject, ExplicitSubjectPlural, Object, ExplicitObject, ExplicitObjectPlural);
	}

	public static string VariableReplace(StringBuilder Message, GameObject Subject = null, string ExplicitSubject = null, bool ExplicitSubjectPlural = false, GameObject Object = null, string ExplicitObject = null, bool ExplicitObjectPlural = false)
	{
		if (Message == null)
		{
			return null;
		}
		if (Message.Length == 0)
		{
			return "";
		}
		bool flag = false;
		if (Message.Contains('='))
		{
			if (Message.Contains("=GGRESULT="))
			{
				Message.Replace("=GGRESULT=", ScriptCallToArmsPart.GenerateResultConversation());
				flag = true;
			}
			if (Message.Contains("=name="))
			{
				Message.Replace("=name=", The.Game?.PlayerName ?? "");
			}
			if (Message.Contains("=alchemist="))
			{
				Message.Replace("=alchemist=", GetRandomAlchemistSaying());
			}
			if (Message.Contains("=generic="))
			{
				Message.Replace("=generic=", Subject.GetPropertyOrTag("SimpleConversation") ?? "");
			}
			if (Message.Contains("=villageZeroName="))
			{
				Message.Replace("=villageZeroName=", The.Game?.GetStringGameState("villageZeroName") ?? "");
			}
			if (Message.Contains("=V0tinkeraddendum="))
			{
				XRLGame game = The.Game;
				if (game != null && game.AlternateStart)
				{
					Message.Replace("=V0tinkeraddendum=", " A tinker must have detected it and then recorded it onto the data disk you brought us. However, we've known about since it went live over a year ago.");
				}
				else
				{
					Message.Replace("=V0tinkeraddendum=", " It's been live for over a year.");
				}
			}
			if (Message.Contains("=EitherOrWhisper="))
			{
				Message.Replace("=EitherOrWhisper=", PetEitherOr.GetEitherOrAccomplishment());
			}
			if (Message.Contains("=SULTAN4="))
			{
				Message.Replace("=SULTAN4=", ((The.Game?.sultanHistory)?.GetEntitiesByDelegate(delegate(HistoricEntity e)
				{
					HistoricEntitySnapshot currentSnapshot = e.GetCurrentSnapshot();
					if (currentSnapshot != null && currentSnapshot.GetProperty("type")?.Equals("sultan") == true)
					{
						HistoricEntitySnapshot currentSnapshot2 = e.GetCurrentSnapshot();
						if (currentSnapshot2 == null)
						{
							return false;
						}
						return currentSnapshot2.GetProperty("period")?.Equals("4") == true;
					}
					return false;
				})?.GetRandomElement()?.GetCurrentSnapshot())?.GetProperty("name") ?? "the fourth sultan");
			}
		}
		if (Message.Contains('\r'))
		{
			Message.Replace("\r\n", "\n").Replace("\r", "\n");
		}
		if (Message.Contains('='))
		{
			if (The.Game != null)
			{
				Message.Replace("=GR1=", The.Game.GetStringGameState("GlotrotCure1"));
				Message.Replace("=GR2=", The.Game.GetStringGameState("GlotrotCure2"));
				Message.Replace("=GR3=", The.Game.GetStringGameState("GlotrotCure3"));
				Message.Replace("=IS1=", The.Game.GetStringGameState("IronshankCure"));
				Message.Replace("=MC1=", The.Game.GetStringGameState("MonochromeCure"));
				Message.Replace("=FCC=", The.Game.GetStringGameState("FungalCureWormDisplay"));
				Message.Replace("=FCL=", The.Game.GetStringGameState("FungalCureLiquidDisplay"));
				Message.Replace("=EskhindRoadDirection=", The.Game.GetStringGameState("EskhindRoadDirection"));
				Message.Replace("=SEEKERENEMY=", The.Game.GetStringGameState("SeekerEnemyFaction"));
				if (Message.Contains("=stringgamestate:"))
				{
					string[] array = Message.ToString().Split(new string[1] { "=stringgamestate:" }, StringSplitOptions.None);
					for (int i = 1; i < array.Length; i++)
					{
						int num = array[i].IndexOf('=');
						string @default = null;
						string text = ((num != -1) ? array[i].Substring(0, num) : array[i]);
						bool flag2 = false;
						bool flag3 = false;
						bool flag4 = false;
						while (true)
						{
							if (text.EndsWith(":capitalize"))
							{
								text = text.Substring(0, text.Length - ":capitalize".Length);
								flag2 = true;
								continue;
							}
							if (text.EndsWith(":lower"))
							{
								text = text.Substring(0, text.Length - ":lower".Length);
								flag3 = true;
								continue;
							}
							if (!text.EndsWith(":upper"))
							{
								break;
							}
							text = text.Substring(0, text.Length - ":upper".Length);
							flag4 = true;
						}
						int num2 = text.IndexOf(':');
						if (num2 != -1)
						{
							@default = text.Substring(num2 + 1);
							text = text.Substring(0, num2);
						}
						string text2 = The.Game.GetStringGameState(text, @default) ?? "";
						if (flag4)
						{
							text2 = ColorUtility.ToUpperExceptFormatting(text2);
						}
						if (flag3)
						{
							text2 = ColorUtility.ToLowerExceptFormatting(text2);
						}
						if (flag2)
						{
							text2 = ColorUtility.CapitalizeExceptFormatting(text2);
						}
						if (num == -1)
						{
							array[i] = text2;
						}
						else
						{
							array[i] = text2 + array[i].Substring(num + 1);
						}
					}
					Message.Clear();
					string[] array2 = array;
					foreach (string value in array2)
					{
						Message.Append(value);
					}
				}
				if (Message.Contains("=intgamestate:"))
				{
					string[] array3 = Message.ToString().Split(new string[1] { "=intgamestate:" }, StringSplitOptions.None);
					for (int k = 1; k < array3.Length; k++)
					{
						int num3 = array3[k].IndexOf('=');
						int result = 0;
						string text3 = ((num3 != -1) ? array3[k].Substring(0, num3) : array3[k]);
						bool flag5 = false;
						bool flag6 = false;
						bool flag7 = false;
						bool flag8 = false;
						bool flag9 = false;
						bool flag10 = false;
						bool flag11 = false;
						while (true)
						{
							if (text3.EndsWith(":cardinal"))
							{
								text3 = text3.Substring(0, text3.Length - ":cardinal".Length);
								flag5 = true;
								continue;
							}
							if (text3.EndsWith(":ordinal"))
							{
								text3 = text3.Substring(0, text3.Length - ":ordinal".Length);
								flag6 = true;
								continue;
							}
							if (text3.EndsWith(":multiplicative"))
							{
								text3 = text3.Substring(0, text3.Length - ":multiplicative".Length);
								flag7 = true;
								continue;
							}
							if (text3.EndsWith(":roman"))
							{
								text3 = text3.Substring(0, text3.Length - ":roman".Length);
								flag8 = true;
								continue;
							}
							if (text3.EndsWith(":capitalize"))
							{
								text3 = text3.Substring(0, text3.Length - ":capitalize".Length);
								flag9 = true;
								continue;
							}
							if (text3.EndsWith(":lower"))
							{
								text3 = text3.Substring(0, text3.Length - ":lower".Length);
								flag11 = true;
								continue;
							}
							if (!text3.EndsWith(":upper"))
							{
								break;
							}
							text3 = text3.Substring(0, text3.Length - ":upper".Length);
							flag10 = true;
						}
						int num4 = text3.IndexOf(':');
						if (num4 != -1)
						{
							int.TryParse(text3.Substring(num4 + 1), out result);
							text3 = text3.Substring(0, num4);
						}
						int intGameState = The.Game.GetIntGameState(text3, result);
						string text4 = (flag8 ? Grammar.GetRomanNumeral(intGameState) : (flag5 ? Grammar.Cardinal(intGameState) : (flag6 ? Grammar.Ordinal(intGameState) : ((!flag7) ? intGameState.ToString() : Grammar.Multiplicative(intGameState)))));
						if (flag10)
						{
							text4 = ColorUtility.ToUpperExceptFormatting(text4);
						}
						if (flag11)
						{
							text4 = ColorUtility.ToLowerExceptFormatting(text4);
						}
						if (flag9)
						{
							text4 = ColorUtility.CapitalizeExceptFormatting(text4);
						}
						if (num3 == -1)
						{
							array3[k] = text4;
						}
						else
						{
							array3[k] = text4 + array3[k].Substring(num3 + 1);
						}
					}
					Message.Clear();
					string[] array2 = array3;
					foreach (string value2 in array2)
					{
						Message.Append(value2);
					}
				}
				if (Message.Contains("=int64gamestate:"))
				{
					string[] array4 = Message.ToString().Split(new string[1] { "=int64gamestate:" }, StringSplitOptions.None);
					for (int l = 1; l < array4.Length; l++)
					{
						int num5 = array4[l].IndexOf('=');
						long result2 = 0L;
						string text5 = ((num5 != -1) ? array4[l].Substring(0, num5) : array4[l]);
						bool flag12 = false;
						bool flag13 = false;
						bool flag14 = false;
						bool flag15 = false;
						bool flag16 = false;
						bool flag17 = false;
						while (true)
						{
							if (text5.EndsWith(":cardinal"))
							{
								text5 = text5.Substring(0, text5.Length - ":cardinal".Length);
								flag12 = true;
								continue;
							}
							if (text5.EndsWith(":ordinal"))
							{
								text5 = text5.Substring(0, text5.Length - ":ordinal".Length);
								flag13 = true;
								continue;
							}
							if (text5.EndsWith(":roman"))
							{
								text5 = text5.Substring(0, text5.Length - ":roman".Length);
								flag14 = true;
								continue;
							}
							if (text5.EndsWith(":capitalize"))
							{
								text5 = text5.Substring(0, text5.Length - ":capitalize".Length);
								flag15 = true;
								continue;
							}
							if (text5.EndsWith(":lower"))
							{
								text5 = text5.Substring(0, text5.Length - ":lower".Length);
								flag17 = true;
								continue;
							}
							if (!text5.EndsWith(":upper"))
							{
								break;
							}
							text5 = text5.Substring(0, text5.Length - ":upper".Length);
							flag16 = true;
						}
						int num6 = text5.IndexOf(':');
						if (num6 != -1)
						{
							long.TryParse(text5.Substring(num6 + 1), out result2);
							text5 = text5.Substring(0, num6);
						}
						long int64GameState = The.Game.GetInt64GameState(text5, result2);
						string text6 = (flag14 ? Grammar.GetRomanNumeral(int64GameState) : (flag12 ? Grammar.Cardinal(int64GameState) : ((!flag13) ? int64GameState.ToString() : Grammar.Ordinal(int64GameState))));
						if (flag16)
						{
							text6 = ColorUtility.ToUpperExceptFormatting(text6);
						}
						if (flag17)
						{
							text6 = ColorUtility.ToLowerExceptFormatting(text6);
						}
						if (flag15)
						{
							text6 = ColorUtility.CapitalizeExceptFormatting(text6);
						}
						if (num5 == -1)
						{
							array4[l] = text6;
						}
						else
						{
							array4[l] = text6 + array4[l].Substring(num5 + 1);
						}
					}
					Message.Clear();
					string[] array2 = array4;
					foreach (string value3 in array2)
					{
						Message.Append(value3);
					}
				}
				if (Message.Contains("=booleangamestate:"))
				{
					string[] array5 = Message.ToString().Split(new string[1] { "=booleangamestate:" }, StringSplitOptions.None);
					for (int m = 1; m < array5.Length; m++)
					{
						int num7 = array5[m].IndexOf('=');
						bool result3 = false;
						string text7 = "true";
						string text8 = "false";
						string text9 = ((num7 != -1) ? array5[m].Substring(0, num7) : array5[m]);
						bool flag18 = false;
						bool flag19 = false;
						bool flag20 = false;
						while (true)
						{
							if (text9.EndsWith(":capitalize"))
							{
								text9 = text9.Substring(0, text9.Length - ":capitalize".Length);
								flag18 = true;
								continue;
							}
							if (text9.EndsWith(":lower"))
							{
								text9 = text9.Substring(0, text9.Length - ":lower".Length);
								flag20 = true;
								continue;
							}
							if (!text9.EndsWith(":upper"))
							{
								break;
							}
							text9 = text9.Substring(0, text9.Length - ":upper".Length);
							flag19 = true;
						}
						string[] array6 = text9.Split(':');
						if (array6.Length >= 4)
						{
							text8 = array6[3];
						}
						if (array6.Length >= 3)
						{
							text7 = array6[2];
						}
						if (array6.Length >= 2)
						{
							if (array6[1] != "")
							{
								bool.TryParse(array6[1], out result3);
							}
							text9 = array6[0];
						}
						string text10 = (The.Game.GetBooleanGameState(text9, result3) ? text7 : text8);
						if (flag19)
						{
							text10 = ColorUtility.ToUpperExceptFormatting(text10);
						}
						if (flag20)
						{
							text10 = ColorUtility.ToLowerExceptFormatting(text10);
						}
						if (flag18)
						{
							text10 = ColorUtility.CapitalizeExceptFormatting(text10);
						}
						if (num7 == -1)
						{
							array5[m] = text10;
						}
						else
						{
							array5[m] = text10 + array5[m].Substring(num7 + 1);
						}
					}
					Message.Clear();
					string[] array2 = array5;
					foreach (string value4 in array2)
					{
						Message.Append(value4);
					}
				}
			}
			if (Message.Contains("=MARKOVPARAGRAPH="))
			{
				Message.Replace("=MARKOVPARAGRAPH=", GenerateMarkovMessageParagraph());
				flag = true;
			}
			if (Message.Contains("=MARKOVSENTENCE="))
			{
				Message.Replace("=MARKOVSENTENCE=", GenerateMarkovMessageSentence());
				flag = true;
			}
			if (Message.Contains("=MARKOVCORVIDSENTENCE="))
			{
				Message.Replace("=MARKOVCORVIDSENTENCE=", TextFilters.Corvid(GenerateMarkovMessageSentence()));
				flag = true;
			}
			if (Message.Contains("=MARKOVWATERBIRDSENTENCE="))
			{
				Message.Replace("=MARKOVWATERBIRDSENTENCE=", TextFilters.WaterBird(GenerateMarkovMessageSentence()));
				flag = true;
			}
			if (Message.Contains("=MARKOVFISHSENTENCE="))
			{
				Message.Replace("=MARKOVFISHSENTENCE=", TextFilters.Fish(GenerateMarkovMessageSentence()));
				flag = true;
			}
			if (Message.Contains("=WEIRDMARKOVSENTENCE="))
			{
				Message.Replace("=WEIRDMARKOVSENTENCE=", Grammar.Weirdify(GenerateMarkovMessageSentence()));
				flag = true;
			}
			if (Message.Contains("=MarkOfDeath="))
			{
				Message.Replace("=MarkOfDeath=", The.Game?.GetStringGameState("MarkOfDeath") ?? "*MARK OF DEATH MISSING*");
			}
			if (The.Player != null)
			{
				Message.Replace("=player.subjective=", The.Player.it);
				Message.Replace("=player.Subjective=", The.Player.It);
				Message.Replace("=player.objective=", The.Player.them);
				Message.Replace("=player.Objective=", The.Player.Them);
				Message.Replace("=player.possessive=", The.Player.its);
				Message.Replace("=player.Possessive=", The.Player.Its);
				Message.Replace("=player.possessiveAdjective=", The.Player.its);
				Message.Replace("=player.PossessiveAdjective=", The.Player.Its);
				Message.Replace("=player.substantivePossessive=", The.Player.theirs);
				Message.Replace("=player.SubstantivePossessive=", The.Player.Theirs);
				Message.Replace("=player.reflexive=", The.Player.itself);
				Message.Replace("=player.Reflexive=", The.Player.Itself);
				Message.Replace("=player.personTerm=", The.Player.personTerm);
				Message.Replace("=player.PersonTerm=", The.Player.PersonTerm);
				Message.Replace("=player.immaturePersonTerm=", The.Player.immaturePersonTerm);
				Message.Replace("=player.ImmaturePersonTerm=", The.Player.ImmaturePersonTerm);
				Message.Replace("=player.formalAddressTerm=", The.Player.formalAddressTerm);
				Message.Replace("=player.FormalAddressTerm=", The.Player.FormalAddressTerm);
				Message.Replace("=player.offspringTerm=", The.Player.offspringTerm);
				Message.Replace("=player.OffspringTerm=", The.Player.OffspringTerm);
				Message.Replace("=player.siblingTerm=", The.Player.siblingTerm);
				Message.Replace("=player.SiblingTerm=", The.Player.SiblingTerm);
				Message.Replace("=player.parentTerm=", The.Player.parentTerm);
				Message.Replace("=player.ParentTerm=", The.Player.ParentTerm);
				if (Message.Contains("=player.species="))
				{
					Message.Replace("=player.species=", The.Player.GetSpecies());
				}
				if (Message.Contains("=player.Species="))
				{
					Message.Replace("=player.Species=", ColorUtility.CapitalizeExceptFormatting(The.Player.GetSpecies()));
				}
			}
			else
			{
				Message.Replace("=player.subjective=", "you");
				Message.Replace("=player.Subjective=", "You");
				Message.Replace("=player.objective", "you");
				Message.Replace("=player.Objective=", "You");
				Message.Replace("=player.possessive=", "your");
				Message.Replace("=player.Possessive=", "Your");
				Message.Replace("=player.possessiveAdjective=", "your");
				Message.Replace("=player.PossessiveAdjective=", "Your");
				Message.Replace("=player.substantivePossessive=", "yours");
				Message.Replace("=player.SubstantivePossessive=", "Yours");
				Message.Replace("=player.reflexive=", "yourself");
				Message.Replace("=player.Reflexive=", "Yourself");
				Message.Replace("=player.personTerm=", "person");
				Message.Replace("=player.PersonTerm=", "Person");
				Message.Replace("=player.immaturePersonTerm=", "child");
				Message.Replace("=player.ImmaturePersonTerm=", "Child");
				Message.Replace("=player.formalAddressTerm=", "friend");
				Message.Replace("=player.FormalAddressTerm=", "Friend");
				Message.Replace("=player.offspringTerm=", "child");
				Message.Replace("=player.OffspringTerm=", "Child");
				Message.Replace("=player.siblingTerm=", "sib");
				Message.Replace("=player.SiblingTerm=", "Sib");
				Message.Replace("=player.parentTerm=", "parent");
				Message.Replace("=player.ParentTerm=", "Parent");
				Message.Replace("=player.species=", "human");
				Message.Replace("=player.Species=", "Human");
			}
			if (Message.Contains("=subject"))
			{
				if (Subject != null)
				{
					if (Message.Contains("=subject.the="))
					{
						Message.Replace("=subject.the=", Subject.the);
					}
					if (Message.Contains("=subject.The="))
					{
						Message.Replace("=subject.The=", Subject.The);
					}
					if (Message.Contains("=subject.t="))
					{
						Message.Replace("=subject.t=", Subject.IsPlayer() ? "you" : (Subject.the + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.T="))
					{
						Message.Replace("=subject.T=", Subject.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Subject.The + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.t's="))
					{
						Message.Replace("=subject.t's=", Subject.IsPlayer() ? "your" : Grammar.MakePossessive(Subject.the + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.T's="))
					{
						Message.Replace("=subject.T's=", Subject.IsPlayer() ? "Your" : ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(Subject.The + Subject.ShortDisplayName)));
					}
					if (Message.Contains("=subject.a="))
					{
						Message.Replace("=subject.a=", Subject.IsPlayer() ? "you" : (Subject.a + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.A="))
					{
						Message.Replace("=subject.A=", Subject.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Subject.A + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.a's="))
					{
						Message.Replace("=subject.a's=", Subject.IsPlayer() ? "your" : Grammar.MakePossessive(Subject.a + Subject.ShortDisplayName));
					}
					if (Message.Contains("=subject.A's="))
					{
						Message.Replace("=subject.A's=", Subject.IsPlayer() ? "Your" : ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(Subject.A + Subject.ShortDisplayName)));
					}
					if (Message.Contains("=subject.name="))
					{
						Message.Replace("=subject.name=", Subject.IsPlayer() ? "you" : Subject.BaseDisplayNameStripped);
					}
					if (Message.Contains("=subject.Name="))
					{
						Message.Replace("=subject.Name=", Subject.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Subject.BaseDisplayNameStripped));
					}
					if (Message.Contains("=subject.name's="))
					{
						Message.Replace("=subject.name's=", Subject.IsPlayer() ? "your" : Grammar.MakePossessive(Subject.BaseDisplayNameStripped));
					}
					if (Message.Contains("=subject.Name's="))
					{
						Message.Replace("=subject.Name's=", Subject.IsPlayer() ? "Your" : ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(Subject.BaseDisplayNameStripped)));
					}
					if (Message.Contains("=subject.waterRitualLiquid="))
					{
						Message.Replace("=subject.waterRitualLiquid=", ColorUtility.StripFormatting(Subject.GetWaterRitualLiquidName()));
					}
					if (Message.Contains("=subject.WaterRitualLiquid="))
					{
						Message.Replace("=subject.WaterRitualLiquid=", ColorUtility.StripFormatting(ColorUtility.CapitalizeExceptFormatting(Subject.GetWaterRitualLiquidName())));
					}
					if (Message.Contains("=subject.direction="))
					{
						Message.Replace("=subject.direction=", The.Player?.DescribeDirectionToward(Subject) ?? "somewhere");
					}
					if (Message.Contains("=subject.Direction="))
					{
						Message.Replace("=subject.Direction=", ColorUtility.CapitalizeExceptFormatting(The.Player?.DescribeDirectionToward(Subject) ?? "Somewhere"));
					}
					if (Message.Contains("=subject.directionIfAny="))
					{
						string text11 = The.Player?.DescribeDirectionToward(Subject);
						if (!string.IsNullOrEmpty(text11) && text11 != "here")
						{
							Message.Replace("=subject.directionIfAny=", " " + text11);
						}
						else
						{
							Message.Replace("=subject.directionIfAny=", "");
						}
					}
					if (Message.Contains("=subject.DirectionIfAny="))
					{
						string text12 = The.Player?.DescribeDirectionToward(Subject);
						if (!string.IsNullOrEmpty(text12) && text12 != "here")
						{
							Message.Replace("=subject.DirectionIfAny=", " " + ColorUtility.CapitalizeExceptFormatting(text12));
						}
						else
						{
							Message.Replace("=subject.DirectionIfAny=", "");
						}
					}
					if (Message.Contains("=subject.species="))
					{
						Message.Replace("=subject.species=", Subject.GetSpecies());
					}
					if (Message.Contains("=subject.Species="))
					{
						Message.Replace("=subject.Species=", ColorUtility.CapitalizeExceptFormatting(Subject.GetSpecies()));
					}
				}
				else
				{
					if (Message.Contains("=subject.the="))
					{
						Message.Replace("=subject.the=", "the ");
					}
					if (Message.Contains("=subject.The="))
					{
						Message.Replace("=subject.The=", "The ");
					}
					if (ExplicitSubject == null)
					{
						if (Message.Contains("=subject.t="))
						{
							Message.Replace("=subject.t=", "the thing");
						}
						if (Message.Contains("=subject.T="))
						{
							Message.Replace("=subject.T=", "The thing");
						}
						if (Message.Contains("=subject.t's="))
						{
							Message.Replace("=subject.t's=", "the thing's");
						}
						if (Message.Contains("=subject.T's="))
						{
							Message.Replace("=subject.T's=", "The thing's");
						}
						if (Message.Contains("=subject.a="))
						{
							Message.Replace("=subject.a=", "a thing");
						}
						if (Message.Contains("=subject.A="))
						{
							Message.Replace("=subject.A=", "A thing");
						}
						if (Message.Contains("=subject.a's="))
						{
							Message.Replace("=subject.a's=", "a thing's");
						}
						if (Message.Contains("=subject.A's="))
						{
							Message.Replace("=subject.A's=", "A thing's");
						}
						if (Message.Contains("=subject.name="))
						{
							Message.Replace("=subject.name=", "thing");
						}
						if (Message.Contains("=subject.Name="))
						{
							Message.Replace("=subject.Name=", "Thing");
						}
						if (Message.Contains("=subject.name's="))
						{
							Message.Replace("=subject.name's=", "thing's");
						}
						if (Message.Contains("=subject.Name's="))
						{
							Message.Replace("=subject.Name's=", "Thing's");
						}
					}
					else
					{
						if (Message.Contains("=subject.t="))
						{
							Message.Replace("=subject.t=", "the " + ExplicitSubject);
						}
						if (Message.Contains("=subject.T="))
						{
							Message.Replace("=subject.T=", "The " + ExplicitSubject);
						}
						if (Message.Contains("=subject.t's="))
						{
							Message.Replace("=subject.t's=", Grammar.MakePossessive("the " + ExplicitSubject));
						}
						if (Message.Contains("=subject.T's="))
						{
							Message.Replace("=subject.T's=", Grammar.MakePossessive("The " + ExplicitSubject));
						}
						if (Message.Contains("=subject.a="))
						{
							Message.Replace("=subject.a=", Grammar.A(ExplicitSubject));
						}
						if (Message.Contains("=subject.A="))
						{
							Message.Replace("=subject.A=", Grammar.A(ExplicitSubject, capitalize: true));
						}
						if (Message.Contains("=subject.a's="))
						{
							Message.Replace("=subject.a's=", Grammar.MakePossessive(Grammar.A(ExplicitSubject)));
						}
						if (Message.Contains("=subject.A's="))
						{
							Message.Replace("=subject.A's=", Grammar.MakePossessive(Grammar.A(ExplicitSubject, capitalize: true)));
						}
						if (Message.Contains("=subject.name="))
						{
							Message.Replace("=subject.name=", ExplicitSubject);
						}
						if (Message.Contains("=subject.Name="))
						{
							Message.Replace("=subject.Name=", ColorUtility.CapitalizeExceptFormatting(ExplicitSubject));
						}
						if (Message.Contains("=subject.name's="))
						{
							Message.Replace("=subject.name's=", Grammar.MakePossessive(ExplicitSubject));
						}
						if (Message.Contains("=subject.Name's="))
						{
							Message.Replace("=subject.Name's=", ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ExplicitSubject)));
						}
					}
					if (Message.Contains("=subject.direction="))
					{
						Message.Replace("=subject.direction=", "somewhere");
					}
					if (Message.Contains("=subject.Direction="))
					{
						Message.Replace("=subject.Direction=", "Somewhere");
					}
					if (Message.Contains("=subject.directionIfAny="))
					{
						Message.Replace("=subject.direction=", "");
					}
					if (Message.Contains("=subject.DirectionIfAny="))
					{
						Message.Replace("=subject.Direction=", "");
					}
					if (Message.Contains("=subject.species="))
					{
						Message.Replace("=subject.species=", "creature");
					}
					if (Message.Contains("=subject.Species="))
					{
						Message.Replace("=subject.Species=", "Creature");
					}
				}
			}
			if (Message.Contains("=object"))
			{
				if (Object != null)
				{
					if (Message.Contains("=object.the="))
					{
						Message.Replace("=object.the=", Object.the);
					}
					if (Message.Contains("=object.The="))
					{
						Message.Replace("=object.The=", Object.The);
					}
					if (Message.Contains("=object.t="))
					{
						Message.Replace("=object.t=", Object.IsPlayer() ? "you" : (Object.the + Object.ShortDisplayName));
					}
					if (Message.Contains("=object.T="))
					{
						Message.Replace("=object.T=", Object.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Object.The + Object.ShortDisplayName));
					}
					if (Message.Contains("=object.a="))
					{
						Message.Replace("=object.a=", Object.IsPlayer() ? "you" : (Object.a + Object.ShortDisplayName));
					}
					if (Message.Contains("=object.A="))
					{
						Message.Replace("=object.A=", Object.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Object.A + Object.ShortDisplayName));
					}
					if (Message.Contains("=object.name="))
					{
						Message.Replace("=object.name=", Object.IsPlayer() ? "you" : Object.BaseDisplayNameStripped);
					}
					if (Message.Contains("=object.Name="))
					{
						Message.Replace("=object.Name=", Object.IsPlayer() ? "You" : ColorUtility.CapitalizeExceptFormatting(Object.BaseDisplayNameStripped));
					}
					if (Message.Contains("=object.name's="))
					{
						Message.Replace("=object.name's=", Object.IsPlayer() ? "your" : Grammar.MakePossessive(Object.BaseDisplayNameStripped));
					}
					if (Message.Contains("=object.Name's="))
					{
						Message.Replace("=object.Name's=", Object.IsPlayer() ? "Your" : ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(Object.BaseDisplayNameStripped)));
					}
					if (Message.Contains("=object.waterRitualLiquid="))
					{
						Message.Replace("=object.waterRitualLiquid=", ColorUtility.StripFormatting(Object.GetWaterRitualLiquid()));
					}
					if (Message.Contains("=object.WaterRitualLiquid="))
					{
						Message.Replace("=object.WaterRitualLiquid=", ColorUtility.StripFormatting(ColorUtility.CapitalizeExceptFormatting(Object.GetWaterRitualLiquid())));
					}
					if (Message.Contains("=object.direction="))
					{
						Message.Replace("=object.direction=", The.Player?.DescribeDirectionToward(Object) ?? "somewhere");
					}
					if (Message.Contains("=object.Direction="))
					{
						Message.Replace("=object.Direction=", ColorUtility.CapitalizeExceptFormatting(The.Player?.DescribeDirectionToward(Object) ?? "Somewhere"));
					}
					if (Message.Contains("=object.directionIfAny="))
					{
						string text13 = The.Player?.DescribeDirectionToward(Object);
						if (!string.IsNullOrEmpty(text13) && text13 != "here")
						{
							Message.Replace("=object.directionIfAny=", " " + text13);
						}
						else
						{
							Message.Replace("=object.directionIfAny=", "");
						}
					}
					if (Message.Contains("=object.DirectionIfAny="))
					{
						string text14 = The.Player?.DescribeDirectionToward(Object);
						if (!string.IsNullOrEmpty(text14) && text14 != "here")
						{
							Message.Replace("=object.DirectionIfAny=", " " + ColorUtility.CapitalizeExceptFormatting(text14));
						}
						else
						{
							Message.Replace("=object.DirectionIfAny=", "");
						}
					}
					if (Message.Contains("=object.species="))
					{
						Message.Replace("=object.species=", Object.GetSpecies());
					}
					if (Message.Contains("=object.Species="))
					{
						Message.Replace("=object.Species=", ColorUtility.CapitalizeExceptFormatting(Object.GetSpecies()));
					}
				}
				else
				{
					if (Message.Contains("=object.the="))
					{
						Message.Replace("=object.the=", "the ");
					}
					if (Message.Contains("=object.The="))
					{
						Message.Replace("=object.The=", "The ");
					}
					if (ExplicitObject == null)
					{
						if (Message.Contains("=object.t="))
						{
							Message.Replace("=object.t=", "the thing");
						}
						if (Message.Contains("=object.T="))
						{
							Message.Replace("=object.T=", "The thing");
						}
						if (Message.Contains("=object.a="))
						{
							Message.Replace("=object.a=", "a thing");
						}
						if (Message.Contains("=object.A="))
						{
							Message.Replace("=object.A=", "A thing");
						}
						if (Message.Contains("=object.name="))
						{
							Message.Replace("=object.name=", "thing");
						}
						if (Message.Contains("=object.Name="))
						{
							Message.Replace("=object.Name=", "Thing");
						}
						if (Message.Contains("=object.name's="))
						{
							Message.Replace("=object.name's=", "thing's");
						}
						if (Message.Contains("=object.Name's="))
						{
							Message.Replace("=object.Name's=", "Thing's");
						}
					}
					else
					{
						if (Message.Contains("=object.t="))
						{
							Message.Replace("=object.t=", "the " + ExplicitObject);
						}
						if (Message.Contains("=object.T="))
						{
							Message.Replace("=object.T=", "The " + ExplicitObject);
						}
						if (Message.Contains("=object.a="))
						{
							Message.Replace("=object.a=", Grammar.A(ExplicitObject));
						}
						if (Message.Contains("=object.A="))
						{
							Message.Replace("=object.A=", Grammar.A(ExplicitObject, capitalize: true));
						}
						if (Message.Contains("=object.name="))
						{
							Message.Replace("=object.name=", ExplicitObject);
						}
						if (Message.Contains("=object.Name="))
						{
							Message.Replace("=object.Name=", ColorUtility.CapitalizeExceptFormatting(ExplicitObject));
						}
						if (Message.Contains("=object.name's="))
						{
							Message.Replace("=object.name's=", Grammar.MakePossessive(ExplicitObject));
						}
						if (Message.Contains("=object.Name's="))
						{
							Message.Replace("=object.Name's=", ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ExplicitObject)));
						}
					}
					if (Message.Contains("=object.direction="))
					{
						Message.Replace("=object.direction=", "somewhere");
					}
					if (Message.Contains("=object.Direction="))
					{
						Message.Replace("=object.Direction=", "Somewhere");
					}
					if (Message.Contains("=object.directionIfAny="))
					{
						Message.Replace("=object.direction=", "");
					}
					if (Message.Contains("=object.DirectionIfAny="))
					{
						Message.Replace("=object.Direction=", "");
					}
					if (Message.Contains("=object.species="))
					{
						Message.Replace("=object.species=", "creature");
					}
					if (Message.Contains("=object.Species="))
					{
						Message.Replace("=object.Species=", "Creature");
					}
				}
			}
			if (Message.Contains("=pronouns"))
			{
				if (Subject != null)
				{
					if (Message.Contains("=pronouns.subjective="))
					{
						Message.Replace("=pronouns.subjective=", Subject.it);
					}
					if (Message.Contains("=pronouns.Subjective="))
					{
						Message.Replace("=pronouns.Subjective=", Subject.It);
					}
					if (Message.Contains("=pronouns.objective="))
					{
						Message.Replace("=pronouns.objective=", Subject.them);
					}
					if (Message.Contains("=pronouns.Objective="))
					{
						Message.Replace("=pronouns.Objective=", Subject.Them);
					}
					if (Message.Contains("=pronouns.possessive="))
					{
						Message.Replace("=pronouns.possessive=", Subject.its);
					}
					if (Message.Contains("=pronouns.Possessive="))
					{
						Message.Replace("=pronouns.Possessive=", Subject.Its);
					}
					if (Message.Contains("=pronouns.possessiveAdjective="))
					{
						Message.Replace("=pronouns.possessiveAdjective=", Subject.its);
					}
					if (Message.Contains("=pronouns.PossessiveAdjective="))
					{
						Message.Replace("=pronouns.PossessiveAdjective=", Subject.Its);
					}
					if (Message.Contains("=pronouns.substantivePossessive="))
					{
						Message.Replace("=pronouns.substantivePossessive=", Subject.theirs);
					}
					if (Message.Contains("=pronouns.SubstantivePossessive="))
					{
						Message.Replace("=pronouns.SubstantivePossessive=", Subject.Theirs);
					}
					if (Message.Contains("=pronouns.reflexive="))
					{
						Message.Replace("=pronouns.reflexive=", Subject.itself);
					}
					if (Message.Contains("=pronouns.Reflexive="))
					{
						Message.Replace("=pronouns.Reflexive=", Subject.Itself);
					}
					if (Message.Contains("=pronouns.personTerm="))
					{
						Message.Replace("=pronouns.personTerm=", Subject.personTerm);
					}
					if (Message.Contains("=pronouns.PersonTerm="))
					{
						Message.Replace("=pronouns.PersonTerm=", Subject.PersonTerm);
					}
					if (Message.Contains("=pronouns.immaturePersonTerm="))
					{
						Message.Replace("=pronouns.immaturePersonTerm=", Subject.immaturePersonTerm);
					}
					if (Message.Contains("=pronouns.ImmaturePersonTerm="))
					{
						Message.Replace("=pronouns.ImmaturePersonTerm=", Subject.ImmaturePersonTerm);
					}
					if (Message.Contains("=pronouns.formalAddressTerm="))
					{
						Message.Replace("=pronouns.formalAddressTerm=", Subject.formalAddressTerm);
					}
					if (Message.Contains("=pronouns.FormalAddressTerm="))
					{
						Message.Replace("=pronouns.FormalAddressTerm=", Subject.FormalAddressTerm);
					}
					if (Message.Contains("=pronouns.offspringTerm="))
					{
						Message.Replace("=pronouns.offspringTerm=", Subject.offspringTerm);
					}
					if (Message.Contains("=pronouns.OffspringTerm="))
					{
						Message.Replace("=pronouns.OffspringTerm=", Subject.OffspringTerm);
					}
					if (Message.Contains("=pronouns.siblingTerm="))
					{
						Message.Replace("=pronouns.siblingTerm=", Subject.siblingTerm);
					}
					if (Message.Contains("=pronouns.SiblingTerm="))
					{
						Message.Replace("=pronouns.SiblingTerm=", Subject.SiblingTerm);
					}
					if (Message.Contains("=pronouns.parentTerm="))
					{
						Message.Replace("=pronouns.parentTerm=", Subject.parentTerm);
					}
					if (Message.Contains("=pronouns.ParentTerm="))
					{
						Message.Replace("=pronouns.ParentTerm=", Subject.ParentTerm);
					}
					if (Message.Contains("=pronouns.indicativeProximal="))
					{
						Message.Replace("=pronouns.indicativeProximal=", Subject.indicativeProximal);
					}
					if (Message.Contains("=pronouns.IndicativeProximal="))
					{
						Message.Replace("=pronouns.IndicativeProximal=", Subject.IndicativeProximal);
					}
					if (Message.Contains("=pronouns.indicativeDistal="))
					{
						Message.Replace("=pronouns.indicativeDistal=", Subject.indicativeDistal);
					}
					if (Message.Contains("=pronouns.IndicativeDistal="))
					{
						Message.Replace("=pronouns.IndicativeDistal=", Subject.IndicativeDistal);
					}
				}
				else if (ExplicitSubject != null && ExplicitSubjectPlural)
				{
					Message.Replace("=pronouns.subjective=", "they");
					Message.Replace("=pronouns.Subjective=", "They");
					Message.Replace("=pronouns.objective=", "them");
					Message.Replace("=pronouns.Objective=", "Them");
					Message.Replace("=pronouns.possessive=", "their");
					Message.Replace("=pronouns.Possessive=", "Their");
					Message.Replace("=pronouns.possessiveAdjective=", "their");
					Message.Replace("=pronouns.PossessiveAdjective=", "Their");
					Message.Replace("=pronouns.substantivePossessive=", "theirs");
					Message.Replace("=pronouns.SubstantivePossessive=", "Theirs");
					Message.Replace("=pronouns.reflexive=", "themselves");
					Message.Replace("=pronouns.Reflexive=", "Themselves");
					Message.Replace("=pronouns.personTerm=", "humans");
					Message.Replace("=pronouns.PersonTerm=", "Humans");
					Message.Replace("=pronouns.immaturePersonTerm=", "children");
					Message.Replace("=pronouns.ImmaturePersonTerm=", "Children");
					Message.Replace("=pronouns.formalAddressTerm=", "friends");
					Message.Replace("=pronouns.FormalAddressTerm=", "Friends");
					Message.Replace("=pronouns.offspringTerm=", "children");
					Message.Replace("=pronouns.OffspringTerm=", "Children");
					Message.Replace("=pronouns.siblingTerm=", "sibs");
					Message.Replace("=pronouns.SiblingTerm=", "Sibs");
					Message.Replace("=pronouns.parentTerm=", "parents");
					Message.Replace("=pronouns.ParentTerm=", "Parents");
					Message.Replace("=pronouns.indicativeProximal=", "this");
					Message.Replace("=pronouns.IndicativeProximal=", "This");
					Message.Replace("=pronouns.indicativeDistal=", "that");
					Message.Replace("=pronouns.IndicativeDistal=", "That");
				}
				else
				{
					Message.Replace("=pronouns.subjective=", "it");
					Message.Replace("=pronouns.Subjective=", "It");
					Message.Replace("=pronouns.objective=", "it");
					Message.Replace("=pronouns.Objective=", "It");
					Message.Replace("=pronouns.possessive=", "its");
					Message.Replace("=pronouns.Possessive=", "Its");
					Message.Replace("=pronouns.possessiveAdjective=", "its");
					Message.Replace("=pronouns.PossessiveAdjective=", "Its");
					Message.Replace("=pronouns.substantivePossessive=", "its");
					Message.Replace("=pronouns.SubstantivePossessive=", "Its");
					Message.Replace("=pronouns.reflexive=", "itself");
					Message.Replace("=pronouns.Reflexive=", "Itself");
					Message.Replace("=pronouns.personTerm=", "human");
					Message.Replace("=pronouns.PersonTerm=", "Human");
					Message.Replace("=pronouns.immaturePersonTerm=", "child");
					Message.Replace("=pronouns.ImmaturePersonTerm=", "Child");
					Message.Replace("=pronouns.formalAddressTerm=", "friend");
					Message.Replace("=pronouns.FormalAddressTerm=", "Friend");
					Message.Replace("=pronouns.offspringTerm=", "child");
					Message.Replace("=pronouns.OffspringTerm=", "Child");
					Message.Replace("=pronouns.siblingTerm=", "sib");
					Message.Replace("=pronouns.SiblingTerm=", "Sib");
					Message.Replace("=pronouns.parentTerm=", "parent");
					Message.Replace("=pronouns.ParentTerm=", "Parent");
					Message.Replace("=pronouns.indicativeProximal=", "this");
					Message.Replace("=pronouns.IndicativeProximal=", "This");
					Message.Replace("=pronouns.indicativeDistal=", "that");
					Message.Replace("=pronouns.IndicativeDistal=", "That");
				}
			}
			if (Message.Contains("=objpronouns"))
			{
				if (Object != null)
				{
					if (Message.Contains("=objpronouns.subjective="))
					{
						Message.Replace("=objpronouns.subjective=", Object.it);
					}
					if (Message.Contains("=objpronouns.Subjective="))
					{
						Message.Replace("=objpronouns.Subjective=", Object.It);
					}
					if (Message.Contains("=objpronouns.objective="))
					{
						Message.Replace("=objpronouns.objective=", Object.them);
					}
					if (Message.Contains("=objpronouns.Objective="))
					{
						Message.Replace("=objpronouns.Objective=", Object.Them);
					}
					if (Message.Contains("=objpronouns.possessive="))
					{
						Message.Replace("=objpronouns.possessive=", Object.its);
					}
					if (Message.Contains("=objpronouns.Possessive="))
					{
						Message.Replace("=objpronouns.Possessive=", Object.Its);
					}
					if (Message.Contains("=objpronouns.possessiveAdjective="))
					{
						Message.Replace("=objpronouns.possessiveAdjective=", Object.its);
					}
					if (Message.Contains("=objpronouns.PossessiveAdjective="))
					{
						Message.Replace("=objpronouns.PossessiveAdjective=", Object.Its);
					}
					if (Message.Contains("=objpronouns.substantivePossessive="))
					{
						Message.Replace("=objpronouns.substantivePossessive=", Object.theirs);
					}
					if (Message.Contains("=objpronouns.SubstantivePossessive="))
					{
						Message.Replace("=objpronouns.SubstantivePossessive=", Object.Theirs);
					}
					if (Message.Contains("=objpronouns.reflexive="))
					{
						Message.Replace("=objpronouns.reflexive=", Object.itself);
					}
					if (Message.Contains("=objpronouns.Reflexive="))
					{
						Message.Replace("=objpronouns.Reflexive=", Object.Itself);
					}
					if (Message.Contains("=objpronouns.personTerm="))
					{
						Message.Replace("=objpronouns.personTerm=", Object.personTerm);
					}
					if (Message.Contains("=objpronouns.PersonTerm="))
					{
						Message.Replace("=objpronouns.PersonTerm=", Object.PersonTerm);
					}
					if (Message.Contains("=objpronouns.immaturePersonTerm="))
					{
						Message.Replace("=objpronouns.immaturePersonTerm=", Object.immaturePersonTerm);
					}
					if (Message.Contains("=objpronouns.ImmaturePersonTerm="))
					{
						Message.Replace("=objpronouns.ImmaturePersonTerm=", Object.ImmaturePersonTerm);
					}
					if (Message.Contains("=objpronouns.formalAddressTerm="))
					{
						Message.Replace("=objpronouns.formalAddressTerm=", Object.formalAddressTerm);
					}
					if (Message.Contains("=objpronouns.FormalAddressTerm="))
					{
						Message.Replace("=objpronouns.FormalAddressTerm=", Object.FormalAddressTerm);
					}
					if (Message.Contains("=objpronouns.offspringTerm="))
					{
						Message.Replace("=objpronouns.offspringTerm=", Object.offspringTerm);
					}
					if (Message.Contains("=objpronouns.OffspringTerm="))
					{
						Message.Replace("=objpronouns.OffspringTerm=", Object.OffspringTerm);
					}
					if (Message.Contains("=objpronouns.siblingTerm="))
					{
						Message.Replace("=objpronouns.siblingTerm=", Object.siblingTerm);
					}
					if (Message.Contains("=objpronouns.SiblingTerm="))
					{
						Message.Replace("=objpronouns.SiblingTerm=", Object.SiblingTerm);
					}
					if (Message.Contains("=objpronouns.parentTerm="))
					{
						Message.Replace("=objpronouns.parentTerm=", Object.parentTerm);
					}
					if (Message.Contains("=objpronouns.ParentTerm="))
					{
						Message.Replace("=objpronouns.ParentTerm=", Object.ParentTerm);
					}
					if (Message.Contains("=objpronouns.indicativeProximal="))
					{
						Message.Replace("=objpronouns.indicativeProximal=", Object.indicativeProximal);
					}
					if (Message.Contains("=objpronouns.IndicativeProximal="))
					{
						Message.Replace("=objpronouns.IndicativeProximal=", Object.IndicativeProximal);
					}
					if (Message.Contains("=objpronouns.indicativeDistal="))
					{
						Message.Replace("=objpronouns.indicativeDistal=", Object.indicativeDistal);
					}
					if (Message.Contains("=objpronouns.IndicativeDistal="))
					{
						Message.Replace("=objpronouns.IndicativeDistal=", Object.IndicativeDistal);
					}
				}
				else if (ExplicitObject != null && ExplicitObjectPlural)
				{
					Message.Replace("=objpronouns.subjective=", "they");
					Message.Replace("=objpronouns.Subjective=", "They");
					Message.Replace("=objpronouns.objective=", "them");
					Message.Replace("=objpronouns.Objective=", "Them");
					Message.Replace("=objpronouns.possessive=", "their");
					Message.Replace("=objpronouns.Possessive=", "Their");
					Message.Replace("=objpronouns.possessiveAdjective=", "their");
					Message.Replace("=objpronouns.PossessiveAdjective=", "Their");
					Message.Replace("=objpronouns.substantivePossessive=", "theirs");
					Message.Replace("=objpronouns.SubstantivePossessive=", "Theirs");
					Message.Replace("=objpronouns.reflexive=", "themselves");
					Message.Replace("=objpronouns.Reflexive=", "Themselves");
					Message.Replace("=objpronouns.personTerm=", "humans");
					Message.Replace("=objpronouns.PersonTerm=", "Humans");
					Message.Replace("=objpronouns.immaturePersonTerm=", "children");
					Message.Replace("=objpronouns.ImmaturePersonTerm=", "Children");
					Message.Replace("=objpronouns.formalAddressTerm=", "friends");
					Message.Replace("=objpronouns.FormalAddressTerm=", "Friends");
					Message.Replace("=objpronouns.offspringTerm=", "children");
					Message.Replace("=objpronouns.OffspringTerm=", "Children");
					Message.Replace("=objpronouns.siblingTerm=", "sibs");
					Message.Replace("=objpronouns.SiblingTerm=", "Sibs");
					Message.Replace("=objpronouns.parentTerm=", "parents");
					Message.Replace("=objpronouns.ParentTerm=", "Parents");
					Message.Replace("=objpronouns.indicativeProximal=", "this");
					Message.Replace("=objpronouns.IndicativeProximal=", "This");
					Message.Replace("=objpronouns.indicativeDistal=", "that");
					Message.Replace("=objpronouns.IndicativeDistal=", "That");
				}
				else
				{
					Message.Replace("=objpronouns.subjective=", "it");
					Message.Replace("=objpronouns.Subjective=", "It");
					Message.Replace("=objpronouns.objective=", "it");
					Message.Replace("=objpronouns.Objective=", "It");
					Message.Replace("=objpronouns.possessive=", "its");
					Message.Replace("=objpronouns.Possessive=", "Its");
					Message.Replace("=objpronouns.possessiveAdjective=", "its");
					Message.Replace("=objpronouns.PossessiveAdjective=", "Its");
					Message.Replace("=objpronouns.substantivePossessive=", "its");
					Message.Replace("=objpronouns.SubstantivePossessive=", "Its");
					Message.Replace("=objpronouns.reflexive=", "itself");
					Message.Replace("=objpronouns.Reflexive=", "Itself");
					Message.Replace("=objpronouns.personTerm=", "human");
					Message.Replace("=objpronouns.PersonTerm=", "Human");
					Message.Replace("=objpronouns.immaturePersonTerm=", "child");
					Message.Replace("=objpronouns.ImmaturePersonTerm=", "Child");
					Message.Replace("=objpronouns.formalAddressTerm=", "friend");
					Message.Replace("=objpronouns.FormalAddressTerm=", "Friend");
					Message.Replace("=objpronouns.offspringTerm=", "child");
					Message.Replace("=objpronouns.OffspringTerm=", "Child");
					Message.Replace("=objpronouns.siblingTerm=", "sib");
					Message.Replace("=objpronouns.SiblingTerm=", "Sib");
					Message.Replace("=objpronouns.parentTerm=", "parent");
					Message.Replace("=objpronouns.ParentTerm=", "Parent");
					Message.Replace("=objpronouns.indicativeProximal=", "this");
					Message.Replace("=objpronouns.IndicativeProximal=", "This");
					Message.Replace("=objpronouns.indicativeDistal=", "that");
					Message.Replace("=objpronouns.IndicativeDistal=", "That");
				}
			}
			if (Message.Contains("=verb:"))
			{
				string[] array7 = Message.ToString().Split(new string[1] { "=verb:" }, StringSplitOptions.None);
				for (int n = 1; n < array7.Length; n++)
				{
					int num8 = array7[n].IndexOf('=');
					string text15 = ((num8 != -1) ? array7[n].Substring(0, num8) : array7[n]);
					bool pronounAntecedent = false;
					if (text15.EndsWith(":afterpronoun"))
					{
						text15 = text15.Substring(0, text15.Length - ":afterpronoun".Length);
						pronounAntecedent = true;
					}
					if (Subject != null)
					{
						text15 = Subject.GetVerb(text15, PrependSpace: false, pronounAntecedent);
					}
					else if (ExplicitSubject != null && !ExplicitSubjectPlural)
					{
						text15 = Grammar.ThirdPerson(text15);
					}
					if (num8 == -1)
					{
						array7[n] = text15;
					}
					else
					{
						array7[n] = text15 + array7[n].Substring(num8 + 1);
					}
				}
				Message.Clear();
				string[] array2 = array7;
				foreach (string value5 in array2)
				{
					Message.Append(value5);
				}
			}
			if (Message.Contains("=objverb:"))
			{
				string[] array8 = Message.ToString().Split(new string[1] { "=objverb:" }, StringSplitOptions.None);
				for (int num9 = 1; num9 < array8.Length; num9++)
				{
					int num10 = array8[num9].IndexOf('=');
					string text16 = ((num10 != -1) ? array8[num9].Substring(0, num10) : array8[num9]);
					bool pronounAntecedent2 = false;
					if (text16.EndsWith(":afterpronoun"))
					{
						text16 = text16.Substring(0, text16.Length - ":afterpronoun".Length);
						pronounAntecedent2 = true;
					}
					if (Object != null)
					{
						text16 = Object.GetVerb(text16, PrependSpace: false, pronounAntecedent2);
					}
					else if (ExplicitSubject != null && !ExplicitSubjectPlural)
					{
						text16 = Grammar.ThirdPerson(text16);
					}
					if (num10 == -1)
					{
						array8[num9] = text16;
					}
					else
					{
						array8[num9] = text16 + array8[num9].Substring(num10 + 1);
					}
				}
				Message.Clear();
				string[] array2 = array8;
				foreach (string value6 in array2)
				{
					Message.Append(value6);
				}
			}
			if (Message.Contains("=article="))
			{
				string[] array9 = Message.ToString().Split(new string[1] { "=article=" }, StringSplitOptions.None);
				for (int num11 = 1; num11 < array9.Length; num11++)
				{
					int num12 = -1;
					for (int num13 = 0; num13 < array9[num11].Length; num13++)
					{
						char c = array9[num11][num13];
						if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9') && c != '-')
						{
							num12 = num13;
							break;
						}
					}
					string word = ((num12 != -1) ? array9[num11].Substring(0, num12) : array9[num11]);
					word = Grammar.A(word);
					if (num12 == -1)
					{
						array9[num11] = word;
					}
					else
					{
						array9[num11] = word + array9[num11].Substring(num12);
					}
				}
				Message.Clear();
				string[] array2 = array9;
				foreach (string value7 in array2)
				{
					Message.Append(value7);
				}
			}
			if (Message.Contains("=Article="))
			{
				string[] array10 = Message.ToString().Split(new string[1] { "=Article=" }, StringSplitOptions.None);
				for (int num14 = 1; num14 < array10.Length; num14++)
				{
					int num15 = -1;
					for (int num16 = 0; num16 < array10[num14].Length; num16++)
					{
						char c2 = array10[num14][num16];
						if ((c2 < 'a' || c2 > 'z') && (c2 < 'A' || c2 > 'Z') && (c2 < '0' || c2 > '9') && c2 != '-')
						{
							num15 = num16;
							break;
						}
					}
					string word2 = ((num15 != -1) ? array10[num14].Substring(0, num15) : array10[num14]);
					word2 = Grammar.A(word2, capitalize: true);
					if (num15 == -1)
					{
						array10[num14] = word2;
					}
					else
					{
						array10[num14] = word2 + array10[num14].Substring(num15);
					}
				}
				Message.Clear();
				string[] array2 = array10;
				foreach (string value8 in array2)
				{
					Message.Append(value8);
				}
			}
			if (Message.Contains("=pluralize="))
			{
				string[] array11 = Message.ToString().Split(new string[1] { "=pluralize=" }, StringSplitOptions.None);
				for (int num17 = 1; num17 < array11.Length; num17++)
				{
					int num18 = -1;
					for (int num19 = 0; num19 < array11[num17].Length; num19++)
					{
						char c3 = array11[num17][num19];
						if ((c3 < 'a' || c3 > 'z') && (c3 < 'A' || c3 > 'Z') && (c3 < '0' || c3 > '9') && c3 != '-')
						{
							num18 = num19;
							break;
						}
					}
					string word3 = ((num18 != -1) ? array11[num17].Substring(0, num18) : array11[num17]);
					word3 = Grammar.Pluralize(word3);
					if (num18 == -1)
					{
						array11[num17] = word3;
					}
					else
					{
						array11[num17] = word3 + array11[num17].Substring(num18);
					}
				}
				Message.Clear();
				string[] array2 = array11;
				foreach (string value9 in array2)
				{
					Message.Append(value9);
				}
			}
			if (Message.Contains("=pluralizeifplayerplural="))
			{
				string[] array12 = Message.ToString().Split(new string[1] { "=pluralizeifplayerplural=" }, StringSplitOptions.None);
				for (int num20 = 1; num20 < array12.Length; num20++)
				{
					int num21 = -1;
					for (int num22 = 0; num22 < array12[num20].Length; num22++)
					{
						char c4 = array12[num20][num22];
						if ((c4 < 'a' || c4 > 'z') && (c4 < 'A' || c4 > 'Z') && (c4 < '0' || c4 > '9') && c4 != '-')
						{
							num21 = num22;
							break;
						}
					}
					string text17 = ((num21 != -1) ? array12[num20].Substring(0, num21) : array12[num20]);
					if (The.Player != null && The.Player.IsPlural)
					{
						text17 = Grammar.Pluralize(text17);
					}
					if (num21 == -1)
					{
						array12[num20] = text17;
					}
					else
					{
						array12[num20] = text17 + array12[num20].Substring(num21);
					}
				}
				Message.Clear();
				string[] array2 = array12;
				foreach (string value10 in array2)
				{
					Message.Append(value10);
				}
			}
			if (Message.Contains("=ifplayerplural:"))
			{
				string[] array13 = Message.ToString().Split(new string[1] { "=ifplayerplural:" }, StringSplitOptions.None);
				for (int num23 = 1; num23 < array13.Length; num23++)
				{
					int num24 = array13[num23].IndexOf('=');
					string text18 = ((num24 != -1) ? array13[num23].Substring(0, num24) : array13[num23]);
					string[] array14 = text18.Split(':');
					text18 = ((The.Player != null && The.Player.IsPlural) ? array14[0] : ((array14.Length <= 1) ? "" : array14[1]));
					if (num24 == -1)
					{
						array13[num23] = text18;
					}
					else
					{
						array13[num23] = text18 + array13[num23].Substring(num24 + 1);
					}
				}
				Message.Clear();
				string[] array2 = array13;
				foreach (string value11 in array2)
				{
					Message.Append(value11);
				}
			}
			if (Message.Contains("=capitalize="))
			{
				string[] array15 = Message.ToString().Split(new string[1] { "=capitalize=" }, StringSplitOptions.None);
				for (int num25 = 1; num25 < array15.Length; num25++)
				{
					int num26 = -1;
					for (int num27 = 0; num27 < array15[num25].Length; num27++)
					{
						char c5 = array15[num25][num27];
						if ((c5 < 'a' || c5 > 'z') && (c5 < 'A' || c5 > 'Z') && (c5 < '0' || c5 > '9') && c5 != '-')
						{
							num26 = num27;
							break;
						}
					}
					string s = ((num26 != -1) ? array15[num25].Substring(0, num26) : array15[num25]);
					s = ColorUtility.CapitalizeExceptFormatting(s);
					if (num26 == -1)
					{
						array15[num25] = s;
					}
					else
					{
						array15[num25] = s + array15[num25].Substring(num26);
					}
				}
				Message.Clear();
				string[] array2 = array15;
				foreach (string value12 in array2)
				{
					Message.Append(value12);
				}
			}
			if (Message.Contains("=bodypart:"))
			{
				string[] array16 = Message.ToString().Split(new string[1] { "=bodypart:" }, StringSplitOptions.None);
				for (int num28 = 1; num28 < array16.Length; num28++)
				{
					int num29 = array16[num28].IndexOf('=');
					string preferType = ((num29 != -1) ? array16[num28].Substring(0, num29) : array16[num28]);
					string text19 = "body";
					if (Subject != null)
					{
						BodyPart randomConcreteBodyPart = Subject.GetRandomConcreteBodyPart(preferType);
						if (randomConcreteBodyPart != null)
						{
							text19 = randomConcreteBodyPart.GetOrdinalName();
						}
					}
					if (num29 == -1)
					{
						array16[num28] = text19;
					}
					else
					{
						array16[num28] = text19 + array16[num28].Substring(num29 + 1);
					}
				}
				Message.Clear();
				string[] array2 = array16;
				foreach (string value13 in array2)
				{
					Message.Append(value13);
				}
			}
			if (Message.Contains("=objbodypart:"))
			{
				string[] array17 = Message.ToString().Split(new string[1] { "=objbodypart:" }, StringSplitOptions.None);
				for (int num30 = 1; num30 < array17.Length; num30++)
				{
					int num31 = array17[num30].IndexOf('=');
					string preferType2 = ((num31 != -1) ? array17[num30].Substring(0, num31) : array17[num30]);
					string text20 = "body";
					if (Object != null)
					{
						BodyPart randomConcreteBodyPart2 = Object.GetRandomConcreteBodyPart(preferType2);
						if (randomConcreteBodyPart2 != null)
						{
							text20 = randomConcreteBodyPart2.GetOrdinalName();
						}
					}
					if (num31 == -1)
					{
						array17[num30] = text20;
					}
					else
					{
						array17[num30] = text20 + array17[num30].Substring(num31 + 1);
					}
				}
				Message.Clear();
				string[] array2 = array17;
				foreach (string value14 in array2)
				{
					Message.Append(value14);
				}
			}
		}
		if (flag)
		{
			Message = new TextBlock(Message, 70, 20).GetStringBuilder();
		}
		return Message.ToString();
	}

	public static string GenerateMarkovMessageParagraph()
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		return MarkovChain.GenerateParagraph(MarkovBook.CorpusData[text]);
	}

	public static string GenerateMarkovMessageSentence()
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		return MarkovChain.GenerateSentence(MarkovBook.CorpusData[text]);
	}

	public static string RoughConvertSecondPersonToThirdPerson(string text, GameObject who)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (!text.StartsWith("You were"))
		{
			text = ((!text.StartsWith("You")) ? (who.It + who.GetVerb("were", PrependSpace: true, PronounAntecedent: true) + " " + Grammar.InitLower(text)) : text.Replace("You", who.It));
		}
		else
		{
			string text2 = ((The.Player != null) ? ("by " + The.Player.ShortDisplayName + ".") : null);
			if (text2 != null && text.EndsWith(text2))
			{
				text = text.Substring(0, text.Length - text2.Length);
				text = ((!text.Contains(" to death ")) ? (text.Replace("You were", "You") + who.them + ".") : (text.Replace(" to death ", " ").Replace("You were", "You") + who.them + " to death."));
			}
			else
			{
				text = text.Replace("You were", who.It + who.GetVerb("were", PrependSpace: true, PronounAntecedent: true));
			}
		}
		if (text.Contains("yourself"))
		{
			text = text.Replace("yourself", who.itself);
		}
		if (text.Contains("yourselves"))
		{
			text = text.Replace("yourselves", who.itself);
		}
		if (text.Contains(" caused by "))
		{
			string text3 = ((The.Player != null) ? (" caused by " + The.Player.ShortDisplayName + ".") : null);
			if (text3 != null && text.EndsWith(text3))
			{
				text = text.Replace(text3, ", which you caused.");
			}
		}
		return text;
	}
}
