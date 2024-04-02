using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using TMPro;

public class EmailLogin : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button signInButton;
    public Button signUpButton;
    public Button createAccountButton;
    //public Toggle rememberMeToggle;
    public TMP_InputField emailSignupInputField;
    public TMP_InputField passwordSignupInputField;
    public GameObject signupPopup;
    
    private FirebaseAuth auth;

    //private const string RememberedEmailKey = "RememberedEmail";

    void Start()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        //// Load remembered email if available
        //if (PlayerPrefs.HasKey(RememberedEmailKey))
        //{
        //    string rememberedEmail = PlayerPrefs.GetString(RememberedEmailKey);
        //    emailInputField.text = rememberedEmail;
        //}

        // Check if a user is already signed in
        //FirebaseUser currentUser = auth.CurrentUser;
        //if (currentUser != null)
        //{
        //    Debug.Log("User is already signed in: " + currentUser.DisplayName);
        //    // Here you can navigate to your main app scene or perform any other desired actions
        //}
        signUpButton.onClick.AddListener(ShowSignUp);          
        createAccountButton.onClick.AddListener(SignUpWithEmail);


        //signInButton.onClick.AddListener(() =>
        //{
        //    string email = emailInputField.text;
        //    string password = passwordInputField.text;

        //    SignInWithEmail(email, password);

        //    //// Remember email if "Remember Me" is checked
        //    //if (rememberMeToggle.isOn)
        //    //{
        //    //    PlayerPrefs.SetString(RememberedEmailKey, email);
        //    //}
        //    //else
        //    //{
        //    //    PlayerPrefs.DeleteKey(RememberedEmailKey);
        //    //}
        //    //PlayerPrefs.Save();
        //});

       
    }

    void ShowSignUp()
    {
        signupPopup.gameObject.SetActive(true);
    }

    void SignUpWithEmail()
    {
        string email = emailSignupInputField.text;
        string password = passwordSignupInputField.text;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
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
            //SignInWithEmail(email, password);
        });
    }

    //void SignInWithEmail(string email, string password)
    //{
    //    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
    //    {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
    //            return;
    //        }
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
    //            return;
    //        }

    //        // Sign-in successful
    //        AuthResult result = task.Result;
    //        Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);

    //        // Navigate to main app scene or perform any other desired actions upon successful sign-in
    //    });
    //}

    
}
