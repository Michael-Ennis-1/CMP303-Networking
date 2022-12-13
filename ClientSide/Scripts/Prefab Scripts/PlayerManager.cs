using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    // Variables for the player itself
    public int id;
    public string username;
    int speed = 2;

    // Variables for working out the prediction
    public Vector2 direction = new Vector2(0, 0);
    public Vector2 predictedPos;
    public float distance = 0;

    // Variables for lerping
    public float lerpDivision = 0.1f; // Based on GameManager's MS_PER_TICK / TICKS_PER_SEC 
    public float lerpTime = 0;

    // Variables for fixing course if prediction is wrong
    Vector2 oldPrediction;
    bool wrongPosition = false;

    // Variables to check if the newest movement data is up to date and if the player has spawned
    public bool spawned = false;
    public bool updatedInfo = false;

    // Variables for missing packets
    public int missedPacketCount = 0;

    // Variable to store the latest pos
    public movements latestPos;

    // Struct that stores the position of the player, as well as the timestamp of when the player was there
    public struct movements
    {
        public Vector2 position;
        public int timeStamp;
    }

    // Stores the 2 most recent movements
    public List<movements> movementList = new List<movements>();

    // Moves the local player based on keyboard input; uses this input to create a Direction vector then moves by it's speed
    public void LocalPlayerInputMove()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        Vector2 Direction = new Vector2(horizontalAxis, verticalAxis);
        Vector2 Translation = Direction * Time.deltaTime * speed;

        transform.Translate(Translation);
    }

    // Adds a position to the list
    public void AddPosition(movements _movement)
    {
        movementList.Add(_movement);
        CheckMovementList(movementList, _movement);
    }

    // Checks the list to make sure it's accurate
    private void CheckMovementList(List<movements> _list, movements _singleMove)
    {
        // Keeps the most recent 2 movements
        if (_list.Count > 2)
        {
            _list.RemoveRange(0, _list.Count - 2);
        }

        // Makes sure there's always 2 movements in the list
        if (_list.Count < 2)
        {
            _list.Add(_singleMove);
        }
    }

    // Returns the position of the player
    public Vector2 GetPosition()
    {
        return transform.position;
    }

    // Predicts a new position the player will be at, based on it's last two positions
    public void NextMovePrediction()
    {
        Vector2 currentPos = movementList[1].position; 
        Vector2 previousPos = movementList[0].position;

        // Checks if the prediction was wrong or not, if it was wrong, saves the old prediction
        CheckPrediction();

        // Generates a distance and direction between the current position and the previous one, and uses it to make a new predicted position
        distance = Vector2.Distance(currentPos, previousPos);
        direction = new Vector2(currentPos.x - previousPos.x, currentPos.y - previousPos.y);
        direction.Normalize();

        Vector2 translation = direction * distance;
        predictedPos = new Vector2(currentPos.x + translation.x, currentPos.y + translation.y);
    }

    // Checks if the prediction was correct and if so, saves the old predicted position
    public void CheckPrediction()
    {
        if (predictedPos == movementList[1].position)
        {
            wrongPosition = false;
        }
        else
        {
            oldPrediction = predictedPos;
            wrongPosition = true;
        }
    }

    // Moves the player towards the prediction, to attempt to smooth movement
    public void LerpToPredict()
    {
        // Updates the lerp time, so the Lerp knows how far along the player must move
        lerpTime += Time.deltaTime / lerpDivision;

        // If the prediction was correct, move from the current position to the next predicted position
        if (wrongPosition == false)
        {
            Vector2 newPos = new Vector2(Mathf.Lerp(movementList[1].position.x, predictedPos.x, lerpTime), Mathf.Lerp(movementList[1].position.y, predictedPos.y, lerpTime));

            transform.position = newPos;
        }

        // If the prediction was wrong, move from the old prediction to the new prediction
        if (wrongPosition == true)
        {
            Vector2 newPos = new Vector2(Mathf.Lerp(oldPrediction.x, predictedPos.x, lerpTime), Mathf.Lerp(oldPrediction.y, predictedPos.y, lerpTime));

            transform.position = newPos;
        }

    }

    // Destoy the player
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
