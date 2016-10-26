using UnityEngine;
using UnityEngine.Rendering;

public class CubemapAtlas : MonoBehaviour{

    public Cubemap[] Cubemaps;

    private RenderTexture albedoCubemapRT_256;
    private RenderTexture albedoCubemapRT_128;
    public Material material;

    private const int MAX_TEXTURE_SIZE = 4096;


	void Start () {
        CreateAtlas();

        material.SetTexture("_MainTex", albedoCubemapRT_256);
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
        Cubemap[] cubemaps_128 = new Cubemap[MAX_TEXTURE_SIZE / 128];

        int cubemapNum_256 = 0;
        int cubemapNum_128 = 0;
        foreach (Cubemap c in Cubemaps) {
            if (c.width == 256) {
                cubemaps_256[cubemapNum_256++] = c;
            }
            else if(c.width == 128) {
                cubemaps_128[cubemapNum_128++] = c;
            }
            else {
                Debug.LogError("Group Only for size 256 , 128 , 64 now.");
                Debug.Assert(false);
            }
        }

        CreateAtlasFromCubemaps(cubemaps_256, cubemapNum_256, 256, RenderTextureFormat.ARGB32, ref albedoCubemapRT_256);
        CreateAtlasFromCubemaps(cubemaps_128, cubemapNum_128, 128, RenderTextureFormat.ARGB32, ref albedoCubemapRT_128);

    }
}
