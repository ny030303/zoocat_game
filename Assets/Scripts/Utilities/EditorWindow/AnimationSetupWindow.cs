using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

/// <summary>
/// 유닛, 에너미 둘다 가능
/// isEnemy로 구분하고, 애니메이션 클립 이름 앞글자 보고 맞춰서 생성함
/// </summary>

public partial class AnimationSetupWindow : EditorWindow
{
    private List<Object> spriteSheets = new List<Object>();
    private List<string> animationNames = new List<string>();
    private bool isEnemy;
    private string animatorControllerName = "CHA_";
    private string folderPath = "Assets/Animations"; // 폴더 경로를 설정할 수 있는 필드 추가

    [MenuItem("Tools/Animation Creator Window")]
    public static void ShowWindow()
    {
        GetWindow<AnimationSetupWindow>("Animation Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Settings", EditorStyles.boldLabel);

        isEnemy = EditorGUILayout.Toggle("Is Enemy?", isEnemy);

        GUILayout.Label("Animator Controller Name", EditorStyles.boldLabel);
        animatorControllerName = EditorGUILayout.TextField("Controller Name", animatorControllerName);

        GUILayout.Label("Output Folder", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath); // 폴더 경로를 입력받을 수 있는 텍스트 필드 추가

        GUILayout.Label("Sprite Sheets and Animation Names", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Sprite Sheet and Name"))
        {
            spriteSheets.Add(null);
            animationNames.Add("AnimationName");
        }

        for (int i = 0; i < spriteSheets.Count; i++)
        {
            spriteSheets[i] = EditorGUILayout.ObjectField($"Sprite Sheet {i + 1}", spriteSheets[i], typeof(Object), false);
            animationNames[i] = EditorGUILayout.TextField($"Animation Name {i + 1}", animationNames[i]);
        }

        if (GUILayout.Button("Generate Animations"))
        {
            GenerateAnimations();
        }
    }

    private AnimatorController GenerateAnimations()
    {
        if (spriteSheets.Count == 0 || animationNames.Count == 0)
        {
            Debug.LogWarning("Sprite Sheets and Animation Names must be provided.");
            return null;
        }

        // 폴더 생성
        string fullPath = $"{folderPath}/{animatorControllerName}";
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(folderPath, animatorControllerName);
        }

        // 애니메이터 컨트롤러 생성
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath($"{fullPath}/{animatorControllerName}Animator.controller");


        // 파라미터 추가
        animatorController.AddParameter("Speed", AnimatorControllerParameterType.Float);
        animatorController.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        animatorController.AddParameter("AttackTrigger", AnimatorControllerParameterType.Trigger);

        AnimatorState idleState = null;
        AnimatorState attackState = null;
        AnimatorState walkState = null;
        AnimatorState dieState = null;

        for (int i = 0; i < spriteSheets.Count; i++)
        {
            Object spriteSheet = spriteSheets[i];
            string animationName = animationNames[i];

            if (spriteSheet != null && !string.IsNullOrEmpty(animationName))
            {
                string clipName = $"{animationName}_{i + 1}";
                AnimationClip animationClip = CreateAnimationClip(spriteSheet, clipName, fullPath);

                AnimatorState state = animatorController.layers[0].stateMachine.AddState(animationName);
                state.motion = animationClip;
                // 특정 애니메이션 이름에 따라 루프 설정
                if (animationName.ToLower().Contains("idle") || animationName.ToLower().Contains("walk"))
                {
                    animationClip.wrapMode = WrapMode.Loop; // 루프 모드 설정

                    // 루프 설정 활성화
                    var settings = AnimationUtility.GetAnimationClipSettings(animationClip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(animationClip, settings);
                }

                // 상태 저장
                if (animationName.ToLower().Contains("idle"))
                {
                    idleState = state;
                }
                else if (animationName.ToLower().Contains("attack"))
                {
                    attackState = state;
                }
                else if (animationName.ToLower().Contains("walk"))
                {
                    walkState = state;
                }
                else if (animationName.ToLower().Contains("die"))
                {
                    dieState = state;
                }
            }
        }

        // 적일 때의 애니메이션 설정
        if (isEnemy)
        {
            if (walkState != null && dieState != null)
            {
                // Entry -> Walk Transition
                animatorController.layers[0].stateMachine.defaultState = walkState;

                // Any State -> Die Transition
                AnimatorStateTransition anyToDieTransition = animatorController.layers[0].stateMachine.AddAnyStateTransition(dieState);
                anyToDieTransition.AddCondition(AnimatorConditionMode.If, 0, "IsDead");

                // Die -> Exit Transition
                AnimatorStateTransition dieToExitTransition = dieState.AddExitTransition();
            }
            else
            {
                Debug.LogWarning("Enemy animations (walkAnimation or dieAnimation) could not be found.");
            }
        }
        else
        {
            // 상태 간의 전환 설정
            if (idleState != null && attackState != null)
            {
                AnimatorStateTransition toAttack = idleState.AddTransition(attackState);
                toAttack.hasExitTime = false;
                toAttack.AddCondition(AnimatorConditionMode.If, 0, "AttackTrigger");

                AnimatorStateTransition toIdle = attackState.AddTransition(idleState);
                toIdle.hasExitTime = true;
                toIdle.exitTime = 0.9f; // 애니메이션의 90% 진행 후 전환
            }
        }

        return animatorController;
    }

    private AnimationClip CreateAnimationClip(Object spriteSheet, string animationName, string outputFolderPath)
    {
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> spriteList = new List<Sprite>();

        foreach (var sprite in sprites)
        {
            if (sprite is Sprite)
            {
                spriteList.Add((Sprite)sprite);
            }
        }

        AnimationClip animationClip = new AnimationClip();
        animationClip.frameRate = 3; // 프레임 레이트 설정

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteList.Count];
        for (int i = 0; i < spriteList.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / animationClip.frameRate,
                value = spriteList[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, keyframes);

        string animationPath = $"{outputFolderPath}/{animationName}.anim";
        AssetDatabase.CreateAsset(animationClip, animationPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Animation {animationName}Animator created.");

        return animationClip;
    }
}
