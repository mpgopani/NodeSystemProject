using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Stores a value that can be retrieved by other nodes.
    /// </summary>
    public class SetValueNode : Node
    {
        [SerializeField] private string variableName = "myVar";
        [SerializeField] private float floatValue = 0f;

        private static System.Collections.Generic.Dictionary<string, object> globalValues =
            new System.Collections.Generic.Dictionary<string, object>();

        public override void Execute()
        {
            globalValues[variableName] = floatValue;
            Debug.Log($"[SetValueNode] Set {variableName} = {floatValue}");
        }

        public override object GetOutputValue(string portName)
        {
            if (portName == "Value")
            {
                globalValues.TryGetValue(variableName, out var value);
                return value;
            }
            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Set Value";

            if (outputPorts.Count == 0)
            {
                AddOutputPort("Value", typeof(float));
            }
        }

        public static float GetGlobalValue(string name, float defaultValue = 0f)
        {
            if (globalValues.TryGetValue(name, out var value) && value is float f)
                return f;
            return defaultValue;
        }
    }
}
