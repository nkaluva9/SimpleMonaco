# SimpleMonaco
WPFで動作するシンプルなMonacoEditorです。
コマンドライン引数を指定することで、開くファイルと言語シンタックスを選択することができます。
Ctrl＋Sで名前を付けて保存or上書き保存を行うことができます。

# DEMO
![image](https://user-images.githubusercontent.com/54029057/163663583-d5bce2d0-8b5d-4bde-90f0-6e346363c618.png)

# Usage
- コマンドライン引数なし
```
SimpleMonaco.exe
```
- コマンドライン引数（ファイル指定）
```
SimpleMonaco.exe C:\Users\Desktop\Sample.js
```
- コマンドライン引数（ファイル指定＋言語指定）
```
SimpleMonaco.exe C:\Users\Desktop\Sample.js javascript 
```

# Note
- VisualStudio ビルドイベント
exe出力先にhtmlファルダを配置するためにビルドイベント追加
```
xcopy "$(ProjectDir)\html" "$(OutDir)\html" /i /s /y
```
![image](https://user-images.githubusercontent.com/54029057/163672256-dbd99c23-9432-40ee-908c-5259547278db.png)

# Installation
- Monaco Editor

https://microsoft.github.io/monaco-editor/

- Monaco Editorとインストールと更新
```
npm install monaco-editor@{VERSION}
```
以下のディレクトリにインストールした「monaco-editor」を配置する。
```
/SimpleMonaco/html/node_modules/
```
