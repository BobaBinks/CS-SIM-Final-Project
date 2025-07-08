#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeElement : VisualElement
{
    //DungeonLayout dungeonLayout;
    Box box;
    DropdownField dropDownField;
    DungeonLayout dungeonLayout;

    public NodeElement(DungeonLayout dungeonLayout, VisualElement root, Vector2 mousePosition, DungeonRoom room = null)
    {
        this.dungeonLayout = dungeonLayout;
        // set the position of the node in the editor window
        this.style.position = Position.Absolute;
        this.style.left = mousePosition.x;
        this.style.top = mousePosition.y;

        // add node manipulator dragging/moving node
        this.AddManipulator(new NodeManipulator(dungeonLayout));

        CreateNode(root, dungeonLayout, room);
    }

    #region Creating/Managing Node
    /// <summary>
    /// Create a dungeon room and a visual element box as representation
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    void CreateNode(VisualElement root, DungeonLayout dungeonLayout, DungeonRoom room)
    {
        box = CreateNodeBox(dungeonLayout);
        AddNodeContextMenu(this, root, dungeonLayout);


        // Create dungeon room instance and save as an asset for persistence
        if (!room)
            userData = CreateDungeonRoomAsset(dungeonLayout);
        else
        {
            userData = room;
            name = "Room " + room.name;
        }
        // Add dropdown menu in box for room type selection
        dropDownField = CreateDropDownMenuToNode(room);
        box.Add(dropDownField);
    }

    Box CreateNodeBox(DungeonLayout layout)
    {
        // creates new box visual element
        Box box = new Box();

        // add this box to node-box class for styling by USS
        box.AddToClassList("node-box");
        box.name = "Box";

        // set to ignore so that when trying to manipulate the node the node element is picked instead of the box
        box.pickingMode = PickingMode.Ignore;


        this.Add(box);
        return box;
    }

    DropdownField CreateDropDownMenuToNode(DungeonRoom room)
    {
        // get room types
        List<RoomTypes> roomTypes;
        if (layout == null || dungeonLayout.roomTypeList.roomTypeList == null)
            roomTypes = new List<RoomTypes>();
        else
            roomTypes = dungeonLayout.roomTypeList.roomTypeList;

        // choices for dropdown menu
        List<string> choices = new List<string>();

        int defaultIndex = 0;

        // add the room types to choices
        for(int i = 0; i < roomTypes.Count; ++i)
        {
            string name = roomTypes[i].name;
            choices.Add(name);

            if(room != null && room.roomType.name == name)
                defaultIndex = i;
        }

        DropdownField menu = new DropdownField(choices, defaultIndex);
        menu.RegisterValueChangedCallback(evt =>
        {
            Debug.Log("Target: " + evt.target);

            // change room type
            // get the dropdownfield?
            VisualElement target = evt.target as VisualElement;

            // get the room to change its room type
            NodeElement node = target.parent.parent as NodeElement;
            DungeonRoom room = node.userData as DungeonRoom;


            // get new room type
            RoomTypes newRoomType = GetRoomType(evt.newValue, roomTypes);

            if (room != null && newRoomType != null)
            {
                room.roomType = newRoomType;
            }

            Debug.Log("New value: " + evt.newValue);
        });
        return menu;
    }

    /// <summary>
    /// Gets the room type from room type list with matching name as string provided
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="typeList"></param>
    /// <returns></returns>
    RoomTypes GetRoomType(string typeName, List<RoomTypes> roomTypes)
    {
        foreach (RoomTypes type in roomTypes)
        {
            if (type == null) continue;

            if (type.name == typeName) return type;
        }
        return null;
    }

    /// <summary>
    /// Create dungeon room instance and save as an asset for persistence
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="box"></param>
    DungeonRoom CreateDungeonRoomAsset(DungeonLayout layout)
    {
        // add node to current layout
        DungeonRoom room = ScriptableObject.CreateInstance<DungeonRoom>();
        room.guid = Guid.NewGuid().ToString();

        // set room type
        room.roomType = layout.roomTypeList.roomTypeList[0];
        room.name = "Room " + room.guid.ToString();
        room.styleTop = style.top.value.value;
        room.styleLeft = style.left.value.value;

        this.name = "Room " + room.name;
        

        // saves the scriptable object instance as an asset so it persists
        // https://docs.unity3d.com/ScriptReference/ScriptableObject.html
        string path = $"Assets/Rooms/{room.name}.asset";
        AssetDatabase.CreateAsset(room, path);
        AssetDatabase.SaveAssets();


        layout.dungeonRoomList.Add(room);
        EditorUtility.SetDirty(layout);
        EditorUtility.SetDirty(room);
        return room;
    }

    /// <summary>
    /// Adds context menu to nodes for adding connections
    /// </summary>
    /// <param name="canvas"></param>
    void AddNodeContextMenu(NodeElement node, VisualElement root, DungeonLayout dungeonLayout)
    {
        node.AddManipulator(new ContextualMenuManipulator((evt) =>
        {
            evt.menu.AppendAction("Connect Node", (x) => {

                // add manipulator to node for creating connection
                root.AddManipulator(new NodeConnectionManipulator(node, root, dungeonLayout));
                Debug.Log("Adding Connection");
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Remove Connection", (x) => {
                // add manipulator to node for creating connection
                root.AddManipulator(new NodeConnectionManipulator(node, root, dungeonLayout, true));
                Debug.Log("Removing Connection");
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Remove Node", (x) => {

                RemoveNode();
            }, DropdownMenuAction.AlwaysEnabled);
        }));
    }

    /// <summary>
    /// Removes and delete it's connections and dungeon room
    /// </summary>
    private void RemoveNode()
    {
        if (userData == null || layout == null) return;
        // get layout

        // get dungeon room from userdata
        // remove all connections that has this room
        dungeonLayout.RemoveAllConnections(this);

        // remove dungeon room from layout
        DungeonRoom room = userData as DungeonRoom;
        if (room == null)
            return;

        dungeonLayout.dungeonRoomList.Remove(room);

        // remove node element from layout
        dungeonLayout.nodeElements.Remove(this);

        // destroy this game object and the scriptable object
        string path = AssetDatabase.GetAssetPath(room);

        if(string.IsNullOrEmpty(path)) return;

        AssetDatabase.DeleteAsset($"Assets/Rooms/{room.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(dungeonLayout);

        this.RemoveFromHierarchy();
    }
    #endregion
}
#endif