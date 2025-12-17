using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Layers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class Amebic_Vial : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public static bool[] CanBeDeflected => OriginsSets.Projectiles.CanBeDeflected;
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Waist_Glow_Layer>(Item.waistSlot, Texture + "_Waist_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 26);
			Item.maxStack = 1;
			Item.dye = -1;
			Item.hairDye = -1;
			Item.shoot = ModContent.ProjectileType<Amebic_Vial_Tentacle>();
			Item.rare = ItemRarityID.Orange;
			Item.master = true;
			Item.hasVanityEffects = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.glowMask = glowmask;
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.amebicVialCooldown > 0) {
				originPlayer.amebicVialVisible = false;
				return;
			}
			if (!isHidden) player.EnableShadow<Amebic_Vial_Shadow>();
			const float maxDist = 64 * 64;
			Vector2 target = default;
			float bestWeight = 0;
			Projectile projectile;
			Vector2 currentPos;
			Vector2 diff;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				projectile = Main.projectile[i];
				if (projectile.active && (projectile.hostile || (Main.player[projectile.owner].hostile && Main.player[projectile.owner].team != player.team)) && Amebic_Vial.CanBeDeflected[projectile.type]) {
					currentPos = projectile.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(projectile.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(projectile.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			NPC npc;
			for (int i = 0; i < Main.maxNPCs; i++) {
				npc = Main.npc[i];
				if (npc.active && npc.aiStyle == NPCAIStyleID.Spell) {
					currentPos = npc.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(npc.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(npc.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			if (bestWeight > 0) {
				float dir = (target.Y > player.MountedCenter.Y == target.X > player.MountedCenter.X) ? -1 : 1;
				Projectile.NewProjectile(player.GetSource_Accessory(Item), player.MountedCenter, (target - player.MountedCenter).SafeNormalize(default).RotatedBy(dir * -1f) * 3.2f, Item.shoot, 1, 0, player.whoAmI, ai1: dir);
				originPlayer.amebicVialCooldown = 120;
			}
		}
		public override void UpdateVanity(Player player) {
			player.EnableShadow<Amebic_Vial_Shadow>();
			//player.GetModPlayer<OriginPlayer>().amebicVialVisible = true;
		}
	}
	public class Amebic_Vial_Tentacle : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Summoner/Flagellash_P";
		public override string GlowTexture => Texture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 2;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 40;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
			Projectile.alpha = 150;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);

			Projectile.Center = ownerMountedCenter;

			if (movementFactor == 0f) {
				movementFactor = 5.1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft < 20) {
				movementFactor -= 1f;
			} else {
				movementFactor += 1f;
			}

			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1] * 0.05f);
			Projectile.position += Projectile.velocity * movementFactor;

			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
			Projectile other;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				other = Main.projectile[i];
				if (other.active && other.hostile && Amebic_Vial.CanBeDeflected[other.type] && (Colliding(Projectile.Hitbox, other.Hitbox) ?? false)) {
					other.velocity = Vector2.Lerp(other.velocity, Projectile.velocity, 0.5f);
				}
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox) || Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[Projectile.owner].MountedCenter + Projectile.velocity * 2, Projectile.Center)) {
				return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Vector2 diff = Projectile.Center - ownerMountedCenter;
			int dist = (int)Math.Min(diff.Length(), 180);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				ownerMountedCenter - Main.screenPosition,
				new Rectangle(0, 180 - dist, 6, dist),
				Color.White,
				Projectile.rotation,
				new Vector2(3, 0),
				Projectile.scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
	public class Amebic_Vial_Shadow : ShadowType {
		public override IEnumerable<ShadowType> SortBelow() => [PartialEffects];
		public override IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from) {
			const float offset = 2;
			Vector2 position = from.Position;
			void ApplyOffset(Vector2 offset) {
				from.Position = position + offset;
				from.PreDraw = () => Origins.amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(offset);
			}
			from.Shader = Origins.amebicProtectionShaderID;
			ApplyOffset(Vector2.UnitX * offset);
			yield return from;
			ApplyOffset(Vector2.UnitX * -offset);
			yield return from;

			ApplyOffset(Vector2.UnitY * offset);
			yield return from;
			ApplyOffset(Vector2.UnitY * -offset);
			yield return from;
		}
	}
}
