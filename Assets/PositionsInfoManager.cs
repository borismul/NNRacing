using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionsInfoManager : MonoBehaviour {

    public static PositionsInfoManager instance;
    public GameObject positionsInfoPrefab;
    List<GameObject> curObjs = new List<GameObject>();
    public GameObject finishCanvas;

    private void Awake()
    {
        instance = this;
    }

    public void CreatePositionList(List<CarController> cars)
    {
        DestroyAll();

        float height = cars.Count * positionsInfoPrefab.GetComponent<RectTransform>().rect.height;
        float width = GetComponent<RectTransform>().rect.width;
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, height);

        float bestTime = 0;
        bool setFinishedCanvas = false;
        for (int i = 0; i < cars.Count; i++)
        {
            for (int j = 0; j < cars.Count; j++)
            {
                if(cars[j].GetPosition() == (i+1))
                {
                    if (i == 0)
                    {
                        bestTime = cars[j].GetFinishTime();
                    }

                    if(cars[j].humanPlayer != null)
                    {
                        setFinishedCanvas = true;
                        RacingCanvasController.racingCanvas.FinishedCanvas(cars[j]);
                    }

                    GameObject obj = Instantiate(positionsInfoPrefab, transform , true);


                    curObjs.Add(obj);
                    obj.SetActive(true);

                    PositionBar posBar = obj.GetComponent<PositionBar>();

                    posBar.positionText.text = (i + 1).ToString();

                    if (cars[j].humanPlayer != null)
                    {
                        if (cars[j].humanPlayer.name != "")
                            posBar.nameText.text = cars[j].humanPlayer.name;
                        else
                            posBar.nameText.text = "Player";
                    }
                    else
                    {
                        posBar.nameText.text = cars[j].aIPlayer.name;
                    }

                    int timeM = (int)(cars[j].GetFinishTime() / 60);
                    int timeS = (int)(cars[j].GetFinishTime() - timeM * 60);
                    int timeMS = (int)((cars[j].GetFinishTime() - timeM * 60 - timeS) * 100);

                    posBar.totalTimeText.text = timeM.ToString("D2") + ":" + timeS.ToString("D2") + ":" + timeMS.ToString("D2");

                    if (i > 0)
                    {
                        float diffTime = cars[j].GetFinishTime() - bestTime;

                        timeM = (int)(diffTime / 60);
                        timeS = (int)(diffTime - timeM * 60);
                        timeMS = (int)((diffTime - timeM * 60 - timeS) * 100);

                        if (timeM != 0)
                            posBar.diffText.text = "+ " + timeM.ToString() + ":" + timeS.ToString("D2") + ":" + timeMS.ToString("D2");
                        else if (timeM == 0 && timeS != 0)
                            posBar.diffText.text = "+ " + timeS.ToString() + ":" + timeMS.ToString("D2");
                        else if (timeM == 0 && timeS == 0 && timeMS != 0)
                            posBar.diffText.text = "+ " + timeS.ToString("D1") + ":" + timeMS.ToString("D2");

                        break;
                    }
                    else
                        posBar.diffText.text = "";
                }
            }
        }

        if(!setFinishedCanvas)
            RacingCanvasController.racingCanvas.FinishedCanvas(null);

    }

    void DestroyAll()
    {
        for (int i = 0; i < curObjs.Count; i++)
            Destroy(curObjs[i]);

        curObjs.Clear();
    }

}
