using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManage : MonoBehaviour
{
    public bool initFinish;
    private int bulletID;
    private Transform bulletParent;

    private Dictionary<int, BulletBase> dic_bullets;
    private List<BulletBase> list_destoryBullet;
    private GameObject prefabBullet;

    public void InitData(Transform _bulletParent)
    {
        initFinish = false;
        bulletParent = _bulletParent;
        bulletID = 0;
        dic_bullets = new Dictionary<int, BulletBase>();
        list_destoryBullet = new List<BulletBase>();
        StartCoroutine(LoadBullet());
    }


    private IEnumerator LoadBullet()
    {
        prefabBullet = Resources.Load<GameObject>("BattleScene/Bullet/Bullet");
        yield return new WaitForEndOfFrame();
        initFinish = true;
    }

    public void AddBullet(int _owerID, GameVector2 _logicPos, int _moveDir)
    {
        bulletID++;

        var _bulletBase = Instantiate(prefabBullet, bulletParent);
        var _bullet = _bulletBase.GetComponent<BulletBase>();
        _bullet.InitData(_owerID, bulletID, _logicPos, _moveDir);
        dic_bullets[bulletID] = _bullet;
    }

    public void Logic_Move()
    {
        foreach (var item in dic_bullets.Values) item.Logic_Move();
    }

    public void Logic_Collision()
    {
        foreach (var item in dic_bullets.Values) item.Logic_Collision();
    }

    public void Logic_Destory()
    {
        foreach (var item in dic_bullets.Values)
            if (item.Logic_Destory())
                list_destoryBullet.Add(item);


        foreach (var item in list_destoryBullet) dic_bullets.Remove(item.GetBulletID());

        list_destoryBullet.Clear();
    }
}