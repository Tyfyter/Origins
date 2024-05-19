using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Tiles.Dusk;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Bleeding {
    [AutoloadEquip(EquipType.Head)]
	public class Bleeding_Obsidian_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => new string[] {
            "HardmodeArmorSet",
            "ExplosiveBoostGear",
			"SelfDamageProtek"
		};
        public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Bleeding/Bleeding_Obsidian_Helmet_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.2f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Bleeding_Obsidian_Breastplate>() && legs.type == ModContent.ItemType<Bleeding_Obsidian_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.BleedingObsidian");
			player.GetModPlayer<OriginPlayer>().bleedingObsidianSet = true;
		}
		public override void ArmorSetShadows(Player player) {
			if (Main.rand.NextBool(5)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.RedTorch, 0, 0, 100, new Color(255, 60, 30));
				dust.noGravity = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ObsidianHelm);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 2);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public string ArmorSetName => "Bleeding_Obsidian_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Bleeding_Obsidian_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Bleeding_Obsidian_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Bleeding_Obsidian_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Bleeding/Bleeding_Obsidian_Breastplate_Body_Glow");
				if (Mod.RequestAssetIfExists("Items/Armor/Bleeding/Bleeding_Obsidian_Breastplate_Cloth_Legs", out Asset<Texture2D> asset)) {
					Origins.TorsoLegLayers.Add(Item.bodySlot, asset);
				}
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 10;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.25f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ObsidianShirt);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 4);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Bleeding_Obsidian_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Bleeding/Bleeding_Obsidian_Greaves_Legs_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 9;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.15f;
			player.runAcceleration += 0.02f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ObsidianPants);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 3);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
