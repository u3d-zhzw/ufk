using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Samples.Helpers;

public class LoadGameViewSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.Load();
    }

    void Load()
    {
        /* 
        这段测试代码的想法，源于新人在开发界面前，容易忘记要先在GameView设置分辨率大小。
        导致界面效果布局异常。

        从技术层面，想到的一个解决办法是，分辨默在配置表中定义。当首次打开Unity工程时，
        GameView默认使用这个分辨率。

        由于Unity没有公开其内部YAML接口，以下使用了第三方库YamlDotNet。
        主要逻辑是往Unity的分辨率配置文件GameViewSizes.asset，加入一项分辨率配置。
        
        但YamlDotNet输出的内容，Unity的YAML格式，有些差别，见图

        */
        string inFile = UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder + "/GameViewSizes.asset";
        StreamReader input = File.OpenText(inFile);

        YamlStream yaml = new YamlStream();
        yaml.Load(input);
        input.Dispose();

        // 添加一项分辨率配置
        // YamlMappingNode mapping = (YamlMappingNode)(yaml.Documents[0].RootNode);
        // YamlMappingNode standaloneNode = (YamlMappingNode)(mapping["MonoBehaviour"]["m_Standalone"]);
        // YamlSequenceNode m_Custom = (YamlSequenceNode)(standaloneNode["m_Custom"]);

        // YamlMappingNode node = new YamlMappingNode();
        // node.Add("m_BaseText", "test");
        // node.Add("m_SizeType", "1");    
        // node.Add("m_Width", "1");
        // node.Add("m_Height", "2");
        // m_Custom.Add(node);

        string outFile = UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder + "/GameViewSizesTemp.asset";
        StreamWriter output = File.CreateText(outFile);
        yaml.Save(output);
        output.Dispose();
    }
}
