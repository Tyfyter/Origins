﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Banner_NPC : ModNPC {
		public override string Texture => "Terraria/Images/Item_1";
		public override void Load() => this.AddBanner();
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.Generic.Defiled_Antibody");
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
	}
}
