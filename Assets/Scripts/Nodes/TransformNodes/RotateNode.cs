using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Rotates a game object by specified euler angles.
    /// </summary>
    public class RotateNode : Node
    {
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Vector3 rotationEuler = Vector3.zero;

        public override void Execute()
        {
            var inputObj = GetInputValue("Object");
            if (inputObj is GameObject obj)
            {
                targetObject = obj;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("[RotateNode] No target object");
                return;
            }

            targetObject.transform.rotation = Quaternion.Euler(rotationEuler);
            Debug.Log($"[RotateNode] Rotated {targetObject.name} to {rotationEuler}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Rotate Object";

            if (inputPorts.Count == 0)
            {
                AddInputPort("Object", typeof(GameObject));
                AddOutputPort("Done", typeof(bool));
            }
        }
    }
}
