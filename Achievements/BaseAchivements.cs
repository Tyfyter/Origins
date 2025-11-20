using Terraria.ModLoader;

namespace Origins.Achievements {
	public abstract class SlayerAchivement<T> : ModAchievement where T : ModNPC {
		public override void SetStaticDefaults() {
			AddNPCKilledCondition(ModContent.NPCType<T>());
		}
	}
}
