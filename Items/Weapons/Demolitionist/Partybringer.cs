using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Partybringer : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Partybringer_P>(14, 50, 8f, 46, 28, true);
			Item.value = Item.sellPrice(silver: 24);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 1;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, -8f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (Main.rand.Next(4)) {
				default:
				type = Item.shoot;
				break;
				case 1:
				type = ModContent.ProjectileType<Partybringer_P1>();
				break;
				case 2:
				type = ModContent.ProjectileType<Partybringer_P2>();
				break;
				case 3:
				type = ModContent.ProjectileType<Partybringer_P3>();
				break;
			}
			Vector2 offset = velocity.SafeNormalize(default);
			position += offset.RotatedBy(player.direction * -MathHelper.PiOver2) * 6 - offset * 8;
			position += offset * (CollisionExt.Raymarch(position, offset, 32) - 8);
			position -= velocity;
		}
	}
	public abstract class Partybringer_P_Base : ModProjectile {
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.tileCollide = true;
			Projectile.width = Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.02f * (MathF.Pow((++Projectile.ai[0]) / 30, 0.5f) + 1);
			Projectile.rotation += Projectile.direction * 0.15f;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(139, 143), Projectile.velocity.X, Projectile.velocity.Y, 0, default(Color), 1.2f);
				dust.velocity.X += Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.Y += Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.X *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.Y *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.X += Main.rand.Next(-50, 51) * 0.05f;
				dust.velocity.Y += Main.rand.Next(-50, 51) * 0.05f;
				dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
			}

			for (int i = 0; i < 15; i++) {
				Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Main.rand.Next(276, 283));
				gore.velocity.X += Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.Y += Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.X *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.Y *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				gore.scale *= 1f + Main.rand.Next(-20, 21) * 0.01f;
				gore.velocity.X += Main.rand.Next(-50, 51) * 0.05f;
				gore.velocity.Y += Main.rand.Next(-50, 51) * 0.05f;
			}
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
		}
	}
	public class Partybringer_P : Partybringer_P_Base, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
	}
	public class Partybringer_P1 : Partybringer_P_Base {
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Main.rand.NextBool()) {

			} else {
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Partybringer_Fog>(),
					Projectile.damage / 3,
					1,
					Projectile.owner
				);
			}
		}
	}
	public class Partybringer_Fog : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 96;
			Projectile.timeLeft = 200;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 100;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				for (int i = 0; i < 100; i++) {
					float scale = 4f + Main.rand.NextFloat() * 0.8f;
					Vector2 velocity = Main.rand.NextVector2Circular(0.25f, 0.25f) * 0.2f + Main.rand.NextVector2CircularEdge(0.4f, 0.4f) * 0.2f;
					velocity *= 4f;
					Gore.NewGorePerfect(Projectile.GetSource_FromThis(), Projectile.Center + velocity * 4, velocity, ModContent.GoreType<Partybringer_Fog_Gore>(), scale);
				}
				Projectile.ai[0] = 1;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Blind_Debuff>(), 100);
		}
	}
	public class Partybringer_Fog_Gore : ModGore {
		public override string Texture => typeof(Partybringer_Fog).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			GoreID.Sets.SpecialAI[Type] = 5;
		}
		public override bool Update(Gore gore) {
			if (gore.timeLeft > 60) {
				if (gore.alpha > 240 && Main.rand.NextBool()) gore.alpha--;
			} else if (Main.rand.NextBool()) {
				gore.alpha++;
			}
			return true;
		}
	}
	public class Partybringer_P2 : Partybringer_P_Base {
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Main.rand.NextBool()) {
				if (Projectile.owner == Main.myPlayer) {
					int i = Main.rand.Next(1, 4);
					bool forceStar = i == 1;
					for (; i-->0;) {
						int item = Item.NewItem(
							Projectile.GetSource_Death(),
							Projectile.Center,
							ItemID.Heart
						);
						if (Main.netMode == NetmodeID.MultiplayerClient) {
							NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
						}
					}
					for (i = Main.rand.Next(forceStar ? 1 : 0, 4); i-->0;) {
						int item = Item.NewItem(
							Projectile.GetSource_Death(),
							Projectile.Center,
							ItemID.Star
						);
						if (Main.netMode == NetmodeID.MultiplayerClient) {
							NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
						}
					}
				}
			} else {
				if (Projectile.owner == Main.myPlayer) {
					int type = ModContent.ProjectileType<Bang_Snap_P>();
					int count = Main.rand.Next(4, 8);
					float rot = MathHelper.TwoPi / count;
					for (int i = count; i-- > 0;) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							GeometryUtils.Vec2FromPolar(8, rot * i + Main.rand.NextFloat(-0.1f, 0.1f)) + Main.rand.NextVector2Unit(),
							type,
							Projectile.damage / 5,
							6,
							Projectile.owner
						);
					}
				}
			}
		}
	}
	public class Partybringer_P3 : Partybringer_P_Base {
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Main.rand.NextBool()) {

			} else {
				const float speed = 8f;
				const float maxDist = 120 * 120;
				List<(float angle, float weight)> targets = [];
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.CanBeChasedBy(Projectile)) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Projectile.Center);
						Vector2 diff = currentPos - Projectile.Center;
						if (GeometryUtils.AngleToTarget(currentPos - Projectile.Center, speed, 0.16f) is not float angle) continue;
						float dist = diff.LengthSquared();
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (Projectile.damage / 2 > npc.life + npc.defense) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (angle, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((angle, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						GeometryUtils.Vec2FromPolar(speed, targets[i].angle),
						ProjectileID.PartyGirlGrenade,
						Projectile.damage / 2,
						10
					);
				}
			}
		}
	}
}
