using Assets.Player;
using Assets.Road;
using Assets.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IHasGameObject
    {
        public GameObject gameObject { get; }
    }

    public class ExtendController
    {

        public interface IExtendCallback
        {
            //public bool ShouldRemove();
            public bool ShouldExtend(GameObject lastGameObject);
            public void OnExtend();
            /// <summary>
            /// 调用此函数时，gameObject需要手动从列表删除，否则会自动删除
            /// </summary>
            /// <param name="index">gameObject在列表中的索引</param>
            /// <param name="obj">需要移除的gameObject</param>
            public void OnRemove(int index, GameObject obj);
        }

        private readonly IExtendCallback callback = null;
        private YieldInstruction yieldInstruction = null;
        public YieldInstruction YieldInstruction { get => yieldInstruction; set => yieldInstruction = value; }


        public ExtendController(IExtendCallback callback, YieldInstruction yieldInstruction)
        {
            this.callback = callback;
            this.yieldInstruction = yieldInstruction;
        }


        public IEnumerator CheckAndExtend(List<GameObject> gameObjects)
        {
            while (true)
            {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GameObject gameObject = gameObjects[i];
                    if (gameObject == null)
                    {
                        gameObjects.RemoveAt(i--);
                        continue;
                    }
                    // 玩家在路段前方的距离
                    float diff = PlayerController.Instance.PlayerTransform.position.z - gameObject.transform.position.z;
                    if (diff < 0)
                    {
                        break; // 前方路段不需要继续遍历
                    }
                    else if (diff >= Mathf.Abs(GameManager.Instance.Config.PostDestroyedDistance)) // 20 为两个路段的长度
                    {
                        callback.OnRemove(i, gameObject);
                        if (i < gameObjects.Count && gameObjects[i].gameObject != null && gameObjects[i] == gameObject) // 如果OnRemove没有正确删除gameObject，则手动删除
                        {
                            gameObjects.RemoveAt(i);
                        }
                        --i; // 删除后索引回退一位
                    }
                }
                // 生成前方路段
                if (gameObjects.Count > 0 && callback.ShouldExtend(gameObjects.Last())) // pregenerateRoadDistance为预生成距离
                {
                    // 最远路段接近玩家，需要生成新的路段
                    callback.OnExtend();
                }
                yield return yieldInstruction;
            }
        }
        public IEnumerator CheckAndExtend<W>(List<W> gameObjects) where W : IHasGameObject
        {
            while (true)
            {
                // 生成前方路段
                if (gameObjects.Count == 0 || (gameObjects.Count > 0 && callback.ShouldExtend(gameObjects.Last().gameObject))) // pregenerateRoadDistance预生成距离
                {
                    // 最远路段接近玩家，需要生成新的路段
                    callback.OnExtend();
                }
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GameObject gameObject = gameObjects[i].gameObject;
                    if (gameObject == null)
                    {
                        gameObjects.RemoveAt(i--);
                        continue;
                    }
                    // 玩家在路段前方的距离
                    float diff = PlayerController.Instance.PlayerTransform.position.z - gameObject.transform.position.z;
                    if (diff < 0)
                    {
                        break; // 前方路段不需要继续遍历
                    }
                    else if (diff >= Mathf.Abs(GameManager.Instance.Config.PostDestroyedDistance)) // 20 为两个路段的长度
                    {
                        // 该路段已经远离玩家，销毁
                        callback.OnRemove(i, gameObject);
                        if (i < gameObjects.Count && gameObjects[i].gameObject != null && gameObjects[i].gameObject == gameObject) // 如果OnRemove没有正确删除gameObject，则手动删除
                        {
                            gameObjects.RemoveAt(i);
                        }
                        --i; // 删除后索引回退一位
                    }
                }
                yield return yieldInstruction;
            }
        }


    }
}
