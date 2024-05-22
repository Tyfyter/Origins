using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace Tyfyter.Utils {
	public static class WaveFunctionCollapse {
		public class Generator<T> {
			UnifiedRandom random;
			List<Tuple<Cell, double>> cellTypes;
			WeightedRandom<Cell>[,] potentials;
			Cell?[,] actuals;
			bool matchAll;
			public Generator(int width, int height, UnifiedRandom random = null, bool matchAll = false, params Tuple<Cell, double>[] cellTypes) {
				this.random = random ?? Main.rand;
				potentials = new WeightedRandom<Cell>[width, height];
				actuals = new Cell?[width, height];
				this.cellTypes = cellTypes.ToList();
				this.matchAll = matchAll;
				Reset();
			}
			public Generator(Cell?[,] actuals, UnifiedRandom random = null, bool matchAll = false, params Tuple<Cell, double>[] cellTypes) {
				this.random = random ?? Main.rand;
				potentials = new WeightedRandom<Cell>[actuals.GetLength(0), actuals.GetLength(1)];
				this.actuals = actuals;
				this.cellTypes = cellTypes.ToList();
				this.matchAll = matchAll;
				Reset(false);
				Force(actuals);
			}
			public void Force(Cell?[,] values, int iOffset = 0, int jOffset = 0, bool refresh = true) {
				for (int i = 0; i < values.GetLength(0); i++) {
					for (int j = 0; j < values.GetLength(1); j++) {
						if (values[i, j] is Cell value) potentials[i - iOffset, j - jOffset] = new WeightedRandom<Cell>(random, new Tuple<Cell, double>(value, 1));
					}
				}
				if (refresh) Refresh();
			}
			public void Force(int i, int j, Cell value, bool refresh = true) {
				potentials[i, j] = new WeightedRandom<Cell>(random, new Tuple<Cell, double>(value, 1));
				if (refresh) Refresh();
			}
			public void Collapse() {
				List<(int x, int y)> targets = new();
				int targetCount = int.MaxValue;
				cont:
				for (int i = 0; i < potentials.GetLength(0); i++) {
					for (int j = 0; j < potentials.GetLength(1); j++) {
						if (potentials[i, j].elements.Count <= 1) continue;
						if (potentials[i, j].elements.Count < targetCount) {
							targetCount = potentials[i, j].elements.Count;
							targets.Clear();
							targets.Add((i, j));
						} else if (potentials[i, j].elements.Count == targetCount) {
							targets.Add((i, j));
						}
					}
				}
				if (targets.Count <= 0) {
					return;
				}
				while (!Refresh()) {
					int index = random.Next(targets.Count);
					(int i1, int j1) = targets[index];
					targets.RemoveAt(index);
					potentials[i1, j1] = new WeightedRandom<Cell>(random, new Tuple<Cell, double>((actuals[i1, j1] = potentials[i1, j1].Get()).Value, 1));
					if (targets.Count <= 0) goto cont;
				}
			}
			public void CollapseWith(Action<int, int, T> act) {
				List<(int x, int y)> targets = new();
				int targetCount = int.MaxValue;
				cont:
				for (int i = 0; i < potentials.GetLength(0); i++) {
					for (int j = 0; j < potentials.GetLength(1); j++) {
						if (potentials[i, j].elements.Count <= 1) continue;
						if (potentials[i, j].elements.Count < targetCount) {
							targetCount = potentials[i, j].elements.Count;
							targets.Clear();
							targets.Add((i, j));
						} else if (potentials[i, j].elements.Count == targetCount) {
							targets.Add((i, j));
						}
					}
				}
				if (targets.Count <= 0) {
					return;
				}
				while (!Refresh()) {
					int index = random.Next(targets.Count);
					(int i1, int j1) = targets[index];
					targets.RemoveAt(index);
					potentials[i1, j1] = new WeightedRandom<Cell>(random, new Tuple<Cell, double>((actuals[i1, j1] = potentials[i1, j1].Get()).Value, 1));
					act(i1, j1, actuals[i1, j1].Value.value);
					if (targets.Count <= 0) goto cont;
				}
			}
			public void Reset(bool refresh = true) {
				for (int i = 0; i < potentials.GetLength(0); i++) {
					for (int j = 0; j < potentials.GetLength(1); j++) {
						potentials[i, j] = new WeightedRandom<Cell>(random, cellTypes.ToArray());
					}
				}
				if (refresh) Refresh();
			}
			/// <summary>
			/// removes all invalid potentials
			/// </summary>
			/// <returns>true if the actuals are all determined</returns>
			public bool Refresh() {
				const ushort not_1_mask = ushort.MaxValue ^ 1;
				bool changed = false;
				bool collapsed = true;
				int tries = 0;
				int culls = 0;
				string log = "";
				loop:
				for (int i = 0; i < potentials.GetLength(0); i++) {
					for (int j = 0; j < potentials.GetLength(1); j++) {
						if (potentials[i, j].elements.Count == 1) {
							actuals[i, j] = potentials[i, j].elements[0].Item1;
							continue;
						}
						if (potentials[i, j].elements.Count < 1) {
							throw new InvalidWFCException($"cell {i}, {j} has 0 potentials");
						}
						collapsed = false;
						potentials[i, j].elements.RemoveAll((v) => {
							bool matches = false;
							if (j > 0) {
								matches = potentials[i, j - 1].elements.Any(
									(o) => {
										if (matchAll) {
											return (v.Item1.top & not_1_mask) == (o.Item1.bottom & not_1_mask);
										}
										return (v.Item1.top & o.Item1.bottom & not_1_mask) != 0;
									}
								);
							} else {
								matches = (v.Item1.top & 1) != 0;
							}
							if (!matches) {
								changed = true;
								culls++;
								return true;
							}
							return false;
						});
						if (potentials[i, j].elements.Count < 1) {
							throw new InvalidWFCException($"cell {i}, {j} has 0 potentials after top comparison");
						}
						log = "";
						potentials[i, j].elements.RemoveAll((v) => {
							bool matches = false;
							if (i > 0) {
								matches = potentials[i - 1, j].elements.Any(
									(o) => {
										if (matchAll) {
											log += $"{v.Item1.left} & {not_1_mask} ?= {o.Item1.right} & {not_1_mask}\n";
											return (v.Item1.left & not_1_mask) == (o.Item1.right & not_1_mask);
										}
										log += $"{v.Item1.left} & {o.Item1.right} & {not_1_mask} ?= {v.Item1.left & o.Item1.right & not_1_mask}\n";
										return (v.Item1.left & o.Item1.right & not_1_mask) != 0;
									}
								);
							} else {
								matches = (v.Item1.left & 1) != 0;
							}
							if (!matches) {
								changed = true;
								culls++;
								return true;
							}
							return false;
						});
						if (potentials[i, j].elements.Count < 1) {
							Origins.Origins.instance.Logger.Info(log);
							throw new InvalidWFCException($"cell {i}, {j} has 0 potentials after left comparison");
						}
						potentials[i, j].elements.RemoveAll((v) => {
							bool matches = false;
							if (j + 1 < potentials.GetLength(1)) {
								matches = potentials[i, j + 1].elements.Any(
									(o) => {
										if (matchAll) {
											return (v.Item1.bottom & not_1_mask) == (o.Item1.top & not_1_mask);
										}
										return (v.Item1.bottom & o.Item1.top & not_1_mask) != 0;
									}
								);
							} else {
								matches = (v.Item1.bottom & 1) != 0;
							}
							if (!matches) {
								changed = true;
								culls++;
								return true;
							}
							return false;
						});
						if (potentials[i, j].elements.Count < 1) {
							throw new InvalidWFCException($"cell {i}, {j} has 0 potentials after bottom comparison");
						}
						potentials[i, j].elements.RemoveAll((v) => {
							bool matches = false;
							if (i + 1 < potentials.GetLength(0)) {
								matches = potentials[i + 1, j].elements.Any(
									(o) => {
										if (matchAll) {
											return (v.Item1.right & not_1_mask) == (o.Item1.left & not_1_mask);
										}
										return (v.Item1.right & o.Item1.left & not_1_mask) != 0;
									}
								);
							} else {
								matches = (v.Item1.right & 1) != 0;
							}
							if (!matches) {
								changed = true;
								culls++;
								return true;
							}
							return false;
						});
						if (potentials[i, j].elements.Count < 1) {
							throw new InvalidWFCException($"cell {i}, {j} has 0 potentials after right comparison");
						}
					}
				}
				if (changed) {
					if (++tries < 1000) {
						changed = false;
						collapsed = true;
						goto loop;
					}
					throw new InvalidWFCException("reached 1000 tries without stable conclusion");
				}
				Origins.Origins.instance.Logger.Info($"culled {culls} entries");
				return collapsed;
			}
			public bool GetCollapsed() {
				for (int i = 0; i < actuals.GetLength(0); i++) {
					for (int j = 0; j < actuals.GetLength(1); j++) {
						if (!actuals[i, j].HasValue) return false;
					}
				}
				return true;
			}
			public T GetActual(int i, int j) {
				if (!actuals[i, j].HasValue) {
					Collapse();
				}
				return actuals[i, j].Value.value;
			}
			/// <summary>
			/// defines the compatibility between cells, note that 1 is reserved for edges
			/// </summary>
			public struct Cell {
				public readonly ushort top;
				public readonly ushort left;
				public readonly ushort bottom;
				public readonly ushort right;
				public readonly T value;
				public Cell(T value, ushort top = 0, ushort left = 0, ushort bottom = 0, ushort right = 0) {
					this.value = value;
					this.top = top;
					this.left = left;
					this.bottom = bottom;
					this.right = right;
				}
			}
		}

		[Serializable]
		public class InvalidWFCException : Exception {
			public InvalidWFCException() { }
			public InvalidWFCException(string message) : base(message) { }
			public InvalidWFCException(string message, Exception inner) : base(message, inner) { }
		}
	}
}
