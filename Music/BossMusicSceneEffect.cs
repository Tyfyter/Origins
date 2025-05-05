using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Fargowiltas.FargoSets;

namespace Origins.Music {
	public abstract class BossMusicSceneEffect : ModSceneEffect {
		public abstract override int Music { get; }
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
		public virtual int ActiveRange => 5000;
		protected bool[] npcIDs = [];
		public sealed override void SetupContent() {
			npcIDs = NPCID.Sets.Factory.CreateBoolSet(false);
			SetStaticDefaults();
		}
		public override bool IsSceneEffectActive(Player player) {
			Rectangle screenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
			Rectangle npcRect = new(0, 0, ActiveRange * 2, ActiveRange * 2);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npcIDs[npc.type] && screenRect.Intersects(npcRect.Recentered(npc.Center)) && CanNPCActivateSceneEffect(npc)) return true;
			}
			return false;
		}
		public virtual bool CanNPCActivateSceneEffect(NPC npc) => true;
	}
	public abstract class BossMusicSceneEffect<TBoss> : BossMusicSceneEffect where TBoss : ModNPC {
		public override void SetStaticDefaults() {
			npcIDs[ModContent.NPCType<TBoss>()] = true;
		}
	}
	public abstract class CoolItemSceneEffect : ModSceneEffect {
		public abstract override int Music { get; }
		public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
		public virtual int ActiveRange => 5000;
		protected bool[] itemIDs = [];
		public sealed override void SetupContent() {
			itemIDs = ItemID.Sets.Factory.CreateBoolSet(false);
			SetStaticDefaults();
		}
		public override bool IsSceneEffectActive(Player player) {
			Rectangle screenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
			Rectangle itemRect = new(0, 0, ActiveRange * 2, ActiveRange * 2);
			foreach (Item item in Main.ActiveItems) {
				if (itemIDs[item.type] && screenRect.Intersects(itemRect.Recentered(item.Center)) && CanItemActivateSceneEffect(item)) return true;
			}
			return false;
		}
		public virtual bool CanItemActivateSceneEffect(Item item) => item.newAndShiny;
	}
	public abstract class CoolItemMusicSceneEffect<TItem> : CoolItemSceneEffect where TItem : ModItem {
		public override void SetStaticDefaults() {
			itemIDs[ModContent.ItemType<TItem>()] = true;
		}
	}
}
