using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Components;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Serializing
{
    public class Deserializer
    {

        public static Scene Deserialize(string file_src)
        {
            JsonObject json = JsonObject.Parse(file_src).AsObject();

            Scene scene = new Scene();
            scene.SceneName = (string)json["Name"];

            JsonArray gObjects = json["GameObjects"].AsArray();

            for (int i = 0; i < gObjects.Count; i++) 
            {
                JsonObject gObject = gObjects[i].AsObject();
                GameObject gameObject = scene.NewGameObject((string)gObject["Name"]);
                gameObject.ID = (int)gObject["ID"];
                gameObject.transform.position = DeserializeVector3(gObject["Position"]);
                gameObject.transform.rotation = DeserializeVector3(gObject["Rotation"]);
                gameObject.transform.scale = DeserializeVector3(gObject["Scale"]);

                JsonArray cObjects = gObject["Components"].AsArray();

                for (int x = 0; x < cObjects.Count; x++)
                {
                    JsonObject cObject = cObjects[x].AsObject();

                    Type type = Type.GetType("DevoidEngine.Engine.Components." + (string)cObject["Type"]);

                    if (type == null) continue;

                    Component component = (Component)Activator.CreateInstance(type);

                    FieldInfo[] fields = component.GetType().GetFields();

                    for (int y = 0; y < fields.Length; y++)
                    {
                        FieldInfo field = fields[y];
                        string fieldName = field.Name;
                        JsonNode fieldValue = cObject[fieldName];

                        if (field.FieldType == typeof(int))
                        {
                            field.SetValue(component, fieldValue.GetValue<int>());
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            field.SetValue(component, fieldValue.GetValue<float>());
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            field.SetValue(component, fieldValue.GetValue<string>());
                        }
                        else if (field.FieldType == typeof(Vector2))
                        {
                            field.SetValue(component, DeserializeVector2(fieldValue));
                        }
                        else if (field.FieldType == typeof(Vector3))
                        {
                            field.SetValue(component, DeserializeVector3(fieldValue));
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            field.SetValue(component, fieldValue.GetValue<bool>());
                        }
                        else if (field.FieldType == typeof(Color4))
                        {
                            field.SetValue(component, DeserializeColor4(fieldValue));
                        }
                        else if (field.FieldType == typeof(List<Mesh>))
                        {
                            JsonArray jArray = fieldValue.AsArray();

                            List<Mesh> myList = new List<Mesh>();

                            for (int z = 0; z < jArray.Count; z++)
                            {
                                string fileName = (string)jArray[z];
                                Console.WriteLine(fileName);

                                Mesh[] meshes = (Mesh[])Resources.Load(fileName);
                                if (meshes.Length > 0)
                                {
                                    myList.AddRange(meshes);
                                }
                            }
                            field.SetValue(component, myList);
                        }
                        else if (field.FieldType.IsEnum)
                        {

                        }
                        else if (field.FieldType == typeof(Texture))
                        {
                            field.SetValue(component, (Texture)Resources.Load((string)fieldValue));
                        }
                        else if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                        {
                            JsonArray jArray = fieldValue.AsArray();

                            List<object> myList = new List<object>();

                            for (int z = 0; z < jArray.Count; z++)
                            {
                                myList.Add(jArray[z]);
                            }

                            Type elementType = field.FieldType.GetGenericArguments()[0];

                            //List<object> myTypedList = myList.Select(x => Convert.ChangeType(x, elementType)).ToList();

                            //field.SetValue(component,myTypedList);
                        }
                        else
                        {

                        }
                    }

                    gameObject.AddComponent(component);

                }
            }

            return scene;
        }

        static Vector3 DeserializeVector3(JsonNode value)
        {
            return new Vector3((float)value["X"], (float)value["Y"], (float)value["Z"]);
        }
        static Vector2 DeserializeVector2(JsonNode value)
        {
            return new Vector2((float)value["X"], (float)value["Y"]);
        }

        static Color4 DeserializeColor4(JsonNode value)
        {
            return new Color4((float)value["R"], (float)value["G"], (float)value["B"], (float)value["A"]);
        }

    }
}
