using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Riven_Splitter : Harpoon_Gun {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Riven Splitter");
			// Tooltip.SetDefault("Uses harpoons as ammo\n87.5% chance not to consume ammo");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 24;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 56;
			Item.height = 26;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 18.75f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(8)) {
				ammo.stack++;
			} else {
				consume = true;
			}
		}
	}
}
