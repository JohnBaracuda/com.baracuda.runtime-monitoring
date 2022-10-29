using Baracuda.Monitoring;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Example.Scripts
{
    [RequireComponent(typeof(Button))]
    public class ButtonFilter : MonoBehaviour
    {
        [SerializeField] private bool isReset;

        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                GetComponentInChildren<Text>().text = _filter;
            }
        }
        private string _filter;

        private void Awake()
        {
            _filter = GetComponentInChildren<Text>().text;

            var button = GetComponent<Button>();

            if (isReset)
            {
                button.onClick.AddListener(() => Monitor.UI.ResetFilter());
            }
            else
            {
                button.onClick.AddListener(() => Monitor.UI.ApplyFilter(Filter));
            }
        }
    }
}