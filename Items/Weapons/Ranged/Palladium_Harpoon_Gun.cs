using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Palladium_Harpoon_Gun : Harpoon_Gun {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
				if (proj.TryGetGlobalProjectile(out HarpoonGlobalProjectile harpoonGlobal)) harpoonGlobal.extraGravity.Y -= 0.21f;
			});
		}
		public override void SetDefaults() {
			DefaultToHarpoonGun();
			Item.damage = 50;
			Item.knockBack = 5;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.reuseDelay = 2;
			Item.width = 56;
			Item.height = 26;
			Item.shootSpeed = 11.75f;
			Item.value = Item.sellPrice(gold: 1, silver: 60);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PalladiumBar, 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(7);
		}
	}
}
