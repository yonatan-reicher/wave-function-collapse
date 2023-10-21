using System;
using System.Collections.Generic;
using System.Linq;

record class StepResult;
record class StepSuccess(int x, int y) : StepResult;
record class StepBacktrack : StepResult;
record class StepContradiction : StepResult;

public class Map {
    Tile?[,] tiles;
    IEnumerator<StepResult> generator;
    Random random = new Random();

    public int Width {
        get {
            return tiles.GetLength(0);
        }
    }

    public int Height {
        get {
            return tiles.GetLength(1);
        }
    }

    public Map(int width, int height) {
        tiles = new Tile?[width, height];
        generator = Generator().GetEnumerator();
    }

    public bool GenerateTile() {
        if (generator.Current is not StepSuccess and not null) Console.WriteLine(generator.Current);
        return generator.MoveNext();
    }

    public void GenerateAll() {
        while (generator.MoveNext()) {}
    }

    public void Shuffle<T>(IList<T> list)  {  
        int n = list.Count;  

        for (int i = list.Count - 1; i >= 1; i--) {
            int rnd = random.Next(i + 1);  

            T val = list[rnd];  
            list[rnd] = list[i];  
            list[i] = val;
        }
    }

    IEnumerable<StepResult> Generator() {
        // Pick a minimum entropy position at random.
        var leastEntropy = GetLeastEntropy();
        if (leastEntropy.Count == 0) {
            yield break;
        }
        var index = random.Next(leastEntropy.Count);
        var (x, y) = leastEntropy[index];
        // Pick a tile at random from the possible tiles at that position.
        var possibleTiles = GetPossibleTiles(x, y).ToArray();
        Shuffle(possibleTiles);
        // Try every possiblity until reaching one without contradiction.
        foreach (var tile in possibleTiles) {
            tiles[x, y] = tile;
            yield return new StepSuccess(x, y);
            var backtrackingList = new List<(int x, int y)>();
            var didBacktracking = false;
            foreach (var result in Generator()) {
                if (result is StepSuccess success) {
                    backtrackingList.Add((success.x, success.y));
                    yield return result;
                } else if (result is StepContradiction) {
                    // Do backtracking!
                    foreach (var pos in backtrackingList) {
                        tiles[pos.x, pos.y] = null;
                        yield return new StepBacktrack();
                    }
                    didBacktracking = true;
                    break;
                } else if (result is StepBacktrack) {
                    yield return result;
                } else {
                    throw new Exception("Invalid instance of StepResult!");
                }
            }
            if (!didBacktracking) yield break;
        }
        // Unwind more!
        yield return new StepContradiction();
    }

    /// Returns a list of positions where entropy is minimal (skipping
    /// positions already chosen)
    List<(int x, int y)> GetLeastEntropy() {
        // Make a list of all the tiles with the least entropy, and remember 
        // the minimum encountered entropy value.
        // Entropy = number of possible tiles at a position.
        var leastEntropy = new List<(int x, int y)>();
        var leastEntropyValue = int.MaxValue;

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                if (tiles[x, y] is not null) continue;

                // Look at the entropy of this position.
                var entropy = GetPossibleTiles(x, y).ToArray().Length;

                if (entropy < leastEntropyValue) {
                    leastEntropyValue = entropy;
                    leastEntropy.Clear();
                    leastEntropy.Add((x, y));
                } else if (entropy == leastEntropyValue) {
                    leastEntropy.Add((x, y));
                }
            }
        }

        return leastEntropy;
    }

    public IEnumerable<Tile> GetPossibleTiles(int x, int y) {
        // If a choice has been made - it is the only possiblity!
        if (tiles[x, y] is not null) {
            yield return tiles[x, y].Value;
            yield break;
        }
        foreach (Tile tile in (Tile[])Enum.GetValues(typeof(Tile))) {
            if (CanPlaceTile(tile, x, y)) {
                yield return tile;
            }
        }
    }

    bool CanPlaceTile(Tile tile, int x, int y) {
        Tile? left = x > 0 ? tiles[x - 1, y] : null;
        Tile? right = x < Width - 1 ? tiles[x + 1, y] : null;
        Tile? up = y > 0 ? tiles[x, y - 1] : null;
        Tile? down = y < Height - 1 ? tiles[x, y + 1] : null;

        bool AreEdgesMismatched(Tile a, Tile? b, Dir aToB) {
            var bToA = aToB.Opposite();
            return b is Tile bV && a.GetEdge(aToB) != bV.GetEdge(bToA);
        }

        return
            !AreEdgesMismatched(tile, left, Dir.Left)
            && !AreEdgesMismatched(tile, right, Dir.Right)
            && !AreEdgesMismatched(tile, up, Dir.Up)
            && !AreEdgesMismatched(tile, down, Dir.Down);
    }

    public Tile? this[int x, int y] {
        get {
            return tiles[x, y];
        }
    }

    public void Reset() {
        tiles = new Tile?[Width, Height];
        generator = Generator().GetEnumerator();
    }
}
