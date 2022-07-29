using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Cryosten {
    [AutoloadEquip(EquipType.Head)]
	public class Cryosten_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Helmet");
			Tooltip.SetDefault("Increased life regeneration");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 2;
		}
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().cryostenHelmet = true;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Cryosten_Breastplate>() && legs.type == ModContent.ItemType<Cryosten_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Life restoration from hearts increased. Immune to Chilled, Frozen, and Frostburn";
            player.GetModPlayer<OriginPlayer>().cryostenSet = true;
            if(player.HasBuff(BuffID.Chilled))player.buffTime[player.FindBuffIndex(BuffID.Chilled)]--;
            if(player.HasBuff(BuffID.Frozen))player.buffTime[player.FindBuffIndex(BuffID.Frozen)]--;
            if(player.HasBuff(BuffID.Frostburn))player.buffTime[player.FindBuffIndex(BuffID.Frostburn)]--;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IceBlock, 45);
            recipe.AddIngredient(ItemID.Shiverthorn, 8);
            recipe.AddIngredient(ItemID.LifeCrystal);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
	public class Cryosten_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Breastplate");
			Tooltip.SetDefault("20% increased maximum health");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 3;
		}
        public override void UpdateEquip(Player player) {
            player.statLifeMax2+=player.statLifeMax2/5;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IceBlock, 60);
            recipe.AddIngredient(ItemID.Shiverthorn, 12);
            recipe.AddIngredient(ItemID.LifeCrystal);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
	public class Cryosten_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Greaves");
			Tooltip.SetDefault("5% increased movement speed\nincreased movement speed on ice");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 2;
		}
        public override void UpdateEquip(Player player) {
            player.moveSpeed+=0.05f;
            player.iceSkate = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IceBlock, 30);
            recipe.AddIngredient(ItemID.Shiverthorn, 4);
            recipe.AddIngredient(ItemID.LifeCrystal);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
