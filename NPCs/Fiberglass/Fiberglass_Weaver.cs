using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Fiberglass;
using Origins.Items.Weapons.Fiberglass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Tyfyter.Utils;
using static Origins.OriginExtensions;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.NPCs.Fiberglass {
    public class Fiberglass_Weaver : ModNPC {
		public override string Texture => "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Body";
        public static AutoCastingAsset<Texture2D> UpperLegTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> LowerLegTexture { get; private set; }
        Arm[] legs;
        const float upperLegLength = 34.2f;
        const float lowerLegLength = 33.4f;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass Weaver");
            if (!Main.dedServ) {
                UpperLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Weaver_Leg_Upper");
                LowerLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Weaver_Leg_Lower");
            }
        }
		public override void Unload() {
			base.Unload();
		}
		public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.PossessedArmor);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.damage = 10;
            NPC.life = NPC.lifeMax = 950;
            NPC.defense = 20;
            NPC.aiStyle = 0;
            NPC.width = NPC.height = 68;
			if (legs is null) {
                legs = new Arm[8];
                for (int i = 0; i < 8; i++) {
                    legs[i] = new Arm() {
                        bone0 = new PolarVec2(upperLegLength, 0),
                        bone1 = new PolarVec2(lowerLegLength, 0)
                    };
					switch (i / 2) {
                        case 0:
                        legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -33);
                        break;
                        case 1:
                        legs[i].start = new Vector2(i % 2 == 0? -15 : 15, -27);
                        break;
                        case 2:
                        legs[i].start = new Vector2(i % 2 == 0 ? -17 : 17, -15);
                        break;
                        case 3:
                        legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -7);
                        break;
                    }
                }
            }
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            //bestiaryEntry.

        }
		public override void AI() {
            NPCAimedTarget target = NPC.GetTargetData();
            NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(ModContent.ItemType<Fiberglass_Helmet>(), ModContent.ItemType<Fiberglass_Body>(), ModContent.ItemType<Fiberglass_Legs>(), 1));
            npcLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(ModContent.ItemType<Fiberglass_Bow>(), ModContent.ItemType<Fiberglass_Sword>(), ModContent.ItemType<Fiberglass_Pistol>(), 1));
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            Main.CurrentDrawnEntityShader = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveDye);
            for (int i = 0; i < 8; i++) {
                bool flip = (i % 2 != 0) == i < 4;
                Vector2 baseStart = legs[i].start;
                legs[i].start = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
                float[] targets = (i / 2 == 0) ? legs[i].GetTargetAngles(NPC.GetTargetData().Center, flip)  : legs[i].GetTargetAngles(((baseStart + new Vector2(0, 16)) * new Vector2(1, 2f)).RotatedBy(NPC.rotation) * 4 + NPC.Center, flip);
                AngularSmoothing(ref legs[i].bone0.Theta, targets[0], 0.2f);
                AngularSmoothing(ref legs[i].bone1.Theta, targets[1], 0.2f);

                Vector2 screenStart = legs[i].start - Main.screenPosition;
                Main.EntitySpriteDraw(UpperLegTexture, screenStart, null, drawColor, legs[i].bone0.Theta, new Vector2(3, flip ? 3 : 9), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                Main.EntitySpriteDraw(LowerLegTexture, screenStart + (Vector2)legs[i].bone0, null, drawColor, legs[i].bone0.Theta + legs[i].bone1.Theta, new Vector2(4, flip ? 0 : 8), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                legs[i].start = baseStart;
            }
            Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, null, drawColor, NPC.rotation, new Vector2(34, 70), 1f, SpriteEffects.None, 0);
            return false;
		}
		public override void HitEffect(int hitDirection, double damage) {
            NPC.velocity.X += hitDirection * 3;
            if(NPC.life<0) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG1_Gore"));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG2_Gore"));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG3_Gore"));
            } else if(damage>NPC.lifeMax*0.1f){
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot($"Gores/NPCs/FG{Main.rand.Next(3)+1}_Gore"));
            }
        }
    }
}
