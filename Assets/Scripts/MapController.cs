using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Attach to the map GameObject, each chunk will be saved as a new gameobject under the map

public class MapController : MonoBehaviour
{

    // noise variables
    public float frequency = 2f;
    public float magnitude = 5f;
    public int layers = 5;
    public int offset = 0;

    public int chunkSize = 50; //size of a side of a square chunk
    public int renderBoxSize = 2; // how many layers of a chunk grid will be rendered

    public Material chunkMat; // default material used for generated chunks

    //xy position of a chunk with a player transform inside
    private int currentChunkx = 0;
    private int currentChunky = 0;

    //xy of player
    private int playerx = 0;
    private int playery = 0;

    GameObject Player;

    struct chunkIndex { int x; int y; }

    private chunkIndex[,] currentChunkIndex;
    private chunkIndex[,] newChunkIndex;

    private void Awake(){

        // Generate player and get player xy position

        Player = GameObject.FindGameObjectWithTag("Player");
        playerx = Mathf.RoundToInt(Player.transform.position.x);
        playery = Mathf.RoundToInt(Player.transform.position.z);

        //  initialise chunk indexes
        currentChunkIndex = new chunkIndex[renderBoxSize,renderBoxSize];
        newChunkIndex = new chunkIndex[renderBoxSize,renderBoxSize];

        RenderNear(playerx,playery);    // renders chunks near player


    }

    void Update(){


        //Update playerposition
        playerx = Mathf.RoundToInt(Player.transform.position.x/chunkSize);
        playery = Mathf.RoundToInt(Player.transform.position.z/chunkSize);

        //checks if player is on another chunk
        if (!(currentChunkx- playerx == 0 && currentChunky - playery == 0)) {
            currentChunkx = playerx;
            currentChunky = playery;
            RenderNear(currentChunkx, currentChunky);
        }
    }

    void RenderNear(int chunkx, int chunky)
    {
        //Loads chunk at player position and renderBoxSize chunk layers around it
        for (int i = chunkx - renderBoxSize; i <= chunkx + renderBoxSize; i++)
        {
            for (int j = chunky - renderBoxSize; j <= chunky + renderBoxSize; j++)
            {
                LoadChunk(i, j);
            }
        }


        //disables chunks at rederBoxSize + 1 layer (removes chunks currently not in use)
        for (int i = chunkx - renderBoxSize - 1; i <= chunkx + renderBoxSize+ 1; i++)
        {
            for (int j = chunky - renderBoxSize - 1; j <= chunky + renderBoxSize + 1; j++)
            {

                if (i == chunkx - renderBoxSize - 1 || i == chunkx + renderBoxSize + 1) {
                    DisableChunk(i, j);
                }
                else if(j == chunky - renderBoxSize - 1 || j == chunky + renderBoxSize + 1)
                {
                    DisableChunk(i, j);
                }
            }
        }

    }

    void DisableChunk(int x, int y)
    {

        if (GameObject.Find("chunk: " + x + ", " + y))
            GameObject.Find("chunk: " + x + ", " + y).SetActive(false);

    }

    public void SaveChunk(Mesh mesh) {
        string path = Path.Combine(Application.persistentDataPath,"chunkData/" + mesh.name);
        byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
        File.WriteAllBytes(path, bytes);
    }

    public void LoadChunk(int x, int y) {
        
        //if chunk has already been initialised, return or set active if not active
        if (GameObject.Find("chunk: " + x + ", " + y)) {

            if (GameObject.Find("chunk: " + x + ", " + y).activeSelf)
                return;

            GameObject.Find("chunk: " + x + ", " + y).SetActive(true);

        }

        //chunk gameobject setup

        GameObject newChunk = MakeChunk(x, y);
        Mesh chunkMesh = new Mesh();
        chunkMesh.name =  x + ", " + y;

        // loading a chunk mesh, if not found a new one is generated

        string path = Path.Combine(Application.persistentDataPath, x + ", " + y);
        if (File.Exists(path) == true)
        {
            byte[] bytes = File.ReadAllBytes(path);
            chunkMesh = MeshSerializer.ReadMesh(bytes);
        }
        else {
            chunkMesh = GenerateChunk(x, y, chunkSize, magnitude, frequency);
        }

        chunkMesh.RecalculateNormals();
        newChunk.GetComponent<MeshFilter>().mesh = chunkMesh;
        newChunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
        newChunk.GetComponent<MeshRenderer>().material = chunkMat;

    }

    public GameObject MakeChunk(int x, int y) {
        GameObject chunk = new GameObject();
        chunk.name = "chunk: " + x + ", " + y;

        //chunk.transform.parent = mapObject.transform; //makes GameObject the script is attached to a parent of the chunk 
        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
        chunk.AddComponent<MeshCollider>();

        return chunk;
    }

    public Mesh GenerateChunk(int chunkx, int chunky, int chunkSize, float magnitude, float frequency) {

        Mesh chunkMesh = new Mesh();

        int offsetx = chunkx * chunkSize;
        int offsety = chunky * chunkSize;

        // Heightmap from the Noise generator used
        float[] noiseMap = Noise.GenerateNoisemap(chunkx, chunky, chunkSize, magnitude, frequency, layers, offset);

        //Creating Mesh Script

        Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++, i++)
            {
                vertices[i] = new Vector3(offsetx + x, noiseMap[i], offsety + y);
                uv[i] = new Vector2((float)x / chunkSize, (float)y / chunkSize);
                tangents[i] = tangent;
            }
        }

        chunkMesh.vertices = vertices;

        int[] triangles = new int[chunkSize * chunkSize * 6];
        for (int ti = 0, vi = 0, y = 0; y < chunkSize; y++, vi++)
        {
            for (int x = 0; x < chunkSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + chunkSize + 1;
                triangles[ti + 5] = vi + chunkSize + 2;
            }
        }

        chunkMesh.triangles = triangles;
        chunkMesh.uv = uv;
        chunkMesh.tangents = tangents;

        return chunkMesh;

    }

    public static void CacheItem(string url, Mesh mesh)
    {
        string path = Path.Combine(Application.persistentDataPath, url);
        byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
        File.WriteAllBytes(path, bytes);
    }

    public static Mesh GetCacheItem(string url)
    {
        string path = Path.Combine(Application.persistentDataPath, url);
        if (File.Exists(path) == true)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return MeshSerializer.ReadMesh(bytes);
        }
        return null;
    }

}
