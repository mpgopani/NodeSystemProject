using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Scales a game object to specified scale values.
    /// </summary>
    public class ScaleNode : Node
    {
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Vector3 targetScale = Vector3.one;

        public override void Execute()
        {
            var inputObj = GetInputValue("Object");
            if (inputObj is GameObject obj)
            {
                targetObject = obj;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("[ScaleNode] No target object");
                return;
            }

            targetObject.transform.localScale = targetScale;
            Debug.Log($"[ScaleNode] Scaled {targetObject.name} to {targetScale}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Scale Object";

            if (inputPorts.Count == 0)
            {
                AddInputPort("Object", typeof(GameObject));
                AddOutputPort("Done", typeof(bool));
            }
        }
    }
}
