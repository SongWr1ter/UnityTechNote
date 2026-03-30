using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 创建数学平面，这里假设地面高度为 0，法线向上
            Plane plane = new Plane(Vector3.forward, Vector3.zero);

            // 2. 获取射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 3. 计算射线与平面的交点距离
            if (plane.Raycast(ray, out float distance))
            {
                // 根据距离获取射线上的点
                Vector3 worldPosition = ray.GetPoint(distance);
            
                Debug.Log($"平面上的世界坐标: {worldPosition}");
            }
        }
    }
}
