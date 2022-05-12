using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public class GridUtil {
        public static int ManhattanDistance(Vector2Int a, Vector2Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public static int ManhattanDistance(Vector3Int a, Vector3Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /**
        * returns true if a given cell on the level map has an obstacle or is invalid terrain
        */
        public static bool IsCellFilled(Vector3Int pos) {
            return GameHandler.Instance.floorGrid.GetTile(pos) == null || GameHandler.Instance.wallGrid.GetTile(pos) != null;
        }
        public static bool IsCellFilled(Vector3 point) {
            var pos = GameHandler.Instance.CurrentLevel.WorldToCell(point);
            return GameHandler.Instance.floorGrid.GetTile(pos) == null || GameHandler.Instance.wallGrid.GetTile(pos) != null;
        }

        public static bool IsPointInRange(GameObject src, Vector3 point, int dist) {
            Vector3Int gridSrc = GameHandler.Instance.CurrentLevel.WorldToCell(src.transform.position);
            Vector3Int gridPoint = GameHandler.Instance.CurrentLevel.WorldToCell(point);
            return ManhattanDistance(gridPoint, gridSrc) <= dist;
        }

        public static bool IsPointInRange(GameObject src, Vector3Int point, int dist) {
            Vector3Int gridSrc = GameHandler.Instance.CurrentLevel.WorldToCell(src.transform.position);
            return ManhattanDistance(point, gridSrc) <= dist;
        }

        public static bool IsPointInSelectRange(Vector3 point) {
            var pos = GameHandler.Instance.CurrentLevel.WorldToCell(point);
            return GameHandler.Instance.selectGrid.GetTile(pos) != null;
        }

        public static bool IsPointInSelectRange(Vector3Int point) {
            return GameHandler.Instance.selectGrid.GetTile(point) != null;
        }

        public static List<Vector3Int> GetValidAdjacent(Vector3Int cell) {
            var results = new List<Vector3Int>();

            if (!IsCellFilled(cell + Vector3Int.right))
                results.Add(cell + Vector3Int.right);
            if (!IsCellFilled(cell + Vector3Int.left))
                results.Add(cell + Vector3Int.left);
            if (!IsCellFilled(cell + Vector3Int.up))
                results.Add(cell + Vector3Int.up);
            if (!IsCellFilled(cell + Vector3Int.down))
                results.Add(cell + Vector3Int.down);
            return results;
        }

        public static List<Vector3Int> GetValidSelectedAdjacent(Vector3Int cell) {
            var results = new List<Vector3Int>();

            if (GameHandler.Instance.selectGrid.GetTile(cell + Vector3Int.right) != null)
                results.Add(cell + Vector3Int.right);
            if (GameHandler.Instance.selectGrid.GetTile(cell + Vector3Int.left) != null)
                results.Add(cell + Vector3Int.left);
            if (GameHandler.Instance.selectGrid.GetTile(cell + Vector3Int.up) != null)
                results.Add(cell + Vector3Int.up);
            if (GameHandler.Instance.selectGrid.GetTile(cell + Vector3Int.down) != null)
                results.Add(cell + Vector3Int.down);
            return results;
        }
    }


    public class Pathfinding {
        private static Dictionary<Vector3Int, Vector3Int> previous = new Dictionary<Vector3Int, Vector3Int>();
        private static Dictionary<Vector3Int, int> costs = new Dictionary<Vector3Int, int>();
        private static PriorityQueue<Vector3Int> open = new PriorityQueue<Vector3Int>();

        /**
        * returns the list of positions that must be traversed
        * to get from the source to the destination.
        * this includes the destination, but not the source
        *
        * if inSelect is true, will only consider paths in the select range;
        */
        public static List<Vector3Int> GetPath(Vector3Int src, Vector3Int dst, bool inSelect = false) {
            // setup
            open.Clear();
            previous.Clear();
            costs.Clear();
            
            open.Put(src, 0);
            previous[src] = default(Vector3Int);
            costs[src] = 0;

            while (!open.Empty()) {
                var current = open.Pop();

                if (current == dst)
                    break;

                List<Vector3Int> adj = null;
                if (inSelect) {
                    adj = GridUtil.GetValidSelectedAdjacent(current);
                } else {
                    adj = GridUtil.GetValidAdjacent(current);
                }

                foreach (var next in adj) {
                    int cost = costs[current] + 1;
                    if (!costs.ContainsKey(next) || cost < costs[next]) {
                        costs[next] = cost;
                        previous[next] = current;
                        open.Put(next, cost + GridUtil.ManhattanDistance(dst, next));
                    }
                }
            }

            // collect and return final path
            var trace = dst;
            var output = new List<Vector3Int>();
            while (trace != src) {
                output.Add(trace);
                trace = previous[trace];
            }
            return output;
        }

        public static void PrintPathCosts() {
            GameHandler.Instance.ClearText();
            foreach (var cost in costs) {
                GameHandler.Instance.DrawText(GameHandler.Instance.CurrentLevel.CellToWorld(cost.Key) + new Vector3(0.5f, 0.5f, 0), cost.Value.ToString());
            }
        }
    }
}
