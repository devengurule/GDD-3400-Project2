using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDD3400.Labyrinth
{
    public class Pathfinder
    {
        public static List<PathNode> FindPath(PathNode startNode, PathNode endNode)
        {
            // Nodes we might want to look at
            List<PathNode> openSet = new List<PathNode>();

            // Nodes we have looked at
            List<PathNode> closedSet = new List<PathNode>();

            // Saves path info back to start
            Dictionary<PathNode, PathNode> cameFromNode = new Dictionary<PathNode, PathNode>();

            // Keeping track of cost from beginning to end and end to beginning
            Dictionary<PathNode, float> costSoFar = new Dictionary<PathNode, float>();
            Dictionary<PathNode, float> costToEnd = new Dictionary<PathNode, float>();

            // Initialize Starting Info
            openSet.Add(startNode);
            costSoFar.Add(startNode, 0f);
            costToEnd.Add(startNode, Heuristic(startNode, endNode));

            while(openSet.Count > 0)
            {
                // THIS IS LOOKING AT THE CURRENT NODE, NOT ANY NEIGHBORS ONLY THE SINGULAR NODE

                // Current gets the lowest cost node to the end
                PathNode current = GetLowestCost(openSet, costToEnd);

                // Found the goal, break out and return the final path
                if(current == endNode)
                {
                    return ReconstructPath(cameFromNode, current);
                }

                // Move the current node we're looking at from the open set to the closed set
                openSet.Remove(current);
                closedSet.Add(current);


                // NOW WE START LOOKING AT NEIGHBORS AND THE COSTS TO THOSE NEIGHBORS

                foreach(var connection in current.Connections)
                {
                    PathNode neighbor = connection.Key;

                    //Have already looked at it
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeCostFromStart = costSoFar[current] + connection.Value;

                    // Adding neighbor nodes to openSet only if they are closer to the end than the current node
                    // If we havent looked at the neighbor node, add it to openset
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    // Otherwise if the cost from start tp end is greater than skip this neighbor
                    else if(tentativeCostFromStart >= costSoFar[neighbor])
                    {
                        continue;
                    }

                    cameFromNode[neighbor] = current;
                    costSoFar[neighbor] = tentativeCostFromStart;
                    costToEnd[neighbor] = costSoFar[neighbor] + Heuristic(neighbor, endNode);

                }




            }




            return new List<PathNode>(); // Return an empty path if no path is found
        }

        // Calculate the heuristic cost from the start node to the end node, manhattan distance
        private static float Heuristic(PathNode startNode, PathNode endNode)
        {
            return Vector3.Distance(startNode.transform.position, endNode.transform.position);
        }

        // Get the node in the provided open set with the lowest cost (eg closest to the end node)
        private static PathNode GetLowestCost(List<PathNode> openSet, Dictionary<PathNode, float> costs)
        {
            PathNode lowest = openSet[0];
            float lowestCost = costs[lowest];

            foreach (var node in openSet)
            {
                float cost = costs[node];
                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    lowest = node;
                }
            }

            return lowest;
        }

        // Reconstruct the path from the cameFrom map
        private static List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
        {
            List<PathNode> totalPath = new List<PathNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }
    }
}
