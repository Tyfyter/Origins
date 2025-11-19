using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Medicine {
	[ReinitializeDuringResizeArrays]
	public abstract class MedicineBase : ModItem {
		protected sealed override bool CloneNewInstances => true;
		public abstract int HealAmount { get; }
		public bool[] ImmunitySet { get; protected set; }
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
		public virtual LocalizedText BuffTooltip => Tooltip;
		public virtual string BuffTexture => null;
		public LocalizedText ImmunityListOverride { get; protected set; }
		public sealed override void Load() {
			if (!HasSpecialBuff) Mod.AddContent(ImmunityBuff = new(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
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
		public override void ModifyPotionDelay(Player player, ref int baseDelay) {
			baseDelay += CooldownIncrease;
		}
		public sealed override bool? UseItem(Player player) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (ImmunitySet[player.buffType[i]]) player.DelBuff(i--);
			}
			if (ImmunityBuff is not null) player.AddBuff(ImmunityBuff.Type, Item.buffTime);
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
			PostModifyTooltips(tooltips);
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
		protected virtual void SetupImmunitySet() {
			ImmunitySet = BuffID.Sets.Factory.CreateNamedSet($"{ImmunityName}_Immunity")
			.Description($"Controls which buffs {ImmunityName} provides immunity to")
			.RegisterBoolSet(GetDefaultImmunity().Where(b => b >= 0).ToArray());
			ImmunitySet[BuffID.PotionSickness] = false;
			ImmunitySet[BuffID.MonsterBanner] = false;
		}
		static MedicineBase() {
			foreach (MedicineBase medicine in ModContent.GetContent<MedicineBase>()) medicine.SetupImmunitySet();
		}
	}
	[Autoload(false)]
	public class MedicineBuff(MedicineBase medicine) : ModBuff {
		public override string Name => medicine.Name + "_Buff";
		public override LocalizedText Description => medicine.BuffTooltip;
		public override string Texture => medicine.BuffTexture ?? base.Texture;
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
	public class AnyDifferentMedicine : ModItem, IItemObtainabilityProvider {
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.RecipeGroups.AnyDifferentMedicine");
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override string Texture => typeof(Morphine).GetDefaultTMLName();
		public IEnumerable<int> ProvideItemObtainability() => [Type];
		public override void SetStaticDefaults() {
			RecipeGroup.IconicItemId = Type;
			RecipeGroup.ValidItems.Add(Type);
			foreach (MedicineBase medicine in ModContent.GetContent<MedicineBase>()) {
				if (medicine is not Multimed) RecipeGroup.ValidItems.Add(medicine.Type);
			}
			RecipeGroup.ValidItems.Remove(ItemID.HealingPotion);
		}
		public override void AddRecipes() => new FakeRecipeGroupRemover().Register();
		public static RecipeGroup RecipeGroup { get; private set; } = new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.AnyDifferentMedicine").Value, ItemID.HealingPotion);
	}
	public class FakeRecipeGroupRemover() : AbstractNPCShop(NPCID.BlueSlime, "FakeRecipeGroupRemoverShop") {
		public override IEnumerable<Entry> ActiveEntries => [];
		public override void FillShop(ICollection<Item> items, NPC npc) { }
		public override void FillShop(Item[] items, NPC npc, out bool overflow) => overflow = false;
		public override void FinishSetup() {
			AnyDifferentMedicine.RecipeGroup.ValidItems.Remove(ModContent.ItemType<AnyDifferentMedicine>());
			/*for (int i = 0; i < Main.recipe.Length; i++) {
				Main.recipe[i].acceptedGroups.Remove(AnyDifferentMedicine.RecipeGroup.RegisteredId);
			}*/
		}
	}
}
