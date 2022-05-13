using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Utils
{
    public static Quaternion Rotate(Vector2 dir) {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public class GridUtil {
        public static int ManhattanDistance(Vector2Int a, Vector2Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public static int ManhattanDistance(Vector3Int a, Vector3Int b) {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private static Queue<Vector3Int> bfs = new Queue<Vector3Int>();
        private static Dictionary<Vector3Int, bool> bfsVisited = new Dictionary<Vector3Int, bool>();
        public static void SnapToLevelGrid(GameObject entity) {

            Vector3Int tilePos = GameHandler.Instance.currentLevel.WorldToCell(entity.transform.position);

            if (IsCellFilled(tilePos)) {
                bfs.Clear();
                bfsVisited.Clear();

                bfs.Enqueue(tilePos);
                bfsVisited[tilePos] = true;

                while (bfs.Count > 0) {
                    var current = bfs.Dequeue();
                    
                    if (!IsCellFilled(current)) {
                        tilePos = current;
                        break;
                    }

                    if (!bfsVisited.ContainsKey(current + Vector3Int.up)) {
                        bfs.Enqueue(current + Vector3Int.up);
                        bfsVisited[current + Vector3Int.up] = true;
                    }
                    if (!bfsVisited.ContainsKey(current + Vector3Int.down)) {
                        bfs.Enqueue(current + Vector3Int.down);
                        bfsVisited[current + Vector3Int.down] = true;
                    }
                    if (!bfsVisited.ContainsKey(current + Vector3Int.right)) {
                        bfs.Enqueue(current + Vector3Int.right);
                        bfsVisited[current + Vector3Int.right] = true;
                    }
                    if (!bfsVisited.ContainsKey(current + Vector3Int.left)) {
                        bfs.Enqueue(current + Vector3Int.left);
                        bfsVisited[current + Vector3Int.left] = true;
                    }
                }
            }
            
            entity.transform.position = new Vector3(0.5f, 0.5f, 0) + GameHandler.Instance.currentLevel.CellToWorld(tilePos);
        }

        /**
        * returns true if a given cell on the level map has an obstacle or is invalid terrain
        */
        public static bool IsCellFilled(Vector3Int pos) {
            return GameHandler.Instance.floorGrid.GetTile(pos) == null || isTileDiagonal(pos) ||  GameHandler.Instance.wallGrid.GetTile(pos) != null;
        }
        public static bool IsCellFilled(Vector3 point) {
            var pos = GameHandler.Instance.currentLevel.WorldToCell(point);
            return GameHandler.Instance.floorGrid.GetTile(pos) == null || isTileDiagonal(pos) || GameHandler.Instance.wallGrid.GetTile(pos) != null;
        }

        private static Matrix4x4 transform;
        private static bool isTileDiagonal(Vector3Int pos) {
            var north = GameHandler.Instance.floorGrid.GetTile(pos + Vector3Int.up) == null;
            var east = GameHandler.Instance.floorGrid.GetTile(pos + Vector3Int.right) == null;
            var south = GameHandler.Instance.floorGrid.GetTile(pos + Vector3Int.down) == null;
            var west = GameHandler.Instance.floorGrid.GetTile(pos + Vector3Int.left) == null;
            return checkDiag(north, east, south, west) 
                || checkDiag(east, south, west, north) 
                || checkDiag(south, west, north, east) 
                || checkDiag(west, north, east, south);
        }

        private static bool checkDiag(bool a, bool b, bool c, bool d) { 
            return a && b && !c && !d;
        }

        public static bool IsPointInRange(GameObject src, Vector3 point, int dist) {
            Vector3Int gridSrc = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);
            Vector3Int gridPoint = GameHandler.Instance.currentLevel.WorldToCell(point);
            return ManhattanDistance(gridPoint, gridSrc) <= dist;
        }

        public static bool IsPointInRange(GameObject src, Vector3Int point, int dist) {
            Vector3Int gridSrc = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);
            return ManhattanDistance(point, gridSrc) <= dist;
        }

        public static bool IsPointInSelectRange(Vector3 point, CombatManager manager) {
            var pos = GameHandler.Instance.currentLevel.WorldToCell(point);
            return manager.selectGrid.GetTile(pos) != null;
        }

        public static bool IsPointInSelectRange(Vector3Int point, CombatManager manager) {
            return manager.selectGrid.GetTile(point) != null;
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

        public static List<Vector3Int> GetValidSelectedAdjacent(Vector3Int cell, CombatManager manager) {
            var results = new List<Vector3Int>();

            if (manager.selectGrid.GetTile(cell + Vector3Int.right) != null)
                results.Add(cell + Vector3Int.right);
            if (manager.selectGrid.GetTile(cell + Vector3Int.left) != null)
                results.Add(cell + Vector3Int.left);
            if (manager.selectGrid.GetTile(cell + Vector3Int.up) != null)
                results.Add(cell + Vector3Int.up);
            if (manager.selectGrid.GetTile(cell + Vector3Int.down) != null)
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
        public static List<Vector3Int> GetPath(Vector3Int src, Vector3Int dst, CombatManager manager = null) {
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
                if (manager != null) {
                    adj = GridUtil.GetValidSelectedAdjacent(current, manager);
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
                GameHandler.Instance.DrawText(GameHandler.Instance.currentLevel.CellToWorld(cost.Key) + new Vector3(0.5f, 0.5f, 0), cost.Value.ToString());
            }
        }
    }
}
