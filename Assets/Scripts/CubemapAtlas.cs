using UnityEngine;
using UnityEngine.Rendering;

public class CubemapAtlas : MonoBehaviour{


    public Transform[] CubemapCameraPosition;

    private RenderTexture albedoCubemapRT_256;
    public Material material;

    private const int MAX_TEXTURE_SIZE = 4096;


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

        int cubemapNum_256 = 0;
        foreach (Transform t in CubemapCameraPosition)
        {
            Camera cubemapCamera = (Camera)Camera.Instantiate(Camera.main, t.position,
                Quaternion.FromToRotation(new Vector3(0, 0, 0), new Vector3(0, 0, 1)));
            cubemapCamera.GetComponent<AudioListener>().enabled = false;
            cubemapCamera.targetDisplay = 1;

            Cubemap c = new Cubemap(256, TextureFormat.ARGB32, false);
            cubemapCamera.RenderToCubemap(c);
            cubemaps_256[cubemapNum_256++] = c;

        }


        CreateAtlasFromCubemaps(cubemaps_256, cubemapNum_256, 256, RenderTextureFormat.ARGB32, ref albedoCubemapRT_256);

    }
}
