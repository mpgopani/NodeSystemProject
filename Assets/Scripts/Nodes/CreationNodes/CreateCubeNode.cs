using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Creates a cube in the scene and outputs its reference.
    /// </summary>
    public class CreateCubeNode : Node
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private GameObject createdObject;

        public override void Execute()
        {
            createdObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            createdObject.transform.position = position;
            createdObject.name = "CreatedCube";

            Debug.Log($"[CreateCubeNode] Created cube at {position}");
        }

        public override object GetOutputValue(string portName)
        {
            if (portName == "Object")
                return createdObject;
            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Create Cube";

            // Clear ports and recreate them
            if (outputPorts.Count == 0)
            {
                AddOutputPort("Object", typeof(GameObject));
            }
        }
    }
}
