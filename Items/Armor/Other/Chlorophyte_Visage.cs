using CalamityMod.Graphics.Renderers;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Other {
    [AutoloadEquip(EquipType.Head)]
	public class Chlorophyte_Visage : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public override void SetDefaults() {
			Item.width = 26;
			Item.height = 24;
			Item.defense = 5;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Lime;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveBlastRadius += 0.35f;
			originPlayer.explosiveFuseTime *= 0.75f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ItemID.ChlorophytePlateMail && legs.type == ItemID.ChlorophyteGreaves;
		}
		public override void ArmorSetShadows(Player player) {
			if (player.body == ArmorIDs.Body.ChlorophytePlateMail && player.legs == ArmorIDs.Legs.ChlorophyteGreaves) {
				player.armorEffectDrawShadowSubtle = true;
			}
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Chlorophyte");
			player.AddBuff(BuffID.LeafCrystal, 18000);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string ArmorSetName => Name + "_Set";
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.ChlorophytePlateMail;
		public int LegsItemID => ItemID.ChlorophyteGreaves;
	}
}
