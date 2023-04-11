using System;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Text.Json;
using DevoidEngine.Engine.Serializing.Converters;
using System.Text.Json.Nodes;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace DevoidEngine.Engine.Serializing
{
    public class Serializer
    {
        public static JsonObject Serialize(Scene scene)
        {
            JsonObject SceneJson = new JsonObject();

            GameObject[] gameObjects = scene.GetSceneRegistry().GetAllGameObjects();

            JsonObject[] GameObjectsJSON = new JsonObject[gameObjects.Length];

            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObjectsJSON[i] = SerializeGameObject(gameObjects[i]);
            }

            SceneJson.Add("Name", scene.SceneName);

            SceneJson.Add("GameObjects", new JsonArray(GameObjectsJSON));

            return SceneJson;
        }

        public static JsonObject SerializeGameObject(GameObject gameObject)
        {
            Component[] components = gameObject.GetAllComponents();

            JsonObject jObject = new JsonObject();

            //POS
            JsonObject position = new JsonObject();
            position.Add("X", gameObject.transform.position.X);
            position.Add("Y", gameObject.transform.position.Y);
            position.Add("Z", gameObject.transform.position.Z);

            //ROT
            JsonObject rotation = new JsonObject();
            rotation.Add("X", gameObject.transform.rotation.X);
            rotation.Add("Y", gameObject.transform.rotation.Y);
            rotation.Add("Z", gameObject.transform.rotation.Z);

            //SCALE
            JsonObject scale = new JsonObject();
            scale.Add("X", gameObject.transform.scale.X);
            scale.Add("Y", gameObject.transform.scale.Y);
            scale.Add("Z", gameObject.transform.scale.Z);

            jObject.Add("Name", gameObject.name);
            jObject.Add("ID", gameObject.ID);
            jObject.Add("Position", position);
            jObject.Add("Rotation", rotation);
            jObject.Add("Scale", scale);

            List<JsonObject> ComponentsJSON = new List<JsonObject>();

            for (int i = 0; i < components.Length; i++)
            {
                JsonObject cObj = SerializeComponent(components[i]);
                if (cObj != null)
                    ComponentsJSON.Add(cObj);
            }

            jObject.Add("Components", new JsonArray(ComponentsJSON.ToArray()));

            return jObject;
        }

        public static JsonObject SerializeComponent(Component component)
        {
            if (component.GetType() == typeof(Transform)) return null;

            JsonObject ComponentJSON = new JsonObject();

            ComponentJSON.Add("Type", component.Type);

            FieldInfo[] fieldInfo = component.GetType().GetFields();

            for (int i = 0; i < fieldInfo.Length; i++)
            {
                FieldInfo field = fieldInfo[i];
                string fieldName = field.Name;
                object fieldValue = field.GetValue(component);

                if (field.FieldType == typeof(int))
                {
                    ComponentJSON.Add(fieldName, (int)fieldValue);
                }
                else if (field.FieldType == typeof(float))
                {
                    ComponentJSON.Add(fieldName, (float)fieldValue);
                }
                else if (field.FieldType == typeof(string))
                {
                    ComponentJSON.Add(fieldName, (string)fieldValue);
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    ComponentJSON.Add(fieldName, SerializeField((Vector3)fieldValue));
                }
                else if (field.FieldType == typeof(bool))
                {
                    ComponentJSON.Add(fieldName, (bool)fieldValue);
                }
                else if (field.FieldType == typeof(Color4))
                {
                    ComponentJSON.Add(fieldName, SerializeField((Color4)fieldValue));
                }
                else if (field.FieldType.IsEnum)
                {
                    ComponentJSON.Add(fieldName, (int)fieldValue);
                }
                else if (field.FieldType == typeof(Texture))
                {
                    ComponentJSON.Add(fieldName, "Texture");
                }
                else if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    JsonArray jsonArray = new JsonArray();

                    if (fieldValue is IEnumerable enumerable)
                    {
                        List<object> myList = enumerable.Cast<object>().ToList(); // Convert the value to a list of objects

                        for (int y = 0; y < myList.Count; y++)
                        {
                            jsonArray.Add(myList[y]);
                        }

                    }

                    ComponentJSON.Add(fieldName, jsonArray);
                }
                else
                {
                    //Console.WriteLine(field.FieldType);
                }
            }
            return ComponentJSON;
        }

        public static JsonObject SerializeFields(FieldInfo field, object fieldValue)
        {


            return null;
        }

        public static JsonObject SerializeField(Vector3 value)
        {
            JsonObject vObject = new JsonObject();
            vObject.Add("X", value.X);
            vObject.Add("Y", value.Y);
            vObject.Add("Z", value.Z);
            return vObject;

        }

        public static JsonObject SerializeField(Color4 value)
        {
            JsonObject vObject = new JsonObject();
            vObject.Add("R", value.R);
            vObject.Add("G", value.G);
            vObject.Add("B", value.B);
            vObject.Add("A", value.A);
            return vObject;
        }

    }
}
