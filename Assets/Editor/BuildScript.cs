using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 환경별 Android 빌드 진입점. 에디터 메뉴와 CI(-executeMethod) 모두
/// <see cref="ConfigureEnvironment"/> 단일 경로를 통과하므로 두 빌드 결과가 일치한다.
///
/// CI 예:
///   Unity -batchmode -quit -projectPath . -logFile - \
///     -executeMethod BuildScript.BuildAndroidDev \
///     -outputPath Builds/dev/zoocat-dev.apk -versionCode 42
///
///   Unity -batchmode -quit -projectPath . -logFile - \
///     -executeMethod BuildScript.BuildAndroidProd \
///     -outputPath Builds/prod/zoocat.aab -versionName 1.2.0 -versionCode 42 \
///     -keystorePath keystore/zoocat-release.keystore -keystorePass *** \
///     -keyaliasName zoocat -keyaliasPass ***
/// </summary>
public static class BuildScript
{
    private enum Env { Dev, Prod }

    private const string DevApplicationId = "cloud.zoocat.game.dev";
    private const string ProdApplicationId = "cloud.zoocat.game";
    private const string DevProductName = "우당탕고양이 DEV";
    private const string ProdProductName = "우당탕 고양이";

    // ---------- 메뉴 (로컬) ----------

    [MenuItem("Tools/Build/Android Dev (APK)")]
    public static void BuildAndroidDev() => Run(Env.Dev);

    [MenuItem("Tools/Build/Android Prod (AAB)")]
    public static void BuildAndroidProd() => Run(Env.Prod);

    // ---------- 코어 ----------

    private static void Run(Env env)
    {
        ConfigureEnvironment(env);

        string outputPath = GetArg("-outputPath", DefaultOutputPath(env));
        string dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("[BuildScript] EditorBuildSettings 에 enabled 씬이 없습니다.");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = env == Env.Dev
                ? (BuildOptions.Development | BuildOptions.AllowDebugging)
                : BuildOptions.None,
        };

        Debug.Log($"[BuildScript] env={env} out={outputPath} scenes={scenes.Length}");

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
            throw new Exception($"[BuildScript] build FAILED: result={summary.result} errors={summary.totalErrors}");

        Debug.Log($"[BuildScript] build OK: {summary.outputPath} ({summary.totalSize} bytes)");
    }

    private static void ConfigureEnvironment(Env env)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        bool isDev = env == Env.Dev;

        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Android, isDev ? DevApplicationId : ProdApplicationId);
        PlayerSettings.productName = isDev ? DevProductName : ProdProductName;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            BuildTargetGroup.Android, isDev ? "ENV_DEV;DEBUG_MENU" : "ENV_PROD");

        // Mono는 Android arm64를 지원하지 않는다. 최신 기기가 64-bit 전용이라 dev/prod 모두 IL2CPP 사용.
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        // dev: ARM64만(빌드 빠름). prod: ARMv7+ARM64(Play 도달 범위).
        PlayerSettings.Android.targetArchitectures = isDev
            ? AndroidArchitecture.ARM64
            : (AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);

        EditorUserBuildSettings.development = isDev;
        EditorUserBuildSettings.buildAppBundle = !isDev; // Dev=APK, Prod=AAB

        string versionName = GetArg("-versionName", null);
        if (!string.IsNullOrEmpty(versionName))
            PlayerSettings.bundleVersion = versionName;

        // -versionCode 인자가 있으면 그 값으로 고정(CI), 없으면 +1 자동 증가(dev/prod 모두).
        // Play 는 versionCode 가 앱마다 유니크 + 단조 증가여야 함.
        if (int.TryParse(GetArg("-versionCode", null), out int vc))
            PlayerSettings.Android.bundleVersionCode = vc;
        else
            PlayerSettings.Android.bundleVersionCode += 1;

        if (isDev)
        {
            PlayerSettings.Android.useCustomKeystore = false;
        }
        else
        {
            string ksPath = GetArg("-keystorePath", null);
            string ksPass = GetArg("-keystorePass", null);
            string aliasN = GetArg("-keyaliasName", null);
            string aliasP = GetArg("-keyaliasPass", null);

            bool haveCliArgs = !string.IsNullOrEmpty(ksPath) && !string.IsNullOrEmpty(ksPass)
                            && !string.IsNullOrEmpty(aliasN) && !string.IsNullOrEmpty(aliasP);

            if (haveCliArgs)
            {
                // CI 경로: 시크릿을 인자로 주입
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = ksPath;
                PlayerSettings.Android.keystorePass = ksPass;
                PlayerSettings.Android.keyaliasName = aliasN;
                PlayerSettings.Android.keyaliasPass = aliasP;
            }
            else if (PlayerSettings.Android.useCustomKeystore
                     && !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
            {
                // 로컬 메뉴 빌드: Player > Publishing Settings에 설정된 키스토어 사용.
                // 비밀번호는 이번 Unity 세션에 Publishing Settings에서 입력돼 있어야 한다.
                Debug.Log("[BuildScript] Prod: Publishing Settings의 Custom Keystore 사용 - "
                          + PlayerSettings.Android.keystoreName + " (alias: " + PlayerSettings.Android.keyaliasName + ")");
            }
            else
            {
                throw new Exception(
                    "[BuildScript] Prod 빌드에는 -keystorePath/-keystorePass/-keyaliasName/-keyaliasPass 인자가 필요하거나, "
                    + "Unity의 Player > Publishing Settings에 Custom Keystore가 설정돼 있어야 합니다.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            $"[BuildScript] configured: id={PlayerSettings.applicationIdentifier} name={PlayerSettings.productName} " +
            $"backend={PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)} aab={EditorUserBuildSettings.buildAppBundle} " +
            $"versionName={PlayerSettings.bundleVersion} versionCode={PlayerSettings.Android.bundleVersionCode}");
    }

    private static string DefaultOutputPath(Env env) => env == Env.Dev
        ? "Builds/dev/zoocat-dev.apk"
        : "Builds/prod/zoocat.aab";

    private static string GetArg(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return fallback;
    }
}
