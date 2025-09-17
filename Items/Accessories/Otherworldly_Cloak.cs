using Origins.Buffs;
using Origins.Items.Accessories;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front, EquipType.Back)]
	public class Otherworldly_Cloak : Lazy_Cloak {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 30;
			Item.shoot = Otherworldly_Cloak_P.ID;
			Item.value = Item.sellPrice(gold: 3, silver: 35);
			Item.buffType = Otherworldly_Cloak_Buff.ID;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			player.jumpSpeedBoost += 12;
			player.noFallDmg = true;
			if (player.controlJump) {
				player.gravity = 0.15f;
			} else if (player.controlDown && player.velocity.Y != 0f) {
				player.gravity = 1.4f;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Feather_Cape>()
			.AddIngredient<Lazy_Cloak>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Otherworldly_Cloak_P : Lazy_Cloak_P {
		public static new int ID { get; private set; }
		public override int BuffID => Otherworldly_Cloak_Buff.ID;
	}
}
namespace Origins.Buffs {
	public class Otherworldly_Cloak_Buff : Lazy_Cloak_Buff {
		public static new int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Otherworldly_Cloak_P.ID
		];
	}
}