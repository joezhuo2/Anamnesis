using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.SkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeDefinition", menuName = "Skill Tree/Definition")]
    public class SkillTreeDefinition : ScriptableObject
    {
        public List<SkillNodeDef> allNodes = new();
    }
}
