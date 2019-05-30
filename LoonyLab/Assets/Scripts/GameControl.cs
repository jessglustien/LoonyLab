﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameControl : MonoBehaviour
{
    public GameObject player;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject balancingScreen;
    public GameObject customer1;
    public GameObject customer2;

    public Text chem1;
    public Text chem2;
    public Text result;
    public Text customer1Text;
    public Text customer2Text;

    public Text fix2;
    public Text fix3;
    public Text fix4;
    public Text fix5;

    public InputField inChem1;
    public InputField inChem2;
    public InputField inResult;

    public int threshold;
    private Dictionary<string, Color> chemicals = new Dictionary<string, Color>();
    private Dictionary<Tuple<string, string>, Tuple<string, int, int, int, Color>> results = new Dictionary<Tuple<string, string>, Tuple<string, int, int, int, Color>>();
    private string leftName = "X";
    private string rightName = "X";

    private bool balancing = false;

    private char sub_2 = (char)8322;
    private char sub_3 = (char)8323;
    private List<string> chemNames = new List<string>();
    private List<string> orders = new List<string>();

    // Start is called before the first frame update
    void Start()
    {

        chemicals["Fe"] = Color.blue;
        chemicals["Cl" + sub_2] = Color.red;
        chemicals["O" + sub_2] = Color.green;
        chemicals["H" + sub_2] = Color.gray;

        results[Tuple.Create("Fe", "Cl" + sub_2)] = Tuple.Create("FeCl" + sub_3, 2, 3, 2, Color.magenta);
        results[Tuple.Create("Cl" + sub_2, "Fe")] = Tuple.Create("FeCl" + sub_3, 3, 2, 2, Color.magenta);
        results[Tuple.Create("Fe", "O" + sub_2)] = Tuple.Create("Fe" + sub_2 +"O" + sub_3, 4, 3, 2, Color.cyan);
        results[Tuple.Create("O" + sub_2, "Fe")] = Tuple.Create("Fe" + sub_2 + "O" + sub_3, 3, 4, 2, Color.cyan);
        results[Tuple.Create("O" + sub_2, "H" + sub_2)] = Tuple.Create("H" + sub_2 + "O", 1, 2, 2, Color.black);
        results[Tuple.Create("H" + sub_2, "O" + sub_2)] = Tuple.Create("H" + sub_2 + "O", 2, 1, 2, Color.black);


        chemNames.Add("Fe");
        chemNames.Add("Cl" + sub_2);
        chemNames.Add("O" + sub_2);
        chemNames.Add("H" + sub_2);

        orders.Add("FeCl" + sub_3);
        orders.Add("Fe" + sub_2 + "O" + sub_3);
        orders.Add("FeCl" + sub_3);
        orders.Add("H" + sub_2 + "O");
        orders.Add("Fe" + sub_2 + "O" + sub_3);

        fix2.text = "Cl" + sub_2;
        fix3.text = "O" + sub_2;
        fix4.text = "H" + sub_2;
        fix5.text = "Fe" + sub_2 + "O" + sub_3;

        GenerateCustomers();
        InvokeRepeating("GenerateCustomers", 2.0f, 5.0f);
    }

    // Update is called once per frame
    void Update()
    {
       if (balancing)
        {
            if (CheckCorrect())
            {
                var tpl = Tuple.Create(leftName, rightName);
                balancing = false;
                balancingScreen.SetActive(false);
                rightHand.SetActive(true);
                rightName = results[tpl].Item1;
                SpriteRenderer sr = rightHand.GetComponent<SpriteRenderer>();
                sr.color = results[tpl].Item5;
                inChem1.text = "";
                inChem2.text = "";
                inResult.text = "";
            }
        } 
    }

    public void GenerateCustomers()
    {
        if (orders.Count != 0)
        {
            if (!customer1.activeSelf)
            {
                customer1.SetActive(true);
                customer1Text.text = orders[0];
                orders.RemoveAt(0);
            }
            if (!customer2.activeSelf && orders.Count != 0)
            {
                customer2.SetActive(true);
                customer2Text.text = orders[0];
                orders.RemoveAt(0);
            }
        }
    }


    public void ChemicalClick(int chemNum)
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;
        string chemName = chemNames[chemNum];

        if (Math.Abs(player_y - 250) < threshold && player_x < 0)
        {
            if (!rightHand.activeSelf)
            {
                rightHand.SetActive(true);
                SpriteRenderer sr = rightHand.GetComponent<SpriteRenderer>();
                rightName = chemName;
                sr.color = chemicals[chemName];
            }
            else if (!leftHand.activeSelf)
            {
                leftHand.SetActive(true);
                SpriteRenderer sr = leftHand.GetComponent<SpriteRenderer>();
                leftName = chemName;
                sr.color = chemicals[chemName];
            }
        }
    }

    public bool CheckCorrect()
    {
        string a = inChem1.text;
        string b = inChem2.text;
        string c = inResult.text;


        var tpl = Tuple.Create(leftName, rightName);

        return (a == results[tpl].Item2.ToString() && b == results[tpl].Item3.ToString() && c == results[tpl].Item4.ToString());

    }

    public void TrashClick(bool right)
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (Math.Abs(player_y + 250) < threshold)
        {
            if (right && Math.Abs(player_x + 100) < threshold)
            {
                rightHand.SetActive(false);
                rightName = "X";
            }
            else if (!right && Math.Abs(player_x - 70) < threshold)
            {
                leftHand.SetActive(false);
                leftName = "X";
            }
        }
    }

    public void BalanceClick()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y < threshold && 6.5 < player_x && leftHand.activeSelf && rightHand.activeSelf)
        {
            if (CheckChemicals())
            {
                balancingScreen.SetActive(true);
                leftHand.SetActive(false);
                rightHand.SetActive(false);

                balancing = true;

                var tpl = Tuple.Create(leftName, rightName);
                chem1.text = leftName;
                chem2.text = rightName;
                result.text = results[tpl].Item1;
            }

        }

    }

    public bool CheckChemicals()
    {
        return results.ContainsKey(Tuple.Create(leftName, rightName));
    }

    public void CustomerClick(int num)
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;
        string order = "";
        if (num == 0)
        {
            order = customer1Text.text;
        }
        else if (num == 1)
        {
            order = customer2Text.text;
        }

        if (player_x < -5.5 && player_y > -1)
        {
            if (order == rightName)
            {
                if (num == 0)
                    customer1.SetActive(false);
                else if (num == 1)
                    customer2.SetActive(false);
                rightHand.SetActive(false);

            }
        }

    }

}

