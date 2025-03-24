using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.NPCs;

namespace Origins.CrossMod.Thorium.NPCs {
	[ExtendsFromMod(nameof(ThoriumMod))]
	public class OThoriumGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC?.Mod is ThoriumMod.ThoriumMod;
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == ModContent.NPCType<ConfusedZombie>()) {
				shop.InsertAfter<Strange_String>(ItemID.Vertebrae);
				shop.InsertAfter<Bud_Barnacle>(ItemID.Vertebrae);
				shop.InsertAfter<Biocomponent10>(ItemID.Vertebrae, Condition.DrunkWorld);
				shop.InsertAfter<Nerve_Impulse_Manipulator>(ItemID.BloodySpine, Condition.Hardmode);
				shop.InsertAfter<Sus_Ice_Cream>(ItemID.BloodySpine, Condition.Hardmode);
				shop.InsertAfter<Broken_Record>(ItemID.BloodySpine, Condition.Hardmode, Condition.DrunkWorld);
			}
		}
	}
}
