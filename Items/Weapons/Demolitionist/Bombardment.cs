using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Bombardment : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bombardment");
			Tooltip.SetDefault("Releases a barrage of mines");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.damage = 13;
			Item.useTime = 6;
			Item.useAnimation = 36;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine>();
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.shootSpeed = 9;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 49);
			Item.UseSound = null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.15f);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
		}
	}
}
