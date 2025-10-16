using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

using Origins.Dev;
using PegasusLib;
namespace Origins.Items.Weapons.Magic {
    public class Manasynk : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.OtherMagic
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.maxStack = 1;
			Item.damage = 8;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.mana = 7;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.ammo = AmmoID.None;
			Item.shoot = ModContent.ProjectileType<Manasynk_P>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) => false;
		public override bool? CanBeChosenAsAmmo(Item weapon, Player player) {
			return weapon.useAmmo == AmmoID.Snowball && player.CheckMana(Item, pay: false);
		}
		public override bool CanBeConsumedAsAmmo(Item weapon, Player player) {
			player.CheckMana(Item, pay: true);
			player.manaRegenDelay = (int)player.maxRegenDelay;
			return false;
		}
	}
	public class Manasynk_P : ModProjectile {
		PolarVec2 embedPos;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RubyBolt);
			Projectile.timeLeft = 180;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 10;
			Projectile.hide = false;
			Projectile.alpha = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
		}
		public override void AI() {
			if (Projectile.ai[0] >= 0) {
				NPC embedTarget = Main.npc[(int)Projectile.ai[0]];
				if (embedTarget.active) {
					Projectile.Center = embedTarget.Center + (Vector2)embedPos.RotatedBy(embedTarget.rotation);
				} else {
					Projectile.ai[1] *= 2f; //double stolen mana if target dies
					Projectile.Kill();
				}
				if (++Projectile.frameCounter >= 4) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 3) {
						Projectile.frame = 0;
					}
				}
			} else {
				if (Projectile.timeLeft < 140) Projectile.velocity.Y += 0.04f;
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (++Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= Main.projFrames[Type]) {
						Projectile.frame = 0;
					}
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			return Projectile.ai[0] >= 0 ? target.whoAmI == (int)Projectile.ai[0] : null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			const float manaStealFactor = 0.5f;
			if (target.CanBeChasedBy()) {
				if (Projectile.ai[0] < 0) {
					Projectile.ai[0] = target.whoAmI;
					Projectile.damage /= 3;
					Projectile.damage += Projectile.ArmorPenetration;
					Projectile.ArmorPenetration = (target.defense + 2);
					Projectile.knockBack = 0;
					Projectile.velocity = Vector2.Zero;
					embedPos = ((PolarVec2)(Projectile.Center - target.Center)).RotatedBy(-target.rotation);
					Projectile.netUpdate = true;
				}
				Projectile.ai[1] += damageDone * manaStealFactor;
				CombatText.NewText(Projectile.Hitbox, new Color(150, 65, 200), (int)(Projectile.ai[1] * manaStealFactor));
			} else {
				Projectile.Kill();
			}
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.ai[1] > 0) {
				if (Projectile.owner == Main.myPlayer) {
					int item = Item.NewItem(
						Projectile.GetSource_Death(),
						Projectile.Center,
						ModContent.ItemType<Manasynk_Pickup>(),
						(int)Projectile.ai[1]
					);
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
				NPC embedTarget = Main.npc[(int)Projectile.ai[0]];
				ParticleOrchestrator.RequestParticleSpawn(
					false,
					ParticleOrchestraType.RainbowRodHit,
					new ParticleOrchestraSettings() {
						PositionInWorld = Projectile.Center
					}
				);
				for (int i = 0; i < 6 * 2; i++) {
					Terraria.Graphics.Renderers.IParticle iParticle = Main.ParticleSystem_World_OverPlayers.Particles[^(i + 1)];
					if (iParticle is Terraria.Graphics.Renderers.PrettySparkleParticle sparkleParticle) {
						float strength = (sparkleParticle.ColorTint.R / 255f + sparkleParticle.ColorTint.G / 255f + sparkleParticle.ColorTint.B / 255f) / 3;
						sparkleParticle.ColorTint = new Color(new Vector3(0.169f, 0.725f, 1f) * strength);
						sparkleParticle.Velocity += embedTarget.velocity;
						sparkleParticle.AccelerationPerFrame -= embedTarget.velocity / 60;
					}
				}
			} else {
				ParticleOrchestrator.RequestParticleSpawn(
					false,
					ParticleOrchestraType.RainbowRodHit,
					new ParticleOrchestraSettings() {
						PositionInWorld = Projectile.Center
					}
				);
				Vector2 velocity = Projectile.oldVelocity * 0.25f;
				for (int i = 0; i < 6 * 2; i++) {
					Terraria.Graphics.Renderers.IParticle iParticle = Main.ParticleSystem_World_OverPlayers.Particles[^(i + 1)];
					if (iParticle is Terraria.Graphics.Renderers.PrettySparkleParticle sparkleParticle) {
						float strength = (sparkleParticle.ColorTint.R / 255f + sparkleParticle.ColorTint.G / 255f + sparkleParticle.ColorTint.B / 255f) / 3;
						sparkleParticle.ColorTint = new Color(new Vector3(0.169f, 0.725f, 1f) * strength);
						sparkleParticle.Velocity += velocity;
						sparkleParticle.AccelerationPerFrame -= velocity / 60;
					}
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				new Vector2(36, 6),
				Projectile.scale,
				Projectile.direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None,
			0);
			return false;
		}
	}
	public class Manasynk_Pickup : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.IsAPickup[Type] = true;
		}
		public override void SetDefaults() {
			Item.maxStack = 9999;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			gravity *= 0.5f;
			Lighting.AddLight(Item.Center, 0.169f, 0.725f, 1f);
		}
		public override bool ItemSpace(Player player) => true;
		public override bool OnPickup(Player player) {
			int healMana = Math.Min(Item.stack, player.statManaMax2 - player.statMana);
			player.statMana += healMana;
			CombatText.NewText(
				new Rectangle((int)player.position.X, (int)player.position.Y, player.width, 8),
				new Color(43, 185, 255),
				healMana
			);
			return false;
		}
		public override void GrabRange(Player player, ref int grabRange) {
			if (player.statMana < player.statManaMax2) {
				grabRange *= 4;
			}
		}
	}
}
