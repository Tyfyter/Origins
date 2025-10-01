using AltLibrary.Common.AltBiomes;
using MonoMod.Cil;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	[ReinitializeDuringResizeArrays]
	public class Chambersite_Stone_Wall : ModWall {
		public static Dictionary<ushort, ushort> AddChambersite = [];
		public static List<int> chambersiteWalls = [];
		public static int[] wallCounts = WallID.Sets.Factory.CreateIntSet();
		public override void Unload() {
			AddChambersite = null;
		}
		public override void Load() {
			try {
				IL_SceneMetrics.ScanAndExportToMain += il => {
					ILCursor c = new(il);
					int loc = -1;
					c.GotoNext(MoveType.AfterLabel,
						i => i.MatchLdloca(out loc),
						i => i.MatchCall<Tile>("active"),
						i => i.MatchBrtrue(out _)
					);
					c.EmitLdloc(loc);
					c.EmitDelegate((Tile tile) => {
						wallCounts[tile.WallType]++;
					});
				};
			} catch (Exception ex) {
				if (Origins.LogLoadingILError($"{nameof(Chambersite_Stone_Wall)}.CountWalls", ex)) throw;
			}
			//IL_00c6: ldloca.s 7
			//IL_00c8: call instance bool Terraria.Tile::active()
			//IL_00cd: brtrue.s IL_00f9
		}

		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.Stone));
			RegisterItemDrop(ItemType<Chambersite_Item>());
			DustType = DustID.GemEmerald;
			//AddChambersite.Add(WallID.Stone, Type);
			chambersiteWalls.Add(Type);
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
			Main.wallBlend[Type] = WallID.CrimstoneEcho;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.CrimstoneUnsafe));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CrimsonAltBiome>());
			RegisterItemDrop(ItemType<Chambersite_Item>());
			Chambersite_Stone_Wall.AddChambersite.Add(WallID.CrimstoneUnsafe, Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
	public class Chambersite_Ebonstone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.EbonstoneEcho;//what wall type this wall is considered to be when blending
			AddMapEntry(GetWallMapColor(WallID.EbonstoneUnsafe));
			Chambersite_Stone_Wall.AddChild(Type, GetInstance<CorruptionAltBiome>());
			RegisterItemDrop(ItemType<Chambersite_Item>());
			Chambersite_Stone_Wall.AddChambersite.Add(WallID.EbonstoneUnsafe, Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
	public class Chambersite_Defiled_Stone_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallType<Defiled_Stone_Wall>();//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(150, 150, 150));
			RegisterItemDrop(ItemType<Chambersite_Item>());
			Chambersite_Stone_Wall.AddChambersite.Add((ushort)WallType<Defiled_Stone_Wall>(), Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
	public class Chambersite_Riven_Flesh_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallType<Riven_Flesh_Wall>();//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(40, 140, 200));
			RegisterItemDrop(ItemType<Chambersite_Item>());
			Chambersite_Stone_Wall.AddChambersite.Add((ushort)WallType<Riven_Flesh_Wall>(), Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
	public class Chambersite_Tainted_Stone_Wall : ModWall {
		public override string Texture => typeof(Chambersite_Defiled_Stone_Wall).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallType<Tainted_Stone_Wall>();//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(255, 150, 150));
			RegisterItemDrop(ItemType<Chambersite_Item>());
			Chambersite_Stone_Wall.AddChambersite.Add((ushort)WallType<Tainted_Stone_Wall>(), Type);
			DustType = DustID.GemEmerald;
			Chambersite_Stone_Wall.chambersiteWalls.Add(Type);
		}
	}
}
