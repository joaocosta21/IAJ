using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class TriangleFormation : FormationPattern
    {
        // This is a very simple line formation, with the anchor being the position of the character at index 0.
        private static readonly float offset = -3.0f;

        public TriangleFormation()
        {
            this.FreeAnchor = false;
        }

        public override Vector3 GetOrientation(FormationManager formation)
        {
            return formation.SlotAssignment.Keys.First().transform.forward;
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber) => slotNumber switch
        {
            0 => formation.AnchorPosition,
            1 => formation.AnchorPosition + Quaternion.AngleAxis(60, Vector3.up) * this.GetOrientation(formation) * offset,
            2 => formation.AnchorPosition + Quaternion.AngleAxis(-60, Vector3.up) * this.GetOrientation(formation) * offset,
            _ => formation.AnchorPosition + offset * slotNumber * this.GetOrientation(formation)
        };

        public override bool SupportSlot(int slotCount)
        {
            return (slotCount <= 3);
        }


    }
}