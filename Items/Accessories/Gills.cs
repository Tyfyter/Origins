using Terraria.Audio;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Wings)]
public class Gills : ModItem {
	public override void SetStaticDefaults() {
		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new(60, 3.25f, hasHoldDownHoverFeatures: true);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Blue;
	}
	public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		const float braking_factor = 0.97f;
		if (player.TryingToHoverDown || player.wingTime <= 45) {
			player.velocity.Y *= braking_factor;
			player.wingTime += 0.5f;
			ascentWhenFalling = player.gravity + player.velocity.Y * 0.05f * player.gravDir;
			ascentWhenRising = -(player.gravity + player.velocity.Y * 0.05f * player.gravDir);
			constantAscend = -player.gravity;
		}
	}
}
