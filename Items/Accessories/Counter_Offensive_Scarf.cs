using Microsoft.Xna.Framework;
using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Counter_Offensive_Scarf : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality,
			WikiCategories.Combat,
			WikiCategories.GenericBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Comb>())
			.AddIngredient(ItemID.WormScarf)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			const float min_distance = 4 * 16;
			const float max_distance = 20 * 16;
			float bestDist = max_distance * max_distance;
			Vector2 center = player.MountedCenter;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy()) continue;
				float currentDist = npc.DistanceSQ(center);
				if (currentDist < bestDist) {
					bestDist = currentDist;
					if (bestDist <= min_distance * min_distance) break;
				}
			}
			float distance = (MathF.Sqrt(bestDist) - min_distance) / (max_distance - min_distance);
			if (distance < 0) distance = 0;

			player.endurance += (1 - player.endurance) * distance * 0.2f;
			player.GetDamage(DamageClass.Generic) += (1 - distance) * 0.1f;
		}
	}
}
