using Origins.Dev;
using Origins.Projectiles;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Shimmer_Dynamite : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ThrownExplosive,
			WikiCategories.IsDynamite,
			WikiCategories.ExpendableWeapon
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Shimmer];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 220;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.shoot = ModContent.ProjectileType<Shimmer_Dynamite_P>();
			Item.shootSpeed *= 2f;
			Item.width = 8;
			Item.height = 28;
			Item.UseSound = SoundID.Item1;
			Item.consumable = false;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(platinum: 1);
			Item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottomlessShimmerBucket)
			.AddIngredient(ItemID.Dynamite)
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
			Projectile.timeLeft = 180;
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
			if (Main.player[Projectile.owner].OriginPlayer().pincushion) return;
			if (!Projectile.IsLocallyOwned()) return;
			new Shimmer_Dynamite_Action(Projectile.Center / 16).Perform();
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.ModifyHitInfo += (ref NPC.HitInfo info) => {
				if (NPCID.Sets.ShimmerTransformToNPC[target.type] >= 0 || NPCID.Sets.ShimmerTransformToItem[target.type] >= 0) {
					if (info.Damage >= target.life) {
						target.shimmerTransparency = 1;
						info.Damage = target.life - 1;
					}
				}
			};
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Shimmer, 91);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.Knockback *= 0;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.Shimmer, 60);
		}
	}
	public record class Shimmer_Dynamite_Action(Vector2 Center) : SyncedAction {
		public Shimmer_Dynamite_Action() : this(default(Vector2)) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Center = reader.ReadPackedVector2()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.WritePackedVector2(Center);
		}
		protected override void Perform() {
			const int rad = 8;
			int x = (int)Center.X;
			int y = (int)Center.Y;
			int minI = x - rad;
			int maxI = x + rad;
			int minJ = y - rad;
			int maxJ = y + rad;
			for (int i = minI; i <= maxI; i++) {
				for (int j = minJ; j <= maxJ; j++) {
					float num = Math.Abs(i - Center.X);
					float num2 = Math.Abs(j - Center.Y);
					if (num * num + num2 * num2 >= rad * rad)
						continue;

					Tile tile = Main.tile[i, j];
					if (Main.tile[i, j] != null) {
						if (tile.HasTile) {
							void TransformToTile(int type) {
								if (type >= 0) {
									tile.TileType = (ushort)type;
									if (tile.TileType == TileID.Torches) {
										tile.TileFrameY = 23 * 22;
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
								}
								break;

								default:
								TransformToTile(OriginsSets.Tiles.ShimmerTransformToTile[tile.TileType]);
								break;
							}
						}
						switch (tile.LiquidType) {
							case LiquidID.Water:
							tile.LiquidType = LiquidID.Lava;
							break;
							case LiquidID.Lava:
							tile.LiquidType = LiquidID.Honey;
							break;
							case LiquidID.Honey:
							tile.LiquidType = LiquidID.Water;
							break;
						}
					}
				}
			}
			for (int i = minI; i <= maxI; i++) {
				for (int j = minJ; j <= maxJ; j++) {
					WorldGen.SquareTileFrame(i, j);
				}
			}
		}
	}
}
