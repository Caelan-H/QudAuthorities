using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Capabilities;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;

using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.Core;
using XRL.CharacterBuilds.Qud;
using System.Runtime.CompilerServices;
using QudAuthorities.Mod;
using Qud.API;



namespace XRL.World
{
    [Serializable]
    [HasGameBasedStaticCache]
    public class Checkpointer : BaseMutation
    {
        public void CopyFolder()
        {
            The.Game.GetCacheDirectory("Return.sav");
            //The.Game.Get
        }
    }
}