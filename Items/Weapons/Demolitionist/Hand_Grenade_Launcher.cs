using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ShootAction = System.Action<Terraria.Player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float>;

namespace Origins.Items.Weapons.Demolitionist {
	[ReinitializeDuringResizeArrays]
	public class Hand_Grenade_Launcher : ModItem {
		public static ShootAction[] AltFireAction = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltFireAction)}")
		.RegisterCustomSet<ShootAction>(null);
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(16, 50, 44, 18);
			Item.shoot = ProjectileID.Grenade;
			Item.useAmmo = ItemID.Grenade;
			Item.shootSpeed = 5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			if (player.altFunctionUse == 2 && AltFireAction[ammo.shoot] is null) return false;
			return base.CanChooseAmmo(ammo, player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2 && AltFireAction[type] is ShootAction shootAction) {
				shootAction(player, source, position, velocity, type, damage, knockback);
				return false;
			}
			return true;
		}
	}
}
