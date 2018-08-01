using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRenderer : MonoBehaviour {
    
    public Material lineUpMat;
    public Material lineDownMat;
    private const int GRAPH_LENGTH = 25;

    public void drawLines(List<float> values) {
        GL.Clear(false, false, new Color());
        GL.Begin(GL.LINES);
        float[] temp = values.ToArray();
        int x = 0;
        for (int i = values.Count - GRAPH_LENGTH; i < values.Count - 1; i++) {

            if (i < 0)
            {
                x++;
                continue;
            }
            float price = temp[i];

            if (price > temp[i + 1]) {
                lineDownMat.SetPass(0);
                GL.Color(lineDownMat.color);
            }
            else {
                lineUpMat.SetPass(0);
                GL.Color(lineUpMat.color);
            }


            if (i == values.Count - GRAPH_LENGTH || i == 0)
            {
                GL.Vertex3(getX(x), priceToY(price), -1f);
            }

            GL.Vertex3(getX(x), priceToY(price), -1f);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Vertex3(getX(x), priceToY(price), -1f);
            x++;
        }


        GL.End();
    }

    private float priceToY(float price) {
        //Range is 25-75
        //Height of field 4.75
        //Pos 1.62

        price -= 25f;
        price /= 50f;
        price *= 4.75f;
        price += 1.62f;
        price -= 4.75f / 2f;
        return price;
    }

    private float getX(float x) {
        x *= 9.75f;
        x /= float.Parse(GRAPH_LENGTH.ToString());//Number of things to display
        x += 0.12f;
        x -= 9.75f / 2f;
        return x;
    }

    void OnPostRender()
    {
        drawLines(GameObject.Find("GameManagerObject").GetComponent<GameManager>().stockValues);
    }
}
