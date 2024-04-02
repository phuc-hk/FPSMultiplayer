using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using System.Text.RegularExpressions;

public class GoogleSignInDemo : MonoBehaviour
{
    private FirebaseAuth auth;

    [Tooltip("Set up google sign in")]
    public TextMeshProUGUI infoText;
    private string webClientId = "190299701177-uhm7dqlm193970mcu69b4f4cendt2bqe.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Tooltip("Set up email sign up sign in")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField emailSignupInputField;
    public TMP_InputField passwordSignupInputField;
    public GameObject signupPopup;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    //if (auth == null)
                    auth = FirebaseAuth.DefaultInstance;
                    CheckUserAlreadySignedIn();
                }                       
                else
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    private void CheckUserAlreadySignedIn()
    {
        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            Debug.Log("User is already signed in: " + currentUser.DisplayName);
            // Here you can navigate to your main app scene or perform any other desired actions
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex != 1)
                SceneManager.LoadScene(1);
        }
    }

    //SIGN IN WITH GOOGLE//
    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
        }
        else
        {
            AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            AddToInformation("Email = " + task.Result.Email);
            AddToInformation("Google ID Token = " + task.Result.IdToken);
            AddToInformation("Email = " + task.Result.Email);
            SignInWithGoogleOnFirebase(task.Result.IdToken);
            SceneManager.LoadScene(1);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
                AddToInformation("Sign In Successful.");
            }
        });
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToInformation("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void AddToInformation(string str) { infoText.text += "\n" + str; }

    //ANONYMOUS SIGN IN//
    public void QuestButton()
    {
        SignInAnonymously();
    }

    void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            SceneManager.LoadScene(1);
            QuestLoginSuccess();
        });
    }

    private void QuestLoginSuccess()
    {
        SceneManager.LoadScene(1);
    }

    //EMAIL SIGN UP AND SIGN IN//
    public void ShowSignUp()
    {
        signupPopup.gameObject.SetActive(true);
    }

    public void SignUpWithEmail()
    {
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
        auth.SignOut();
        Debug.Log("User signed out successfully.");

        // Perform any additional actions after sign out if needed
        SceneManager.LoadScene(0);
    }

    bool ValidateEmailAndPassword(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            infoText.text = "Email and password cannot be empty.";
            return false;
        }

        if (!IsValidEmail(email))
        {
            infoText.text = "Invalid email format.";
            return false;
        }

        if (password.Length < 6)
        {
            infoText.text = "Password must be at least 6 characters long.";
            return false;
        }

        // Check for at least one uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            infoText.text = "Password must contain at least one uppercase letter.";
            return false;
        }

        // Check for at least one special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()\,.?]"))
        {
            infoText.text = "Password must contain at least one special character.";
            return false;
        }

        // Clear error message if validation passes
        infoText.text = "";
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