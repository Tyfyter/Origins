using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Splitsplash : ModItem, ICustomWikiStat {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
        public string[] Categories => [
            WikiCategories.SpellBook
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalStorm);
			Item.damage = 16;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 8;
			Item.useAnimation = 24;
			Item.shoot = ModContent.ProjectileType<Splitsplash_P>();
			Item.shootSpeed = 8.75f;
			Item.mana = 16;
			Item.knockBack = 0f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item21;
			Item.reuseDelay = 8;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Splitsplash_P : ModProjectile {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 5;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.scale = 0.75f;
			Projectile.alpha = 150;
		}
		public override void AI() {
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 7) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			float speed = oldVelocity.Length();
			Projectile.velocity = Projectile.velocity + Projectile.velocity - oldVelocity;
			Projectile.velocity *= speed / Projectile.velocity.Length();
			Projectile.penetrate--;
			Split();
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 hitbox = Projectile.Hitbox.Center.ToVector2();
			Vector2 intersect = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center.ToVector2();
			bool bounced = false;
			if (hitbox.X != intersect.X) {
				Projectile.velocity.X = -Projectile.velocity.X;
				bounced = true;
			}
			if (hitbox.Y != intersect.Y) {
				Projectile.velocity.Y = -Projectile.velocity.Y;
				bounced = true;
			}
			if (!bounced) {
				if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
					Projectile.velocity.X = -Projectile.velocity.X;
				} else if (Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X)) {
					Projectile.velocity.Y = -Projectile.velocity.Y;
				}
			}
			Split();
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		}
		void Split() {
			if (Type != Splitsplash_P.ID) return;
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center - new Vector2(15),
					OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Projectile.velocity.Length()),
					Splitsplash_Smol_P.ID,
					Projectile.damage / 4,
					Projectile.knockBack,
					Projectile.owner
				);
			}
		}
	}
	public class Splitsplash_Smol_P : Splitsplash_P {
		public static new int ID { get; private set; }
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 2;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 60;
			Projectile.scale = 0.5f;
			Projectile.alpha = 150;
		}
	}
}
