using Origins.Buffs;
using Origins.Items.Accessories;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front, EquipType.Back)]
	public class Master_Magician_Cloak : Lazy_Cloak {
		public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 50;
			Item.shoot = Master_Magician_Cloak_P.ID;
			Item.value = Item.sellPrice(gold: 6, silver: 35);
			Item.buffType = Master_Magician_Cloak_Buff.ID;
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
			player.manaFlower = true;
			player.manaCost -= 0.08f;
			player.starCloakItem = Item;
			player.starCloakItem_manaCloakOverrideItem = Item;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ManaCloak)
			.AddIngredient<Otherworldly_Cloak>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Master_Magician_Cloak_P : Lazy_Cloak_P {
		public static new int ID { get; private set; }
		public override int BuffID => Master_Magician_Cloak_Buff.ID;
	}
}
namespace Origins.Buffs {
	public class Master_Magician_Cloak_Buff : Lazy_Cloak_Buff {
		public static new int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Master_Magician_Cloak_P.ID
		];
	}
}