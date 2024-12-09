using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Wave.Essence.Tracker;
using Wave.Native;
using Wave.OpenXR;

namespace VRSStudio.Tracker
{
    public class VRSTrackerManager : MonoBehaviour
    {
        const string TAG = "VRSTrackerManager";
        readonly List<TrackerId> connected = new List<TrackerId>();
        readonly Dictionary<TrackerRole, TrackerId> dictForId = new Dictionary<TrackerRole, TrackerId>();
        readonly Dictionary<TrackerId, TrackerRole> dictForRole = new Dictionary<TrackerId, TrackerRole>();
        readonly Dictionary<TrackerId, onReallocateTrackerDelegate> allocatedPair = new Dictionary<TrackerId, onReallocateTrackerDelegate>();
        readonly List<onReallocateTrackerDelegate> customers = new List<onReallocateTrackerDelegate>();
        readonly List<InputDevice> inputDevices = new List<InputDevice>();

        int frameChecked = 0;

        void CheckOnceInThisFrame()
        {
            if (frameChecked == Time.frameCount) return;

            frameChecked = Time.frameCount;

            UpdateDictionary();
        }

        void OnEnable()
        {
            if (Instance == null)
                Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        public static VRSTrackerManager Instance { get; private set; }

        /// <summary>
        /// Role is used for bodytracking.  Check if trackers for your bodytracking usage are all online.
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public bool IsAllRoleAvailable(List<TrackerRole> roles)
        {
            CheckOnceInThisFrame();
            foreach (var role in roles)
            {
                if (!dictForId.TryGetValue(role, out _)) return false;
            }
            return true;
        }

        /// <summary>
        /// Role is used for bodytracking.  Check if tracker for your bodytracking usage is online.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsRoleAvailable(TrackerRole role)
        {
            CheckOnceInThisFrame();
            return dictForId.TryGetValue(role, out _);
        }

        /// <summary>
        /// Convert your role list to id list
        /// </summary>
        /// <param name="roles">Role list</param>
        /// <param name="ids">Id list</param>
        /// <returns>true if all id in role list are found.</returns>
        public bool GetAllTrackerIdByRole(List<TrackerRole> roles, out List<TrackerId> ids)
        {
            CheckOnceInThisFrame();
            ids = new List<TrackerId>();
            foreach (var role in roles)
            {
                if (dictForId.TryGetValue(role, out var id))
                    ids.Add(id);
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get all connected tracker ids.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackerId[] GetConnectedIds()
        {
            CheckOnceInThisFrame();
            return connected.ToArray();
        }


        /// <summary>
        /// Get an TrackerId which is not currently used by others.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool GetFreeId(out TrackerId id)
        {
            CheckOnceInThisFrame();
            foreach (var c in connected)
            {
                var tm = TrackerManager.Instance;
                var role = tm.GetTrackerRole(c);
                if (!allocatedPair.ContainsKey(c) && role == TrackerRole.Standalone)
                {
                    id = c;
                    return true;
                }
            }
            id = TrackerId.Tracker0 - 1;
            return false;
        }

        ///// <summary>
        ///// Get trackername from InputDevice.  Look up tracker id.
        ///// </summary>
        ///// <param name="trackerName"></param>
        ///// <param name="id"></param>
        ///// <returns>true if device is connected and found id.</returns>
        //public bool GetTrackerId(string trackerName, out TrackerId id)
        //{
        //    CheckOnceInThisFrame();
        //    var tm = TrackerManager.Instance;
        //    id = TrackerId.Tracker0 - 1;
        //    if (tm == null)
        //    {
        //        Log.e(TAG, "TrackerManager not found");
        //        return false;
        //    }
        //    for (int i = 0; i < connected.Count; i++)
        //    {
        //        tm.GetTrackerDeviceName((TrackerId)i, out string name);
        //        if (name == trackerName)
        //        {
        //            id = (TrackerId)i;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public bool GetInputDevice(TrackerId id, out InputDevice dev)
        {
            CheckOnceInThisFrame();
            InputDeviceCharacteristics chars = InputDeviceCharacteristics.TrackedDevice;
            inputDevices.Clear();
            InputDevices.GetDevicesWithCharacteristics(chars, inputDevices);

            var targetSN = InputDeviceTracker.SerialNumber((InputDeviceTracker.TrackerId)id);

            //InputDeviceTracker.TrackerId trackerId = (InputDeviceTracker.TrackerId)dev.trackerId;
            for (int i = 0; i < inputDevices.Count; i++)
            {
                try
                {
                    if (inputDevices[i].serialNumber.Equals(targetSN))
                    {
                        dev = inputDevices[i];
                        Debug.Log($"Found Tracker {inputDevices[i].serialNumber}");
                        return true;
                    }
                }
                catch (System.Exception) { }
            }
            dev = default;
            return false;
        }

        /// <summary>
        /// Check if device id is occupied by other customer.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsIdOccupied(TrackerId id)
        {
            return !allocatedPair.ContainsKey(id);
        }

        /// <summary>
        /// Use to notify customer that your trackers are occupied by other customer.
        /// </summary>
        /// <param name="rolesWillBeReallocated">if null, means all tracker are will be reallocated.</param>
        public delegate void onReallocateTrackerDelegate(List<TrackerId> rolesWillBeReallocated);

        /// <summary>
        /// If user want some trackers, means these trackers are already not in the orignal using purpose.  Notify callbacks to stop their usage.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="reallocateNotifier"></param>
        /// <returns>
        /// If no tracker available, still return true.
        /// </returns>
        public bool RequestTrackers(List<TrackerId> ids, onReallocateTrackerDelegate reallocateNotifier)
        {
            Log.d(TAG, "RequesetTracker");
            // Notify all customer who occupy the trackers
            List<onReallocateTrackerDelegate> needBeNotified = new List<onReallocateTrackerDelegate>();
            List<TrackerId> needBeRemoved = new List<TrackerId>();
            List<TrackerId> needBeAdded = new List<TrackerId>();
            foreach (var id in ids)
            {
                if (allocatedPair.ContainsKey(id))
                {
                    allocatedPair.TryGetValue(id, out var customer);
                    // If same customer request same tracker again, skip add.
                    if (customer == reallocateNotifier)
                        continue;
                    needBeAdded.Add(id);
                    needBeRemoved.Add(id);
                    // Make it unique.  Just call it once.
                    if (!needBeNotified.Contains(customer))
                    {
                        needBeNotified.Add(customer);
                    }
                }
                else
                    needBeAdded.Add(id);
            }

            foreach (var customer in needBeNotified)
            {
                customer(ids);
            }
            needBeNotified.Clear();

            foreach (var id in needBeRemoved)
            {
                allocatedPair.Remove(id);
            }
            needBeRemoved.Clear();

            foreach (var id in needBeAdded)
            {
                allocatedPair.Add(id, reallocateNotifier);
            }
            customers.Add(reallocateNotifier);

            return true;
        }

        /// <summary>
        /// Notify all cutomers that stop using any tracker.
        /// </summary>
        public void FreeAllTrackers()
        {
            var allTrackers = allocatedPair.Keys.ToList();
            foreach (var customer in customers)
            {
                customer(allTrackers);
            }
            customers.Clear();
            allocatedPair.Clear();
            return;
        }

        /// <summary>
        /// Free this cutomer's all trackers.
        /// Use this function, no notification will be received.
        /// </summary>
        /// <param name="reallocateNotifier">the customer</param>
        public void FreeAllTrackers(onReallocateTrackerDelegate reallocateNotifier)
        {
            List<TrackerId> needBeRemoved = new List<TrackerId>();
            foreach (var id in allocatedPair.Keys)
            {
                if (allocatedPair[id] != reallocateNotifier)
                    continue;
                needBeRemoved.Add(id);
            }

            foreach (var id in needBeRemoved)
            {
                allocatedPair.Remove(id);
            }
            needBeRemoved.Clear();
        }


        /// <summary>
        /// If customer request trackers, customer can release trakers requested. 
        /// Use this function, no notification will be received.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="reallocateNotifier"></param>
        public void FreeTrackers(List<TrackerId> ids, onReallocateTrackerDelegate reallocateNotifier)
        {
            List<TrackerId> needBeRemoved = new List<TrackerId>();
            foreach (var id in ids)
            {
                if (allocatedPair.ContainsKey(id))
                {
                    if (allocatedPair[id] != reallocateNotifier)
                        continue;
                    needBeRemoved.Add(id);
                }
            }

            foreach (var id in needBeRemoved)
            {
                allocatedPair.Remove(id);
            }
            needBeRemoved.Clear();
        }

        void UpdateDictionary()
        {
            dictForRole.Clear();
            dictForId.Clear();
            connected.Clear();
            var tm = TrackerManager.Instance;
            if (tm == null)
            {
                if (Log.gpl.Print)
                    Log.e(TAG, "TrackerManager not found");
                return;
            }

            var stateRequired = UnityEngine.XR.InputTrackingState.Rotation/* | UnityEngine.XR.InputTrackingState.Position*/;
            if (tm.GetTrackerStatus() == TrackerManager.TrackerStatus.Available)
            {
                //Log.gpl.d(TAG, "Has traker manager");
                for (int i = 0; i < 16; i++)
                {
                    var trackerId = (TrackerId)i;
                    tm.GetTrackerTrackingState(trackerId, out var state);
                    if (state != InputTrackingState.None)
                        Log.gpl.d(TAG, $"GetTrackerTrackingState {trackerId}: {state}");
                    if ((state & stateRequired) != stateRequired)
                        continue;
                    var role = tm.GetTrackerRole(trackerId);
                    Log.gpl.d(TAG, $"GetTrackerRole {trackerId}: {role}");

                    connected.Add(trackerId);
                    if (role != TrackerRole.Undefined)
                    {
                        dictForRole[trackerId] = role;
                        dictForId[role] = trackerId;
                    }
                }
            }
        }
    }
}