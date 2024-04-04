using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class User
{
    public string userID;
    public string userName;
    public string email;

    public User(string userID, string userName, string email)    {
        this.userID = userID;
        this.userName = userName;
        this.email = email;
    }

}
public class DataHandler : MonoBehaviour
{
    [SerializeField] User user;
    DatabaseReference referenceDatabase;
    // Start is called before the first frame update
    void Start()
    {
        referenceDatabase = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(user);
        referenceDatabase.Child("users").Child(user.userID).SetRawJsonValueAsync(json);
    }

    public void LoadData()
    {
        referenceDatabase.Child("users").Child(user.userID)
        .GetValueAsync().ContinueWithOnMainThread(task => {
        if (task.IsFaulted)
        {
            // Handle the error...
            Debug.Log("There are some errors");
        }
        else if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;
            string json = snapshot.GetRawJsonValue();
            user = JsonUtility.FromJson<User>(json);
        }
  });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
