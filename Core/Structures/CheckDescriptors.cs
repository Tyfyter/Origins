using System.Collections.Generic;
using System.Linq;

namespace Origins.Core.Structures;
public class RequireRoom : SerializableCheckDescriptor {
	protected override Accumulator<StructureInstance, bool> Create(string[] parameters, string originalText) => Create(parameters.ToHashSet());
	public static Accumulator<StructureInstance, bool> Create(HashSet<string> requiredRooms) => (StructureInstance instance, ref bool output) => 
	output &= instance.rooms.Any(r => requiredRooms.Contains(r.Room.Identifier));
}