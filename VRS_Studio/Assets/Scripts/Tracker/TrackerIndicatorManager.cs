using System.Collections.Generic;
using UnityEngine;
using VRSStudio.Common;
using Wave.Essence.Tracker;

namespace VRSStudio.Tracker
{
    public class TrackerIndicatorManager : MonoBehaviour
    {
        readonly Dictionary<TrackerId, TrackerIndicator> dict = new Dictionary<TrackerId, TrackerIndicator>();

        Timer updateTimer = new Timer(0.5f);

        public GameObject indicatorPrefab;

        private void OnEnable()
        {
            if (indicatorPrefab == null)
                enabled = false;
        }

        private void OnDisable()
        {
            foreach (var indicator in dict.Values)
            {
                Destroy(indicator.gameObject);
            }
            dict.Clear();
        }

        void Update()
        {
            if (!updateTimer.IsSet)
                updateTimer.Set();
            if (!updateTimer.Check())
                return;

            var tm = VRSTrackerManager.Instance;
            if (tm == null) return;

            var connected = tm.GetConnectedIds();
            foreach (var connectedId in connected)
            {
                if (dict.TryGetValue(connectedId, out TrackerIndicator indicator))
                {
                    if (indicator.GetTrackerId() != (int)connectedId)
                    {
                        tm.GetInputDevice(connectedId, out var dev);
                        indicator.SetDevice((int)connectedId, dev, connectedId.ToString());
                    }
                }
                else
                {
                    var instance = Instantiate(indicatorPrefab);
                    indicator = instance.GetComponent<TrackerIndicator>();
                    indicator.name = connectedId.ToString();
                    tm.GetInputDevice(connectedId, out var dev);
                    indicator.SetDevice((int)connectedId, dev, connectedId.ToString());
                    dict.Add(connectedId, indicator);
                }
            }
        }
    }
}