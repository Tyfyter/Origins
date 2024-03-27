using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class CanisterGlobalItem : GlobalItem {
		public static Dictionary<int, int> ItemToCanisterID { get; private set; } = new();
		public static Dictionary<Type, int> TypeToCanisterID { get; private set; } = new();
		public static List<CanisterData> CanisterDatas { get; private set; } = new();
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			if (entity.ModItem is ICanisterAmmo canister) {
				if (!ItemToCanisterID.TryGetValue(entity.type, out _)) {
					ItemToCanisterID.Add(entity.type, CanisterDatas.Count);
					TypeToCanisterID.Add(entity.ModItem.GetType(), CanisterDatas.Count);
					CanisterDatas.Add(canister.GetCanisterData with { WhatAmI = CanisterDatas.Count });
				}
				return true;
			}
			return false;
		}
		public static int GetCanisterType(int type) {
			return ItemToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : -1;
		}
		public static int GetCanisterType(Type type) {
			return TypeToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : - 1;
		}
	}
	public interface ICanisterAmmo {
		CanisterData GetCanisterData { get; }
	}
	public record struct CanisterData(Color OuterColor, Color InnerColor, int WhatAmI = -999);
	public class CanisterGlobalProjectile : GlobalProjectile {
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is ICanisterProjectile canister) {
				return true;
			}
			return false;
		}
	}
	public interface ICanisterProjectile {
	}
}
