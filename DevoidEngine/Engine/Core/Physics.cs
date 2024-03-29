﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using System.Diagnostics.CodeAnalysis;

namespace DevoidEngine.Engine.Core
{
    public unsafe struct NarrowPhaseCallback : INarrowPhaseCallbacks
    {
        public SpringSettings ContactSpringiness;
        public float MaximumRecoveryVelocity;
        public float FrictionCoefficient;

        public void Initialize(Simulation simulation) 
        {

            if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
            {
                ContactSpringiness = new(30, 1);
                MaximumRecoveryVelocity = 2f;
                FrictionCoefficient = 1f;
            }

        }

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin) 
        {
            Console.Write(a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic);
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = FrictionCoefficient;
            pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
            pairMaterial.SpringSettings = ContactSpringiness;
            return true;
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }

    public unsafe struct PoseIntegratorCallback : IPoseIntegratorCallbacks
    {
        Vector3Wide gravity;

        public AngularIntegrationMode AngularIntegrationMode => new();

        public bool AllowSubstepsForUnconstrainedBodies => true;

        public bool IntegrateVelocityForKinematics => true;

        public void Initialize(Simulation simulation) { }

        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity) {

            velocity.Linear = (velocity.Linear + gravity);
        }

        public void PrepareForIntegration(float dt) {

            gravity = Vector3Wide.Broadcast(new System.Numerics.Vector3(0, -9.8f, 0) * dt);

        }
    }

    public class Physics
    {
        Simulation Simulation;
        public float timeStepDuration = 0f;

        public void Init()
        {
            Simulation = new Simulation();

            Simulation = Simulation.Create(new BufferPool(), new NarrowPhaseCallback(), new PoseIntegratorCallback(), new SolveDescription(4, 1));
        }

        public void Update(float deltaTime)
        {
            Simulation.Timestep(1/60f);
        }

        public TypedIndex AddCollider<TShape>(TShape shape) where TShape : unmanaged, IShape
        {
            return Simulation.Shapes.Add(shape);
        }

        public void RemoveCollider(TypedIndex index)
        {
            Simulation.Shapes.Remove(index);
        }

        public IShape GetCollider<TShape>(TypedIndex tIndex) where TShape: unmanaged, IShape
        {
            return Simulation.Shapes.GetShape<TShape>(tIndex.Index);
        }

        public void RemoveStatic(StaticReference staticReference)
        {
            Simulation.Statics.Remove(staticReference.Handle);
        }

        public void RemoveBody(BodyReference bodyReference)
        {
            Simulation.Bodies.Remove(bodyReference.Handle);
        }

        public BodyReference AddDynamicBody(OpenTK.Mathematics.Vector3 position, OpenTK.Mathematics.Vector3 rotation, BodyInertia bodyInertia, TypedIndex colliderIndex)
        {
            BodyDescription bDesc = BodyDescription.CreateDynamic(new RigidPose(Vector3ToNumerics(position), PhysicsHelper.ToQuaternion(Vector3ToNumerics(rotation))), bodyInertia, colliderIndex, 0.01f);

            return Simulation.Bodies.GetBodyReference(Simulation.Bodies.Add(bDesc));
        }
        public StaticReference AddStaticBody(OpenTK.Mathematics.Vector3 position, OpenTK.Mathematics.Vector3 rotation, TypedIndex colliderIndex)
        {
            StaticHandle sHandle = Simulation.Statics.Add(new StaticDescription(Vector3ToNumerics(position), PhysicsHelper.ToQuaternion(Vector3ToNumerics(rotation)), colliderIndex));

            return Simulation.Statics.GetStaticReference(sHandle);
        }

        public static System.Numerics.Vector3 Vector3ToNumerics(OpenTK.Mathematics.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static OpenTK.Mathematics.Vector3 NumericsToVector3(System.Numerics.Vector3 vector)
        {
            return new OpenTK.Mathematics.Vector3(vector.X, vector.Y, vector.Z);
        }
    }

    public static class PhysicsHelper 
    {
        public static System.Numerics.Quaternion ToQuaternion(System.Numerics.Vector3 Euler)
        {
            double cy = Math.Cos(Euler.Z * 0.5);
            double sy = Math.Sin(Euler.Z * 0.5);
            double cp = Math.Cos(Euler.Y * 0.5);
            double sp = Math.Sin(Euler.Y * 0.5);
            double cr = Math.Cos(Euler.X * 0.5);
            double sr = Math.Sin(Euler.X * 0.5);

            System.Numerics.Quaternion q = new System.Numerics.Quaternion();
            q.W = (float)(cr * cp * cy + sr * sp * sy);
            q.X = (float)(sr * cp * cy - cr * sp * sy);
            q.Y = (float)(cr * sp * cy + sr * cp * sy);
            q.Z = (float)(cr * cp * sy - sr * sp * cy);

            return q;
        }

        public static System.Numerics.Vector3 ToEulerAngles(System.Numerics.Quaternion q)
        {
            System.Numerics.Vector3 angles = new();
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.Y * q.Y + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);
            return angles;
        }


    }
}
