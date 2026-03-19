using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Variables")]
    public class TraceVariables : MonoBehaviour
    {
        [Serializable]
        public class Variable
        {
            public string name;
            public string type = "number";
            public string value = "0";
        }

        public List<Variable> variables = new List<Variable>();

        public JObject SerializeToBehavior()
        {
            var arr = new JArray();
            foreach (var v in variables)
            {
                var obj = new JObject
                {
                    new JProperty("name", v.name),
                    new JProperty("type", v.type),
                };
                switch (v.type)
                {
                    case "number":
                        float.TryParse(v.value, out var fval);
                        obj.Add("value", fval);
                        break;
                    case "boolean":
                        obj.Add("value", v.value == "true" || v.value == "True");
                        break;
                    default:
                        obj.Add("value", v.value);
                        break;
                }
                arr.Add(obj);
            }
            return new JObject
            {
                new JProperty("@type", "Variables"),
                new JProperty("variables", arr),
            };
        }
    }
}
