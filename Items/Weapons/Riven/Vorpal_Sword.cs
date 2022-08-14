using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
    public class Vorpal_Sword : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Vorpal Sword");
			Tooltip.SetDefault("");
			//glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 17;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 50;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 7.5f;
			Item.value = 5000;
            Item.useTurn = true;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 9999;
			//Item.glowMask = glowmask;
		}
		static int textIndex = -1;
		static int delayTime = 0;
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			string text = "";
			var color = Main.MouseTextColorReal;
			switch (++textIndex) {
				case 0:
				text = "Twas brillig, and the slithy toves";
				break;
				case 1:
				text = "  Did gyre and gimble in the wabe;";
				break;
				case 2:
				text = "All mimsy were the borogoves,";
				break;
				case 3:
				text = "  And the mome raths outgrabe.";
				break;
				case 4:
				text = "\"Beware the Jabberwock, my son!";
				break;
				case 5:
				text = "  The jaws that bite, the claws that catch!";
				break;
				case 6:
				text = "Beware the Jubjub bird, and shun";
				break;
				case 7:
				text = "  The frumious Bandersnatch!\"";
				break;
				case 8:
				text = "He took his vorpal sword in hand:";
				break;
				case 9:
				text = "  Long time the manxome foe he sought—";
				break;
				case 10:
				text = "So rested he by the Tumtum tree,";
				break;
				case 11:
				text = "  And stood awhile in thought.";
				break;
				case 12:
				text = "And as in uffish thought he stood,";
				break;
				case 13:
				text = "  The Jabberwock, with eyes of flame,";
				break;
				case 14:
				text = "Came whiffling through the tulgey wood,";
				break;
				case 15:
				text = "  And burbled as it came!";
				break;
				case 16:
				text = "One, two! One, two! And through and through";
				break;
				case 17:
				text = $"   The vorpal blade went snicker-snack!";
				color = color.MultiplyRGBA(new(1f, 0f, 0f, delayTime / 13f));
				if (delayTime == 0) delayTime = 13;
				break;
				case 18:
				text = "He left it dead, and with its head";
				break;
				case 19:
				text = "  He went galumphing back.";
				break;
				case 20:
				text = "\"And hast thou slain the Jabberwock?";
				break;
				case 21:
				text = "  Come to my arms, my beamish boy!";
				break;
				case 22:
				text = $"O frabjous day! Callooh! Callay!\"";
				//color = color.MultiplyRGBA(new(1f, 0.8f, 0f, delayTime / 13f));
				//if (delayTime == 0) delayTime = 13;
				break;
				case 23:
				text = "  He chortled in his joy.";
				break;
				case 24:
				text = "'Twas brillig, and the slithy toves";
				break;
				case 25:
				text = "  Did gyre and gimble in the wabe;";
				break;
				case 26:
				text = "All mimsy were the borogoves,";
				break;
				case 27:
				text = "  And the mome raths outgrabe. ";
				break;
				default:
				textIndex = 0;
				goto case 0;
			}
			if (delayTime == 0) delayTime = 2;
			if (delayTime > 0) {
				delayTime--;
				if (delayTime > 0) {
					textIndex--;
				}
			}
			tooltips.Add(new TooltipLine(Mod, "SnickerSnack", text) {
				OverrideColor = color
			});
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if (line.Name == "SnickerSnack") {
				line.X += Main.rand.Next(-2, 3);
				line.Y += Main.rand.Next(-2, 3);
			}
			return true;
		}
	}
}
