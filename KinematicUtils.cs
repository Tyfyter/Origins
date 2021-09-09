using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameInput;

namespace Tyfyter.Utils {
	public static class KinematicUtils {
		public class Arm {
			public Vector2 start;
			public PolarVec2 bone0;
			public PolarVec2 bone1;
			public float[] GetTargetAngles(Vector2 target, bool mirrored = false) {
				float[] jointAngles = new float[2];
				// Angle from start and target
				Vector2 diff = target - start;
				float dist = diff.Length();
				double atan = Math.Atan2(diff.Y, diff.X);
				// Is the target reachable?
				// If not, we stretch as far as possible
				if (bone0.R + bone1.R < dist) {
					jointAngles[0] = (float)atan;
					//jointAngles[1] = 0f;
				} else {
					double distSq = (dist * dist);
					double len0Sq = (bone0.R * bone0.R);
					double len1Sq = (bone1.R * bone1.R);
					double cosAngle0 = (distSq + len0Sq - len1Sq) / (2 * dist * bone0.R);
					double angle0 = Math.Acos(cosAngle0);
					double cosAngle1 = (len1Sq + len0Sq - distSq) / (2 * bone1.R * bone0.R);
					double angle1 = Math.Acos(cosAngle1);

					jointAngles[0] = (float)(atan - angle0);
					jointAngles[1] = (float)(Math.PI - angle1);
                    if (mirrored) {
						float diffAngle = diff.ToRotation();
						jointAngles[0] = diffAngle - (jointAngles[0] - diffAngle);
						jointAngles[1] = -jointAngles[1];
					}
				}
				return jointAngles;
			}
		}
	}
}