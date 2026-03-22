using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Creates a cylinder in the scene.
    /// </summary>
    public class CreateCylinderNode : Node
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private GameObject createdObject;

        public override void Execute()
        {
            createdObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            createdObject.transform.position = position;
            createdObject.name = "CreatedCylinder";

            Debug.Log($"[CreateCylinderNode] Created cylinder at {position}");
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
            nodeName = "Create Cylinder";

            if (outputPorts.Count == 0)
            {
                AddOutputPort("Object", typeof(GameObject));
            }
        }
    }
}
