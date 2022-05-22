using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.World;

namespace Origins.Items.Armor.Mimic {
	[AutoloadEquip(EquipType.Head)]
	public class Mimic_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mimic Helmet");
			Tooltip.SetDefault("Slightly increased explosive velocity");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(item.headSlot, "Items/Armor/Mimic/Mimic_Helmet_Head_Glow");
			}
		}
		public override void SetDefaults() {
			item.defense = 11;
			item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.1f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Mimic_Breastplate>() && legs.type == ModContent.ItemType<Mimic_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.mimicSet = true;
			float defiledPercentage = OriginWorld.totalDefiled / (float)WorldGen.totalSolid;
			originPlayer.explosiveThrowSpeed += 0.2f * defiledPercentage;
			player.lifeRegenCount += (int)(4 * defiledPercentage);
			originPlayer.explosiveDamage += 0.2f * defiledPercentage;
			player.setBonus = string.Format("Not yet fully implemented\nSet bonus scales with the percentage of the world taken over by the defiled wastelands\nCurrent percentage: {0:P1}, ", defiledPercentage);
			Origins.instance.SetMimicSetUI();
			if (defiledPercentage > 0.33) {
				switch (originPlayer.GetMimicSetChoice(0)) {
					case 1:
					if (player.wings == 0) {
						player.wings = 8;
					}
					if (player.wingsLogic == 0 || player.wingTimeMax <= 180) {
						player.wingsLogic = 26;
						player.wingTimeMax = 180;
					}
					break;
					case 2:
					break;
					case 3:
					break;
				}
			}
			if (defiledPercentage > 0.66) {
				switch (originPlayer.GetMimicSetChoice(0)) {
					case 1:
					player.moveSpeed += 0.66f;
					player.runAcceleration += 0.066f;
					break;
					case 2:
					break;
					case 3:
					break;
				}
			}
			if (defiledPercentage > 0.99) {
				switch (originPlayer.GetMimicSetChoice(0)) {
					case 1:
					player.extraAccessorySlots++;
					break;
					case 2:
					break;
					case 3:
					break;
				}
			}
		}
		public override void AddRecipes() {
			/*ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
			//recipe.AddIngredient(ModContent.ItemType<>(), 10);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipe();*/
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Mimic_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mimic Breastplate");
			Tooltip.SetDefault("Increased life regeneration");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(item.bodySlot, "Items/Armor/Mimic/Mimic_Breastplate_Body_Glow");
				Origins.AddBreastplateGlowmask(-item.bodySlot, "Items/Armor/Mimic/Mimic_Breastplate_FemaleBody_Glow");
			}
		}
		public override void SetDefaults() {
			item.defense = 15;
			item.wornArmor = true;
			item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegenCount += 2;
		}
		public override void AddRecipes() {
			/*ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
			//recipe.AddIngredient(ModContent.ItemType<>(), 20);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipe();*/
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Mimic_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mimic Greaves");
			Tooltip.SetDefault("10% increased explosive damage");
			if (Main.netMode != NetmodeID.Server) {
				//Origins.AddLeggingGlowMask(item.legSlot, "Items/Armor/Mimic/Mimic_Greaves_Legs_Glow");
			}
		}
		public override void SetDefaults() {
			item.defense = 12;
			item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveDamage += 0.1f;
		}
		public override void AddRecipes() {
			/*ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
			//recipe.AddIngredient(ModContent.ItemType<>(), 15);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.AddRecipe();*/
		}
	}
}
