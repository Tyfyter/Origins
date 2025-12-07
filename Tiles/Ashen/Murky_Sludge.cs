using PegasusLib;
using System;
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
			DustType = DustID.Mud;
			HitSound = SoundID.NPCHit18;
			LateSetupActions.Add(() => {
				TileMergeOverlay mergeOverlay = new(merge + "Murk_Overlay", Type);
				for (int i = 0; i < TileLoader.TileCount; i++) {
					switch (i) {
						case TileID.Dirt:
						case TileID.Mud:
						case TileID.Ash:
						break;

						default:
						if (!TileID.Sets.Grass[i]) continue;
						break;
					}
					VanillaTileOverlays.AddOverlay(i, mergeOverlay);
				}
			});
		}
		public override void FloorVisuals(Player player) {
			player.AddBuff(BuffType<Murky_Sludge_Debuff>(), 2);
			if (player.gfxOffY < 4) MathUtils.LinearSmoothing(ref player.gfxOffY, 4, 4);
		}
		public override bool HasWalkDust() {
			return Main.rand.NextBool(3, 25);
		}
		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Snow;
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
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer oP = player.OriginPlayer();
			oP.moveSpeedMult *= 0.75f;
			oP.murkySludge = true;
			player.jump -= 8;
			//if (!oP.collidingY) player.buffTime[buffIndex] = 1;
		}
	}
}
