# CSharp 自用库

## 简介

这是一个 CSharp 自用库，包含了一些自用的函数和类。
```Csharp
namespace Dopamine;
```

## 目录

|                    内容                    |              功能              |
| :----------------------------------------: | :----------------------------: |
|     [`DynamicClass`](####DynamicClass)     | 能够动态解析.json 格式文件的类 |
| [`ExtensionMethods`](####ExtensionMethods) |        额外拓展快捷方法        |
|     [`Singleton<T>`](####Singleton<T>)     |   Unity MonoBehavior泛型单例   |
|         [`Recorder`](####Recorder)         |         静态日志生成器         |

### 详细

#### `DynamicClass`
 - 动态类
 - 继承自`DynamicObject`
 - ```Csharp
   // 构造器
   public class DynamicClass : DynamicObject
   ```
 - 封装了一个`ConcurrentDictionary<string, object>`，用于存储键值对。
 - 实现了`DynamicObject`类中的虚方法`TryGetMember()`和`TrySetMember()`以及索引器访问`this[string key]`。
 - 使用`JsonConverterAttribute`属性自定义了专属Json解析器。
 - 示例
    ```Csharp
   string jsonStr = "{\"name\":\"John\",\"age\":30}";
   DynamicClass obj = new DynamicClass();
   obj = JsonSerializer.Deserialize<DynamicClass>(jsonStr);
   Console.WriteLine(obj["name"]); // John
   ```

#### `ExtensionMethods`
 - 静态类
 - 包含了一些扩展方法

|         方法名         |          功能          |
| :--------------------: | :--------------------: |
| `String.ShowInTrace()` | 在\[输出\]中打印字符串 |

#### `Singleton<T>`
 - 继承自MonoBehavior
 - ```Csharp
   // 构造器
   public class Singleton<T> : MonoBehaviour where T : notnull, MonoBehaviour
   ```
 - 实现了类中的虚方法`Awake()`和```Instance```方法。

#### `Recorder`
 - 静态类

<details>
<summary>
日志
</summary>

|   日期    |                       批注                       |
| :-------: | :----------------------------------------------: |
| 2025.4.20 |                 整理LeetCode问题                 |
| 2025.4.16 |              DeepSeek控制台访问改进              |
| 2025.3.24 | 添加UnityEngine动态库，添加DeepSeekApi访问控制器 |
| 2025.3.21 |            杂项整合，以及一些方法更新            |
| 2025.3.13 |           更新杂项，新增数据帧快捷方法           |
| 2025.3.13 |               新增电表检定规程文档               |
| 2025.3.3  |    增加IEnumerable<byte>和校验以及异或和校验     |
| 2025.2.18 |                   .mdb合并软件                   |
| 2025.2.16 |                     代码整理                     |
| 2025.2.16 |               测试HTTP Client功能                |
| 2025.2.15 |          添加新测试项目，正则表达式测试          |
| 2025.2.15 |                整理代码，进度同步                |
| 2025.2.12 |                    添加WebApp                    |
| 2025.2.9  |                   添加幼圆字体                   |
| 2025.2.8  |                   更新自述文件                   |
| 2025.2.8  |                 格式修改以及整合                 |
| 2025.2.8  |                   第 2 次修改                    |
| 2025.2.8  |                     初次修改                     |
| 2025.2.8  |                     初次上传                     |

 </details>
