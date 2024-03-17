using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Demolitionist {
    public class Self_Destruct : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
            "OtherExplosive"
        };
		public override void SetDefaults() {
			Item.DamageType = DamageClasses.Explosive;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.damage = 125;
			Item.crit = 24;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.UseSound = Origins.Sounds.IMustScream;
			Item.shoot = ModContent.ProjectileType<Self_Destruct_P>();
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofMight, 15)
            .AddIngredient(ModContent.ItemType<Busted_Servo>(), 28)
            .AddIngredient(ModContent.ItemType<Power_Core>(), 2)
            .AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = player.MountedCenter;
			velocity = player.velocity;
		}
	}
	public class Self_Destruct_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Self_Destruct_Body";
		public override void SetDefaults() {
			Projectile.timeLeft = 80;
			Projectile.tileCollide = false;
			Projectile.width = 32;
			Projectile.height = 48;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.velocity *= 0.9f;
			Projectile.Center = player.MountedCenter;
			Projectile.velocity = player.velocity;
			player.heldProj = Projectile.whoAmI;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override void OnKill(int timeLeft) {
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				default,
				ModContent.ProjectileType<Self_Destruct_Explosion>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Rectangle frame = new Rectangle((player.bodyFrame.Y / player.bodyFrame.Height == 5 ? 1 : 0) * 40, (player.Male ? 0 : 2) * 56, 40, 56);
			Vector2 position = new Vector2(
					(int)(player.position.X - (player.bodyFrame.Width / 2) + (player.width / 2)),
					(int)(player.position.Y + player.height - player.bodyFrame.Height)
				)
				+ player.bodyPosition
				+ new Vector2(player.bodyFrame.Width / 2, player.bodyFrame.Height / 2)
				+ Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				position - Main.screenPosition,
				frame,
				Color.White,
				player.bodyRotation,
				new Vector2(player.legFrame.Width * 0.5f, player.legFrame.Height * 0.5f),
				1f,
				player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
	}
	public class Self_Destruct_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 5;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 352;
			Projectile.height = 352;
			Projectile.hide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(
					Projectile,
					true,
					sound: SoundID.Item62
				);
				ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
				Projectile.ai[0] = 1;
			}
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Self_Destruct_Flash>(), 0, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
        }
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
    public class Self_Destruct_Flash : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Demolitionist/Self_Destruct_Visual";
        public override void SetDefaults() {
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.alpha = 100;
        }
        public override void AI() {
            Lighting.AddLight(Projectile.Center, new Vector3(2, 0, 0));
        }
        public override bool PreDraw(ref Color lightColor) {
            const float scale = 3f;
            Main.spriteBatch.Restart(SpriteSortMode.Immediate);
            DrawData data = new DrawData(
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
            Origins.blackHoleShade.UseColor(3, 0, 0);
            Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
            Origins.blackHoleShade.Apply(data);
            Main.EntitySpriteDraw(data);
            Main.spriteBatch.Restart();
            return false;
        }
    }
}
