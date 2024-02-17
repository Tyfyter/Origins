using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
    public class Traffic_Cone : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(200, 80, 0), Language.GetOrRegister(this.GetLocalizationKey("DisplayName")));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<Traffic_Cone_TE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
		}
	}
	public class Traffic_Cone_TE : ModTileEntity {
		public static new int ID { get; private set; } = -1;
		public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].TileIsType(ModContent.TileType<Traffic_Cone>());
		public override void OnNetPlace() {
			// This hook is only ever called on the server; its purpose is to give more freedom in terms of syncing FROM the server to clients, which we take advantage of
			// by making sure to sync whenever this hook is called:
			NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
		}
		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			return Place(i, j);
		}
		public static void UpdateCones() {
			const float range = 12 * 16; // 12 tiles
			if (ID == -1) ID = ModContent.TileEntityType<Traffic_Cone_TE>();
			foreach (TileEntity entity in ByID.Values) {
				if (entity.type == ID) {
					Vector2 pos = entity.Position.ToWorldCoordinates();
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC npc = Main.npc[i];
						if (npc.CanBeChasedBy(entity) && npc.DistanceSQ(pos) < range * range) {
							npc.AddBuff(Slow_Debuff.ID, 10);
						}
					}
				}
			}
		}
	}
	public class Traffic_Cone_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LampPost);
			Item.createTile = ModContent.TileType<Traffic_Cone>();
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 6);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
}
