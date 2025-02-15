using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
namespace Origins.Items.Other.Testing {
	/// <summary>
	/// can be used to add NPCs to an NPC drop table
	/// </summary>
	public class NPC_Spawner : ModItem {
		public override string Texture => "Terraria/Images/Extra_74";
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
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
	}
}
