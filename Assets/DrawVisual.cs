using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawVisual : MonoBehaviour {

    public enum SortType
    {
        InsertionSort,
        BubbleSort,
        MergeSort
    }

    public SortType selectedSort;

	public Color defC = Color.black;
    public int border = 20;

    public int total = 7200;
    public int shuffleSpeed = 100;
    public int sortSpeed = 7200;

    List<int> arr = new List<int>();

    float angleZero = 90f;
    float ovalWidth;
    float ovalHeight;
    Vector2 centerOffset;

    List<Color32> blank = new List<Color32>();
    Texture2D tex;

    int ticker = 0;
    bool shuffled = false;
    bool sorting = false;
    bool done = false;

    int drawTicker = 0;

	void Start () {

        // Build array of sortable numbers
        for (int i = 0; i < total; i++) arr.Add(i);

        //tex = new Texture2D(1366, 768);
        tex = new Texture2D(672, 378);
        GetComponent<Renderer>().material.mainTexture = tex;

        Camera.main.backgroundColor = defC;

        // Set up canvas/texture
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        transform.localScale = new Vector3((float)tex.width / (float)tex.height, 1f, 1f);

        DefineValues();

        // Create blank texture for re-rendering
        for (int i = 0; i < tex.width * tex.height; i++) blank.Add(defC);

        DrawDots();

	}

    // Set draw constraints
    void DefineValues()
    {
        ovalWidth = (tex.width / 2f) - border;
        ovalHeight = (tex.height / 2f) - border;
        centerOffset = new Vector2(tex.width / 2f, tex.height / 2f);
    }

	void Update () {
        if (sorting) return;

        if (done) return;

        if (shuffled && !sorting)
        {
            sorting = true;
            StartCoroutine(selectedSort.ToString());
            return;
        }

        // Stop shuffling after two rotations throught the array
        if (ticker >= total * 2f)
        {
            shuffled = true;

            ticker = 0;
            return;
        }

        // Shuffling, iterate through <shuffleSpeed> times before drawing to texture
        for (int i = 0; i < shuffleSpeed && ticker < total * 2f; i++)
        {
            int r1 = ticker % total;
            int r2 = Random.Range(0, total);

            // Swap r1 and r2
            int temp = arr[r1];

            arr[r1] = arr[r2];
            arr[r2] = temp;

            ticker++;
        }

        DrawDots();
    }

    // Iterates through array, moving each value back to its sorted position one at a time.
    IEnumerator InsertionSort()
    {
        int j;
        for (int i = 0; i < total; i++)
        {
            int key = arr[i];
            j = i - 1;

            while (j >= 0 && arr[j] > key)
            {
                // Move <key> back in the array until it is in a sorted position, offsetting all array values with each check
                arr[j + 1] = arr[j];
                drawTicker++;
                if (drawTicker >= sortSpeed) yield return DrawRoutine();

                j--;
            }

            // Assign <key> to its sorted position
            arr[j + 1] = key;
            drawTicker++;
            if (drawTicker >= sortSpeed) yield return DrawRoutine();
        }

        DrawDots();
        done = true;
    }

    // Steps through all adjacent pairs in array, flipping their positions to sort
    IEnumerator BubbleSort()
    {
        for (int i = 0; i < total - 1; i++)
        {
            bool flipped = false;

            // For each <i>, flip all values from [0 ... i] if the lower position is a higher value
            for (int j = 0; j < total - 1 - i; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    flipped = true;
                    int temp = arr[j];

                    arr[j] = arr[j + 1];
                    drawTicker++;
                    if (drawTicker >= sortSpeed) yield return DrawRoutine();

                    arr[j + 1] = temp;
                    drawTicker++;
                    if (drawTicker >= sortSpeed) yield return DrawRoutine();
                }
            }
            if (!flipped)
            {
                // If no flips occur for an entire loop through <i>, then array is sorted
                DrawDots();
                done = true;
                yield break;
            }
        }
    }

    // Used to start a merge sort without parameters
    IEnumerator MergeSort()
    {
        yield return MergeSort(0, total - 1);
    }

    // Sorts the full array by sections, delimited by a left (l) and right (r) pointer.
    IEnumerator MergeSort(int l, int r)
    {
        if (l < r)
        {
            // Find the middle, sort the left side and right side. Then, merge the two sides together.
            int m = (l + r) / 2;
            yield return MergeSort(l, m);
            yield return MergeSort(m + 1, r);

            // Set pointer limits for left and right
            int lLimit = m - l + 1;
            int rLimit = r - m;

            // Create "left side" and "right side" subLists of the main array
            List<int> subL = new List<int>();
            for (int i = 0; i < lLimit; i++) subL.Add(arr[l + i]);

            List<int> subR = new List<int>();
            for (int i = 0; i < rLimit; i++) subR.Add(arr[m + i + 1]);

            int pointer = l;
            int lp = 0;
            int rp = 0;

            // Merges sorted left and right sides together
            // Left and right are sorted by recursive MergeSort calls
            while (lp < lLimit && rp < rLimit)
            {
                if (subL[lp] < subR[rp])
                {
                    arr[pointer] = subL[lp];
                    drawTicker++;
                    if (drawTicker >= sortSpeed) yield return DrawRoutine();

                    lp++;
                }
                else
                {
                    arr[pointer] = subR[rp];
                    drawTicker++;
                    if (drawTicker >= sortSpeed) yield return DrawRoutine();

                    rp++;
                }
                pointer++;
            }

            // If the left side is larger, be sure to merge any remaining values in
            while (lp < lLimit)
            {
                arr[pointer] = subL[lp];
                drawTicker++;
                if (drawTicker >= sortSpeed) yield return DrawRoutine();

                lp++;
                pointer++;
            }

            // Same for right side
            while (rp < rLimit)
            {
                arr[pointer] = subR[rp];
                drawTicker++;
                if (drawTicker >= sortSpeed) yield return DrawRoutine();

                rp++;
                pointer++;
            }

            // Be sure to draw every time a merge is complete
            DrawDots();

            // If this is the end of the initial sort (Not a recursive sort), then done
            if (l == 0 && r == total - 1)
            done = true;
        }
    }

    // Called every step in the sorting coroutines. Updates texture per frame
    YieldInstruction DrawRoutine()
    {
            DrawDots();
            drawTicker = 0;
            return new WaitForEndOfFrame();

    }

    // Clear texture
    void Clear()
    {
        tex.SetPixels32(blank.ToArray());
    }

    // Draw a dot for each number in the array, given an HSV color and a position relative to how far away it is from sorted.
    // A number <i> is sorted when it is in array position [i].
    void DrawDots()
    {
        Clear();

        for (int i = 0; i < total; i++)
        {
            // Calculate how far away arr[i] is from its sorted position
            float shortest = Mathf.Min(Mathf.Abs(arr[i] - i), Mathf.Abs(arr[i] + total - i));
            float correctness = Mathf.Clamp01(1f - shortest / (total / 2f));

            float degrees = ((float)i / total) * 360f;

            float radians = (degrees + angleZero) * Mathf.Deg2Rad;
            Vector2 angle = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            // Set position and oval radii based on correctness
            float setWidth = ovalWidth * correctness;
            float setHeight = ovalHeight * correctness;

            int x = (int)(angle.x * setWidth + centerOffset.x);
            int y = (int)(angle.y * setHeight + centerOffset.y);

            Color c = Color.HSVToRGB((float)arr[i] / total, 1f, 1f);

            tex.SetPixel(x, y, c);
        }

        tex.Apply();
    }
}
