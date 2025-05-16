using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Buffs;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items.Donate;
using static System.Net.Mime.MediaTypeNames;

namespace Origins.Items.Weapons.Magic {
	public class Shinedown : ModItem {
		public override string Texture => typeof(Bled_Out_Staff).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClass.Magic;
			Item.useStyle = -1;
			Item.damage = 20;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Shinedown_Staff_P>();
			Item.shootSpeed = 300f;
			Item.mana = 13;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = false;
			Item.channel = true;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.bodyFrame.Y = player.bodyFrame.Height * 2;
			player.itemLocation = player.MountedCenter + new Vector2(6 * player.direction, 0);
		}
	}
	public class Shinedown_Staff_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MedusaHeadRay;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
		}
		public override bool ShouldUpdatePosition() => false;
		Aim[] aims;
		Aim[] decayingAims;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.itemRotation = MathHelper.PiOver4 * -player.direction;
			Projectile.position = player.RotatedRelativePoint(player.MountedCenter + new Vector2(6 * player.direction, -48));
			if (Projectile.ai[1] == 1) {
				Projectile.ai[1] = 2;
				for (int i = 0; i < aims.Length; i++) {
					if (aims[i].active) {
						aims[i].active = false;
						AddDecayingAim(aims[i]);
					}
				}
			}
			if (Projectile.ai[1] == 2) {
				Projectile.aiStyle = -1;
				for (int i = 0; i < decayingAims.Length; i++) {
					decayingAims[i].UpdateDecaying();
					if (decayingAims[i].active) Projectile.timeLeft = 2;
				}
				if (Projectile.timeLeft == 2) player.SetDummyItemTime(2);
				return;
			}
			player.SetDummyItemTime(2);
			aims ??= new Aim[Main.maxNPCs];
			decayingAims ??= new Aim[20];
			float maxLengthSQ = Projectile.velocity.LengthSquared();
			if (!player.noItems && !player.CCed) {
				if (--Projectile.ai[0] <= 0) {
					if (Main.myPlayer == Projectile.owner) {
						if (!player.channel || !ActuallyShoot(maxLengthSQ)) Projectile.ai[1] = 1;
					}
				}
				if (Projectile.localAI[0] != Projectile.ai[0]) {
					if (Projectile.ai[0] > Projectile.localAI[0]) Projectile.localAI[1] = Projectile.ai[0];
					Projectile.localAI[0] = Projectile.ai[0];
				}
			} else {
				Projectile.ai[1] = 1;
			}
			Vector2 center = Projectile.Center;
			int activeAims = 0;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) {
					activeAims++;
					if (aims[i].Update(center, maxLengthSQ)) AddDecayingAim(aims[i]);
				}
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				decayingAims[i].UpdateDecaying();
			}
			Triangle hitTri;
			Vector2 perp;
			int totalDamage = 0;
			OriginPlayer originPlayer = player.OriginPlayer();
			foreach (NPC npc in Main.ActiveNPCs) {
				Rectangle npcHitbox = npc.Hitbox;
				for (int i = 0; i < aims.Length; i++) {
					if (!aims[i].active) continue;
					Vector2 motion = aims[i].Motion;
					if (motion == Vector2.Zero) continue;
					Vector2 norm = motion.SafeNormalize(Vector2.Zero);
					perp.X = norm.Y;
					perp.Y = -norm.X;
					hitTri = new(center + perp * 16, center - perp * 16, center + motion * Projectile.scale + norm * 16);
					if (hitTri.Intersects(npcHitbox)) {
						NPC.HitModifiers hitModifiers = npc.GetIncomingStrikeModifiers(Projectile.DamageType, 0);
						hitModifiers.Defense = new StatModifier(0, 0);
						int damage = hitModifiers.GetDamage(Projectile.damage, false);
						npc.GetGlobalNPC<OriginGlobalNPC>().shinedownDamage += damage;
						totalDamage += damage;
						if (Projectile.owner == Main.myPlayer && Main.rand.NextFloat() < Projectile.knockBack / 60f) {
							switch (Main.rand.Next(4)) {
								case 0:
								npc.AddBuff(BuffID.CursedInferno, 60);
								break;

								case 1:
								npc.AddBuff(BuffID.Ichor, 60);
								break;

								case 2:
								npc.AddBuff(Rasterized_Debuff.ID, 20);
								break;

								case 3:
								OriginGlobalNPC.InflictTorn(npc, 60, 60, source: originPlayer);
								break;
							}
						}
						break;
					}
				}
			}
			player.addDPS(Main.rand.RandomRound(totalDamage / 30f));
		}
		void AddDecayingAim(Aim aim) {
			aim.active = true;
			float bestLength = float.PositiveInfinity;
			int bestDecaying = 0;
			for (int i = 0; i < decayingAims.Length; i++) {
				if (decayingAims[i].active) {
					float length = decayingAims[i].Motion.LengthSquared();
					if (bestLength > length) {
						bestLength = length;
						bestDecaying = i;
					}
				} else {
					bestDecaying = i;
					break;
				}
			}
			decayingAims[bestDecaying] = aim;
		}
		bool ActuallyShoot(float maxLengthSQ) {
			Player player = Main.player[Projectile.owner];
			float bestAngle = 0.5f;
			Vector2 aimOrigin = player.Top;
			Vector2 aimVector = aimOrigin.DirectionTo(Main.MouseWorld);
			Projectile.ai[0] = 1;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (aims[npc.whoAmI].active) continue;
				//if (!npc.CanBeChasedBy(Projectile)) continue;
				Vector2 diff = Main.MouseWorld.Clamp(npc.Hitbox) - aimOrigin;
				float lengthSQ = diff.LengthSquared();
				if (lengthSQ > maxLengthSQ) continue;
				diff /= MathF.Sqrt(lengthSQ);
				float angle = Vector2.Dot(diff, aimVector);
				if (angle > bestAngle) {
					aims[npc.whoAmI].Set(npc);
				}
			}

			Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
			Projectile.netUpdate = true;
			return true;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Vector2 origin = texture.Frame().Bottom();
			float spriteLengthFactor = 1f / texture.Height;
			Color color = Color.Black * 0.4f;
			for (int i = 0; i < aims.Length; i++) {
				if (!aims[i].active) continue;
				Vector2 motion = aims[i].Motion;
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				if (!decayingAims[i].active) continue;
				Vector2 motion = decayingAims[i].Motion;
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			return false;
		}
		struct Aim {
			int index;
			int type;
			Vector2 motion;
			public bool active;
			public readonly Vector2 Motion => motion;
			public void Set(NPC target) {
				index = target.whoAmI;
				type = target.type;
				motion = default;
				active = true;
			}
			public bool Update(Vector2 position, float maxLengthSQ) {
				NPC target = Main.npc[index];
				if (!target.active || target.type != type) target = null;
				if (target is null) {
					active = false;
					return true;
				}
				Vector2 diff = target.Center - position;
				if (diff.LengthSquared() > maxLengthSQ) {
					active = false;
					return true;
				}
				MathUtils.LinearSmoothing(ref motion, diff, 4);
				motion = Utils.rotateTowards(Vector2.Zero, motion, diff, 0.3f);
				return false;
			}
			public void UpdateDecaying() {
				if (active) {
					float length = Motion.Length();
					motion *= 0.99f * ((length - 2) / length);
					active = length > 4;
				}
			}
		}
	}
}
