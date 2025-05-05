using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Extremophile {
	[AutoloadEquip(EquipType.Head)]
	public class Extremophile_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ArmorSet"
        ];
		public override void SetDefaults() {
			Item.defense = 11;
			Item.rare = ItemRarityID.Pink;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Extremophile_Breastplate>() && legs.type == ModContent.ItemType<Extremophile_Greaves>();
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().projectileSpeedBoost += 0.15f;
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Extremophile");
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.extremophileSet = true;
			player.statDefense += originPlayer.extremophileSetTime / 30;
		}
		public string ArmorSetName => "Extremophile_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Extremophile_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Extremophile_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Extremophile_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 13;
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.15f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Extremophile_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 11;
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += Collision.DrownCollision(player.position, player.width, player.height, player.gravDir) ? 0.35f : 0.25f;
			player.accFlipper = true;
		}
	}
}
