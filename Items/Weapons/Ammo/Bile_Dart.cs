using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Bile_Dart : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Dart",
			"RasterSource"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedDart);
			Item.damage = 11;
			Item.shoot = ModContent.ProjectileType<Bile_Dart_P>();
			Item.shootSpeed = 3f;
			Item.knockBack = 2.2f;
			Item.value = Item.sellPrice(copper: 6);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 100)
			.AddIngredient(ModContent.ItemType<Black_Bile>())
			.Register();
		}
	}
	public class Bile_Dart_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Bile_Dart";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedDart);
		}
		public override void AI() {
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[0] > 3f)
				Projectile.alpha = 0;

			if (Projectile.ai[0] >= 20f) {
				Projectile.ai[0] = 20f;
				Projectile.velocity.Y += 0.075f;
			}
			int auraProjIndex = (int)Projectile.ai[1] - 1;
			if (auraProjIndex < 0) {
				if (Projectile.owner == Main.myPlayer) Projectile.ai[1] = Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					Bile_Dart_Aura.ID,
					Projectile.damage / 2,
					0,
					Projectile.owner,
					Projectile.whoAmI
				) + 1;
			} else {
				Projectile auraProj = Main.projectile[auraProjIndex];
				if (auraProj.active && auraProj.type == Bile_Dart_Aura.ID) {
					auraProj.Center = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = 0;
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return Projectile.alpha == 0 ? new Color(255, 255, 255, 200) : Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			SoundEngine.PlaySound(SoundID.NPCHit22.WithVolume(0.5f), Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 30);
		}
	}
	public class Bile_Dart_Aura : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.hide = false;
			Projectile.width = Projectile.height = 72;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
			Projectile.scale = 1.5f;
			Projectile.ArmorPenetration += 100;
		}
		public override void AI() {
			Projectile parent = Projectile.GetRelatedProjectile_Depreciated(0);
			if (parent is null) {
				Projectile.scale *= 0.95f;
				Projectile.scale -= 0.05f;
				if (Projectile.scale <= 0) Projectile.Kill();
			} else {
				if (float.IsInfinity(Projectile.scale)) Projectile.timeLeft -= 5;
				if (parent.active) {
					Projectile.scale = parent.scale * 1.5f;
					Projectile.Center = parent.Center;
					Projectile.rotation = parent.rotation;
				} else {
					Projectile.Center = parent.Center;
					Projectile.ai[0] = -1;
				}
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int inflation = (int)(hitbox.Width * (Projectile.scale / 1.5f - 1) * 0.5f);
			hitbox.Inflate(inflation, inflation);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 12);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Mask_Rasterize.QueueProjectile(Projectile.whoAmI)) return false;
			Vector2 screenCenter = Main.ScreenSize.ToVector2() * 0.5f;
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[ID].Value,
				(Projectile.Center - Main.screenPosition - screenCenter) * Main.GameViewMatrix.Zoom + screenCenter,
				null,
				new Color(
					MathHelper.Clamp(Projectile.velocity.X / 16 + 0.5f, 0, 1),
					MathHelper.Clamp(Projectile.velocity.Y / 16 + 0.5f, 0, 1),
				0f),
				0,
				new Vector2(36),
				Projectile.scale * Main.GameViewMatrix.Zoom.X,
				0,
			0);
			return false;
		}
	}
}
