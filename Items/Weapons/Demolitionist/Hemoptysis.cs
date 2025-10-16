using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Hemoptysis : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Launcher
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Hemoptysis_P>(14, 50, 8f, 46, 28, true);
			Item.value = Item.sellPrice(silver: 45);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.NPCDeath17.WithVolume(0.5f);
			Item.ArmorPenetration += 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimtaneBar, 10)
			.AddIngredient(ItemID.TissueSample, 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, -8f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
	}
	public class Hemoptysis_P : ModProjectile {
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.tileCollide = true;
			Projectile.width = Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			if (++Projectile.ai[0] > 60) {
				Projectile.velocity.Y += 0.02f;
			}
			Projectile.rotation += Projectile.direction * 0.05f;
			if (Main.rand.NextBool(2)) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				);
			if (Projectile.owner != Main.myPlayer) return true;
			if (Projectile.velocity.X != oldVelocity.X) {
				Vector2 dir = new(oldVelocity.X - Projectile.velocity.X, 0);
				dir.Normalize();
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + dir * CollisionExt.Raymarch(Projectile.Center, dir, 32),
					dir,
					ModContent.ProjectileType<Hemoptysis_P2>(),
					Projectile.damage / 2,
					Projectile.knockBack / 10,
					Projectile.owner
				);
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Vector2 dir = new(0, oldVelocity.Y - Projectile.velocity.Y);
				dir.Normalize();
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + dir * CollisionExt.Raymarch(Projectile.Center, dir, 32),
					dir,
					ModContent.ProjectileType<Hemoptysis_P2>(),
					Projectile.damage / 2,
					Projectile.knockBack / 10,
					Projectile.owner
				);
			}
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(4)) target.AddBuff(BuffID.Bleeding, 600);
			Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				);
			for (int i = 0; i < 5; i++) {
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					Main.rand.NextVector2CircularEdge(2, 2),
					ModContent.ProjectileType<Hemoptysis_Blood_P>(),
					Projectile.damage / 2,
					Projectile.knockBack / 3,
					Projectile.owner
				);
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
		}
	}
	public class Hemoptysis_P2 : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.aiStyle = 0;
			Projectile.penetrate = 25;
			Projectile.extraUpdates = 0;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.ignoreWater = false;
			Projectile.hide = true;
			Projectile.timeLeft = 20 * 60;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(4)) target.AddBuff(BuffID.Bleeding, 600);
			target.AddBuff(Slow_Debuff.ID, 60);
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
			DrawOriginOffsetX = -32;
			DrawOriginOffsetY = -6;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 2; i++) {
				Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Blood);
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new(18, 8),
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
	public class Hemoptysis_Blood_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Hemorang";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.aiStyle = 1;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.ignoreWater = false;
			Projectile.hide = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(4)) target.AddBuff(BuffID.Bleeding, 600);
		}
		public override void AI() {
			if (Projectile.numUpdates == -1) Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Blood).velocity *= 0.25f;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 2; i++) {
				Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Blood);
			}
		}
	}
}
