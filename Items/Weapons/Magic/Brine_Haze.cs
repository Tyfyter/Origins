using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Accessories;
using Origins.Tiles.Brine;
using Origins.Walls;
using System;
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
			Item.knockBack = 8;
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
				Projectile.NewProjectile(source, position, velocity + (toAdd * 74 / 32), type, damage, knockback, player.whoAmI);
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
			Projectile.height = 68 * 2;
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
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;/*
			float val = 1.5f;
			if (Projectile.ai[0]++ <= 60 && Projectile.ai[0] >= 0) val -= Projectile.ai[0];
			width = (int)(width / val);
			height = (int)(height / val);*/
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffType<Toxic_Shock_Debuff>(), 5 * 60);
		}/*
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.DrawDebugOutline(dustType: DustType<Tintable_Torch_Dust>(), color: Color.Red);
		}*/
		public override void PostDraw(Color lightColor) {
			if (Main.dedServ) return;
			if (Main.rand.NextFloat(1000) < Main.gfxQuality * (!Projectile.wet ? 1000 : 850)) {
				float width = Projectile.width + Projectile.velocity.X * 1.5f;
				float height = Projectile.height + Projectile.velocity.Y * 1.5f;
				Vector2 start = Projectile.TopLeft - new Vector2(Projectile.width, Projectile.height) / 4;
				Dust.NewDustDirect(start, (int)width, (int)height, Main.rand.Next(Brine_Cloud_Dust.dusts), newColor: new(18, 73, 56)).velocity *= 0.1f;
				int tmp1 = (int)start.X;
				int tmp2 = (int)start.Y;
				int tmp3 = (int)width;
				int tmp4 = (int)height;
				new Rectangle(tmp1, tmp2, tmp3, tmp4).DrawDebugOutline(dustType: DustType<Tintable_Torch_Dust>(), color: Color.Blue);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Vector2 pos = Projectile.Bottom - new Vector2(0, 4) - Main.screenPosition;
			Main.EntitySpriteDraw(
				texture,
				pos,
				frame,
				Projectile.GetAlpha(lightColor.MultiplyRGBA(new Color(18, 73, 56, 200))),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height - 4),
				new Vector2(Projectile.scale * 1.5f, Projectile.scale * 1.05f),
				SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				texture,
				pos - new Vector2(0, frame.Height),
				frame,
				Projectile.GetAlpha(lightColor.MultiplyRGBA(new Color(18, 73, 56, 200))),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height - 4),
				new Vector2(Projectile.scale * 1.5f, Projectile.scale * 1.05f),
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
