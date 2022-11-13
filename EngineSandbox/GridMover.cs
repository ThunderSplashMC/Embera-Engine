using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.EngineSandbox
{
    class GridMover : Component
    {

        Vector2 GridPos;
        Vector2 StartPos;
        Vector2 Direction;
        Vector2 PrevDirection;

        bool ismoving;

        public float lerpDuration = 2f;

        public override void OnStart()
        {

            GridPos = new Vector2(gameObject.transform.position.X + 2, gameObject.transform.position.Z);
            StartPos = new Vector2(gameObject.transform.position.X, gameObject.transform.position.Z);

            base.OnStart();
        }

        float timeelapsed;

        public override void OnUpdate(float deltaTime)
        {

            //if (Input.GetKeyDown(KeyCode.W))
            //    Direction = Vector2.UnitY;
            //if (Input.GetKeyDown(KeyCode.A))
            //    Direction = -Vector2.UnitX;
            //if (Input.GetKeyDown(KeyCode.S))
            //    Direction = Vector2.UnitY;
            //if (Input.GetKeyDown(KeyCode.D))
            //    Direction = Vector2.UnitX;

            if (Input.GetKeyDown(KeyCode.W))
                move(Vector2.UnitY);
            if (Input.GetKeyDown(KeyCode.A))
                move(-Vector2.UnitX);
            if (Input.GetKeyDown(KeyCode.S))
                move(-Vector2.UnitY);
            if (Input.GetKeyDown(KeyCode.D))
                move(Vector2.UnitX);


            if (timeelapsed < lerpDuration)
            {
                Vector2 lerppos = Vector2.Lerp(StartPos, GridPos, timeelapsed/lerpDuration);
                gameObject.transform.position = new Vector3(lerppos.X, gameObject.transform.position.Y, lerppos.Y);
                timeelapsed += deltaTime;
            } else
            {
                ismoving = false;
            }

            base.OnUpdate(deltaTime);
        }

        public void move(Vector2 direction)
        {
            if (ismoving) { return; }
            timeelapsed = 0;
            GridPos = new Vector2(gameObject.transform.position.X, gameObject.transform.position.Z) + direction;
            StartPos = new Vector2(gameObject.transform.position.X, gameObject.transform.position.Z);
            PrevDirection = Direction;
            ismoving = true;
        }

    }
}
