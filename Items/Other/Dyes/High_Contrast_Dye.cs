using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Dyes {
	public class High_Contrast_Dye : Dye_Item {
		public override bool UseShaderOnSelf => false;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("High-Contrast Dye");
			SacrificeTotal = 3;
		}
	}
}
