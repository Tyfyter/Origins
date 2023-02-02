using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Fiberglass_Wall : ModWall {
		public override void SetStaticDefaults() {
			AddMapEntry(new Color(38, 105, 144));
		}
	}
	public class Fiberglass_Wall_Safe : Defiled_Stone_Wall {
		public override string Texture => "Origins/Walls/Fiberglass_Wall";
		public override void SetStaticDefaults() {
			ItemDrop = ItemType<Fiberglass_Wall_Item>();
			Main.wallHouse[Type] = true;
			base.SetStaticDefaults();
		}
	}
	public class Fiberglass_Wall_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Wall");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GlassWall);
			Item.createWall = WallType<Fiberglass_Wall_Safe>();
		}
	}
}
