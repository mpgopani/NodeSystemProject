using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Retrieves a stored value by name from the global value store (set by SetValueNode).
    /// Outputs the value so other nodes can consume it.
    /// </summary>
    public class GetValueNode : Node
    {
        [SerializeField] private string variableName = "myVar";

        private float cachedValue = 0f;

        public override void Execute()
        {
            cachedValue = SetValueNode.GetGlobalValue(variableName);
            Debug.Log($"[GetValueNode] Got {variableName} = {cachedValue}");
        }

        public override object GetOutputValue(string portName)
        {
            if (portName == "Value")
                return cachedValue;
            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Get Value";

            if (outputPorts.Count == 0)
            {
                AddOutputPort("Value", typeof(float));
            }
        }
    }
}
