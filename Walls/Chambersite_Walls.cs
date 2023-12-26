using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Chambersite_Stone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.Stone));
			RegisterItemDrop(ItemType<Chambersite_Item>());
		}
		public static void AddChild(int type, AltBiome biome) {
			biome.AddWallConversions(type, WallType<Chambersite_Stone_Wall>());
		}
	}
	public class Chambersite_Stone_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall((ushort)WallType<Chambersite_Stone_Wall>());
		}
	}
	public class Chambersite_Crimstone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.CrimsonUnsafe1;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.CrimsonUnsafe1));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CrimsonAltBiome>());
			RegisterItemDrop(ItemType<Chambersite_Item>());
		}
	}
	public class Chambersite_Ebonstone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.CorruptionUnsafe1;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.CorruptionUnsafe1));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CorruptionAltBiome>());
			RegisterItemDrop(ItemType<Chambersite_Item>());
		}
	}
	public class Chambersite_Defiled_Stone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallType<Defiled_Stone_Wall>();//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(150, 150, 150));
			RegisterItemDrop(ItemType<Chambersite_Item>());
		}
	}
	public class Chambersite_Riven_Flesh_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallType<Riven_Flesh_Wall>();//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(40, 140, 200));
			RegisterItemDrop(ItemType<Chambersite_Item>());
		}
	}
}
