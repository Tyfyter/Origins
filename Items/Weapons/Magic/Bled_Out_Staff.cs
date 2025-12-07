using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Bled_Out_Staff : ModItem {
        public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 45;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 16;
			Item.shoot = ModContent.ProjectileType<Bled_Out_Staff_P>();
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddRecipeGroup("Origins:Gem Staves")
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Bled_Out_Staff_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_125";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DiamondBolt);
			Projectile.penetrate = 2;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GemDiamond, 0f, 0f, 0, new Color(255, 0, 255), 1f);
			dust.noGravity = true;
			dust.velocity /= 2;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Collision.HitTiles(Projectile.oldPosition, oldVelocity, Projectile.width, Projectile.height);
			return true;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			Projectile.velocity *= 0.5f;
			for (int i = 0; i < 8; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GemDiamond, Projectile.velocity.X, Projectile.velocity.Y, 0, new Color(255, 0, 255), 1f);
				dust.noGravity = true;
			}
		}
	}
}
