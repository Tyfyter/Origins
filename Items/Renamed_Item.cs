using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items {
	public abstract class Renamed_Item<TNewType> : ModItem where TNewType : ModItem {
		public override string Texture => "ModLoader/UnloadedItem";
		public override void SetDefaults() {
			if(!Main.gameMenu) Item.SetDefaults(ModContent.ItemType<TNewType>());
		}
	}
}
