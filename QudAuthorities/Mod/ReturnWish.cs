using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
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

        [WishCommand(Command = "Gluttony")]
        public static void GluttonyWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if(mutations.HasMutation("Gluttony"))
            {
                AuthorityAwakeningGluttonyEvent.Send(The.Player);
               
            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Gluttony)), 1);
            }
            

        }

        [WishCommand(Command = "Greed")]
        public static void GreedWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Greed"))
            {
                AuthorityAwakeningGreedEvent.Send(The.Player);

            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Greed)), 1);
            }


        }

        [WishCommand(Command = "Lust")]
        public static void LustWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Lust"))
            {
                AuthorityAwakeningLustEvent.Send(The.Player);

            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Lust)), 1);
            }


        }

        [WishCommand(Command = "Sloth")]
        public static void SlothWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Sloth"))
            {
                AuthorityAwakeningSlothEvent.Send(The.Player);

            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Sloth)), 1);
            }


        }

        [WishCommand(Command = "Wrath")]
        public static void WrathWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Wrath"))
            {
                AuthorityAwakeningWrathEvent.Send(The.Player);

            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Wrath)), 1);
            }


        }

        [WishCommand(Command = "Pride")]
        public static void PrideWish()
        {
            // Popup.Show("Matched it the short way");
            XRL.World.Parts.Mutations mutations = The.Player.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Pride"))
            {
                AuthorityAwakeningPrideEvent.Send(The.Player);

            }
            else
            {
                mutations.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Pride)), 1);
            }


        }


    }

    }
