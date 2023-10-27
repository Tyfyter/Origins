using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles {
	public class TreeShaking : ILoadable, IItemObtainabilityProvider {
		public delegate double WeightProvider(ITree tree);
		public record struct TreeShakeLoot(int Type, WeightProvider Weight, int Min = 1, int Max = 1);
		public static (float chance, TreeShakeLoot[])[] ShakeLoot => _shakeLoot ??= new (float chance, TreeShakeLoot[])[] {
			(1 / 15f, new TreeShakeLoot[] {// fruit
				new (ItemType<Bileberry>(), tree => tree is Defiled_Tree ? 1 : 0),
				new (ItemType<Prickly_Pear>(), tree => tree is Defiled_Tree ? 1 : 0),
				new (ItemType<Pawpaw>(), tree => tree is Riven_Tree ? 1 : 0),
				new (ItemType<Periven>(), tree => tree is Riven_Tree ? 1 : 0),
			}),
		};
		public static (float chance, TreeShakeLoot[])[] DryShakeLoot => _dryShakeLoot ??= new (float chance, TreeShakeLoot[])[] {
			(1 / 20f, new TreeShakeLoot[] {
				new (ItemType<Tree_Sap>(), _ => 1),
				new (ItemType<Bark>(), _ => 1)
			}),
		};
		public static IEnumerable<TreeShakeLoot> GetLoot((float chance, TreeShakeLoot[])[] lootPool, ITree tree) {
			for (int i = 0; i < lootPool.Length; i++) {
				if (WorldGen.genRand.NextFloat() < lootPool[i].chance) {
					Tuple<TreeShakeLoot, double>[] loots = lootPool[i].Item2.Select(l => new Tuple<TreeShakeLoot, double>(l, l.Weight(tree))).Where(l => l.Item2 > 0).ToArray();
					if (loots.Length > 0) yield return new WeightedRandom<TreeShakeLoot>(loots).Get();
				}
			}
		}
		public IEnumerable<int> ProvideItemObtainability() => ShakeLoot.Concat(DryShakeLoot).SelectMany(l => l.Item2).Select(l => l.Type);
		static (float chance, TreeShakeLoot[])[] _shakeLoot;
		static (float chance, TreeShakeLoot[])[] _dryShakeLoot;
		public void Load(Mod mod) { }
		public void Unload() {
			_shakeLoot = null;
			_dryShakeLoot = null;
		}
	}
}
