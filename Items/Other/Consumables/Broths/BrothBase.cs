﻿using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public abstract class BrothBase : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Broth"
		];
		protected override bool CloneNewInstances => true;
		public BrothBuff Buff { get; private set; }
		public override void Load() {
			Mod.AddContent(Buff = new(this));
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				30, 28,
				Buff.Type,
				60 * 60 * 6,
				true
			);
			Item.useStyle = ItemUseStyleID.EatFood;
		}
		public override bool? UseItem(Player player) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (Origins.BrothBuffs[player.buffType[i]]) {
					player.DelBuff(i--);
				}
			}
			return true;
		}
		public virtual void UpdateBuff(Player player, ref int buffIndex) { }
		public virtual void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }
		public virtual void PreUpdateMinion(Projectile minion) { }
		internal static bool On_Player_QuickBuff_ShouldBotherUsingThisBuff(On_Player.orig_QuickBuff_ShouldBotherUsingThisBuff orig, Player self, int attemptedType) {
			bool result = orig(self, attemptedType);
			bool isBroth = Origins.BrothBuffs[attemptedType];
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (isBroth && Origins.BrothBuffs[self.buffType[i]]) {
					result = false;
					break;
				}
			}
			return result;
		}
	}
	[Autoload(false)]
	public class BrothBuff(BrothBase item) : ModBuff {
		public override string Name => item.Name + "_Buff";
		public override void SetStaticDefaults() {
			Origins.BrothBuffs[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			item.UpdateBuff(player, ref buffIndex);
			player.OriginPlayer().broth = item;
		}
	}
}
