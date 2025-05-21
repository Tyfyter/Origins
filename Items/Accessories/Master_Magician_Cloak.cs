using Origins.Buffs;
using Origins.Items.Accessories;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front)]
	public class Master_Magician_Cloak : Lazy_Cloak {
		public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			base.SetDefaults();
			Item.shoot = Master_Magician_Cloak_P.ID;
			Item.value = Item.sellPrice(gold: 6);
			Item.buffType = Master_Magician_Cloak_Buff.ID;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			if (player.OriginPlayer().lazyCloakOffPlayer <= 0) {
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
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Otherworldly_Cloak>()
			.AddIngredient(ItemID.ManaCloak)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Master_Magician_Cloak_P : Lazy_Cloak_P {
		public static new int ID { get; private set; }
		public override int BuffID => Otherworldly_Cloak_Buff.ID;
	}
}
namespace Origins.Buffs {
	public class Master_Magician_Cloak_Buff : Lazy_Cloak_Buff {
		public static new int ID { get; private set; }
		public override string Texture => typeof(Lazy_Cloak_Buff).GetDefaultTMLName();
		public override IEnumerable<int> ProjectileTypes() => [
			Master_Magician_Cloak_P.ID
		];
	}
}