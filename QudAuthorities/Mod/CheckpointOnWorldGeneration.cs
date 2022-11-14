using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRL;
using XRL.UI;
using XRL.World.WorldBuilders;

namespace QudAuthorities.Mod
{
    //The game code instantiates an instance of this class during the JoppaWorld generation process
    [JoppaWorldBuilderExtension]
    public class CheckpointOnWorldGeneration : IJoppaWorldBuilderExtension
    {
        public override void OnBeforeBuild(JoppaWorldBuilder builder)
        {
            //The game calls this method before JoppaWorld generation takes place. JoppaWorld generation includes the creation of lairs, historic ruins, villages, and more.
        }

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            //The.Core.SaveGame("Return.sav");
            
        }
    }






   
    }
