using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

//https://docs.unity3d.com/Manual/UIE-drag-across-windows.html
//https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.PointerManipulator.html
//https://docs.unity3d.com/Manual/UIE-manipulators.html
public class NodeManipulator : PointerManipulator
{
    bool dragging;
    Vector3 start;
    DungeonLayout layout;
    public NodeManipulator(DungeonLayout layout)
    {
        this.layout = layout;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.MiddleMouse });
        dragging = false;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    private void OnPointerDown(PointerDownEvent e)
    {
        if (!CanStartManipulation(e))
        {
            return;
        }

        dragging = true;
        start = e.localPosition;
        target.CapturePointer(e.pointerId);
        e.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent e)
    {
        if (!dragging || !CanStopManipulation(e))
        {
            return;
        }

        DungeonRoom room = target.userData as DungeonRoom;
        if(room != null) 
        {
            // updating position of dungeon room's visual element representation
            room.styleTop = target.style.top.value.value;
            room.styleLeft = target.style.left.value.value;
        }

        dragging = false;
        target.ReleasePointer(e.pointerId);
        e.StopPropagation();
    }
    private void OnPointerMove(PointerMoveEvent e)
    {
        if (!dragging || !target.HasPointerCapture(e.pointerId))
            return;

        Vector2 diff = e.localPosition - start;
        target.style.left = target.layout.x + diff.x;
        target.style.top = target.layout.y + diff.y;
        target.style.position = Position.Absolute;

        // update all connections
        NodeElement node = target as NodeElement;
        layout.UpdateConnection(node);
    }
}
