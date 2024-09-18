using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Felnum_Longbow : ModItem, ICustomWikiStat {
		public const int baseDamage = 19;
        public string[] Categories => [
            "Bow"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldBow);
			Item.damage = baseDamage;
			Item.width = 18;
			Item.height = 58;
			Item.useTime = Item.useAnimation = 32;
			Item.shootSpeed *= 2.5f;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
	}
}
