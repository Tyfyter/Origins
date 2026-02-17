using Origins.Items.Tools.Liquids;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Super_Sludge : ComplexFrameTile, IAshenTile {
		public override void SetStaticDefaults() {
			Murky_Sludge.TilesForSound[Type] = true;
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x2c212a));
			DustType = DustID.Mud;
			HitSound = SoundID.NPCHit18;
		}
		public override void FloorVisuals(Player player) {
			player.AddBuff(BuffType<Super_Sludge_Debuff>(), 30);
			if (player.gfxOffY < 4) MathUtils.LinearSmoothing(ref player.gfxOffY, 4, 4);
		}
		public override bool HasWalkDust() {
			return Main.rand.NextBool(3, 25);
		}
		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Snow;
			color = FromHexRGB(0x2c212a);
		}
	}
	public class Super_Sludge_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Super_Sludge>());
		}
	}
	public class Super_Sludge_Debuff : ModBuff {
		public override string Texture => typeof(Murky_Sludge_Debuff).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer oP = player.OriginPlayer();
			oP.moveSpeedMult *= 1.75f;
			oP.superSludge = true;
			player.gravity = 0;
			//if (!oP.collidingY) player.buffTime[buffIndex] = 1;
		}
	}
}
