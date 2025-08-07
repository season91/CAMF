using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Firestore;
using Google;

public enum LoginType
{
    Guest,
    Google
}

/// <summary>
/// Firebase 관련 기능 
/// </summary>
public class FirebaseManager : Singleton<FirebaseManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser DbUser { get; private set; }
    
    public bool IsNewUser { get; private set; } = false;
    
    private LoginType CurrentLoginType { get; set; } = LoginType.Guest; // 기본값 게스트

    /// <summary>
    /// DB 체크 및 앱 초기화
    /// </summary>
    public async Task InitializeFirebase()
    {
        var result = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (result == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            MyDebug.Log("Firebase 초기화 완료");
            
#if UNITY_EDITOR
#else
            // 실시간 확인을 위함. 디바이스에서만 동작함
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.SetSessionTimeoutDuration(TimeSpan.FromSeconds(1800));
#endif
        }
        else
        {
            MyDebug.LogError("Firebase 초기화 실패: " + result);
        }
    }

    // 시간 텀이 필요함
    private IEnumerator CheckForUpdate()
    {
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// UIentry에서 호출
    /// </summary>
    public void SignInWithGoogle(Action onComplete = null)
    {
        StartCoroutine(CheckForUpdate());
        // Google 로그인 시작
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = FirebaseSecrets.WebClientId, // Firebase 콘솔에서 추가한 안드 id
            RequestIdToken = true,
            RequestEmail = true
        };
        MyDebug.Log("GoogleSignIn.Configuration 설정 완료");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                MyDebug.LogError("Google Sign-In was canceled");
                return;
            }

            if (task.IsFaulted)
            {
                MyDebug.LogError("Google Sign-In failed: " + task.Exception);
            }

            MyDebug.Log("Google Sign-In success: " + task.Result.DisplayName);
            GoogleSignInUser user = task.Result;

            // Firebase에 구글 인증 정보 전달
            Credential credential = GoogleAuthProvider.GetCredential(user.IdToken, null);
            Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(async authTask => {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    MyDebug.LogError("Firebase Sign-In 실패: " + authTask.Exception);
                    return;
                }

                DbUser =  authTask.Result;
                MyDebug.Log($"Firebase 로그인 성공: UID={DbUser.UserId}, 이메일={DbUser.Email}");
                
                // 신규 유저 여부 판단
                // await HandlePostLoginAsync();
                // Firestore 유저 존재 여부 확인
                bool exists = await FirestoreUploader.CheckUserExists(DbUser.UserId);
                MyDebug.Log($"CheckUserExists 결과: UID={DbUser.UserId}, exists={exists}");
                
                IsNewUser = !exists;
                onComplete?.Invoke(); 
            });
        });

        CurrentLoginType = LoginType.Google;
    }

    /// <summary>
    /// Firebase 익명 게스트 로그인 처리 및 기존 계정 상태 분기 처리
    /// </summary>
    public async Task SignInAsGuestAsync()
    {
        // 현재 로그인된 유저가 있는 경우
        if (Auth.CurrentUser != null)
        {
            // 현재 로그인된 계정이 구글 계정인지 확인
            if (IsGoogleSignedIn())
            {
                // 구글 로그인 상태면 로그아웃 후 익명 로그인 진행
                await SignOutAndSignInAsGuestAsync();
            }
            else
            {
                // 익명 계정일 경우 유효성 검사 및 로딩 처리
                await ReloadAnonymousUserAsync();
            }
        }
        else
        {
            // 앱 첫 실행 등으로 로그인 정보가 없는 경우 → 새 익명 로그인
            await SignInNewGuestAsync();
        }
    }
    
    /// <summary>
    /// 현재 로그인된 계정이 구글 로그인인지 확인
    /// </summary>
    private bool IsGoogleSignedIn()
    {
        // FirebaseUser.ProviderData에 "google.com"이 포함돼 있으면 구글 계정
        return Auth.CurrentUser?.ProviderData.Any(p => p.ProviderId == "google.com") == true;
    }

    /// <summary>
    /// 구글 로그인 상태일 때 로그아웃 처리 후 익명 로그인 재시도
    /// </summary>
    private async Task SignOutAndSignInAsGuestAsync()
    {
        MyDebug.Log("구글 계정 로그인 상태 → 로그아웃 후 게스트 로그인");

        // 현재 로그인 세션 종료
        Auth.SignOut();

        // 새로운 익명 계정으로 로그인
        await SignInNewGuestAsync();
    }

    /// <summary>
    /// 기존 익명 유저의 유효성 검사 및 데이터 로드
    /// </summary>
    private async Task ReloadAnonymousUserAsync()
    {
        try
        {
            // 서버로부터 유저 정보 강제 갱신
            await Auth.CurrentUser.ReloadAsync();

            // 정상적으로 유지된 익명 계정 처리
            DbUser = Auth.CurrentUser;
            MyDebug.Log("기존 게스트 유저 유지됨. UID: " + DbUser.UserId);

            // 유저 데이터 로드
            await LoadUserDataAsync();
        }
        catch (FirebaseException e)
        {
            // 이미 삭제된 유저일 경우: "no user record" 메시지 포함
            if (e.Message.Contains("no user record"))
            {
                MyDebug.LogWarning("삭제된 유저 감지됨. 새로 로그인 시도");

                // 새 익명 로그인 수행
                await SignInNewGuestAsync();
            }
            else
            {
                // 기타 오류는 로그만 출력
                MyDebug.LogError("Reload 실패: " + e);
            }
        }
    }

    /// <summary>
    /// 새 익명 계정으로 로그인
    /// </summary>
    private async Task SignInNewGuestAsync()
    {
        // Firebase 익명 로그인
        var result = await Auth.SignInAnonymouslyAsync();
        MyDebug.Log("신규 게스트 로그인: " + result.User.UserId);

        // 유저 정보 저장 및 닉네임 입력 요청
        DbUser = result.User;
        // OpenInputNicNameUI();
        
        // Firestore 존재 여부로 신규 유저 판별
        bool exists = await FirestoreUploader.CheckUserExists(DbUser.UserId);
        IsNewUser = !exists;
    }
    

    /// <summary>
    /// 로그인 후 유저 데이터 Load
    /// </summary>
    public async Task LoadUserDataAsync()
    {
        await UserDataHandler.LoadToUserData(DbUser.UserId);
        UserData.userProfile.lastLoginAt = Timestamp.GetCurrentTimestamp();
        await FirestoreUploader.UploadAccountData(UserData.userProfile);
        UIManager.Instance.GetUI<UIEntry>().ReadyEnterLobby();
    }

    /// <summary>
    /// 로그아웃처리
    /// </summary>
    public void SignOut()
    {
        // Firebase 로그아웃
        FirebaseAuth.DefaultInstance.SignOut();
        MyDebug.Log("Firebase 로그아웃 완료");

        // GoogleSignIn 로그아웃
        if (CurrentLoginType == LoginType.Google)
        {
            GoogleSignIn.DefaultInstance.SignOut();
            MyDebug.Log("GoogleSignIn 로그아웃 완료");
        }

        CurrentLoginType = LoginType.Guest; // 초기화
    }


    /// <summary>
    /// 자주 변하지 않는 데이터 정보 저장
    /// Save 데이터 빈 값으로 저장
    /// </summary>
    public async Task UploadFirstUserData(string nickName)
    {
        string uid = DbUser.UserId;
        string email = DbUser.Email;

        // 계정 정보
        AccountData userProfile = new AccountData(uid, email, nickName);
        try
        {
            // 게임 user 정보
            await FirestoreUploader.UploadAccountData(userProfile);
            UserInventory inventory = new UserInventory(true);
            await FirestoreUploader.SaveDataInventory(uid, inventory);
            // 기본 유닛 지급 (어셔)
            await FirestoreUploader.SaveInventoryUnit(DbUser.UserId, new InventoryUnit(SignUpData.DefaultUnitCode));
            await FirestoreUploader.SaveDataUserInfo(uid, new UserInfo());
            await FirestoreUploader.SaveUserCollectedInit(uid, new UserCollected());
            await FirestoreUploader.SaveDataUserStageInit(uid, new UserStage());
        }
        catch (Exception ex)
        {
            MyDebug.LogError($"UploadFirstUserData failed: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
