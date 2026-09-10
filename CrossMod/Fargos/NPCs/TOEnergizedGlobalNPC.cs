using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.NPCs {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOEnergizedGlobalNPC : GlobalNPC {
		public static bool SwarmActive;
		public static bool UseHardmodeScaling = false;
		public static int SwarmItemsUsed = 0;
		public bool isSwarmBoss = false;
		public Entity getSwarmMinionBoss;

		public override bool InstancePerEntity => true;
		public override void OnSpawn(NPC npc, IEntitySource source) {
			isSwarmBoss = source?.Context == OriginsModIntegrations.SwarmContext;
			if(isSwarmBoss || (source is EntitySource_Parent { Entity: NPC parent } && parent.GetGlobalNPC<TOEnergizedGlobalNPC>().isSwarmBoss)) {
				int newHealth = (UseHardmodeScaling ? 160 : 28) * 1000;

				npc.lifeMax = (int)(newHealth * ModCompatSets.EnergizedHealthMultiplier[npc.type]);
				if (SwarmItemsUsed > 1) npc.lifeMax *= SwarmItemsUsed;
				npc.life = npc.lifeMax;

				Fargowiltas.Fargowiltas.HardmodeSwarmActive = UseHardmodeScaling;
				Fargowiltas.Fargowiltas.SwarmItemsUsed = SwarmItemsUsed;
				int minDamage = Fargowiltas.Fargowiltas.SwarmMinDamage * 2;
				if (!npc.townNPC && npc.lifeMax > 10 && npc.damage > 0 && npc.damage < minDamage)
					npc.damage = minDamage;

				if (source is EntitySource_Parent fromParent && !isSwarmBoss) getSwarmMinionBoss = fromParent.Entity;
			}
		}
		public override void OnKill(NPC npc) {
			if (isSwarmBoss) {
				(int Bag, int Trophy, int Energizer) = ModCompatSets.EnergizedBossItems[npc.type];

				if (Bag > 0) npc.DropItemInstanced(npc.Center, npc.Size, Bag, (SwarmItemsUsed * 5) - 1);
				if (Trophy > 0 && SwarmItemsUsed >= 3) Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, Trophy, SwarmItemsUsed / 3);
				if (Energizer > 0 && SwarmItemsUsed >= 10) Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, Energizer, SwarmItemsUsed / 10);

				SwarmActive = false;
				Fargowiltas.Fargowiltas.SwarmActive = false;

				foreach (NPC minion in Main.ActiveNPCs) {
					if (minion.life <= 0) continue;
					TOEnergizedGlobalNPC glob = minion.GetGlobalNPC<TOEnergizedGlobalNPC>();
					if (glob?.getSwarmMinionBoss == npc) minion.StrikeNPC(new() { InstantKill = true });
				}
			}
		}
	}
}
