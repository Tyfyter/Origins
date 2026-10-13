using Origins.Items.Tools.Wiring;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Testing; 
public class Enemy_Freezer : TestingItem {
	public override string Texture => typeof(Screwdriver_Logic_White).GetDefaultTMLName();
	public override void SetDefaults() {
		Item.damage = 1;
		Item.DamageType = DamageClass.Melee;
		Item.width = 42;
		Item.height = 42;
		Item.useTime = 16;
		Item.useAnimation = 16;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.knockBack = 2;
		Item.autoReuse = true;
		Item.useTurn = true;
		Item.color = new(150, 255, 255);
		Item.rare = ItemRarityID.Green;
		Item.UseSound = SoundID.Item1;
	}
	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) =>
		target.GetGlobalNPC<Global>().frozen ^= true;
	class Global : GlobalNPC {
		public bool frozen;
		public override bool InstancePerEntity => true;
#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => DebugConfig.Instance.ForceEnableDebugItems;
#endif
		public override bool PreAI(NPC npc) {
			if (!frozen) return true;
			npc.velocity *= 0.93f;
			npc.direction = (Main.LocalPlayer.Center.X > npc.Center.X).ToDirectionInt();
			return false;
		}
	}
}
