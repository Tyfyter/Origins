using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Microsoft.Xna.Framework.MathHelper;
using Origins.Dev;
using PegasusLib;
namespace Origins.Items.Weapons.Demolitionist {
	public class Black_Hole_Bomb : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsBomb",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 455;
			Item.value *= 2;
			Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);
			Item.shoot = ModContent.ProjectileType<Black_Hole_Bomb_P>();
			Item.shootSpeed *= 2;
			Item.knockBack = 50f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = CrimsonRarity.ID;
		}
	}
	public class Black_Hole_Bomb_P : ModProjectile {
		const int initDur = 5;
		const int maxDur = 1800;
		const int growDur = 90;
		const float distExp = 2f;
		const float strengthMult = 512f;
		const float knockbackResistanceSignificance = 0.9f;
		const float dotDivisor = 10;
		const int collapseDur = 10;
		const int totalDur = growDur + collapseDur;
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Black_Hole_Bomb";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.aiStyle = 14;
			Projectile.penetrate = -1;
			Projectile.timeLeft = maxDur;
			Projectile.scale = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 4;
			Projectile.width = 26;
			Projectile.height = 24;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Main.netMode != NetmodeID.Server) {
				if (Projectile.timeLeft > maxDur - initDur) {
					Projectile.scale += 1f / initDur;
				}
				if (Projectile.timeLeft <= 190) {
					Projectile.alpha = 255;
					Projectile.scale += 1f / totalDur;
				}
			}
			float percent = Clamp((totalDur - Projectile.timeLeft) / (float)growDur, 0, 1) * 2;
			float scale = 0;
			if (Projectile.timeLeft < collapseDur) {
				scale = (collapseDur - Projectile.timeLeft) / (collapseDur / 2f);
			}
			float strength = strengthMult * (1 + percent) * (Projectile.scale + scale);
			NPC target;
			float kbrs;
			for (int i = 0; i < Main.npc.Length; i++) {
				target = Main.npc[i];
				kbrs = knockbackResistanceSignificance * (target.defense / 50f);
				if ((target.CanBeChasedBy() || (target.lifeMax == 1 && !target.dontTakeDamage)) && !(target.type == NPCID.MoonLordFreeEye || target.type == NPCID.MoonLordHand || target.type == NPCID.MoonLordHead || target.type == NPCID.MoonLordCore)) {
					float dist = (target.Center - Projectile.Center).Length() / 16f;
					float distSQ = (float)Math.Pow(dist, distExp);
					float force = strength / distSQ;
					Vector2 dir = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero);
					float point = (target.knockBackResist * kbrs + (1f - kbrs));
					point *= (float)Math.Min(GeometryUtils.AngleDif(dir.ToRotation(), target.velocity.ToRotation(), out _) + Clamp(force - target.velocity.Length(), 0, 1), 1);
					if (force > 1) target.velocity = Vector2.Lerp(target.velocity, dir * Min(force, dist), point);
					if (force >= (Projectile.Center.Clamp(target.Hitbox) - Projectile.Center).Length()) {

						if (Projectile.timeLeft > totalDur && Projectile.Hitbox.Intersects(target.Hitbox)) OnHitNPC(target, new NPC.HitInfo(), 0);
						if (Projectile.localNPCImmunity[target.whoAmI] <= 0) {
							target.SimpleStrikeNPC((int)(Projectile.damage / dotDivisor), 0);
							Projectile.localNPCImmunity[target.whoAmI] = 10;
						}
					}
				}
			}
			Item targetItem;
			for (int i = 0; i < Main.item.Length; i++) {
				targetItem = Main.item[i];
				float dist = (targetItem.Center - Projectile.Center).Length() / 16f;
				float distSQ = (float)Math.Pow(dist, distExp);
				float force = strength / distSQ;
				if (force > 1) targetItem.velocity = Vector2.Lerp(targetItem.velocity, (Projectile.Center - targetItem.Center).SafeNormalize(Vector2.Zero) * Min(force, dist), 0.9f);
			}
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.timeLeft <= totalDur) return false;
			Projectile.aiStyle = 0;
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft = totalDur;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.timeLeft <= totalDur) return;
			Projectile.aiStyle = 0;
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft = totalDur;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Dynamite;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 192;
			Projectile.height = 192;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
            ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
        }
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void PostDraw(Color lightColor) {
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			float percent = Clamp((totalDur - Projectile.timeLeft) / (float)growDur, 0, 1);
			float scale = 0;
			if (Projectile.timeLeft < collapseDur) {
				scale = (collapseDur - Projectile.timeLeft) / (collapseDur / 2f);
			}
			DrawData data = new DrawData(Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(0, 0, 0, 255), 0, new Vector2(0.5f, 0.5f), new Vector2(160, 160) * (Projectile.scale - scale), SpriteEffects.None, 0);
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(3f + percent);
			Origins.blackHoleShade.UseColor(0, 0, 0);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
		}
	}
}
