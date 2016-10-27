using UnityEngine;
using UnityEngine.Rendering;

public class CubemapAtlas : MonoBehaviour {


    public Transform[] CubemapCameraTransformFromEditor;

    private RenderTexture albedoCubemapRT_64;
    private RenderTexture normalCubemapRT_64;
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
        CreateAtlas(CubemapCameraTransformFromEditor, 64, CubemapCameraTransformFromEditor.Length, ref albedoCubemapRT_64, ref normalCubemapRT_64);

        material.SetTexture("_MainTex", albedoCubemapRT_64);
	}

    void CreateCubemapAtalsRenderTarget(int cubemapSize, int cubemapNum, ref RenderTexture rt, RenderTextureFormat format)
    {
        int atlasWidth = 6 * cubemapSize;
        int atlasHeight = cubemapNum * cubemapSize;
        rt = new RenderTexture(atlasWidth, atlasHeight, 0, format);
        rt.enableRandomWrite = true;
        rt.Create();
    }

    void CreateAtlas(Transform[] cubemapCameraTransforms, int cubemapSizeGBuffer, int cubemapNum, ref RenderTexture albeodoRT, ref RenderTexture normalRT)
    {
        Vector3[] positions = new Vector3[cubemapNum];
        for (int c_i = 0; c_i < cubemapNum; c_i++)
        {
            Debug.Assert(cubemapCameraTransforms[c_i] != null);
            positions[c_i] = cubemapCameraTransforms[c_i].position;
        }

        CreateAtlas(positions, cubemapSizeGBuffer, cubemapNum, ref albeodoRT, ref normalRT);
    }

    void CreateAtlas(Vector3[] cubemapCameraPositions, int cubemapSizeGBuffer, int cubemapNum, ref RenderTexture albedoRT, ref RenderTexture normalRT)
    {
        Debug.Assert(cubemapNum > 0, "Please add camera transform.");
        Debug.Assert(cubemapNum * cubemapSizeGBuffer < MAX_TEXTURE_SIZE, "Can not create such big atlas, with size: " + cubemapNum + "CubemapSize:" + cubemapSizeGBuffer);

        CreateCubemapAtalsRenderTarget(cubemapSizeGBuffer, cubemapNum, ref albedoRT, RenderTextureFormat.ARGB32);

        RenderTexture specularRT = null;
        CreateCubemapAtalsRenderTarget(cubemapSizeGBuffer, cubemapNum, ref specularRT, RenderTextureFormat.ARGB32);
        // https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html
        // RT2, ARGB2101010 World space normal buffer, 
        CreateCubemapAtalsRenderTarget(cubemapSizeGBuffer, cubemapNum, ref normalRT, RenderTextureFormat.ARGB2101010);

        float unitUVSize = 1.0f / cubemapNum;
        int cubemapIndex = 0;

        Camera cubemapCamera = (Camera)Camera.Instantiate(Camera.main, new Vector3(0, 0, 0),
        Quaternion.FromToRotation(new Vector3(0, 0, 0), new Vector3(0, 0, 1)));
        //cubemapCamera.renderingPath = RenderingPath.DeferredShading;

        cubemapCamera.fieldOfView = 90;
        cubemapCamera.targetTexture = albedoRT;
        RenderBuffer[] TargetBuffers = { albedoRT.colorBuffer, specularRT.colorBuffer , normalRT.colorBuffer };
        //cubemapCamera.SetTargetBuffers(TargetBuffers, albedoRT.depthBuffer);
        cubemapCamera.clearFlags = CameraClearFlags.Depth;
        cubemapCamera.GetComponent<AudioListener>().enabled = false;
        Graphics.SetRenderTarget(TargetBuffers, albedoCubemapRT_64.depthBuffer);
        foreach (Vector3 cameraPos in cubemapCameraPositions)
        {
            for (int faceIndex = 0; faceIndex < 6; faceIndex++) {
                cubemapCamera.transform.position = cameraPos;
                cubemapCamera.transform.LookAt(cameraPos + cubemapCameraStructs[faceIndex].lookAt, cubemapCameraStructs[faceIndex].up);
                cubemapCamera.rect = new Rect(new Vector2(faceIndex * 1.0f / 6.0f, cubemapIndex * 1.0f / cubemapNum), new Vector2(1.0f / 6.0f, unitUVSize));
                Debug.Log("Actual rendering path:" + cubemapCamera.actualRenderingPath);
                cubemapCamera.Render();
            }
            cubemapIndex++; 
        }
    }
}
