using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using Firebase;

public class EmailSignUpSignIn : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField emailSignupInputField;
    public TMP_InputField passwordSignupInputField;
    public GameObject signupPopup;
    public TextMeshProUGUI errorMessageText;

    private FirebaseAuth auth;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(CheckSignInStatus());
    }

    IEnumerator CheckSignInStatus()
    {
        // Wait until Firebase initialization is complete
        while (!FirebaseApp.CheckAndFixDependenciesAsync().IsCompleted)
        {
            yield return null;
        }

        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        //Check if a user is already signed in
        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            Debug.Log("User is already signed in: " + currentUser.DisplayName);
            // Here you can navigate to your main app scene or perform any other desired actions
            Scene currentScene = SceneManager.GetActiveScene();
            if(currentScene.buildIndex != 1)
                SceneManager.LoadScene(1);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowSignUp()
    {
        signupPopup.gameObject.SetActive(true);
    }

    public void SignUpWithEmail()
    {
        auth = FirebaseAuth.DefaultInstance;
        string email = emailSignupInputField.text;
        string password = passwordSignupInputField.text;
        if (!ValidateEmailAndPassword(email, password)) return;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Sign-up successful
            AuthResult result = task.Result;
            Debug.LogFormat("User created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);

            // Optionally, you can automatically sign in the user after they sign up
            emailInputField.text = email;
            passwordInputField.text = password;
            SignInWithEmail();
        });
    }
    
    public void SignInWithEmail()
    {
        auth = FirebaseAuth.DefaultInstance;
        string email = emailInputField.text;
        string password = passwordInputField.text;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Sign-in successful
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);

            // Navigate to main app scene or perform any other desired actions upon successful sign-in
            SceneManager.LoadScene(1);
        });
    }

    public void SignOut()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();
        Debug.Log("User signed out successfully.");

        // Perform any additional actions after sign out if needed
        SceneManager.LoadScene(0);
    }

    bool ValidateEmailAndPassword(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorMessageText.text = "Email and password cannot be empty.";
            return false;
        }

        if (!IsValidEmail(email))
        {
            errorMessageText.text = "Invalid email format.";
            return false;
        }

        if (password.Length < 6)
        {
            errorMessageText.text = "Password must be at least 6 characters long.";
            return false;
        }

        // Check for at least one uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errorMessageText.text = "Password must contain at least one uppercase letter.";
            return false;
        }

        // Check for at least one special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()\,.?]"))
    {
            errorMessageText.text = "Password must contain at least one special character.";
            return false;
        }

        // Clear error message if validation passes
        errorMessageText.text = "";
        return true;
    }

    bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
