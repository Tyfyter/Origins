using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Cryosten {
	[AutoloadEquip(EquipType.Head)]
	public class Cryosten_Helmet : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cryosten Helmet");
			// Tooltip.SetDefault("Increased life regeneration");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 7);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().cryostenHelmet = true;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Cryosten_Breastplate>() && legs.type == ModContent.ItemType<Cryosten_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Life restoration from hearts increased. Resistance to Chilled, Frozen, and Frostburn";
			player.GetModPlayer<OriginPlayer>().cryostenSet = true;
			if (player.HasBuff(BuffID.Chilled)) player.buffTime[player.FindBuffIndex(BuffID.Chilled)]--;
			if (player.HasBuff(BuffID.Frozen)) player.buffTime[player.FindBuffIndex(BuffID.Frozen)]--;
			if (player.HasBuff(BuffID.Frostburn)) player.buffTime[player.FindBuffIndex(BuffID.Frostburn)]--;
		}
		public override void ArmorSetShadows(Player player) {
			if (Main.rand.NextBool(5)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Snow);
				dust.noGravity = true;
				dust.velocity *= 0.1f;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IceBlock, 45);
			recipe.AddIngredient(ItemID.Shiverthorn, 4);
			recipe.AddIngredient(ItemID.LifeCrystal);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Cryosten_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cryosten Breastplate");
			// Tooltip.SetDefault("12% increased maximum life");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 7);
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += (int)(player.statLifeMax2 * 0.12);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IceBlock, 60);
			recipe.AddIngredient(ItemID.Shiverthorn, 6);
			recipe.AddIngredient(ItemID.LifeCrystal);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Cryosten_Greaves : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cryosten Greaves");
			// Tooltip.SetDefault("5% increased movement speed\nIncreased movement speed on ice");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 7);
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.05f;
			player.iceSkate = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IceBlock, 30);
			recipe.AddIngredient(ItemID.Shiverthorn, 2);
			recipe.AddIngredient(ItemID.LifeCrystal);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
