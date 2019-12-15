using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Voxel;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Settings))]
public class VoxelSettingsEditorGui : Editor
{
    VisualElement VisualRoot;
    VisualTreeAsset MasterTreeAsset;
    VisualTreeAsset RuleTreeAsset;

    public void OnEnable()
    {
        MasterTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/GuiContents/VoxelSettingsTemplate.uxml");
        RuleTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/GuiContents/RuleVoxelSettingsTemplate.uxml");
        VisualRoot = new VisualElement();
    }
    public override VisualElement CreateInspectorGUI()
    {
        VisualRoot.Clear();
        MasterTreeAsset.CloneTree(VisualRoot);

        Settings settings = target as Settings;

        VisualElement Rules = VisualRoot.Q(className: "rules-view");
        for (int i = 0; settings.RenderRules != null && i < settings.RenderRules.Length; i++)
            RuleUI(Rules, settings, i, i > 0 ? settings.RenderRules[i - 1].Max + 1 : 0);

        Button addButton = VisualRoot.Q<Button>(className: "add-rule");

        addButton.RegisterCallback<MouseCaptureEvent>(evt =>{
            AddRuleButton(settings);

            float minimum = settings.RenderRules.Length > 1 ? settings.RenderRules[settings.RenderRules.Length - 2].Max + 1 : 0;
            
            if (settings.RenderRules.Length > 1)
            {
                Rules.Q(className: "rule", name: (settings.RenderRules.Length - 2).ToString()).Q<MinMaxSlider>(className: "MinMax").
                highLimit = minimum - 1;
            }
            RuleUI(Rules, settings, settings.RenderRules.Length - 1, minimum);
            
        });

        return VisualRoot;
    }
    public void RuleUI(VisualElement view, Settings rules, int index, float min)
    {
        RuleTreeAsset.CloneTree(view);

        var ruleElement = VisualRoot.Q(className: "rule", name: "Rule");
        ruleElement.name = index.ToString();

        TogglesUi(ruleElement, rules, index);
        MinMaxSliderUI(ruleElement, rules, index, min);

        var objfield = ruleElement.Q<ObjectField>(className: "object-field");
        objfield.objectType = typeof(Material);
        objfield.value = rules.RenderRules[index].material;

        objfield.RegisterCallback<ChangeEvent<Object>>(evt => {
            var element = evt.target as ObjectField;
            string test = (evt.newValue as Material).name;
            rules.RenderRules[index].material = evt.newValue as Material;
        });
    }
    public void MinMaxSliderUI(VisualElement ruleElement, Settings rules, int index, float min)
    {
        var slider = ruleElement.Q<MinMaxSlider>(className: "MinMax");
        slider.highLimit = index + 1 >= rules.RenderRules.Length ? rules.MaxTypeOfVoxel : rules.RenderRules[index + 1].Min - 1;
        slider.lowLimit = min;
        slider.maxValue = rules.RenderRules[index].Max;
        slider.value= (new Vector2(rules.RenderRules[index].Min, rules.RenderRules[index].Max));

        var info = ruleElement.Q<Label>(className: "MinMaxInfo");

        info.text = "min :" + (uint)slider.minValue + "   max :" + (uint)slider.maxValue;
        slider.RegisterCallback<MouseCaptureOutEvent>(evt =>
        {
            var tmpSlider = evt.target as MinMaxSlider;
            info.text = "min :" + (uint)tmpSlider.minValue + "   max :" + (uint)tmpSlider.maxValue;
            rules.RenderRules[index].Min = tmpSlider.minValue;
            rules.RenderRules[index].Max = tmpSlider.maxValue;

            if (index > 0)
            {
                tmpSlider.parent.parent.Q(className: "rule", name: (index - 1).ToString()).Q<MinMaxSlider>(className: "MinMax").
                highLimit = rules.RenderRules[index].Min - 1;

            }
            if (index + 1 < rules.RenderRules.Length)
            {
                tmpSlider.parent.parent.Q(className: "rule", name: (index + 1).ToString()).Q<MinMaxSlider>(className: "MinMax").
                lowLimit = rules.RenderRules[index].Max + 1;
            }
        });
    } 
    public void TogglesUi(VisualElement ruleElement, Settings rules, int index)
    {
        ruleElement.Query<Toggle>(classes: "Toggle").ForEach(toggle =>
        {
            if (toggle.name == "Transparent")
            {
                toggle.value = rules.RenderRules[index].Transparent;
                toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    var tmpToggle = evt.target as Toggle;
                    if (tmpToggle.parent.name == "Rule")
                        return;
                    rules.RenderRules[index].Transparent = evt.newValue;
                });
            }
            else if (toggle.name == "FaceTransparent")
            {
                toggle.value = rules.RenderRules[index].NeighbourTransparentFaced;
                toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    var tmpToggle = evt.target as Toggle;
                    if (tmpToggle.parent.name == "Rule")
                        return;
                    rules.RenderRules[index].NeighbourTransparentFaced = evt.newValue;
                });
            }
            else if (toggle.name == "FaceOpaque")
            {
                toggle.value = rules.RenderRules[index].NeighbourOpaqueFaced;
                toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    var tmpToggle = evt.target as Toggle;
                    if (tmpToggle.parent.name == "Rule")
                        return;
                    rules.RenderRules[index].NeighbourOpaqueFaced = evt.newValue;
                });
            }
        });
    }
    private void AddRuleButton(Settings settings)
    {
        var list = settings.RenderRules != null ? new List<Settings.RenderRule>(settings.RenderRules) : new List<Settings.RenderRule>();
        var renderRule = new Settings.RenderRule
        {
            Min = settings.RenderRules != null && settings.RenderRules.Length != 0 ? list[list.Count - 1].Max : 0,
            Max = settings.MaxTypeOfVoxel,
            NeighbourTransparentFaced = true
        };
        list.Insert(list.Count, renderRule);
        settings.RenderRules = list.ToArray();
    }
}
