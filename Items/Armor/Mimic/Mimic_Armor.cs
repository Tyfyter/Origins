using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Mimic {
	[AutoloadEquip(EquipType.Head)]
	public class Mimic_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mimic Helmet");
			Tooltip.SetDefault("Slightly increased explosive velocity");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Mimic/Mimic_Helmet_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 11;
			Item.rare = ItemRarityID.Pink;
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

			float defiledPercentage = 3f;//((OriginWorld.totalDefiled * 3) / (float)WorldGen.totalSolid);

			originPlayer.explosiveThrowSpeed += 0.2f * defiledPercentage;
			player.lifeRegenCount += (int)(4 * defiledPercentage);
			player.GetDamage(DamageClasses.Explosive) += 0.2f * defiledPercentage;

			player.setBonus = string.Format("Not yet fully implemented\nSet bonus scales with the percentage of the world taken over by the defiled wastelands\nCurrent percentage: {0:P1}, ", defiledPercentage/3);
			Origins.SetMimicSetUI();

			if (defiledPercentage >= 1) {
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
			if (defiledPercentage >= 2) {
				switch (originPlayer.GetMimicSetChoice(1)) {
					case 1:
					originPlayer.setActiveAbility = 1;
					break;
					case 2:
					originPlayer.setActiveAbility = 2;
					break;
					case 3:
					break;
				}
			}
			if (defiledPercentage >= 3) {
				switch (originPlayer.GetMimicSetChoice(2)) {
					case 1:
					player.extraAccessorySlots++;
					break;
					case 2:
					player.moveSpeed += 1.98f;
					player.runAcceleration += 0.099f;
					break;
					case 3:
					if (player.velocity.Y == 0) {
						player.GetDamage(DamageClass.Generic) *= 1.10f;
						player.GetAttackSpeed(DamageClass.Generic) *= 1.10f;
						player.GetCritChance(DamageClass.Generic) += 5;
						player.manaCost *= 0.7f;
					}
					break;
				}
			}
		}
		public override void AddRecipes() {
			/*Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
			//recipe.AddIngredient(ModContent.ItemType<>(), 10);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();*/
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Mimic_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mimic Breastplate");
			Tooltip.SetDefault("Increased life regeneration");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Mimic/Mimic_Breastplate_Body_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 15;
			Item.wornArmor = true;
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegenCount += 2;
		}
		public override void AddRecipes() {
			/*Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
			//recipe.AddIngredient(ModContent.ItemType<>(), 20);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();*/
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
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 12;
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.1f;
		}
		public override void AddRecipes() {
			/*Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
			//recipe.AddIngredient(ModContent.ItemType<>(), 15);
			recipe.SetResult(this);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();*/
		}
	}
}
