using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;

namespace Origins.Items.Weapons.Demolitionist {
    public class Boomphracken : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "HandCannon"
        };
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 80;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Boomphracken_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 10f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 8;
        }
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IllegalGunParts, 2);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Hallowed_Cleaver>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
	public class Boomphracken_P : Metal_Slug_P {
		public override string Texture => "Origins/Projectiles/Ammo/Boomphracken_P";
	}
}
