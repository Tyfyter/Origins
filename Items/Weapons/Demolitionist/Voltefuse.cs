using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Voltefuse : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 54;
			Item.shoot = ModContent.ProjectileType<Voltefuse_P>();
			Item.shootSpeed *= 1.5f;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.Dynamite, 8)
			.AddIngredient<Aetherite_Bar>()
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Voltefuse_P : ModProjectile, IIsExplodingProjectile {public override string Texture => typeof(Voltefuse).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Voltefuse)}.DisplayName");
		static int FuseTime => 240;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 1;
			ProjectileID.Sets.TrailCacheLength[Type] = FuseTime;
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.timeLeft = FuseTime;
			Projectile.friendly = false;
			AIType = ProjectileID.Grenade;
			DrawOriginOffsetY = -16;
		}
		public override bool ShouldUpdatePosition() => Projectile.localAI[1] <= 0;
		public override void AI() {
			if (Projectile.localAI[1] <= 0) {
				Projectile.numUpdates++;
				Projectile.timeLeft = FuseTime;
				Projectile.oldPos[(int)Projectile.localAI[0]] = Projectile.position;
				Projectile.oldRot[(int)Projectile.localAI[0]] = Projectile.rotation;
				Projectile.localAI[0]++;
				Projectile.ai[2] = -1;
				if (Projectile.localAI[0] >= FuseTime || (Projectile.localAI[2] > 0 && --Projectile.localAI[2] <= 0)) {
					Projectile.localAI[1]++;
					Vector2 direction = Vector2.UnitY * 6;
					Projectile.ai[2] = Projectile.SpawnProjectile(null,
						Projectile.Center,
						direction.RotatedBy(Projectile.rotation),
						ModContent.ProjectileType<Voltefuse_Star>(),
						Projectile.damage,
						Projectile.knockBack
					)?.identity ?? 0;
					Projectile.SpawnProjectile(null,
						Projectile.Center,
						direction.RotatedBy(Projectile.rotation + MathHelper.TwoPi / 3),
						ModContent.ProjectileType<Voltefuse_Star>(),
						Projectile.damage,
						Projectile.knockBack
					);
					Projectile.SpawnProjectile(null,
						Projectile.Center,
						direction.RotatedBy(Projectile.rotation - MathHelper.TwoPi / 3),
						ModContent.ProjectileType<Voltefuse_Star>(),
						Projectile.damage,
						Projectile.knockBack
					);
					Projectile.netUpdate = true;
				}
			} else {
				if (Projectile.GetRelatedProjectile_Depreciated(2)?.active ?? false) {
					Projectile.timeLeft = FuseTime;
					Projectile.localAI[1] = 1;
					return;
				}
				Projectile.ai[2] = -1;
				AIType = ProjectileID.None;
				Projectile.rotation = Projectile.oldRot[(int)(Projectile.localAI[0] - Projectile.localAI[1])];
				Projectile.position = Projectile.oldPos[(int)(Projectile.localAI[0] - Projectile.localAI[1])];
				if (++Projectile.localAI[1] > Projectile.localAI[0]) Projectile.Kill();
			}
		}

		public bool IsExploding() => Projectile.localAI[0] >= FuseTime;
		public void Explode(int delay = 0) {
			if (Projectile.localAI[2] <= 0) Projectile.localAI[2] = 1 + delay;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.localAI[1] <= 1) lightColor *= 0.2f;
			return lightColor;
		}
	}
	public class Voltefuse_Star : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
		static int MaxTime => 120;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = MaxTime;
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[1] <= 0;
		public override void AI() {
			Projectile.scale = 1 / (Projectile.ai[0] + 1);
			int size = (int)(48 * Projectile.scale);
			if (Projectile.width != size) Projectile.Resize(size, size);
			if (Projectile.ai[1] <= 0) {
				Projectile.numUpdates++;
				Projectile.timeLeft = MaxTime;
				Projectile.oldPos[(int)Projectile.localAI[0]] = Projectile.position;
				Projectile.localAI[0]++;
				Projectile.velocity *= MathF.Pow(0.998f, Projectile.localAI[0] + Projectile.ai[0]);
				Projectile.ai[2] = -1;
				if (Projectile.localAI[0] >= MaxTime || Projectile.velocity.LengthSquared() < 1) {
					Projectile.ai[1]++;
					if (Projectile.ai[0] < 2) {
						Projectile.rotation = Projectile.velocity.ToRotation();
						Vector2 direction = Vector2.UnitY * (6 - (Projectile.ai[0] + 1) * 2);
						Projectile.ai[2] = Projectile.SpawnProjectile(null,
							Projectile.Center,
							direction.RotatedBy(Projectile.rotation),
							ModContent.ProjectileType<Voltefuse_Star>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.ai[0] + 1
						)?.identity ?? 0;
						Projectile.SpawnProjectile(null,
							Projectile.Center,
							direction.RotatedBy(Projectile.rotation + MathHelper.TwoPi / 3),
							ModContent.ProjectileType<Voltefuse_Star>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.ai[0] + 1
						);
						Projectile.SpawnProjectile(null,
							Projectile.Center,
							direction.RotatedBy(Projectile.rotation - MathHelper.TwoPi / 3),
							ModContent.ProjectileType<Voltefuse_Star>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.ai[0] + 1
						);
						Projectile.netUpdate = true;
					}
				}
			} else {
				if (Projectile.GetRelatedProjectile_Depreciated(2)?.active ?? false) {
					Projectile.timeLeft = MaxTime;
					Projectile.ai[1] = 1;
					return;
				}
				Projectile.ai[2] = -1;
				Projectile.rotation = 0;
				Projectile.hide = false;
				Projectile.friendly = true;
				Projectile.position = Projectile.oldPos[(int)(Projectile.localAI[0] - Projectile.ai[1])];
				if (++Projectile.ai[1] > Projectile.localAI[0]) Projectile.Kill();
				const int HalfSpriteWidth = 54 / 2;
				const int HalfSpriteHeight = 54 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;

				// Vanilla configuration for "hitbox in middle of sprite"
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			}
		}
		public override Color? GetAlpha(Color lightColor) => new(1f, 1f, 1f, 0.9f);
	}
}
