using System;
using System.Collections.Generic;

namespace DevoidEngine.Elemental.EditorAttributes
{
    class EditorCustomScript : Attribute
    {
        public Type type, type1;
        public EditorCustomScript(Type type, Type type1)
        {
            this.type = type;
            this.type1 = type1;
        }
    }
}
