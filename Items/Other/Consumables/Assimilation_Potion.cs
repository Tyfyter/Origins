using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public abstract class Assimilation_Potion<TAssimilation> : ModItem where TAssimilation : AssimilationDebuff {
		[field: CloneByReference]
		public readonly Assimilation_Potion_Player player = new();
		protected override bool CloneNewInstances => true;
		public override LocalizedText Tooltip => Mod.GetLocalization($"{LocalizationCategory}.Assimilation_Potion.{nameof(Tooltip)}").WithFormatArgs(10, ModContent.GetInstance<TAssimilation>().FullName);
		public sealed override void Load() {
			Mod.AddContent(player);
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public sealed override void SetDefaults() {
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.width = 14;
			Item.height = 24;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 5);
			PostSetDefaults();
		}
		public virtual void PostSetDefaults() { }
		public override bool? UseItem(Player player) {
			player.GetModPlayer(this.player).remainingAssimilation += 10;
			return true;
		}
		public class Assimilation_Potion_Player : ModPlayer {
			public override string Name => $"{ModContent.GetInstance<TAssimilation>().Name}_{base.Name}";
			public float remainingAssimilation = 0;
			public override void ResetEffects() {
				if (remainingAssimilation != 0) {
					float oldValue = remainingAssimilation;
					MathUtils.LinearSmoothing(ref remainingAssimilation, 0, 1f / 10);
					Player.GetAssimilation<TAssimilation>().Percent += 0.01f * (oldValue - remainingAssimilation);
				}
			}
			public override void UpdateDead() {
				remainingAssimilation = 0;
			}
		}
	}
	public class Bad_Juju_Stone : Assimilation_Potion<Corrupt_Assimilation> {
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.RottenChunk, 5)
			.AddIngredient(ItemID.UnholyWater)
			.AddIngredient(ItemID.VileMushroom, 2)
			.AddTile(TileID.DemonAltar)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();
			CreateRecipe()
			.AddIngredient(ItemID.RottenChunk, 5)
			.AddIngredient(ItemID.UnholyWater)
			.AddIngredient(ItemID.VileMushroom, 2)
			.AddTile<Cleansing_Station>()
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
	public class Contaminated_Infusion : Assimilation_Potion<Crimson_Assimilation> {
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BloodWater)
			.AddIngredient(ItemID.Vertebrae, 5)
			.AddIngredient(ItemID.ViciousMushroom, 2)
			.AddTile(TileID.DemonAltar)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();
			CreateRecipe()
			.AddIngredient(ItemID.BloodWater)
			.AddIngredient(ItemID.Vertebrae, 5)
			.AddIngredient(ItemID.ViciousMushroom, 2)
			.AddTile<Cleansing_Station>()
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
	public class Pen_Ink : Assimilation_Potion<Defiled_Assimilation> {
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Soulspore_Item>(2)
			.AddIngredient<Strange_String>(5)
			.AddIngredient<White_Water>()
			.AddTile(TileID.DemonAltar)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();
			CreateRecipe()
			.AddIngredient<Soulspore_Item>(2)
			.AddIngredient<Strange_String>(5)
			.AddIngredient<White_Water>()
			.AddTile<Cleansing_Station>()
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
	public class Edible_Hair_Gel : Assimilation_Potion<Riven_Assimilation> {
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Bud_Barnacle>(5)
			.AddIngredient<Gooey_Water>()
			.AddIngredient<Wrycoral_Item>(2)
			.AddTile(TileID.DemonAltar)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();
			CreateRecipe()
			.AddIngredient<Bud_Barnacle>(5)
			.AddIngredient<Gooey_Water>()
			.AddIngredient<Wrycoral_Item>(2)
			.AddTile<Cleansing_Station>()
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
}
