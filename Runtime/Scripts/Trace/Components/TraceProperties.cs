using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Properties")]
    public class TraceProperties : MonoBehaviour
    {
        [Serializable]
        public class Property
        {
            public string name;
            public string type = "number";
            public string value = "0";
            public string label;
            public float min = 0;
            public float max = 1;
            public float step = 0.01f;
        }

        public List<Property> properties = new List<Property>();

        public JObject SerializeToBehavior()
        {
            var arr = new JArray();
            foreach (var p in properties)
            {
                var obj = new JObject
                {
                    new JProperty("name", p.name),
                    new JProperty("type", p.type),
                };
                switch (p.type)
                {
                    case "number":
                        float.TryParse(p.value, out var fval);
                        obj.Add("value", fval);
                        obj.Add("min", p.min);
                        obj.Add("max", p.max);
                        obj.Add("step", p.step);
                        break;
                    case "boolean":
                        obj.Add("value", p.value == "true" || p.value == "True");
                        break;
                    default:
                        obj.Add("value", p.value);
                        break;
                }
                if (!string.IsNullOrEmpty(p.label))
                    obj.Add("label", p.label);
                arr.Add(obj);
            }
            return new JObject
            {
                new JProperty("@type", "Properties"),
                new JProperty("properties", arr),
            };
        }
    }
}
