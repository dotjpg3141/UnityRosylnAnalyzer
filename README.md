# UnityRosylnAnalyzer

UnityRosylnAnalyzer is a set of Roslyn analyzers and code fixes for Unity & Visual Studio.

## Preview

![Preview](https://raw.githubusercontent.com/dotjpg3141/UnityRosylnAnalyzer/master/Images/preview.gif)

## How to install

Install the [VSIX-File](https://github.com/dotjpg3141/UnityRosylnAnalyzer/releases/latest) or build it from the source code.

## Available Roslyn analyzers and code fixes

- Cache invocation of various Unity methods like `GetComponent<T>()`, which are called in Update or FixedUpdate
- Detect usage of Unity method with `string` parameters like `GetComponent("Camera")` or `StartCoroutine("MyCoroutine")` and provides code fixes: `GetComponent<Camera>()` and `StartCoroutine(MyCoroutine())`
- Detects if a coroutine method has a return type and `System.Collections.Generic.IEnumerator<T>` and replaces it with  the type `System.Collections.IEnumerator`
