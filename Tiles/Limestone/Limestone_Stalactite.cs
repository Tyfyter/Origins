using Origins.NPCs.Critters;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Questing.Peat_Moss_Quest;

namespace Origins.Tiles.Limestone {
	public class Limestone_Stalactite : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new(203, 194, 149));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.CoordinateHeights = new int[3] {
				16,
				16,
				16
			};

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.Origin = new();
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 1;
			TileObjectData.addTile(Type);

			DustType = DustID.Sand;
		}
	}
	public class Limestone_Stalagmite : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new(203, 194, 149));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.CoordinateHeights = new int[3] {
				16,
				16,
				16
			};

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 1;
			TileObjectData.addTile(Type);

			DustType = DustID.Sand;
		}
		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int critterType = ModContent.NPCType<Hyrax>();
			int chance = 150, _ = 0;
			TileLoader.DropCritterChance(i, j, Type, ref _, ref chance, ref _);
			if (chance > 0 && NPC.CountNPCS(critterType) < 5 && WorldGen.genRand.NextBool(chance)) {
				NPC npc = NPC.NewNPCDirect(new EntitySource_TileBreak(i, j), i * 16 + 10, j * 16, critterType);
				npc.TargetClosest();
				npc.velocity.Y = WorldGen.genRand.Next(-50, -21) * 0.1f;
				npc.velocity.X = WorldGen.genRand.Next(0, 26) * -0.1f * npc.direction;
				npc.direction *= -1;
				npc.netUpdate = true;
			}
			yield break;
		}
	}
	public class Limestone_Stalactite_Fake : Limestone_Stalactite {
		public override string Texture => base.Texture[..^"_Fake".Length];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			FlexibleTileWand.RubblePlacementMedium.SetupRubblemakerClone<Limestone_Item>(this, 0, 1, 2);
		}
	}
	public class Limestone_Stalagmite_Fake : Limestone_Stalagmite {
		public override string Texture => base.Texture[..^"_Fake".Length];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			FlexibleTileWand.RubblePlacementMedium.SetupRubblemakerClone<Limestone_Item>(this, 0, 1, 2);
		}
		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			return [ new Item(ModContent.ItemType<Limestone_Item>()) ];
		}
	}
}
