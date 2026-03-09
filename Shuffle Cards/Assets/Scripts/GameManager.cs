using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum CardColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public enum GamePhase
{
    Setup,
    Memorize,
    Shuffle,
    Play,
    Result
}

public class GameManager : MonoBehaviour
{
    [Header("CardRefs")]
    public GameObject[] opponentCardObject = new GameObject[4];
    public GameObject[] playerCardObject = new GameObject[4];

    [Header("Game Settings")]
    public float shuffleAnimDur = 0.75f;
    public float cardSwapDur = 0.2f;

    [Header("Lifes")]
    public float opponentLifes = 4f;
    public float playerLifes = 3f;

    //intern vars
    private List<CardColor> opponentCards = new List<CardColor>();
    private List<CardColor> playerCards = new List<CardColor>();
    private int[] playerPos = new int[4]; // contains PlayerCards positions
    private Vector3[] originalPlayerPositions = new Vector3[4]; //original positions!
    private Vector3[] originalCardScale = new Vector3[4]; //original scale!
    private Color[] originalOpponentColors = new Color[4]; //original colors!
    private GamePhase currentPhase = GamePhase.Setup;
    private int selectedCardIndex = -1; //for key input

    void Start()
    {
        //safe original positions of playerCards
        for (int i = 0; i < 4; i++)
        {
            originalPlayerPositions[i] = playerCardObject[i].transform.position;
            originalCardScale[i] = playerCardObject[i].transform.localScale;
        }

        InitializeGame();
    }

    //========================
    //1st generate pattern
    //========================

