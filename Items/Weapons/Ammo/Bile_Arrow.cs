using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Bile_Arrow : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Arrow",
			"RasterSource"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenArrow);
			Item.maxStack = 9999;
			Item.damage = 15;
			Item.shoot = ModContent.ProjectileType<Bile_Arrow_P>();
			Item.shootSpeed = 4f;
			Item.knockBack = 2.2f;
			Item.value = Item.sellPrice(silver: 9, copper: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 150);
			recipe.AddIngredient(ItemID.WoodenArrow, 150);
			recipe.AddIngredient(ModContent.ItemType<Black_Bile>());
			recipe.Register();
		}
	}
	public class Bile_Arrow_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Bile_Arrow_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.penetrate = 1;
			Projectile.width = 14;
			Projectile.height = 32;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item171, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 25);
		}
	}
}
