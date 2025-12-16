using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Tiles.Brine;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Fumethrower : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Venom];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ElfMelter);
			Item.damage = 25;
			Item.useAnimation = 20;
			Item.useTime = 5;
			Item.width = 36;
			Item.height = 16;
			Item.useAmmo = AmmoID.Gel;
			Item.shoot = ModContent.ProjectileType<Fumethrower_P>();
			Item.shootSpeed = 8f;
			Item.reuseDelay = 6;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration = 25;
			Item.UseSound = SoundID.Item34;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Alkaliphiliac_Tissue>(10)
			.AddIngredient<Baryte_Item>(20)
			.AddIngredient<Brineglow_Item>(8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += velocity.SafeNormalize(default) * 36;
		}
	}
	public class Fumethrower_P : ModProjectile {
		public static float Lifetime => 108f;
		public static float MinSize => 16f;
		public static float MaxSize => 66f;
		private readonly float[] sizes = new float[32];
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 32;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void AI() {
			if (Projectile.localAI[2] == 0) {
				Projectile.localAI[2] = 1 + Projectile.wet.ToInt();
			}
			Projectile.localAI[0] += 1f;
			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			if (Projectile.localAI[2] == 1) {
				Lighting.AddLight(Projectile.Center, 0f, 0.85f, 0.4f);
			}
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
			Projectile.ai[0]++;
			Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			if (Projectile.ai[0] > Lifetime) {
				Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)(Size / 2) - hitbox.Width;
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Venom, hit.Crit ? 600 : 360);
		}
		public override bool PreDraw(ref Color lightColor) {
			float progress = Projectile.ai[0] / Lifetime;
			Flamethrower_Drawer.Draw(Projectile,
				1 - progress,
				TextureAssets.Projectile[Type].Value,
				Color.DarkCyan,
				sizes,
				8,
				smokeAmount: (Projectile.localAI[2] - 1) * 0.5f + progress * 0.5f,
				sizeProgressOverride: i => Math.Min(1 - ((Projectile.ai[0] - i) / Lifetime), 1) * 0.25f,
				alphaMultiplier: Projectile.localAI[2] * 0.55f,
				tint: i => Color.White * (1 - float.Pow(progress, 1 - i / 32))
			);
			return false;
			/*Color[] colors = [
				new(18, 33, 22),
				new(26, 45, 31),
				new(37, 61, 43),
				new(18, 33, 22)
			];
			if (Projectile.localAI[2] == 1) {
				//float progress = Utils.Remap(Projectile.ai[0], 0f, Lifetime, 0, 1);
				for (int i = 0; i < colors.Length; i++) {
					Color color = colors[i];
					color.G = (byte)(color.G + (1 - progress) * color.G * 2);
					color.B = (byte)(color.B + progress * color.B * 2);
					color.A = 100;
					colors[i] = color;
				}
			} else {
				for (int i = 0; i < colors.Length; i++) {
					colors[i] *= 0.8f;
				}
			}
			Projectile.DrawFlamethrower(colors[0], colors[1], colors[2], colors[3]);
			return false;*/
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}
