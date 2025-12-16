using Origins.Items.Accessories;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using static Tyfyter.Utils.ChestLootCache;
using static Tyfyter.Utils.ChestLootCache.LootQueueAction;

namespace Origins.World {
    public class ChestLoot : ILoadable, IItemObtainabilityProvider {
		public static (LootQueueAction action, int param, float weight)[] Actions => [
			(CHANGE_QUEUE, ChestID.Normal, 0b0000),
			(ENQUEUE, ModContent.ItemType<Cyah_Nara>(), 1f),
			(SET_COUNT_RANGE, 50, 186),
			(ENQUEUE, ModContent.ItemType<Bang_Snap>(), 1f),
			(SET_COUNT_RANGE, 1, 1),
			(CHANGE_QUEUE, ChestID.LivingWood, 0b0000),
			(ENQUEUE, ModContent.ItemType<Woodsprite_Staff>(), 1f),

			(CHANGE_QUEUE, ChestID.LockedShadow, 0b0000),
			(ENQUEUE, ModContent.ItemType<Boiler>(), 0.5f),
			(ENQUEUE, ModContent.ItemType<Firespit>(), 0.5f),
			(ENQUEUE, ModContent.ItemType<Dragons_Breath>(), 0.5f),
			(ENQUEUE, ModContent.ItemType<Hand_Grenade_Launcher>(), 0.5f),

			(CHANGE_QUEUE, ChestID.Ice, 0b0000),
			(ENQUEUE, ModContent.ItemType<Cryostrike>(), 1f),

			(CHANGE_QUEUE, ChestID.Gold, 0b0101),
			(ENQUEUE, ModContent.ItemType<Bomb_Charm>(), 1f),
			(ENQUEUE, ModContent.ItemType<Beginners_Tome>(), 1f),

			(CHANGE_QUEUE, ChestID.Gold, 0b1101),
			(ENQUEUE, ModContent.ItemType<Nitro_Crate>(), 1f),
			(ENQUEUE, ModContent.ItemType<Bomb_Charm>(), 1f),
			(ENQUEUE, ModContent.ItemType<Beginners_Tome>(), 1f),
			(ENQUEUE, ModContent.ItemType<Broken_Terratotem>(), 0.75f),

			(CHANGE_QUEUE, ChestID.DeadMan, 0b0000),
			(ENQUEUE, ModContent.ItemType<Magic_Tripwire>(), 1f),
			(ENQUEUE, ModContent.ItemType<Trap_Charm>(), 1f),

			(CHANGE_QUEUE, ChestID.LockedGold, 0b0000),
			(ENQUEUE, ModContent.ItemType<Tones_Of_Agony>(), 1f),
			(ENQUEUE, ModContent.ItemType<Asylum_Whistle>(), 1f),
			(ENQUEUE, ModContent.ItemType<Bomb_Launcher>(), 1f),
			(ENQUEUE, ModContent.ItemType<Bomb_Handling_Device>(), 1f),

			(CHANGE_QUEUE, ChestID.LockedHallow, 0b0000),
			(ENQUEUE, ModContent.ItemType<The_Calibrator>(), 1f),

			(CHANGE_QUEUE, ChestID.Gold, 0b0011), 
			(ENQUEUE, ModContent.ItemType<Desert_Crown>(), 0.3f),

			(CHANGE_QUEUE, ChestID.Water, 0b0000),
			(ENQUEUE, ModContent.ItemType<Ocean_Amulet>(), 1f)
		];
		public IEnumerable<int> ProvideItemObtainability() => Actions.Where(a => a.action == ENQUEUE).Select(a => a.param);
		public void Load(Mod mod) { }
		public void Unload() { }
	}
}
