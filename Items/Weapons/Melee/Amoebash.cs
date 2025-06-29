using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Journal;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Projectiles.Weapons;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.BardItems;
using Tyfyter.Utils;
namespace Origins.Items.Weapons.Melee {
	public class Amoebash : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword"
        ];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 87;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 34;
			Item.useAnimation = 34;
			Item.shoot = ModContent.ProjectileType<Amoebash_Smash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 12f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 0;
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => true;
	}
	public class Amoebash_Smash : MeleeSlamProjectile {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Amoebash).GetDefaultTMLName() + "_Glow";
		public override string Texture => typeof(Amoebash).GetDefaultTMLName();
		public override bool CanHitTiles() => Projectile.rotation * Projectile.ai[1] > -0.85f;
		public override void OnHitTiles(Vector2 position, Vector2 direction) {
			Vector2 slamDir = direction.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
			Collision.HitTiles(position, slamDir, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, position + Projectile.Size * 0.5f);
			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				Projectile.Center, 5f, 3f, 12, 500f, -1f, nameof(Amoebash)
			));

			IEntitySource source = Projectile.GetSource_FromAI();
			int projType = ModContent.ProjectileType<Amoebash_Shrapnel>();
			for (int j = 0; j <= 4; j++) {
				if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(
					source,
					position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
					direction.RotatedBy(Projectile.ai[1] * -0.5f + Main.rand.NextFloat(-0.2f, 0.4f)) * Main.rand.NextFloat(0.2f, 0.3f),
					projType,
					Projectile.damage / 2,
					Projectile.knockBack * 0.2f,
					Projectile.owner
				);
				if (!Main.dedServ) Gore.NewGore(
					source,
					position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
					slamDir * 0.1f,
					Main.rand.Next(R_Effect_Blood1.GoreIDs),
					1f
				);
			}
		}
		static bool forcedCrit = false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			forcedCrit = false;
			if (!target.noTileCollide && float.IsNaN(Projectile.ai[2])) {
				Rectangle hitbox = target.Hitbox;
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation + Projectile.ai[1] * 2.5f).SafeNormalize(default);
				hitbox.Offset((dir * 8).ToPoint());
				if (hitbox.OverlapsAnyTiles(fallThrough: false)) {
					Collision.HitTiles(hitbox.TopLeft(), dir, hitbox.Width, hitbox.Height);
					modifiers.SetCrit();
					forcedCrit = true;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 120, targetSeverity: 0.4f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
			target.velocity -= target.velocity * target.knockBackResist;
			if (!float.IsNaN(hit.Knockback)) {
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation);
				if (!forcedCrit) dir += dir.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
				target.velocity += dir.SafeNormalize(default) * hit.Knockback;
			}
			target.SyncCustomKnockback();
			forcedCrit = false;
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = GetDrawData(lightColor, new Vector2(10, 39 + 28));
			Main.EntitySpriteDraw(data);
			data.texture = glowTexture;
			data.color = Riven_Hive.GetGlowAlpha(lightColor);
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
	public class Amoebash_Shrapnel : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Ameballoon_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 3600;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 80, targetSeverity: 0.15f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft < 3597) {
				SoundEngine.PlaySound(SoundID.NPCHit18.WithPitch(0.15f).WithVolumeScale(0.5f), Projectile.Center);
				for (int i = Main.rand.Next(6, 12); i-->0;) {
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(0f, 1f), ModContent.GoreType<R_Effect_Blood1_Small>());
				}
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
