using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Flashbang : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Slow];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 32;
			Item.crit += 6;
			Item.shootSpeed *= 1.75f;
			Item.shoot = ModContent.ProjectileType<Flashbang_P>();
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 15);
			Item.ArmorPenetration += 4;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 25)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.Grenade, 25)
			.Register();
		}
	}
	public class Flashbang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flashbang";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Hand_Grenade_Launcher.AltFireAction[Type] = (player, source, position, velocity, type, damage, knockback) => {
				Projectile.NewProjectileDirect(source, position, velocity * 0.6f, ModContent.ProjectileType<Flashbang_Sun>(), damage, knockback, player.whoAmI);
			};
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Flash_P>(), 0, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal) && blindGlobal.blindable) {
				target.AddBuff(Blind_Debuff.ID, 120);
			} else {
				target.AddBuff(BuffID.Confused, 220);
			}
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(BuffID.Darkness, 120);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Flashbang_Debuff.ID, 60);
		}
	}
	public class Flash_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flash";
		public override void SetDefaults() {
			Projectile.timeLeft = 25;
			Projectile.tileCollide = false;
			Projectile.alpha = 100;
			Projectile.hide = true;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overWiresUI.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			const float scale = 2f;
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			DrawData data = new(
				Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 1, 1),
				new Color(0, 0, 0, 255),
				0, new Vector2(0.5f, 0.5f),
				new Vector2(160, 160) * scale,
				SpriteEffects.None,
			0);
			float percent = Projectile.timeLeft / 10f;
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(0f + percent);
			Origins.blackHoleShade.UseColor(1, 1, 1);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
			return false;
		}
	}
	public class Flashbang_Sun : ModProjectile, IBroken {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flash";
		public static string BrokenReason => "needs balancing, make this less evil while still sticking to the 'RoR2 Grandparent sun attack', increase volume of explosion sound but keep pitch at -2.5f";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 136;
			Projectile.height = 136;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.timeLeft = 60;
			Projectile.ContinuouslyUpdateDamageStats = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overWiresUI.Add(index);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[1] == 0) Projectile.ai[1] = Projectile.scale;
			if (Main.myPlayer == Projectile.owner) {
				Projectile.position = player.MountedCenter - new Vector2(Projectile.width / 2, (Projectile.height * 1.5f) + player.gfxOffY);
				player.heldProj = Projectile.whoAmI;
				player.SetDummyItemTime(2);
				player.velocity.X *= 0.9f;
				player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(-Projectile.position.Y * player.direction, player.direction));

				if (player.channel && !player.noItems && !player.CCed) {
					Projectile.timeLeft++;
					Projectile.scale.Warmup(3 + Projectile.ai[1], 0.01f);
					if (Projectile.ai[0] >= 30) {
						bool HasLoS(Entity target) => CollisionExt.CanHitRay(Projectile.Center, target.Top) || CollisionExt.CanHitRay(Projectile.Center, target.Center) || CollisionExt.CanHitRay(Projectile.Center, target.Bottom);

						foreach (NPC target in Main.ActiveNPCs) {
							if (Projectile.scale + Projectile.ai[1] > 0.3f && target.Distance(Projectile.Center) <= 16 * 60 * (Projectile.scale * 0.25f) && HasLoS(target)) {
								target.AddBuff(BuffID.OnFire, 3 * 60);
								if (target.Distance(Projectile.Center) <= 16 * 30 * (Projectile.scale * 0.25f)) {
									if (target.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal) && blindGlobal.blindable) {
										target.AddBuff(Blind_Debuff.ID, 120);
									} else {
										target.AddBuff(BuffID.Confused, 220);
									}
								}
								target.AddBuff(BuffID.Slow, 250);
								if (Projectile.scale >= 2.5f + Projectile.ai[1]) target.AddBuff(BuffID.OnFire3, 3 * 60);
							}
						}

						foreach (Player target in Main.ActivePlayers) {
							if (Projectile.scale + Projectile.ai[1] > 0.3f) {
								if ((target.Distance(Projectile.Center) <= 16 * 60 * (Projectile.scale * 0.25f) && HasLoS(target)) || target == player) {
									target.AddBuff(BuffID.OnFire, 3 * 60);
									if (target.Distance(Projectile.Center) <= 16 * 30 * (Projectile.scale * 0.25f) && target != player) {
										target.AddBuff(Flashbang_Debuff.ID, 120); // think of a less mean/disruptive debuff
									}
									target.AddBuff(BuffID.Slow, 250);
									target.AddBuff(BuffID.Darkness, 120);
									if (Projectile.scale >= 2.5f + Projectile.ai[1]) target.AddBuff(BuffID.OnFire3, 3 * 60);
								}
							}
						}
					} else Projectile.ai[0]++;
				} else {
					if (Projectile.scale >= 0.3f + Projectile.ai[1]) Projectile.Kill();
					else Projectile.active = false;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, 3 * 60);
			if (Projectile.scale >= 2.5f + Projectile.ai[1]) {
				if (target.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal) && blindGlobal.blindable) {
					target.AddBuff(Blind_Debuff.ID, 120);
				} else {
					target.AddBuff(BuffID.Confused, 220);
				}
				target.AddBuff(BuffID.OnFire3, 5 * 60);
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire, 3 * 60);
			if (Projectile.scale >= 2.5f + Projectile.ai[1]) {
				target.AddBuff(Flashbang_Debuff.ID, 120); // think of a less mean/disruptive debuff
				target.AddBuff(BuffID.OnFire3, 5 * 60);
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(
				Projectile,
				(int)(256 * Projectile.scale),
				sound: SoundID.Item62 with { Pitch = -2.5f }, // I like the pitch but needs to be louder
				fireDustAmount: (int)(53 * (2 * Projectile.scale)),
				smokeDustAmount: (int)(58 * (2 * Projectile.scale)),
				smokeGoreAmount: (int)(6 * Projectile.scale),
				hostile: true, alsoFriendly: true);
		}
		public override bool PreDraw(ref Color lightColor) {
			float scale = 0.5f + Projectile.scale;
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			DrawData data = new(
				Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 1, 1),
				Color.Transparent,
				0, new Vector2(0.5f, 0.5f),
				new Vector2(160, 160) * scale,
				SpriteEffects.None,
			0);
			float percent = Projectile.ai[0] / 40f;
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(0f + percent);
			Color color = Color.Lerp(FromHexRGB(0xFFD951), FromHexRGB(0xFF8560), Utils.PingPongFrom01To010(Projectile.ai[2]));
			Projectile.ai[2] += 0.005f;
			Origins.blackHoleShade.UseColor(color);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.08f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
			return false;
		}
	}
}
