using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
    public class Magma_Grappler : Harpoon_Gun {
        public override void SetDefaults() {
			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.reuseDelay = 2;
			Item.width = 58;
			Item.height = 22;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 15.25f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.HellstoneBar, 12)
			.AddIngredient(ModContent.ItemType<Harpoon_Gun>())
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(5);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Item.shoot) {
				type = Flammable_Harpoon_P.ID;
			}
		}
	}
}
