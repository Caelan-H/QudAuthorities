using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.Wish;

[HasWishCommand]
public static class MutationBGoneWishHandler
{
	[WishCommand(null, null, Command = "mutationbgone")]
	public static bool MutationBGone()
	{
		Mutations mutations = GetMutations();
		List<BaseMutation> mutationList = mutations.MutationList;
		if (mutationList.Count > 0)
		{
			int num = Popup.ShowOptionList("Choose a mutation for me to gobble up!", mutationList.ConvertAll((BaseMutation mutation) => mutation.DisplayName).ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num != -1)
			{
				RemoveMutation(mutations, mutationList[num]);
			}
		}
		else
		{
			Popup.Show("Huh? Get some mutations first if you want me to eat them!");
		}
		return true;
	}

	[WishCommand(null, null, Command = "mutationbgone")]
	public static bool MutationBGone(string argument)
	{
		Mutations part = XRLCore.Core.Game.Player.Body.GetPart<Mutations>();
		BaseMutation mutation = part.GetMutation(argument);
		if (mutation == null)
		{
			Popup.Show("Didn't find that one. Try again?");
		}
		else
		{
			RemoveMutation(part, mutation);
		}
		return true;
	}

	public static Mutations GetMutations()
	{
		return XRLCore.Core.Game.Player.Body.GetPart<Mutations>();
	}

	public static void RemoveMutation(Mutations mutations, BaseMutation mutation)
	{
		mutations.RemoveMutation(mutation);
		Popup.Show("Om nom nom! " + mutation.DisplayName + " is gone! {{w|*belch*}}");
	}
}
