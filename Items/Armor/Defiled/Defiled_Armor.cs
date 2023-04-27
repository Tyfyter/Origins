using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Defiled {
	[AutoloadEquip(EquipType.Head)]
	public class Defiled_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Helmet");
			Tooltip.SetDefault("Increased mana regeneration rate");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Defiled/Defiled_Helmet_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.buyPrice(gold: 1, silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.manaRegen += 2;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Defiled_Breastplate>() && legs.type == ModContent.ItemType<Defiled_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "15% of damage taken is redirected to mana";
			player.GetModPlayer<OriginPlayer>().defiledSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Defiled_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Breastplate");
			Tooltip.SetDefault("15% increased magic damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.buyPrice(gold: 1, silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Magic) += 0.15f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Defiled_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Greaves");
			Tooltip.SetDefault("Increased movement speed");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.buyPrice(gold: 1, silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.06f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Defiled2_Helmet : Defiled_Helmet {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient {$Defiled} Helmet");
		}
		[AutoloadEquip(EquipType.Body)]
		public class Defiled2_Breastplate : Defiled_Breastplate {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient {$Defiled} Breastplate");
			}
		}
		[AutoloadEquip(EquipType.Legs)]
		public class Defiled2_Greaves : Defiled_Greaves {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient {$Defiled} Greaves");
			}
		}
	}
}
namespace Origins.Buffs {
	public class Defiled_Exhaustion_Buff : ModBuff {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$Defiled} Exhaustion");
		}
		public override void Update(Player player, ref int buffIndex) {
			player.manaRegenBuff = false;
			player.manaRegen = 0;
			player.manaRegenCount = 0;
			player.manaRegenBonus = 0;
		}
	}
}
