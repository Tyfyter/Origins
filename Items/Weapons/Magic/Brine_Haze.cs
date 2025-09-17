using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Weapons.Magic {
	public class Brine_Haze : ModItem, ICustomWikiStat {
        public string[] Categories => [
			"UsesBookcase",
			"SpellBook"
		];
        public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamethrower);
			Item.damage = 48;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ProjectileType<Brine_Haze_P>();
			Item.mana = 24;
			Item.useAmmo = AmmoID.None;
			Item.noUseGraphic = false;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.knockBack = 2;
			Item.shootSpeed = 14f;
			Item.value = Item.sellPrice(gold: 2);
			Item.UseSound = SoundID.Item82;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.rare = ItemRarityID.Pink;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2[] vel = [new(4, 0), new(3, 0), new(2, 0), new(1, 0)];
			for (int i = 0; i < vel.Length * 2; i++) {
				Vector2 toAdd = vel[i % vel.Length];
				if (i >= vel.Length) toAdd.X = -toAdd.X;
				Projectile.NewProjectile(source, position, velocity + (toAdd * 74 / 32) + Main.rand.NextVector2Circular(1, 1), type, damage, knockback, player.whoAmI);
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Venom_Fang>(3)
			.AddIngredient(ItemID.SpellTome)
            .AddIngredient(ItemID.SoulofNight, 15)
            .AddTile(TileID.Bookcases)
			.Register();
		}
	}
	public class Brine_Haze_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Misc/Smonk";

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = (int)(74 * 1.5f);
			Projectile.height = 68;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
			Projectile.timeLeft = 5 * 60;
		}
		public override bool? CanCutTiles() {
			return false;
		}
		public override void AI() {
			#region General behavior
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.X *= 0.97f;
				if (Projectile.velocity.X > -0.01 && Projectile.velocity.X < 0.01) {
					Projectile.velocity.X = 0f;
				}
			} else {
				Projectile.velocity *= 0.979f;
				if (Projectile.wet) Projectile.timeLeft++;
				else Projectile.timeLeft += 2;
			}
			if (!Projectile.wet) Projectile.timeLeft -= 1;
			if (Math.Abs(Projectile.velocity.Y) <= 2) Projectile.velocity.Y += 0.02f;
			#endregion

			#region Animation and visuals
			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			if (++Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			#endregion

			Rectangle hitbox = Projectile.Hitbox;
			if (++Projectile.ai[0] <= 30) {
				ShrinkHitbox(ref hitbox, Projectile.ai[0]);
				Vector2 dir = Vector2.Zero;
				if (hitbox.OverlapsAnyTiles(out List<Point> intersectingTiles)) {
					float mult = 1f / intersectingTiles.Count;
					for (int i = 0; i < intersectingTiles.Count; i++) {
						dir -= (intersectingTiles[i].ToWorldCoordinates() - Projectile.Center) * mult;
					}
				}
				if (dir != Vector2.Zero) {
					Vector2 weight = dir.SafeNormalize(default);
					Projectile.position += weight * new Vector2(Projectile.width, Projectile.height) * 0.52f / 30;
				}
			}
			if (Main.dedServ) return;
			if (Main.rand.NextFloat(1000) < Main.gfxQuality * (!Projectile.wet ? 400 : 320)) {
				float width = hitbox.Width * 1.5f;
				float height = hitbox.Height * 1.5f;
				Vector2 start = hitbox.TopLeft() - new Vector2(hitbox.Width, hitbox.Height) / 4;
				EfficientDust.NewDustDirect(
					start,
					(int)width,
					(int)height,
					Main.rand.Next(Brine_Cloud_Dust.dusts),
					Projectile.velocity.X,
					Projectile.velocity.Y,
					newColor: new(43, 217, 162)
				).velocity *= 0.1f;
			}
		}
		static void ShrinkHitbox(ref Rectangle hitbox, float frame) {
			Vector2 center = hitbox.Center();
			float val = 1f;
			if (frame < 30) val *= frame / 30;
			hitbox.Width = (int)(hitbox.Width * val);
			hitbox.Height = (int)(hitbox.Height * val);
			hitbox = hitbox.Recentered(center);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;
			Rectangle hitbox = new(0, 0, width, height);
			ShrinkHitbox(ref hitbox, Projectile.ai[0]);
			width = hitbox.Width;
			height = hitbox.Height;
			return true;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			Vector2 center = hitbox.Center();
			float val = 1f;
			if (Projectile.ai[0] <= 30) val *= Projectile.ai[0] / 30;
			hitbox.Width = (int)(hitbox.Width * val);
			hitbox.Height = (int)(hitbox.Height * val);
			hitbox = hitbox.Recentered(center);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 5 * 60);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			float scale = Projectile.scale * Math.Min(Projectile.ai[0] / 30, 1);
			Vector2 pos = Projectile.Center - new Vector2(0, 4) - Main.screenPosition;
			Main.EntitySpriteDraw(
				texture,
				pos,
				frame,
				Projectile.GetAlpha(lightColor.MultiplyRGBA(new Color(18, 73, 56, 200))),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height / 2.3f),
				new Vector2(scale * 1.5f, scale * 1.05f),
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
