using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.Math;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Core
{
    class Physics
    {
        CollisionConfiguration CollisionConfig;
        CollisionDispatcher CollisionDispatcher;

        DbvtBroadphase DbvtBroadphase;

        List<CollisionShape> CollisionShapes = new List<CollisionShape>();

        public DiscreteDynamicsWorld PhysicsWorld;

        public void Init()
        {
            CollisionConfig = new DefaultCollisionConfiguration();
            CollisionDispatcher = new CollisionDispatcher(CollisionConfig);

            DbvtBroadphase = new DbvtBroadphase();
            PhysicsWorld = new DiscreteDynamicsWorld(CollisionDispatcher, DbvtBroadphase, null, CollisionConfig);
            PhysicsWorld.Gravity = new BulletSharp.Math.Vector3(0, -10, 0);

        }

        float elapsedTime = 0f;

        public void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            PhysicsWorld.StepSimulation(elapsedTime);

        }

        public RigidBody CreateRigidBody(float mass, Matrix4 transform, CollisionShape shape)
        {
            bool isDynamic = (mass != 0.0f);

            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;

            if (isDynamic)
            {
                shape.CalculateLocalInertia(mass, out localInertia);
            }

            
            DefaultMotionState motionState = new DefaultMotionState(ConvertOpenTKMatrix4ToBullet(transform));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, motionState, shape, localInertia);

            RigidBody body = new RigidBody(rbInfo);

            PhysicsWorld.AddRigidBody(body);

            return body;
        }

        public BulletSharp.Math.Matrix ConvertOpenTKMatrix4ToBullet(Matrix4 transform)
        {
            return new Matrix(transform.M11, transform.M12, transform.M13, transform.M14, transform.M21, transform.M22, transform.M23, transform.M24, transform.M31, transform.M32, transform.M33, transform.M34, transform.M41, transform.M42, transform.M43, transform.M44);
        }

        public Matrix4 ConvertBulletMatrix4ToOpenTK(BulletSharp.Math.Matrix transform)
        {
            return new Matrix4(transform.M11, transform.M12, transform.M13, transform.M14, transform.M21, transform.M22, transform.M23, transform.M24, transform.M31, transform.M32, transform.M33, transform.M34, transform.M41, transform.M42, transform.M43, transform.M44);
        }
    }
}
