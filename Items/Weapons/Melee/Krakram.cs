using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class Krakram : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Boomerang"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.damage = 25;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.shoot = ModContent.ProjectileType<Krakram_P>();
			Item.shootSpeed = 9.75f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Krakram_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Krakram";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.localAI[0] = 1;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.localAI[0] * 0.15f);
			Projectile.localAI[0] = (float)System.Math.Sin(Projectile.timeLeft);
			return true;
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
	}
}
