using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Dusk;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Rift {
    [AutoloadEquip(EquipType.Head)]
	public class Rift_Helmet : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bleeding Obsidian Hardee");
			// Tooltip.SetDefault("Increased explosive velocity");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Rift/Rift_Helmet_Head_Glow");
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
			return body.type == ModContent.ItemType<Rift_Breastplate>() && legs.type == ModContent.ItemType<Rift_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Explosive projectiles have a chance to be duplicated";
			player.GetModPlayer<OriginPlayer>().riftSet = true;
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
	}
	[AutoloadEquip(EquipType.Body)]
	public class Rift_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bleeding Obsidian Trenchcoat");
			// Tooltip.SetDefault("-25% explosive self-damage");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Rift/Rift_Breastplate_Body_Glow");
				if (Mod.RequestAssetIfExists("Items/Armor/Rift/Rift_Trenchcoat_Cloth_Legs", out Asset<Texture2D> asset)) {
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
	public class Rift_Greaves : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bleeding Obsidian Chaps");
			// Tooltip.SetDefault("15% increased movement speed");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Rift/Rift_Greaves_Legs_Glow");
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
