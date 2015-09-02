# QiniuUWP
Qiniu SDK for Universal Windows Platform.

Please read this carefully before you start using the SDK.

The project is modified from Official C# SDK. What I have done is porting APIs to WinRT's. So the usage of this SDK should be similar to the original, of course there are some different:
1.Config your key directly in Config.cs file.
2.Plenty of methods now return Task<>, remember to await if you need.