    void InitializeGame()
    {
        //generate random pattern for both
        opponentCards = GenerateRandomOrder();
        playerCards = new List<CardColor>(opponentCards); //Copy from opponent

        //initialize positions (0, 1, 2, 3)
        for (int i = 0; i < 4; i++)
        {
            playerPos[i] = i;
        }

        //place cards visually
        UpdateCardVisuals();

        //safe colors after updating!
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer sr = opponentCardObject[i].GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                originalOpponentColors[i] = sr.color;
            }
        }

        currentPhase = GamePhase.Memorize;
        Debug.Log("Game started - Memorize the pattern!");

        //shuffle after 3 sek. (optional)
        StartCoroutine(AutoShuffleAfterDelay(3f));
    }

    List<CardColor> GenerateRandomOrder()
    {
        List<CardColor> cards = new List<CardColor>
        {
            CardColor.Red,
            CardColor.Blue,
            CardColor.Green,
            CardColor.Yellow
        };

        //Fisher-Yates shuflle algorythm
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardColor temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }

        return cards;
    }

    //===================================
    //2nd shuffle cards with animation
    //===================================

    IEnumerator AutoShuffleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShufflePlayerCards();
    }

    public void ShufflePlayerCards()
    {
        if (currentPhase != GamePhase.Memorize) return;

        currentPhase = GamePhase.Shuffle;
        StartCoroutine(ShuffleAnimation());
    }

    IEnumerator ShuffleAnimation()
    {
        //shuffle positions
        List<int> shuffleIndices = new List<int> { 0, 1, 2, 3 };
        for (int i = shuffleIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffleIndices[i];
            shuffleIndices[i] = shuffleIndices[j];
            shuffleIndices[j] = temp;
        }

        // save new order
        for (int i = 0; i < 4; i++)
        {
            playerPos[i] = shuffleIndices[i];
        }

        //save start positions
        Vector3[] startPositions = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            startPositions[i] = playerCardObject[i].transform.position;
        }

        //animate card movement
        float elapsed = 0f;

        while (elapsed < shuffleAnimDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shuffleAnimDur;

            //every card is moving to their new position
            for (int i = 0; i < 4; i++)
            {
                int targetSlot = playerPos[i]; //Where should the card go?
                playerCardObject[i].transform.position = Vector3.Lerp(
                    startPositions[i],
                    originalPlayerPositions[targetSlot],
                    t);
            }

            yield return null;
        }

        //make sure that end positions are exact
        for (int i = 0; i < 4; i++)
        {
            int targetSlot = playerPos[i];
            playerCardObject[i].transform.position = originalPlayerPositions[targetSlot];
        }

        //important! flip oponent cards!
        FlipOpponentCards(true);

        currentPhase = GamePhase.Play;
        Debug.Log("Cards shuffeld - Sort them in the right pattern using the 1-4 keys!");
    }

    //======================================================
    //Flip opponent cards
    //======================================================

    void FlipOpponentCards(bool hideColors)
    {
        Color greyColor = new Color(0.5f, 0.5f, 0.5f, 1f); //dark grey

        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer sr = opponentCardObject[i].GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = hideColors ? greyColor : originalOpponentColors[i];
            }
        }

        Debug.Log(hideColors ? "hidden" : "visible");
    }

    //======================================================
    //3rd Input & replacing cards
    //======================================================

    void Update()
    {
        if (currentPhase != GamePhase.Play) return;

        //Keybinds 1, 2, 3, 4
        if (Input.GetKeyDown(KeyCode.Alpha1)) HandleCardInput(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) HandleCardInput(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) HandleCardInput(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) HandleCardInput(3);

        //check with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckOrder();
        }
    }

    void HandleCardInput(int keyIndex)
    {
        if (selectedCardIndex == -1)
        {
            //Step 1: Select card
            selectedCardIndex = keyIndex;
            HighlightCard(keyIndex, true);
        }
        else
        {
            //Step 2: choose new position
            int targetSlot = keyIndex;

            if (selectedCardIndex == targetSlot)
            {
                //same position = cancel selection
                HighlightCard(selectedCardIndex, false);
                selectedCardIndex = -1;
            }
            else
            {
                //find out which card is in the selected slot
                int cardAtTarget = -1;

                for (int i = 0; i < 4; i++)
                {
                    if (playerPos[i] == targetSlot)
                    {
                        cardAtTarget = i;
                        break;
                    }
                }

                if (cardAtTarget != -1)
                {
                    //swap selected cards
                    HighlightCard(selectedCardIndex, false);
                    StartCoroutine(SwapCards(selectedCardIndex, cardAtTarget));
                    selectedCardIndex = -1;
                }
            }
        }
    }

    IEnumerator SwapCards(int index1, int index2)
    {
        //switch positions in array
        int temp = playerPos[index1];
        playerPos[index1] = playerPos[index2];
        playerPos[index2] = temp;

        //get current + swap positions
        Vector3 pos1 = playerCardObject[index1].transform.position;
        Vector3 pos2 = playerCardObject[index2].transform.position;

        float elapsed = 0f;
        //animate movement
        while (elapsed < cardSwapDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cardSwapDur;

            playerCardObject[index1].transform.position = Vector3.Lerp(pos1, pos2, t);
            playerCardObject[index2].transform.position = Vector3.Lerp(pos2, pos1, t);

            yield return null;
        }

        playerCardObject[index1].transform.position = pos2;
        playerCardObject[index2].transform.position = pos1;

        Debug.Log($"Cards {index1 + 1} and {index2 + 1} swaped!");
    }

    //===================================
    //4th checking order
    //===================================

    public void CheckOrder()
    {
        if (currentPhase != GamePhase.Play) return;

        //show opponent cards  again!
        FlipOpponentCards(false);

        int correctCards = 0;
        int wrongCards = 0;

        //compare order
        for (int i = 0; i < 4; i++)
        {
            CardColor playerCard = playerCards[playerPos[i]];
            CardColor opponentCard = opponentCards[i];

            if (playerCard == opponentCard)
            {
                correctCards++;
                Debug.Log($"Position {i}: correct ({playerCard})");
            }
            else
            {
                wrongCards++;
                Debug.LogError($"Position {i}: wrong ({playerCard} instead {opponentCard})");
            }
        }

        //oponent looses lifes for each correct card
        opponentLifes -= correctCards;
        opponentLifes = Mathf.Max(0, opponentLifes);

        //player looses lifes if more then 2 cards incorrect
        if (wrongCards > 2)
        {
            playerLifes--;
            playerLifes = Mathf.Max(0, playerLifes);
            Debug.LogError("More then 2 cards wrong -> -1 life");
        }

        Debug.Log($"Result: {correctCards} right, {wrongCards} wrong");
        Debug.Log($"Lifes - Enemy: {opponentLifes}, Player: {playerLifes}");

        currentPhase = GamePhase.Result;

        //check results
        if (opponentLifes <= 0)
        {
            Debug.Log("You won!");
        }
        else if (playerLifes <= 0)
        {
            Debug.LogError("You lost!");
        }
        else
        {
            //start next round after 2 sek.
            StartCoroutine(StartNextRoundAfterDelay(3f));
        }
    }

    IEnumerator StartNextRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        InitializeGame();
    }

    //===================================
    //Helping Functions
    //===================================

    void UpdateCardVisuals()
    {
        //you would set the sprites/colors here
        //example:
        for (int i = 0; i < 4; i++)
        {
            //set player card
            SetCardVisual(playerCardObject[i], playerCards[i]);

            //set opponent card
            SetCardVisual(opponentCardObject[i], opponentCards[i]);
        }
    }

    void SetCardVisual(GameObject cardObject, CardColor color)
    {
        //Example: set color of the SpriteRenderer
        SpriteRenderer sr = cardObject.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            switch (color)
            {
                case CardColor.Red:
                    sr.color = Color.red;
                    break;
                case CardColor.Blue:
                    sr.color = Color.blue;
                    break;
                case CardColor.Green:
                    sr.color = Color.green;
                    break;
                case CardColor.Yellow:
                    sr.color = Color.yellow;
                    break;
            }
        }

        //Or set a sprite via: sr.sprite = yourCardSprite[(int)color];
    }

    void HighlightCard(int cardIndex, bool highlight)
    {
        //scale card by 15% (works for each size)
        if (cardIndex >= 0 && cardIndex < playerCardObject.Length)
        {
            Vector3 originalScale = originalCardScale[cardIndex];
            playerCardObject[cardIndex].transform.localScale =
                highlight ? originalScale * 1.15f : originalScale;
        }
    }
}
