using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Cryosten {
	[AutoloadEquip(EquipType.Head)]
	public class Cryosten_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ArmorSet"
        ];
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
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Cryosten");
			player.GetModPlayer<OriginPlayer>().cryostenSet = true;
			if (player.HasBuff(BuffID.Chilled)) player.buffTime[player.FindBuffIndex(BuffID.Chilled)]--;
			if (player.HasBuff(BuffID.Frozen)) player.buffTime[player.FindBuffIndex(BuffID.Frozen)]--;
		}
		public override void ArmorSetShadows(Player player) {
			if (Main.rand.NextBool(5)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Snow);
				dust.noGravity = true;
				dust.velocity *= 0.1f;
			}
			if (player.GetModPlayer<OriginPlayer>().cryostenLifeRegenCount > 0) {
				player.armorEffectDrawOutlinesForbidden = true;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IceBlock, 45)
            .AddIngredient(ItemID.LifeCrystal)
            .AddIngredient(ItemID.Shiverthorn, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Cryosten_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Cryosten_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Cryosten_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Cryosten_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 7);
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += (int)(player.statLifeMax2 * 0.12);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IceBlock, 60)
            .AddIngredient(ItemID.LifeCrystal)
            .AddIngredient(ItemID.Shiverthorn, 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Cryosten_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(silver: 7);
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.05f;
			player.iceSkate = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.LifeCrystal)
            .AddIngredient(ItemID.Shiverthorn, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
