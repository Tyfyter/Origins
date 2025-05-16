using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
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
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.damage = 20;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Shinedown_Staff_P>();
			Item.shootSpeed = 12f;
			Item.mana = 13;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = false;
			Item.channel = true;
		}
	}
	public class Shinedown_Staff_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MedusaHeadRay;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.aiStyle = ProjAIStyleID.HeldProjectile;
		}
		Aim[] aims;
		Aim[] decayingAims;
		public override void AI() {
			aims ??= new Aim[Main.maxNPCs];
			decayingAims ??= new Aim[20];
			Player player = Main.player[Projectile.owner];
			if (!player.noItems && !player.CCed) {
				if (--Projectile.ai[0] <= 0) {
					if (Main.myPlayer == Projectile.owner) {
						if (!player.channel || !ActuallyShoot()) Projectile.Kill();
					}
				}
				if (Projectile.localAI[0] != Projectile.ai[0]) {
					if (Projectile.ai[0] > Projectile.localAI[0]) Projectile.localAI[1] = Projectile.ai[0];
					Projectile.localAI[0] = Projectile.ai[0];
				}
			} else {
				Projectile.Kill();
			}
			int activeAims = 0;
			Vector2 center = Projectile.Center;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) {
					activeAims++;
					if (aims[i].Update(center)) AddDecayingAim(aims[i]);
				}
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				decayingAims[i].UpdateDecaying();
			}
			Triangle hitTri;
			Vector2 perp;
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
						npc.GetGlobalNPC<OriginGlobalNPC>().shinedownDamage += hitModifiers.GetDamage(Projectile.damage, false);
						break;
					}
				}
			}
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
		bool ActuallyShoot() {
			Player player = Main.player[Projectile.owner];
			Vector2 position = Projectile.position;
			EntitySource_ItemUse projectileSource = new(player, player.HeldItem);
			float bestAngle = 0.5f;
			Vector2 aimOrigin = player.Top;
			Vector2 aimVector = aimOrigin.DirectionTo(Main.MouseWorld);
			Projectile.ai[0] = 1;
			const float range = 16 * 25;
			foreach (NPC npc in Main.ActiveNPCs) {
				Vector2 diff = Main.MouseWorld.Clamp(npc.Hitbox) - aimOrigin;
				float lengthSQ = diff.LengthSquared();
				if (lengthSQ > range * range) continue;
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
			for (int i = 0; i < aims.Length; i++) {
				if (!aims[i].active) continue;
				Vector2 motion = aims[i].Motion;
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					Color.Black,
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
					Color.Black,
					motion.ToRotation(),
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
				active = true;
			}
			public bool Update(Vector2 position) {
				NPC target = Main.npc[index];
				if (!target.active || target.type != type) target = null;
				if (target is null) {
					active = false;
					return true;
				}
				MathUtils.LinearSmoothing(ref motion, target.Center - position, 4);
				motion = Utils.rotateTowards(Vector2.Zero, motion, target.Center - position, 0.3f);
				return false;
			}
			public void UpdateDecaying() {
				if (active) {
					motion *= 0.93f;
					active = Motion.LengthSquared() < 16;
				}
			}
		}
	}
}
