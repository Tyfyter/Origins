using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Misc;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Flare_Launcher : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetStaticDefaults() {
			Flare_Dummy_Canister onFireFlare = new(BuffID.OnFire);
			CanisterGlobalItem.RegisterCanister(ItemID.Flare, new(new(220, 15, 0), new(220, 15, 0), ammo: onFireFlare));
			CanisterGlobalItem.RegisterCanister(ItemID.BlueFlare, new(new(0, 80, 220), new(0, 80, 220), ammo: onFireFlare));
			CanisterGlobalItem.RegisterCanister(ItemID.SpelunkerFlare, new(new(220, 207, 0), new(220, 207, 0), ammo: new Spelunker_Flare_Dummy_Canister()));
			CanisterGlobalItem.RegisterCanister(ItemID.CursedFlare, new(new(109, 220, 0), new(109, 220, 0), ammo: new Flare_Dummy_Canister(BuffID.CursedInferno)));
			Rainbow_Flare_Dummy_Canister rainbow = new();
			rainbow.canisterType = CanisterGlobalItem.RegisterCanister(ItemID.RainbowFlare, new(Color.Black, Color.Black, ammo: rainbow));
			Shimmer_Flare_Dummy_Canister shimmer = new();
			shimmer.canisterType = CanisterGlobalItem.RegisterCanister(ItemID.ShimmerFlare, new(Color.Black, Color.Black, ammo: shimmer));
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Flare_Launcher_P>(24, 32, 14f, 44, 24);
			Item.knockBack = 2;
			Item.reuseDelay = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FlareGun)
			.AddIngredient(ItemID.IllegalGunParts)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.ShadowScales, 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2, 0);
		}
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			if (ammo.ammo == AmmoID.Flare) {
				return CanisterGlobalItem.ItemToCanisterID.ContainsKey(ammo.type);
			}
			return null;
		}
	}
	public class Flare_Dummy_Canister(int debuffType) : ICanisterAmmo {
		public CanisterData GetCanisterData => throw new NotImplementedException();
		public static void Initialize(Projectile projectile) {
			if (projectile.ai[0] == 0) {
				if (projectile.timeLeft == 300) {
					projectile.timeLeft = 3600;
					projectile.penetrate = -1;
				}
			} else if (projectile.ai[0] == 1) {
				projectile.ai[0] = 2;
			}
		}
		public void AI(Projectile projectile, bool child) {
			if (!child) Initialize(projectile);
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(debuffType, Main.rand.NextBool(3) ? 600 : 300);
		}
		public void OnKill(Projectile projectile, bool child) { }
	}
	public class Spelunker_Flare_Dummy_Canister : ICanisterAmmo {
		public CanisterData GetCanisterData => throw new NotImplementedException();
		public void AI(Projectile projectile, bool child) {
			if (!child) Flare_Dummy_Canister.Initialize(projectile);
			if (!Main.dedServ) {
				float range = Main.screenWidth + 30 * 16;
				if (Main.LocalPlayer.Center.DistanceSQ(projectile.Center) < range * range) {
					Main.instance.SpelunkerProjectileHelper.AddSpotToCheck(projectile.Center);
				}
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.OnFire, Main.rand.NextBool(3) ? 600 : 300);
		}
		public void OnKill(Projectile projectile, bool child) { }
	}
	public class Rainbow_Flare_Dummy_Canister : ICanisterAmmo {
		public CanisterData GetCanisterData => throw new NotImplementedException();
		public int canisterType = -1;
		readonly FrameCachedValue<Color> color = new(() => Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.6f % 1f, 1f, 0.5f));
		public void AI(Projectile projectile, bool child) {
			if (!child) Flare_Dummy_Canister.Initialize(projectile);
			if (!Main.dedServ) {
				CanisterData canisterData = CanisterGlobalItem.CanisterDatas[canisterType];
				Color color = this.color.GetValue();
				canisterData.OuterColor = color;
				canisterData.InnerColor = color;
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.OnFire, Main.rand.NextBool(3) ? 600 : 300);
		}
		public void OnKill(Projectile projectile, bool child) { }
	}
	public class Shimmer_Flare_Dummy_Canister : ICanisterAmmo {
		public CanisterData GetCanisterData => throw new NotImplementedException();
		public int canisterType = -1;
		readonly FrameCachedValue<Color> color = new(() => new(LiquidRenderer.GetShimmerBaseColor(0, 0)));
		public void AI(Projectile projectile, bool child) {
			if (!child) Flare_Dummy_Canister.Initialize(projectile);
			if (!Main.dedServ) {
				CanisterData canisterData = CanisterGlobalItem.CanisterDatas[canisterType];
				Color color = this.color.GetValue();
				canisterData.OuterColor = color;
				canisterData.InnerColor = color;
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.OnFire, Main.rand.NextBool(3) ? 600 : 300);
		}
		public void OnKill(Projectile projectile, bool child) { }
	}
	public class Flare_Launcher_P : ModProjectile, IIsExplodingProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Flare_Launcher_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Flare_Launcher_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 24;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 512 * 16;
			ID = Type;
		}
		//bool initialized = false;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 300;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.extraUpdates = 0;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.netImportant = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				this.DoGravity(0.06f);
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else {
				if (Projectile.ai[0] == 1) Projectile.timeLeft--;
				if (!CollisionExtensions.OverlapsAnyTiles(Projectile.Hitbox)) {
					Projectile.velocity = Vector2.UnitY;
					Projectile.tileCollide = true;
					Projectile.ai[0] = 0;
				}
			}
			if (Projectile.position.Y + Projectile.velocity.Y < 0) {
				Projectile.position.Y -= Projectile.velocity.Y;
			}
			Lighting.AddLight(Projectile.Center, Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterData.InnerColor.ToVector3());
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[0] = 1;
			Projectile.tileCollide = false;
			float len = oldVelocity.Length();
			Projectile.position += Projectile.velocity + oldVelocity * (8 / len);
			Projectile.velocity = oldVelocity;
			return false;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] == 0;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.ai[0] != 0 && Projectile.timeLeft > 0) modifiers.DisableKnockback();
		}
		public override bool PreKill(int timeLeft) {
			if (Projectile.ai[0] != 0) Projectile.velocity = default;
			return true;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 center = projectile.Center;
			Dust.NewDustPerfect(
				center - Projectile.velocity.SafeNormalize(default),
				ModContent.DustType<Flare_Dust>(),
				-projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1f),
				newColor: canisterData.InnerColor,
				Scale: 0.85f
			).noGravity = true;
			Main.EntitySpriteDraw(
				OuterTexture,
				center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				new Vector2(4, 6),
				projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(
				innerTexture,
				center - Main.screenPosition,
				null,
				canisterData.InnerColor * (lightColor.A / 255f),
				projectile.rotation,
				new Vector2(4, 6),
				projectile.scale,
				SpriteEffects.None
			);
			Flare_Launcher_Glow_P.DrawGlow(Projectile, lightColor.A / 255f, false);
		}
		public override void OnKill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner) {
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					default,
					Flare_Launcher_Glow_P.ID,
					1,
					1,
					Projectile.owner
				);
			}
		}
		public bool IsExploding() => Projectile.ai[2] == 1;
		public void DefaultExplosion(Projectile projectile, int fireDustType = DustID.Torch, int size = 96) {
			projectile.ai[2] = 1;
			CanisterGlobalProjectile.DefaultExplosion(projectile, false, fireDustType: -1, size: size);
			Color flareColor = Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterData.InnerColor;
			for (int i = 0; i < 20; i++) {
				Dust.NewDustPerfect(
					Projectile.Center,
					ModContent.DustType<Flare_Dust>(),
					Main.rand.NextVector2Circular(16, 16),
					newColor: flareColor,
					Scale: 2f
				);
			}
		}
	}
	public class Flare_Launcher_Glow_P : ModProjectile, ICanisterChildProjectile {
		public static int ID { get; private set; }
		public override string Texture => typeof(Flare_Launcher_P).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 512 * 16;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 11;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.netImportant = true;
			Projectile.ai[0] = -1;
		}
		public override void AI() {
			/*if (Projectile.ai[0] != -1) {
				int projIndex = Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]);
				if (Main.projectile.IndexInRange(projIndex)) UpdateGlowPos(Main.projectile[projIndex], Projectile);
			}*/
		}
		public static void UpdateGlowPos(Projectile flare, Projectile glow) {
			if (!flare.active || flare.type != Flare_Launcher_P.ID) {
				glow.ai[0] = -1;
				return;
			}
			if (flare.timeLeft > 2) glow.timeLeft = 11;
			glow.position = flare.Center;
		}
		public static void DrawGlow(Projectile projectile, float alpha, bool isAfterEffect) {
			CanisterData canisterData = projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile canister) ? canister.CanisterData : projectile.GetGlobalProjectile<CanisterChildGlobalProjectile>().CanisterData;
			if (canisterData is null) return;
			Vector2 center = projectile.Center;
			Rectangle screen = new((int)Main.Camera.ScaledPosition.X, (int)Main.Camera.ScaledPosition.Y, (int)Main.Camera.ScaledSize.X, (int)Main.Camera.ScaledSize.Y);
			Vector2 closest = screen.Contains(center) ? center : CollisionExtensions.GetCenterProjectedPoint(screen, center);
			Vector2 diff = closest - center;
			float offScreenDist = diff.Length();
			Color glowColor = canisterData.InnerColor * alpha;
			float boomFactor = 1f;
			int timeLeft = isAfterEffect ? projectile.timeLeft : projectile.timeLeft + 9;
			if (timeLeft < 10) {
				boomFactor = Math.Min(timeLeft / 10f, 1);
				if (canisterData.Ammo is ModItem) {
					boomFactor *= 1 + boomFactor;
				}
			}
			if (offScreenDist > 16) {
				if (!CollisionExt.CanHitRay(center + (diff / offScreenDist) * 64, closest)) return;
				glowColor.A = 0;
				float sqrt = MathF.Sqrt(offScreenDist / 32f);
				float iSqrt = MathF.Pow(1 / sqrt, 0.5f) * boomFactor;
				float colorFactor = MathF.Pow(iSqrt, 0.5f);
				if (offScreenDist < 90f) colorFactor *= (offScreenDist - 16) / 90f;
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[projectile.type].Value,
					closest - Main.screenPosition,
					null,
					glowColor * colorFactor,
					(center - closest).ToRotation(),
					new Vector2(45, 45),
					new Vector2(5 * iSqrt, 1 * colorFactor),
					SpriteEffects.None
				);
				return;
			}
			glowColor.A = 0;
			//boomFactor = MathF.Pow(boomFactor, 0.5f);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[projectile.type].Value,
				center - Main.screenPosition,
				null,
				glowColor * MathF.Pow(boomFactor, 0.5f),
				projectile.rotation,
				new Vector2(45, 45),
				projectile.scale * MathF.Pow(boomFactor, 1.5f),
				SpriteEffects.None
			);
		}
		public override bool PreKill(int timeLeft) => false;
		public override bool PreDraw(ref Color lightColor) {
			DrawGlow(Projectile, lightColor.A / 255f, true);
			return false;
		}
	}
}
