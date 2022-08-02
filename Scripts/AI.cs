using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JBirdLib {

    /// <summary>
    /// A library of functions to assist with AI behaviors and pathfinding.
    /// </summary>
    namespace AI {

        #region A* INTERFACE
        /// <summary>
        /// Interface for an A*-capable node.
        /// </summary>
        public interface INode<NodeType> {

            /// <summary>
            /// Must return a list of all possible connections to this node.
            /// </summary>
            public List<NodeType> connections { get; }
            /// <summary>
            /// The cost of the path to this node.
            /// Will be treated as Mathf.Infinity if null.
            /// </summary>
            public float? pathCost { get; set; }
            /// <summary>
            /// The expected distance to the destination.
            /// Will be treated as Mathf.Infinity if null.
            /// </summary>
            public float? heuristicValue { get; set; }
            /// <summary>
            /// The node that we came from to get to this node.
            /// </summary>
            public NodeType cameFrom { get; set; }
            /// <summary>
            /// The position of the center of the node.
            /// Z-position will be ignored if using a 2D heuristic mode.
            /// </summary>
            public Vector3 position { get; }

        }

        /// <summary>
        /// Heuristic Mode for the A* function.
        /// </summary>
        public enum HeuristicMode
        {
            /// <summary>
            /// Standard Euclidian distance.
            /// </summary>
            Euclidian,
            /// <summary>
            /// Standard Euclidian distance on the xy-plane.
            /// </summary>
            Euclidian2D,
            /// <summary>
            /// "Manhattan" or "Taxi Cab" distance.
            /// </summary>
            Manhattan,
            /// <summary>
            /// "Manhattan" or "Taxi Cab" distance on the xy-plane.
            /// </summary>
            Manhattan2D,
            /// <summary>
            /// Uses custom hex grid defined in JBirdHexagonal.
            /// </summary>
            Hexagonal,
        }
        #endregion

        /// <summary>
        /// Contains functions to make programming AI easier.
        /// </summary>
        public static class AIHelper {

            #region RANGE FUNCTIONS
            /// <summary>
            /// An extension function for checking if an object is within a specified range of the given position.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="position">The position to compare to.</param>
            /// <param name="range">The distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange<ComponentType>(this ComponentType self, Vector3 position, float range) where ComponentType : Component {
                return (Vector3.Distance(self.transform.position, position) <= range);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of the given position.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="position">The position to compare to.</param>
            /// <param name="min">The minimum distance in meters.</param>
            /// <param name="max">The maximum distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange<ComponentType>(this ComponentType self, Vector3 position, float min, float max) where ComponentType : Component {
                float dist = Vector3.Distance(self.transform.position, position);
                return (dist >= min && dist <= max);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of another object.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="other">The other component to use the position of.</param>
            /// <param name="range">The distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange<ComponentType>(this ComponentType self, ComponentType other, float range) where ComponentType : Component {
                return self.WithinRange(other.transform.position, range);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of another object.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="other">The other component to use the position of.</param>
            /// <param name="min">The minimum distance in meters.</param>
            /// <param name="max">The maximum distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange<ComponentType>(this ComponentType self, ComponentType other, float min, float max) where ComponentType : Component {
                return self.WithinRange(other.transform.position, min, max);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of the given position on the xy-plane.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="position">The position to compare to.</param>
            /// <param name="range">The distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange2D<ComponentType>(this ComponentType self, Vector2 position, float range) where ComponentType : Component {
                return (Vector2.Distance(self.transform.position, position) <= range);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of the given position on the xy-plane.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="position">The position to compare to.</param>
            /// <param name="min">The minimum distance in meters.</param>
            /// <param name="max">The maximum distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange2D<ComponentType>(this ComponentType self, Vector2 position, float min, float max) where ComponentType : Component {
                float dist = Vector2.Distance(self.transform.position, position);
                return (dist >= min && dist <= max);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of another object on the xy-plane.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="other">The other component to use the position of.</param>
            /// <param name="range">The distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange2D<ComponentType>(this ComponentType self, ComponentType other, float range) where ComponentType : Component {
                return self.WithinRange2D(other.transform.position, range);
            }

            /// <summary>
            /// An extension function for checking if an object is within a specified range of another object on the xy-plane.
            /// </summary>
            /// <typeparam name="ComponentType">Must inherit from Unity Component class.</typeparam>
            /// <param name="self">The object to check.</param>
            /// <param name="other">The other component to use the position of.</param>
            /// <param name="min">The minimum distance in meters.</param>
            /// <param name="max">The maximum distance in meters.</param>
            /// <returns>True if the object is within the range specified, False otherwise.</returns>
            public static bool WithinRange2D<ComponentType>(this ComponentType self, ComponentType other, float min, float max) where ComponentType : Component {
                return self.WithinRange2D(other.transform.position, min, max);
            }
            #endregion

            #region A* IMPLEMENTATION
            /// <summary>
            /// Uses A* to find a path over a node graph using the INode interface.
            /// </summary>
            /// <returns>A list of nodes that represents the fastest path from start to end.</returns>
            /// <param name="start">Start node.</param>
            /// <param name="end">End node.</param>
            /// <param name="maxDist">Max distance to search from the start node (defaults to Mathf.Infinity).</param>
            /// <param name="mode">Heuristic mode (defaults to Euclidian).</param>
            /// <typeparam name="NodeType">The node type.</typeparam>
            public static List<NodeType> AStar<NodeType>(NodeType start, NodeType end, float maxDist = Mathf.Infinity, HeuristicMode mode = HeuristicMode.Euclidian) where NodeType : INode<NodeType> {
                // Initialize all lists as empty.
                List<NodeType> path = new List<NodeType>();                 // the final path
                HashSet<NodeType> openNodes = new HashSet<NodeType>();      // the nodes we should check
                HashSet<NodeType> closedNodes = new HashSet<NodeType>();    // the nodes we've exhausted
                // Set initial conditions of the start node.
                start.Reset();
                start.pathCost = 0f;
                start.heuristicValue = start.GetHeuristic(end, mode);
                // Check connections from the start node first.
                openNodes.Add(start);
                // While we still have available nodes...
                while (openNodes.Count > 0) {
                    // Get the node that has the best odds of being on the path.
                    NodeType currentNode = openNodes.WithBestValue();
                    if (currentNode.Equals(end)) {
                        // We have found the endpoint.
                        break;
                    }
                    // Check every connection, set values, and add to open list if within range.
                    openNodes = currentNode.connections
                        .Where(n => n != null && !closedNodes.Contains(n))
                        .Aggregate(openNodes, (l, n) => {
                            currentNode.SetCostAndHeuristic(n, end, mode);
                            if (n.GetValue() < maxDist) {
                                l.Add(n);
                            }
                            return l;
                        });
                    // We've exhausted this node.
                    openNodes.Remove(currentNode);
                    closedNodes.Add(currentNode);
                }
                // Reverse engineer the path to the end.
                path.Add(end);
                NodeType nextCameFrom = end.cameFrom;
                while (nextCameFrom != null) {
                    path.Add(nextCameFrom);
                    nextCameFrom = nextCameFrom.cameFrom;
                }
                path.Reverse();
                // Reset all the nodes.
                foreach (NodeType node in openNodes) {
                    node.Reset();
                }
                foreach (NodeType node in closedNodes) {
                    node.Reset();
                }
                return path;
            }

            internal static void Reset<NodeType>(this NodeType self) where NodeType : INode<NodeType> {
                self.cameFrom = default;
                self.heuristicValue = null;
                self.pathCost = null;
            }

            /// <summary>
            /// /// Used by the A* function to set the values on the next node in the potential path.
            /// </summary>
            /// <typeparam name="NodeType">The node type.</typeparam>
            /// <param name="self">The current node.</param>
            /// <param name="next">The next node in the sequence.</param>
            /// <param name="end">The endpoint for pathfinding.</param>
            /// <param name="mode">The heuristic mode.</param>
            internal static void SetCostAndHeuristic<NodeType>(this NodeType self, NodeType next, NodeType end, HeuristicMode mode) where NodeType : INode<NodeType> {
                next.pathCost = Mathf.Min(self.pathCost ?? 0f + self.GetHeuristic(next, mode), next.pathCost ?? Mathf.Infinity);
                next.heuristicValue = next.GetHeuristic(end, mode);
                next.cameFrom = self;
            }

            /// <summary>
            /// Used by the A* function to get the best node in the open set.
            /// </summary>
            /// <typeparam name="NodeType">The node type.</typeparam>
            /// <param name="nodes">The open set of nodes.</param>
            /// <returns>The first node with the lowest estimated value.</returns>
            internal static NodeType WithBestValue<NodeType>(this IEnumerable<NodeType> nodes) where NodeType : INode<NodeType> {
                if (nodes.Count() == 0) {
                    throw new ArgumentException("JBirdLib.AIHelper: Unexpected empty list in A* function."); // Unless the A* function changes, this should never be thrown.
                }
                return nodes.OrderBy(n => n.GetValue()).First();
            }

            /// <summary>
            /// Used by the A* function to get the estimated value of a node.
            /// </summary>
            /// <typeparam name="NodeType">The node type.</typeparam>
            /// <param name="self">The node to check.</param>
            /// <returns>The path cost summed with the heuristic estimate.</returns>
            internal static float GetValue<NodeType>(this NodeType self) where NodeType : INode<NodeType> {
                return self.pathCost ?? Mathf.Infinity + self.heuristicValue ?? Mathf.Infinity;
            }

            /// <summary>
            /// Returns the predicted distance from start to end based on the given heuristic mode.
            /// </summary>
            /// <param name="start">The start node.</param>
            /// <param name="end">The end node.</param>
            /// <param name="mode">The heuristic mode.</param>
            /// <typeparam name="NodeType">The node type.</typeparam>
            public static float GetHeuristic<NodeType>(this NodeType start, NodeType end, HeuristicMode mode) where NodeType : INode<NodeType> {
                return GetHeuristic(start.position, end.position, mode);
            }

            /// <summary>
            /// Returns the predicted distance from start to end based on the given heuristic mode.
            /// </summary>
            /// <param name="start">The start position.</param>
            /// <param name="end">The end position.</param>
            /// <param name="mode">The heuristic mode.</param>
            public static float GetHeuristic(Vector3 start, Vector3 end, HeuristicMode mode) {
                switch (mode) {
                    case HeuristicMode.Euclidian:
                        return Vector3.Distance(start, end);
                    case HeuristicMode.Euclidian2D:
                        return Vector2.Distance(start, end);
                    case HeuristicMode.Manhattan:
                        return Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y) + Mathf.Abs(end.z - start.z);
                    case HeuristicMode.Manhattan2D:
                        return Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
                    case HeuristicMode.Hexagonal:
                        return (Mathf.Abs(Vector3.Dot(end - start, Hexagonal.HexGrid.cornerUpRight)) * Vector3.Distance(end, start) +
                                Mathf.Abs(Vector3.Dot(end - start, Hexagonal.HexGrid.cornerDownRight)) * Vector3.Distance(end, start) +
                                Mathf.Abs(Vector3.Dot(end - start, Hexagonal.HexGrid.cornerLeft)) * Vector3.Distance(end, start)) / 2f;
                    default:
                        Debug.LogError("JBirdLib.AIHelper: Attempting to use unimplemented Heuristic Mode!");
                        return 0f;
                }
            }

        }
        #endregion

    }

}
