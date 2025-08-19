using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.ColoredDamageTypes {
	[Autoload(false)]
	public class SentryExplosive : DamageClass { }
	public class ColoredDamageTypes : ModSystem {
		private static DamageClass explosiveSentry;
		public static DamageClass ExplosiveSentry => explosiveSentry ??= ModContent.GetInstance<SentryExplosive>();
		public static Mod ColoredTypesMod;
		public static Color explColor = new(234, 56, 103);
		public static Color explCritColor = new(235, 0, 59);
		public static Dictionary<DamageClass, (Color dmgColor, Color critColor)> colors;

		public override bool IsLoadingEnabled(Mod mod) {
			return ModLoader.HasMod("ColoredDamageTypes");
		}
		public override void Load() {
			Mod.AddContent(explosiveSentry = new SentryExplosive());
			if (ModLoader.TryGetMod("ColoredDamageTypes", out ColoredTypesMod)) {
				static bool PushesDamageClass(ILContext il, Instruction instruction) {
					if (instruction.MatchLdarg(out int arg)) return il.Method.Parameters[arg].ParameterType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdloc(out int loc)) return il.Body.Variables[loc].VariableType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchCallOrCallvirt(out MethodReference method)) return method.ReturnType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdfld(out FieldReference field) || instruction.MatchLdsfld(out field)) return field.FieldType.FullName == il.Import(typeof(DamageClass)).FullName;
					return false;
				}
				static void FixMethods(ILContext il) {
					ILCursor c = new(il);
					while (c.TryGotoNext(MoveType.Before,
						i => i.MatchCallOrCallvirt<object>(nameof(ToString)) && PushesDamageClass(il, i.Previous)
					)) {
						c.Remove();
						c.EmitCallvirt(typeof(ModType).GetMethod("get_" + nameof(ModType.FullName)));
					}
				}
				foreach (MethodInfo method in ColoredTypesMod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					MonoModHooks.Modify(method, FixMethods);
				}
				foreach (MethodInfo method in ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageTypes")?.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? []) {
					MonoModHooks.Modify(method, FixMethods);
				}
				//MonoModHooks.Modify(coloredDamageTypes.GetType().GetMethod("LoadModdedDamageTypes", BindingFlags.Public | BindingFlags.Static), FixMethods);
			}
		}
		public override void PostSetupContent() {
			Type DamageConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageConfig");
			FieldInfo VanillaInstance = DamageConfig?.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaVanilla = VanillaInstance.FieldType?.GetField("VanillaDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaMeleeDmg = VanillaVanilla.FieldType?.GetField("MeleeDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaRangeDmg = VanillaVanilla.FieldType?.GetField("RangedDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaMagicDmg = VanillaVanilla.FieldType?.GetField("MagicDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaThrowDmg = VanillaVanilla.FieldType?.GetField("ThrowingDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaSummonDmg = VanillaVanilla.FieldType?.GetField("SummonDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo VanillaSentryDmg = VanillaVanilla.FieldType?.GetField("SentryDmg", BindingFlags.NonPublic | BindingFlags.Instance);
			Color MeleeColor = (Color)VanillaMeleeDmg.FieldType?.GetField("MeleeDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color MeleeCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("MeleeDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color RangeColor = (Color)VanillaMeleeDmg.FieldType?.GetField("RangedDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color RangeCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("RangedDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color MagicColor = (Color)VanillaMeleeDmg.FieldType?.GetField("MagicDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color MagicCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("MagicDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color ThrowColor = (Color)VanillaMeleeDmg.FieldType?.GetField("ThrowingDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color ThrowCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("ThrowingDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color SummonColor = (Color)VanillaMeleeDmg.FieldType?.GetField("SummonDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color SummonCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("SummonDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color SentryColor = (Color)VanillaMeleeDmg.FieldType?.GetField("SentryDamage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);
			Color SentryCritColor = (Color)VanillaMeleeDmg.FieldType?.GetField("SentryDamageCrit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DamageConfig);

			colors.Add(DamageClass.Melee, (MeleeColor,  MeleeCritColor));
			colors.Add(DamageClass.Ranged, (RangeColor, RangeCritColor));
			colors.Add(DamageClass.Magic, (MagicColor, MagicCritColor));
			colors.Add(DamageClass.Throwing, (ThrowColor, ThrowCritColor));
			colors.Add(DamageClass.Summon, (SummonColor, SummonCritColor));

			if (ColoredTypesMod.TryFind("SentryClass", out DamageClass sentryClass)) {
				if (DamageClasses.ExplosiveVersion.ContainsKey(sentryClass)) DamageClasses.ExplosiveVersion[sentryClass] = ExplosiveSentry;
				else DamageClasses.ExplosiveVersion.Add(sentryClass, ExplosiveSentry);
				colors.Add(sentryClass, (SentryColor, SentryCritColor));
			}

			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Explosive,
				explColor,
				explColor,
				explCritColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Incantation,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].critColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.MeleeMagic,
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.RangedMagic,
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				ModContent.GetInstance<Chambersite_Mine_Launcher_Damage>(),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(explCritColor)
			);

			if (OriginsModIntegrations.Thorium is not null) {
				if (OriginsModIntegrations.Thorium.TryFind("BardClass", out DamageClass bard)) colors.Add(bard, (new(), new()));
			}

			foreach (KeyValuePair<DamageClass, (Color dmgColor, Color critColor)> item in colors) {
				ColoredTypesMod.Call("AddDamageType",
					DamageClasses.ExplosiveVersion[item.Key],
					item.Value.dmgColor.MultiplyRGB(explColor),
					item.Value.dmgColor.MultiplyRGB(explColor),
					item.Value.critColor.MultiplyRGB(explCritColor)
				);
			}
		}
	}
}
