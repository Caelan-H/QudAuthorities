using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRL; // to abbreviate XRL.PlayerMutator and XRL.IPlayerMutator
using XRL.UI;
using XRL.World; // to abbreviate XRL.World.GameObject 
using XRL.Core; // for XRLCore

using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
/// <summary>
///             Contains all the caves of qud specific boot handler logic
///             </summary>
///             
namespace QudAuthorities.Mod
    {
        public class ReturnByDeathInitializer : AbstractEmbarkBuilderModule
    {
            public override void InitFromSeed(string seed)
            {
            }

            public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
            {

                if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING)
                {

                    The.Core.SaveGame("Return.sav");
                }
                return base.handleBootEvent(id, game, info, element);
            }
        }
    }


