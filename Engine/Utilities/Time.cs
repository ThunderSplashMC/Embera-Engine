using System;
using System.Collections.Generic;

namespace DevoidEngine.Engine.Utilities
{
    class Time
    {
        public float deltaTime;

        public Time()
        {

        }

        public void SetDeltaTime(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}
