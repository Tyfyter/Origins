using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib;
using Terraria.Utilities;

namespace Origins.Items.Other.Testing {
	/// <summary>
	/// can be used to add NPCs to an NPC drop table
	/// </summary>
	public class NPC_Spawner : TestingItem {
		public override string Texture => "Terraria/Images/Extra_74";
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.useStyle = ItemUseStyleID.Guitar;
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Master;
			Item.maxStack = NPCLoader.NPCCount - 1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is not EntitySource_Misc miscSource || miscSource.Context != "ThrowItem") {
				NPC.NewNPC(source, (int)Item.position.X, (int)Item.position.Y, Item.stack);
				Item.active = false;
				Item.stack = 0;
			}
		}
		public override bool CanPickup(Player player) => false;
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (Item.timeSinceItemSpawned > 15) {
				if (Item.stack < NPCLoader.NPCCount) NPC.NewNPC(new EntitySource_Misc("ThrowItem"), (int)Item.position.X, (int)Item.position.Y, Item.stack);
				Item.active = false;
			}
		}
		public override bool? UseItem(Player player) {
			Point playerTile = player.Bottom.ToTileCoordinates();
			Point npcTile = Main.MouseWorld.ToTileCoordinates();
			NPCSpawnInfo spawnInfo = new() {
				PlayerFloorX = playerTile.X,
				PlayerFloorY = playerTile.Y + 1,
				Player = player,
				SpawnTileX = npcTile.X,
				SpawnTileY = npcTile.Y + 1,
				SpawnTileType = ModContent.TileType<Spug_Flesh>()
			};
			(int x, int y) = (spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
			(Tile tile1, Tile tile2) = (Main.tile[x, y + 1], Main.tile[x, y + 2]);
			bool isWet = tile1.LiquidAmount > 0 && tile2.LiquidAmount > 0 && (tile1.LiquidType == LiquidID.Honey || tile1.LiquidType == LiquidID.Water);
			spawnInfo.Water = isWet;
			WeightedRandom<int> pool = new(Main.rand);
			SpawnPool selectedPool = ModContent.GetInstance<Riven_Hive.SpawnRates>();
			for (int k = 0; k < selectedPool.Spawns.Count; k++) {
				(int npcType, SpawnRate condition) = selectedPool.Spawns[k];
				if (npcType != Type) {
					pool.Add(npcType, condition.Rate(spawnInfo));
				}
			}
			NPCLoader.GetNPC(pool.Get()).SpawnNPC((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
			return true;
		}
	}
}
