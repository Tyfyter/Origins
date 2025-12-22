using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Small_Storage_Container : ModChest {
		TileItem item;
		public override void Load() {
			Mod.AddContent(item = new TileItem(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddIngredient(ModContent.ItemType<Scrap>(), 8)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}));
			On_WorldGen.WouldTileReplacementWork += On_WorldGen_WouldTileReplacementWork;
		}
		bool On_WorldGen_WouldTileReplacementWork(On_WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y) {
			if (attemptingToReplaceWith == Type && Main.tile[x, y].TileType == attemptingToReplaceWith) return false;
			return orig(attemptingToReplaceWith, x, y);
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileShine[Type] = 12000;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			for (int i = TileObjectData.GetTileData(Type, 0).RandomStyleRange; i > 0; i--) {
				AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			}
			AdjTiles = [TileID.Containers];
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(item.Type);
		}
		public override void ModifyTileData() => TileObjectData.newTile.RandomStyleRange = 3;
		public override bool IsLockedChest(int i, int j) => false;
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) => drawData.addFrY = 0;
	}
}
