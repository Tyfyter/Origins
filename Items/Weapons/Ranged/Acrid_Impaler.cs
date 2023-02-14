﻿using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    //extends harpoon gun so it doesn't have to have redundant overrides for CanShoot, CanConsumeAmmo, etc.
    public class Acrid_Impaler : Harpoon_Gun {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Impaler");
			Tooltip.SetDefault("Uses harpoons as ammo\n85.7% chance not to consume ammo");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 33;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 58;
			Item.height = 22;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 14.75f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.buyPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(7)) {
				ammo.stack++;
			} else {
				consume = true;
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Item.shoot) {
				type = Acid_Harpoon_P.ID;
			}
		}
	}
}