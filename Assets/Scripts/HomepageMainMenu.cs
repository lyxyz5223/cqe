using GLTFast;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HomepageMainMenu : MonoBehaviour
{
    [SerializeField] PlayerShowController playerShowController;

    private Queue<Action> runInMainThreadQueue = new();
    private object runInMainThreadQueueLock = new();

    private void OnEnable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lock (runInMainThreadQueueLock)
            if (runInMainThreadQueue.TryDequeue(out var action))
                action?.Invoke();
    }

    public async void OnOptionsBtnClicked()
    {
        if (playerShowController == null)
        {
            Debug.LogError("PlayerShowController is not set.");
            return;
        }
        var gameManager = GameManager.Instance;
        int index = gameManager.CurrentPlayerIndex;
        if (++index >= gameManager.Config.PlayerModelPrefabs.Length)
            index = 0;
        playerShowController.ChangePlayerModel(index);
        //string path = EditorUtility.OpenFilePanel("选择glTF文件", "", "gltf,glb");
        //if (!string.IsNullOrEmpty(path))
        //{
        //    await LoadGltfFromFile(path);
        //}
    }

    //async Task LoadGltfFromFile(string path)
    //{
    //    var gltf = new GltfImport();

    //    // 加载glTF文件（会自动解析所有数据）
    //    bool success = await gltf.Load(path);

    //    if (success)
    //    {
    //        // 实例化模型到场景中
    //        Transform parent = modelParent != null ? modelParent.transform : transform;
    //        GameObject player = new("player");
    //        player.transform.SetParent(parent);
    //        player.transform.localPosition = Vector3.zero;
    //        player.transform.localRotation = Quaternion.Euler(0, 0, 0);
    //        await gltf.InstantiateMainSceneAsync(player.transform);
    //        Animator animator = player.AddComponent<Animator>();
    //        // 用模型的骨骼Transform创建Avatar
    //        HumanDescription humanDescription = new ();
    //        Avatar avatar = AvatarBuilder.BuildHumanAvatar(player, humanDescription);
    //        if (avatar.isValid)
    //        {
    //            animator.avatar = avatar;
    //            Debug.Log("Avatar 创建成功");
    //        }
    //        else
    //        {
    //            Debug.LogWarning("无法创建 Avatar，模型可能缺少骨骼信息");
    //        }
    //        lock (runInMainThreadQueueLock)
    //            runInMainThreadQueue.Enqueue(() => SetupPlayerModel(player));
    //    }
    //    else
    //    {
    //        Debug.LogError("模型加载失败！");
    //    }
    //}

    //void SetupPlayerModel(GameObject modelObj)
    //{
    //    GameObject scene = null;
    //    foreach (Transform tr in modelObj.transform)
    //    {
    //        if (tr != currentPlayer.transform)
    //            scene = tr.gameObject;
    //    }
    //    CapsuleCollider collider = modelObj.AddComponent<CapsuleCollider>();
    //    CapsuleCollider prevCollider = currentPlayer.GetComponent<CapsuleCollider>();
    //    collider.center = prevCollider.center;
    //    collider.radius = prevCollider.radius;
    //    collider.isTrigger = prevCollider.isTrigger;
    //    collider.height = prevCollider.height;
    //    collider.direction = prevCollider.direction;
    //    if (currentPlayer != null)
    //        Destroy(currentPlayer);
    //    currentPlayer = modelObj;
    //}

    public void OnStart()
    {
        GameManager.Instance.SwitchSceneAndStartGame();
    }

    public void OnExit()
    {
        GameManager.Instance.ExitGame();
    }

}
