using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Sleetfire : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ElfMelter);
			Item.damage = 9;
			Item.useAnimation = 20;
			Item.useTime = 4;
			Item.width = 36;
			Item.height = 16;
			Item.useAmmo = AmmoID.Gel;
			Item.shoot = ModContent.ProjectileType<Sleetfire_P>();
			Item.shootSpeed = 7f;
			Item.reuseDelay = 9;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration = 5;
			Item.UseSound = SoundID.Item34;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.IceTorch)
            .AddIngredient(ItemID.Shiverthorn, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(4, 0);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            position += velocity.SafeNormalize(default) * 36;
        }
    }
	public class Sleetfire_P : ModProjectile {
        public static float Lifetime => 30f;
        public static float MinSize => 16f;
        public static float MaxSize => 66f;
		private readonly float[] sizes = new float[21];
		public override void SetStaticDefaults() {
            Main.projFrames[Type] = 7;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Frostburn];
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void SetDefaults() {
            Projectile.width = Projectile.height = 6;
            Projectile.penetrate = 2;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
        public override void AI() {
            Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.85f);
            if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
            Projectile.ai[0]++;
			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			Projectile.scale = Size / 96f;
            Projectile.alpha = (int)(200 * (1 - (Projectile.ai[0] / Lifetime)));
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
            target.AddBuff(BuffID.Frostburn, hit.Crit ? 600 : 300);
        }
        public override bool PreDraw(ref Color lightColor) {
			Flamethrower_Drawer.Draw(Projectile, 1 - (Projectile.ai[0] / Lifetime), TextureAssets.Projectile[Type].Value, Color.AliceBlue, sizes, 8, 1);
			return false;
			/*Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: 3);
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                new Color(128, 200, 255, 128) * (1 - Projectile.alpha / 255f),
                Projectile.rotation,
                frame.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
            0);
            return false;*/
        }
    }
}
