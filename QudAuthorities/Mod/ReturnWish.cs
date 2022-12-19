using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts.Mutation;

namespace QudAuthorities.Mod
{
    [HasWishCommand]
    public class ReturnWish
    {

        // Handles "testwish" with nothing else! (no string param)
        [WishCommand(Command = "Return")]
        public static void TestWishHandler()
        {
           // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(ReturnByDeath)),1);
            
        }

        [WishCommand(Command = "ReturnSave")]
        public static void SaveWishHandler()
        {
            // Popup.Show("Matched it the short way");
            ReturnByDeath.CopyZone();
            The.Core.SaveGame("Return.sav");

        }



    }

    }
