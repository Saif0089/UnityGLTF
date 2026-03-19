using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Interactions")]
    public class TraceInteractions : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class Action
        {
            public string type = "setActive";
            public GameObject target;
            public bool state = true;
            public int index = 0;
            public string clip;
            public string url;
            public string property;
            public float speed = 1f;
            public float volume = 1f;
            public string eventName;
            public string value;
        }

        [Serializable]
        public class Interaction
        {
            public string on = "select";
            public float distance = 5f;
            public List<Action> enter = new List<Action>();
            public List<Action> exit = new List<Action>();
        }

        public List<Interaction> interactions = new List<Interaction>();

        private void Start()
        {
            foreach (var interaction in interactions)
            {
                if (interaction.on == "start")
                    ExecuteActions(interaction.enter);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.on == "select")
                    ExecuteActions(interaction.enter);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.on == "hover")
                    ExecuteActions(interaction.enter);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var interaction in interactions)
            {
                if (interaction.on == "hover")
                    ExecuteActions(interaction.exit);
            }
        }

        private void ExecuteActions(List<Action> actions)
        {
            foreach (var action in actions)
            {
                var target = action.target != null ? action.target : gameObject;
                switch (action.type)
                {
                    case "setActive":
                        target.SetActive(action.state);
                        break;
                    case "setActiveChild":
                        var t = target.transform;
                        for (int i = 0; i < t.childCount; i++)
                            t.GetChild(i).gameObject.SetActive(i == action.index);
                        break;
                    case "animation":
                        var anim = target.GetComponent<Animation>();
                        if (anim != null)
                        {
                            if (!string.IsNullOrEmpty(action.clip))
                                anim.Play(action.clip);
                            else
                                anim.Play();
                        }
                        break;
                    case "navigate":
                        if (!string.IsNullOrEmpty(action.url))
                            Application.OpenURL(action.url);
                        break;
                }
            }
        }

        public JObject SerializeToBehavior()
        {
            var interactionsArray = new JArray();
            foreach (var interaction in interactions)
            {
                var obj = new JObject { new JProperty("on", interaction.on) };
                if (interaction.on == "proximity")
                    obj.Add("distance", interaction.distance);
                if (interaction.enter.Count > 0)
                    obj.Add("enter", SerializeActions(interaction.enter));
                if (interaction.exit.Count > 0)
                    obj.Add("exit", SerializeActions(interaction.exit));
                interactionsArray.Add(obj);
            }
            return new JObject
            {
                new JProperty("@type", "Interactions"),
                new JProperty("interactions", interactionsArray),
            };
        }

        private JArray SerializeActions(List<Action> actions)
        {
            var arr = new JArray();
            foreach (var a in actions)
            {
                var obj = new JObject { new JProperty("type", a.type) };
                if (a.target != null)
                    obj.Add("target", a.target.name);
                switch (a.type)
                {
                    case "setActive":
                        obj.Add("state", a.state);
                        break;
                    case "setActiveChild":
                        obj.Add("index", a.index);
                        break;
                    case "animation":
                        if (!string.IsNullOrEmpty(a.clip)) obj.Add("clip", a.clip);
                        if (a.speed != 1f) obj.Add("speed", a.speed);
                        break;
                    case "navigate":
                        obj.Add("url", a.url);
                        break;
                    case "setProperty":
                        obj.Add("property", a.property);
                        obj.Add("value", a.value);
                        break;
                    case "dispatch":
                        obj.Add("event", a.eventName);
                        break;
                    case "video":
                        obj.Add("play", a.state);
                        if (a.volume >= 0) obj.Add("volume", a.volume);
                        break;
                }
                arr.Add(obj);
            }
            return arr;
        }
    }
}
