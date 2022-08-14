using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JBirdLib
{

    /// <summary>
    /// Helpful math functions that don't exist in base UnityEngine.
    /// </summary>
    public static class MathHelper
    {

        /// <summary>
        /// Returns n raised to the p power as an extension of the integer class.
        /// </summary>
        public static int Pow(this int n, uint p) {
            if (p == 0) {
                return 1;
            }
            return n * Pow(n, p - 1);
        }

        /// <summary>
        /// Returns n raised to the p power as an extension of the integer class.
        /// </summary>
        public static int Pow(this int n, int p) {
            if (p < 0) {
                Debug.LogWarning("MathHelper: Trying to use negative exponent for integer exponentiation.");
                return 1;
            }
            return n.Pow((uint)p);
        }

        /// <summary>
        /// Returns the float but with numbers that approximate to zero returned as zero.
        /// </summary>
        public static float ApproximateZero(this float n, float tolerance = 0.00001f) {
            return Mathf.Abs(n) < tolerance ? 0f : n;
        }

    }

    /// <summary>
    /// Contains extension methods for Unity base functionality.
    /// </summary>
    public static class UnityHelper
    {

        /// <summary>
        /// Calls SetActive on the component's gameObject.
        /// </summary>
        public static void SetActive(this Component component, bool value) {
            component.gameObject.SetActive(value);
        }

        /// <summary>
        /// Enables/disables a behaviour and sets gameObject activeness accordingly.
        /// </summary>
        public static void SetActiveAndEnabled(this Behaviour behaviour, bool value) {
            behaviour.gameObject.SetActive(value);
            behaviour.enabled = value;
        }

    }

    /// <summary>
    /// Contains functions for managing singleton classes.
    /// </summary>
    public static class Singleton
    {
        private static Dictionary<string, Component> registered;

        /// <summary>
        /// Assures that there's only one instance of this component in the active scene. Recommended for use in Awake().
        /// If the class already has a registered singleton, will destroy the entire GameObject this instance is attached to.
        /// </summary>
        /// <param name="obj">This instance.</param>
        /// <typeparam name="T">Must inherit from Component.</typeparam>
        public static bool CreateSingleton<T>(this T obj, bool persist = false) where T : Component {
            if (!Application.isPlaying) {
                obj.Error("Attempting to make a singleton instance when the application is not playing!");
                return false;
            }
            string typeName = obj.GetType().Name;
            if (!registered.ContainsKey(typeName)) {
                registered[typeName] = obj;
                if (persist) {
                    UnityEngine.Object.DontDestroyOnLoad(obj.gameObject);
                }
                return true;
            }
            else {
                UnityEngine.Object.Destroy(obj.gameObject);
                return false;
            }
        }

        /// <summary>
        /// Get the registered singleton instance of the given type.
        /// Recommended for use inside a static instance property.
        /// </summary>
        /// <typeparam name="T">The type of singleton to retrieve.</typeparam>
        public static T Get<T>() where T : Component {
            string typeName = typeof(T).Name;
            if (registered.ContainsKey(typeName)) {
                return (T)registered[typeName];
            }
            else {
                Debug.LogErrorFormat("{0}: Attempting to fetch a singleton that hasn't been initialized yet!", typeName);
                return null;
            }
        }

    }

    /// <summary>
    /// Contains functions for easily making enums into flags.
    /// </summary>
    public static class EnumHelper
    {

        /// <summary>
        /// Returns an enum that is a combination of the given flag sets.
        /// </summary>
        public static T CombineFlags<T>(params T[] flagSets) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (T)Enum.ToObject(typeof(T), flagSets.Aggregate(0, (r, f) => r | Convert.ToInt32(f)));
        }

        /// <summary>
        /// Returns a concatenation of this flag set and any number of additional flag sets.
        /// </summary>
        public static T Concat<T>(this T flagSet, params T[] flagSets) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (T)Enum.ToObject(typeof(T), flagSets.Aggregate(Convert.ToInt32(flagSet), (r, f) => r | Convert.ToInt32(f)));
        }

        /// <summary>
        /// Returns this flag set with the given flags toggled.
        /// May return unwanted results if the same flag is present more than once.
        /// </summary>
        public static T Toggle<T>(this T flagSet, params T[] toggleSets) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (T)Enum.ToObject(typeof(T), toggleSets.Aggregate(Convert.ToInt32(flagSet), (r, f) => r ^ Convert.ToInt32(f)));
        }

        /// <summary>
        /// Returns this flag set with the given flags removed.
        /// If a flag is not in the original flag set, it will be ignored.
        /// </summary>
        public static T Remove<T>(this T flagSet, params T[] removeSets) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (T)Enum.ToObject(typeof(T), removeSets.Aggregate(Convert.ToInt32(flagSet), (r, f) => r & (r ^ Convert.ToInt32(f))));
        }

        /// <summary>
        /// Returns whether or not this flag set perfectly contains a given collection of flags.
        /// </summary>
        public static bool Contains<T>(this T flagSet, T checkFor) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (Convert.ToInt32(flagSet) & Convert.ToInt32(checkFor)) == Convert.ToInt32(checkFor);
        }

        /// <summary>
        /// Attempts to convert a string value to an enum value of the given type. Returns boolean based on success.
        /// </summary>
        /// <typeparam name="T">Enum type to convert to.</typeparam>
        /// <param name="value">String to convert.</param>
        /// <param name="returnEnum">Enum created from string.</param>
        /// <returns>True if the enum can be parsed from the string, false otherwise.</returns>
        public static bool TryParse<T>(this string value, out T returnEnum) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            returnEnum = default;
            if (Enum.IsDefined(typeof(T), value)) {
                returnEnum = (T)Enum.Parse(typeof(T), value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to convert a string value to an enum value of the given type. Returns default on failure.
        /// </summary>
        public static T ToEnum<T>(this string value) where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            if (!TryParse(value, out T enumValue)) {
                Debug.LogErrorFormat("ToEnum<{0}>(): Value '{1}' does not exist within {0}.", typeof(T), value);
            }
            return enumValue;
        }

        /// <summary>
        /// Get the combination of all possible flags of the given type.
        /// </summary>
        public static T Everything<T>() where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return CombineFlags(Enum.GetValues(typeof(T)) as T[]);
        }

        /// <summary>
        /// Get the empty set of flags for the given type.
        /// </summary>
        public static T Nothing<T>() where T : Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(string.Format("EnumHelper: {0} is not an enumeration.", typeof(T)));
            }
            return (T)Enum.ToObject(typeof(T), 0);
        }

    }

    /// <summary>
    /// Contains functions for list management and statistics.
    /// </summary>
    public static class ListHelper
    {

        /// <summary>
        /// Creates a list out of the argument parameters.
        /// </summary>
        public static List<T> MakeList<T>(params T[] objects) {
            return objects.ToList();
        }

        /// <summary>
        /// Returns the element of the list which is the closest to a given position (or null if none are within a specified range).
        /// </summary>
        public static T GetClosestToPosition<T>(this List<T> objects, Vector3 position, float maxDist = Mathf.Infinity) where T : Component {
            return objects
                .Select(o => new { obj = o, dist = Vector3.Distance(o.transform.position, position) })
                .Where(a => a.dist < maxDist)
                .OrderBy(a => a.dist)
                .Select(a => a.obj)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns the element of the list which is the closest to a given position within a certain range (or null if none are within range).
        /// </summary>
        public static T GetClosestWithinRange<T>(this List<T> objects, Vector3 position, float maxDist) where T : Component {
            return GetClosestToPosition(objects, position, maxDist);
        }

        /// <summary>
        /// Returns the first element from a list and then removes it from the list.
        /// </summary>
        public static T PopFront<T>(this List<T> objects) {
            if (objects.Count == 0) {
                throw new ArgumentOutOfRangeException("PopFront: Attempting to pop from empty list.");
            }
            return objects.PopFrontOrDefault();
        }

        /// <summary>
        /// Returns the first element from a list and then removes it from the list.
        /// Returns default of the specified type if list is empty.
        /// </summary>
        public static T PopFrontOrDefault<T>(this List<T> objects) {
            if (objects.Count == 0) {
                return default;
            }
            T temp = objects.FirstOrDefault();
            objects.RemoveAt(0);
            return temp;
        }

        /// <summary>
        /// Returns the last element from a list and then removes it from the list.
        /// </summary>
        public static T PopBack<T>(this List<T> objects) {
            if (objects.Count == 0) {
                throw new ArgumentOutOfRangeException("PopBack: Attempting to pop from empty list.");
            }
            return objects.PopBackOrDefault();
        }

        /// <summary>
        /// Returns the last element from a list and then removes it from the list.
        /// Returns default of the specified type if list is empty.
        /// </summary>
        public static T PopBackOrDefault<T>(this List<T> objects) {
            if (objects.Count == 0) {
                return default;
            }
            T temp = objects.LastOrDefault();
            objects.RemoveAt(objects.Count - 1);
            return temp;
        }

        /// <summary>
        /// Removes the first occurence of an item from the list, but leaves an empty slot in its place with the default value for that type.
        /// </summary>
        public static bool RemoveToDefault<T>(this List<T> objects, T item) {
            if (objects.Contains(item)) {
                objects[objects.IndexOf(item)] = default;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the item to the first empty slot in the list (emptiness defined by the default value for the element type).
        /// Will expand the list by default if there's not enough space, but this can be disabled by setting expandList to false.
        /// </summary>
        public static bool AddToFirstEmpty<T>(this List<T> objects, T item, bool expandList = true) where T : class {
            int index = objects.IndexOf(default);
            if (index == -1) {
                if (expandList) {
                    objects.Add(item);
                    return true;
                }
            }
            else {
                objects[index] = item;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the item to the beginning of the list.
        /// </summary>
        public static void AppendToFront<T>(this List<T> objects, T item) {
            List<T> tempList = objects.ToList();
            objects.Clear();
            objects.Add(item);
            foreach (T obj in tempList) {
                objects.Add(obj);
            }
        }

    }

    /// <summary>
    /// Contains helper functions for char type.
    /// </summary>
    public static class CharHelper
    {

        /// <summary>
        /// Capitalize the specified char (must be alphabetical).
        /// </summary>
        public static char Capitalize(this char c) {
            int i = c;
            if (97 <= i && i <= 122) {
                return (char)(i - 32);
            }
            else {
                return c;
            }
        }

        /// <summary>
        /// Lowercase the specified char (must be alphabetical).
        /// </summary>
        public static char Lowercase(this char c) {
            int i = c;
            if (65 <= i && i <= 90) {
                return (char)(i + 32);
            }
            else {
                return c;
            }
        }

    }

    /// <summary>
    /// A Vector3 where the x and y components are treated as azimuth and elevation angles, respectively. The z value is treated as the magnitude.
    /// Can be implicitly cast and serialized as a Vector3.
    /// </summary>
    [System.Serializable]
    public class AngleVector3
    {
        public AngleVector3() {
            azimuth = 0;
            elevation = 0;
            magnitude = 1;
        }

        public AngleVector3(Angle a, Angle e, float m) {
            azimuth = a;
            elevation = e;
            magnitude = m;
        }

        [SerializeField]
        private float _azimuth;
        [SerializeField]
        private float _elevation;

        public Angle azimuth { get { return _azimuth; } set { _azimuth = value; } }
        public Angle elevation { get { return _elevation; } set { _elevation = value; } }
        public float magnitude;

        public static implicit operator Vector3(AngleVector3 aVec) {
            return aVec.ToVector3();
        }

        public static implicit operator AngleVector3(Vector3 vec) {
            float a = vec.GetAzimuth(Vector3.zero, Vector3.up, Vector3.forward, out float e);
            return new AngleVector3(a, e, vec.magnitude);
        }

        public Vector3 ToVector3() {
            return VectorHelper.FromAzimuthAndElevation(this) * magnitude;
        }
    }

    /// <summary>
    /// A float that automatically uses modulo 360.
    /// Can be implicitly cast as a float.
    /// </summary>
    [System.Serializable]
    public class Angle
    {

        public Angle(float f) {
            val = f;
        }

        [SerializeField]
        private float _val;
        public float val { get { return _val % 360f; } set { _val = value % 360f; } }

        public static implicit operator float(Angle angle) {
            return angle.val;
        }

        public static implicit operator Angle(float f) {
            return new Angle(f);
        }
    }

    /// <summary>
    /// Contains helper functions for Vector3 type.
    /// </summary>
    public static class VectorHelper
    {

        /// <summary>
        /// Returns the midpoint between two vectors.
        /// </summary>
        public static Vector3 Midpoint(Vector3 v1, Vector3 v2) {
            return (v1 + v2) / 2f;
        }

        /// <summary>
        /// Returns a unit vector created via azimuth and elevation angles relative to world coordinates.
        /// </summary>
        /// <param name="azimuth">Angle around the y-axis.</param>
        /// <param name="elevation">Angle between the desired vector and the xz-plane.</param>
        public static Vector3 FromAzimuthAndElevation(Angle azimuth, Angle elevation) {
            return FromAzimuthAndElevation(azimuth, elevation, Vector3.up, Vector3.forward);
        }

        /// <summary>
        /// Returns a unit vector created via azimuth and elevation angles relative to world coordinates.
        /// </summary>
        public static Vector3 FromAzimuthAndElevation(AngleVector3 angles) {
            return FromAzimuthAndElevation(angles, Vector3.up, Vector3.forward);
        }

        /// <summary>
        /// Returns a unit vector created via azimuth and elevation angles relative to the given directional vectors.
        /// </summary>
        /// <param name="angles">The set of angles to for the calculation.</param>
        /// <param name="up">The local positive y direction.</param>
        /// <param name="forward">The local positive z direction.</param>
        public static Vector3 FromAzimuthAndElevation(AngleVector3 angles, Vector3 up, Vector3 forward) {
            return FromAzimuthAndElevation(angles.azimuth, angles.elevation, up, forward);
        }

        /// <summary>
        /// Returns a unit vector created via azimuth and elevation angles relative to the given directional vectors.
        /// </summary>
        /// <param name="azimuth">Angle around the y-axis.</param>
        /// <param name="elevation">Angle between the desired vector and the xz-plane.</param>
        /// <param name="up">The local positive y direction.</param>
        /// <param name="forward">The local positive z direction.</param>
        public static Vector3 FromAzimuthAndElevation(Angle azimuth, Angle elevation, Vector3 up, Vector3 forward) {
            Vector3 compoundVec = Vector3.zero;
            // relative x component
            float x = (Mathf.Sin(Mathf.Deg2Rad * azimuth) * Mathf.Cos(Mathf.Deg2Rad * elevation)).ApproximateZero();
            compoundVec += Vector3.Cross(up.normalized, forward.normalized) * x;
            // relative y component
            float y = Mathf.Sin(Mathf.Deg2Rad * elevation).ApproximateZero();
            compoundVec += up.normalized * y;
            // relative z component
            float z = (Mathf.Cos(Mathf.Deg2Rad * azimuth) * Mathf.Cos(Mathf.Deg2Rad * elevation)).ApproximateZero();
            compoundVec += forward.normalized * z;
            return compoundVec;
        }

        /// <summary>
        /// Get the elevation angle to this vector from a reference point.
        /// </summary>
        /// <param name="position">The vector to find the elevation of.</param>
        /// <param name="center">The reference point.</param>
        /// <param name="up">The normal of the plane to find the elevation from.</param>
        public static float GetElevation(this Vector3 position, Vector3 center, Vector3 up) {
            return Mathf.Asin(Vector3.Dot((position - center).normalized, up.normalized)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Get the azimuth angle to this vector from a reference point.
        /// </summary>
        /// <param name="position">The vector to find the azimuth of.</param>
        /// <param name="center">The reference point.</param>
        /// <param name="up">The normal of the reference plane.</param>
        /// <param name="forward">The forward direction to find the azimuth from.</param>
        public static float GetAzimuth(this Vector3 position, Vector3 center, Vector3 up, Vector3 forward) {
            return position.GetAzimuth(center, up, forward, out _);
        }

        /// <summary>
        /// Get the azimuth angle to this vector from a reference point.
        /// </summary>
        /// <param name="position">The vector to find the azimuth of.</param>
        /// <param name="center">The reference point.</param>
        /// <param name="up">The normal of the reference plane.</param>
        /// <param name="forward">The forward direction to find the azimuth from.</param>
        /// <param name="elevation">Optional out parameter to get the elevation value used during azimuth calculation.</param>
        public static float GetAzimuth(this Vector3 position, Vector3 center, Vector3 up, Vector3 forward, out float elevation) {
            elevation = position.GetElevation(center, up);
            Vector3 right = Vector3.Cross(up.normalized, forward.normalized).normalized;
            float azimuth = Mathf.Acos(Vector3.Dot((position - center).normalized, forward.normalized) / Mathf.Cos(elevation * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
            if (Vector3.Dot((position - center).normalized, right) > 0f) {
                azimuth = 360f - azimuth;
            }
            return azimuth;
        }

    }

}