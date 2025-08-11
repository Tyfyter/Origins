﻿using MonoMod.Cil;
using Origins.Items.Tools;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.CrossMod {
	[ReinitializeDuringResizeArrays]
	public abstract class CritType : ILoadable {
		public static void SetCritType<TCritType>(int type) where TCritType : CritType {
			if (ModEnabled) ForcedCritTypes[type] = ModContent.GetInstance<TCritType>();
		}
		protected static CritType[] ForcedCritTypes = ItemID.Sets.Factory.CreateCustomSet<CritType>(null);
		class CriticalStrikesOverhaul : ModSystem {
			public const string mod_name = "CritRework";
			public static List<CritType> critTypes = [];
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
			public override void PostSetupContent() {
				Mod critMod = CritMod;
				if (critMod.Version == new Version(1, 0, 1)) {
					Type ModCalledCritType = critMod.Code.GetType("CritRework.Content.CritTypes.ModCalledCritType");
					MonoModHooks.Modify(
						ModCalledCritType.GetConstructor([typeof(Mod), typeof(bool), typeof(Func<Player, Item, Projectile, NPC, NPC.HitModifiers, bool>), typeof(Func<Player, Item, float>), typeof(Func<Item, bool>), typeof(Func<Item, bool>), typeof(LocalizedText), typeof(string)]),
						il => {
							ILCursor c = new(il);
							c.GotoNext(MoveType.Before, static i => i.MatchRet());
							c.EmitLdarg(0);
							c.EmitLdarg(5);
							c.EmitStfld(ModCalledCritType.GetField("forceOnItem"));
						}
					);
					Type CritItem = critMod.Code.GetType("CritRework.Common.Globals.CritItem");
					MonoModHooks.Modify(
						CritItem.GetMethod(nameof(GlobalItem.LoadData)),
						il => {
							ILCursor c = new(il);
							c.EmitLdarg(2);
							c.EmitDelegate((TagCompound tag) => {
								if (tag.SafeGet<string>("critType") == "CritRework.Content.CritTypes.ModCalledCritType") {
									tag.Remove("critType");
								}
							});
						}
					);
				}
				for (int i = 0; i < critTypes.Count; i++) {
					CritType critType = critTypes[i];
					critType.PreSetup();
					string internalName = $"{Mod.Name}/{critType.GetType().Name}";
					critMod.Call("AddCritType",
						Mod,
						critType.InRandomPool,
						(Func<Player, Item, Projectile, NPC, NPC.HitModifiers, bool>)critType.CritCondition,
						(Func<Player, Item, float>)critType.CritMultiplier,
						(Func<Item, bool>)critType.ForceForItem,
						(Func<Item, bool>)critType.AllowedForItem,
						critType.Description,
						internalName
					);
				}
				RefreshForcedCritType(Indestructible_Saddle.ID);
			}
			static void RefreshForcedCritType(int type) {
				Item item = ContentSamples.ItemsByType[type];
				foreach (GlobalItem global in item.EntityGlobals) {
					if (global?.Name == "CritItem") {
						global.SetDefaults(item);
						break;
					}
				}
			}
			public override void Unload() {
				critTypes = null;
			}
		}
		public virtual bool InRandomPool => false;
		static Mod critMod;
		public static Mod CritMod => critMod ??= ModLoader.GetMod(CriticalStrikesOverhaul.mod_name);
		static bool? modEnabled;
		public static bool ModEnabled => modEnabled ??= ModLoader.HasMod(CriticalStrikesOverhaul.mod_name);
		public virtual LocalizedText Description => Language.GetOrRegister($"Mods.Origins.CritType.{GetType().Name}");
		public void Load(Mod mod) {
			if (ModEnabled) CriticalStrikesOverhaul.critTypes.Add(this);
		}
		public void Unload() { }
		public abstract bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers);
		public abstract float CritMultiplier(Player player, Item item);
		public virtual bool ForceForItem(Item item) => ForcedCritTypes[item.type] == this;
		public virtual bool AllowedForItem(Item item) => ForcedCritTypes[item.type] == this;
		public virtual void PreSetup() { }
		protected class CritModPlayer : ModPlayer {
			public override string Name => "CritRework_" + base.Name;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
		}
		protected class CritGlobalProjectile : GlobalProjectile {
			public override string Name => "CritRework_" + base.Name;
			public override bool InstancePerEntity => true;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
		}
		protected class CritGlobalNPC : GlobalNPC {
			public override string Name => "CritRework_" + base.Name;
			public override bool InstancePerEntity => true;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
		}
	}
	public abstract class CritType<TItem> : CritType where TItem : ModItem {
		public override void PreSetup() {
			ForcedCritTypes[ModContent.ItemType<TItem>()] = this;
			if (this is Miter_Saw_Crit_Type) {

			}
		}
	}
	public class Felnum_Crit_Type : CritType {
		public override LocalizedText Description => OriginsModIntegrations.CheckAprilFools() ? base.Description : CritMod.GetLocalization($"CritTypes.FoeIsWet.Description");
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			return target.wet || target.HasBuff(BuffID.Wet);
		}
		public override float CritMultiplier(Player player, Item item) => 1.4f;
	}
	public class Flak_Crit_Type : CritType {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => !target.collideX && !target.collideY;
		public override float CritMultiplier(Player player, Item item) => 1.4f;
	}
}
