using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Acrid_Handcannon : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "HandCannon"
        };
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 72;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Acrid_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 6;
        }
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 18);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
	public class Acrid_Slug_P : Metal_Slug_P {
		public override string Texture => "Origins/Projectiles/Ammo/Acrid_Slug_P";
	}
}
