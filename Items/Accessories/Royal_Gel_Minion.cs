using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Royal_Gel_Global : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.RoyalGel;
		public override void SetDefaults(Item entity) {
			entity.damage = 11;
		}
		public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
			if (player.whoAmI != Main.myPlayer) return;
			int minionID = ModContent.ProjectileType<Spiked_Slime_Minion>();
			if (player.ownedProjectileCounts[minionID] <= 0) {
				Projectile.NewProjectile(player.GetSource_Accessory(item), player.MountedCenter, Vector2.Zero, minionID, player.GetWeaponDamage(item), player.GetWeaponKnockback(item), player.whoAmI);
			}
		}
	}
	public class Spiked_Slime_Minion : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.BabySlime];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BabySlime);
			Projectile.minionSlots = 0;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			AIType = ProjectileID.BabySlime;
		}
		public override void AI() {
			DrawOffsetX = -Projectile.width / 2;
			DrawOriginOffsetY = (int)(Projectile.height * -0.65f);
			if (Projectile.frame >= 2) {
				if (Projectile.ai[2] < 0) Projectile.ai[2]++;
				return;
			}
			//DrawOffsetX = -Projectile.width / 2;
			int npcTarget = Main.player[Projectile.owner].MinionAttackTargetNPC;
			if (npcTarget >= 0 && Projectile.ai[2] >= 0) {
				NPC target = Main.npc[npcTarget];
				const float dist = 8 * 16;
				if (target.position.Y <= Projectile.position.Y + Projectile.height && Projectile.DistanceSQ(target.Center) < dist * dist) {
					ShootSpikes();
				}
			}
			static bool IsPlatform(Tile tile) {
				return tile.HasTile && Main.tileSolidTop[tile.TileType];
			}
			if (Projectile.tileCollide
				&& Projectile.velocity.Y > 0
				&& (IsPlatform(Framing.GetTileSafely((Projectile.BottomLeft + Vector2.UnitY).ToTileCoordinates()))
				|| IsPlatform(Framing.GetTileSafely((Projectile.BottomRight + Vector2.UnitY).ToTileCoordinates())))
			) {
				Projectile.velocity.Y = 0;
				float tileEmbedpos = Projectile.position.Y + Projectile.height;
				if ((int)tileEmbedpos / 16 == (int)(tileEmbedpos + 1) / 16) {
					Projectile.position.Y -= tileEmbedpos % 16;
				}
			}
			if (Projectile.ai[2] >= 0) {
				if (Projectile.velocity.Y < -5.9f || Main.LocalPlayer.controlDown) {
					Projectile.ai[2] = 1;
				} else if (Projectile.ai[2] == 1 && Projectile.velocity.Y >= 0) {
					ShootSpikes();
				}
			} else Projectile.ai[2]++;
			if (Projectile.owner == Main.myPlayer && !Main.LocalPlayer.npcTypeNoAggro[1]) Projectile.Kill();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[2] >= 0) ShootSpikes();
		}
		public void ShootSpikes() {
			if (Projectile.owner == Main.myPlayer) {
				int spikeCount = Main.rand.Next(3, 7);
				for (int i = 0; i < spikeCount; i++) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						new Vector2(0, -5).RotatedBy(1 - (i / (float)(spikeCount - 1)) * 2),
						ModContent.ProjectileType<Spiked_Slime_Minion_Spike>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
				Projectile.ai[2] = -45;
			}
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;
			return true;
		}
		public override Color? GetAlpha(Color lightColor) {
			return lightColor * 0.8f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
	}
	public class Spiked_Slime_Minion_Spike : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.SpikedSlimeSpike];
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpikedSlimeSpike);
			Projectile.aiStyle = 1;
			Projectile.penetrate = -1;
			Projectile.npcProj = false;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			//AIType = ProjectileID.SpikedSlimeSpike;
		}
		public override void AI() {
			if (Projectile.ai[0] >= 5f) {
				Projectile.ai[0] = 5f;
				Projectile.velocity.Y += 0.15f;
			}

			Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity,
				Projectile.width,
				Projectile.height,
				DustID.TintableDust,
				0f,
				0f,
				50,
				new Color(43, 185, 255, 150),
				1.0f
			);
			dust.velocity *= 0.3f;
			dust.velocity += Projectile.velocity * 0.3f;
			dust.noGravity = true;
			Projectile.alpha -= 50;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
	}
}
