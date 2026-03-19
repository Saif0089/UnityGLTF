using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Pages")]
    public class TracePages : MonoBehaviour
    {
        public float autoplayPageDuration = 0f;
        public bool autoloop = false;
        public bool nested = false;

        private int _currentPage = 0;
        private float _timer = 0f;

        private void Start()
        {
            ShowPage(0);
        }

        private void Update()
        {
            if (autoplayPageDuration > 0)
            {
                _timer += Time.deltaTime;
                if (_timer >= autoplayPageDuration)
                {
                    _timer = 0;
                    NextPage();
                }
            }
        }

        public void ShowPage(int index)
        {
            _currentPage = index;
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(i == index);
        }

        public void NextPage()
        {
            var next = _currentPage + 1;
            if (next >= transform.childCount)
                next = autoloop ? 0 : transform.childCount - 1;
            ShowPage(next);
        }
    }
}
