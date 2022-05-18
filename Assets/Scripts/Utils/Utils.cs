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
        public static void SnapToLevelGrid(GameObject entity, CombatManager manager) {

            Vector3Int tilePos = GameHandler.Instance.currentLevel.WorldToCell(entity.transform.position);

            if (IsCellFilled(tilePos) || manager.CellHasEntity(tilePos)) {
                bfs.Clear();
                bfsVisited.Clear();

                bfs.Enqueue(tilePos);
                bfsVisited[tilePos] = true;

                while (bfs.Count > 0) {
                    var current = bfs.Dequeue();
                    
                    if (!IsCellFilled(current) && !manager.CellHasEntity(current)) {
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
        public static List<Vector3Int> GetValidAdjacent(Vector3Int cell, CombatManager manager = null, bool respectEntities = false, bool respectSelection = false, bool respectExclusion = false, Vector3Int exclude = default(Vector3Int)) {
            var results = new List<Vector3Int>();

            if (!IsCellFilled(cell + Vector3Int.right)
                && (!respectEntities || (!manager.CellHasEntity(cell + Vector3Int.right) || (respectExclusion && cell + Vector3Int.right == exclude)))
                && (!respectSelection || manager.selectGrid.GetTile(cell + Vector3Int.right) != null))
                results.Add(cell + Vector3Int.right);
            if (!IsCellFilled(cell + Vector3Int.left)
                && (!respectEntities || (!manager.CellHasEntity(cell + Vector3Int.left) || (respectExclusion && cell + Vector3Int.left == exclude)))
                && (!respectSelection || manager.selectGrid.GetTile(cell + Vector3Int.left) != null))
                results.Add(cell + Vector3Int.left);
            if (!IsCellFilled(cell + Vector3Int.up)
                && (!respectEntities || (!manager.CellHasEntity(cell + Vector3Int.up) || (respectExclusion && cell + Vector3Int.up == exclude)))
                && (!respectSelection || manager.selectGrid.GetTile(cell + Vector3Int.up) != null))
                results.Add(cell + Vector3Int.up);
            if (!IsCellFilled(cell + Vector3Int.down)
                && (!respectEntities || (!manager.CellHasEntity(cell + Vector3Int.down) || (respectExclusion && cell + Vector3Int.down == exclude)))
                && (!respectSelection || manager.selectGrid.GetTile(cell + Vector3Int.down) != null))
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
        public static List<Vector3Int> GetPath(Vector3Int src, Vector3Int dst, CombatManager manager = null, bool respectEntities = false, bool respectSelection = false) {
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

                List<Vector3Int> adj = GridUtil.GetValidAdjacent(current, manager, respectEntities, respectSelection, true, dst);

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

        private static Dictionary<Vector3Int, int> bfsDist = new Dictionary<Vector3Int, int>();
        private static Queue<Vector3Int> bfs = new Queue<Vector3Int>();
        public static List<Vector3Int> GetBFS(Vector3Int src, int dist, CombatManager manager, bool respectEntities, bool respectSelection) {
            bfs.Clear();
            bfsDist.Clear();
            List<Vector3Int> output = new List<Vector3Int>();

            bfs.Enqueue(src);
            bfsDist[src] = 0;

            while (bfs.Count > 0) {
                var current = bfs.Dequeue();
                
                output.Add(current);
                var cost = bfsDist[current] + 1;
                if (cost > dist)
                    continue;
                foreach (var next in Utils.GridUtil.GetValidAdjacent(current, manager, respectEntities, respectSelection)) {
                    if (bfsDist.ContainsKey(next))
                        continue;
                    
                    bfs.Enqueue(next);
                    bfsDist[next] = cost;
                }
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

    public class CombatUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePos"></param>
        /// <param name="destPos"></param>
        /// <param name="combatManager"></param>
        /// <param name="targetType"></param>
        /// <param name="range"></param>
        /// <param name="size"></param>
        /// <returns>Null in case on invalid destination</returns>
        public static List<Vector3Int> GetTilesInAttack(Vector3Int sourcePos, Vector3 destPos, CombatManager combatManager, Skill.Target targetType, int range, int size)
        {
            List<Vector3Int> validTiles = new List<Vector3Int>();
            switch(targetType)
            {
                case Skill.Target.SQUARE:
                    {
                        //Square

                        Vector3Int topLeftTile;

                        if (size % 2 == 0)
                        {
                            //Even size, targeting is a bit messier
                            List<Vector3Int> centerTiles = new List<Vector3Int>();
                            centerTiles.Add(combatManager.entityGrid.WorldToCell(destPos + new Vector3(0.5f, 0.5f, 0)));
                            centerTiles.Add(combatManager.entityGrid.WorldToCell(destPos + new Vector3(-0.5f, 0.5f, 0)));
                            centerTiles.Add(combatManager.entityGrid.WorldToCell(destPos + new Vector3(-0.5f, -0.5f, 0)));
                            centerTiles.Add(combatManager.entityGrid.WorldToCell(destPos + new Vector3(0.5f, -0.5f, 0)));

                            #region GetTopLeftCenterTile
                            Vector3Int topLeftCenterTile = centerTiles[0];
                            Vector2Int topLeftPos = new Vector2Int(centerTiles[0].x, centerTiles[0].y);

                            for(int i = 1; i< centerTiles.Count; i++)
                            {
                                Vector2Int currCellPos = new Vector2Int(centerTiles[i].x, centerTiles[i].y);

                                if((currCellPos.x < topLeftPos.x) ||(currCellPos.y < topLeftPos.y))
                                {
                                    topLeftCenterTile = centerTiles[i];
                                    topLeftPos = currCellPos;
                                }
                            }
                            #endregion

                            topLeftTile = topLeftCenterTile - new Vector3Int(size/2 - 1, size / 2 - 1, 0);
                        }
                        else
                        {
                            //odd size, brain is happy :)
                            int radius = (size - 1) / 2;
                            topLeftTile = combatManager.entityGrid.WorldToCell(destPos - new Vector3(radius, radius, 0));
                        }

                        //from top left tile, get tiles that will be hit by move
                        for (int i = 0; i < size; i++)
                        {
                            for (int j = 0; j < size; j++)
                            {
                                validTiles.Add(topLeftTile + new Vector3Int(i, j, 0));
                            }
                        }

                        break;
                    }
                case Skill.Target.RADIUS:
                    {
                        //Diamond

                        validTiles = Pathfinding.GetBFS(combatManager.entityGrid.WorldToCell(destPos), size, combatManager, false, false);

                        break;
                    }
                case Skill.Target.LINE:
                    {
                        //Straight line

                        break;
                    }
                case Skill.Target.ARC:
                    {
                        //Goes around the end of a diamond

                        break;
                    }
            }

            bool inRange = false;
            foreach (Vector3Int tile in validTiles)
            {
                if (GridUtil.ManhattanDistance(tile, sourcePos) <= range)
                {
                    inRange = true;
                    break;
                }
            }

            if (inRange == false)
                return null;

            return validTiles;
        }

        public static List<CombatEntity> GetEntitiesInAttack(Vector3Int sourcePos, Vector3 destPos, CombatManager combatManager, Skill.Target targetType, int range, int size)
        {
            List<Vector3Int> attackTiles = GetTilesInAttack(sourcePos, destPos, combatManager, targetType, range, size);

            List<CombatEntity> entities = new List<CombatEntity>();

            foreach (Vector3Int tile in attackTiles)
            {
                EntityReference entityRef = combatManager.GetEntityInCell(tile);

                if (entityRef != null)
                {
                    CombatEntity entityToAdd = entityRef.entity;
                    entities.Add(entityToAdd);
                }
            }

            return entities;
        }
    }
}
