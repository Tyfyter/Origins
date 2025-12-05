using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Cryosten {
	[AutoloadEquip(EquipType.Head)]
	public class Cryosten_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.ArmorSet
        ];
        public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().explosiveBlastRadius += 0.2f;
			player.lifeRegen += 4;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			if (body.type != ModContent.ItemType<Cryosten_Breastplate>() && body.type != ModContent.ItemType<Pink_Cryosten_Breastplate>()) return false;
			return legs.type == ModContent.ItemType<Cryosten_Greaves>() || legs.type == ModContent.ItemType<Pink_Cryosten_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Cryosten");
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.cryostenSet = true;
			originPlayer.explosiveProjectileSpeed += 0.1f;
			player.buffImmune[BuffID.Chilled] = true;
			player.buffImmune[BuffID.Frozen] = true;
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
			.AddIngredient(ItemID.EskimoHood)
			.AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.LifeCrystal)
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
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.cryostenBody = true;
			originPlayer.explosiveBlastRadius += 0.2f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.EskimoCoat)
			.AddIngredient(ItemID.IceBlock, 40)
			.AddIngredient(ItemID.LifeCrystal)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Cryosten_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.1f;
			if (player.slippy) {
				player.moveSpeed += 0.05f;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.EskimoPants)
			.AddIngredient(ItemID.IceBlock, 20)
			.AddIngredient(ItemID.LifeCrystal)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Pink_Cryosten_Helmet : Cryosten_Helmet {
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PinkEskimoHood)
			.AddIngredient(ItemID.IceBlock, 30)
			.AddIngredient(ItemID.LifeCrystal)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Pink_Cryosten_Breastplate : Cryosten_Breastplate {
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PinkEskimoCoat)
			.AddIngredient(ItemID.IceBlock, 40)
			.AddIngredient(ItemID.LifeCrystal)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Pink_Cryosten_Greaves : Cryosten_Greaves {
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.PinkEskimoPants)
			.AddIngredient(ItemID.IceBlock, 20)
			.AddIngredient(ItemID.LifeCrystal)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
