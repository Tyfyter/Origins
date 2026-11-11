using PegasusLib.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Blind_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Darkness;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Darkness,
				BuffID.Obstructed
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.TryGetGlobalNPC(out Blind_Debuff_Global global)) global.blinded = true;
		}
	}
	[ReinitializeDuringResizeArrays]
	public class Blind_Debuff_Global : GlobalNPC {
		public override bool InstancePerEntity => true;
		public static bool[] IsBlindable = NPCID.Sets.Factory.CreateNamedSet($"{nameof(Terraria)}/{nameof(IsBlindable)}").RegisterBoolSet();
		public bool blinded = false;
		public bool blindable = false;
		public NPCAimedTarget lastTarget;
		public override void Load() {
			On_NPC.GetTargetData += static (orig, self, ignorePlayerTankPets) => {
				if (self.TryGetGlobalNPC(out Blind_Debuff_Global global)) {
					global.blindable = true;
					if (global.blinded) return global.lastTarget;
					return global.lastTarget = orig(self, ignorePlayerTankPets);
				}
				return orig(self, ignorePlayerTankPets);
			};
		}
		public override void SetDefaults(NPC entity) => blindable = IsBlindable[entity.type];
		public override void ResetEffects(NPC npc) {
			blinded = false;
		}
		public bool IsReallyBlinded => blindable && blinded;
	}
}
