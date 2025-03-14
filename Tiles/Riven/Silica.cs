﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Silica : ModTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			// Sand specific properties
			Main.tileSand[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			TileID.Sets.Conversion.Sand[Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true; // Allows Sandshark enemies to "swim" in this sand.
			TileID.Sets.CanBeDugByShovel[Type] = true;
			TileID.Sets.Falling[Type] = true;
			TileID.Sets.Suffocate[Type] = true;
			TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ProjectileType<Silica_Ball_Falling>(), 10); // Tells which falling projectile to spawn when the tile should fall.

			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileID.Sets.ChecksForMerge[Type] = true;

			MineResist = 0.5f; // Sand tile typically require half as many hits to mine.
			AddMapEntry(new Color(194, 200, 200));
			DustType = DustID.Ghost;
		}

		public override bool HasWalkDust() {
			return Main.rand.NextBool(3);
		}

		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Bone;
		}
	}
	public class Silica_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ProjectileType<Silica_Ball_Gun>(), 10);
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.SandBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Silica>());
			Item.width = 12;
			Item.height = 12;
			Item.ammo = AmmoID.Sand;
			Item.notAmmo = true;
		}
	}
	public abstract class Silica_Ball : ModProjectile {
		public override string Texture => typeof(Silica_Ball).GetDefaultTMLName();

		public override void SetStaticDefaults() {
			ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Type] = true;
			ProjectileID.Sets.ForcePlateDetection[Type] = true;
		}
	}

	public class Silica_Ball_Falling : Silica_Ball {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(TileType<Silica>(), ItemType<Silica_Item>());
		}

		public override void SetDefaults() {
			// The falling projectile when compared to the sandgun projectile is hostile.
			Projectile.CloneDefaults(ProjectileID.PearlSandBallFalling);
		}
	}

	public class Silica_Ball_Gun : Silica_Ball {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(TileType<Silica>());
		}

		public override void SetDefaults() {
			// The sandgun projectile when compared to the falling projectile has a ranged damage type, isn't hostile, and has extraupdates = 1.
			// Note that EbonsandBallGun has infinite penetration, unlike SandBallGun
			Projectile.CloneDefaults(ProjectileID.PearlSandBallGun);
			AIType = ProjectileID.PearlSandBallGun; // This is needed for some logic in the ProjAIStyleID.FallingTile code.
		}
	}
}
