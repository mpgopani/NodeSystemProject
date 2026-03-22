using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Moves a game object to a specified position.
    /// </summary>
    public class MoveNode : Node
    {
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Vector3 targetPosition = Vector3.zero;

        public override void Execute()
        {
            // Get the object from input port
            var inputObj = GetInputValue("Object");
            if (inputObj is GameObject obj)
            {
                targetObject = obj;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("[MoveNode] No target object");
                return;
            }

            targetObject.transform.position = targetPosition;
            Debug.Log($"[MoveNode] Moved {targetObject.name} to {targetPosition}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Move Object";

            if (inputPorts.Count == 0)
            {
                AddInputPort("Object", typeof(GameObject));
                AddOutputPort("Done", typeof(bool));
            }
        }
    }
}
