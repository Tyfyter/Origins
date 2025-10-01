using Origins.NPCs.Critters;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Ashen_Large_Foliage : ModTile {
		public override string Texture => typeof(Defiled_Large_Foliage).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(255, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);

			int[] validTiles = [
				ModContent.TileType<Ashen_Grass>(),
				ModContent.TileType<Ashen_Jungle_Grass>()
			];

			TileObjectData.newTile.AnchorValidTiles = [..validTiles,
				TileID.Stone,
				TileID.Grass
			];

			TileObjectData.addTile(Type);

			PileConversionGlobal.AddConversion(TileID.LargePiles, [7, 8, 9, 10, 11, 12], Type, [..validTiles]);
			//soundType = SoundID.Grass;
		}
		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int critterType = ModContent.NPCType<Bug>();
			int chance = 6, _ = 0;
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
