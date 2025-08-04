using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "GraphRewriteRules", menuName = "Scriptable Objects/GraphRewriteRule")]
public class GraphRewriteRule : ScriptableObject
{
    public RoomTypes LHS;
    public DungeonLayout RHS;
}
