using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod {
	public abstract class CritType : ILoadable {
		class CriticalStrikesOverhaul : ModSystem {
			public const string mod_name = "CritRework";
			public static List<CritType> critTypes = [];
			public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod(mod_name);
			public override void PostSetupContent() {
				Mod critMod = CritMod;
				for (int i = 0; i < critTypes.Count; i++) {
					CritType critType = critTypes[i];
					critMod.Call("AddCritType",
						Mod,
						critType.InRandomPool,
						(Func<Player, Item, Projectile, NPC, NPC.HitModifiers, bool>)critType.CritCondition,
						(Func<Player, Item, float>)critType.CritMultiplier,
						(Func<Item, bool>)critType.ForceForItem,
						(Func<Item, bool>)critType.AllowedForItem,
						Language.GetOrRegister($"Mods.{Mod.Name}.CritType.{critType.GetType().Name}"),
						critType.GetType().Name
					);
				}
			}
			public override void Unload() {
				critTypes = null;
			}
		}
		public virtual bool InRandomPool => false;
		public static Mod CritMod => ModLoader.GetMod(CriticalStrikesOverhaul.mod_name);
		public void Load(Mod mod) {
			if (ModLoader.HasMod(CriticalStrikesOverhaul.mod_name)) CriticalStrikesOverhaul.critTypes.Add(this);
		}
		public void Unload() { }
		public abstract bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers);
		public abstract float CritMultiplier(Player player, Item item);
		public abstract bool ForceForItem(Item item);
		public virtual bool AllowedForItem(Item item) => ForceForItem(item);
	}
	public class Felnum_Crit_Type : CritType {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			return target.wet || target.HasBuff(BuffID.Wet);
		}
		public override float CritMultiplier(Player player, Item item) => 1.4f;
		public override bool ForceForItem(Item item) => OriginsSets.Items.FelnumItem[item.type];
	}
}
