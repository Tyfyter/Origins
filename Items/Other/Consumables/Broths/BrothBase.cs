using Origins.Dev;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public abstract class BrothBase : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Broth
		];
		protected override bool CloneNewInstances => true;
		[field: CloneByReference]
		public BrothBuff Buff { get; private set; }
		public virtual LocalizedText BuffDisplayName => DisplayName;
		public virtual LocalizedText BuffDescription => Tooltip;
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
				60 * 60 * Duration,
				true
			);
			Item.useStyle = ItemUseStyleID.EatFood;
		}
		public virtual int Duration => 6;
		public virtual void UpdateBuff(Player player, ref int buffIndex) { }
		public virtual void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }
		public virtual void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { }
		public virtual void PreUpdateMinion(Projectile minion) { }
		public virtual void UpdateMinion(Projectile minion, int time) { }
#pragma warning disable CS0618 // Type or member is obsolete
		[Obsolete("Use the overload with a IArtifactDamageSource")]
		public virtual void ModifyHurt(Projectile minion, ref int damage, bool fromDoT) { }
		public virtual void ModifyHurt(Projectile minion, ref int damage, bool fromDoT, IArtifactDamageSource damageSource) => ModifyHurt(minion, ref damage, fromDoT);
		[Obsolete("Use the overload with a IArtifactDamageSource")]
		public virtual void OnHurt(Projectile minion, int damage, bool fromDoT) { }
		public virtual void OnHurt(Projectile minion, int damage, bool fromDoT, IArtifactDamageSource damageSource) => OnHurt(minion, damage, fromDoT);
#pragma warning restore CS0618 // Type or member is obsolete
		public virtual void PostDrawMinion(Projectile minion, Color lightColor) { }
		/// <summary>
		/// Runs when the broth a minion is affected by changes, including when a new minion is spawned
		/// </summary>
		/// <param name="minion">The minion</param>
		/// <param name="active">1 if the broth is becoming active, -1 if the broth is becoming inactive</param>
		public virtual void SwitchActive(Projectile minion, int active) { }
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

		internal static void On_Player_AddBuff_RemoveOldMeleeBuffsOfMatchingType(On_Player.orig_AddBuff_RemoveOldMeleeBuffsOfMatchingType orig, Player self, int type) {
			orig(self, type);
			if (Origins.BrothBuffs[type]) {
				for (int i = self.buffType.Length - 1; i >= 0; i--) {
					if (self.buffType[i] != type && Origins.BrothBuffs[self.buffType[i]]) {
						self.DelBuff(i);
					}
				}
			}
		}
	}
	[Autoload(false)]
	public class BrothBuff(BrothBase item) : ModBuff {
		public override string Name => item.Name + "_Buff";
		public override LocalizedText DisplayName => item.BuffDisplayName;
		public override LocalizedText Description => item.BuffDescription;
		public override void SetStaticDefaults() {
			Main.persistentBuff[Type] = true;
			Origins.BrothBuffs[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			item.UpdateBuff(player, ref buffIndex);
			player.OriginPlayer().broth = item;
		}
	}
}
