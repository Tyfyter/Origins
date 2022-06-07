using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.NPCs.Defiled {
    public class Defiled_Flyer : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Phantom");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Bunny);
            NPC.aiStyle = 14;
            NPC.lifeMax = 40;
            NPC.defense = 8;
            NPC.damage = 20;
            NPC.width = 104;
            NPC.height = 38;
            NPC.friendly = false;
            NPC.lifeRegen = 50;
        }
        public override void AI() {
            NPC.FaceTarget();
            if(!NPC.HasValidTarget)NPC.direction = Math.Sign(NPC.velocity.X);
            NPC.spriteDirection = NPC.direction;
            if(++NPC.frameCounter>5) {
                NPC.frame = new Rectangle(0, (NPC.frame.Y+38)%152, 104, 36);
                NPC.frameCounter = 0;
            }
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(NPC.height)), new Vector2(knockback*player.direction, -0.1f*knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life<0) {
                for(int i = 0; i < 2; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
