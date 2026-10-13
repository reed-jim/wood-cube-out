// using UnityEditor;
// using UnityEngine;

// [CustomPropertyDrawer(typeof(GenericAugmentData))]
// public class GenericAugmentDataDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);

//         float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
//         Rect currentRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

//         // Helper to draw a property and advance the Y position
//         void DrawField(string propertyName, bool condition = true)
//         {
//             if (!condition) return;
//             SerializedProperty prop = property.FindPropertyRelative(propertyName);
//             if (prop != null)
//             {
//                 currentRect.height = EditorGUI.GetPropertyHeight(prop, true);
//                 EditorGUI.PropertyField(currentRect, prop, true);
//                 currentRect.y += currentRect.height + verticalSpacing;
//             }
//         }

//         // --- Trigger & Scope ---
//         EditorGUI.LabelField(currentRect, "Trigger & Scope", EditorStyles.boldLabel);
//         currentRect.y += EditorGUIUtility.singleLineHeight + verticalSpacing;

//         DrawField("trigger");
//         DrawField("scope");

//         EffectTrigger trigger = (EffectTrigger)property.FindPropertyRelative("trigger").enumValueIndex;
//         if (trigger == EffectTrigger.EveryXCycles)
//         {
//             DrawField("cycleFrequency");
//         }

//         // --- Stats Logic ---
//         currentRect.y += 5;
//         EditorGUI.LabelField(currentRect, "Stats Logic", EditorStyles.boldLabel);
//         currentRect.y += EditorGUIUtility.singleLineHeight + verticalSpacing;

//         DrawField("targetStat");
//         DrawField("operation");

//         // --- Mathematical Scaling ---
//         currentRect.y += 5;
//         EditorGUI.LabelField(currentRect, "Mathematical Scaling", EditorStyles.boldLabel);
//         currentRect.y += EditorGUIUtility.singleLineHeight + verticalSpacing;

//         SerializedProperty valueSourceProp = property.FindPropertyRelative("valueSource");
//         DrawField("valueSource");

//         AugmentValueSource valueSource = (AugmentValueSource)valueSourceProp.enumValueIndex;
//         if (valueSource == AugmentValueSource.SourceStat)
//         {
//             DrawField("sourceStat");
//             DrawField("scalingFunction");
//             DrawField("scalingFactor");
//         }
//         DrawField("baseValue");

//         // --- Conditions ---
//         currentRect.y += 5;
//         EditorGUI.LabelField(currentRect, "Conditions", EditorStyles.boldLabel);
//         currentRect.y += EditorGUIUtility.singleLineHeight + verticalSpacing;

//         SerializedProperty conditionProp = property.FindPropertyRelative("condition");
//         DrawField("condition");

//         ConditionType condition = (ConditionType)conditionProp.enumValueIndex;
//         if (condition != ConditionType.AlwaysActive)
//         {
//             DrawField("comparisonStat");
//             DrawField("thresholdValue");
//         }

//         // --- Duration & Lifetime ---
//         currentRect.y += 5;
//         EditorGUI.LabelField(currentRect, "Duration & Lifetime", EditorStyles.boldLabel);
//         currentRect.y += EditorGUIUtility.singleLineHeight + verticalSpacing;

//         SerializedProperty durationProp = property.FindPropertyRelative("durationType");
//         DrawField("durationType");

//         EffectDuration duration = (EffectDuration)durationProp.enumValueIndex;
//         if (duration == EffectDuration.TemporaryBurst)
//         {
//             DrawField("durationInSeconds");
//         }
//         else if (duration == EffectDuration.Decaying)
//         {
//             DrawField("decayRatePerSecond");
//         }
//         else if (duration == EffectDuration.Stacking)
//         {
//             DrawField("maxStacks");
//         }

//         EditorGUI.EndProperty();
//     }

//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         float totalHeight = 0f;
//         float spacing = EditorGUIUtility.standardVerticalSpacing;

//         // Helper to calculate heights dynamically based on active fields
//         float GetHeight(string propName, bool condition = true)
//         {
//             if (!condition) return 0f;
//             SerializedProperty prop = property.FindPropertyRelative(propName);
//             return prop != null ? EditorGUI.GetPropertyHeight(prop, true) + spacing : 0f;
//         }

//         // Base headers and fields height estimation
//         totalHeight += EditorGUIUtility.singleLineHeight + spacing; // Trigger header
//         totalHeight += GetHeight("trigger") + GetHeight("scope");
        
//         if ((EffectTrigger)property.FindPropertyRelative("trigger").enumValueIndex == EffectTrigger.EveryXCycles)
//             totalHeight += GetHeight("cycleFrequency");

//         totalHeight += EditorGUIUtility.singleLineHeight + spacing + 5; // Stats header
//         totalHeight += GetHeight("targetStat") + GetHeight("operation");

//         totalHeight += EditorGUIUtility.singleLineHeight + spacing + 5; // Scaling header
//         totalHeight += GetHeight("valueSource");
        
//         if ((AugmentValueSource)property.FindPropertyRelative("valueSource").enumValueIndex == AugmentValueSource.SourceStat)
//         {
//             totalHeight += GetHeight("sourceStat") + GetHeight("scalingFunction") + GetHeight("scalingFactor");
//         }
//         totalHeight += GetHeight("baseValue");

//         totalHeight += EditorGUIUtility.singleLineHeight + spacing + 5; // Conditions header
//         totalHeight += GetHeight("condition");
//         if ((ConditionType)property.FindPropertyRelative("condition").enumValueIndex != ConditionType.AlwaysActive)
//         {
//             totalHeight += GetHeight("comparisonStat") + GetHeight("thresholdValue");
//         }

//         totalHeight += EditorGUIUtility.singleLineHeight + spacing + 5; // Duration header
//         totalHeight += GetHeight("durationType");
        
//         EffectDuration duration = (EffectDuration)property.FindPropertyRelative("durationType").enumValueIndex;
//         if (duration == EffectDuration.TemporaryBurst) totalHeight += GetHeight("durationInSeconds");
//         else if (duration == EffectDuration.Decaying) totalHeight += GetHeight("decayRatePerSecond");
//         else if (duration == EffectDuration.Stacking) totalHeight += GetHeight("maxStacks");

//         return totalHeight;
//     }
// }