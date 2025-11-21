using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	public class Armor_Power_Up : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			ItemID.Sets.IsAPickup[Type] = true;
			ItemID.Sets.ItemIconPulse[Type] = true;
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Heart);
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			gravity = 0.2f;
			maxFallSpeed = 2;
			float num = Main.rand.Next(90, 111) * 0.01f;
			num *= (Main.essScale + 0.5f) / 2f;
			Lighting.AddLight(Item.Center, 0, 0.6f * num, num);
			Item.velocity *= 1.02f;
		}
		public override bool OnPickup(Player player) {
			SoundEngine.PlaySound(Origins.Sounds.PowerUp);
			player.Heal(350);
			return false;
		}
		public override Color? GetAlpha(Color lightColor) => Color.White * (Main.essScale + 0.5f);
		public override bool ItemSpace(Player player) => true;
	}
}
