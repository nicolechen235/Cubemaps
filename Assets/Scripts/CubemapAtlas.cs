using UnityEngine;
using UnityEngine.Rendering;

public class CubemapAtlas : MonoBehaviour {


    public Transform[] CubemapCameraPosition;

    private RenderTexture albedoCubemapRT_256;
    public Material material;

    private const int MAX_TEXTURE_SIZE = 4096;

    private struct CubemapCameraStruct {
        public Vector3 lookAt;
        public Vector3 up ;
        public CubemapCameraStruct(Vector3 lookAt, Vector3 up) {
            this.lookAt = lookAt;
            this.up = up;
        }
    }

    private readonly CubemapCameraStruct[] cubemapCameraStructs = {
        
        new CubemapCameraStruct(new Vector3(1, 0, 0), new Vector3(0, 1, 0)), // +X
        new CubemapCameraStruct(new Vector3(-1, 0, 0), new Vector3(0, 1, 0)), // -X 
        new CubemapCameraStruct(new Vector3(0, 1, 0), new Vector3(0, 0, -1)), // +Y
        new CubemapCameraStruct(new Vector3(0, -1, 0), new Vector3(0, 0, 1)), // -Y 
        new CubemapCameraStruct(new Vector3(0, 0, 1), new Vector3(0, 1, 0)), // +Z
        new CubemapCameraStruct(new Vector3(0, 0, -1), new Vector3(0, 1, 0)), // -Z 

    };


    void Start () {
        CreateAtlas();

        material.SetTexture("_MainTex", albedoCubemapRT_256);
	}

    void CreateAtlasFromCubemapCamera(Transform[] cubemapCameraPosition, int cameraNum)
    {
    }
    void CreateAtlasFromCubemaps(Cubemap[] cubemaps, int cubemapNum, int cubemapSize, RenderTextureFormat format, ref RenderTexture dstTexture)
    {
        if (cubemapNum <= 0) {
            return;
        }

        int atlasWidth = 6 * cubemapSize;
        int atlasHeight = cubemapNum * cubemapSize;
        
        dstTexture = new RenderTexture(atlasWidth, atlasHeight, 0, format);
        dstTexture.enableRandomWrite = true;
        dstTexture.Create();

        for (int cubeIndex = 0; cubeIndex < cubemapNum; cubeIndex++) {
            Cubemap c = cubemaps[cubeIndex];
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if ((SystemInfo.copyTextureSupport & CopyTextureSupport.TextureToRT) != 0)
                {
                    Graphics.CopyTexture(
                        c, faceIndex, 0, 0, 0, cubemapSize, cubemapSize,
                        dstTexture, 0, 0, faceIndex * cubemapSize, cubeIndex * cubemapSize);
                    
                }
                else
                {
                    // not implement yet
                    Debug.Assert(false);
                }
            }
        }
    }

    void CreateAtlas() {
        Cubemap[] cubemaps_256 = new Cubemap[MAX_TEXTURE_SIZE / 256];

        int cubemapSize = 256;
        int cubemapIndex = 0;

        int cubemapNum = CubemapCameraPosition.Length;
        int atlasWidth = 6 * cubemapSize;
        int atlasHeight = cubemapNum * cubemapSize;

        float unitUVSize = 1.0f / cubemapNum;
        albedoCubemapRT_256= new RenderTexture(atlasWidth, atlasHeight, 0, RenderTextureFormat.ARGB32);
        albedoCubemapRT_256.enableRandomWrite = true;
        albedoCubemapRT_256.Create();

        Camera cubemapCamera = (Camera)Camera.Instantiate(Camera.main, new Vector3(0, 0, 0),
        Quaternion.FromToRotation(new Vector3(0, 0, 0), new Vector3(0, 0, 1)));
        cubemapCamera.fieldOfView = 90;
        cubemapCamera.targetTexture = albedoCubemapRT_256;
        cubemapCamera.clearFlags = CameraClearFlags.Color;
        cubemapCamera.GetComponent<AudioListener>().enabled = false;
        foreach (Transform t in CubemapCameraPosition)
        {
            for (int faceIndex = 0; faceIndex < 6; faceIndex++) {
                cubemapCamera.transform.position = t.position;
                cubemapCamera.transform.LookAt(t.position + cubemapCameraStructs[faceIndex].lookAt, cubemapCameraStructs[faceIndex].up);
                cubemapCamera.rect = new Rect(new Vector2(faceIndex * 1.0f / 6.0f, cubemapIndex * 1.0f / cubemapNum), new Vector2(1.0f / 6.0f, unitUVSize));
                cubemapCamera.Render();
            }
            cubemapIndex++; 
        }

        //CreateAtlasFromCubemaps(cubemaps_256, cubemapNum_256, cubemapSize, RenderTextureFormat.ARGB32, ref albedoCubemapRT_256);

    }
}
