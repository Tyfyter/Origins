using CalamityMod.NPCs.TownNPCs;
using Newtonsoft.Json.Linq;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Other.Fish;
using Origins.Items.Weapons;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Greater_Summoning_Potion : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
			OriginsSets.Items.InfoAccessorySlots_IsAMechanicalAccessory[Type] = false;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.width = 14;
			Item.height = 24;
			Item.buffTime = 4 * 60 * 60;
			Item.rare = ItemRarityID.Blue;
			Item.buffType = Greater_Summoning_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			int stack = Item.stack;
			if (OriginsModIntegrations.CheckAprilFools()) Item.SetDefaults(Greater_Summoning_Potato.ID);
			Item.stack = stack;
		}
		public override void UpdateInventory(Player player) {
			int stack = Item.stack;
			if (OriginsModIntegrations.CheckAprilFools()) Item.SetDefaults(Greater_Summoning_Potato.ID);
			Item.stack = stack;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Toadfish>())
			.AddIngredient(ItemID.Moonglow)
			.AddTile(TileID.Bottles)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();
		}
	}
	public class Greater_Summoning_Potato : ModItem {
		public override string Texture => base.Texture.Replace("Potato", "Potion_AF");
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(216, 209, 135),
				new Color(116, 170, 45),
				new Color(181, 148, 58)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ContentSamples.CreativeResearchItemPersistentIdOverride[Type] = ModContent.ItemType<Greater_Summoning_Potion>();
			OriginsSets.Items.InfoAccessorySlots_IsAMechanicalAccessory[Type] = false;
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DefaultToFood(14, 24, Greater_Summoning_Buff.ID, 4 * 60 * 60);
			Item.shoot = Greater_Summoning_Potato_P.ID;
		}
		public override bool? CanBeChosenAsAmmo(Item weapon, Player player) => weapon.useAmmo == ModContent.ItemType<Potato>();
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			int stack = Item.stack;
			if (!OriginsModIntegrations.CheckAprilFools()) Item.SetDefaults(Greater_Summoning_Potion.ID);
			Item.stack = stack;
		}
		public override void UpdateInventory(Player player) {
			int stack = Item.stack;
			if (!OriginsModIntegrations.CheckAprilFools()) Item.SetDefaults(Greater_Summoning_Potion.ID);
			Item.stack = stack;
		}
		public override bool CanShoot(Player player) {
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Toadfish>())
			.AddIngredient(ItemID.Moonglow)
			.AddTile(TileID.Bottles)
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
}
