using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "GraphRewriteRuleList", menuName = "Scriptable Objects/GraphRewriteRuleList")]
public class GraphRewriteRuleList : ScriptableObject
{
    public List<GraphRewriteRule> rules;
}
