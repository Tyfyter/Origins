using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Shattered_Goldfish : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 38, 28);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			DefiledGlobalNPC.NPCTransformations.Add(NPCID.Goldfish, Type);
		}
		public override void FindFrame(int frameHeight) {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				IsWet = true
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CorruptGoldfish);
			NPC.lifeMax = 110;
			NPC.defense = 7;
			NPC.damage = 32;
			NPC.width = 26;
			NPC.height = 18;
			NPC.frame.Height = 28;
			NPC.value = 500;
			AnimationType = NPCID.CorruptGoldfish;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(2.1f, 2.35f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(2.1f, 2.35f);
			this.CopyBanner<Defiled_Banner_NPC>();
		}
        public bool ForceSyncMana => false;
        public float Mana { get => 1; set { } }
        public void Regenerate(out int lifeRegen)
        {
            if (NPC.life > 20)
            {
                lifeRegen = 75 / (NPC.life / 20);
            }
            else
            {
                lifeRegen = 0;
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText(),
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon
            });
		}
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for (int i = Main.rand.Next(3); i-- > 0;)
                Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
                for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
            }
        }
    }
}
