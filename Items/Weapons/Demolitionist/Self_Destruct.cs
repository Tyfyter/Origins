using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Self_Destruct : ModItem {
		public string[] Categories => new string[] {
            "OtherExplosive"
        };
		public override void SetDefaults() {
			Item.DamageType = DamageClasses.Explosive;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.damage = 125;
			Item.useTime = 120;
			Item.useAnimation = 120;
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
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.width = 0;
			Projectile.height = 0;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.velocity.HasNaNs()) Projectile.velocity = player.velocity;
			Projectile.velocity *= 0.9f;
			player.MountedCenter = Projectile.position;
			player.velocity = Projectile.velocity;
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
		public override void PostDraw(Color lightColor) {
			float factor = (1 - Projectile.ai[1] / 180);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(factor, factor, factor, factor * 0.5f),
				Projectile.rotation,
				new Vector2(15, 13),
				Projectile.scale,
				SpriteEffects.None
			);
		}
	}
	public class Self_Destruct_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 5;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 608;
			Projectile.height = 608;
			Projectile.hide = true;
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
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
}
