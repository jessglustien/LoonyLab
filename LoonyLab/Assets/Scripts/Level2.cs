﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class Level2 : MonoBehaviour
{
    public GameObject player; // Player object.
    public GameObject Hand; // Player's hand (inventory).
    public GameObject balancingScreen; // Screen that appears when balancing chemicals.
    public GameObject customer1; // First customer object. 
    public GameObject endScreen; // End of level screen.

    public Text chem1; //Text used in balancing screen.
    public Text chem2;
    public Text result;
    public Text chem1Total;
    public Text chem2Total;
    public Text resultTotal;
    public Text customer1Text;

    public Text fix1; //Adding subscripts to element labels.
    public Text fix2;
    public Text fix3;

    public int threshold; //Distance player can be from objects in order to interact.

    private Chemical InHand; // Chemical player is currently holding. 


    private char sub_2 = (char)8322; // Subscript 2
    private char sub_3 = (char)8323; // Subscript 3


    private List<Chemical> chemicals = new List<Chemical>(); // List of chemicals player can pick up. 
    private List<string> orders = new List<string>(); // List of orders customers will make throughout level. 

    private Dictionary<Tuple<Chemical, Chemical>, Chemical> results = new Dictionary<Tuple<Chemical, Chemical>, Chemical>(); // Dictionary of possible reactions player can complete.

    private BalancingStation balanceStn = new BalancingStation(); // Balancing station object. Used to complete reactions.

    // Start is called before the first frame update
    void Start()
    {
        // Create chemicals used in the level.

        Chemical fe = new Chemical("Fe", 1, 0, false, Color.blue, "Fe");
        Chemical cl2 = new Chemical("Cl" + sub_2, 2, 0, false, Color.red, "Cl");
        Chemical o2 = new Chemical("O" + sub_2, 2, 0, false, Color.green, "O");
        Chemical h2 = new Chemical("H" + sub_2, 2, 0, false, Color.magenta, "H");

        // Load possible reactions into dictionary.

        results[Tuple.Create(fe, cl2)] = new Chemical("FeCl" + sub_3, 1, 3, true, Color.yellow, "FeCl");
        results[Tuple.Create(cl2, fe)] = new Chemical("FeCl" + sub_3, 3, 1, true, Color.yellow, "FeCl");
        results[Tuple.Create(fe, o2)] = new Chemical("Fe" + sub_2 + "O" + sub_3, 2, 3, true, Color.cyan, "FeO");
        results[Tuple.Create(o2, fe)] = new Chemical("Fe" + sub_2 + "O" + sub_3, 3, 2, true, Color.cyan, "FeO");
        results[Tuple.Create(h2, o2)] = new Chemical("H" + sub_2 + "O", 2, 1, true, Color.black, "HO");
        results[Tuple.Create(o2, h2)] = new Chemical("H" + sub_2 + "O", 1, 2, true, Color.black, "HO");

        // Add chemicals to list.

        chemicals.Add(fe);
        chemicals.Add(cl2);
        chemicals.Add(o2);
        chemicals.Add(h2);

        // Load orders for the level.

        orders.Add("Fe" + sub_2 + "O" + sub_3);
        orders.Add("H" + sub_2 + "O");
        orders.Add("FeCl" + sub_3);
        orders.Add("H" + sub_2 + "O");
        orders.Add("Fe" + sub_2 + "O" + sub_3);

        // Fix subscripts.

        fix1.text = "H" + sub_2;
        fix2.text = "Cl" + sub_2;
        fix3.text = "O" + sub_2;

        // Load customers for the level and have new ones appear as player completes orders.

        GenerateCustomers();
        InvokeRepeating("GenerateCustomers", 2.0f, 2.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateCustomers()
    {
        // Load new customers if there are empty spaces available.

        if (orders.Count != 0) // Check if there are still orders left to complete. 
        {
            if (!customer1.activeSelf) // Check if customer 1 space is available.
            {
                customer1.SetActive(true);
                customer1Text.text = orders[0]; // Load next order in line. 
                orders.RemoveAt(0); // Remove loaded order from list. 
            }
        }
    }


    public void ChemicalClick(int chemNum)
    {
        // Allow player to pick up selected chemical if player is not already holding something. 

        Chemical chem = chemicals[chemNum];
            if (!Hand.activeSelf)
            {
                Hand.SetActive(true);
                SpriteRenderer sr = Hand.GetComponent<SpriteRenderer>();
                InHand = chem;
                sr.color = InHand.Colour; 
            }
        
    }


    public void TrashClick()
    { 
        // Allow player to throwout whatever chemical they are currently holding. 

        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        Hand.SetActive(false);
            
        
    }

    public void BalanceClick()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (Hand.activeSelf)
        {

            balancingScreen.SetActive(true);
            Hand.SetActive(false);
            if (balanceStn.Reactant1 == null)
            {
                balanceStn.Reactant1 = InHand;
                balanceStn.QuantityR1 = 1;
                chem1.text = "1  " + InHand.Name;
                chem1Total.text = "Total: " + InHand.Subscript1.ToString() + " " + InHand.SingleName + " Molecules";

            }
            else
            {
                if (balanceStn.Reactant1 == InHand)
                {
                    balanceStn.QuantityR1++;
                    chem1.text = balanceStn.QuantityR1.ToString() + " " + InHand.Name;
                    chem1Total.text = "Total: " + (balanceStn.Reactant1.Subscript1 * balanceStn.QuantityR1).ToString() + " " + InHand.SingleName + " Molecules";
                    UpdateBalanced();
                }
                else
                {
                    if (balanceStn.Reactant2 == null)
                    {
                        balanceStn.Reactant2 = InHand;
                        balanceStn.QuantityR2 = 1;
                        chem2.text = "1  " + InHand.Name;
                        chem2Total.text = "Total: " + InHand.Subscript1 + " " + InHand.SingleName + " Molecules";

                        if (results.ContainsKey(Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)))
                        {
                            balanceStn.Product = results[Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)];
                            UpdateBalanced();
                        }
                        
                    }
                    else
                    {
                        if (balanceStn.Reactant2 == InHand)
                        {
                            balanceStn.QuantityR2++;
                            chem2.text = balanceStn.QuantityR2.ToString() + " " + InHand.Name;
                            chem2Total.text = "Total: " + (InHand.Subscript1 * balanceStn.QuantityR2).ToString() + " " + InHand.SingleName + " Molecules";
                            UpdateBalanced();
                        }
                        else
                        {
                            balancingScreen.SetActive(false);
                            Hand.SetActive(true);
                        }
                    }
                }
            }
        }
        balancingScreen.SetActive(true);
        player.GetComponent<PlayerController>().balancing = true;

    }

    public void UpdateBalanced()
    {

        int num1 = (int)Math.Ceiling(balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
        int num2 = (int)Math.Ceiling(balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
        int goal = Math.Max(num1, num2);
        result.text = goal.ToString() + " " + balanceStn.Product.Name;
        int missing1 = balanceStn.Product.Subscript1 * goal - balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1;
        int missing2 = balanceStn.Product.Subscript2 * goal - balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1;

        if (missing1 == 0 && missing2 == 0)
        {
            // Balanced 

            resultTotal.text = "Balanced! You have finished the reaction for creating " + balanceStn.Product.Name + "!";
            Hand.SetActive(true);
            InHand = balanceStn.Product;

            SpriteRenderer sr = Hand.GetComponent<SpriteRenderer>();
            sr.color = InHand.Colour;
        }
        else
        {
            // Not balanced
            string text1 = "Needed: " + (balanceStn.Product.Subscript1 * goal).ToString() + " " + balanceStn.Reactant1.SingleName;
            string text2 = " " + (balanceStn.Product.Subscript2 * goal).ToString() + " " + balanceStn.Reactant2.SingleName;

            resultTotal.text = text1 + text2 + " Missing " + missing1.ToString() + " " + balanceStn.Reactant1.SingleName + " and " + missing2.ToString() + " " + balanceStn.Reactant2.SingleName;
        }
    }


    public void CustomerClick()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        string order = customer1Text.text;

            if (order == InHand.Name)
            {
                customer1.SetActive(false);

                Hand.SetActive(false);
                if (orders.Count == 0)
                {
                    EndLevel();
                }

            }
        

    }

    public void EndLevel()
    {
        endScreen.SetActive(true);
    }

    public void CloseScreen()
    {
        balancingScreen.SetActive(false);
        player.GetComponent<PlayerController>().balancing = false;
        if (Hand.activeSelf)
            ClearScreen();
    }

    public void ClearScreen()
    {
        balanceStn.Reactant1 = null;
        balanceStn.Reactant2 = null;
        balanceStn.QuantityR1 = 0;
        balanceStn.QuantityR2 = 0;
        balanceStn.Product = null;

        chem1.text = "";
        chem2.text = "";
        result.text = "";
        chem1Total.text = "";
        chem2Total.text = "";
        resultTotal.text = "";
    }

    public void NextLevel()
    {
        SceneManager.LoadScene("Level2");
    }

}






