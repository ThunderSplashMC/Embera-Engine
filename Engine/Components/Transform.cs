using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class Transform : Component
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.One;

        private Matrix4 transformMatrix;

        public Matrix4 GetTransform()
        {
            Matrix4 transformMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            transformMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            transformMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));

            transformMatrix *= Matrix4.CreateScale(scale);
            transformMatrix *= Matrix4.CreateTranslation(position);

            return transformMatrix;
        }

        public void SetTransform(Matrix4 transform)
        {
            transformMatrix = transform;

        }

        public override void OnUpdate(float deltaTime)
        {
            transformMatrix = Matrix4.CreateTranslation(position);
            base.OnUpdate(deltaTime);
        }
    }
}
