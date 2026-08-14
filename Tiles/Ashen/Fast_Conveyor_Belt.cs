using Origins.Items.Tools.Liquids;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen; 
public class Fast_Conveyor_Belt : ModTile {
	public static Action<Item> CreateRecipes<TOther>(int slowVersion) where TOther : ModTile => item => {
		const int count_per_bucket = 25;
		Recipe.Create(item.type)
		.AddIngredient(TileItem.ItemType<TOther>())
		.Register();

		Recipe.Create(item.type, count_per_bucket)
		.AddIngredient(slowVersion, count_per_bucket)
		.AddRecipeGroup(RecipeGroupID.IronBar)
		.AddIngredient<Oil_Bucket>()
		.AddOnCraftCallback(CraftingCallbacks.BucketCrafting<Oil_Bucket>)
		.Register();

		Recipe.Create(item.type, count_per_bucket)
		.AddIngredient(slowVersion, count_per_bucket)
		.AddRecipeGroup(RecipeGroupID.IronBar)
		.AddIngredient<Oil_Bottomless_Bucket>()
		.AddOnCraftCallback(CraftingCallbacks.NoConsumeCrafting<Oil_Bottomless_Bucket>)
		.Register();
	};
	public static void SharedDefaults(Item item) {
		item.value = Item.sellPrice(silver: 10);
		item.rare = ItemRarityID.Blue;
	}
	public static Condition HasFastConveyorBelt { get; private set; } = new Condition(
		Language.GetOrRegister("Mods.Origins.Items.Fast_Conveyor_Belt_Item.ConditionDescription"),
		() => Main.LocalPlayer.HasItemInAnyInventory((Item item) => item.type == TileItem.ItemType<Fast_Conveyor_Belt>() || item.type == TileItem.ItemType<Fast_Conveyor_Belt_CC>())
	);
	public sealed override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
		.WithExtraDefaults(SharedDefaults)
		.WithOnAddRecipes(CreateRecipes<Fast_Conveyor_Belt_CC>(ItemID.ConveyorBeltLeft))
		.RegisterItem();
	}
	public override void SetStaticDefaults() {
		Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
		Main.tileSolid[Type] = true;
		Main.tileMerge[Type][TileID.ConveyorBeltLeft] = true;
		Main.tileMerge[TileID.ConveyorBeltLeft][Type] = true;
		TileID.Sets.ConveyorDirection[Type] = 2;
		TileID.Sets.HasSlopeFrames[Type] = true;
		TileID.Sets.IsSkippedForNPCSpawningGroundTypeCheck[Type] = true;
		AddMapEntry(new(116, 111, 111));

		MinPick = 65;
		HitSound = SoundID.Tink;
		DustType = Ashen_Biome.DefaultTileDust;
		AnimationFrameHeight = 90;
		//Rarity = ItemRarityID.Blue;
	}
	public override void HitWire(int i, int j) {
		if (Main.tile[i, j].HasActuator) return;
		Main.tile[i, j].TileType = (ushort)TileType<Fast_Conveyor_Belt_CC>();
		WorldGen.SquareTileFrame(i, j);
		NetMessage.SendTileSquare(-1, i, j);
	}
	public override void AnimateTile(ref int frame, ref int frameCounter) {
		if (frameCounter.CycleUp(4)) frame.CycleUp(4);
	}
}
public class Fast_Conveyor_Belt_CC : ModTile {
	public sealed override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
		.WithExtraDefaults(Fast_Conveyor_Belt.SharedDefaults)
		.WithOnAddRecipes(Fast_Conveyor_Belt.CreateRecipes<Fast_Conveyor_Belt>(ItemID.ConveyorBeltRight))
		.RegisterItem();
	}
	public override void SetStaticDefaults() {
		Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
		Main.tileSolid[Type] = true;
		Main.tileMerge[Type][TileID.ConveyorBeltRight] = true;
		Main.tileMerge[TileID.ConveyorBeltRight][Type] = true;
		TileID.Sets.ConveyorDirection[Type] = -2;
		TileID.Sets.HasSlopeFrames[Type] = true;
		TileID.Sets.IsSkippedForNPCSpawningGroundTypeCheck[Type] = true;
		AddMapEntry(new(116, 111, 111));

		MinPick = 65;
		HitSound = SoundID.Tink;
		DustType = Ashen_Biome.DefaultTileDust;
		AnimationFrameHeight = 90;
	}
	public override void HitWire(int i, int j) {
		if (Main.tile[i, j].HasActuator) return;
		Main.tile[i, j].TileType = (ushort)TileType<Fast_Conveyor_Belt>();
		WorldGen.SquareTileFrame(i, j);
		NetMessage.SendTileSquare(-1, i, j);
	}
	public override void AnimateTile(ref int frame, ref int frameCounter) {
		if (frameCounter.CycleUp(4)) frame.CycleDownWithZero(4);
	}
}
