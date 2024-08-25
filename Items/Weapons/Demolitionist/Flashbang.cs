using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Flashbang : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 26;
			Item.knockBack = 0;
			Item.crit += 16;
			Item.shootSpeed *= 2;
			Item.shoot = ModContent.ProjectileType<Flashbang_P>();
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 15);
			Item.maxStack = 999;
            Item.ArmorPenetration += 2;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ItemID.Grenade, 5);
			recipe.Register();
		}
	}
	public class Flashbang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flashbang";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Flash_P>(), 0, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Confused, 220);
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(BuffID.Darkness, 60);
		}
	}
	public class Flash_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flash";
		public override void SetDefaults() {
			Projectile.timeLeft = 25;
			Projectile.tileCollide = false;
			Projectile.alpha = 100;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1));
		}
        public override bool PreDraw(ref Color lightColor) {
			const float scale = 2f;
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			DrawData data = new(
				Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 1, 1),
				new Color(0, 0, 0, 255),
				0, new Vector2(0.5f, 0.5f),
				new Vector2(160, 160) * scale,
				SpriteEffects.None,
			0);
			float percent = Projectile.timeLeft / 10f;
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(0f + percent);
			Origins.blackHoleShade.UseColor(1, 1, 1);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
			return false;
		}
    }
}
