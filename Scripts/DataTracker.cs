using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JBirdLib {

	namespace DataTracking {

        /// <summary>
        /// Interface for making a class have trackable data.
        /// </summary>
        public interface ITrackableData<DataType> {
            DataType GetData();
        }

        /// <summary>
        /// A class that will track data of any object that susbcribes to it.
        /// </summary>
        [System.Serializable]
        public class DataTracker<SubscriberType, DataType> : JBehaviour where SubscriberType : ITrackableData<DataType> {

            public Dictionary<SubscriberType,List<DataType>> subscriberHistory { get; private set; }
            [Min(0f)]
            public float dataTrackingTimestep;
            [Min(1)]
            public int maxHistoryLength;
            public bool autoUpdate = true;
            private float timestep;

            void Awake () {
                subscriberHistory = new Dictionary<SubscriberType, List<DataType>>();
            }

            /// <summary>
            /// Adds the specified instance as a subscriber to this DataTracker.
            /// </summary>
            public bool Subscribe (SubscriberType subscriber) {
                if (subscriberHistory.Keys.Contains(subscriber)) {
                    Debug.LogError(string.Format("{0}: Attempting to subscribe to {1} when already subscribed!", subscriber.GetType(), GetType()));
                    return false;
                }
                else {
                    subscriberHistory[subscriber] = new List<DataType>();
                    return true;
                }
            }

            public bool Unsubscribe (SubscriberType subscriber) {
                if (subscriberHistory.Keys.Contains(subscriber)) {
                    subscriberHistory.Remove(subscriber);
                    return true;
                }
                else {
                    Debug.LogError(string.Format("{0}: Attempting to unsubscribe to {1} when not subscribed!", subscriber.GetType(), GetType()));
                    return false;
                }
            }

            public void FixedUpdate () {
                if (autoUpdate) {
                    timestep += Time.fixedDeltaTime;
                    if (timestep >= dataTrackingTimestep) {
                        timestep = 0;
                        FetchData();
                    }
                }
            }

            /// <summary>
            /// Function to retrieve and record the data from all subscribers.
            /// </summary>
            public void FetchData () {
                foreach (SubscriberType subscriber in subscriberHistory.Keys) {
                    if (subscriberHistory[subscriber].Count >= maxHistoryLength) {
                        while (subscriberHistory[subscriber].Count > maxHistoryLength - 1) {
                            subscriberHistory[subscriber].RemoveAt(0);
                        }
                    }
                    subscriberHistory[subscriber].Add(subscriber.GetData());
                }
            }

            /// <summary>
            /// Returns the history recorded from the specified subscriber as a list of data.
            /// </summary>
            public List<DataType> GetItemDataHistory (SubscriberType subscriber) {
                if (subscriberHistory.Keys.Contains(subscriber)) {
                    return subscriberHistory[subscriber].ToList();
                }
                Debug.LogErrorFormat("DataTracker<{0}>: Instance not subscribed!", typeof(SubscriberType).ToString());
                return null;
            }

        }

    }

}
