using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class LevelStatus : MonoBehaviour
{
    public GameObject LevelStatusObject;
    public GameObject GridGoalsObject;
    public GameObject GridColumnObject;

    public GameObject GoalPrefab;
    public Sprite VaseSpritePrefab;
    public Sprite StoneSpritePrefab;
    public Sprite BoxSpritePrefab;

    public int MoveCount = 0;
    public int BoxCount = 0;
    public int StoneCount = 0;
    public int VaseCount = 0;

    public GameObject MoveDowncounter;

    private GameObject BoxGoal = null;
    private GameObject StoneGoal = null;
    private GameObject VaseGoal = null;

    public void StartUp(int boxCount, int stoneCount, int vaseCount, int moveCount)
    {
        BoxCount = boxCount;
        StoneCount = stoneCount;
        VaseCount = vaseCount;
        InitializeGoals();


        MoveCount = moveCount;
        MoveDowncounter.GetComponent<TMP_Text>().text = moveCount.ToString();
    }

    private void InitializeGoals()
    {
        int obstacleTypeCounter = 0;
        if(BoxCount > 0)
        {
            obstacleTypeCounter++;
            BoxGoal = Instantiate(GoalPrefab, GridColumnObject.transform);
            BoxGoal.GetComponent<UnityEngine.UI.Image>().sprite = BoxSpritePrefab;
            GameObject Tick = BoxGoal.transform.Find("Tick").gameObject;
            Tick.SetActive(false);
            GameObject Counter = BoxGoal.transform.Find("Counter").gameObject;
            Counter.GetComponent<TMP_Text>().text = BoxCount.ToString();
        }

        if (StoneCount > 0)
        {
            obstacleTypeCounter++;
            StoneGoal = Instantiate(GoalPrefab, GridColumnObject.transform);
            StoneGoal.GetComponent<UnityEngine.UI.Image>().sprite = StoneSpritePrefab;
            GameObject Tick = StoneGoal.transform.Find("Tick").gameObject;
            Tick.SetActive(false);
            GameObject Counter = StoneGoal.transform.Find("Counter").gameObject;
            Counter.GetComponent<TMP_Text>().text = StoneCount.ToString();
        }

        if (VaseCount > 0)
        {
            obstacleTypeCounter++;
            GameObject gridSecondColumn;
            if (obstacleTypeCounter == 3)
            {
                gridSecondColumn = Instantiate(GridColumnObject, GridGoalsObject.transform);
                for (int i = gridSecondColumn.transform.childCount - 1; i > -1; i--)
                {
                    GameObject.Destroy(gridSecondColumn.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                gridSecondColumn = GridColumnObject;
            }
            VaseGoal = Instantiate(GoalPrefab, gridSecondColumn.transform);
            VaseGoal.GetComponent<UnityEngine.UI.Image>().sprite = VaseSpritePrefab;
            GameObject Tick = VaseGoal.transform.Find("Tick").gameObject;
            Tick.SetActive(false);
            GameObject Counter = VaseGoal.transform.Find("Counter").gameObject;
            Counter.GetComponent<TMP_Text>().text = VaseCount.ToString();
        }

    }

    public void decreaseBoxCounter()
    {
        if (BoxGoal != null && BoxCount > 0)
        {
            BoxCount--;
            if(BoxCount == 0)
            {
                GameObject Counter = BoxGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = BoxCount.ToString();
                Counter.SetActive(false);
                GameObject Tick = BoxGoal.transform.Find("Tick").gameObject;
                Tick.SetActive(true);
            }
            else 
            {
                GameObject Counter = BoxGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = BoxCount.ToString();
            }
        }
    }

    public void decreaseStoneCounter()
    {
        if (StoneGoal != null && StoneCount > 0)
        {
            StoneCount--;
            if (StoneCount == 0)
            {
                GameObject Counter = StoneGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = StoneCount.ToString();
                Counter.SetActive(false);
                GameObject Tick = StoneGoal.transform.Find("Tick").gameObject;
                Tick.SetActive(true);
            }
            else
            {
                GameObject Counter = StoneGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = StoneCount.ToString();
            }
        }
    }

    public void decreaseVaseCounter()
    {
        if (VaseGoal != null && VaseCount > 0)
        {
            VaseCount--;
            if (VaseCount == 0)
            {
                GameObject Counter = VaseGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = VaseCount.ToString();
                Counter.SetActive(false);
                GameObject Tick = VaseGoal.transform.Find("Tick").gameObject;
                Tick.SetActive(true);
            }
            else
            {
                GameObject Counter = VaseGoal.transform.Find("Counter").gameObject;
                Counter.GetComponent<TMP_Text>().text = VaseCount.ToString();
            }
        }
    }

    public void decreaseMoveCounter()
    {
        if (MoveCount > 0)
        {
            MoveCount--;
            MoveDowncounter.GetComponent<TMP_Text>().text = MoveCount.ToString();
        }
    }
}
