﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using static StaticVars;

public class Level3 : MonoBehaviour
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
    public Text balanceHoverText;
    public Text ordersDone;
    public Image compoundImage;
    private List<Sprite> ordersImages = new List<Sprite>();

    public GameObject CHover;
    public GameObject O2Hover;
    public GameObject H2Hover;
    public GameObject BalanceHover;
    public GameObject CustomerHover;
    public GameObject TrashHover;

    public List<GameObject> sec1;

    public List<GameObject> sec2;

    public List<GameObject> sec3;

    public Sprite c_sprite;
    public Sprite n2_sprite;
    public Sprite o2_sprite;

    public List<Sprite> n20_sprites;
    public List<Sprite> co_sprites;
    public List<Sprite> n20_backwards_sprites;
    public List<Sprite> co_backwards_sprites;


    public Text fix1;
    public Text fix2; //Adding subscripts to element labels.
    public Text fix3;

    private Chemical InHand; // Chemical player is currently holding. 

    public List<Sprite> molecules;

    private bool balancing = false;

    private int ordersCompleted = 0;

    private bool reset = false;


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

        Chemical c = new Chemical("C", 1, 0, false, new List<Sprite> { c_sprite }, "C");
        Chemical o2 = new Chemical("O" + sub_2, 2, 0, false, new List<Sprite> { o2_sprite }, "O");
        Chemical n2 = new Chemical("N" + sub_2, 2, 0, false, new List<Sprite> { n2_sprite }, "N");

        // Load possible reactions into dictionary.

        results[Tuple.Create(n2, o2)] = new Chemical("N" + sub_2 + "O", 2, 1, true, n20_sprites, "NO");
        results[Tuple.Create(o2, n2)] = new Chemical("N" + sub_2 + "O", 1, 2, true, n20_backwards_sprites, "NO");
        results[Tuple.Create(c, o2)] = new Chemical("CO", 1, 1, true, co_sprites, "CO");
        results[Tuple.Create(o2, c)] = new Chemical("CO", 1, 1, true, co_backwards_sprites, "CO");

        // Add chemicals to list.

        chemicals.Add(c);
        chemicals.Add(o2);
        chemicals.Add(n2);

        // Load orders for the level.

        orders.Add("CO");
        orders.Add("N" + sub_2 + "O");
        orders.Add("CO");

        ordersImages.Add(co_sprites[co_sprites.Count - 1]);
        ordersImages.Add(n20_sprites[n20_sprites.Count - 1]);
        ordersImages.Add(co_sprites[co_sprites.Count - 1]);

        // Fix subscripts.

        fix1.text = "C";
        fix2.text = "O" + sub_2;
        fix3.text = "N" + sub_2;

        InHand = n2;

        // Load customers for the level and have new ones appear as player completes orders.

        GenerateCustomers();
        InvokeRepeating("GenerateCustomers", 2.0f, 2.0f);


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (endScreen.activeSelf)
            {
                NextLevel();
            }
            else if (balancing)
            {
                CloseScreen();
            }
            else
            {
                if (CheckBalance())
                    BalanceClick();
                if (CheckFe())
                    ChemicalClick(0);
                if (CheckCl2())
                    ChemicalClick(1);
                if (CheckO2())
                    ChemicalClick(2);
                if (CheckCustomer())
                    CustomerClick();
                if (CheckTrash())
                    TrashClick();
            }
        }
        else if (Input.GetKeyDown(KeyCode.R) && balancing)
        {
            ClearScreen();
        }

        if (CheckBalance() && Hand.activeSelf && !InHand.Product)
        {
            BalanceHover.SetActive(true);
            balanceHoverText.text = "Press SPACE to add item to balancing station.";
        }
        else if (CheckBalance())
        {
            BalanceHover.SetActive(true);
            balanceHoverText.text = "Press SPACE to view items in balancing station.";
        }
        else
            BalanceHover.SetActive(false);


        if (CheckFe() && !Hand.activeSelf)
            CHover.SetActive(true);
        else
            CHover.SetActive(false);

        if (CheckCl2() && !Hand.activeSelf)
            O2Hover.SetActive(true);
        else
            O2Hover.SetActive(false);

        if (CheckCustomer() && Hand.activeSelf)
        { 
            if (InHand.Product)
                CustomerHover.SetActive(true);
        }
        else
            CustomerHover.SetActive(false);
        if (CheckO2() && !Hand.activeSelf)
            H2Hover.SetActive(true);
        else
            H2Hover.SetActive(false);
        if (CheckTrash() && Hand.activeSelf)
            TrashHover.SetActive(true);
        else
            TrashHover.SetActive(false);
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
                compoundImage.sprite = ordersImages[0];
                ordersImages.RemoveAt(0);
            }
        }
    }


    public void ChemicalClick(int chemNum)
    {
        // Allow player to pick up selected chemical if player is not already holding something. 

        Chemical chem = chemicals[chemNum];
            if (!Hand.activeSelf)
            {
                chemNumPublic = chemNum;
                chemClickEvent = true;
                FindObjectOfType<AudioManager>().Play("cabinetUse");
                Hand.SetActive(true);
                SpriteRenderer sr = Hand.GetComponent<SpriteRenderer>();
                InHand = chem;
                sr.sprite = InHand.Colour[0];
            }
        
    }


    public void TrashClick()
    { 
        // Allow player to throwout whatever chemical they are currently holding. 
        Hand.SetActive(false);
        InHand = chemicals[0];
        FindObjectOfType<AudioManager>().Play("trashUse");
        trashClickEvent = true;
            
        
    }

    public void BalanceClick()
    {
        if (!CheckAddItem())
        {
            result.text = "Balance Station Overloaded!";
            resultTotal.text = "It will now reset.";
            reset = true;
            Hand.SetActive(false);
        }
        if (Hand.activeSelf && !InHand.Product)
        {

            balancingScreen.SetActive(true);
            Hand.SetActive(false);
            FindObjectOfType<AudioManager>().Play("addToBalStation");
            balstationActive = true;

            if (balanceStn.Reactant1 == null && InHand != balanceStn.Reactant1)
            {
                balanceStn.Reactant1 = InHand;
                balanceStn.QuantityR1 = 1;
                chem1.text = "1  " + InHand.Name;
                chem1Total.text = "Total: " + InHand.Subscript1.ToString() + " " + InHand.SingleName + " Atoms";

            }
            else
            {
                if (balanceStn.Reactant1 == InHand && CheckAddItem())
                {
                    balanceStn.QuantityR1++;
                    chem1.text = balanceStn.QuantityR1.ToString() + " " + InHand.Name;
                    chem1Total.text = "Total: " + (balanceStn.Reactant1.Subscript1 * balanceStn.QuantityR1).ToString() + " " + InHand.SingleName + " Atoms";
                    if (balanceStn.Product != null && balanceStn.Reactant2 != null)
                        UpdateBalanced();
                }
                else
                {
                    if (balanceStn.Reactant2 == null)
                    {
                        balanceStn.Reactant2 = InHand;
                        balanceStn.QuantityR2 = 1;
                        chem2.text = "1  " + InHand.Name;
                        chem2Total.text = "Total: " + InHand.Subscript1 + " " + InHand.SingleName + " Atoms";

                        if (results.ContainsKey(Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)))
                        {
                            balanceStn.Product = results[Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)];
                            UpdateBalanced();
                        }

                    }
                    else
                    {
                        if (balanceStn.Reactant2 == InHand && CheckAddItem())
                        {
                            balanceStn.QuantityR2++;
                            chem2.text = balanceStn.QuantityR2.ToString() + " " + InHand.Name;
                            chem2Total.text = "Total: " + (InHand.Subscript1 * balanceStn.QuantityR2).ToString() + " " + InHand.SingleName + " Atoms";
                            UpdateBalanced();
                        }
                        else
                        {
                            balancingScreen.SetActive(false);
                            Hand.SetActive(true);
                        }
                    }
                }
                if (balanceStn.Reactant1 != null && balanceStn.Reactant2 != null)
                {
                    if (!results.ContainsKey(Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)))
                    {
                        result.text = "Wrong ingredients!";
                        resultTotal.text = "Reset balancing station and try again!";
                        FindObjectOfType<AudioManager>().Play("incorrectIngredient");
                    }
                }
            }
        }
        if (!CheckAddItem2())
        {
            result.text = "Balance Station Overloaded!";
            resultTotal.text = "It will now reset.";
            reset = true;
            Hand.SetActive(false);
        }
        DisplayAtomImages();
        FindObjectOfType<AudioManager>().Play("viewBalStation");
        balancingScreen.SetActive(true);
        player.GetComponent<PlayerController>().balancing = true;
        balancing = true;
    }


    public void UpdateBalanced()
    {

        int num1 = (int)Math.Ceiling(balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
        int num2 = (int)Math.Ceiling(balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
        int goal = Math.Max(num1, num2);
        result.text = "_ " + balanceStn.Product.Name;
        int missing1 = balanceStn.Product.Subscript1 * goal - balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1;
        int missing2 = balanceStn.Product.Subscript2 * goal - balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1;

        if (missing1 == 0 && missing2 == 0)
        {
            // Balanced 
            result.text = goal.ToString() + " " + balanceStn.Product.Name;
            resultTotal.text = "Balanced! You have finished the reaction for creating " + balanceStn.Product.Name + "!";
            Hand.SetActive(true);
            InHand = balanceStn.Product;

            SpriteRenderer sr = Hand.GetComponent<SpriteRenderer>();
            sr.sprite = molecules[7];
            FindObjectOfType<AudioManager>().Play("successBalance");
            deliveryLightsOn = true;
            compoundMade = true;
        }
        else
        {
            // Not balanced
           

            resultTotal.text = " Missing: " + missing1.ToString() + " " + balanceStn.Reactant1.SingleName + " and " + missing2.ToString() + " " + balanceStn.Reactant2.SingleName;
        }
    }


    public void CustomerClick()
    {

        string order = customer1Text.text;

            if (order == InHand.Name) {
                customer1.SetActive(false);
                ordersCompleted++;
                ordersDone.text = ordersCompleted.ToString() + "/3";
            customer1Text.text = "";

                Hand.SetActive(false);
                if (orders.Count == 0) {
                    EndLevel();
                }
                deliveryLightsOn = false;
                conveyorBeltEvent = true;
                FindObjectOfType<AudioManager>().Play("deliverCompound");
            }
    }

    public void EndLevel()
    {
        chemNumPublic = 99;
        chemClickEvent = false;
        trashClickEvent = false;
        deliveryLightsOn = false;
        conveyorBeltEvent = false;
        FindObjectOfType<AudioManager>().Play("winLevel");
        endScreen.SetActive(true);
        player.GetComponent<PlayerController>().balancing = true;
    }

    public void CloseScreen()
    {
        balancingScreen.SetActive(false);
        balancing = false;
        player.GetComponent<PlayerController>().balancing = false;
        if (Hand.activeSelf || reset)
        {
            ClearScreen();
            reset = false;
        }
    }

    public void ClearScreen()
    {
        FindObjectOfType<AudioManager>().Play("clearBalStation");
        balstationActive = false;
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
        ResetAtomImages();
    }

    public void NextLevel()
    {
        SceneManager.LoadScene("Level4");
    }

    public bool CheckBalance()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y > -1.13 && player_y < 1.55)
        {
            if (player_x > 3.86)
                return true;
        }
        return false;
    }

    public bool CheckCustomer()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y > -0.8 && player_y < 0.6)
        {
            if (player_x < -3.26)
                return true;
        }
        return false;
    }

    public bool CheckFe() // Actually C on top right
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y > 0.82)
        {
            if (player_x > 2.1 && player_x < 2.9)
                return true;
        }
        return false;
    }

    public bool CheckCl2() // Actually O2 on bottom right
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y < -1.2)
        {
            if (player_x > 2.1 && player_x < 2.9)
                return true;
        }
        return false;
    }

    public bool CheckO2()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y > 0.82)
        {
            if (player_x > -2.9 && player_x < -1.9)
                return true;
        }
        return false;
    }

    public bool CheckTrash()
    {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.y;

        if (player_y > 0.82)
        {
            if (player_x > -0.91 && player_x < -0.21)
                return true;
        }
        return false;
    }

    public void DisplayAtomImages()
    {
        if (balanceStn.Reactant1 != null)
        {
            for (int i = 0; i < balanceStn.QuantityR1; i++)
            {
                sec1[i].SetActive(true);
                Image sr = sec1[i].GetComponent<Image>();
                sr.sprite = balanceStn.Reactant1.Colour[0];

            }
        }

        if (balanceStn.Reactant2 != null)
        {
            for (int i = 0; i < balanceStn.QuantityR2; i++)
            {
                sec2[i].SetActive(true);
                Image sr = sec2[i].GetComponent<Image>();
                sr.sprite = balanceStn.Reactant2.Colour[0];

            }
        }



        if (results.ContainsKey(Tuple.Create(balanceStn.Reactant1, balanceStn.Reactant2)))
        {
            int num1 = (int)Math.Ceiling(balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
            int num2 = (int)Math.Ceiling(balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
            int total_needed = Math.Max(num1, num2);

            int remaining_r1 = balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1;
            int remaining_r2 = balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1;

            for (int i = 0; i < total_needed; i++)
            {
                sec3[i].SetActive(true);
                Image sr2 = sec3[i].GetComponent<Image>();
                if (remaining_r1 >= balanceStn.Product.Subscript1 && remaining_r2 >= balanceStn.Product.Subscript2)
                {
                    sr2.sprite = balanceStn.Product.Colour[balanceStn.Product.Colour.Count - 1];
                    remaining_r1 -= balanceStn.Product.Subscript1;
                    remaining_r2 -= balanceStn.Product.Subscript2;
                }
                else if (remaining_r1 >= balanceStn.Product.Subscript1)
                {
                    int partial_sprite = (balanceStn.Product.Subscript2 + 1) * balanceStn.Product.Subscript1 + remaining_r2;
                    sr2.sprite = balanceStn.Product.Colour[partial_sprite];
                    remaining_r1 -= balanceStn.Product.Subscript1;
                    remaining_r2 = 0;
                }
                else if (remaining_r2 >= balanceStn.Product.Subscript2)
                {
                    int partial_sprite = remaining_r1 * (balanceStn.Product.Subscript2 + 1) + balanceStn.Product.Subscript2;
                    sr2.sprite = balanceStn.Product.Colour[partial_sprite];
                    remaining_r2 -= balanceStn.Product.Subscript2;
                    remaining_r1 = 0;
                }
                else
                {
                    int partial_sprite = remaining_r1 * (balanceStn.Product.Subscript2 + 1) + remaining_r2;
                    sr2.sprite = balanceStn.Product.Colour[partial_sprite];
                }

            }



        }


    }

    public void ResetAtomImages()
    {
        for (int i = 0; i < 4; i++)
        {
            sec1[i].SetActive(false);
            sec2[i].SetActive(false);
            sec3[i].SetActive(false);
        }
    }

    public bool CheckAddItem()
    {
        if (InHand == balanceStn.Reactant1 && balanceStn.Reactant2 != null)
        {
            int num1 = (int)Math.Ceiling((balanceStn.QuantityR1 + 1) * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
            int num2 = (int)Math.Ceiling(balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
            int goal = Math.Max(num1, num2);

            return (balanceStn.QuantityR1 < 4 && goal < 5);
        }
        else if (InHand == balanceStn.Reactant2 && balanceStn.Product != null)
        {
            int num1 = (int)Math.Ceiling(balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
            int num2 = (int)Math.Ceiling((balanceStn.QuantityR2 + 1) * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
            int goal = Math.Max(num1, num2);

            return (balanceStn.QuantityR2 < 4 && goal < 5);
        }
        else if (InHand == balanceStn.Reactant1)
        {
            return balanceStn.QuantityR1 < 4;
        }
        return true;

    }

    public bool CheckAddItem2()
    {
        if (InHand == balanceStn.Reactant1 && balanceStn.Reactant2 != null)
        {
            int num1 = (int)Math.Ceiling((balanceStn.QuantityR1) * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
            int num2 = (int)Math.Ceiling(balanceStn.QuantityR2 * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
            int goal = Math.Max(num1, num2);

            return (balanceStn.QuantityR1 < 5 && goal < 5);
        }
        else if (InHand == balanceStn.Reactant2 && balanceStn.Product != null)
        {
            int num1 = (int)Math.Ceiling(balanceStn.QuantityR1 * balanceStn.Reactant1.Subscript1 / (float)balanceStn.Product.Subscript1);
            int num2 = (int)Math.Ceiling((balanceStn.QuantityR2) * balanceStn.Reactant2.Subscript1 / (float)balanceStn.Product.Subscript2);
            int goal = Math.Max(num1, num2);

            return (balanceStn.QuantityR2 < 5 && goal < 5);
        }
        else if (InHand == balanceStn.Reactant1)
        {
            return balanceStn.QuantityR1 < 5;
        }
        return true;

    }

}







