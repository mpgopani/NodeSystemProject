using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Creates a sphere in the scene.
    /// </summary>
    public class CreateSphereNode : Node
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private GameObject createdObject;

        public override void Execute()
        {
            createdObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            createdObject.transform.position = position;
            createdObject.name = "CreatedSphere";

            Debug.Log($"[CreateSphereNode] Created sphere at {position}");
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
            nodeName = "Create Sphere";

            if (outputPorts.Count == 0)
            {
                AddOutputPort("Object", typeof(GameObject));
            }
        }
    }
}
