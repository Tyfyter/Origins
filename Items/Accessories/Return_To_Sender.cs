using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Return_To_Sender : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Return To Sender");
			Tooltip.SetDefault("Attackers receive all damage inflicted to the user");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.shoot = ModContent.ProjectileType<Return_To_Sender_Thorns>();
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.thorns += 1f;
			player.GetModPlayer<OriginPlayer>().thornsVisualProjType = Item.shoot;
		}
	}
	public class Return_To_Sender_Thorns : ModProjectile {
		const int frames = 3;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frames;
		}
		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 144 / frames;
		}
		public override void AI() {
			Projectile.Center = Main.player[Projectile.owner].MountedCenter;
			Projectile.timeLeft = 5;
			if (++Projectile.frameCounter >= 4) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % frames;
				if (Projectile.frame == 0) Projectile.Kill();
			}
		}
	}
}
