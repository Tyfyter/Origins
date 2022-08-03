using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace Origins.Items.Other.Testing {
	/// <summary>
	/// can be used to add NPCs to an NPC drop table
	/// </summary>
	public class NPC_Spawner : ModItem {
		public override string Texture => "Terraria/Images/Extra_74";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("NPC Spawner");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Master;
			Item.maxStack = NPCLoader.NPCCount;
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.NewNPC(new EntitySource_Misc("PlayerDropItemCheck"), (int)Item.position.X, (int)Item.position.Y, Item.stack);
			Item.active = false;
		}
		public override bool CanPickup(Player player) => false;
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (Item.timeSinceItemSpawned > 15) {
				NPC.NewNPC(new EntitySource_Misc("PlayerDropItemCheck"), (int)Item.position.X, (int)Item.position.Y, Item.stack);
				Item.active = false;
			}
		}
	}
}
