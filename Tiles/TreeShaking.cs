using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles {
	public class TreeShaking : ILoadable, IItemObtainabilityProvider {
		public delegate double WeightProvider(ITree tree);
		public record struct TreeShakeLoot(int Type, WeightProvider Weight, int Min = 1, int Max = 1);
		public static (float chance, TreeShakeLoot[])[] ShakeLoot => _shakeLoot ??= [
			(143 / 2000f, new TreeShakeLoot[] {// fruit
				new (ItemType<Bileberry>(), tree => tree is Petrified_Tree ? 1 : 0),
				new (ItemType<Prickly_Pear>(), tree => tree is Petrified_Tree ? 1 : 0),
                new (ItemType<Pawpaw>(), tree => tree is Exoskeletal_Tree ? 1 : 0),
				new (ItemType<Periven>(), tree => tree is Exoskeletal_Tree ? 1 : 0),
			}),
            (3 / 100f, new TreeShakeLoot[] {
                new (ItemType<Petrified_Prickly_Pear>(), tree => tree is Petrified_Tree ? 1 : 0),
            }),
        ];
		public static (float chance, TreeShakeLoot[])[] DryShakeLoot => _dryShakeLoot ??= [
			(1 / 20f, new TreeShakeLoot[] {
				new (ItemType<Tree_Sap>(), _ => 1),
				new (ItemType<Bark>(), _ => 1)
			}),
		];
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
