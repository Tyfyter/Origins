using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Kruncher : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            WikiCategories.Gun
        ];
        public override void SetStaticDefaults() {
			Origins.FlatDamageMultiplier[Type] = 3f / 8f;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 4;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.crit = -1;
			Item.width = 56;
			Item.height = 18;
			Item.useTime = 27;
			Item.useAnimation = 27;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shootSpeed = 15f;
			Item.shoot = ProjectileID.Bullet;
			Item.useAmmo = AmmoID.Bullet;
			Item.useTurn = false;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.05f);
			Item.glowMask = glowmask;
			Item.autoReuse = true;
			Item.ArmorPenetration = 6;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 5; i++) Projectile.NewProjectile(source, position, velocity.RotatedByRandom(i / 10f), type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IllegalGunParts)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 6)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 3)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
