using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Adamantite_Harpoon_Gun : Harpoon_Gun {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
				global.extraGravity.Y -= 0.24f;
			});
		}
		public override void SetDefaults() {
			Item.damage = 59;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 56;
			Item.height = 26;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 13.75f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.AdamantiteBar, 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(6);
		}
	}
}
