﻿using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static Origins.OriginExtensions;
using Terraria.DataStructures;
using Tyfyter.Utils;
using PegasusLib;

namespace Origins.Gores.NPCs {
	public class FG2_Gore : ModGore {
		public override void OnSpawn(Gore gore, IEntitySource source) {
			gore.frame = 4;
		}
		public override bool Update(Gore gore) {
			if (gore.velocity.Y >= 0f && gore.velocity.Y < 0.5f) {
				if (gore.frame > 0) gore.frame--;
				if (gore.frame == 0) {
					AngularSmoothing(ref gore.rotation, Math.Abs(GeometryUtils.AngleDif(gore.rotation, 0)) > MathHelper.PiOver2 ? MathHelper.PiOver2 : 0, 0.5f);
					gore.velocity.X *= 0.95f;
				}
			}
			return true;
		}
	}
}
