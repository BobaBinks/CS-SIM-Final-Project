using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class NodeConnection
{
    NodeElement node1;
    NodeElement node2;

    NodeConnectionElement line;
    DungeonLayout layout;

    public NodeConnection(NodeElement node1, NodeElement node2, NodeConnectionElement line, DungeonLayout layout)
    {
        this.node1 = node1;
        this.node2 = node2;
        this.line = line;
        this.layout = layout;
    }

    public bool NodeInConnection(NodeElement node)
    {
        return node == node1 || node == node2;
    }

    public static bool CheckNodesInConnection(NodeConnection connection, NodeElement node1, NodeElement node2)
    {
        if(connection == null || node1 == null || node2 == null)
            return false;

        // check if node1 is either input nodes
        if (node1 != connection.node1 && node1 != connection.node2)
            return false;

        // check if node2 is either input nodes
        if (node2 != connection.node1 && node2 != connection.node2)
            return false;

        if (connection.node1 == connection.node2)
            return false;

        return true;
    }

    public void UpdateLine()
    {
        if (node1 == null || node2 == null) return;

        line.UpdateLine(node1.layout.center, node2.layout.center);
    }

    public NodeConnectionElement GetLine() { return line; }

    public KeyValuePair<NodeElement,NodeElement> GetNodeElements()
    {
        return new KeyValuePair<NodeElement, NodeElement>(node1, node2);
    }
}
