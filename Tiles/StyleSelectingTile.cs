using Origins.Tiles.Ashen;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles; 
//TODO: move to PegasusLib
public abstract class StyleSelectingTileItem : ModItem {
	protected override bool CloneNewInstances => true;
	[CloneByReference]
	protected StyleSelectingTile[] tiles;
	int index = 0;
	public sealed override void SetStaticDefaults() {
		tiles = SetupTiles().ToArray();
		OnSetStaticDefaults();
	}
	public sealed override void SetDefaults() {
		Item.DefaultToPlaceableTile(tiles?[index].Type ?? TileID.TargetDummy);
		Item.width = 24;
		Item.height = 24;
		OnSetDefaults();
	}
	public virtual void OnSetStaticDefaults() { }
	public virtual void OnSetDefaults() { }
	public override void HoldItem(Player player) {
		if (PlayerInput.Triggers.JustPressed.Up && Item.placeStyle.CycleUp(tiles[index].StyleCount)) {
			index.CycleUp(tiles.Length);
		} else if (PlayerInput.Triggers.JustPressed.Down && (--Item.placeStyle < 0)) {
			index.CycleDownWithZero(tiles.Length);
			Item.placeStyle = tiles[index].StyleCount - 1;
		}
		Item.createTile = tiles[index].Type;
	}
	protected abstract IEnumerable<StyleSelectingTile> SetupTiles();
	protected static StyleSelectingTile Tile<T>() where T : ModTile, IStyleSelectingTile => ModContent.GetInstance<T>();
	protected readonly struct StyleSelectingTile : IStyleSelectingTile {
		public ModTile Tile { get; private init; }
		public int Type => Tile.Type;
		public int StyleCount => ((IStyleSelectingTile)Tile).StyleCount;
		public static implicit operator StyleSelectingTile(ModTile tile) => tile is IStyleSelectingTile ?
			new StyleSelectingTile() { Tile = tile }
			: throw new ArgumentException($"Provided tile must implement {nameof(IStyleSelectingTile)}", nameof(tile));
	}
}
public interface IStyleSelectingTile {
	public int StyleCount { get; }
}