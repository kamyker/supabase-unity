<h3 align="center">Stage: Alpha</h3>
<h5 align="center">(Tested in Unity 2021.3 Windows Mono/Il2cpp)</h5>

---

Integrate your [Supabase](https://supabase.io) projects with Unity.

Combined [spabase-csharp repo](https://github.com/supabase-community/supabase-csharp) into one easy to install Unity package.

## Installation

### Install via git URL
In Unity click Package Manager -> "+" -> Install via git URL

You can install cs files only: https://github.com/kamyker/supabase-unity.git?path=Unity

Or whole builder to be able to generate .meta files in Unity: https://github.com/kamyker/supabase-unity.git

Then you may have to add dependand dlls that aren't already in your project. They could be installed by other packages. Simply check Unity editor log and add what's needed. For ex:

```
Packages\supabase-unity\Unity\supabase-cloned\supabase-csharp\modules\realtime-csharp\Realtime\Socket.cs(36,17):
error CS0246: The type or namespace name 'WebsocketClient' could not be found
(are you missing a using directive or an assembly reference?)
```

Means you have to install https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/Websocket.Client (via git URL in package manager). 

Dlls list (may be incomplete check .UnityDlls folder for more):

```
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/Websocket.Client
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/System.Reactive
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/Newtonsoft.Json
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/MimeMapping
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/System.Runtime.InteropServices.WindowsRuntime
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/System.Runtime.CompilerServices.Unsafe
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/System.Threading.Channels
https://github.com/kamyker/supabase-unity.git?path=.UnityDlls/System.Threading.Tasks.Extensions
```

---

## Common issues
### Il2cpp
You may encounter runtime errors: "ExecutionEngineException: Attempting to call method (...) for which no ahead of time (AOT) code was generated.  Consider increasing the --generic-virtual-method-iterations=1 argument."

This post describe how to fix it in Unity 2021 https://github.com/SixLabors/ImageSharp/issues/1703#issuecomment-896900448

### Platforms other than Windows
Possible there may be errors because of use of System.Reactive by Websocket.Client. Here's a dll without WindowsRuntiem dependency: https://drive.google.com/file/d/1dvO7GXPpXBWS9zQmmTmD2rAOayrSGW_1/view?usp=sharing . Try this dll instead of package above if you get any errors.
