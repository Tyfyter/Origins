using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Murky_Sludge : ComplexFrameTile, IAshenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x2c212a));
			DustType = Ashen_Biome.DefaultTileDust;
			HitSound = SoundID.Item167.WithPitchOffset(-2f);
		}
		protected override IEnumerable<TileOverlay> GetOverlays() { // make these tiles instead have murky overlay (i think it'd look best
			yield return new TileMergeOverlay(merge + "Dirt_Overlay", TileID.Dirt);
			yield return new TileMergeOverlay(merge + "Mud_Overlay", TileID.Mud);
			yield return new TileMergeOverlay(merge + "Ash_Overlay", TileID.Ash);
		}
		public override void FloorVisuals(Player player) {
			player.AddBuff(BuffType<Murky_Sludge_Debuff>(), 2);
		}
		public override bool HasWalkDust() {
			return Main.rand.NextBool(3, 25);
		}
		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Sand;
			color = FromHexRGB(0x2c212a);
		}
	}
	public class Murky_Sludge_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Murky_Sludge>());
		}
		public override void AddRecipes() {
			CreateRecipe(10)
			.AddIngredient(ItemID.Gel, 10)
			.AddIngredient(ItemID.AshBlock, 5)
			.AddIngredient(ItemID.MudBlock, 5)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
		}
	}
	public class Murky_Sludge_Debuff : ModBuff {
		public override string Texture => base.Texture.Replace("Debuff", "Item");
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer oP = player.OriginPlayer();
			oP.moveSpeedMult *= 0.75f;
			player.jumpSpeedBoost -= 3.5f;
			if (!oP.collidingY) player.buffTime[buffIndex] = 1;
		}
	}
}
