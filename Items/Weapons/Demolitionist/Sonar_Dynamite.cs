using AltLibrary.Common.Systems;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Tools;
using Origins.NPCs;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Sonar_Dynamite : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ThrownExplosive",
			"IsDynamite",
			"ExpendableWeapon"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 175;
			Item.shoot = ModContent.ProjectileType<Sonar_Dynamite_P>();
			Item.value = Item.sellPrice(silver: 25);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddRecipeGroup(RecipeGroups.CopperBars, 3)
			.AddIngredient(ItemID.Dynamite, 8)
			.AddIngredient<Alkaliphiliac_Tissue>(5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Sonar_Dynamite_P : ModProjectile {
		public override string Texture => typeof(Sonar_Dynamite).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Sonar_Dynamite)}.DisplayName");
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 1;
			ProjectileID.Sets.Explosive[Type] = true;
			ProjectileID.Sets.NeedsUUID[Type] = true;
			Hydrolantern_Force_Global.ProjectileTypes.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.width = Projectile.height = 14;
			Projectile.timeLeft = 3600;
			Projectile.friendly = false;
			Projectile.aiStyle = 0;
			//DrawOriginOffsetY = -16;
		}
		float rotationSpeed = 0;
		public override void AI() {
			Projectile.velocity *= Projectile.wet ? 0.96f : 1f;
			Projectile.ai[0]++;
			if (Projectile.wet) {
				float waveFactor = MathF.Sin(++Projectile.localAI[1] * 0.01f);
				Projectile.velocity.Y += waveFactor * 0.01f;
				Projectile.rotation += rotationSpeed;
				Projectile.rotation = MathF.Atan2(MathF.Sin(Projectile.rotation), MathF.Cos(Projectile.rotation));
				float upDiff = GeometryUtils.AngleDif(Projectile.rotation, 0, out int upDir);
				float downDiff = GeometryUtils.AngleDif(Projectile.rotation, MathHelper.Pi, out int downDir);
				if (upDiff > downDiff) {
					rotationSpeed += downDiff * 0.001f * downDir;
				} else {
					rotationSpeed += upDiff * 0.001f * upDir;
				}
				rotationSpeed *= 0.99f;
				rotationSpeed += Projectile.velocity.X * 0.0002f;
			} else {
				if (Projectile.ai[0] > 5f) {
					if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f) {
						Projectile.velocity.X *= 0.96f;

						if (Projectile.velocity.X > -0.01f && Projectile.velocity.X < 0.01f) {
							Projectile.velocity.X = 0f;
							Projectile.netUpdate = true;
						}
					}

					Projectile.velocity.Y += 0.2f;
				}

				rotationSpeed = Projectile.velocity.X * 0.1f;
				Projectile.rotation += rotationSpeed;
			}
			if (Projectile.numUpdates == -1 && Projectile.ai[0] >= 120) {
				if (++Projectile.ai[1] > 3) {
					Projectile.Kill();
					return;
				}
				SoundEngine.PlaySound(SoundID.Zombie82.WithPitch(-3).WithVolume(0.2f) with { MaxInstances = 0 }, Projectile.Center);
				SoundEngine.PlaySound(Origins.Sounds.DeepBoom with { MaxInstances = 0 }, Projectile.Center);
				if (Projectile.owner == Main.myPlayer) {
					Projectile.NewProjectileDirect(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<Sonar_Dynamite_Ping>(),
						Projectile.damage,
						0,
						ai0: Projectile.identity
					);
				}
				Projectile.ai[0] = 0;
			}
			DrawOriginOffsetY = -5;
			Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.4f);
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 300, sound: SoundID.Item14);
			const int rad = 7;
			Vector2 center = Projectile.Center;
			int i = (int)(center.X / 16);
			int j = (int)(center.Y / 16);
			Projectile.ExplodeTiles(
				center,
				rad,
				i - rad,
				i + rad,
				j - rad,
				j + rad,
				Projectile.ShouldWallExplode(center, rad, i - rad, i + rad, j - rad, j + rad)
			);
		}
	}
	public class Sonar_Dynamite_Ping : ModProjectile {
		public override string Texture => typeof(Sonar_Dynamite).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Projectile parent = null;
			if (Projectile.ai[0] >= 0 && Projectile.owner < OriginSystem.projectilesByOwnerAndID.GetLength(0) && Projectile.ai[0] < OriginSystem.projectilesByOwnerAndID.GetLength(1)) {
				parent = OriginSystem.projectilesByOwnerAndID[Projectile.owner, (int)Projectile.ai[0]];
			}
			if (parent?.ModProjectile is not Sonar_Dynamite_P || !parent.active) Projectile.ai[0] = -1;
			else Projectile.position = parent.Center;
			if (Projectile.ai[1] > 1) Projectile.Kill();
			Projectile.ai[1] += 1 / 30f;
			if (parent is null) return;
			float range = 224.5f * Projectile.ai[1];
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy(Projectile) && Projectile.Center.Clamp(npc.Hitbox).WithinRange(Projectile.Center, range)) {
					if (Projectile.IsLocallyOwned()) parent.timeLeft = 5;
					npc.GetGlobalNPC<OriginGlobalNPC>().sonarDynamiteTime = 5;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			float scale = 2 * Projectile.ai[1];
			DrawData data = new(TextureAssets.Extra[27].Value, Projectile.position - Main.screenPosition, null, Color.Cyan * (1 - Projectile.ai[1]), 0, new Vector2(9.5f, 49), scale, SpriteEffects.None);
			for (int i = 0; i < 4; i++) {
				Main.EntitySpriteDraw(data);
				data.rotation += MathHelper.PiOver2;
			}
			return false;
		}
	}
}
