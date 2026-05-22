using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Head, EquipType.Body)]
	public class Exo_Suit : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement,
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public static int HeadID { get; private set; }
		public static int BodyID { get; private set; }
		public override void Load() {
			BodyID = EquipLoader.AddEquipTexture(Mod, $"{Texture}_Body", EquipType.Body, name: $"{Name}_Body");
			HeadID = EquipLoader.AddEquipTexture(Mod, $"{Texture}_Head", EquipType.Head, name: $"{Name}_Head");
		}
		public override void SetStaticDefaults() {
			Accessory_Glow_Layer.AddGlowMask(EquipType.Head, HeadID, $"{Texture}_Head_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.handOnSlot = Exo_Arm.HandsOnID;
			Item.shoeSlot = Exo_Legs.ShoeID;
			Item.backSlot = Exo_Weapon_Mount.BackID;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			Max(ref originPlayer.exoArmMult, 1);
			ModContent.GetInstance<Exo_Legs>().UpdateAccessory(player, hideVisual);
			Exo_Weapon_Mount.SetStrength(player, 0.8f);
		}
		public override void UpdateVisibleAccessory(Player player, bool hideVisual) {
			if (hideVisual) return;
			player.head = HeadID;
			player.body = BodyID;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Exo_Arm>()
			.AddIngredient<Exo_Legs>()
			.AddIngredient<Exo_Weapon_Mount>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
}
