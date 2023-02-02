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
	public class Defiled_Stone_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Stone[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			Origins.WallHammerRequirement[Type] = 70;
			AddMapEntry(new Color(150, 150, 150));
		}
	}
	public class Defiled_Stone_Wall_Safe : Defiled_Stone_Wall {
		public override string Texture => "Origins/Walls/Defiled_Stone_Wall";
		public override void SetStaticDefaults() {
			ItemDrop = ItemType<Defiled_Stone_Wall_Item>();
			Main.wallHouse[Type] = true;
			base.SetStaticDefaults();
		}
	}
	public class Defiled_Stone_Wall_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Stone Wall");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Defiled_Stone_Wall_Safe>();
		}
	}
}
