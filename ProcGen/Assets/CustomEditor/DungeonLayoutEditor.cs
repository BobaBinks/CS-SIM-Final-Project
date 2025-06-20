using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class DungeonLayoutEditor : EditorWindow
{
    private string layoutPrefKey = "DungeonLayoutEditorKey";
    private DungeonLayout currentDungeonLayout = null;
    private VisualElement canvas;
    public DungeonLayout CurrentDungeonLayout
    {
        get { return currentDungeonLayout; }
        set 
        {
            currentDungeonLayout = value;

            if (rootVisualElement != null)
            {
                // render dungeon layout
                LoadLayoutGraph(rootVisualElement);
            }
        }
    }

    [MenuItem("Window/DungeonLayoutEditor")]
    public static void ShowExample()
    {
        DungeonLayoutEditor wnd = GetWindow<DungeonLayoutEditor>();
        wnd.titleContent = new GUIContent("DungeonLayoutEditor");
    }

    private void OnDisable()
    {
        // saves current layout to editor pref
        SaveLayoutToPref(CurrentDungeonLayout,layoutPrefKey);
    }

    private void OnDestroy()
    {
        // saves current layout to editor pref
        SaveLayoutToPref(CurrentDungeonLayout, layoutPrefKey);
    }

    public void CreateGUI()
    {
        Init();
    }

    public void Init(DungeonLayout layout = null)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CustomEditor/DungeonLayoutEditor.uss");

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // add canvas with context menu for adding nodes
        AddCanvas(root);

        // add style sheet to root element
        root.styleSheets.Add(styleSheet);

        if (layout != null)
        {
            CurrentDungeonLayout = layout;
            Debug.Log("Opening " + CurrentDungeonLayout.name);
            return;
        }


        CurrentDungeonLayout = GetLayoutFromPref(layoutPrefKey);

        if (CurrentDungeonLayout == null)
        {
            Debug.LogWarning("Could not get dungeon layout");
        }
        Debug.Log("Opening " + CurrentDungeonLayout.name);


    }
    /// <summary>
    /// Adds context menu to canvas for adding nodes
    /// </summary>
    /// <param name="canvas"></param>
    //https://docs.unity3d.com/Manual//UIE-contextual-menus.html
    void AddCanvasContextMenu(VisualElement canvas)
    {
        canvas.AddManipulator(new ContextualMenuManipulator((evt) =>
        {
            evt.menu.AppendAction("Add Node", (x) => {
                NodeElement node = new NodeElement(CurrentDungeonLayout,
                                                   rootVisualElement,
                                                   x.eventInfo.localMousePosition);
                rootVisualElement.Add(node);
                //CreateNode(canvas,x.eventInfo.localMousePosition);
                Debug.Log("Adding Node"); 
            }, DropdownMenuAction.AlwaysEnabled);
        }));
    }

    /// <summary>
    /// Adds a canvas background to the editor. Has a contextual menu (right click on editor) for adding nodes
    /// </summary>
    /// <param name="element"></param>
    void AddCanvas(VisualElement element)
    {
        // add canvas area
        VisualElement canvas = new VisualElement();

        // fill up space
        canvas.style.flexGrow = 1;
        canvas.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        AddCanvasContextMenu(canvas);
        this.canvas = canvas;
        element.Add(canvas);
    }

    /// <summary>
    /// Add nodes and connection visual elements to root visual to display layout
    /// </summary>
    /// <param name="root"></param>
    void LoadLayoutGraph(VisualElement root)
    {
        List<NodeElement> nodeElements = CurrentDungeonLayout.nodeElements;
        List<NodeConnection> nodeConnections = CurrentDungeonLayout.nodeConnections;

        nodeElements.Clear();
        nodeConnections.Clear();

        if (nodeElements == null || nodeConnections == null) return;

        // add all existing rooms to root
        foreach (DungeonRoom room in CurrentDungeonLayout.dungeonRoomList)
        {
            if (room == null) continue;

            NodeElement element = new NodeElement(CurrentDungeonLayout, root, new Vector2(room.styleLeft, room.styleTop), room);
            element.userData = room;

            // if room have Node visual element saved, add it to root
            root.Add(element);

            nodeElements.Add(element);
        }

        // recreate the connection only after rooms have been recreated
        foreach(NodeElement node in nodeElements)
        {
            // get dungeon room to get connections
            DungeonRoom room = node.userData as DungeonRoom;

            if(room == null) continue;


            foreach(DungeonRoom r in room.connectionList)
            {
                if (r == null) continue;

                // get node element containing this dungeon room
                NodeElement node2 = GetNodeElementFromDungeonRoom(r, nodeElements);

                // check if the node connection list in layout already contains this connection
                if (CurrentDungeonLayout.NodeElementConnectionExist(node, node2))
                    continue;

                // create connection between both dungeon room
                NodeConnectionElement line = new NodeConnectionElement(room.GetPosition(), r.GetPosition());
                NodeConnection connection = new NodeConnection(node, node2, line, CurrentDungeonLayout);
                nodeConnections.Add(connection);
                root.Add(line);

                // make sure the lines are behind the nodes in z coordinates
                SendCanvasAndVisualElementBack(line);

                // correct the placement of the line to be at the center of the node
                // need to be scheduled because layout.center is initially NaN.
                // schedule updating line until center is no longer NaN
                //https://docs.unity3d.com/6000.1/Documentation/ScriptReference/UIElements.IVisualElementScheduler.html
                root.schedule.Execute(() =>
                {
                    CurrentDungeonLayout.UpdateConnection(node);
                }).Every(1000).Until(() =>
                {
                    // stop scheduled update when the center is not NaN
                    Vector2 center = node.layout.center;
                    return !float.IsNaN(center.x) && !float.IsNaN(center.y);
                });
            }

        }
    }
    /// <summary>
    /// Used to send the element back first before sending canvas back 
    /// to ensure canvas is always the bottom most visual element
    /// </summary>
    /// <param name="element"></param>
    public void SendCanvasAndVisualElementBack(VisualElement element)
    {
        if (element == null) return;

        element.SendToBack();
        canvas.SendToBack();
    }

    private NodeElement GetNodeElementFromDungeonRoom(DungeonRoom room,List<NodeElement> nodes)
    {
        if (room != null || nodes != null)
        {
            foreach (NodeElement ne in nodes)
            {
                if (ne == null) continue;

                DungeonRoom r = ne.userData as DungeonRoom;
                if (r == room)
                    return ne;
            }
        }
        return null;
    }

    #region Layout Persistence
    /// <summary>
    /// Save current dungeon layout to EditorPrefs
    /// </summary>
    /// <param name="dungeonLayout"></param>
    /// <param name="prefKey"></param>
    void SaveLayoutToPref(DungeonLayout dungeonLayout, string prefKey)
    {
        if (dungeonLayout == null)
        {
            Debug.LogWarning("Unable to save layout to pref");
            return;
        }
        EditorUtility.SetDirty(CurrentDungeonLayout);
        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(CurrentDungeonLayout.GetInstanceID()));
        EditorPrefs.SetString(prefKey, guid);
    }

    /// <summary>
    /// Get current dungeon layout from EditorPrefs
    /// </summary>
    /// <param name="prefKey"></param>
    DungeonLayout GetLayoutFromPref(string prefKey)
    {
        string guid = EditorPrefs.GetString(prefKey, "");

        if (guid == "")
        {
            Debug.LogWarning("Could not get guid for editor's layout.");
            return null;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        return AssetDatabase.LoadAssetAtPath<DungeonLayout>(assetPath);
    }
    #endregion



}
