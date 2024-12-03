using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Demo1Mgr : MonoBehaviour
{
    public Button btn1, btn2, btn3, btn4;
    public Demo1 Food;
    private List<Demo1> foods = new List<Demo1>();
    int index;
    private void Start()
    {
        Application.targetFrameRate = 120;
        btn1.onClick.AddListener(() => CreateBigFood());
        btn2.onClick.AddListener(() => CreateFood());
        btn3.onClick.AddListener(() => HideFood());
        btn4.onClick.AddListener(() => FoodMove());
    }

    void HideFood()
    {
        foreach (var food in foods)
        {
            if (food.isHide == false)
            {
                food.isHide = true;
                food.transform.position += Vector3.right * 10;
                break;
            }
        }
    }

    void CreateBigFood()
    {
        if (index > 9) { index = 0; }
        var asset = Resources.Load<TextAsset>("mesh_" + index);
        var data = JsonUtility.FromJson(asset.text, typeof(MeshGroupData)) as MeshGroupData;
        var texture = Resources.Load<Texture>("ore/texture_" + index);

        var foodGo = GameObject.Instantiate(Food.gameObject);
        foodGo.name = "mesh_" + index;
        foodGo.transform.parent = transform;
        foodGo.gameObject.SetActive(true);
        var food = foodGo.GetComponent<Demo1>();
        food.SetMat(texture);
        food.CreateMeshes(data);
        food.SetMove(randomMove);
        foodGo.transform.localScale = Vector3.one * 0.6f;
        foodGo.transform.localPosition = new Vector3(-16.2f, 7.37f - index * 0.6f, 0);
        foods.Add(food);
        index++;
    }

    void CreateFood()
    {
        if (index > 9) { index = 0; }
        var asset = Resources.Load<TextAsset>("mesh_" + index);
        var data = JsonUtility.FromJson(asset.text, typeof(MeshGroupData)) as MeshGroupData;
        var texture = Resources.Load<Texture>("ore/texture_" + index);

        var foodGo = GameObject.Instantiate(Food.gameObject);
        foodGo.name = "mesh_" + index;
        foodGo.transform.parent = transform;
        foodGo.gameObject.SetActive(true);
        var food = foodGo.GetComponent<Demo1>();
        food.SetMat(texture);
        food.CreateMeshes(data);
        foodGo.transform.localScale = Vector3.one * 0.2f;
        foodGo.transform.localPosition = GetPos();
        food.SetMove(randomMove);
        foods.Add(food);
        index++;
    }
    Vector3 GetPos()
    {
        if (index % 2 == 0)
        {
            return new Vector3(-11, 9.72f - index * 0.8f, 0);
        }
        else
        {
            return new Vector3(-8f, 9.72f - (index - 1) * 0.8f, 0);
        }
    }

    private bool randomMove = false;
    void FoodMove()
    {
        randomMove = !randomMove;
        foreach (var food in foods) food.SetMove(randomMove);
    }
}
