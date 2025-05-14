using CalamityMod.NPCs.TownNPCs;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Shimmer_Dynamite : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ThrownExplosive",
			"IsDynamite",
			"ExpendableWeapon"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.damage = 65;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.shoot = ModContent.ProjectileType<Shimmer_Dynamite_P>();
			Item.shootSpeed = 8f;
			Item.width = 8;
			Item.height = 28;
			Item.UseSound = SoundID.Item1;
			Item.consumable = false;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(platinum: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Dynamite)
			.AddIngredient(ItemID.BottomlessShimmerBucket)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
	public class Shimmer_Dynamite_P : ModProjectile {
		public override string Texture => typeof(Shimmer_Dynamite).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Shimmer_Dynamite)}.DisplayName");
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 1;
			ProjectileID.Sets.Explosive[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.timeLeft = 360;
			Projectile.friendly = false;
			DrawOriginOffsetY = -16;
		}
		public override void AI() {
			base.AI();
		}
		public override void PrepareBombToBlow() {
			Projectile.Resize(250, 250);
			Projectile.ai[1] = 0f;
			Projectile.friendly = true;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile.Hitbox, SoundID.Item14);
		}
		public override void OnKill(int timeLeft) {
			const int rad = 8;
			Vector2 center = Projectile.Center / 16;
			int x = (int)center.X;
			int y = (int)center.Y;
			int minI = x - rad;
			int maxI = x + rad;
			int minJ = y - rad;
			int maxJ = y + rad;
			for (int i = minI; i <= maxI; i++) {
				for (int j = minJ; j <= maxJ; j++) {
					float num = Math.Abs(i - center.X);
					float num2 = Math.Abs(j - center.Y);
					if (num * num + num2 * num2 >= rad * rad)
						continue;

					Tile tile = Main.tile[i, j];
					if (Main.tile[i, j] != null && tile.HasTile && Projectile.CanExplodeTile(i, j)) {
						void TransformToTile(int type) {
							if (type >= 0) {
								tile.TileType = (ushort)type;
								if (tile.TileType == TileID.Torches) {
									tile.TileFrameY = 23 * 22;
								}
								if (Main.netMode != NetmodeID.SinglePlayer) {
									NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
								}
							}
						}
						switch (tile.TileType) {
							case TileID.LunarBrick: {
								switch (Main.GetMoonPhase()) {
									case MoonPhase.Full:
									TransformToTile(TileID.HeavenforgeBrick);
									break;
									case MoonPhase.ThreeQuartersAtLeft:
									TransformToTile(TileID.LunarRustBrick);
									break;
									case MoonPhase.HalfAtLeft:
									TransformToTile(TileID.AstraBrick);
									break;
									case MoonPhase.QuarterAtLeft:
									TransformToTile(TileID.DarkCelestialBrick);
									break;
									case MoonPhase.Empty:
									TransformToTile(TileID.MercuryBrick);
									break;
									case MoonPhase.QuarterAtRight:
									TransformToTile(TileID.StarRoyaleBrick);
									break;
									case MoonPhase.HalfAtRight:
									TransformToTile(TileID.CryocoreBrick);
									break;
									case MoonPhase.ThreeQuartersAtRight:
									TransformToTile(TileID.CosmicEmberBrick);
									break;
								}
								break;
							}
							case TileID.ExposedGems:
							if (tile.TileFrameX >= 18) {
								tile.TileFrameX -= 18;
								if (Main.netMode != NetmodeID.SinglePlayer) {
									NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
								}
							}
							break;
							default:
							TransformToTile(OriginsSets.Tiles.ShimmerTransformToTile[tile.TileType]);
							break;
						}

					}
					/*for (int k = i - 1; k <= i + 1; k++) {
						for (int l = j - 1; l <= j + 1; l++) {
							if (Main.tile[k, l] != null && Main.tile[k, l].wall > 0 && wallSplode) {
								if (!WallLoader.CanExplode(k, l, Main.tile[k, l].wall)) {
									continue;
								}

								WorldGen.KillWall(k, l);
								if (Main.tile[k, l].wall == 0 && Main.netMode != 0)
									NetMessage.SendData(17, -1, -1, null, 2, k, l);
							}
						}
					}*/
				}
			}
			for (int i = minI; i <= maxI; i++) {
				for (int j = minJ; j <= maxJ; j++) {
					WorldGen.SquareTileFrame(i, j);
				}
			}
			//WorldGen.RangeFrame(minI - 1, maxI + 1, minJ - 1, maxJ + 1);
		}
	}
}
