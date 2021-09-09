using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.Items.Other.Testing {
    public class Chocolate_Bar : ModItem {
        const float upperLegAngle = 0.1171116f;
        const float lowerLegAngle = -0.1504474f;
        const float upperLegLength = 34.2f;
        const float lowerLegLength = 33.4f;
        Arm arm;
        Arm arm2;
        Vector2 target = Vector2.Zero;
        Vector2 target2 = Vector2.Zero;
        int mode;
        const int modeCount = 10;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Animation Testing Item");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            //item.name = "jfdjfrbh";
            item.width = 16;
            item.height = 26;
            item.value = 25000;
            item.rare = 2;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useAnimation = 10;
            item.useTime = 10;
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool UseItem(Player player) {
            if(Main.myPlayer == player.whoAmI){
                if(player.altFunctionUse == 2) {
                    if(player.controlSmart) {
                        mode = (mode + modeCount - 1)%modeCount;
                    } else {
                        mode = (mode + 1)%modeCount;
                    }
                    arm = null;
                    switch (mode) {
                        case 0:
                        arm = new Arm() {
                            bone0 = new PolarVec2(upperLegLength, upperLegAngle),
                            bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
                        };
                        arm2 = new Arm() {
                            bone0 = new PolarVec2(upperLegLength, upperLegAngle),
                            bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
                        };
                        break;
                    }
                } else {
                    switch (mode) {
                        case 0:
                        if (player.controlSmart) {
                            target2 = Main.MouseWorld;
                        } else {
                            target = Main.MouseWorld;
                        }
                        break;
                    }
                }
                return true;
            }
            return false;
        }
        public void DrawAnimations(PlayerDrawInfo drawInfo) {
            switch (mode) {
                case 0:
                Texture2D upperLegTexture = mod.GetTexture("NPCs/Fiberglass/Fiberglass_Threader_Leg_Upper");
                Texture2D lowerLegTexture = mod.GetTexture("NPCs/Fiberglass/Fiberglass_Threader_Leg_Lower");
                Player player = Main.LocalPlayer;
                Vector2 start = player.Right;
                if (arm is null) {
                    arm = new Arm() {
                        bone0 = new PolarVec2(upperLegLength, upperLegAngle),
                        bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
                    };
                }
                if (arm2 is null) {
                    arm2 = new Arm() {
                        bone0 = new PolarVec2(upperLegLength, upperLegAngle),
                        bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
                    };
                }
                arm.start = start;
                float[] targets = arm.GetTargetAngles(target);
                OriginExtensions.AngularSmoothing(ref arm.bone0.Theta, targets[0], 0.2f);
                OriginExtensions.AngularSmoothing(ref arm.bone1.Theta, targets[1], 0.2f);

                Vector2 screenStart = arm.start - Main.screenPosition;
                Main.playerDrawData.Add(new DrawData(upperLegTexture, screenStart, null, Color.White, arm.bone0.Theta, new Vector2(3, 9), 1f, SpriteEffects.None, 0));
                Main.playerDrawData.Add(new DrawData(lowerLegTexture, screenStart + (Vector2)arm.bone0, null, Color.White, arm.bone0.Theta + arm.bone1.Theta, new Vector2(4, 8), 1f, SpriteEffects.None, 0));

                Vector2 start2 = player.Left;
                
                arm2.start = start2;
                float[] targets2 = arm2.GetTargetAngles(target2, true);
                OriginExtensions.AngularSmoothing(ref arm2.bone0.Theta, targets2[0], 0.2f);
                OriginExtensions.AngularSmoothing(ref arm2.bone1.Theta, targets2[1], 0.2f);

                Vector2 screenStart2 = arm2.start - Main.screenPosition;
                Main.playerDrawData.Add(new DrawData(upperLegTexture, screenStart2, null, Color.White, arm2.bone0.Theta, new Vector2(3, 3), 1f, SpriteEffects.FlipVertically, 0));
                Main.playerDrawData.Add(new DrawData(lowerLegTexture, screenStart2 + (Vector2)arm2.bone0, null, Color.White, arm2.bone0.Theta + arm2.bone1.Theta, new Vector2(4, 0), 1f, SpriteEffects.FlipVertically, 0));
                
                /*Vector2 diff = (target - start);
                float dist = diff.Length() / (upperLegLength + lowerLegLength);
                float minLength = 0.7f;
                float maxLength = 0.9f;
                if (player.controlJump) {
                    minLength = 1f;
                    maxLength = 1f;
                }
                if (dist != 0) {
                    if (dist < minLength) {
                        player.velocity -= diff.SafeNormalize(Vector2.Zero) / (dist * 3);
                    } else if (dist < 1f && dist > maxLength) {
                        player.velocity += diff.SafeNormalize(Vector2.Zero) * (dist * 8 - 4);
                    }
                }*/
                break;
            }
        }
    }
}
