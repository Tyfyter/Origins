using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public class OriginInstancedGlobalNPC : GlobalNPC {
        public override bool CloneNewInstances => true;
        public override bool InstancePerEntity => true;
        //internal int shrapnelCount = 0;
    }
}
