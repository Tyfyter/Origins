using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Medicine {
	[ReinitializeDuringResizeArrays]
	public abstract class MedicineBase : ModItem {
		protected sealed override bool CloneNewInstances => true;
		public abstract int HealAmount { get; }
		public bool[] ImmunitySet { get; private set; }
		public abstract int ImmunityDuration { get; }
		/// <summary>
		/// Used to set up <see cref="ImmunitySet"/>
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<int> GetDefaultImmunity();
		public virtual int Value => Item.sellPrice(silver: 2);
		public virtual string ImmunityName => Name;
		public virtual int CooldownIncrease => 60 * 15;
		public MedicineBuff ImmunityBuff { get; protected set; }
		public virtual bool HasSpecialBuff => false;
		public LocalizedText ImmunityListOverride { get; protected set; }
		public sealed override void Load() {
			if (!HasSpecialBuff) Mod.AddContent(ImmunityBuff = new(this));
		}
		public sealed override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 30;
			PostSetStaticDefaults();
			_ = ImmunityListOverride;
		}
		public virtual void PostSetStaticDefaults() { }
		public override void SetDefaults() {
			Item.DefaultToHealingPotion(16, 16, HealAmount);
			Item.buffTime = ImmunityDuration;
			Item.value = Value;
		}
		// TODO: https://github.com/tModLoader/tModLoader/pull/4786 arrives in stable in November
#if !TML_2025_08
		public override void ModifyPotionDelay(Player player, ref int baseDelay) {
			baseDelay += CooldownIncrease;
		}
#endif
		public sealed override bool? UseItem(Player player) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (ImmunitySet[player.buffType[i]]) player.DelBuff(i--);
			}
			player.AddBuff(ImmunityBuff.Type, Item.buffTime);
			OnUseItem(player);
			return base.UseItem(player);
		}
		public virtual void OnUseItem(Player player) { }
		public sealed override void ModifyTooltips(List<TooltipLine> tooltips) {
			List<int> debuffs = ImmunitySet.GetTrueIndexes();
			if (debuffs.Count > 0) {
				for (int i = 0; i < tooltips.Count; i++) {
					if (tooltips[i].Name == "Tooltip0") {
						tooltips.Insert(i, new(Mod, "HealDebuffs", TextUtils.Format("Mods.Origins.Items.GenericTooltip.HealDebuffs", ImmunityList(debuffs))));
						break;
					}
				}
			}

		}
		public IEnumerable<string> ImmunityList(List<int> debuffs) {
			if (ImmunityListOverride is null) {
				return debuffs.Select(b => $"[buffhint/self:{b}]");
			} else {
				return [ImmunityListOverride.Value];
			}
		}
		public virtual void PostModifyTooltips(List<TooltipLine> tooltips) { }
		public virtual void UpdateBuff(Player player, ref int buffIndex) { }
		static MedicineBase() {
			foreach (MedicineBase medicine in ModContent.GetContent<MedicineBase>()) {
				medicine.ImmunitySet = BuffID.Sets.Factory.CreateNamedSet($"{medicine.ImmunityName}_Immunity")
				.Description($"Controls which buffs {medicine.ImmunityName} provides immunity to")
				.RegisterBoolSet(medicine.GetDefaultImmunity().Where(b => b >= 0).ToArray());
				medicine.ImmunitySet[BuffID.PotionSickness] = false;
				medicine.ImmunitySet[BuffID.MonsterBanner] = false;
			}
		}
	}
	[Autoload(false)]
	public class MedicineBuff(MedicineBase medicine) : ModBuff {
		public override string Name => medicine.Name + "_Buff";
		public override void Update(Player player, ref int buffIndex) {
			for (int i = 0; i < player.buffImmune.Length; i++) {
				player.buffImmune[i] |= medicine.ImmunitySet[i];
			}
			medicine.UpdateBuff(player, ref buffIndex);
		}
		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) {
			List<int> debuffs = medicine.ImmunitySet.GetTrueIndexes();
			if (debuffs.Count > 0) tip = TextUtils.Format("Mods.Origins.Items.GenericTooltip.HealDebuffs", medicine.ImmunityList(debuffs)) + '\n' + tip;
		}
	}
}
