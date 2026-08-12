using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen; 
public class Fast_Conveyor_Belt : ModTile {
	public sealed override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
		.WithOnAddRecipes(item => {
			Recipe.Create(item.type)
			.AddIngredient(TileItem.ItemType<Fast_Conveyor_Belt_CC>())
			.Register();
		}).RegisterItem();
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
		.WithOnAddRecipes(item => {
			Recipe.Create(item.type)
			.AddIngredient(TileItem.ItemType<Fast_Conveyor_Belt>())
			.Register();
		}).RegisterItem();
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
