using Origins.NPCs.Critters;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Ashen_Medium_Foliage : ModTile {
		public override string Texture => typeof(Defiled_Medium_Foliage).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);

			int[] validTiles = [
				ModContent.TileType<Ashen_Grass>(),
				ModContent.TileType<Ashen_Jungle_Grass>(),
				ModContent.TileType<Ashen_Murky_Sludge_Grass>()
			];

			TileObjectData.newTile.AnchorValidTiles = [..validTiles,
				TileID.Stone,
				TileID.Grass
			];

			TileObjectData.newTile.RandomStyleRange = 3;

			TileObjectData.addTile(Type);
			//soundType = SoundID.Grass;
			
			PileConversionGlobal.AddConversion(TileID.SmallPiles, [100, 101, 102, 103, 104, 105], Type, [..validTiles]);
		}

		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int critterType = ModContent.NPCType<Bug>();
			int chance = 12, _ = 0;
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
}
